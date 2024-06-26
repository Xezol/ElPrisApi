using Common.Models;
using ElPrisApi.Helpers;
using ElPrisApi.Models;

namespace ElPrisApi.Services
{
    public class GpsCoordinateChecker
    {
        private readonly List<Polygon> _polygons;

        public GpsCoordinateChecker(IWebHostEnvironment env)
        {
            string contentRootPath = env.ContentRootPath;
            string geoJsonFilePath = Path.Combine(contentRootPath, "EnergyAreas.geojson");
            _polygons = GeoJsonParser.ParseGeoJson(geoJsonFilePath);
        }

        public (bool inside, Area? area) CheckPoint(double latitude, double longitude)
        {
            var point = new Point(longitude, latitude);

            foreach (var polygon in _polygons)
            {
                if (PointInPolygonChecker.IsPointInPolygon(point, polygon))
                {
                    return (true, polygon.Area);
                }
            }

            return (false, null);
        }
    }
}
