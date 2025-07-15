using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EasySave.Core.Models;
using logger;
namespace EasySave.Core.Services
{
    public class RealTimeLogger
    {


        private static RealTimeLogger instance;
        public static RealTimeLogger Instance => instance ??= new RealTimeLogger();

        /// <summary>
        /// Retrieve the path to save the real-time state logs.
        /// </summary>
        public string GetFilePath()
        {
            ConfigManager config = new();
            config.LoadConfig();
            string logPath = config.GetRealTimeFilePath();
            string filePathRealTime = Path.Combine(logPath, "state.json");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            return filePathRealTime;
        }

        /// <summary>
        /// Retrieve the list of the real-time state logs.
        /// </summary>
        public List<dynamic> LoadRealTimeList()
        {

            string filePathRealTime = GetFilePath();
            if (!File.Exists(filePathRealTime))
            {
                File.WriteAllText(filePathRealTime, "[]"); // Cr�ation du fichier vide
                return new List<dynamic>();
            }

            string jsonContent = File.ReadAllText(filePathRealTime);
            return string.IsNullOrWhiteSpace(jsonContent) ? new List<dynamic>() : JsonSerializer.Deserialize<List<dynamic>>(jsonContent) ?? new List<dynamic>();

        }

        /// <summary>
        /// Add or modify the file list in the JSON to update the real-time logs.
        /// </summary>
        /// <param name="files">The list of files that need to be copied.</param>
        /// <param name="file">The files that need to be copied.</param>
        /// <param name="saveName">The name of the save.</param>
        /// <param name="timeStamp">The time when the file was copied.</param>
        /// <param name="IsActive">Say whether the backup is active or not.</param>
        /// <param name="sourceFilePath">The file source.</param>
        /// <param name="targetFilePath">The file destination.</param>
        public void UpdateState(string[] files, string file, string saveName, DateTime timeStamp, bool IsActive, string sourceFilePath, string targetFilePath)
        {
            long totalFilesSize = new DirectoryInfo(sourceFilePath).EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
            long totalFilesToCopy = files.Length;
            long nbFilesLeftToDo = files.Length - (Array.IndexOf(files, file) + 1);
            long totalFilesSizeLeft = files.Skip(Array.IndexOf(files, file) + 1).Sum(f => new FileInfo(f).Length);

            var realTime = new
            {
                Name = saveName,
                SourceFilePath = file,
                TargetFilePath = targetFilePath,
                State = IsActive ? "ACTIVE" : "END",
                TotalFilesToCopy = IsActive ? totalFilesToCopy : 0,
                TotalFilesSize = IsActive ? totalFilesSize : 0,
                NbFilesLeftToDo = IsActive ? nbFilesLeftToDo : 0,
                TotalFilesSizeLeft = IsActive ? totalFilesSizeLeft : 0,
                Progression = totalFilesToCopy > 0 ? ((totalFilesToCopy - nbFilesLeftToDo) * 100 / totalFilesToCopy) : 0
            };

            List<dynamic> realTimeList = LoadRealTimeList();

            int index = realTimeList.FindIndex(state =>
            {
                if (state is JsonElement element && element.TryGetProperty("Name", out JsonElement nameElement))
                {
                    return nameElement.GetString() == saveName;
                }
                return false;
            });

            if (index != -1)
            {
                realTimeList[index] = realTime;
            }
            else
            {
                realTimeList.Add(realTime);
            }

            WriteLogToFile(realTimeList);
        }

        /// <summary>
        /// Write the list of real-time logs to a JSON file.
        /// </summary>
        /// <param name="logContent">The list of real-times state logs.</param>
        public void WriteLogToFile(List<dynamic> logContent)
        {
            string filePathRealTime = GetFilePath();
            string jsonContent = JsonSerializer.Serialize(logContent, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePathRealTime, jsonContent);
        }

        /// <summary>
        /// Delete the real-time logs when the backup is finished.
        /// </summary>
        public void DeleteStateFile()
        {
            string filePathRealTime = GetFilePath();
            if (File.Exists(filePathRealTime))
            {
                File.Delete(filePathRealTime);
            }
        }
    }
}