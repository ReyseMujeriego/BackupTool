using System.Text.Json;
using Logger.Services;

namespace EasySave.Core.Services
{
    public class RealTimeLogger
    {
        private static RealTimeLogger? instance;
        public static RealTimeLogger Instance => instance ??= new RealTimeLogger();

        private static readonly Mutex fileMutex = new(); // Mutex --> to 1 enter and the other in a queue

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

            // Wait the first one and stop de others in a queue
            fileMutex.WaitOne();
            try
            {
                if (!File.Exists(filePathRealTime))
                {
                    File.WriteAllText(filePathRealTime, "[]"); // Crée le fichier vide si nécessaire
                    return new List<dynamic>();
                }

                string jsonContent = File.ReadAllText(filePathRealTime);
                return string.IsNullOrWhiteSpace(jsonContent) ? new List<dynamic>() : JsonSerializer.Deserialize<List<dynamic>>(jsonContent) ?? new List<dynamic>();
            }
            finally
            {
                // Free the mutex after the file access
                fileMutex.ReleaseMutex();
            }
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
        public void UpdateState(string[] files, string file, string saveName, DateTime timeStamp, bool IsActive, string sourceFilePath, string targetFilePath, string Status)
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
                State = Status,
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
        /// <summary>
        /// Write the list of real-time logs to a JSON file.
        /// </summary>
        public void WriteLogToFile(List<dynamic> logContent)
        {
            string filePathRealTime = GetFilePath();

            // Wait the first one and stop de others in a queue
            fileMutex.WaitOne();
            try
            {
                string jsonContent = JsonSerializer.Serialize(logContent, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePathRealTime, jsonContent);
            }
            finally
            {
                // Free the mutex after file access
                fileMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Delete the real-time logs when the backup is finished.
        /// </summary>
        public void DeleteStateFile()
        {
            string filePathRealTime = GetFilePath();

            // Wait the first one and stop de others in a queue
            fileMutex.WaitOne();
            try
            {
                if (File.Exists(filePathRealTime))
                {
                    File.Delete(filePathRealTime);
                }
            }
            finally
            {
                // Free the mutex
                fileMutex.ReleaseMutex();
            }
        }
    }
}