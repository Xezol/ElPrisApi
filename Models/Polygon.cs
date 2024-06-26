using Common.Models;

namespace ElPrisApi.Models
{
    public class Polygon
    {
        public List<Point> Vertices { get; set; }
        public Area Area { get; set; }

        public Polygon(List<Point> vertices, Area area)
        {
            Vertices = vertices;
            Area = area;
        }
    }
}
