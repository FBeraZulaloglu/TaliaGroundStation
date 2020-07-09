using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaliaGroundStation
{
    public class GaugeViewModel : INotifyPropertyChanged
    {
        int startPoint = 0;
        int stopPoint = 0;

        public event PropertyChangedEventHandler PropertyChanged; // event handler oluştur
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public GaugeViewModel(int start, int stop)
        {
            this.stopPoint = stop;
            this.startPoint = start;
            Value = 0;
            Angle = (Value * (178 / (stop - start))) - 89;
        }

        public GaugeViewModel()
        {
            
            Value = 0;
            Angle = Value  - 89;
        }



        double _angle;
        public double Angle
        {
            get
            {
                return _angle;
            }
            private set
            {
                _angle = value;
                NotifyPropertyChanged("Angle");
            }

        }

        double _value;
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value >= 0 && value <= 178)
                {
                    _value = value;
                    Angle = (Value * (178.4 / (stopPoint - startPoint ))) - 89.7;
                    NotifyPropertyChanged("Value");
                }

            }

        }
    }
}
