using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;

namespace TaliaGroundStation
{
    /// <summary>
    /// Interaction logic for TransmitVideo.xaml
    /// </summary>
    public partial class TransmitVideo : Window
    {
        public TransmitVideo()
        {
            InitializeComponent();
            videoElement.LoadedBehavior = MediaState.Manual;
            videoElement.UnloadedBehavior = MediaState.Manual;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {


            if (!Directory.Exists(@"sendVideoToSat.mp4"))
            {
                //File.Move(@"aktar1.mp4", @"aktar_yedek.mp4");
                File.Create(@"sendVideoToSat.mp4");
            }
            else
            {
                File.Move(@"" + sourceFile.Text, @"sendVideoToSat.mp4");
                Console.WriteLine("File taşındı");
                Console.WriteLine(sourceFile.Text);
            }

        }

        private void fileGöster(object sender, RoutedEventArgs e)
        {
            try
            {

                videoElement.Source = new Uri(sourceFile.Text);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show("Böyle bir file yok");
            }
        }

        private void dropTheFile(object sender, DragEventArgs e)
        {
            Console.WriteLine("Drop file a girildi");
            string file = (string)((DataObject)e.Data).GetFileDropList()[0];

            try
            {
                videoElement.Source = new Uri(file);

                Console.WriteLine(file);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }
            
            videoElement.Play();
            sourceFile.Text = file;
        }


        private void solTiklaBırak(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            Console.WriteLine("Sol tıkla bırakıldı");
        }


        private void StopVideo(object sender, RoutedEventArgs e)
        {
            
            if (videoElement.Source.ToString() != "")
            {
                videoElement.Stop();
            }
            else
            {
                MessageBox.Show("Herhangi bir video bulunmamaktadır.");
            }

        }

        private void StartVideo(object sender, RoutedEventArgs e)
        {
            if (videoElement.Source.ToString() != "")
            {
                videoElement.Play();
            }
            else
            {
                MessageBox.Show("Herhangi bir video bulunmamaktadır.");
            }
        }
    }
}
