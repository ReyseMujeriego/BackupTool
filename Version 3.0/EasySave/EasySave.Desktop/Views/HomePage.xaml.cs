using System.Windows;
using EasySave.Desktop.ViewModels;
using EasySave.Desktop.Utils;


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
            viewModel.RequestBackupConfigWindow += OnRequestBackupConfigWindow;
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

        private void OnRequestBackupConfigWindow(object sender, EventArgs e)
        {
            BackupConfigPage configWindow = new BackupConfigPage();
            configWindow.ShowDialog();
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


    }
}
