using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    [ProtoContract]
    //the class for the coordinates
    public class Coordinates : IComparable<Coordinates>
    {
        [ProtoMember(1)]
        public Coordinate Latitude { get; set; }
        [ProtoMember(2)]
        public Coordinate Longitude { get; set; }
        public Coordinates(Coordinate latitude, Coordinate longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }
        public override string ToString()
        {
            return this.Latitude.ToString() + " " + this.Longitude.ToString();
        }


        #region IComparable<Coordinates> Members

        public int CompareTo(Coordinates coordinates)
        {
            int compare = coordinates.Latitude.toDecimal().CompareTo(this.Latitude.toDecimal());
            if (compare == 0)
                return coordinates.Longitude.toDecimal().CompareTo(this.Longitude.toDecimal());
            return compare;
        }

        #endregion
    }
    [ProtoContract]
    //the class for the coordinate
    public class Coordinate
    {
        
        public enum Directions { N, S, W, E };

        [ProtoMember(1)]
        public int Degrees { get; set; }
        [ProtoMember(2)]
        public int Minutes { get; set; }
        [ProtoMember(3)]
        public int Seconds { get; set; }
        [ProtoMember(4)]
        public Directions Direction { get; set; }
        public Coordinate(int degrees, int minutes, int seconds, Directions direction)
        {
            this.Degrees = degrees;
            this.Minutes = minutes;
            this.Seconds = seconds;
            this.Direction = direction;

        
        }
        public override string ToString()
        {
            string degrees = String.Format("{0:000}", this.Degrees);
            string minutes = String.Format("{0:00}", this.Minutes);
            string seconds = String.Format("{0:00}", this.Seconds);
            return degrees + "°" + minutes + "'" + seconds + "''" + Direction;
        }

        //returns a coordinate as decimal
        public double toDecimal()
        {
            double dec = Convert.ToInt64(this.Degrees) + ((double)this.Minutes) / 60.0 + ((double)this.Seconds) / 3600.0;
            if (this.Direction == Directions.S || this.Direction == Directions.W)
                dec *= -1;
            return dec;

        }
        //returns a decimal latitude coordiate as a coordinate
        public static Coordinate LatitudeToCoordinate(double coordinate)
        {
            Directions direction;
            if (coordinate < 0)
            {
                direction = Directions.S;
                coordinate = coordinate * -1;
            }
            else
                direction = Directions.N;


            int degrees = Convert.ToInt32(Math.Floor(coordinate));
            double dMinutes = (coordinate - degrees) * 60;
            int minutes = (Convert.ToInt32(Math.Floor(dMinutes)));
            int seconds = Convert.ToInt32(Math.Floor((dMinutes - minutes) * 60));

            return new Coordinate(degrees, minutes, seconds, direction);
        }
        //returns a decimal longitude coordiate as a coordinate
        public static Coordinate LongitudeToCoordinate(double coordinate)
        {
            Directions direction;
            if (coordinate < 0)
            {
                direction = Directions.W;
                coordinate = coordinate * -1;
            }
            else
                direction = Directions.E;


            int degrees = Convert.ToInt32(Math.Floor(coordinate));
            double dMinutes = (coordinate - degrees) * 60;
            int minutes = (Convert.ToInt32(Math.Floor(dMinutes)));
            int seconds = Convert.ToInt32(Math.Floor((dMinutes - minutes) * 60));

            return new Coordinate(degrees, minutes, seconds, direction);
        }
        //parse a string to a coordinate
        public static Coordinate Parse(string coordinate)
        {
            string[] split = coordinate.Split(new string[] { "°", "'", "''" }, StringSplitOptions.RemoveEmptyEntries);
            int degrees = Int32.Parse(split[0]);
            int minutes = Int32.Parse(split[1]);
            int seconds = Int32.Parse(split[2]);
            Directions direction = (Directions)Enum.Parse(typeof(Directions), split[3]);
            return new Coordinate(degrees, minutes, seconds, direction);
        }
    }
}
