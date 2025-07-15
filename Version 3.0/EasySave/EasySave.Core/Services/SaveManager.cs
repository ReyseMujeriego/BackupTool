using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using EasySave.Core.Models;

namespace EasySave.Core.Services
{
    public class SaveManager
    {
        private ISave? savemethod;

        public event Action<string>? OnMessageLogged;

        public static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);

        private readonly Dictionary<Job, (Thread thread, string Status, ManualResetEvent PauseEvent, CancellationTokenSource CancellationTokenSource)> _jobControls = [];

        public List<string> priorityExtensions = new() { };
        public int largeFileSizeLimit;

        public void PerformSaveSequential(List<Job> jobs)
        {
            // Notify that multiple jobs are starting
            OnMessageLogged?.Invoke(LanguageManager.GetString("MultipleJobsExecuting"));


            List<Thread> threads = new();
            if (!JobManager.Instance.EncryptionSettings.EnableEncryption)
            {
                OnMessageLogged?.Invoke(LanguageManager.GetString("EncryptionDisabled"));
            }

            ManualResetEvent pauseEvent = new(true);
            var cancellationTokenSource = new CancellationTokenSource();


            // If the extension priority is enabled
            foreach (var job in jobs)
            {
                // Create a new thread for each job
                Thread thread = new(() => PerformSave(job, priorityExtensions, true, largeFileSizeLimit, cancellationTokenSource.Token, pauseEvent, _jobControls[job].Status));
                _jobControls[job] = (thread, "InProgress", pauseEvent, cancellationTokenSource);
                threads.Add(thread);
                thread.Start(); // Start the thread
            }
            MonitorBusinessSoftware();
            foreach (var thread in threads)
            {
                thread.Join(); // Wait each thread finishes before continuing
            }

            threads.Clear(); // Reset thread list

            // If the extension priority is false
            foreach (var job in jobs)
            {
                // Create a new thread for each job
                Thread thread = new(() => PerformSave(job, priorityExtensions, false, largeFileSizeLimit, cancellationTokenSource.Token, pauseEvent, _jobControls[job].Status));
                _jobControls[job] = (thread, "InProgress", pauseEvent, cancellationTokenSource);
                threads.Add(thread);
                thread.Start();

            }

            foreach (var thread in threads)
            {
                thread.Join(); // Wait each thread finishes before continuing
            }

            threads.Clear(); // Reset thread list
        }

        public void PerformSave(Job job, List<string> extensions, bool isPriority, int largeFileSizeLimit, CancellationToken cancellationToken, ManualResetEvent pauseEvent, string Status)
        {

            switch (job.SaveType)
            {
                case SaveTypes.Complete:
                    savemethod = new Complete(Semaphore);
                    break;

                case SaveTypes.Differential:
                    savemethod = new Differential(Semaphore);
                    break;

                default:
                    OnMessageLogged?.Invoke(LanguageManager.GetString("UnknownSaveType"));
                    return;
            }

            if (savemethod == null) return;

            // Subscribe to the save method's OnMessageLogged event to relay messages
            savemethod.OnMessageLogged += (message) => OnMessageLogged?.Invoke(message);

            // Execute the save operation for the job
            savemethod.ExecuteSave(job, extensions, isPriority, largeFileSizeLimit, cancellationToken, pauseEvent, Status);

            savemethod.OnMessageLogged -= (message) => OnMessageLogged?.Invoke(message);

            RealTimeLogger.Instance.DeleteStateFile(); // Resets the state by deleting the state file and free up the memory
        }

        public void PauseJob(List<Job> jobs)
        {
            foreach (var job in jobs)
            {
                if (_jobControls.TryGetValue(job, out var control))
                {
                    control.PauseEvent.Reset();
                    _jobControls[job] = (control.thread, "Paused", control.PauseEvent, control.CancellationTokenSource);
                    OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("JobPaused"), job.Name));
                }
            }
        }

        public void ResumeJob(List<Job> jobs)
        {
            foreach (var job in jobs)
            {
                if (_jobControls.TryGetValue(job, out var control))
                {
                    control.PauseEvent.Set();
                    _jobControls[job] = (control.thread, "InProgress", control.PauseEvent, control.CancellationTokenSource);
                    OnMessageLogged?.Invoke(string.Format(LanguageManager.GetString("JobResumed"), job.Name));
                }
            }
        }

        public void StopJob(List<Job> jobs)
        {
            OnMessageLogged?.Invoke(LanguageManager.GetString("StopEnabled"));
            foreach (var job in jobs)
            {

                if (_jobControls.TryGetValue(job, out var control))
                {

                    control.PauseEvent.Reset();
                    control.CancellationTokenSource.Cancel();
                    _jobControls[job] = (control.thread, "Stopped", control.PauseEvent, control.CancellationTokenSource);
                }

            }
        }
        private void MonitorBusinessSoftware()
        {
            Task.Run(() =>
            {
                bool isPaused = false;

                while (_jobControls.Count > 0)
                {
                    if (JobManager.IsBusinessSoftwareRunning())
                    {
                        if (!isPaused) // Évite de remettre en pause en boucle
                        {
                            OnMessageLogged?.Invoke(LanguageManager.GetString("BusinessPause"));

                            foreach (var job in _jobControls.Keys)
                            {
                                if (_jobControls.TryGetValue(job, out var control))
                                {
                                    control.CancellationTokenSource.Cancel(); // Demande la pause via CancellationToken
                                }
                            }

                            isPaused = true;
                        }
                    }
                    else
                    {
                        if (isPaused) // Évite de spammer la reprise en boucle
                        {
                            OnMessageLogged?.Invoke(LanguageManager.GetString("BusinessResume"));

                            foreach (var job in _jobControls.Keys)
                            {
                                if (_jobControls.TryGetValue(job, out var control))
                                {
                                    var newTokenSource = new CancellationTokenSource(); // Nouveau token pour la reprise
                                    _jobControls[job] = (control.thread, "InProgress", control.PauseEvent, newTokenSource);
                                }
                            }

                            ResumeJob(_jobControls.Keys.ToList());
                            isPaused = false;
                        }
                    }

                    Thread.Sleep(3000);
                }
            });
        }




    }
}
