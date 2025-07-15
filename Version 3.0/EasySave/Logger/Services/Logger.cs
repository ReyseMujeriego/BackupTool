using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using System.Threading;
using Logger.Models;

namespace Logger.Services
{
    public class Logger
    {
        private static Logger instance;
        public static Logger Instance => instance ??= new Logger();

        private readonly string logDirectory;
        private static readonly Mutex logMutex = new(); // Mutex --> to 1 enter and the other in a queue

        // Constructor: Initializes the log folder
        private Logger()
        {
            ConfigManager config = new ConfigManager();
            config.LoadConfig();
            logDirectory = config.GetLogFilePath();

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory); // Creates the folder if it does not exist
            }
        }

        /// <summary>
        /// Format a JSON with the necessary attributes.
        /// </summary>
        /// <param name="timeStamp">The time when the file was copied.</param>
        /// <param name="saveName">The name of the save.</param>
        /// <param name="fileSize">The file size.</param>
        /// <param name="fileTransferTime">The time it took for the file to copy.</param>
        /// <param name="fileSource">The file source.</param>
        /// <param name="fileTarget">The file destination.</param>
        public void LogFileTransfert(LoggerModel loggerModel)
        {
            ConfigManager config = new ConfigManager();
            config.LoadConfig();
            string logFormat = config.GetLogFormat().ToLower();

            // Check if the format is supported
            if (logFormat != "json" && logFormat != "xml")
            {
                throw new ArgumentException("Unsupported log format. Use 'json' or 'xml'.");
            }

            SaveLog(loggerModel, logFormat);
        }

        /// <summary>
        /// Serializes a log in JSON or XML and saves it while maintaining the correct format.
        /// </summary>
        private void SaveLog(LoggerModel log, string format)
        {
            string logFilePath = GetLogFilePath(format);

            // Wait the first one and stop the others in a queue
            logMutex.WaitOne();
            try
            {
                List<LoggerModel> logs = LoadExistingLogs(logFilePath, format); // Load existing logs
                logs.Add(log); // 🔹 Add the new log to the list

                // Write the file in JSON or XML format
                if (format == "json")
                {
                    string json = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(logFilePath, json);
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(logFilePath, false)) // 🔹 `false` to avoid duplication
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<LoggerModel>));
                        serializer.Serialize(writer, logs);
                    }
                }
            }
            finally
            {
                // Free the mutex after the writing process
                logMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Retrieves the log file path based on the format (JSON/XML).
        /// </summary>
        private string GetLogFilePath(string format)
        {
            string extension = format == "xml" ? "xml" : "json";
            string fileName = $"Log-{DateTime.Now:yyyy-MM-dd}.{extension}";
            return Path.Combine(logDirectory, fileName);
        }

        /// <summary>
        /// Loads existing logs to avoid overwriting old ones and prevent XML structure errors.
        /// </summary>
        private List<LoggerModel> LoadExistingLogs(string filePath, string format)
        {
            if (!File.Exists(filePath))
                return new List<LoggerModel>();

            try
            {
                if (format == "json")
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<List<LoggerModel>>(json) ?? new List<LoggerModel>();
                }
                else
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<LoggerModel>));
                        return (List<LoggerModel>)serializer.Deserialize(reader) ?? new List<LoggerModel>();
                    }
                }
            }
            catch
            {
                return new List<LoggerModel>(); // If an error occurs, return an empty list
            }
        }
    }
}
