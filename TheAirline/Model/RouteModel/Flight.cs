﻿using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;

    /*! Flight.
  * This is used for a actually flight.
  * The class needs parameter for the time table entry which the flight flights after
  */

    [Serializable]
    public class Flight : ISerializable
    {
        #region Constructors and Destructors

        public Flight(RouteTimeTableEntry entry)
        {
            this.Entry = entry;

            if (this.Entry.TimeTable.Route.Type == Route.RouteType.Passenger
                || this.Entry.TimeTable.Route.Type == Route.RouteType.Mixed || this.Entry.TimeTable.Route.Type == Route.RouteType.Helicopter)
            {
                this.Classes = new List<FlightAirlinerClass>();

                if (this.Entry != null)
                {
                    this.Airliner = this.Entry.Airliner;
                    this.FlightTime = MathHelpers.ConvertEntryToDate(this.Entry);
                    this.ScheduledFlightTime = this.FlightTime;
                }

                this.IsOnTime = true;
            }
            if (this.Entry.TimeTable.Route.Type == Route.RouteType.Cargo
                || this.Entry.TimeTable.Route.Type == Route.RouteType.Mixed)
            {
                if (this.Entry != null)
                {
                    this.Airliner = this.Entry.Airliner;
                    this.FlightTime = MathHelpers.ConvertEntryToDate(this.Entry);
                    this.ScheduledFlightTime = this.FlightTime;
                }

                this.IsOnTime = true;
            }

            this.DistanceToDestination =
                MathHelpers.GetDistance(
                    this.Entry.Destination.Airport.Profile.Coordinates.ConvertToGeoCoordinate(),
                    this.Entry.DepartureAirport.Profile.Coordinates.ConvertToGeoCoordinate());
        }

        protected Flight(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            IEnumerable<FieldInfo> fields =
                this.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    this.GetType()
                        .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            IEnumerable<MemberInfo> propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop =
                    propsAndFields.FirstOrDefault(
                        p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);

                if (prop != null)
                {
                    if (prop is FieldInfo)
                    {
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    }
                    else
                    {
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                    }
                }
            }

            IEnumerable<MemberInfo> notSetProps =
                propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                var ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                    {
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    }
                    else
                    {
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);
                    }
                }
            }
        }

        #endregion

        #region Public Properties

        [Versioning("airliner")]
        public FleetAirliner Airliner { get; set; }

        [Versioning("cargo")]
        public double Cargo { get; set; }

        [Versioning("classes")]
        public List<FlightAirlinerClass> Classes { get; set; }

        [Versioning("distance")]
        public double DistanceToDestination { get; set; }

        [Versioning("entry")]
        public RouteTimeTableEntry Entry { get; set; }

        public DateTime ExpectedLanding
        {
            get
            {
                return this.getExpectedLandingTime();
            }
            set
            {
                ;
            }
        }

        [Versioning("flighttime")]
        public DateTime FlightTime { get; set; }

        [Versioning("isontime")]
        public Boolean IsOnTime { get; set; }

        [Versioning("scheduled")]
        public DateTime ScheduledFlightTime { get; set; }

        #endregion

        #region Public Methods and Operators

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();

            IEnumerable<FieldInfo> fields =
                myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            IEnumerable<MemberInfo> propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                {
                    propValue = ((FieldInfo)member).GetValue(this);
                }
                else
                {
                    propValue = ((PropertyInfo)member).GetValue(this, null);
                }

                var att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }
        }

        //adds some delay minutes to the flight
        public virtual void addDelayMinutes(int minutes)
        {
            this.FlightTime = this.FlightTime.AddMinutes(minutes);
        }

        //returns the scheduled expected landing time

        //returns the cargo price
        public double getCargoPrice()
        {
            if (this.Entry.TimeTable.Route is CargoRoute)
                return ((CargoRoute)this.Entry.TimeTable.Route).PricePerUnit;
            else
                return ((CombiRoute)this.Entry.TimeTable.Route).PricePerUnit;
        }

        public Airport getDepartureAirport()
        {
            return this.getNextDestination();
        }

        public DateTime getExpectedLandingTime()
        {
            return
                this.FlightTime.Add(
                    MathHelpers.GetFlightTime(
                        this.Entry.DepartureAirport.Profile.Coordinates.ConvertToGeoCoordinate(),
                        this.Entry.Destination.Airport.Profile.Coordinates.ConvertToGeoCoordinate(),
                        this.Airliner.Airliner.Type));
        }

        //returns the total number of passengers

        //returns the flight airliner class for a specific class type
        public FlightAirlinerClass getFlightAirlinerClass(AirlinerClass.ClassType type)
        {
            return this.Classes.Find(c => c.AirlinerClass.Type == type);
        }

        //returns the next destination
        public Airport getNextDestination()
        {
            return this.Entry.Destination.Airport == this.Entry.TimeTable.Route.Destination1
                ? this.Entry.TimeTable.Route.Destination2
                : this.Entry.TimeTable.Route.Destination1;
        }

        public DateTime getScheduledLandingTime()
        {
            return
                this.ScheduledFlightTime.Add(
                    MathHelpers.GetFlightTime(
                        this.Entry.DepartureAirport.Profile.Coordinates.ConvertToGeoCoordinate(),
                        this.Entry.Destination.Airport.Profile.Coordinates.ConvertToGeoCoordinate(),
                        this.Airliner.Airliner.Type));
        }

        public int getTotalPassengers()
        {
            int passengers = 0;

            foreach (FlightAirlinerClass aClass in this.Classes)
            {
                passengers += aClass.Passengers;
            }

            if (passengers < 0)
            {
                passengers = 0;
            }

            return passengers;
        }

        //returns the departure location

        //returns if it is a cargo flight
        public Boolean isCargoFlight()
        {
            return this.Entry.TimeTable.Route.Type == Route.RouteType.Mixed
                   || this.Entry.TimeTable.Route.Type == Route.RouteType.Cargo;
        }

        public Boolean isPassengerFlight()
        {
            return this.Entry.TimeTable.Route.Type == Route.RouteType.Mixed
                   || this.Entry.TimeTable.Route.Type == Route.RouteType.Passenger
                   || this.Entry.TimeTable.Route.Type == Route.RouteType.Helicopter;
        }

        #endregion
    }
}