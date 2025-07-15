using System.Text.Json;
using System.Text;

namespace logger
{
    public class Logger
    {

            private static Logger instance;
            public static Logger Instance => instance ??= new Logger();


            /// <summary>
            /// Format a JSON with the necessary attributes.
            /// </summary>
            /// <param name="timeStamp">The time when the file was copied.</param>
            /// <param name="saveName">The name of the save.</param>
            /// <param name="fileSize">The file size.</param>
            /// <param name="fileTransferTime">The time it took for the file to copy.</param>
            /// <param name="fileSource">The file source.</param>
            /// <param name="fileTarget">The file destination.</param>
            public void LogFileTransfert(DateTime timeStamp, string saveName, long fileSize, double fileTransferTime, string fileSource, string fileTarget)
            {
                var logMessage = new
                {
                    Name = saveName,
                    FileSource = fileSource,
                    FileTarget = fileTarget,
                    FileSize = fileSize,
                    FileTransferTime = fileTransferTime + " ms",
                    time = timeStamp
                };

                string jsonLogMessage = JsonSerializer.Serialize(logMessage, new JsonSerializerOptions { WriteIndented = true });

                WriteLogToFile(jsonLogMessage);
            }


            /// <summary>
            /// Write the logs to a JSON file.
            /// </summary>
            /// <param name="logContent">Contains the message in JSON.</param>
            private void WriteLogToFile(string logContent)
            {
                ConfigManager config = new ConfigManager();

                config.LoadConfig();

                string logPath = config.GetLogFilePath();

                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }

                string fileNameLog = $"Log-{DateTime.Now.ToString("yyyy-MM-dd")}.json";

                string filePathLog = Path.Combine(logPath, fileNameLog);

                //Using using to ensure that FileStream is properly released, even if an exception occurs.
                using (FileStream fs = new FileStream(filePathLog, FileMode.Append, FileAccess.Write)) //Append to open an existing file / create a file if it doesn't exist. Write to write to the file.
                {
                    byte[] logfs = new UTF8Encoding(true).GetBytes(logContent);
                    fs.Write(logfs, 0, logfs.Length);
                }
            }
        


    }
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
    }
    }


