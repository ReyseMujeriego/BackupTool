using System.Diagnostics;
using EasySave.Core.Models;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Manages job creation, updates, deletion, listing, and execution.
    /// Implements a Singleton pattern to ensure a single instance.
    /// Communicates with SaveManager to execute jobs.
    /// </summary>
    public class JobManager
    {
        // List of all jobs created and managed
        private readonly List<Job> _jobs;

        // Instance of SaveManager to execute job operations
        private readonly SaveManager saver;

        public EncryptionSettings EncryptionSettings { get; private set; } = new EncryptionSettings();

        public event EventHandler<Job>? JobAdded;

        /// <summary>
        /// Event triggered to send real-time log messages.
        /// Used for UI updates or logging purposes.
        /// </summary>
        public event Action<string>? OnMessageLogged;

        /// <summary>
        /// Private constructor (Singleton pattern).
        /// Initializes the job list and subscribes to SaveManager's logging events.
        /// </summary>
        private JobManager()
        {
            _jobs = [];
            saver = new SaveManager();
            saver.OnMessageLogged += (message) => OnMessageLogged?.Invoke(message);
        }

        // Singleton instance
        private static JobManager? _instance;

        /// <summary>
        /// Singleton pattern - Ensures only one instance of JobManager exists.
        /// </summary>
        public static JobManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new JobManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Adds a new job to the list if it doesn't already exist.
        /// </summary>
        /// <param name="job">The job to be added.</param>
        public void AddJob(Job job)
        {
            // Check if a job with the same name already exists
            if (SearchJob(job.Name) != null)
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("JobExists", job.Name));
                return;
            }

            _jobs.Add(job);
            OnMessageLogged?.Invoke(LanguageManager.GetString("JobAdded", job.Name, job.SaveType, job.SourcePath, job.DestinationPath));
            JobAdded?.Invoke(this, job);
        }

        /// <summary>
        /// Removes a job from the list if it exists.
        /// </summary>
        /// <param name="jobName">The name of the job to remove.</param>
        public void RemoveJob(string jobName)
        {
            Job jobToRemove = SearchJob(jobName);
            if (jobToRemove != null)
            {
                _jobs.Remove(jobToRemove);
                OnMessageLogged?.Invoke(LanguageManager.GetString("JobDeleted", jobName));
            }
            else
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("JobNotFound", jobName));
            }
        }

        /// <summary>
        /// Updates an existing job with new details.
        /// </summary>
        /// <param name="jobName">The name of the job to update.</param>
        /// <param name="newName">New name for the job (optional).</param>
        /// <param name="newSource">New source path (optional).</param>
        /// <param name="newDestination">New destination path (optional).</param>
        public void UpdateJob(string jobName, string? newName, string? newSource, string? newDestination)
        {
            Job jobToUpdate = SearchJob(jobName);
            if (jobToUpdate != null)
            {
                // Update only if the new values are not empty
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    jobToUpdate.Name = newName;
                }

                if (!string.IsNullOrWhiteSpace(newSource))
                {
                    jobToUpdate.SourcePath = newSource;
                }

                if (!string.IsNullOrWhiteSpace(newDestination))
                {
                    jobToUpdate.DestinationPath = newDestination;
                }

                // Notify about the update
                OnMessageLogged?.Invoke(LanguageManager.GetString("JobUpdated", jobName, jobToUpdate.Name, jobToUpdate.SaveType, jobToUpdate.SourcePath, jobToUpdate.DestinationPath));
            }
            else
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("JobNotFound", jobName));
            }
        }

        /// <summary>
        /// Retrieves and optionally displays the list of all saved jobs.
        /// </summary>
        /// <param name="isConsoleMode">
        /// If set to <c>true</c>, the job list is displayed in the console;
        /// otherwise, the list is returned for further use in a UI (e.g., WPF).
        /// </param>
        /// <returns>
        /// A list of <see cref="Job"/> objects. If there are no jobs, returns an empty list.
        /// </returns>
        public List<Job> ListJobs(bool isConsoleMode = false)
        {
            // ✅ If no jobs exist, log a message and return an empty list
            if (_jobs.Count == 0)
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("NoJobs"));
                return new List<Job>();
            }

            // ✅ If in console mode, log the list of jobs
            if (isConsoleMode)
            {
                OnMessageLogged?.Invoke("\n📁 " + LanguageManager.GetString("JobList") + ":\n");
                for (int i = 0; i < _jobs.Count; i++)
                {
                    var job = _jobs[i];
                    OnMessageLogged?.Invoke($"{i + 1} - {job.Name} ({job.SaveType}) : {job.SourcePath} → {job.DestinationPath}");
                }
            }

            // ✅ Return a copy of the job list for UI usage (WPF)
            return new List<Job>(_jobs);
        }

        /// <summary>
        /// Executes a single job by its index.
        /// </summary>
        /// <param name="index">The index of the job in the list.</param>
        public void ExecuteJob(int index)
        {
            if (index >= 0 && index < _jobs.Count)
            {
                var job = _jobs[index];
                OnMessageLogged?.Invoke(LanguageManager.GetString("ExecutingJob", job.Name));
                saver.PerformSave(job);
            }
            else
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("JobIndexNotFound", index + 1));
            }
        }

        /// <summary>
        /// Executes multiple jobs sequentially.
        /// </summary>
        /// <param name="indices">Array of job indices to execute.</param>
        public void ExecuteJobsSequentially(params int[] indices)
        {
            List<Job> selectedJobs = new();

            // Validate job indices
            foreach (int index in indices)
            {
                if (index >= 0 && index < _jobs.Count)
                {
                    selectedJobs.Add(_jobs[index]);
                }
                else
                {
                    OnMessageLogged?.Invoke(LanguageManager.GetString("JobIndexNotFound", index + 1));
                    return;
                }
            }

            if (selectedJobs.Count > 0)
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("ExecutingMultipleJobs", selectedJobs.Count));
                saver.PerformSaveSequential(selectedJobs);
            }
            else
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("NoJobsToExecute"));
            }
        }

        /// <summary>
        /// Searches for a job by name.
        /// </summary>
        /// <param name="name">The name of the job.</param>
        /// <returns>The job object if found, otherwise null.</returns>
        public Job SearchJob(string name)
        {
            return _jobs.Find(job => job.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsBusinessSoftwareRunning()
        {
            string[] businessProcesses = { "CalculatorApp", "calc.exe" };
            return Process.GetProcesses().Any(p => businessProcesses.Contains(p.ProcessName));
        }
    }
}