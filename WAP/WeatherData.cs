using System.Collections.Generic;


namespace WAP
{
    public class WeatherData
    {
        public string Name { get; set; }

        public List<Weather> Weather { get; set; }

        public MainParameters Main { get; set; }

        

        public Wind Wind { get; set; }
    }

    public class Weather
    {
        public string Main { get; set; }

        public string Description { get; set; }
    }

    public class MainParameters
    {
        public double Temp { get; set; }

        

        public int Pressure { get; set; }


        public int Humidity { get; set; }
    }

    public class Wind
    {
        public double Speed { get; set; }

        public int Deg { get; set; }

        public double Gust { get; set; }

    }
}
