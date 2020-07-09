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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaliaGroundStation
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenLock(object sender, RoutedEventArgs e)
        {
            //servonun çalışması için komut gönder
            //send command to start the servo motor
            MessageBox.Show("Ayrılma Mekanizması Açıldı!!");
        }

        private void OpenBuzzer(object sender, RoutedEventArgs e)
        {
            //buzzerın çalışmkası için komut gönder
            //send command to start buzzer
            MessageBox.Show("Buzzer Çalıştırıldı");
        }

        private void TelemetryTable_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void videoSending(object sender, RoutedEventArgs e)
        {
            TransmitVideo tVideo = new TransmitVideo();
            tVideo.Show();
        }
    }
}
