using System;
using System.Windows;
using EasySave.Desktop.ViewModels;
using EasySave.Desktop.Utils;
using System.Windows.Controls;
using EasySave.Core.Models;
using System.Collections.ObjectModel;

namespace EasySave.Desktop.Views
{
    public partial class HomePage : Window
    {
        private readonly HomePageViewModel viewModel;
        private readonly UILanguageManager languageManager;

        public HomePage()
        {
            InitializeComponent();
            viewModel = new HomePageViewModel();
            DataContext = viewModel;
            viewModel.RequestAddJobWindow += OnRequestAddJobWindow;
            viewModel.RequestEditJobWindow += OnRequestEditJobWindow;
            languageManager = (UILanguageManager)Resources["LangManager"];
        }

        /// <summary>
        /// Opens the Add Job window when requested.
        /// </summary>
        private void OnRequestAddJobWindow(object sender, EventArgs e)
        {
            // Create and display the Add Job window with its ViewModel
            AddJobPage addJobWindow = new()
            {
                DataContext = new AddJobViewModel()
            };
            addJobWindow.ShowDialog();
        }

        /// <summary>
        /// Opens the Edit Job window for the selected job.
        /// </summary>
        private void OnRequestEditJobWindow(object sender, EventArgs e)
        {
            if (viewModel.SelectedJob == null)
            {
                MessageBox.Show("Veuillez sélectionner un job à modifier.", "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
                // If no job is selected, show a warning message and exit the function
                return;
            }

            // Create and display the Modify Job window with the selected job data
            ModifyPage modifyPage = new(viewModel.SelectedJob);
            modifyPage.ShowDialog();
        }

        /// <summary>
        /// Closes the application when the Quit button is clicked.
        /// </summary>
        private void Quitter_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Changes the application's language to French.
        /// </summary>
        private void ChangeLanguageFR(object sender, RoutedEventArgs e)
        {
            languageManager.SetLanguage("fr");
        }

        /// <summary>
        /// Changes the application's language to English.
        /// </summary>
        private void ChangeLanguageEN(object sender, RoutedEventArgs e)
        {
            languageManager.SetLanguage("en");
        }

        /// <summary>
        /// Updates the list of selected jobs when the selection in the job list view changes.
        /// </summary>
        private void JobListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is HomePageViewModel viewModel)
            {
                // Update the selected jobs collection with the newly selected items
                viewModel.SelectedJobs = new ObservableCollection<Job>(JobListView.SelectedItems.Cast<Job>());
            }
        }

    }
}
