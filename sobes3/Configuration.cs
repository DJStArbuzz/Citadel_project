using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Configuration
{
    [JsonPropertyName("period_seconds")]
    public int period { get; set; } = 2;

    [JsonPropertyName("output_mode")]
    public string resultFormat { get; set; } = "Console";

    [JsonPropertyName("log_file_path")]
    public string logFilePath { get; set; } = "metrics.log";

    public static Configuration Load(string path = "config.json")
    {
        if (!File.Exists(path))
        {
            var defaultConfig = new Configuration();
            string json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            
            return defaultConfig;
        }

        string content = File.ReadAllText(path);

        return JsonSerializer.Deserialize<Configuration>(content) ?? new Configuration();
    }
}