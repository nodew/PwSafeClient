using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PwSafeClient.Console
{
    public class Config
    {
        [JsonPropertyName("defaultDb")]
        public string? DefaultDatabase { get; set; }

        [JsonPropertyName("databases")]
        public Dictionary<string, string> Databases { get; set; } = new Dictionary<string, string>();
    }

    public static class ConfigManager
    {
        public const string DefaultAlias = "default";

        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public static string GetDbPath(Config config, string? alias)
        {
            alias = alias ?? config.DefaultDatabase ?? DefaultAlias;
            if (config.Databases.TryGetValue(alias, out string? filePath)) {
                return filePath ?? string.Empty;
            }

            return string.Empty;
        }

        public static string ToJson(Config config)
        {
            string json = JsonSerializer.Serialize(config, options);;
            return json;
        }

        public static async Task ToFile(Config config, string filepath)
        {
            using FileStream fileStream = File.Open(filepath, FileMode.Create);
            using StreamWriter writer = new StreamWriter(fileStream);
            string json = ToJson(config);
            await writer.WriteAsync(json);
        }

        public static Config FromJson(string json)
        {
            try
            {
                Config? config = JsonSerializer.Deserialize<Config>(json, options);
                return config ?? new Config();
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e.Message);
                throw;
            }
        }

        public static async Task<Config> FromFile(string filepath)
        {
            using FileStream fileStream = File.Open(filepath, FileMode.Open);
            using StreamReader reader = new StreamReader(fileStream);
            string json = await reader.ReadToEndAsync();
            return FromJson(json);
        }
    }
}
