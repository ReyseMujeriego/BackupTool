using System.Diagnostics;
using EasySave.Core.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EasySave.Core.Services
{
    public class Differential : ISave
    {
        // Event triggered to send real-time log messages.
        public event Action<string>? OnMessageLogged;

        public readonly SemaphoreSlim Semaphore;
        //private const int LargeFileSizeLimit = 1024 * 1500;

        public Differential(SemaphoreSlim semaphore)
        {
            Semaphore = semaphore;
        }

        /// <summary>
        /// Executes a differential save operation.
        /// Copies only modified files from the source path to the destination path.
        /// </summary>
        /// <param name="job">The job containing source and destination paths.</param>
        public void ExecuteSave(Job job, List<string> priorityExtensions, bool isPriority, int largeFileSizeLimit, CancellationToken cancellationToken, ManualResetEvent pauseEvent, string Status)
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

            var filteredFiles = files.Where(file =>
            {
                string extension = Path.GetExtension(file);
                bool isFilePriority = priorityExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
                return isPriority ? isFilePriority : !isFilePriority;
            }).ToArray();

            foreach (string file in filteredFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("CopyCancelled"), file));
                    break;
                }

                long fileSize = new FileInfo(file).Length;
                bool isLargeFile = fileSize > largeFileSizeLimit;

                if (isLargeFile)
                {
                    Thread thread = new Thread(() =>
                    {
                        if (!cancellationToken.IsCancellationRequested)
                            FileCopie(file, job, filteredFiles, largeFileSizeLimit, cancellationToken, pauseEvent, Status);
                    });
                    thread.Start();
                }
                else
                {
                    FileCopie(file, job, filteredFiles, largeFileSizeLimit, cancellationToken, pauseEvent, Status);
                }
            }

            OnMessageLogged?.Invoke(LanguageManager.GetString("DifferentialSaveSuccess")); // Notify that the save operation is finished
        }

        private object _progressLock = new();
        private int _lastProgress = -1; // Shared variable to track progress

        /// <summary>
        /// Copies a single file from the source directory to the destination if it has been modified.
        /// </summary>
        /// <param name="file">The file to be copied.</param>
        /// <param name="job">The job containing source and destination paths.</param>
        /// <param name="files">The list of files that need to be copied.</param>
        public void FileCopie(string file, Job job, string[] files, int LargeFileSizeLimit, CancellationToken cancellationToken, ManualResetEvent pauseEvent, string Status)
        {
            DateTime timeStamp = DateTime.Now;

            // Construct the full path (path + file) for source and destination
            string relativePath = Path.GetRelativePath(job.SourcePath, file);
            string destinationFile = Path.Combine(job.DestinationPath, relativePath);

            long fileSize = new FileInfo(file).Length;

            EnsureDestinationDirectory(destinationFile);

            int encryptionTime = CryptoService.EncryptFile(file);

            // Check if the file exists and is modified
            if (File.Exists(destinationFile) && File.GetLastWriteTime(file) >= File.GetLastWriteTime(destinationFile))
            {
                return; // Skip unchanged files
            }

            // Calculate time for the log and realtime
            Stopwatch startTime = new();
            startTime.Start();
            try
            {
                pauseEvent.WaitOne();
                if (fileSize > LargeFileSizeLimit)
                {
                    Semaphore.Wait();

                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("CopyCancelled"), file));
                            return;
                        }

                        pauseEvent.WaitOne();

                        RealTimeLogger.Instance.UpdateState(files, file, job.Name, timeStamp, true, job.SourcePath, destinationFile, Status);
                        File.Copy(file, destinationFile, true); // Copy the file to the destination
                        // Calculate time for the log and realtime
                        startTime.Stop();
                        double deltaTime = startTime.Elapsed.TotalMilliseconds;
                        LogFileTransfer(files, file, job, timeStamp, destinationFile, deltaTime, encryptionTime, Status);
                    }
                    finally
                    {
                        Semaphore.Release();
                    }
                }
                else
                {

                    if (cancellationToken.IsCancellationRequested)
                    {
                        OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("CopyCancelled"), file));
                        return;
                    }

                    pauseEvent.WaitOne();

                    RealTimeLogger.Instance.UpdateState(files, file, job.Name, timeStamp, true, job.SourcePath, destinationFile, Status);
                    File.Copy(file, destinationFile, true); // Copy the file to the destination
                    // Calculate time for the log and realtime
                    startTime.Stop();
                    double deltaTime = startTime.Elapsed.TotalMilliseconds;
                    LogFileTransfer(files, file, job, timeStamp, destinationFile, deltaTime, encryptionTime, Status);
                }

                // Calculate progress and update only if it increases
                int totalFilesToCopy = files.Length;
                int index = Array.IndexOf(files, file);

                if (index >= 0 && index < totalFilesToCopy)
                {
                    int progress = (index + 1) * 100 / totalFilesToCopy;

                    lock (_progressLock)
                    {
                        if (progress > _lastProgress)
                        {
                            _lastProgress = progress;
                            OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("Progression"), job.Name, progress));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("FileCopyError"), file, ex.Message));
            }
        }

        /// <summary>
        /// Logs the file transfer details after ensuring the file has been successfully copied.
        /// </summary>
        /// <param name="files">The list of files to be copied.</param>
        /// <param name="file">The current file being copied.</param>
        /// <param name="job">The job containing source and destination paths.</param>
        /// <param name="timeStamp">The timestamp when the file transfer started.</param>
        /// <param name="destinationFile">The destination path of the file.</param>
        /// <param name="deltaTime">The time taken for the file transfer, in milliseconds.</param>
        /// <param name="encryptionTime">The time taken for file encryption, in milliseconds.</param>
        private void LogFileTransfer(string[] files, string file, Job job, DateTime timeStamp, string destinationFile, double deltaTime, int encryptionTime, string Status)
        {
            // Ensure the file exists before logging
            if (File.Exists(destinationFile))
            {
                long length = new System.IO.FileInfo(destinationFile).Length; // Calculate the length for each file

                // Update real-time state
                RealTimeLogger.Instance.UpdateState(files, file, job.Name, timeStamp, false, job.SourcePath, destinationFile, Status);

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
                OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("FileNotFound", destinationFile)));
            }
        }

        /// <summary>
        /// Ensures that the destination directory exists before performing any file operations.
        /// If the directory doesn't exist, it will be created.
        /// </summary>
        /// <param name="destinationFile">The path of the file to be copied. The directory of this file will be checked.</param>
        private void EnsureDestinationDirectory(string destinationFile)
        {
            string? destinationDirectory = Path.GetDirectoryName(destinationFile);
            if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }
        }
    }
}