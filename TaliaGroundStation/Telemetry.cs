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
        maxHeight,
        falling,
        leaving,
        afterLeaving,
        waitingToRescue,
    }

    public class Telemetry
    {
        public String TakımNo { get; set; }
        public int PaketNo { get; set; }
        public String Time { get; set; }
        public double Pressure { get; set; }
        public double Height { get; set; }
        public double Velocity { get; set; }
        public double Temperature { get; set; }
        public double Volt { get; set; }
        public double Gps_lat { get; set; }//map
        public double Gps_long { get; set; }//map
        public double Altitude { get; set; }
        public Status Status { get; set; }//text boxta göster
        public double Pitch { get; set; }
        public double Roll { get; set; }
        public double Yaw { get; set; }
        public int RollCount { get; set; }
        public string IsVideoSent { get; set; } //
    }


}
