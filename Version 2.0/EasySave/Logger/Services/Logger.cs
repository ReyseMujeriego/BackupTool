using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using Logger.Models;

namespace Logger.Services
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
        public void LogFileTransfert(LoggerModel loggerModel)
        {
            ConfigManager config = new ConfigManager();
            config.LoadConfig();
            string logFormat = config.GetLogFormat();

            string logContent = logFormat switch
            {
                "xml" => SerializeToXml(loggerModel),
                _ => JsonSerializer.Serialize(loggerModel, new JsonSerializerOptions { WriteIndented = true }) // Par défaut JSON
            };

            WriteLogToFile(logContent, logFormat);
        }

        private string SerializeToXml(LoggerModel loggerModel)
        {
            using StringWriter stringWriter = new StringWriter();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(LoggerModel));
            xmlSerializer.Serialize(stringWriter, loggerModel);
            return stringWriter.ToString();
        }

        private void WriteLogToFile(string logContent, string format)
        {
            ConfigManager config = new ConfigManager();
            string logPath = config.GetLogFilePath();

            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            string fileExtension = format == "xml" ? "xml" : "json";
            string fileNameLog = $"Log-{DateTime.Now:yyyy-MM-dd}.{fileExtension}";
            string filePathLog = Path.Combine(logPath, fileNameLog);

            File.AppendAllText(filePathLog, logContent + Environment.NewLine);
        }

    }
}