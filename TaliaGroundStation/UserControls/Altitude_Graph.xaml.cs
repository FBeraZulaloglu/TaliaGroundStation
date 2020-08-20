using System.Windows.Controls;
using LiveCharts;
using System.Windows.Media;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Timers;

namespace TaliaGroundStation.UserControls
{
    /// <summary>
    /// Interaction logic for Pitch_Roll_Yaw_Graph.xaml
    /// </summary>
    public partial class Altitude_Graph : UserControl
    {
        public double graph_altitude;
        public Altitude_Graph()
        {
            InitializeComponent();


            Timer myTimer = new System.Timers.Timer();
            myTimer.Elapsed += new ElapsedEventHandler(DisplayEvent);
            myTimer.Interval = 1000; // 1000 ms is one half second
            myTimer.Start();


            altitude_values = new ChartValues<ObservableValue>
            {
                //first there is nothing
            };

            var altitude = new LineSeries
            {
                Values = altitude_values,
                StrokeThickness = 4,
                Fill = Brushes.Transparent,
                PointGeometrySize = 0,
                DataLabels = false
            };

            SeriesCollection = new SeriesCollection { altitude };
            altitude.
            DataContext = this;
        }

        private void DisplayEvent(object sender, ElapsedEventArgs e)
        {
            
            altitude_values.Add(new ObservableValue(graph_altitude));
        }

        public ChartValues<ObservableValue> altitude_values { get; set; }
        public SeriesCollection SeriesCollection { get; set; }
        
    }
 }

