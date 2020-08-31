using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaliaGroundStation
{
    /// <summary>
    /// Interaction logic for Simulation.xaml
    /// </summary>
    /// 
    public partial class Simulation : UserControl
    {
        public double roll { get; set; }
        public double pitch { get; set; }
        public double yaw { get; set; }
        public int rollCount { get; set; }
        public int satelite_height { get; set; }
        
        Timer myTimer;
        public Simulation()
        {
            InitializeComponent();
            
            myTimer = new System.Timers.Timer();
            myTimer.Elapsed += new ElapsedEventHandler(DisplayEvent);
            myTimer.Interval = 1000; // 1000 ms is one half second
            myTimer.Start();
            //change();

        }

        public void DisplayEvent(object source, ElapsedEventArgs e)
        {
            // code here will run every half second
            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    yaw_angle.Angle = roll;
                    roll_angle.Angle = pitch;
                    pitch_angle.Angle = yaw;
                }
                ));
            }
            catch(Exception ex)
            {
                Console.WriteLine("Simulation Display event Error:  " + ex.Message);
                //Console.Error();
            }
            

        }

        private void closing(object sender, ContextMenuEventArgs e)
        {
            myTimer.Stop();
        }

        private void change()
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 0;
            animation.To = 200;
            animation.Duration = TimeSpan.FromSeconds(2);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("HELLO");
        }
    }
}
