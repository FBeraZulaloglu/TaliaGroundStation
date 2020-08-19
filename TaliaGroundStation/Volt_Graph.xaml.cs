using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Windows.Controls;
using System.Windows.Media;


namespace TaliaGroundStation
{
    /// <summary>
    /// Interaction logic for Volt_Graph.xaml
    /// </summary>
    public partial class Volt_Graph : UserControl
    {
        public Volt_Graph()
        {
            InitializeComponent();

            volt_values = new ChartValues<ObservableValue>
            {
                //first there is nothing
            };

           
            var volt = new LineSeries
            {
                Values = volt_values,
                StrokeThickness = 4,
                Fill = Brushes.Transparent,
                PointGeometrySize = 0,
                DataLabels = false
            };


            SeriesCollection = new SeriesCollection { volt };

            for (int i = 0; i < 10; i++)
            {
                volt_values.Add(new ObservableValue(i * i + 1));
            }

            DataContext = this;
        }

        public ChartValues<ObservableValue> volt_values { get; set; }
        public SeriesCollection SeriesCollection { get; set; }

        }
    }

