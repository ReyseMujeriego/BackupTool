using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EasySave.Core.Services;
using EasySave.Core.Models;
using EasySave.Desktop.Utils;

namespace EasySave.Desktop.ViewModels
{
    public class AddJobViewModel : INotifyPropertyChanged
    {
        private readonly JobManager jobManager;

        private string _jobName = "";
        private string _sourcePath = "";
        private string _destinationPath = "";
        private string _selectedSaveType = "Complete"; //default value

        public string JobName
        {
            get => _jobName;
            set
            {
                _jobName = value;
                OnPropertyChanged();
            }
        }

        public string SourcePath
        {
            get => _sourcePath;
            set
            {
                _sourcePath = value;
                OnPropertyChanged();
            }
        }

        public string DestinationPath
        {
            get => _destinationPath;
            set
            {
                _destinationPath = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> SaveTypes { get; } =
        [
            "Complete",
            "Differential"
        ];

        public string SelectedSaveType
        {
            get => _selectedSaveType;
            set
            {
                _selectedSaveType = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddJobCommand { get; }

        /// <summary>
        /// ViewModel for adding a new job.
        /// Initializes commands and job manager instance.
        /// </summary>
        public AddJobViewModel()
        {
            jobManager = JobManager.Instance;
            AddJobCommand = new RelayCommand(_ => AddJob());
        }

        /// <summary>
        /// Adds a new job based on user input.
        /// Validates input fields and creates a job using the selected save type.
        /// Updates the job list and resets input fields after addition.
        /// </summary>
        private void AddJob()
        {
            // Ensure required fields are not empty
            if (string.IsNullOrWhiteSpace(JobName) || string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(DestinationPath))
            {
                return;
            }

            // Select the appropriate job factory based on the save type
            JobFactory factory = SelectedSaveType == "Complete"
                ? new CompleteJobFactory()
                : new DifferentialJobFactory();

            // Create and add the job
            Job job = factory.CreateJob(JobName, SourcePath, DestinationPath);
            jobManager.AddJob(job);

            // Reset input fields
            JobName = "";
            SourcePath = "";
            DestinationPath = "";
            OnPropertyChanged(nameof(JobName));
            OnPropertyChanged(nameof(SourcePath));
            OnPropertyChanged(nameof(DestinationPath));
        }

        /// <summary>
        /// Event triggered when a property value changes.
        /// Notifies UI elements to update accordingly.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for UI updates.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}