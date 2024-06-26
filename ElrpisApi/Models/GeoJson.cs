using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElPrisApi.Models
{
    public class GeoJson
    {
        [JsonPropertyName("features")]
        public List<Feature> Features { get; set; } = new List<Feature>();
    }

    public class Feature
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("geometry")]
        public Geometry Geometry { get; set; } = new Geometry();

        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }

    [JsonConverter(typeof(GeometryConverter))]
    public class Geometry
    {
        public string Type { get; set; }
        public object Coordinates { get; set; }
    }

    public class GeometryConverter : JsonConverter<Geometry>
    {
        public override Geometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var geo = new Geometry();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return geo;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "type":
                            geo.Type = reader.GetString();
                            break;
                        case "coordinates":
                            if (geo.Type == "Polygon")
                            {
                                geo.Coordinates = JsonSerializer.Deserialize<List<List<List<double>>>>(ref reader, options);
                            }
                            else if (geo.Type == "MultiPolygon")
                            {
                                geo.Coordinates = JsonSerializer.Deserialize<List<List<List<List<double>>>>>(ref reader, options);
                            }
                            break;
                    }
                }
            }

            throw new JsonException("Invalid JSON for Geometry");
        }

        public override void Write(Utf8JsonWriter writer, Geometry value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("type", value.Type);
            writer.WritePropertyName("coordinates");
            JsonSerializer.Serialize(writer, value.Coordinates, options);
            writer.WriteEndObject();
        }
    }
}
