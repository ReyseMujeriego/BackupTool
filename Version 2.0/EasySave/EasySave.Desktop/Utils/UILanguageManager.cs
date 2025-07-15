using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using EasySave.Core.Services;
using EasySave.Desktop.Properties;

namespace EasySave.Desktop.Utils
{
    public class UILanguageManager : INotifyPropertyChanged
    {
        public UILanguageManager()
        {
            LanguageManager.LanguageChanged += OnLanguageChanged;
        }

        public string AddJobLabel => Resource.AddJob;
        public string EditJobLabel => Resource.EditJob;
        public string DeleteJobLabel => Resource.DeleteJob;
        public string ExecuteJobLabel => Resource.ExecuteJob;
        public string JobsListTitle => Resource.JobsListTitle;
        public string ConsoleLogTitle => Resource.ConsoleLogTitle;


        public string AddJobWindowTitle => Resource.AddJobWindowTitle;
        public string AddJobName => Resource.AddJobName;
        public string AddJobSource => Resource.AddJobSource;
        public string AddJobDestination => Resource.AddJobDestination;
        public string AddJobType => Resource.AddJobType;
        public string AddJobButton => Resource.AddJobButton;
        public string JobName => Resource.JobName;
        public string JobSource => Resource.JobSource;
        public string JobDestination => Resource.JobDestination;
        public string JobType => Resource.JobType;
        public string ExecuteSelectedJobLabel => Resource.ExecuteSelectedJobLabel;
        public string EncryptionConfigurationLabel => Resource.EncryptionConfigurationLabel;
        public string DecryptLabel => Resource.DecryptLabel;


        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets the application's language and updates the current culture settings.
        /// </summary>
        /// <param name="cultureCode">The culture code (e.g., "en", "fr") to set as the current language.</param>
        public void SetLanguage(string cultureCode)
        {
            // Create a new CultureInfo object based on the given culture code.
            CultureInfo newCulture = new CultureInfo(cultureCode);

            // Set the application's current culture and UI culture.
            CultureInfo.CurrentCulture = newCulture;
            CultureInfo.CurrentUICulture = newCulture;

            // Update resource culture if applicable.
            Resource.Culture = newCulture;

            // Notify that the language has changed.
            OnPropertyChanged(string.Empty);
        }

        /// <summary>
        /// Handles actions when the language is changed.
        /// </summary>
        private void OnLanguageChanged()
        {
            OnPropertyChanged(string.Empty);
        }

        /// <summary>
        /// Notifies the UI or bound properties that a property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
