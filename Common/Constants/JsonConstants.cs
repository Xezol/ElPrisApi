using System.Text.Json;

namespace Common.Constants
{
    public static class JsonConstants
    {
        public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

}
