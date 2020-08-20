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
    /// Interaction logic for QuestionDialog.xaml
    /// </summary>
    public partial class QuestionDialog : Window
    {
        public bool confirmed = false;
        public QuestionDialog(String question = null)
        {
            InitializeComponent();
			this.lblQuestion.Content = question;
        }

		private void yesButton_clicked(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			confirmed = true;
		}
		
	}
}
