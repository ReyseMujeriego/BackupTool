using System;
using System.Windows;
using EasySave.RemoteConsole.ViewModels;
using EasySave.RemoteConsole.Utils;

namespace EasySave.RemoteConsole
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel viewModel;
        private readonly UILanguageManager languageManager;

        public MainWindow()
        {
            InitializeComponent();

            // Initialisation du ViewModel
            viewModel = new MainViewModel();
            DataContext = viewModel;

            // Chargement du gestionnaire de langue depuis les ressources
            languageManager = (UILanguageManager)Resources["LangManager"];
        }

        /// <summary>
        /// Change la langue de l'application en français.
        /// </summary>
        private void ChangeLanguageFR(object sender, RoutedEventArgs e)
        {
            languageManager.SetLanguage("fr");
        }

        /// <summary>
        /// Change la langue de l'application en anglais.
        /// </summary>
        private void ChangeLanguageEN(object sender, RoutedEventArgs e)
        {
            languageManager.SetLanguage("en");
        }
    }
}
