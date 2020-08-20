using System.Windows.Controls;
using LiveCharts;
using System.Windows.Media;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Timers;
using System;

namespace TaliaGroundStation.UserControls
{
    /// <summary>
    /// Interaction logic for Pressure_Temperature_Graph.xaml
    /// </summary>
    public partial class Pressure_Temperature_Graph : UserControl
    {
        double pressure;
        double temperature;
        public Pressure_Temperature_Graph()
        {
            InitializeComponent();

            Timer myTimer = new System.Timers.Timer();
            myTimer.Elapsed += new ElapsedEventHandler(DisplayEvent);
            myTimer.Interval = 1000; // 1000 ms is one half second
            myTimer.Start();

            pressure_values = new ChartValues<ObservableValue>
            {
                //first there is nothing
            };

            temperature_values = new ChartValues<ObservableValue>
            {
                //first there is nothing
            };
            var pressure = new LineSeries
            {
                Values = pressure_values,
                StrokeThickness = 4,
                Fill = Brushes.Transparent,
                PointGeometrySize = 0,
                DataLabels = false
            };

            var temperature = new LineSeries
            {
                Values = temperature_values,
                StrokeThickness = 4,
                Fill = Brushes.Transparent,
                PointGeometrySize = 0,
                DataLabels = false
            };


            SeriesCollection = new SeriesCollection { pressure, temperature };

            DataContext = this;
        }

        private void DisplayEvent(object sender, ElapsedEventArgs e)
        {
            pressure_values.Add(new ObservableValue(pressure));
            temperature_values.Add(new ObservableValue(temperature));
        }

        public ChartValues<ObservableValue> pressure_values { get; set; }
        public ChartValues<ObservableValue> temperature_values { get; set; }
        public SeriesCollection SeriesCollection { get; set; }

    }
}
