
using System.Text.Json;
namespace Logger.Services
{
    public class ConfigManager
    {
        public void LoadConfig()
        {
            string nameFilePath = GetNameFilePath();
            string configLogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EasySave");
            if (!Directory.Exists(configLogFilePath))
            {
                Directory.CreateDirectory(configLogFilePath);
            }

            if (!File.Exists(nameFilePath))
            {
                var filePath = new
                {
                    LogFilePath = Path.Combine("C:", "ProgramData", "EasySave", "logs"),
                    RealTimeFilePath = Path.Combine("C:", "ProgramData", "EasySave", "realtimestate")
                };

                string jsonContent = JsonSerializer.Serialize(filePath, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(nameFilePath, jsonContent);
            }
        }

        /// <summary>
        /// Retrieve the log path name from the getfilepath function.
        /// </summary>
        public string GetLogFilePath()
        {
            return GetFilePath("LogFilePath", GetNameFilePath());
        }

        /// <summary>
        /// Retrieve the real-time logs path name from the getfilepath function
        /// </summary>
        public string GetRealTimeFilePath()
        {
            return GetFilePath("RealTimeFilePath", GetNameFilePath());
        }

        /// <summary>
        /// Retrieve the path name from the getfilepath function
        /// </summary>
        /// <param name="propertyName">The path name in the JSON.</param>
        /// <param name="NameFilePath">The file path.</param>
        private string GetFilePath(string propertyName, string NameFilePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(NameFilePath);
                using JsonDocument jsonDoc = JsonDocument.Parse(jsonContent);
                if (jsonDoc.RootElement.TryGetProperty(propertyName, out JsonElement filePath))
                {
                    return filePath.GetString() ?? string.Empty;
                }
            }
            catch (JsonException)
            {
                return string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
        /// Return the path of the file that contains the log and real-time paths.
        /// </summary>
        private string GetNameFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EasySave", "FilePathConfig.json");
        }
        public void SetLogFormat(string format)
        {
            string configFilePath = GetNameFilePath();

            string jsonContent = File.ReadAllText(configFilePath);
            using JsonDocument jsonDoc = JsonDocument.Parse(jsonContent);
            Dictionary<string, string> configData = jsonDoc.RootElement.EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.GetString() ?? "");

            configData["LogFormat"] = format;

            string updatedJson = JsonSerializer.Serialize(configData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configFilePath, updatedJson);
        }
        public string GetLogFormat()
        {
            return GetFilePath("LogFormat", GetNameFilePath()).ToLower() ?? "json"; // JSON par défaut
        }
    }
}