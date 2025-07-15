using System.Windows;
using System.Windows.Controls;
using EasySave.Desktop.Utils;
using EasySave.Desktop.ViewModels;

namespace EasySave.Desktop.Views
{
    /// <summary>
    /// Logique d'interaction pour AddJobPage.xaml
    /// </summary>
    public partial class AddJobPage : Window
    {
        private readonly UILanguageManager languageManager;

        public AddJobPage()
        {
            InitializeComponent();
            DataContext = new AddJobViewModel();
            languageManager = (UILanguageManager)Resources["LangManager"];
        }

        /// <summary>
        /// Function to close the window after adding
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}