using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using AForge.Video;
using System.Net;
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
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using SimpleWifi;
using System.Windows.Controls;

namespace TaliaGroundStation
{
    /// <summary>
    /// Talia Model Uydunun Yer İstasyon Yazılımı.
    /// Verilerin Görünütlenmesi Ve Kaydedilmesi Amaçlı Yazılmıştır.
    /// </summary>
    
    public partial class MainWindow : Window
    {

        #region variables
        readonly AVIWriter writer;
        public ObservableCollection<Telemetry> Telemetry_list { get; set; }
        string video_ip;
        string telemetry_ip;
        string file_ip;
        StringBuilder talia_csv;
        string talia_durum;
        string wifiAd ;
        
        Telemetry current_telemetry;
        int missing_data_counter = 0;
        public StreamReader STR;
        public StreamWriter STW;
        BackgroundWorker backgroundWorker;
        
        GMapMarker talia_marker;
        GMapMarker ground_marker;
        string[] data_array;
        //grafik listeleri
        List<double> volt_list;//series4
        List<double> pressure_list;//series3
        List<double> height_list;//series2
        List<double> speed_list;//series1
        List<double> temp_list;//all series x axis
        List<String> time_list;//all series x axis

        
        Wifi taliaWifi;
        string fileName = null;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            data_array = new string[17];
            video_ip = "http://192.168.4.1/picture";
            telemetry_ip = "http://192.168.4.1/data";
            file_ip = "http://192.168.4.1/file";
            writer = new AVIWriter();
            writer.Open("TMUY2020_57413_VIDEO.avi", 800, 600);
            talia_csv = new StringBuilder();
            talia_csv.Append("TAKIM NO,PAKET NO,ZAMAN,BASINC,YUKSEKLIK,HIZ,SICAKLIK,VOLT,LAT,LONG,ALTITUDE,DURUM,PITCH,ROLL,YAW,DONUS SAYISI,VIDEO AKTARILDI"+"\n");
            talia_marker = new GMapMarker(new PointLatLng(42,21));
            Telemetry_list = new ObservableCollection<Telemetry>();
            pressure_list = new List<double>();
            height_list = new List<double>();
            volt_list = new List<double>();
            speed_list = new List<double>();
            temp_list = new List<double>();
            time_list = new List<String>();
            taliaWifi = new Wifi();
            taliaWifi.ConnectionStatusChanged += ConnectionStatusChanged;
            videoPlayer.NewFrame += VideoPlayer_NewFrame;// works when new frame came
            mapView.OnPositionChanged += new PositionChanged(MainMap_OnPositionChanged);
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            Telemetry_list.CollectionChanged += this.OnCollectionChanged;
            telemetry_table.Items.Clear();// to run the item source
        }

        #region Window Actions
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            SetCharts();
            startVideo();
            videoElement.LoadedBehavior = MediaState.Manual;
            videoElement.UnloadedBehavior = MediaState.Manual;
            telemetry_timer_start(1000);

        }

        private void ProgramClosed(object sender, EventArgs e)
        {
            //video writerı kapat
            writer.Close();
        }
        #endregion

        #region Taze telemetri verisi :)
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableCollection<Telemetry> obsSender = sender as ObservableCollection<Telemetry>;
            
            Dispatcher.BeginInvoke((Action)(() => {
                //yeni data geldiği anda kaydet
                //File.WriteAllText("TMUY2020_57413_TLM.csv", talia_csv.ToString());
                
                durum.Text = talia_durum;
               
                height.Value = current_telemetry.Height;

                //telemetry tablosunu yenile
                telemetry_table.Items.Refresh();
                if(obsSender != null)
                    telemetry_table.ItemsSource = obsSender;
                //Console.WriteLine("Enumarator :: "+telemetry_table.ItemsSource.GetEnumerator());
                //scroll barı sona getir
                if (telemetry_table.Items.Count > 0)
                {
                    var border = System.Windows.Media.VisualTreeHelper.GetChild(telemetry_table, 0) as Decorator;
                    if (border != null)
                    {
                        var scroll = border.Child as System.Windows.Controls.ScrollViewer;
                        if (scroll != null) scroll.ScrollToEnd();
                    }
                }

                //grafiklerde kullanılacak olan zaman verisini ekle
                time_list.Add(DateTime.Now.ToString("hh:mm:ss"));// add time just once
                AddDataToCharts();

                //grafikler 20 veriden fazla veri göstermemeleri için
                if (time_list.Count > 20)
                {
                    time_list.RemoveAt(0);
                }
                if (pressure_list.Count > 20)
                {
                    pressure_list.RemoveAt(0);
                }
                if (temp_list.Count > 20)
                {
                    temp_list.RemoveAt(0);
                }
                if (speed_list.Count > 20)
                {
                    speed_list.RemoveAt(0);
                }
                if (height_list.Count > 20)
                {
                    height_list.RemoveAt(0);
                }
                //simülasyonun değerlerini yerleştir
                simulation.roll = current_telemetry.Roll;
                simulation.pitch = current_telemetry.Pitch;
                simulation.yaw = current_telemetry.Yaw;
                simulation.rollCount = current_telemetry.RollCount;
                //uydu ile yer istasyonu arasındaki mesafeyi bul ve haritayı yenile

                mapView.Position = new PointLatLng(current_telemetry.Gps_lat, current_telemetry.Gps_long);
                talia_marker.Position = new PointLatLng(current_telemetry.Gps_lat, current_telemetry.Gps_long);
            }));
            NotifyCollectionChangedAction action = e.Action;
        }
        #endregion

        #region Commands
        private void OpenLock(object sender, RoutedEventArgs e)
        {
            //servonun çalışması için komut gönder
            //send command to start the servo motor
            using (WebClient wb = new WebClient())
            {
                if (openLock.Content.Equals("Manuel Ayrılma"))
                {
                    byte[] lock_open = { (byte)'4' };
                    try
                    {
                        wb.UploadData("http://192.168.4.1/command", lock_open);
                        MessageBox.Show("Ayrılma Mekanizması Açıldı!!");
                        openLock.Content = "Manuel Birleşme";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
                else
                {
                    byte[] lock_open = { (byte)'5' };
                    try
                    {
                        wb.UploadData("http://192.168.4.1/command", lock_open);
                        MessageBox.Show("Ayrılma Mekanizması Açıldı!!");
                        openLock.Content = "Manuel Ayrılma";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }


        private void OpenBuzzer(object sender, RoutedEventArgs e)
        {
            //buzzerın çalışmkası için komut gönder
            //send command to start buzzer açmak için 2 kapatmak için 3
            using (WebClient wb = new WebClient())
            {
                if (openBuzzer.Content.Equals("Buzzer Aç"))
                {

                    byte[] open = { 2 };
                    try
                    {
                        wb.UploadData("http://192.168.4.1/command", open);
                        openBuzzer.Content = "Buzzer Kapat";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
                else
                {
                    byte[] close = { 3 };
                    wb.UploadData("http://192.168.4.1/command", close);
                    openBuzzer.Content = "Buzzer Aç";
                }
            }
        }


        private void resetEEPROM(object sender, RoutedEventArgs e)
        {
            using (WebClient wb = new WebClient())
            {
                byte[] reset = { (byte)'6' };
                try
                {
                    wb.UploadData("http://192.168.4.1/command", reset);
                    MessageBox.Show("KAPATILDI.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //eeprom sıfırlanamadı tekrar deneyin
                }

            }
        }

        private void OpenAktif(object sender, RoutedEventArgs e)
        {
            using (WebClient wb = new WebClient())
            {
                if (aktifInis.Content.Equals("Manuel Tahrik"))
                {
                    byte[] reset = { (byte)'7' };
                    try
                    {
                        wb.UploadData("http://192.168.4.1/command", reset);
                        MessageBox.Show("Aktif İniş Sistemi Çalıştı.");
                        aktifInis.Content = "Tahrik Kapat";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        //eeprom sıfırlanamadı tekrar deneyin
                    }
                }
                else
                {
                    byte[] reset = { (byte)'8' };
                    try
                    {
                        wb.UploadData("http://192.168.4.1/command", reset);
                        MessageBox.Show("Aktif İniş Sistemi Çalıştı.");
                        aktifInis.Content = "Manuel Tahrik";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        //Motor Kapatılamadı
                    }
                }
                

            }
        }
        #endregion

        #region Extra Windows
        private void videoSending(object sender, RoutedEventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();

                // Send file fileName to remote device
                Console.WriteLine(fileName);
            }
            else
            {
                Console.WriteLine("Background is busy");
            }
            
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //BackgroundWorker worker = sender as BackgroundWorker;

            if (fileName != null)
            {

                using (WebClient client = new WebClient())
                {
                    try
                    {
                        //MKV yi mp4 olarak çeviren metod
                        //await Conversion.Convert("inputfile.mkv", "file.mp4").Start()
                        client.UploadProgressChanged += client_UploadProgressChanged;
                        System.Threading.Tasks.Task<byte []> task;
                        MessageBox.Show("Dosya Gönderme İşlemi Başlatıldı");
                        
                        task = client.UploadFileTaskAsync(new Uri(file_ip), "POST", fileName);
                        Console.WriteLine("TASK RESULT TO STRİNG" + task.Result.ToString());

                        if (task.IsCompleted)
                        {
                            Dispatcher.Invoke((Action)(() =>
                            {
                                IsVideoSent.IsChecked = true;
                            }));
                            
                            MessageBox.Show("Video gönderimi tamamlandı. Tebrikler. Elhamdülillah :)");
                        }

                        //MessageBox.Show("Remote Response: {0}", System.Text.Encoding.ASCII.GetString(rawResponse));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Dosya Gönderilemedi" + ex.ToString());
                    }

                }
            }
            else
            {
                MessageBox.Show("Herhangi bir dosya yüklemediniz.");
            }
            
        }

        private void client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            // file gönderirken aktif olacak event
            Console.WriteLine("Percentege ::: "+e.ProgressPercentage);
            Dispatcher.Invoke((Action)(()=>
            {
                SendVideoPercentage.Value = e.ProgressPercentage;
            }));
            
        }

        private void OpenFullMap(object sender, MouseButtonEventArgs e)
        {
            FullSizeMap fsm = new FullSizeMap();
            fsm.Show();
        }
        #endregion

        #region Charts

        private void setPressureChart()
        {
            pressureArea.AxisX.MajorGrid.LineColor = Color.Red;
            pressureArea.AxisY.MajorGrid.LineColor = Color.Red;

            pressureSeries.Color = Color.Green;
            pressureSeries.BorderWidth = 3;

            pressureArea.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.WordWrap;
            pressureArea.AxisY.Minimum = 80;
            pressureArea.AxisY.Maximum = 120;
            pressureArea.AxisX.Interval = 1.0;

            pressureChart.Titles.Add("Basınç(kPa)/Zaman");
            pressureSeries.Points.DataBindXY(time_list, pressure_list);
        }

        private void setHeightChart()
        {
            heightArea.AxisX.MajorGrid.LineColor = Color.Red;
            heightArea.AxisY.MajorGrid.LineColor = Color.Red;

            heightSeries.BorderWidth = 3;
            heightSeries.Color = Color.Green;

            heightArea.AxisY.Maximum = 700;
            heightArea.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.IncreaseFont;
            heightArea.AxisX.Interval = 1.0;

            heightChart.Titles.Add("Yükseklik(metre)/Zaman");
            heightSeries.Points.DataBindXY(time_list, height_list);
        }

        private void setVoltChart()
        {
            voltArea.AxisX.MajorGrid.LineColor = Color.Red;
            voltArea.AxisY.MajorGrid.LineColor = Color.Red;

            voltSeries.Color = Color.Green;
            voltSeries.BorderWidth = 3;
            voltArea.AxisY.Maximum = 10;
            voltArea.AxisX.Interval = 1.0;
            voltArea.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.IncreaseFont;
            voltArea.AxisY.Maximum = 10;

            voltChart.Titles.Add("Volt(V)/Zaman");
            voltSeries.Points.DataBindXY(time_list, volt_list);
        }

        private void setSpeedChart()
        {
            speedArea.AxisX.MajorGrid.LineColor = Color.Red;
            speedArea.AxisY.MajorGrid.LineColor = Color.Red;
            speedArea.AxisY.Maximum = 20;

            speedSeries.BorderWidth = 3;
            speedSeries.Color = System.Drawing.Color.Green;
            speedChart.Titles.Add("Hiz(m/s)/Zaman");

            speedArea.AxisX.Interval = 1.0;
            speedArea.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.IncreaseFont;

            speedSeries.Points.DataBindXY(time_list, speed_list);
        }

        private void setTempChart()
        {
            tempArea.AxisX.MajorGrid.LineColor = Color.Red;
            tempArea.AxisY.MajorGrid.LineColor = Color.Red;
            tempArea.AxisY.Maximum = 60;
            tempSeries.BorderWidth = 3;
            tempSeries.Color = Color.Green;
            tempChart.Titles.Add("Sıcaklık(Celcius)/Zaman");
            tempArea.AxisX.Interval = 1.0;
            tempArea.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.IncreaseFont;

            tempSeries.Points.DataBindXY(time_list, temp_list);
        }

        private void PressureChartAddData()
        {

            Dispatcher.BeginInvoke((Action)(() =>
            {
                
                pressure_list.Add(current_telemetry.Pressure);
                if(pressure_list.Count == time_list.Count)
                {
                    try
                    {
                        pressureSeries.Points.DataBindXY(time_list, pressure_list);
                        pressureChart.Invalidate();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString()+" Pressure Listte Problem oluştu");
                    }
                    
                }
                
            }));
        }

        private void HeightChartAddData()
        {

            Dispatcher.BeginInvoke((Action)(() =>
            {
                height_list.Add(current_telemetry.Height);
                
                if(height_list.Count == time_list.Count)
                {
                    try
                    {
                        heightSeries.Points.DataBindXY(time_list, height_list);
                        heightChart.Invalidate();// redrawn the graph
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString()+" height listte problem oluştu");
                    }
                    
                }
                

            }));
        }

        private void VoltChartAddData()
        {

            Dispatcher.BeginInvoke((Action)(() =>
            {
                volt_list.Add(current_telemetry.Volt);
                
                if (volt_list.Count == time_list.Count)
                {
                    try
                    {
                        voltSeries.Points.DataBindXY(time_list, volt_list);
                        voltChart.Invalidate();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString()+" Volt listesinde problem oluştu");
                    }
                    
                }
            }));
        }

        private void SpeedChartAddData()
        {
            
            Dispatcher.BeginInvoke((Action)(() =>
            {
                speed_list.Add(current_telemetry.Velocity);
                if (speed_list.Count == time_list.Count)
                {
                    try
                    {
                        speedSeries.Points.DataBindXY(time_list, speed_list);
                        speedChart.Invalidate();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString() + " speed listte hata oluştu.");
                    }
                    
                }
                
            }));
        }

        private void TempChartAddData()
        {

            Dispatcher.BeginInvoke((Action)(() =>
            {
                temp_list.Add(current_telemetry.Temperature);
                if (temp_list.Count == time_list.Count)
                {
                    try
                    {
                        tempSeries.Points.DataBindXY(time_list, temp_list);
                        tempChart.Invalidate();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString() + " speed listte hata oluştu.");
                    }

                }

            }));
        }

        private void SetCharts()
        {
            setPressureChart();
            setVoltChart();
            setTempChart();
            setSpeedChart();
            setHeightChart();
        }
        // adds all charts to necessary data
        private void AddDataToCharts()
        {
            PressureChartAddData();
            VoltChartAddData();
            TempChartAddData();
            SpeedChartAddData();
            HeightChartAddData();

        }
        #endregion

        #region  Telemetry

        // timer to send reques
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
            var data = getTelemetry(telemetry_ip);
            if (data != null)
            {
                
                try
                {
                    File.AppendAllText("TMUY2020_57413_TLM.csv", data+"\n");
                    //split data and show on table
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        talia_csv.Append(data + "\n");
                        Telemetry_list.Add(SetCurrentTelemetry(data));
                        //eklendikten sonra evente gider
                    }));
                }
                catch(Exception ex)
                {
                    Console.WriteLine("DATA GELDİKTEN SONRA  ::::::::   "+ex.ToString());
                }
                
            }
            else
            {
                missing_data_counter++;
                Console.WriteLine("data is missing ..."+missing_data_counter+". data");
                // kaybedilen data sayısını bir texte yazdır.
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
                current_telemetry.TakımNo = data_array[0];
                current_telemetry.PaketNo = int.Parse(data_array[1]);
                current_telemetry.Time = data_array[2];
                current_telemetry.Pressure = double.Parse(data_array[3],CultureInfo.InvariantCulture.NumberFormat);
                current_telemetry.Height = double.Parse(data_array[4], CultureInfo.InvariantCulture.NumberFormat);
                current_telemetry.Velocity = double.Parse(data_array[5], CultureInfo.InvariantCulture.NumberFormat);
                current_telemetry.Temperature = double.Parse(data_array[6], CultureInfo.InvariantCulture.NumberFormat);
                current_telemetry.Volt = double.Parse(data_array[7], CultureInfo.InvariantCulture.NumberFormat);
                current_telemetry.Gps_lat = double.Parse(data_array[8], CultureInfo.InvariantCulture.NumberFormat);
                current_telemetry.Gps_long = double.Parse(data_array[9], CultureInfo.InvariantCulture.NumberFormat);
                current_telemetry.Altitude = double.Parse(data_array[10], CultureInfo.InvariantCulture.NumberFormat);
                decideStatus(data_array[11],current_telemetry);
                Console.WriteLine("Status :"+data_array[11]);
                current_telemetry.Pitch = double.Parse(data_array[12], 
                    CultureInfo.InvariantCulture.NumberFormat);
                current_telemetry.Roll = double.Parse(data_array[13], CultureInfo.InvariantCulture.NumberFormat);
                current_telemetry.Yaw = double.Parse(data_array[14], CultureInfo.InvariantCulture.NumberFormat);
                current_telemetry.RollCount = int.Parse(data_array[15]);
                current_telemetry.IsVideoSent = data_array[16];
                return current_telemetry;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            
        }

        //getting status as an enum
        private void decideStatus(String status, Telemetry current_telemetry)
        {

            switch (int.Parse(status))
            {

                case 0:
                    current_telemetry.Status = Status.ground;
                    talia_durum = "YERDE";
                    break;
                case 1:
                    current_telemetry.Status = Status.climbing;
                    talia_durum = "TIRMANIYOR";
                    break;
                case 2:
                    current_telemetry.Status = Status.maxHeight;
                    talia_durum = "TEPE NOKTASINA ULAŞILDI";
                    break;
                case 3:
                    current_telemetry.Status = Status.falling;
                    talia_durum = "DÜŞÜYOR";
                    break;
                case 4:
                    current_telemetry.Status = Status.leaving;
                    talia_durum = "AYRILMA GERÇEKLEŞTİ";
                    break;
                case 5:
                    current_telemetry.Status = Status.afterLeaving;
                    talia_durum = "AYRILMA SONRASI";
                    break;
                case 6:
                    current_telemetry.Status = Status.waitingToRescue;
                    talia_durum = "KURTARILMAYI BEKLİYOR";
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
            if(image != null)
            {
                try
                {
                    writer.AddFrame(image);// to save the video
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message+" TO SAVE IMAGE FRAME");
                }
                
            }
            
        }

        #endregion

        #region GMap
        private void mapLoaded(object sender, RoutedEventArgs e)
        {
            setUpMap();
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

        private void groundLocationSet(object sender, RoutedEventArgs e)
        {
            //marker yer istasyonu
            if (groundLocation.Content.Equals("Konum Belirle"))
            {
                if (current_telemetry == null)
                {
                    MessageBox.Show("Uyduyu başlattığınızdan emin olunuz.");
                }
                else
                {
                    if (current_telemetry.Gps_lat != 0 && current_telemetry.Gps_long != 0)
                    {
                        mapView.Position = new PointLatLng(current_telemetry.Gps_lat, current_telemetry.Gps_long);
                        ground_marker.Position = new PointLatLng(current_telemetry.Gps_lat, current_telemetry.Gps_long);
                        groundLocation.Content = "Konum Değiştir";
                        gps_lat.Text = current_telemetry.Gps_lat.ToString();
                        gps_long.Text = current_telemetry.Gps_long.ToString();
                        gps_lat.IsEnabled = true;
                        gps_long.IsEnabled = true;
                    }
                    else
                    {
                        MessageBox.Show("Uydudan düzgün değer okunamadı. Lütfen değerleri kendiniz belirleyiniz.");
                    }
                }

            }
            else
            {
                if (gps_lat.Text != "" && gps_long.Text != "")
                {
                    double lat_text = Convert.ToDouble(gps_lat.Text, CultureInfo.InvariantCulture.NumberFormat);
                    double long_text = Convert.ToDouble(gps_long.Text, CultureInfo.InvariantCulture.NumberFormat);
                    setGroundMarker(lat_text, long_text);
                }
                else
                {
                    MessageBox.Show("Latitude ve Longitude değerleri bş bırakılamaz");
                }
            }
        }

        //mapteki konum değiştitğinde yer istasyonu ile uydu arasındaki mesafeyi ölçer ve yazar
        private void MainMap_OnPositionChanged(PointLatLng point)
        {
            Console.WriteLine("konum değişti");
            if (ground_marker != null)
            {
                //DISTANCE FORMULE arccos[(sin(lat1) * sin(lat2)) + cos(lat1) * cos(lat2) * cos(long2 – long1)]
                //https://www.geeksforgeeks.org/program-distance-two-points-earth/#:~:text=For%20this%20divide%20the%20values,is%20the%20radius%20of%20Earth.

                double dlong = toDegree(talia_marker.Position.Lng) - toDegree(ground_marker.Position.Lng);
                double dlat = toDegree(talia_marker.Position.Lat) - toDegree(ground_marker.Position.Lat);
                double dist = Math.Pow(Math.Sin(dlat / 2), 2) +
                              Math.Cos(toDegree(ground_marker.Position.Lat)) *
                              Math.Cos(toDegree(talia_marker.Position.Lat)) *
                              Math.Pow(Math.Sin(dlong / 2), 2);

                dist = 2 * Math.Asin(Math.Sqrt(dist));
                //double d = 3963.0 * Math.Acos((Math.Sin(ground_marker.Position.Lat)* Math.Sin(talia_marker.Position.Lat))+ Math.Cos(ground_marker.Position.Lat)*Math.Cos(talia_marker.Position.Lat)*Math.Cos(talia_marker.Position.Lng-ground_marker.Position.Lng));
                distance.Text = "Distance: " + (int)((dist * 6371) * 1000) + "m";
            }

        }

        private void setGroundMarker(double longitude,double latitude)
        {
            ground_marker = new GMapMarker(new PointLatLng(latitude,longitude));
            mapView.Markers.Add(ground_marker);

            try
            {
                ground_marker.Shape = new System.Windows.Controls.Image
                {
                    Width = 50,
                    Height = 50,
                    Source = new BitmapImage(new Uri(@"C:\Users\faruk\source\repos\TaliaGroundStation\TaliaGroundStation\groundStation.png"))
                };
                MessageBox.Show("Yer istasyonunu konumu setlendi");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ground MARKER " + ex.Message);
            }
        }

        private void ShowRoute(object sender, RoutedEventArgs e)
        {
            

            GMap.NET.WindowsForms.GMapOverlay polyOverlay = new GMap.NET.WindowsForms.GMapOverlay("polygons");
            IList<PointLatLng> points = new List<PointLatLng>();
            points.Add(ground_marker.Position);
            points.Add(talia_marker.Position);
            GMap.NET.WindowsForms.GMapPolygon polygon = new GMap.NET.WindowsForms.GMapPolygon((List<PointLatLng>)points, "mypolygon");
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Red));
            polygon.Stroke = new Pen(Color.Red, 1);
            polyOverlay.Polygons.Add(polygon);
            

            //RoutingProvider routingProvider =
            //mapView.MapProvider as RoutingProvider ?? GMapProviders.GoogleSatelliteMap;

            //MapRoute route = routingProvider.GetRoute(
            //    ground_marker.Position, //start
            //    talia_marker.Position, //end
            //    false, //avoid highways 
            //    false, //walking mode
            //    (int)mapView.Zoom);


            //GMapRoute gmRoute = new GMapRoute(route.Points);

            //mapView.Markers.Add(gmRoute);
            //distance.Text = "Distance: " + route.Distance * 1000 + " m";

        }

        private void getDistance()
        {
            Console.WriteLine("konum değişti");
            if (ground_marker != null)
            {
                //DISTANCE FORMULE arccos[(sin(lat1) * sin(lat2)) + cos(lat1) * cos(lat2) * cos(long2 – long1)]
                //https://www.geeksforgeeks.org/program-distance-two-points-earth/#:~:text=For%20this%20divide%20the%20values,is%20the%20radius%20of%20Earth.

                double dlong = toDegree(talia_marker.Position.Lng) - toDegree(ground_marker.Position.Lng);
                double dlat = toDegree(talia_marker.Position.Lat) - toDegree(ground_marker.Position.Lat);
                double dist = Math.Pow(Math.Sin(dlat / 2), 2) +
                              Math.Cos(toDegree(ground_marker.Position.Lat)) *
                              Math.Cos(toDegree(talia_marker.Position.Lat)) *
                              Math.Pow(Math.Sin(dlong / 2), 2);

                dist = 2 * Math.Asin(Math.Sqrt(dist));
                //double d = 3963.0 * Math.Acos((Math.Sin(ground_marker.Position.Lat)* Math.Sin(talia_marker.Position.Lat))+ Math.Cos(ground_marker.Position.Lat)*Math.Cos(talia_marker.Position.Lat)*Math.Cos(talia_marker.Position.Lng-ground_marker.Position.Lng));
                distance.Text = "Distance: " + (int)((dist * 6371) * 1000) + "m";
            }
        }

        public double toDegree(double degree)
        {
            return (Math.PI / 180) * (degree);
        }
        #endregion

        #region Wifi Connection
       
        private void ConnectToTalia(object sender, RoutedEventArgs e)
        {
            bool isWifiFound = false;
            if (password.Password != "" && wifiName.Text != "")
            {
                List<AccessPoint> pas = taliaWifi.GetAccessPoints();
                foreach (AccessPoint ap in pas)
                {
                    if (ap.Name.Equals(wifiName.Text))
                    {
                        isWifiFound = true;
                        AuthRequest request = new AuthRequest(ap);
                        request.Password = password.Password;
                        bool isConnected = ap.Connect(request);
                        if (isConnected)
                        {
                            wifiQuality.Text = ap.Name + " : " + ap.SignalStrength+"%";
                            IsWifiConnected.IsChecked = true;
                            MessageBox.Show("Bağlantı Gerçekleşti.");
                            wifiAd = wifiName.Text;
                        }
                        else
                        {
                            wifiQuality.Text = "Bağlı olunan bir ağ bulunamadı";
                            Dispatcher.Invoke((Action)(() =>
                            {
                                IsWifiConnected.IsChecked = false;
                            }));
                            
                            MessageBox.Show("Bağlantı Gerçekleşmedi. Tekrar Deneyin.");
                        }
                    }
                }

                if (!isWifiFound)
                {
                    MessageBox.Show("Wifi İsmi Bulunamamaktadır. Tekrar Deneyin.");
                }
            }
            else
            {
                MessageBox.Show("Wifi adı veya şifre boş bırakılamaz");
            }
        }

        private void ConnectionStatusChanged(object sender, WifiStatusEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (taliaWifi.ConnectionStatus.Equals(WifiStatus.Disconnected))
                {
                    wifiQuality.Text = "Ağ bağlantısı koptu";
                    IsWifiConnected.IsChecked = false;
                    Console.WriteLine("WİFİ BAĞLANTISI KOPTU");
                    if (wifiAd != null)
                    {
                        Console.WriteLine("Bağlantı koptu tekrar bağlanıldı.");
                        List<AccessPoint> accesPoints = taliaWifi.GetAccessPoints();

                        foreach (AccessPoint ap in accesPoints)
                        {
                            if (ap.Name.Equals(wifiAd))
                            {
                                AuthRequest request = new AuthRequest(ap);
                                request.Password = password.Password;
                                ap.Connect(request);
                                if (ap.IsConnected)
                                {
                                    wifiQuality.Text = ap.Name + " : " + ap.SignalStrength + "%";
                                    IsWifiConnected.IsChecked = true;
                                }
                            }
                        }
                    }
                }
            }));
        }
        #endregion

        #region Video Gönderme
         

        private void DropVideoToMedia(object sender, DragEventArgs e)
        {
            Console.WriteLine("Drop file a girildi");
            string file = (string)((DataObject)e.Data).GetFileDropList()[0];

            try
            {
                videoElement.Source = new Uri(file);
                fileName = file;
                Console.WriteLine(file);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }

            videoElement.Play();
        }

        private void DropVideo(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        #endregion


    }
}
