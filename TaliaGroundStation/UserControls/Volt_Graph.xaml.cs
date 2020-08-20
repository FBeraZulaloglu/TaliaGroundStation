using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;


namespace TaliaGroundStation
{
    /// <summary>
    /// Interaction logic for Volt_Graph.xaml
    /// </summary>
    public partial class Volt_Graph : System.Windows.Controls.UserControl
    {
        
        public List<Double> volt_values;
        System.Timers.Timer myTimer;
        public Volt_Graph()
        {
            InitializeComponent();

            volt_values = new List<double>();
            myTimer = new System.Timers.Timer();
            myTimer.Elapsed += new ElapsedEventHandler(DisplayEvent);
            myTimer.Interval = 1000; // 1000 ms is one half second
            myTimer.Start();

        }

        private void DisplayEvent(object sender, ElapsedEventArgs e)
        {

            Dispatcher.BeginInvoke((Action)(() => {
                series.Points.Clear();
                //yeni verileri çizdirmeden önce temizle
                foreach (var volt_value in volt_values)
                {
                    series.Points.AddXY(DateTime.Now.ToString("hh:mm:ss"), volt_value);
                }
                
            }));
            

        }
    }
}

