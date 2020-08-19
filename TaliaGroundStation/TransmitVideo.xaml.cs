using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;

namespace TaliaGroundStation
{
    /// <summary>
    /// Interaction logic for TransmitVideo.xaml
    /// </summary>
    public partial class TransmitVideo : Window
    {
        public TransmitVideo()
        {
            InitializeComponent();
            videoElement.LoadedBehavior = MediaState.Manual;
            videoElement.UnloadedBehavior = MediaState.Manual;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string url = "18..";
            
            //Upload(url);
            uploadFile(url, sourceFile.Text);
            /*if (!Directory.Exists(@"sendVideoToSat.mp4"))
            {
                //File.Move(@"aktar1.mp4", @"aktar_yedek.mp4");
                File.Create(@"sendVideoToSat.mp4");
            }
            else
            {
                File.Move(@"" + sourceFile.Text, @"sendVideoToSat.mp4");
                Console.WriteLine("File taşındı");
                Console.WriteLine(sourceFile.Text);
            }*/

        }

        private void fileGöster(object sender, RoutedEventArgs e)
        {
            try
            {

                videoElement.Source = new Uri(sourceFile.Text);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show("Böyle bir file yok");
            }
        }

        private void dropTheFile(object sender, DragEventArgs e)
        {
            Console.WriteLine("Drop file a girildi");
            string file = (string)((DataObject)e.Data).GetFileDropList()[0];

            try
            {
                videoElement.Source = new Uri(file);

                Console.WriteLine(file);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }
            
            videoElement.Play();
            sourceFile.Text = file;
        }


        private void solTiklaBırak(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            Console.WriteLine("Sol tıkla bırakıldı");
        }


        private void StopVideo(object sender, RoutedEventArgs e)
        {
            
            if (videoElement.Source.ToString() != "")
            {
                videoElement.Stop();
            }
            else
            {
                MessageBox.Show("Herhangi bir video bulunmamaktadır.");
            }

        }

        private void StartVideo(object sender, RoutedEventArgs e)
        {
            if (videoElement.Source.ToString() != "")
            {
                videoElement.Play();
            }
            else
            {
                MessageBox.Show("Herhangi bir video bulunmamaktadır.");
            }
        }

        private async Task<System.IO.Stream> Upload(string actionUrl, string paramString, Stream paramFileStream, byte[] paramFileBytes)
        {
            HttpContent stringContent = new StringContent(paramString);
            HttpContent fileStreamContent = new StreamContent(paramFileStream);
            HttpContent bytesContent = new ByteArrayContent(paramFileBytes);
            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                formData.Add(stringContent, "param1", "param1");
                formData.Add(fileStreamContent, "file1", "file1");
                formData.Add(bytesContent, "file2", "file2");
                var response = await client.PostAsync(actionUrl, formData);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                return await response.Content.ReadAsStreamAsync();
            }
        }

        private void uploadFile(String url,String path)
        {
            using (WebClient client = new WebClient())
            {
                client.UploadFile(url, path);
            }
        }

        private void mulipleRequestUpload(String fileUploadUrl,String filePath)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/octet-stream");
                using (Stream fileStream = File.OpenRead(filePath))
                using (Stream requestStream = client.OpenWrite(new Uri(fileUploadUrl), "POST"))
                {
                    fileStream.CopyTo(requestStream);
                }
            }
        }

        public string SendFile(string filePath)
        {
            WebResponse response = null;
            try
            {
                string sWebAddress = "Https://www.address.com";

                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(sWebAddress);
                wr.ContentType = "multipart/form-data; boundary=" + boundary;
                wr.Method = "POST";
                wr.KeepAlive = true;
                wr.Credentials = System.Net.CredentialCache.DefaultCredentials;
                Stream stream = wr.GetRequestStream();
                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

                stream.Write(boundarybytes, 0, boundarybytes.Length);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(filePath);
                stream.Write(formitembytes, 0, formitembytes.Length);
                stream.Write(boundarybytes, 0, boundarybytes.Length);
                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                string header = string.Format(headerTemplate, "file", Path.GetFileName(filePath), Path.GetExtension(filePath));
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                stream.Write(headerbytes, 0, headerbytes.Length);

                FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    stream.Write(buffer, 0, bytesRead);
                fileStream.Close();

                byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                stream.Write(trailer, 0, trailer.Length);
                stream.Close();

                response = wr.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream);
                string responseData = streamReader.ReadToEnd();
                return responseData;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }
    }
}
