using System.Text.Json;

namespace ElPrisApi.Constants
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
