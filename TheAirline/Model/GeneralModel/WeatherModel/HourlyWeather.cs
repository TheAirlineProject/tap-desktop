using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.WeatherModel
{
    //the class for the weather for an hour
    [ProtoContract]
    public class HourlyWeather
    {
        [ProtoMember(1)]
        public double Temperature { get; set; }
        [ProtoMember(2)]
        public Weather.Precipitation Precip { get; set; }
        [ProtoMember(3)]
        public Weather.CloudCover Cover { get; set; }
        [ProtoMember(4)]
        public Weather.WindDirection Direction { get; set; }
        [ProtoMember(5)]
        public Weather.eWindSpeed WindSpeed { get; set; }
        public HourlyWeather(double Temperature, Weather.CloudCover cover, Weather.Precipitation precip, Weather.eWindSpeed windspeed, Weather.WindDirection direction)
        {
            this.Temperature = Temperature;
            this.Precip = precip;
            this.Cover = cover;
            this.WindSpeed = windspeed;
            this.Direction = direction;
        }
    }
}
