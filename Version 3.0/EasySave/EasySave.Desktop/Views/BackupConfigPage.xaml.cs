using System.Windows;
using EasySave.Desktop.Utils;
using EasySave.Desktop.ViewModels;
namespace EasySave.Desktop.Views
{
    
    public partial class BackupConfigPage : Window
    {
        private readonly UILanguageManager languageManager;
        private readonly BackupConfigViewModel viewModel;
        public BackupConfigPage()
        {
            InitializeComponent();
            viewModel = new BackupConfigViewModel(this);
            DataContext = viewModel;
            languageManager = (UILanguageManager)Resources["LangManager"];
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close(); 
        }
    }
}
