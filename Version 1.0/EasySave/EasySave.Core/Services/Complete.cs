using System.Diagnostics;
using EasySave.Core.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using logger;
namespace EasySave.Core.Services
{

    public class Complete : ISave
    {
        
        // Event triggered to send real-time log messages.
        public event Action<string>? OnMessageLogged;

        /// <summary>
        /// Executes a complete save operation.
        /// Copies all files from the source path to the destination path.
        /// </summary>
        /// <param name="job">The job containing source and destination paths.</param>
        public void ExecuteSave(Job job)
        {
            // Notify the start of the complete save operation
            OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("CompleteSaveStart"), job.SourcePath, job.DestinationPath));

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

            
            foreach (string file in files) // Loop through each file and copy it to the destination
            {
                FileCopie(file, job,files);
            }

            OnMessageLogged?.Invoke(LanguageManager.GetString("CompleteSaveSuccess")); // Notify that the complete save operation is finished

        }

        /// <summary>
        /// Copies a single file from the source directory to the destination.
        /// </summary>
        /// <param name="file">The file to be copied.</param>
        /// <param name="job">The job containing source and destination paths.</param>
        /// <param name="files">The list of files that need to be copied.</param>
        public void FileCopie(string file, Job job, string[] files)
        {

            DateTime timeStamp = DateTime.Now;

            // Calculate time for the log and realtime
            Stopwatch startTime = new();
            startTime.Start();

            // Construct the full path (path + file) for source and destination
            string destinationFile = Path.Combine(job.DestinationPath, Path.GetFileName(file));
            string sourceFile = Path.Combine(job.SourcePath, Path.GetFileName(file));

            try
            {
                RealTimeLogger.Instance.UpdateState(files, file, job.Name, timeStamp, true, job.SourcePath, destinationFile);
                File.Copy(file, destinationFile, true); // Copy the file to the destination --> true or false to overwrite
            }
            catch (Exception ex)
            {
                OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("FileCopyError"), file, ex.Message));

            }

            // Calculate time for the log and realtime
            startTime.Stop();
            double deltaTime = startTime.Elapsed.TotalMilliseconds;
            
            long length = new System.IO.FileInfo(sourceFile).Length; // Calculate the length for each file 

            RealTimeLogger.Instance.UpdateState(files, file, job.Name, timeStamp, false, job.SourcePath, destinationFile); // Call the RealTimeLogger function
            Logger.Instance.LogFileTransfert(timeStamp, job.Name, length, deltaTime, sourceFile, destinationFile); // Call the Logger function



        }
    }
}