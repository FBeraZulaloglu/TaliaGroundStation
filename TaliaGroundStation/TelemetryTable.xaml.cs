using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO.Ports;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TaliaGroundStation
{
    /// <summary>
    /// Interaction logic for TelemetryTable.xaml
    /// </summary>
    public partial class TelemetryTable : UserControl
    {
        SerialPort mySerialPort;
        string recieved_data;
        List<Telemetry> allData = new List<Telemetry>();
        public TelemetryTable()
        {
            InitializeComponent();

            //ConfigurSerialPort();

            //GetDatas();
            for (int i = 0; i < 10; i++)
            {
                allData.Add(new Telemetry() { paketNo = i.ToString(), saat = (string)"17:40:30", basınc = (string)"101.2", sıcaklık = (string)"31" });
                telemetry_table.ItemsSource = null;
                telemetry_table.ItemsSource = allData;

            }

        }


        class Telemetry{
            public string paketNo { get; set; }
            public string saat { get; set; }
            public string sıcaklık { get; set; }
            public string basınc { get; set; }
        }

        private void ConfigurSerialPort()
        {
            mySerialPort = new SerialPort("COM4");
            mySerialPort.BaudRate = 9600;
            mySerialPort.Parity = Parity.Odd;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 7;
            //mySerialPort.Handshake = Handshake.RequestToSend; //input buffer dolarsa RTS(request to send) line false olur in put bufferda yer var iken RTS line true olur
            //mySerialPort.Handshake = Handshake.RequestToSendXOnXOff;//
            //mySerialPort.Handshake = Handshake.XOnXOff;//Xoff send to stop transmisson data Xon send to resume the transmission(These controls are used ınstead of request to send or clear to send hardware controls)
            mySerialPort.Handshake = Handshake.None;
            mySerialPort.RtsEnable = false;
            mySerialPort.ReadTimeout = 1000;//1000 milisecond 
            mySerialPort.Encoding = ASCIIEncoding.ASCII;

            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            try
            {
                mySerialPort.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {

            //serialPort = new SerialPort();
            mySerialPort = (SerialPort)sender;

            try
            {
                recieved_data = mySerialPort.ReadLine();
                Console.WriteLine("Data recieve is succesfull");
                Console.WriteLine(recieved_data);

            }
            catch
            {
                Console.WriteLine("Data recieve is not succesfull");
            }

            this.Dispatcher.Invoke((Action)(() =>
            {
                //allData.Add(new Telemetry() { paketNo = 1, saat = "21:22:10", basınc = 12, sıcaklık = 10 });
                telemetry_table.ItemsSource = null;
                telemetry_table.ItemsSource = allData;
                //telemetry_table.Items.Refresh();

                
            }
            ));

        }

        async void GetDatas()
        {
            using (var webClient = new WebClient())
            {
                String rawJson = webClient.DownloadString("https://api.openweathermap.org/data/2.5/weather?q=Ankara,tr&appid=5ff5f6121eced2f3ad373070cbbb2040");
                JObject jObject = JObject.Parse(rawJson);

                //coord bir obje oldugu icin onu da parcalamamiz gerekiyor
                JToken jCoord = jObject["coord"];

                //artik coord nesnesinin elemanlarina ulasabilirim
                Console.WriteLine((string)jCoord["lon"]);
                Console.WriteLine((string)jCoord["lat"]);


                //base bir deger oldugu icin onu dogrudan kullanabiliyorum
                Console.WriteLine((string)jObject["base"]);

                allData.Add(new Telemetry() { paketNo = (string)jObject["base"], saat = (string)jCoord["lon"], basınc = (string)jCoord["lat"], sıcaklık = (string)jCoord["lat"] });
                telemetry_table.ItemsSource = null;
                telemetry_table.ItemsSource = allData;

                /*var border = VisualTreeHelper.GetChild(telemetry_table, 0) as Decorator;
                if (border != null)
                {
                    var scroll = border.Child as ScrollViewer;
                    if (scroll != null)
                    {
                        scroll.ScrollToEnd();
                    }
                }*/
            }
            }
    }
}
