
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    
    /*! Route airliner class for passengers.
    * This class is used for an airliner class onboard of a route airliner for passengers
    * The class needs parameters for type of class and the fare price
    */
       [Serializable]
     public class RouteAirlinerClass
    {
        // chs, 2011-18-10 added seating type to a route airliner class
        public enum SeatingType { Reserved_Seating, Free_Seating }
        
        public SeatingType Seating { get; set; }
        
        public double FarePrice { get; set; }

        public List<RouteFacility> Facilities { get; set; }
        
        public AirlinerClass.ClassType Type { get; set; }
        //public int CabinCrew { get; set; }
        
        public RouteAirlinerClass(AirlinerClass.ClassType type,SeatingType seating, double fareprice)
        {
            this.Facilities = new List<RouteFacility>();
            this.FarePrice =  fareprice;
            this.Seating =  seating;
            this.Type = type;
        }
        //adds a facility to the route class
        public void addFacility(RouteFacility facility)
        {
            if (facility != null)
            {
                if (this.Facilities.Exists(f => f.Type == facility.Type))
                    this.Facilities.RemoveAll(f => f.Type == facility.Type);

                this.Facilities.Add(facility);
            }
        }
        //returns the facility for a type for the route class
        public RouteFacility getFacility(RouteFacility.FacilityType type)
        {
            return this.Facilities.Find(f => f.Type == type);
        }
        //returns all facilities
        public List<RouteFacility> getFacilities()
        {
            return this.Facilities;
        }

    }
  
}
