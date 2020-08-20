using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using AForge.Video;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.IO;
using System.Collections.Generic;
using System.Windows.Threading;
using Accord.Video.VFW;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using GMap.NET.MapProviders;
using System.ComponentModel;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Device.Location;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Forms.DataVisualization.Charting;

namespace TaliaGroundStation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region variables
        AVIWriter writer;
        string video_ip;
        string telemetry_ip;
        string file_ip;
        StringBuilder talia_csv;
        List<Telemetry> telemetry_list;
        Telemetry current_telemetry;
        int missing_data_counter = 0;
        private TcpClient client;
        public StreamReader STR;
        public StreamWriter STW;
        BackgroundWorker backgroundWorker1;
        BackgroundWorker backgroundWorker2;
        GMapMarker talia_marker;
        string[] data_array;
        #endregion

        int height_delete = 100;
        List<Double> volt_list;
        double _volt = 2.1;
        int paket = 0;
        Series lineSeries;

        public MainWindow()
        {
            InitializeComponent();
            data_array = new string[16];
            video_ip = "http://192.168.4.1/picture";
            telemetry_ip = "http://192.168.4.1/data";
            file_ip = "http://192.168.4.1/file";
            writer = new AVIWriter();
            writer.Open("TMUY2020_53417_VIDEO.avi", 800, 600);
            talia_csv = new StringBuilder();
            talia_marker = new GMapMarker(new PointLatLng(42,21));
            telemetry_list = new List<Telemetry>();
            volt_list = new List<Double>();
            videoPlayer.NewFrame += VideoPlayer_NewFrame;// works when new frame came
            lineSeries = new Series("Volt");
            lineSeries.ChartType = SeriesChartType.Spline;

            for (int i = 0; i < 20; i++)
            {
                volt_series1.Points.Add(i,Math.Pow(i,6));
                volt_series2.Points.Add(i,Math.Log(i*10));
                volt_series3.Points.Add(i,i*i);
                volt_series4.Points.Add(i,i-(Math.Log(i)));
            }
            
        }

        

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            startVideo();
            telemetry_timer_start(1000);

            //uncomment if you use tcp server 
            //IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName());

            //foreach (IPAddress address in localIP)
            //{
            //    if (address.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        ipAdress.Text = address.ToString();
            //    }
            //}

            //port.Text = "12";

            //backgroundWorker1 = new BackgroundWorker();
            //backgroundWorker2 = new BackgroundWorker();
            //backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            //backgroundWorker2.DoWork += backgroundWorker2_DoWork;
            //client = new TcpClient();
        }

        #region Commands
        private void OpenLock(object sender, RoutedEventArgs e)
        {
            //servonun çalışması için komut gönder
            //send command to start the servo motor
            MessageBox.Show("Ayrılma Mekanizması Açıldı!!");
        }

        private void OpenBuzzer(object sender, RoutedEventArgs e)
        {
            //buzzerın çalışmkası için komut gönder
            //send command to start buzzer açmak için 2 kapatmak için 3
            if (openBuzzer.Content.Equals("Buzzer Aç"))
            {
                using (WebClient wb = new WebClient())
                {
                    byte[] open = { 2 };
                    try
                    {
                        wb.UploadData("http://192.168.4.1/command", open);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    
                }
                openBuzzer.Content = "Buzzer Kapat";
            }
            else
            {
                using (WebClient wb = new WebClient())
                {
                    byte[] close = { 3 };
                    wb.UploadData("http://192.168.4.1/command", close);
                }
            }
        }
        #endregion

        #region Extra Windows
        private void videoSending(object sender, RoutedEventArgs e)
        {
            /*TransmitVideo tVideo = new TransmitVideo();
            tVideo.Show();*/
            //Show File Dialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Video Files";
            string fileName = null;
            if (openFileDialog.ShowDialog() == true)
            {
                fileName = openFileDialog.FileName;
            }

            // Send file fileName to remote device
            Console.WriteLine(fileName);
            if (fileName != null)
            {
                using (WebClient client = new WebClient())
                {
                    client.UploadFile(file_ip, fileName);
                }
            }
            else
            {
                MessageBox.Show("You Haven't Chosen Any File");
            }
        }

        private void OpenFullMap(object sender, MouseButtonEventArgs e)
        {
            FullSizeMap fsm = new FullSizeMap();
            fsm.Show();
        }
        #endregion

        #region Server Part with another project
        private void Start_Server(object sender, RoutedEventArgs e)
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, Convert.ToInt32(port.Text));
                listener.Start();
                client = listener.AcceptTcpClient();
                STR = new StreamReader(client.GetStream());
                STW = new StreamWriter(client.GetStream());
                STW.AutoFlush = true;
                connection.Text = "Server başladı";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            backgroundWorker1.RunWorkerAsync();
            backgroundWorker2.WorkerSupportsCancellation = true;
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            if (client.Connected)
            {
                connection.Text = "Servera bağlandı.";
                //STW.WriteLine("");
            }
            else
            {
                connection.Text = "Servera bağlanamadı";
                MessageBox.Show("Sending failed");
            }
            backgroundWorker2.CancelAsync();
        }
        // veri geldiğinde değerler okunur
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (client.Connected)
            {
                try
                {
                    var recieve = STR.ReadLine();
                    Console.WriteLine("Recieved == " + recieve);
                    // current telemetryi değiştir.
                    //telemetri listesine ekleme yap
                    
                    //Console.WriteLine("TAKIM NO: "+SetCurrentTelemetry(recieve)._takımNo);
                    telemetry_list.Add(SetCurrentTelemetry(recieve)); //returns a telemetry object
                               
                    //telemetri verilerini kaydet
                    talia_csv.Append(recieve+"\n");
                    File.WriteAllText("TMUY2020_53417_TLM.csv", talia_csv.ToString());
                    
                    Dispatcher.BeginInvoke((Action)(() => {
                        // görünütdeki değişiklikleri gelen telemetriye göre ayara
                        telemetry_table.ItemsSource = null;
                        telemetry_table.ItemsSource = telemetry_list;
                    }));

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private void Refresh_Server(object sender, RoutedEventArgs e)
        {
            Stop_Server(sender,e);
            Start_Server(sender,e);
        }

        private void Stop_Server(object sender, RoutedEventArgs e)
        {
            if (client.Connected)
            {
                client.Close();
            }

        }
        #endregion

        #region  Telemetry

        // timer to send request
        private void telemetry_timer_start(int timeInterval)
        {
            Timer myTimer = new Timer();
            myTimer.Elapsed += new ElapsedEventHandler(getTelemetryData);
            myTimer.Interval = timeInterval; // 1000 ms is one  second
            myTimer.Start();
        }
        // getting value
        private void getTelemetryData(object sender, ElapsedEventArgs e)
        {
            simulation.satelite_height = height_delete;
            
            height_delete += 10;
            
            if(height_delete > 700)
            {
                height_delete = 0;
            }
            Console.WriteLine("Telemtry Data İs Coming ...");
            
            var data = getTelemetry(telemetry_ip);
            ////////////////////////////////////////////NOT GRAPHLA ALAKALI
            //volt_list.Add(_volt);
            //_volt += 0.1;
            //if(_volt > 12)
            //{
            //    _volt = 0;
            //}
            //Dispatcher.BeginInvoke((Action)(() => {
            //    volt_series.Points.Clear();
                
            //    foreach (var item in volt_list)
            //    {
            //        volt_series.Points.AddXY(paket, item);
            //    }
            //    paket++;

            //    if (volt_list.Count > 10)
            //    {
            //        volt_list.RemoveAt(0);
            //    }
            //}));
           

            if (data != null)
            {
                //split data and show on table
                telemetry_list.Add(SetCurrentTelemetry(data));
                talia_csv.Append(data);
                File.WriteAllText("TMUY2020_53417_TLM.csv", talia_csv.ToString());
                

                Dispatcher.BeginInvoke((Action)(() => {
                    //seriyi temizle
                    telemetry_table.ItemsSource = null;
                    telemetry_table.ItemsSource = telemetry_list;
                    mapView.Position = new PointLatLng(current_telemetry._gps_lat, current_telemetry._gps_long);
                    addMarker();
                }));
            }
            else
            {
                missing_data_counter++;
                Console.WriteLine("data is missing ..."+missing_data_counter+". data");
            }
        }
        // getting request
        private string getTelemetry(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    //Console.WriteLine("READER : " + reader.ReadToEnd());
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldnt get the telemtry data" + '\n' + ex.Message);
                return null;
            }
        }
        //setting to an object to all values
        private Telemetry  SetCurrentTelemetry(String data)
        {
            Console.WriteLine("Data :::: "+data);
            

            data_array = data.Split(',');
            try
            {
                current_telemetry = new Telemetry();
                current_telemetry._takımNo = data_array[0];
                current_telemetry._paketNo = int.Parse(data_array[1]);
                current_telemetry._time = data_array[2];
                current_telemetry._pressure = double.Parse(data_array[3]);
                current_telemetry._height = double.Parse(data_array[4]);
                current_telemetry._velocity = double.Parse(data_array[5]);
                current_telemetry._volt = double.Parse(data_array[6]);
                current_telemetry._gps_lat = double.Parse(data_array[8]);
                current_telemetry._gps_long = double.Parse(data_array[9]);
                current_telemetry._altitude = double.Parse(data_array[10]);
                decideStatus(data_array[11],current_telemetry);
                current_telemetry._pitch = double.Parse(data_array[12]);
                current_telemetry._roll = double.Parse(data_array[13]);
                current_telemetry._yaw = double.Parse(data_array[14]);
                current_telemetry._rollCount = int.Parse(data_array[15]);
                current_telemetry._isVideoSent = data_array[16];
                return current_telemetry;


            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            
        }
        //getting status as an enum
        private void decideStatus(String status,Telemetry current_telemetry)
        {
            switch (int.Parse(status))
            {
                case 0:
                    current_telemetry._status = Status.ground;
                    break;
                case 1:
                    current_telemetry._status = Status.climbing;
                    break;
                case 2:
                    current_telemetry._status = Status.falling;
                    break;
                case 3:
                    current_telemetry._status = Status.leaving;
                    break;
                case 4:
                    current_telemetry._status = Status.waitingToRescue;
                    break;
            }
        }


        #endregion

        #region Video

        private void startVideo()
        {

            try
            {
                if (video_ip != null)
                {
                    JPEGStream mjpegSource = new JPEGStream(video_ip);
                    OpenVideoSource(mjpegSource);
                }
                else
                {
                    MessageBox.Show("Video başlatılamaz. IP Adresi geçerli değil.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldnt start the video source  " + ex.Message);
            }
        }

        private void OpenVideoSource(IVideoSource source)
        {
            videoPlayer.SignalToStop();
            videoPlayer.WaitForStop();
            videoPlayer.VideoSource = source;
            videoPlayer.Start();
            MessageBox.Show("Video Source Has Started");
        }

        private void VideoPlayer_NewFrame(object sender, ref Bitmap image)
        {
            writer.AddFrame(image);// to save the video
        }

        #endregion

        #region GMap
        private void mapLoaded(object sender, RoutedEventArgs e)
        {
            setUpMap();
            getCurrentLocation();
        }

        private void getCurrentLocation()
        {
            mapView.Position = new PointLatLng(41.139809, 29.029566);
            GMapMarker marker = new GMapMarker(new PointLatLng(41.139809, 29.029566));
            mapView.Markers.Add(marker);
            marker.Shape = new System.Windows.Controls.Image
            {
                Width = 50,
                Height = 50,
                Source = new BitmapImage(new Uri(@"C:\Users\faruk\Desktop\MODEL UYDU 2020\Documents\Images\computer.png"))
            };
            
        }

        private void setUpMap()
        {
            //mapView.Position = new PointLatLng(42, 21);

            GMapProviders.GoogleMap.ApiKey = "AIzaSyBVGE_WK-JzMB3-i5ntWH1bXqJ0TIGCHK4";
            mapView.MapProvider = GMapProviders.GoogleMap;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            mapView.MaxZoom = 70;
            mapView.MinZoom = 5;
            mapView.Zoom = 15;
            mapView.ShowCenter = false;
            mapView.DragButton = MouseButton.Left;
            

        }

        private void addMarker()
        {
            if(gps_lat.Text != "")
            {
                talia_marker = new GMapMarker(new PointLatLng(Convert.ToDouble(gps_lat.Text), Convert.ToDouble(gps_long.Text)));
                mapView.Markers.Add(talia_marker);

                try
                {
                    talia_marker.Shape = new System.Windows.Controls.Image
                    {
                        Width = 50,
                        Height = 50,
                        Source = new BitmapImage(new Uri(@"C:\Users\faruk\source\repos\VLCMediaPlayerLive\VLCMediaPlayerLive\TaliaLogo.png"))
                    };
                }
                catch (Exception ex)
                {
                    MessageBox.Show("TALIA MARKER "+ex.Message);
                }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //if any case to test g map
            mapView.Position = new PointLatLng(Convert.ToDouble(gps_lat.Text), Convert.ToDouble(gps_long.Text));


            talia_marker = new GMapMarker(new PointLatLng(Convert.ToDouble(gps_lat.Text), Convert.ToDouble(gps_long.Text)));
            mapView.Markers.Add(talia_marker);

            try
            {
                talia_marker.Shape = new System.Windows.Controls.Image
                {
                    Width = 50,
                    Height = 50,
                    Source = new BitmapImage(new Uri(@"C:\Users\faruk\source\repos\VLCMediaPlayerLive\VLCMediaPlayerLive\TaliaLogo.png"))
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        #endregion
    }
}
