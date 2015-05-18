using System;
using System.Device.Location;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Models.General
{
    [Serializable]
    //the class for the coordinates
    public class Coordinates : BaseModel
    {
        #region Constructors and Destructors

        public Coordinates(Coordinate latitude, Coordinate longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        private Coordinates(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("latitude")]
        public Coordinate Latitude { get; set; }

        [Versioning("longitude")]
        public Coordinate Longitude { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public override string ToString()
        {
            return Latitude + " " + Longitude;
        }

        //returns as geocoordinate
        public GeoCoordinate ConvertToGeoCoordinate()
        {
            return
                new GeoCoordinate(
                    MathHelpers.DMStoDeg(Latitude.Degrees, Latitude.Minutes, Latitude.Seconds),
                    MathHelpers.DMStoDeg(Longitude.Degrees, Longitude.Minutes, Longitude.Seconds));
        }

        #endregion
    }

    [Serializable]
    //the class for the coordinate
    public class Coordinate : BaseModel
    {
        #region Constructors and Destructors

        public Coordinate(int degrees, int minutes, int seconds)
        {
            Degrees = degrees;
            Minutes = minutes;
            Seconds = seconds;
        }

        private Coordinate(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum Directions
        {
            N,

            S,

            W,

            E
        };

        #endregion

        #region Public Properties

        [Versioning("degrees")]
        public int Degrees { get; set; }

        [Versioning("minutes")]
        public int Minutes { get; set; }

        [Versioning("seconds")]
        public int Seconds { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public override string ToString()
        {
            return $"{Degrees}°{Minutes}'{Seconds}";
        }

        #endregion
    }
}