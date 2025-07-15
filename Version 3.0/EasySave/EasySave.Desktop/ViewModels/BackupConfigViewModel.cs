using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EasySave.Core.Services;
using EasySave.Desktop.Utils;
using System.Windows;

namespace EasySave.Desktop.ViewModels
{
    public class BackupConfigViewModel : INotifyPropertyChanged
    {
        private readonly Window _window;
        private readonly SaveManager _saveManager;
        private readonly JobManager _jobManager;

        public string PriorityExtensions { get; set; }
        public string LargeFileSizeLimit { get; set; }
        public string BusinessSoftware { get; set; }

        public ICommand SaveCommand { get; }

        public BackupConfigViewModel(Window window)
        {
            _window = window;
            _saveManager = HomePageViewModel.SharedSaveManager;
            _jobManager = JobManager.Instance;

            PriorityExtensions = _saveManager.priorityExtensions.Any() ? string.Join(",", _saveManager.priorityExtensions) : "";
            LargeFileSizeLimit = _saveManager.largeFileSizeLimit > 0 ? _saveManager.largeFileSizeLimit.ToString() : "";
            BusinessSoftware = _jobManager.businessProcesses.Any() ? string.Join(",", _jobManager.businessProcesses) : "";
            SaveCommand = new RelayCommand(_ => SaveConfig());
        }

        private void SaveConfig()
        {
            try
            {
                if (BusinessSoftware != null && LargeFileSizeLimit != null && PriorityExtensions != null)
                {
                    _saveManager.priorityExtensions = (PriorityExtensions ?? "").Split(',', StringSplitOptions.TrimEntries).ToList();
                    _saveManager.largeFileSizeLimit = int.Parse(LargeFileSizeLimit);
                    _jobManager.businessProcesses = BusinessSoftware.Split(',', StringSplitOptions.TrimEntries);
                    MessageBox.Show(
                       LanguageManager.GetString("ConfigSavedSuccess"),
                       LanguageManager.GetString("SuccessTitle"),
                       MessageBoxButton.OK,
                       MessageBoxImage.Information
                   );
                    _window.Close();
                }
                else
                {
                    MessageBox.Show(LanguageManager.GetString("NotEmpty"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("ConfigSaveError"), ex.Message),
                    LanguageManager.GetString("ErrorTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
    