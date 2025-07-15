using EasySave.Core.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using logger;

namespace EasySave.Core.Services
{

    public class Differential : ISave
    {

        // Event triggered to send real-time log messages.
        public event Action<string>? OnMessageLogged;

        /// <summary>
        /// Executes a differential save operation.
        /// Copies only new or modified files from the source path to the destination path.
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


            foreach (string file in files) // Loop through each file and process it
            {
                FileCopie(file, job, files);
            }

            // Notify that the differential save operation is finished
            OnMessageLogged?.Invoke(LanguageManager.GetString("DifferentialSaveSuccess"));
        }

        public void FileCopie(string file, Job job, string[] files)
        {

            DateTime timeStamp = DateTime.Now;

            // Calculate time for the log and realtime
            Stopwatch startTime = new();
            startTime.Start();

            // Construct the full path (path + file) for source and destination
            string destinationFile = Path.Combine(job.DestinationPath, Path.GetFileName(file));
            string sourceFile = Path.Combine(job.SourcePath, Path.GetFileName(file));

            // Check if the file exists in the destination directory
            if (File.Exists(destinationFile))
            {
                // Retrieve last modification times
                DateTime sourceFileLastWriteTime = File.GetLastWriteTime(file);
                DateTime destinationFileLastWriteTime = File.GetLastWriteTime(destinationFile);

                // If the source file is different than the destination, copy it
                if (sourceFileLastWriteTime > destinationFileLastWriteTime)
                {
                    try
                    {
                        File.Copy(file, destinationFile, true); // Copy the file to the destination --> true or false to overwrite
                        RealTimeLogger.Instance.UpdateState(files, file, job.Name, timeStamp, true, job.SourcePath, destinationFile);
                        OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("ModifiedFileCopied"), file, destinationFile));
                    }
                    catch (Exception ex)
                    {

                        OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("FileCopyError"), file, ex.Message));
                    }
                }
                else
                {
                    // If the file has not been modified, log that it was skipped
                    OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("FileNotModified"), file));
                    
                }
            }
            else
            {
                // If the file does not exist in the destination directory, copy it
                try
                {
                    File.Copy(file, destinationFile, true); // Copy the file to the destination --> true or false to overwrite
                    RealTimeLogger.Instance.UpdateState(files, file, job.Name, timeStamp, true, job.SourcePath, destinationFile);
                    OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("NewFileCopied"), file, destinationFile));
                }
                catch (Exception ex)
                {
                    OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("FileCopyError"), file, ex.Message));
                }
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
