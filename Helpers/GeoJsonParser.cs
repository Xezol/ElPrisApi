using Common.Models;
using ElPrisApi.Models;
using System.Text.Json;

namespace ElPrisApi.Helpers
{
    public static class GeoJsonParser
    {
        public static List<Polygon> ParseGeoJson(string geoJsonFilePath)
        {
            var geoJsonData = File.ReadAllText(geoJsonFilePath);
            var geoJson = JsonSerializer.Deserialize<GeoJson>(geoJsonData);

            var polygons = new List<Polygon>();

            foreach (var feature in geoJson.Features)
            {
                var vertices = new List<Point>();
                var areaId = feature.Id; // Get the area id from the top level

                if (feature.Geometry.Type == "Polygon")
                {
                    var coordinates = feature.Geometry.Coordinates as List<List<List<double>>>;
                    foreach (var coordinatePair in coordinates[0])
                    {
                        double x = coordinatePair[0];
                        double y = coordinatePair[1];
                        vertices.Add(new Point(x, y));
                    }
                }
                else if (feature.Geometry.Type == "MultiPolygon")
                {
                    var coordinates = feature.Geometry.Coordinates as List<List<List<List<double>>>>;
                    foreach (var polygon in coordinates)
                    {
                        foreach (var coordinatePair in polygon[0])
                        {
                            double x = coordinatePair[0];
                            double y = coordinatePair[1];
                            vertices.Add(new Point(x, y));
                        }
                    }
                }

                // Convert the id to the Area enum
                if (Enum.TryParse($"SE{areaId}", out Area area))
                {
                    polygons.Add(new Polygon(vertices, area));
                }
            }

            return polygons;
        }
    }
}
