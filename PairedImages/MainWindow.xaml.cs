using System;
using System.Windows;

namespace PairedImages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Height += 20;
            Width += 20;
        }
        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            GamePlay gameWindow = new GamePlay();
            gameWindow.Show();
            this.Close();
        }

        private void ExitGame_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}