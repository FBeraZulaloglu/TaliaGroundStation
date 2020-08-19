using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaliaGroundStation
{
    public enum Status
    {
        ground,
        climbing,
        falling,
        leaving,
        waitingToRescue,
    }

    class Telemetry
    {
        public String _takımNo { get; set; }
        public int _paketNo { get; set; }
        public String _time { get; set; }
        public double _pressure { get; set; }
        public double _height { get; set; }
        public double _velocity { get; set; }
        public double _volt { get; set; }
        public double _gps_lat { get; set; }//map
        public double _gps_long { get; set; }//map
        public double _altitude { get; set; }
        public Status _status { get; set; }//text boxta göster
        public double _pitch { get; set; }
        public double _roll { get; set; }
        public double _yaw { get; set; }
        public int _rollCount { get; set; }
        public string _isVideoSent { get; set; } //
    }


}
