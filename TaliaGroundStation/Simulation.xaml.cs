﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaliaGroundStation
{
    /// <summary>
    /// Interaction logic for Simulation.xaml
    /// </summary>
    public partial class Simulation : UserControl
    {
       
        public Simulation()
        {
            InitializeComponent();
            height.Value = 100;
            
            /*Timer myTimer = new System.Timers.Timer();
            myTimer.Elapsed += new ElapsedEventHandler(DisplayEvent);
            myTimer.Interval = 1000; // 1000 ms is one half second
            myTimer.Start();*/

            
            
        }

        public void DisplayEvent(object source, ElapsedEventArgs e)
        {
            // code here will run every half second
            this.Dispatcher.Invoke((Action)(() =>
            {
                //donusCounter.Text = anaEkran.server.GetPressure();
            }
            ));

        }
    }
}
