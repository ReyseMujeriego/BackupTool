using EasySave.Core.Models;
using EasySave.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasySave.Desktop.Views
{
    public partial class ModifyPage : Window
    {
        /// <summary>
        /// Interaction logic for ModifyPage.xaml
        /// </summary>
        /// <param name="selectedJob">the job selected in the UI</param>
        public ModifyPage(Job selectedJob)
        {
            InitializeComponent();
            var viewModel = new ModifyViewModel(selectedJob);
            DataContext = viewModel;
            viewModel.CloseAction = () => this.Close(); // to close the window when the buttom is clicked
        }
    }
}