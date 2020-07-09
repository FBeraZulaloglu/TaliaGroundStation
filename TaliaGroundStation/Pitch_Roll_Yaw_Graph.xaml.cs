using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Configurations;
using System.Windows.Media;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.IO.Ports;

namespace TaliaGroundStation.UserControls
{
    /// <summary>
    /// Interaction logic for Pitch_Roll_Yaw_Graph.xaml
    /// </summary>
    public partial class Pitch_Roll_Yaw_Graph : UserControl
    {
        public Pitch_Roll_Yaw_Graph()
        {
            InitializeComponent();


            pitch_values = new ChartValues<ObservableValue>
            {
                //first there is nothing
            };

            roll_values = new ChartValues<ObservableValue>
            {
                //first there is nothing
            };

            yaw_values = new ChartValues<ObservableValue>
            {
                //first there is nothing
            };

            var pitch = new LineSeries
            {
                Values = pitch_values,
                StrokeThickness = 4,
                Fill = Brushes.Transparent,
                PointGeometrySize = 0,
                DataLabels = false
            };

            var roll = new LineSeries
            {
                Values = roll_values,
                StrokeThickness = 4,
                Fill = Brushes.Transparent,
                PointGeometrySize = 0,
                DataLabels = false
            };

            var yaw = new LineSeries
            {
                Values = yaw_values,
                StrokeThickness = 4,
                Fill = Brushes.Transparent,
                PointGeometrySize = 0,
                DataLabels = false
            };


            SeriesCollection = new SeriesCollection { pitch, roll,yaw };
            
            for (int i = 0; i < 10; i++)
            {
                pitch_values.Add(new ObservableValue(i*i+1));
                roll_values.Add(new ObservableValue(i * i - 10));
                yaw_values.Add(new ObservableValue(i * 2));
            }

            DataContext = this;
        }

        public ChartValues<ObservableValue> pitch_values { get; set; }
        public ChartValues<ObservableValue> roll_values { get; set; }
        public ChartValues<ObservableValue> yaw_values { get; set; }
        public SeriesCollection SeriesCollection { get; set; }
        
    }
 }

