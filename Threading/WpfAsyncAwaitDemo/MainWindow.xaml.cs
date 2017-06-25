using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfAsyncAwaitDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer _timer;
        private int _step;

        public MainWindow()
        {
            InitializeComponent();
            StartTimer();
        }

        private void StartTimer()
        {
            _timer = new Timer(
                _ => Dispatcher.Invoke(() => lblIteration.Content = _step++),
                null,
                1500,
                330);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            lblOperation.Content = "Syncronous call...";
            Thread.Sleep(3000);
            lblOperation.Content = "Call ended";
        }

        private async void btnAsync_Click(object sender, RoutedEventArgs e)
        {
            lblOperation.Content = "Asyncronous call...";
            await AsyncDelay();
            lblOperation.Content = "Call ended";
        }

        private Task AsyncDelay()
        {
            return Task.Delay(3000);
        }
    }
}
