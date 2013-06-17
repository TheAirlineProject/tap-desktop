
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.PilotModel
{
    //the class for the aircraft for training for students
    [Serializable]
    public class TrainingAircraftType
    {
        
        public string Name { get; set; }
        
        private double APrice;
        public double Price { get{return GeneralHelpers.GetInflationPrice(this.APrice);} private set { ;} }
        
        public int MaxNumberOfStudents { get; set; }
        public TrainingAircraftType(string name, double price, int maxnumberofstudents)
        {
            this.Name = name;
            this.APrice = price;
            this.MaxNumberOfStudents = maxnumberofstudents;
        }
    }
    //the list of training aircrafts types
    public class TrainingAircraftTypes
    {
        private static List<TrainingAircraftType> types= new List<TrainingAircraftType>();
        //adds a type to the list
        public static void AddAircraftType(TrainingAircraftType type)
        {
            types.Add(type);
        }
        //returns all aircraft types
        public static List<TrainingAircraftType> GetAircraftTypes()
        {
            return types;
        }
        //returns an aircraft type
        public static TrainingAircraftType GetAircraftType(string name)
        {
            return types.Find(t => t.Name == name);
        }
        //clears the list of aircrafts 
        public static void Clear()
        {
            types.Clear();
        }
       
    }
}
