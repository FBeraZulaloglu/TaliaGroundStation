using System;
using System.Windows;
using Microsoft.Maps.MapControl.WPF;

namespace TaliaGroundStation
{
    /// <summary>
    /// Interaction logic for FullSizeMap.xaml
    /// </summary>
    public partial class FullSizeMap : Window
    {
        MapPolyline route;
        Pushpin marker, marker1;
        public FullSizeMap()
        {
            InitializeComponent();
            myMap.Focus();// + ile yakınlaşı r- ile ise uzaklasır

            //aerial mode u true yapmak AerıalWithLabelı etkili hale getirir
            //myMap.Mode = new AerialMode(true);
            route = new MapPolyline();
            route.StrokeThickness = 6;
            route.Opacity = 0.7;
            route.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            marker = new Pushpin();
            marker1 = new Pushpin();
        }

        private void LoadToMap(object sender, RoutedEventArgs e)
        {
            var sateliteLocation = new Location(Convert.ToDouble(Satelite_Lat.Text), Convert.ToDouble(Satelite_Lng.Text));
            var myLocation = new Location(Convert.ToDouble(My_Lat.Text), Convert.ToDouble(My_Lng.Text));
            myMap.Center = sateliteLocation;
            route.Locations = new LocationCollection()
            {
                new Location(sateliteLocation),
                new Location(myLocation)
            };
            myMap.Children.Add(route);
            marker.Location = sateliteLocation;
            marker1.Location = myLocation;

            MapLayer.SetPosition(myMarker, sateliteLocation);
            //myMap.Children.Add(myMarker);

        }
    }
}
