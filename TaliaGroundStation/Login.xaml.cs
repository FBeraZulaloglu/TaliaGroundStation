using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TaliaGroundStation
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        
        public Login()
        {
            InitializeComponent();
        }

        
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

        private void OpenMainWindow(object sender, RoutedEventArgs e)
        {
            if(password.Password != "")
            {
                MainWindow main = new MainWindow();
                main.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("You have to set a password first");
            }
            
        }
    }
}
