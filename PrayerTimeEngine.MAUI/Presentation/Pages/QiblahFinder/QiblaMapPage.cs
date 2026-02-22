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
using NetTopologySuite.Geometries;
using PrayerTimeEngine.Presentation.Services;

namespace PrayerTimeEngine.Presentation.Pages.QiblahFinder;

public sealed partial class QiblaMapPage : ContentPage
{
    public static readonly double KAABA_LATITUDE = 21.422487;
    public static readonly double KAABA_LONGITUDE = 39.826206;
    public static readonly MPoint KAABA_POINT = toMercator(KAABA_LATITUDE, KAABA_LONGITUDE);

    private readonly ToastMessageService _toastMessageService = null;
    private readonly MapControl _mapControl = new MapControl();
    private readonly TileLayer _tileLayer;

    public QiblaMapPage(ToastMessageService toastMessageService)
    {
        _toastMessageService = toastMessageService;

        this._mapControl.Map = new Mapsui.Map();

        // Force OSM/WebMercator zoom levels (0..19) as explicit resolution list
        GlobalSphericalMercator schema = new GlobalSphericalMercator();
        this._mapControl.Map.Navigator.OverrideResolutions =
            schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList();

        // Optional: also clamp zoom explicitly to those bounds
        this._mapControl.Map.Navigator.OverrideZoomBounds = new MMinMax(0, 19);

        _tileLayer = OpenStreetMap.CreateTileLayer();
        this._mapControl.Map.Layers.Add(_tileLayer);

        this._mapControl.MapTapped += this._mapControl_MapTapped;

        this.Content = this._mapControl;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        MPoint userPoint = await getCurrentMPoint();
        if (userPoint == null)
        {
            return;
        }

        showForPoint(userPoint);
    }

    private async void _mapControl_MapTapped(object sender, MapEventArgs e)
    {
        showForPoint(e.WorldPosition, jumpToPoint: false);
    }

    private void showForPoint(MPoint userPoint, bool jumpToPoint = true)
    {
        foreach (ILayer layer in this._mapControl.Map.Layers.ToList())
        {
            if (object.ReferenceEquals(layer, _tileLayer))
            {
                continue;
            }

            this._mapControl.Map.Layers.Remove(layer);
        }

        drawQiblahLine(
            new Coordinate(userPoint.X, userPoint.Y),
            new Coordinate(KAABA_POINT.X, KAABA_POINT.Y));

        if (jumpToPoint)
            zoomToLocationWithCorrectRotation(userPoint);
    }

    private async Task<MPoint> getCurrentMPoint()
    {
        var location = await getLocation();
        if (location == null)
        {
            return null;
        }

        return toMercator(location.Latitude, location.Longitude);
    }

    private void zoomToLocationWithCorrectRotation(MPoint userPoint)
    {
        this._mapControl.Map.Navigator.CenterOn(userPoint);

        // Zoom index -> resolution (higher index = deeper zoom)
        int zoomLevel = 19;
        double resolution = this._mapControl.Map.Navigator.OverrideResolutions[zoomLevel];
        this._mapControl.Map.Navigator.ZoomTo(resolution);
        this._mapControl.Map.Navigator.ZoomTo(0.1);

        double angle = calculateScreenAngle(userPoint.ToCoordinate(), KAABA_POINT.ToCoordinate());
        this._mapControl.Map.Navigator.RotateTo(angle);
    }

    private void drawQiblahLine(
        Coordinate coordinate1,
        Coordinate coordinate2)
    {
        drawQiblahToleranceSector(coordinate1, coordinate2);

        var lineString = new LineString([coordinate1, coordinate2]);
        IFeature feature = lineString.ToFeature();
        feature.Styles.Add(new VectorStyle
        {
            Line = new Pen(Mapsui.Styles.Color.Yellow, 2)
        });
        this._mapControl.Map.Layers.Add(new MemoryLayer
        {
            Features = [feature]
        });
    }

    private void drawQiblahToleranceSector(
        Coordinate userCoordinates, 
        Coordinate kaabahCoordinates)
    {
        double angle = calculateScreenAngle(userCoordinates, kaabahCoordinates);
        double startAngle = 360 - angle - 45;
        double fullToleranceAngle = 90.0;

        this._mapControl.Map.Layers.Add(new MemoryLayer
        {
            Features = createSectorOutline(
                center: userCoordinates,
                radiusMeters: 15,
                startAngleDeg: startAngle,
                sweepDeg: fullToleranceAngle),

            Style = new VectorStyle
            {
                Line = null,
                Fill = null,
                Outline = null
            }
        });
    }

    private static MPoint toMercator(double latitude, double longitude)
    {
        (double x, double y) = SphericalMercator.FromLonLat(longitude, latitude);
        return new MPoint(x, y);
    }

    private static async Task<Microsoft.Maui.Devices.Sensors.Location> getLocation()
    {
        var cached = await Geolocation.GetLastKnownLocationAsync();
        if (cached != null)
            return cached;

        return await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
    }

    private static double calculateScreenAngle(
        Coordinate from,
        Coordinate to)
    {
        double dx = to.X - from.X;
        double dy = to.Y - from.Y;

        double angleRadians = Math.Atan2(dx, dy);
        double angleDegrees = angleRadians * 180.0 / Math.PI;

        return 360 - angleDegrees;
    }

    private static List<IFeature> createSectorOutline(
        Coordinate center,
        double radiusMeters,
        double startAngleDeg,
        double sweepDeg)
    {
        int segments = 60;

        var features = new List<IFeature>();

        var arcPoints = new List<Coordinate>();

        foreach (double angle in Enumerable.Range(0, segments)
                    .Select(i => startAngleDeg + (sweepDeg * i / segments)))
        {
            double radians = angle * Math.PI / 180.0;
            double x = center.X + radiusMeters * Math.Sin(radians);
            double y = center.Y + radiusMeters * Math.Cos(radians);
            arcPoints.Add(new Coordinate(x, y));
        }

        // Arc
        var arc = new LineString(arcPoints.ToArray());
        var arcFeature = arc.ToFeature();
        arcFeature.Styles.Add(createPenStyle());

        // Left radial line
        Coordinate leftPoint = arcPoints.First();
        var leftLine = new LineString([center, leftPoint]);
        var leftFeature = leftLine.ToFeature();
        leftFeature.Styles.Add(createPenStyle());

        // Right radial line
        Coordinate rightPoint = arcPoints.Last();
        var rightLine = new LineString([center, rightPoint]);
        var rightFeature = rightLine.ToFeature();
        rightFeature.Styles.Add(createPenStyle());

        features.Add(arcFeature);
        features.Add(leftFeature);
        features.Add(rightFeature);

        return features;
    }

    private static VectorStyle createPenStyle()
    {
        return new VectorStyle
        {
            Outline = null,
            Line = new Pen(Mapsui.Styles.Color.Black, 2)
            {
                PenStyle = PenStyle.Dash
            }
        };
    }
}