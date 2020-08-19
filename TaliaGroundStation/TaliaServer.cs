using System;
using System.Windows.Documents;
using System.Net.Http;
using System.Timers;

namespace TaliaGroundStation
{
    public class TaliaServer
    {
        string myContent;
        public TaliaServer(int zaman_aralığı)
        {
            System.Timers.Timer myTimer = new System.Timers.Timer();
            myTimer.Elapsed += new ElapsedEventHandler(GetTelemetryData);
            myTimer.Interval = zaman_aralığı; // 500 ms is one half second
            myTimer.Start();
        }

        public void GetTelemetryData(object source, ElapsedEventArgs e)
        {
            // code here will run every second
            GetRequest("http://192.168.4.1/readADC");
        }

        async void GetRequest(string url)
        {

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    using (HttpResponseMessage response = await client.GetAsync(url))
                    {
                        using (HttpContent content = response.Content)
                        {
                            myContent = await content.ReadAsStringAsync();
                            Console.WriteLine(myContent);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        public string GetTemp()
        {
            return myContent;
        }

        public string GetPressure()
        {
            return myContent;
        }
    }
}
