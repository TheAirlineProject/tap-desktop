using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.PilotModel
{
    //the class for the aircraft for training for students
    public class TrainingAircraftType
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public TrainingAircraftType(string name, double price)
        {
            this.Name = name;
            this.Price = price;
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
        //clears the list of aircrafts 
        public static void Clear()
        {
            types.Clear();
        }
    }
}
