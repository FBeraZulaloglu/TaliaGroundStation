using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows;
using SimpleWifi;


namespace TaliaGroundStation
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        #region variables
        string password_adress;
        Wifi taliaWifi;
        public string taliaPassword { get; set; }
        public string taliaWifiName { get; set; }
        string password_ip = "http://192.168.4.1/password";
        string command_ip = "http://192.168.4.1/command";
        #endregion

        public Login()
        {
            InitializeComponent();
        }

        #region login window loaded
        private void LoginLoaded(object sender, RoutedEventArgs e)
        {
            taliaWifi = new Wifi();
            password_adress = password_ip;
        }
        #endregion 

        #region verilen passwordu setle
        private void CreatePassword(object sender, RoutedEventArgs e)
        {
            InputDialog passwordInput = new InputDialog("Please enter your password");
            if (passwordInput.ShowDialog() == true)
            {
                QuestionDialog yes_no = new QuestionDialog("Your password is :" + passwordInput.Answer + " Do you confirm ?");
                if(yes_no.ShowDialog() == true)
                {
                    if (yes_no.confirmed == true)
                    {
                        password.Password = passwordInput.Answer;
                        MessageBox.Show("You have set a password");
                    }
                }
                
            }
            
        }
        #endregion

        #region ana sayfayı aç
        private void OpenMainWindow(object sender, RoutedEventArgs e)
        {
            if(password.Password != "")
            {
                if (taliaWifiName != null && taliaPassword != null)
                {
                    try
                    {
                        //passwordu uyduya gönder
                        using (WebClient wb = new WebClient())
                        {
                            byte[] response = wb.UploadData(password_adress, Encoding.UTF8.GetBytes(password.Password));
                            MessageBox.Show(response.ToString());
                        }
                        //gelen responsa göre bu sayfa açılır.
                        MainWindow main = new MainWindow();
                        main.Show();
                        this.Hide();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Herhangi bir ağa bağlanılmadığından bu işlem yapılamaz.");
                }
                
            }
            else
            {
                MessageBox.Show("Şifrenizi girmediniz. Tekrar deneyin!");
            }
        }
        #endregion

        #region wifi Connection
        private void ConnectToTalia(object sender, RoutedEventArgs e)
        {
            // connect talia wifi 
            bool isWifiFound = false;
            if (wifiPassword.Password != "" && wifiName.Text != "")
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
                            MessageBox.Show("Bağlantı Gerçekleşti.");
                            // değişkenleri ata bir sonraki sayfdada kullan
                            taliaPassword = request.Password;
                            taliaWifiName = ap.ToString() ;
                            MessageBox.Show(ap.ToString() + " talianın wifi ismi olarak setlendi");
                            MessageBox.Show(request.Password+" talianın şifresi olarak setlendi");
                        }
                        else
                        {
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

        #endregion

        
    }
}
