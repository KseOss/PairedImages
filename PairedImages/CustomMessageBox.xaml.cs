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

namespace PairedImages
{
    /// <summary>
    /// Логика взаимодействия для CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Stats { get; set; }

        public CustomMessageBox(string title, string stats, string button1Text, string button2Text, string button3Text)
        {
            Title = title;
            Stats = stats;
            Message = "Что вы хотите сделать дальше?";
            InitializeComponent(); DataContext = this;
        }

        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = null;
            this.Close();
        }
    }
}