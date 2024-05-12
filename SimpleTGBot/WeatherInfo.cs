using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTGBot
{
    class WeatherInfo
    {
        public class coord
        {
            public double lon { get; set; }
            public double lat { get; set; }
        }

        public class weather
        {
            public string main { get; set; }
            public string description { get; set; }
        }

        public class main
        {
            public double temp { get; set; }
            public double feels_like { get; set; }
            public int pressure { get; set; }
            public int humidity { get; set; }
        }

        public class wind
        {
            public double speed { get; set; }
        }

        public class rain
        {

        }

        public class snow
        {

        }

        public class sys
        {
            public long sunrise { get; set; }
            public long sunset { get; set; }
        }

        public class now
        {
            public coord coord { get; set; }
            public List<weather> weather { get; set; }
            public main main { get; set; }
            public wind wind { get; set; }
            public sys sys { get; set; }
            public rain rain { get; set; }
            public snow snow { get; set; }  
        }

        public class timestamp
        {
            public string dt_txt { get; set; }
            public main main { get; set; }
            public List<weather> weather { get; set; }
            public wind wind { get; set; }
        }

        public class city
        {
            public string name { get; set; }
        }
        public class day
        {
            public List<timestamp> list { get; set; }
            public city city { get; set; }
            public long sunrize { get; set; }
            public long sunset { get; set; }
        }
    }
}