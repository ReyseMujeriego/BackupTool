using System.Diagnostics;
using System.Text;
using EasySave.Core.Models;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Manages the execution of save operations (Complete or Differential).
    /// Uses the Strategy Pattern to determine the appropriate save method dynamically.
    /// </summary>
    public class SaveManager
    {
        // Instance of the ISave interface
        private ISave? savemethod;

        // Event triggered when a message needs to be logged in real-time.
        public event Action<string>? OnMessageLogged;

        /// <summary>
        /// Executes multiple jobs sequentially.
        /// Calls PerformSave() on each job in the provided list.
        /// </summary>
        /// <param name="jobs">List of jobs to be executed.</param>
        public void PerformSaveSequential(List<Job> jobs)
        {
            // Notify that multiple jobs are starting
            OnMessageLogged?.Invoke(LanguageManager.GetString("MultipleJobsExecuting"));

            foreach (var job in jobs) // Loop through each job
            {
                PerformSave(job);
            }

            // Notify that all jobs have been completed
            OnMessageLogged?.Invoke(LanguageManager.GetString("MultipleJobsComplete"));
        }

        /// <summary>
        /// Executes a single job using the appropriate save method (Complete or Differential).
        /// Uses the Strategy Pattern to determine the correct implementation.
        /// </summary>
        /// <param name="job">The job to be executed.</param>
        public void PerformSave(Job job)
        {
            // Determine which save method to use based on the job's SaveType
            switch (job.SaveType)
            {
                case SaveTypes.Complete:
                    savemethod = new Complete(); // Uses the Complete save strategy
                    break;

                case SaveTypes.Differential:
                    savemethod = new Differential(); // Uses the Differential save strategy
                    break;

                default:
                    OnMessageLogged?.Invoke(LanguageManager.GetString("UnknownSaveType"));
                    return;
            }

            if (savemethod == null)
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("UnknownSaveType"));
                return;
            }

            // Subscribe to the save method's OnMessageLogged event to relay messages
            savemethod.OnMessageLogged += (message) => OnMessageLogged?.Invoke(message);

            // Execute the save operation for the job
            savemethod.ExecuteSave(job);
            savemethod.OnMessageLogged -= (message) => OnMessageLogged?.Invoke(message);

            RealTimeLogger.Instance.DeleteStateFile(); // Resets the state by deleting the state file and free up the memory
        }
    }
}