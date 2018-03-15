using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT_Hub_Test_Client
{
    public class Sensor
    {
        public string SensorName { get; set; }
        public int NoiseLevel { get; set; }
        public string AirQuality { get; set; }
        public int AirPressure { get; set; }
        public DateTime Timecreated { get; set; }

        private Random _rnd = new Random();

        public int GetNoiseLevel()
        {
            int _i = _rnd.Next(50, 120);
            return _i;
        }

        public string GetAirQuality()
        {
            int _i = _rnd.Next(10);
            if (_i == 0)
                return "Poor";
            if (_i > 0 && _i <= 8)
                return "OK";
            else
                return "Good";
        }

        public int GetAirPressure()
        {
            int _iFail = _rnd.Next(15);

            // In 1 of 15 cases, simulate a bad sensor reading.
            if (_iFail == 0)
            {
                return 0;
            }
            else
            {
                int _i = _rnd.Next(500, 1500);
                return _i;
            }
        }
    }

}
