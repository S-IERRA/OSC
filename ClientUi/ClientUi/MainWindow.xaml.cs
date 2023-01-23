using System;
using System.Windows;

namespace ClientUi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Dragging(object sender, System.Windows.Input.MouseButtonEventArgs e) 
            => DragMove();

        private void CloseUiButton_Click(object sender, RoutedEventArgs e)
        {
            //Cleanup
            Environment.Exit(0);
        }

        private void MinimiseUiButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
