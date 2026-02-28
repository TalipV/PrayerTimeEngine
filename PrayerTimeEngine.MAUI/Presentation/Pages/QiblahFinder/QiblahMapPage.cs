using BruTile.Predefined;
using Debounce.Core;
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

namespace PrayerTimeEngine.Presentation.Pages.QiblahFinder;
public sealed partial class QiblahMapPage : ContentPage
{
    public static readonly MPoint KAABA_COORDINATES = toMercator(latitude: 21.422487, longitude: 39.826206);

    private readonly MapControl _mapControl = new MapControl();
    private readonly TileLayer _tileLayer;
    private readonly MemoryLayer _qiblahLineLayer;
    private readonly MemoryLayer _toleranceLayer;

    private (MPoint Point, double QiblahAngle) _currentPoint = (KAABA_COORDINATES, 0);

    public QiblahMapPage()
    {
        this._mapControl.Map = new Mapsui.Map();
        configureZoomLevel();

        this._tileLayer = OpenStreetMap.CreateTileLayer();
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
        this.Content = this._mapControl;

        this._mapControl.MapTapped += this._mapControl_MapTapped;
        _mapControl.Map.Navigator.ViewportChanged += navigator_ViewportChanged;
    }

    private void configureZoomLevel()
    {
        var schema = new GlobalSphericalMercator();

        this._mapControl.Map.Navigator.OverrideResolutions = schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList();
        //this._mapControl.Map.Navigator.OverrideZoomBounds = new MMinMax(0, 19);

        var resolutions = schema.Resolutions
            .Select(r => r.Value.UnitsPerPixel)
            .ToList();

        // allow deeper zoom
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

    private void zoomChanged(MapControl mapControl)
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

        setCurrentPoint(await getCurrentLocation());
        refreshCurrentPoint();
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

    private static async Task<MPoint> getCurrentLocation()
    {
        var location = await Geolocation.GetLastKnownLocationAsync()
            ?? await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
        return toMercator(location.Latitude, location.Longitude);
    }

    private static double calculateScreenAngle(Coordinate coordinates)
    {
        double dx = KAABA_COORDINATES.X - coordinates.X;
        double dy = KAABA_COORDINATES.Y - coordinates.Y;

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

    private static MPoint toMercator(double latitude, double longitude)
    {
        (double x, double y) = SphericalMercator.FromLonLat(longitude, latitude);
        return new MPoint(x, y);
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

    private void navigator_ViewportChanged(object sender, ViewportChangedEventArgs e)
    {
        zoomChanged(_mapControl);
    }
}