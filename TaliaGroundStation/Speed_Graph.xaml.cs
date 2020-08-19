using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Windows.Controls;
using System.Windows.Media;

namespace TaliaGroundStation
{
    /// <summary>
    /// Interaction logic for Speed_Graph.xaml
    /// </summary>
    public partial class Speed_Graph : UserControl
    {
        public Speed_Graph()
        {
            InitializeComponent();

            speed_values = new ChartValues<ObservableValue>
            {
                //first there is nothing
            };


            var speed = new LineSeries
            {
                Values = speed_values,
                StrokeThickness = 4,
                Fill = Brushes.Transparent,
                PointGeometrySize = 0,
                DataLabels = false
            };


            SeriesCollection = new SeriesCollection { speed };

            for (int i = 0; i < 10; i++)
            {
                speed_values.Add(new ObservableValue(i * i + 1));
            }

            DataContext = this;
        }

        public ChartValues<ObservableValue> speed_values { get; set; }
        public SeriesCollection SeriesCollection { get; set; }

    }
}
    

