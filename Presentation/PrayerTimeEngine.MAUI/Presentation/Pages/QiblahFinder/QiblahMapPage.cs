using BruTile.Cache;
using BruTile.Predefined;
using ExCSS;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Mapsui.UI.Maui;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using PrayerTimeEngine.Extensions;
using PrayerTimeEngine.Presentation.Services;
using Brush = Mapsui.Styles.Brush;
using Color = Mapsui.Styles.Color;
using Polygon = NetTopologySuite.Geometries.Polygon;

namespace PrayerTimeEngine.Presentation.Pages.QiblahFinder;
public sealed partial class QiblahMapPage : ContentPage
{
    public static readonly MPoint KAABA_COORDINATES = toMercator(latitude: 21.422487, longitude: 39.826206);

    // OpenStreetMap's volunteer-run tile servers require a User-Agent that clearly identifies
    // the application, otherwise requests are blocked with HTTP 403 (see osm.wiki/Blocked and
    // https://operations.osmfoundation.org/policies/tiles/). Mapsui's default "user-agent-of-*"
    // value does not satisfy this, so we provide an explicit one with app name, version and a
    // contact URL.
    private static readonly string OSM_USER_AGENT =
        $"PrayerTimeEngine/{AppInfo.Current.VersionString} (+https://github.com/TalipV/PrayerTimeEngine)";

    // The OSM tile usage policy requires tiles to be cached locally (min. 7 days) instead of
    // being re-downloaded for repeated views. Mapsui only keeps an in-memory cache by default,
    // which is lost on every app restart, so we provide a persistent one on disk.
    // DefaultCache is read by CreateTileLayer, hence it has to be set before that call.
    private static readonly TimeSpan OSM_TILE_CACHE_DURATION = TimeSpan.FromDays(30);

    static QiblahMapPage()
    {
        OpenStreetMap.DefaultCache ??= new FileCache(
            directory: Path.Combine(FileSystem.CacheDirectory, "osm-tiles"),
            format: "png",
            cacheExpireTime: OSM_TILE_CACHE_DURATION);
    }

    private static int _isRefreshingLocation = 0;

    private readonly ToastMessageService _toastMessageService;
    private readonly ILogger<QiblahMapPage> _logger;

    private readonly MapControl _mapControl = new MapControl();
    private readonly TileLayer _tileLayer;
    private readonly MemoryLayer _qiblahLineLayer;
    private readonly MemoryLayer _toleranceLayer;

    private (MPoint Point, double QiblahAngle) _currentPoint = (KAABA_COORDINATES, 0);

    public QiblahMapPage(
        ToastMessageService toastMessageService,
        ILogger<QiblahMapPage> logger)
    {
        this._toastMessageService = toastMessageService;
        this._logger = logger;

        this._mapControl.Map = new Mapsui.Map();
        configureZoomLevel();

        this._tileLayer = OpenStreetMap.CreateTileLayer(OSM_USER_AGENT);
        this._mapControl.Map.Layers.Add(_tileLayer);

        this._qiblahLineLayer = new MemoryLayer();
        this._mapControl.Map.Layers.Add(_qiblahLineLayer);

        this._toleranceLayer = new MemoryLayer
        {
            Style = new VectorStyle
            {
                Line = null,
                Fill = null,
                Outline = null
            }
        };
        this._mapControl.Map.Layers.Add(_toleranceLayer);

        var gpsButton = new ImageButton
        {
            Source = "location.png",
            BackgroundColor = Microsoft.Maui.Graphics.Colors.White,
            CornerRadius = 22,
            WidthRequest = 44,
            HeightRequest = 44,
            Padding = 8,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(10)
        };
        gpsButton.Clicked += locateButton_Clicked;

        Content = new Grid
        {
            Children = { _mapControl, gpsButton }
        };

        this._mapControl.MapTapped += this._mapControl_MapTapped;
    }

    private async void locateButton_Clicked(object sender, EventArgs e)
    {
        await locateAndShowCurrentPosition();
    }

    /// <summary>
    /// Determines the device location and shows the qiblah direction for it.
    /// </summary>
    /// <remarks>
    /// Called from async void event handlers (OnAppearing, button click), so no exception may escape
    /// from here: an unhandled exception in an async void method crashes the whole app. This is exactly
    /// what happened when the location service of the device was switched off
    /// (<see cref="FeatureNotEnabledException"/>).
    /// </remarks>
    private async Task locateAndShowCurrentPosition()
    {
        try
        {
            MPoint? point = await getCurrentLocation();
            if (point is null)
                return;

            setCurrentPoint(point);
            refreshCurrentPoint();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while showing the qiblah for the current location");
            _toastMessageService.ShowError(exception.Message);
        }
    }

    private void configureZoomLevel()
    {
        var schema = new GlobalSphericalMercator();
        var resolutions = schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList();

        double smallest = resolutions.Last();
        resolutions.Add(smallest / 2);
        resolutions.Add(smallest / 4);
        resolutions.Add(smallest / 8);

        _mapControl.Map.Navigator.OverrideResolutions = resolutions;
    }

    private void zoomToLocationWithCorrectRotation()
    {
        MPoint userCoordinates = _currentPoint.Point;
        double angle = _currentPoint.QiblahAngle;

        this._mapControl.Map.Navigator.CenterOn(userCoordinates);

        // Zoom index -> resolution (higher index = deeper zoom)
        int zoomLevel = 19;
        double resolution = this._mapControl.Map.Navigator.OverrideResolutions[zoomLevel];
        this._mapControl.Map.Navigator.ZoomTo(resolution);
        this._mapControl.Map.Navigator.ZoomTo(0.1);
        this._mapControl.Map.Navigator.RotateTo(angle);
    }

    private void zoomChanged()
    {
        if (_currentPoint.Point == null)
        {
            return;
        }

        drawQiblahToleranceSector();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // subscribed here instead of in the constructor so that it is symmetrical to OnDisappearing
        _mapControl.Map.Navigator.ViewportChanged -= navigator_ViewportChanged;
        _mapControl.Map.Navigator.ViewportChanged += navigator_ViewportChanged;

        await locateAndShowCurrentPosition();
    }

    private void setCurrentPoint(MPoint newPoint)
    {
        double angle = calculateScreenAngle(newPoint.ToCoordinate());
        _currentPoint = (newPoint, angle);
    }

    protected override void OnDisappearing()
    {
        _mapControl.Map.Navigator.ViewportChanged -= navigator_ViewportChanged;
        base.OnDisappearing();
    }

    private async void _mapControl_MapTapped(object sender, MapEventArgs e)
    {
        setCurrentPoint(e.WorldPosition);
        refreshCurrentPoint(jumpToPoint: false);
    }

    private void refreshCurrentPoint(bool jumpToPoint = true)
    {
        if (_currentPoint.Point == null)
        {
            return;
        }

        drawQiblahToleranceSector();
        drawQiblahLine();

        if (jumpToPoint)
            zoomToLocationWithCorrectRotation();
    }

    private void drawQiblahLine()
    {
        var lineString = new LineString([_currentPoint.Point.ToCoordinate(), KAABA_COORDINATES.ToCoordinate()]);
        var feature = lineString.ToFeature();
        feature.Styles.Add(new VectorStyle
        {
            Line = new Pen(Mapsui.Styles.Color.Yellow, 2)
        });

        this._qiblahLineLayer.Features = [feature];
    }

    private void drawQiblahToleranceSector()
    {
        double angle = _currentPoint.QiblahAngle;
        double startAngle = 360 - angle - 45;
        double fullToleranceAngle = 90.0;

        double resolution = _mapControl.Map.Navigator.Viewport.Resolution;
        double radiusMeters = resolution * 120;

        _toleranceLayer.Features = createSectorOutline(
                center: _currentPoint.Point.ToCoordinate(),
                radiusMeters: radiusMeters,
                startAngleDeg: startAngle,
                sweepDeg: fullToleranceAngle);
    }

    /// <returns>The current location or <c>null</c> if it could not be determined (the user has already been informed in that case).</returns>
    private async Task<MPoint?> getCurrentLocation()
    {
        Location? location;

        try
        {
            location = await Geolocation.GetLastKnownLocationAsync();

            if (location is null)
            {
                location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
            }
            else if (location.Timestamp < DateTimeOffset.UtcNow.AddMinutes(-2))
            {
                if (Interlocked.CompareExchange(ref _isRefreshingLocation, 1, 0) == 0)
                {
                    // fire and forget: only warms up the last known location for the next call,
                    // a failure (e.g. location switched off in the meantime) is irrelevant
                    _ = Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best))
                        .ContinueWith(_ => Interlocked.Exchange(ref _isRefreshingLocation, 0));
                }
            }
        }
        catch (FeatureNotEnabledException exception)
        {
            _logger.LogWarning(exception, "Location service is switched off");

            bool openSettings = await DisplayAlertAsync(
                title: "Standort deaktiviert",
                message: "Der Standort ist auf dem Gerät ausgeschaltet. Schalte ihn ein und tippe danach auf den Standort-Button "
                       + "oder tippe auf die Karte, um deine Position manuell zu setzen.",
                accept: "Einstellungen öffnen",
                cancel: "Abbrechen");

            if (openSettings)
                openLocationSettings();

            return null;
        }
        catch (PermissionException exception)
        {
            _logger.LogWarning(exception, "Location permission not granted");

            bool openSettings = await DisplayAlertAsync(
                title: "Keine Berechtigung",
                message: "Die App hat keine Berechtigung für den Standort. Erteile sie in den App-Einstellungen "
                       + "oder tippe auf die Karte, um deine Position manuell zu setzen.",
                accept: "Einstellungen öffnen",
                cancel: "Abbrechen");

            if (openSettings)
                AppInfo.Current.ShowSettingsUI();

            return null;
        }
        catch (FeatureNotSupportedException exception)
        {
            _logger.LogWarning(exception, "Location is not supported on this device");
            _toastMessageService.ShowWarning("Standortbestimmung wird auf diesem Gerät nicht unterstützt. Tippe auf die Karte, um deine Position zu setzen.");
            return null;
        }

        if (location is null)
        {
            _toastMessageService.ShowWarning("Standort konnte nicht ermittelt werden. Tippe auf die Karte, um deine Position zu setzen.");
            return null;
        }

        return toMercator(location.Latitude, location.Longitude);
    }

    private void openLocationSettings()
    {
        try
        {
#if ANDROID
            var intent = new global::Android.Content.Intent(global::Android.Provider.Settings.ActionLocationSourceSettings);
            intent.AddFlags(global::Android.Content.ActivityFlags.NewTask);
            global::Android.App.Application.Context.StartActivity(intent);
#else
            AppInfo.Current.ShowSettingsUI();
#endif
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Opening the location settings failed");
            _toastMessageService.ShowError("Einstellungen konnten nicht geöffnet werden.");
        }
    }

    private static double calculateScreenAngle(Coordinate coordinates)
    {
        double dx = KAABA_COORDINATES.X - coordinates.X;
        double dy = KAABA_COORDINATES.Y - coordinates.Y;

        double angleRadians = Math.Atan2(dx, dy);
        double angleDegrees = angleRadians * 180.0 / Math.PI;

        return 360 - angleDegrees;
    }

    private static List<Mapsui.IFeature> createSectorOutline(
        Coordinate center,
        double radiusMeters,
        double startAngleDeg,
        double sweepDeg)
    {
        int segments = 60;
        List<Coordinate> polygonPoints = [center];

        foreach (double angle in Enumerable.Range(0, segments)
                     .Select(i => startAngleDeg + (sweepDeg * i / segments)))
        {
            double radians = angle * Math.PI / 180.0;
            double x = center.X + radiusMeters * Math.Sin(radians);
            double y = center.Y + radiusMeters * Math.Cos(radians);

            polygonPoints.Add(new Coordinate(x, y));
        }

        polygonPoints.Add(center); // close polygon

        var linearRing = new LinearRing(polygonPoints.ToArray());
        var polygon = new Polygon(linearRing);

        var feature = GeometryExtensions.ToFeature(polygon);

        feature.Styles.Add(new VectorStyle
        {
            Fill = new Brush(Color.LightSlateGray.WithTransparency(40)),
        });

        return [feature];
    }

    private static MPoint toMercator(double latitude, double longitude)
    {
        (double x, double y) = SphericalMercator.FromLonLat(longitude, latitude);
        return new MPoint(x, y);
    }

    private void navigator_ViewportChanged(object sender, ViewportChangedEventArgs e)
    {
        zoomChanged();
    }
}