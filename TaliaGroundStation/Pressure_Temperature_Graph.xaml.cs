using System.Windows.Controls;
using LiveCharts;
using System.Windows.Media;
using LiveCharts.Wpf;
using LiveCharts.Defaults;

namespace TaliaGroundStation.UserControls
{
    /// <summary>
    /// Interaction logic for Pressure_Temperature_Graph.xaml
    /// </summary>
    public partial class Pressure_Temperature_Graph : UserControl
    {
        public Pressure_Temperature_Graph()
        {
            InitializeComponent();

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

            for (int i = 0; i < 10; i++)
            {
                pressure_values.Add(new ObservableValue(i * i + 1));
                temperature_values.Add(new ObservableValue(i * i - 1));
            }

            DataContext = this;
        }

        public ChartValues<ObservableValue> pressure_values { get; set; }
        public ChartValues<ObservableValue> temperature_values { get; set; }
        public SeriesCollection SeriesCollection { get; set; }

    }
}
