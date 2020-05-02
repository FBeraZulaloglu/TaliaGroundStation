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
        public event PropertyChangedEventHandler PropertyChanged; // event handler oluştur
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public GaugeViewModel()
        {
            Value = 0;
            Angle = -85;
        }

        public GaugeViewModel(int value)
        {
            Value = value;
            Angle = value - 85;
        }


        int _angle;
        public int Angle
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

        int _value;
        public int Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value > 0 && value < 170)
                {
                    _value = value;
                    Angle = value - 87;
                    NotifyPropertyChanged("Value");
                }

            }

        }
    }
}
