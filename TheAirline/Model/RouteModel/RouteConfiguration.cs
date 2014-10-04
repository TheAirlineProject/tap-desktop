using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.RouteModel
{
    //the class for the configuration for a route classes
    [Serializable]
    public class RouteClassesConfiguration : Configuration
    {
        #region Constructors and Destructors

        public RouteClassesConfiguration(string name, Boolean standard)
            : base(ConfigurationType.Routeclasses, name, standard)
        {
            Classes = new List<RouteClassConfiguration>();
        }

        //adds a class to the configuration

        private RouteClassesConfiguration(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("classes")]
        public List<RouteClassConfiguration> Classes { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public void AddClass(RouteClassConfiguration routeclass)
        {
            if (Classes.Exists(c => c.Type == routeclass.Type))
            {
                Classes.RemoveAll(c => c.Type == routeclass.Type);
            }
            Classes.Add(routeclass);
        }

        //returns all route classes
        public List<RouteClassConfiguration> GetClasses()
        {
            return Classes;
        }

        #endregion
    }

    //the class for the configuration for a route class
    [Serializable]
    public class RouteClassConfiguration : BaseModel
    {
        #region Constructors and Destructors

        public RouteClassConfiguration(AirlinerClass.ClassType type)
        {
            Type = type;
            Facilities = new List<RouteFacility>();
        }

        //adds a facility to the configuration

        private RouteClassConfiguration(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("facilities")]
        public List<RouteFacility> Facilities { get; set; }

        [Versioning("type")]
        public AirlinerClass.ClassType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public void AddFacility(RouteFacility facility)
        {
            if (Facilities.Exists(f => f.Type == facility.Type))
            {
                Facilities.RemoveAll(f => f.Type == facility.Type);
            }

            Facilities.Add(facility);
        }

        public List<RouteFacility> GetFacilities()
        {
            return Facilities;
        }

        public RouteFacility GetFacility(RouteFacility.FacilityType type)
        {
            return Facilities.Find(f => f.Type == type);
        }

        #endregion
    }
}