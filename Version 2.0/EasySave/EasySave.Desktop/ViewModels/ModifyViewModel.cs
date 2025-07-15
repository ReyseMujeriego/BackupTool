using EasySave.Core.Models;
using EasySave.Core.Services;
using EasySave.Desktop.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EasySave.Desktop.ViewModels
{
    /// <summary>
    /// Interaction logic for ModifyPage.xaml
    /// </summary>
    public class ModifyViewModel : INotifyPropertyChanged
    {
        private string _jobName = string.Empty;
        private string _sourcePath = string.Empty;
        private string _destinationPath = string.Empty;
        private readonly Job _originalJob;

        public Action? CloseAction { get; set; } // Initalize the action

        public event PropertyChangedEventHandler? PropertyChanged;

        public string JobName
        {
            get => _jobName;
            set { _jobName = value; OnPropertyChanged(); } // Define and notify of change
        }

        public string SourcePath
        {
            get => _sourcePath;
            set { _sourcePath = value; OnPropertyChanged(); } // Define and notify of change
        }

        public string DestinationPath
        {
            get => _destinationPath;
            set { _destinationPath = value; OnPropertyChanged(); } // Define and notify of change
        }

        public ICommand SaveCommand { get; }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// ViewModel for modifying an existing job.
        /// Initializes properties based on the provided job instance.
        /// </summary>
        /// <param name="job">The job instance to be modified.</param>
        public ModifyViewModel(Job job)
        {
            _originalJob = job;
            JobName = job.Name;
            SourcePath = job.SourcePath;
            DestinationPath = job.DestinationPath;

            // Initialize the Save command with the SaveChanges method
            SaveCommand = new RelayCommand(_ => SaveChanges());
        }

        /// <summary>
        /// Saves changes made to the job after user confirmation.
        /// Updates the job properties and refreshes the UI accordingly.
        /// </summary>
        public void SaveChanges()
        {
            // Confirmation before saving
            MessageBoxResult result = MessageBox.Show(
                LanguageManager.GetString("ConfirmSaveJobChanges"),
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Call JobManager to update the existing job
                JobManager.Instance.UpdateJob(_originalJob.Name, JobName, SourcePath, DestinationPath);

                // Update original job properties to reflect changes in UI
                _originalJob.Name = JobName;
                _originalJob.SourcePath = SourcePath;
                _originalJob.DestinationPath = DestinationPath;

                // Trigger property change notifications to refresh UI
                OnPropertyChanged(nameof(JobName));
                OnPropertyChanged(nameof(SourcePath));
                OnPropertyChanged(nameof(DestinationPath));

                // Close the modification window after saving
                CloseAction?.Invoke();
            }
        }
    }
}