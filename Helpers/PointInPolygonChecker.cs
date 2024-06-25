using ElPrisApi.Models;

namespace ElPrisApi.Helpers
{
    public static class PointInPolygonChecker
    {
        public static bool IsPointInPolygon(Point point, Polygon polygon)
        {
            int n = polygon.Vertices.Count;
            bool inside = false;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((polygon.Vertices[i].Y > point.Y) != (polygon.Vertices[j].Y > point.Y)) &&
                    (point.X < (polygon.Vertices[j].X - polygon.Vertices[i].X) * (point.Y - polygon.Vertices[i].Y) / (polygon.Vertices[j].Y - polygon.Vertices[i].Y) + polygon.Vertices[i].X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
