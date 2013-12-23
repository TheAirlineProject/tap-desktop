using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel.InvoicesModel;
using TheAirline.Model.GeneralModel.WeatherModel;
using TheAirline.Model.GeneralModel.Helpers;
using System.Runtime.Serialization;
using System.Reflection;


namespace TheAirline.Model.AirlinerModel.RouteModel
{
  [Serializable]
    //the class for a passenger route
    public class PassengerRoute : Route
    {
        //public FleetAirliner Airliner { get; set; }
      [Versioning("classes")]
        public List<RouteAirlinerClass> Classes { get; set; }
        public double IncomePerPassenger { get { return getIncomePerPassenger(); } set { ;} }
        public PassengerRoute(string id, Airport destination1, Airport destination2,DateTime startDate, double farePrice) : base(RouteType.Passenger,id,destination1,destination2,startDate)
        {
           

            this.Classes = new List<RouteAirlinerClass>();

            foreach (AirlinerClass.ClassType ctype in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                RouteAirlinerClass cl = new RouteAirlinerClass(ctype, RouteAirlinerClass.SeatingType.Reserved_Seating, farePrice);

                this.Classes.Add(cl);
            }




        }
       
        //adds a route airliner class to the route
        public void addRouteAirlinerClass(RouteAirlinerClass aClass)
        {
            this.Classes.Add(aClass);
        }
        //returns the route airliner class for a specific class type
        public RouteAirlinerClass getRouteAirlinerClass(AirlinerClass.ClassType type)
        {

            return this.Classes.Find(cl => cl.Type == type);

        }
        //returns the total number of cabin crew for the route based on airliner
        public int getTotalCabinCrew()
        {
            int cabinCrew = 0;

            if (getAirliners().Count > 0)
                foreach (FleetAirliner fa in getAirliners())
                {
                    if (fa.Airliner.Type.TypeAirliner != AirlinerType.TypeOfAirliner.Passenger)
                    {

                    }

                    else
                    {
                        if (((AirlinerPassengerType)fa.Airliner.Type).CabinCrew > cabinCrew)
                        {
                            cabinCrew = ((AirlinerPassengerType)fa.Airliner.Type).CabinCrew;
                        }
                    }
                }

            return cabinCrew;
        }
      
        //get the degree of filling
        public override double getFillingDegree()
        {
            if (this.HasStopovers)
            {
                double fillingDegree = 0;

                var legs = this.Stopovers.SelectMany(s => s.Legs);
                foreach (PassengerRoute leg in legs)
                {
                    fillingDegree += leg.getFillingDegree();
                }
                return fillingDegree / legs.Count();

            }
            else
            {
                double passengers = Convert.ToDouble(this.Statistics.getTotalValue(StatisticsTypes.GetStatisticsType("Passengers")));

                double passengerCapacity = Convert.ToDouble(this.Statistics.getTotalValue(StatisticsTypes.GetStatisticsType("Capacity")));

                return passengers / passengerCapacity;
            }
        }
        //gets the income per passenger
        private double getIncomePerPassenger()
        {
            double totalPassengers = Convert.ToDouble(this.Statistics.getTotalValue(StatisticsTypes.GetStatisticsType("Passengers")));

            return base.Balance / totalPassengers;
        }
        
        //returns the service level for a specific class
        public double getServiceLevel(AirlinerClass.ClassType type)
        {
            return this.Classes.Find(c => c.Type == type).getFacilities().Sum(f => f.ServiceLevel);
        }
        //returns the price for a specific class
        public double getFarePrice(AirlinerClass.ClassType type)
        {
            return this.Classes.Find(c => c.Type == type).FarePrice;
          
        }
       private PassengerRoute(SerializationInfo info, StreamingContext ctxt) : base(info,ctxt)
        {
            int version = info.GetInt16("version");

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop = propsAndFields.FirstOrDefault(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);


                if (prop != null)
                {
                    if (prop is FieldInfo)
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    else
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                }
            }

            var notSetProps = propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                Versioning ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    else
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);

                }

            }
        }

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();

            var fields = myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                    propValue = ((FieldInfo)member).GetValue(this);
                else
                    propValue = ((PropertyInfo)member).GetValue(this, null);

                Versioning att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }


            base.GetObjectData(info, context);
        }
       
    }

}
