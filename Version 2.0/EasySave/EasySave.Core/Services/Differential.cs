using System.Diagnostics;
using EasySave.Core.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EasySave.Core.Services
{
    public class Differential : ISave
    {
        // Event triggered to send real-time log messages.
        public event Action<string>? OnMessageLogged;
        
        /// <summary>
        /// Executes a differential save operation.
        /// Copies only modified files from the source path to the destination path.
        /// </summary>
        /// <param name="job">The job containing source and destination paths.</param>
        public void ExecuteSave(Job job)
        {
            // Notify the start of the differential save operation
            OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("DifferentialSaveStart"), job.SourcePath, job.DestinationPath));

            // Check if the source directory exists
            if (!Directory.Exists(job.SourcePath))
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("SourceDirectoryNotFound"));
                return;
            }

            // Check if the destination directory exists and create it if necessary
            if (!Directory.Exists(job.DestinationPath))
            {
                Directory.CreateDirectory(job.DestinationPath);
                OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("DestinationCreated"), job.DestinationPath));
            }

            // Get all files from the source directory (including subdirectories)
            string[] files = Directory.GetFiles(job.SourcePath, "*", SearchOption.AllDirectories);

            foreach (string file in files) // Loop through each file and copy only if modified
            {
                FileCopie(file, job, files);
            }

            OnMessageLogged?.Invoke(LanguageManager.GetString("DifferentialSaveSuccess")); // Notify that the save operation is finished
        }

        /// <summary>
        /// Copies a single file from the source directory to the destination if it has been modified.
        /// </summary>
        /// <param name="file">The file to be copied.</param>
        /// <param name="job">The job containing source and destination paths.</param>
        /// <param name="files">The list of files that need to be copied.</param>
        public void FileCopie(string file, Job job, string[] files)
        {
            DateTime timeStamp = DateTime.Now;

            // Construct the full path (path + file) for source and destination
            string relativePath = Path.GetRelativePath(job.SourcePath, file);
            string destinationFile = Path.Combine(job.DestinationPath, relativePath);

            // Ensure the destination folder exists
            string? destinationDirectory = Path.GetDirectoryName(destinationFile);
            if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            int encryptionTime = 0; // Default value (0 = no encryption)

            try
            {
                encryptionTime = CryptoService.EncryptFile(file);
            }
            catch (Exception ex)
            {
                encryptionTime = -1; // Negative value in case of encryption error
                OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("FileCopyError"), file, ex.Message));
            }

            // Check if the file exists and is modified
            if (File.Exists(destinationFile) && File.GetLastWriteTime(file) <= File.GetLastWriteTime(destinationFile))
            {
                return; // Skip unchanged files
            }

            // Calculate time for the log and realtime
            Stopwatch startTime = new();
            startTime.Start();

            try
            {
                RealTimeLogger.Instance.UpdateState(files, file, job.Name, timeStamp, true, job.SourcePath, destinationFile);
                File.Copy(file, destinationFile, true); // Copy the file to the destination
            }
            catch (Exception ex)
            {
                encryptionTime = -1;
                OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("FileCopyError"), file, ex.Message));
            }

            // Calculate time for the log and realtime
            startTime.Stop();
            double deltaTime = startTime.Elapsed.TotalMilliseconds;

            // Ensure the file exists before logging
            if (File.Exists(destinationFile))
            {
                long length = new System.IO.FileInfo(destinationFile).Length; // Calculate the length for each file

                // Update real-time state
                RealTimeLogger.Instance.UpdateState(files, file, job.Name, timeStamp, false, job.SourcePath, destinationFile);

                // Log the file transfer
                Logger.Services.Logger.Instance.LogFileTransfert(new Logger.Models.LoggerModel
                {
                    TimeStamp = DateTime.Now,
                    SaveName = job.Name,
                    FileSize = length,
                    FileTransferTime = deltaTime,
                    FileSource = job.SourcePath,
                    FileTarget = destinationFile,
                    EncryptionTime = encryptionTime
                });
            }
            else
            {
                OnMessageLogged?.Invoke($"❌ Error: File '{destinationFile}' was not found after copying.");
            }
        }
    }
}