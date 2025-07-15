using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EasySave.Core.Models;
using EasySave.Core.Services;
using EasySave.Desktop.Utils;
using Logger.Services;

namespace EasySave.Desktop.ViewModels
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        private readonly JobManager jobManager;
        private readonly SaveManager saveManager;
        private string _consoleLog = "";
        private Job _selectedJob;
        private string _language = "fr";
        public ObservableCollection<Job> Jobs { get; }
        private ObservableCollection<Job> _selectedJobs = [];

        public ObservableCollection<Job> SelectedJobs
        {
            get => _selectedJobs;
            set
            {
                _selectedJobs = value;
                OnPropertyChanged();
                (ExecuteMultipleJobsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string ConsoleLog
        {
            get => _consoleLog;
            set { _consoleLog = value; OnPropertyChanged(); }
        }

        public Job SelectedJob
        {
            get => _selectedJob;
            set
            {
                _selectedJob = value;
                OnPropertyChanged();

                (EditJobCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteJobCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ExecuteJobCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand AddJobCommand { get; }
        public ICommand EditJobCommand { get; }
        public ICommand DeleteJobCommand { get; }
        public ICommand ExecuteJobCommand { get; }
        public ICommand ExecuteMultipleJobsCommand { get; }
        public ICommand ChangeLanguageCommand { get; }
        public ICommand SetLogFormatCommand { get; }
        public ICommand ConfigureEncryptionCommand { get; }
        public ICommand DecryptFilesCommand { get; }

        public event EventHandler RequestAddJobWindow;

        public event EventHandler RequestEditJobWindow;

        public HomePageViewModel()
        {
            jobManager = JobManager.Instance;
            saveManager = new SaveManager();
            Jobs = new ObservableCollection<Job>(jobManager.ListJobs(false));

            AddJobCommand = new RelayCommand(_ => OnRequestAddJobWindow());
            EditJobCommand = new RelayCommand(_ => EditJob(), _ => CanModifyOrExecute());
            DeleteJobCommand = new RelayCommand(_ => DeleteJob(), _ => CanModifyOrExecute());
            ExecuteJobCommand = new RelayCommand(_ => ExecuteJob(), _ => CanModifyOrExecute());
            ExecuteMultipleJobsCommand = new RelayCommand(_ => ExecuteMultipleJobs(), _ => SelectedJobs.Count > 0);
            SetLogFormatCommand = new RelayCommand(param => SetLogFormat(param?.ToString()));
            ConfigureEncryptionCommand = new RelayCommand(_ => ConfigureEncryption());
            DecryptFilesCommand = new RelayCommand(_ => DecryptFiles());

            jobManager.JobAdded += OnJobAdded;
            jobManager.OnMessageLogged += AppendConsoleLog;
            saveManager.OnMessageLogged += AppendConsoleLog;
            CryptoService.OnMessageLogged += AppendConsoleLog;
        }

        /// <summary>
        /// Handles the event when a new job is added.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="newJob">The new job that was added</param>
        private void OnJobAdded(object sender, Job newJob)
        {
            Jobs.Add(newJob);
        }

        /// <summary>
        /// Appends a message to the console log.
        /// </summary>
        /// <param name="message">The log message</param>
        private void AppendConsoleLog(string message)
        {
            ConsoleLog += message + "\n";
        }

        private void OnRequestAddJobWindow()
        {
            RequestAddJobWindow?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// To update the new job
        /// </summary>
        /// <param name="SelectedJob">to take the job that the user has selected</param>
        private void EditJob()
        {
            if (SelectedJob == null)
            {
                MessageBox.Show(LanguageManager.GetString("SelectJobToEdit"), "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning); // Message box to confirm the choice
                return;
            }
            RequestEditJobWindow?.Invoke(this, EventArgs.Empty);

            // Create a new Job instance with the new data
            var updatedJob = new Job
            {
                Name = SelectedJob.Name,
                SourcePath = SelectedJob.SourcePath,
                DestinationPath = SelectedJob.DestinationPath
            };

            // Replace old Job with new one in collection at specific index
            int index = Jobs.IndexOf(SelectedJob);
            if (index >= 0)
            {
                Jobs[index] = updatedJob;  // Directly replaces the element at the job index
            }

            AppendConsoleLog(string.Format(LanguageManager.GetString("JobModified"), updatedJob.Name));
        }

        /// <summary>
        /// Deletes the selected job after confirmation.
        /// </summary>
        private void DeleteJob()
        {
            MessageBoxResult result = MessageBox.Show(LanguageManager.GetString("ConfirmDeleteJob"), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                return;
            }

            if (SelectedJob != null)
            {

                jobManager.RemoveJob(SelectedJob.Name);
                Jobs.Remove(SelectedJob);

            }
        }

        /// <summary>
        /// Executes the selected job.
        /// </summary>
        private void ExecuteJob()
        {
            int index = jobManager.ListJobs().FindIndex(j => j.Name == SelectedJob.Name);
            if (index != -1)
            {
                jobManager.ExecuteJob(index);
            }
        }

        /// <summary>
        /// Executes multiple selected jobs sequentially.
        /// Checks if any jobs are selected, retrieves their indices, and executes them in order.
        /// </summary>
        private void ExecuteMultipleJobs()
        {
            // Ensure at least one job is selected
            if (SelectedJobs.Count == 0)
            {
                MessageBox.Show(LanguageManager.GetString("SelectAtLeastOneJob"), "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Retrieve indices of selected jobs
            List<int> indices = jobManager.ListJobs()
                .Select((j, i) => new { Job = j, Index = i })
                .Where(j => SelectedJobs.Contains(j.Job))
                .Select(j => j.Index)
                .ToList();

            if (indices.Count > 0)
            {
                // Execute selected jobs sequentially
                jobManager.ExecuteJobsSequentially([.. indices]);
                AppendConsoleLog(string.Format(LanguageManager.GetString("MultipleJobsExecuted"), indices.Count));
            }
        }


        /// <summary>
        /// Verifies if the selected job can be modified or executed
        /// </summary>
        private bool CanModifyOrExecute()
        {
            return SelectedJob != null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets the log format for logging operations.
        /// Validates the input format and updates the configuration accordingly.
        /// </summary>
        /// <param name="format">The log format string to be set.</param>
        public void SetLogFormat(string format)
        {
            if (string.IsNullOrEmpty(format)) return;

            // Call ConfigManager to save the log format
            ConfigManager configManager = new();
            configManager.SetLogFormat(format);

            // Add a console message to confirm the change
            AppendConsoleLog(string.Format(LanguageManager.GetString("LogFormatChanged"), format.ToUpper()));

            // Force UI refresh
            OnPropertyChanged(nameof(ConsoleLog));
        }


        /// <summary>
        /// Configures the encryption settings based on user input.
        /// Prompts the user to enable or disable encryption, and if enabled,
        /// asks for an encryption key and file extensions to encrypt.
        /// The settings are then applied to the job manager.
        /// </summary>
        private void ConfigureEncryption()
        {
            // Ask the user if they want to enable encryption
            MessageBoxResult result = MessageBox.Show(LanguageManager.GetString("ConfirmEnableEncryption"), "Configuration Encryption", MessageBoxButton.YesNo, MessageBoxImage.Question);
            bool enableEncryption = result == MessageBoxResult.Yes;

            string encryptionKey = "";
            List<string> extensions = [];

            if (enableEncryption)
            {
                
                // Prompt user to enter the encryption key
                encryptionKey = Microsoft.VisualBasic.Interaction.InputBox(LanguageManager.GetString("EnterEncryptionKey"), "Clé de chiffrement", "MaCléSecrète");

                // Prompt user to enter the file extensions to encrypt (comma-separated)
                string? extensionsInput = Microsoft.VisualBasic.Interaction.InputBox(LanguageManager.GetString("EnterEncryptionExtensions"), "Extensions à chiffrer", ".txt,.pdf");
                AppendConsoleLog(string.Format(LanguageManager.GetString("EncryptionConfigured")));
                // Convert input string to a list of trimmed, lowercase extensions
                if (!string.IsNullOrWhiteSpace(extensionsInput))
                    extensions = extensionsInput.Split(',').Select(e => e.Trim().ToLower()).ToList();
            }

            // Apply encryption settings to the job manager
            jobManager.EncryptionSettings.EnableEncryption = enableEncryption;
            jobManager.EncryptionSettings.EncryptionKey = encryptionKey;
            jobManager.EncryptionSettings.EncryptedExtensions = extensions;

            // Log the result to the console
            AppendConsoleLog(enableEncryption
               ? LanguageManager.GetString("EncryptionEnabled")
               : LanguageManager.GetString("EncryptionDisabled"));
        }

        /// <summary>
        /// Decrypts files based on user input.
        /// The user is prompted to decrypt files by job name or by a custom path.
        /// The corresponding files are then decrypted using the stored encryption key.
        /// </summary>
        private void DecryptFiles()
        {
            // Ask the user whether to decrypt a job or a custom path
            MessageBoxResult result = MessageBox.Show(LanguageManager.GetString("DecryptJobOrPath"), "Déchiffrement", MessageBoxButton.YesNo, MessageBoxImage.Question);

            string? jobName = null;
            string? customPath = null;

            if (result == MessageBoxResult.Yes)
            {
                // Prompt user to enter the job name to decrypt
                jobName = Microsoft.VisualBasic.Interaction.InputBox(LanguageManager.GetString("EnterJobToDecrypt"), "Déchiffrement de job", "");
            }
            else
            {
                // Prompt user to enter a custom file path to decrypt
                customPath = Microsoft.VisualBasic.Interaction.InputBox(LanguageManager.GetString("EnterPathToDecrypt"), "Déchiffrement d'un chemin", "");
            }

            // Retrieve encryption key from settings
            string encryptionKey = jobManager.EncryptionSettings.EncryptionKey;

            if (!string.IsNullOrWhiteSpace(jobName))
            {
                // Search for the specified job
                Job? job = jobManager.SearchJob(jobName);
                if (job == null)
                {
                    AppendConsoleLog(string.Format(LanguageManager.GetString("JobNotFound"), jobName));
                    return;
                }

                // Decrypt files in the job's destination path
                CryptoService.DecryptFilesInDirectory(job.DestinationPath, encryptionKey);
                AppendConsoleLog(string.Format(LanguageManager.GetString("DecryptionCompletedJob"), jobName));
            }
            else if (!string.IsNullOrWhiteSpace(customPath))
            {
                // Decrypt files in the specified custom path
                CryptoService.DecryptFilesInDirectory(customPath, encryptionKey);
                AppendConsoleLog(string.Format(LanguageManager.GetString("DecryptionCompletedPath"), customPath));
            }
            else
            {
                // Log message if no action is taken
                AppendConsoleLog(LanguageManager.GetString("DecryptionNoAction"));
            }
        }
    }
}