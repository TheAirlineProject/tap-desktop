using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlinerModel;
using System.Data.SQLite;
using System.Data;

namespace TheAirline.Model.PassengerModel
{
    //the class for a passenger
    public class Passenger
    {
        public enum PassengerType { Business, Tourist, Other }
        public PassengerType CurrentType { get; set; }
        public PassengerType PrimaryType { get; set; }
        public string ID { get; set; }
        public Airport Destination { get; set; }
        public Airport HomeAirport { get; set; }
        public Airport CurrentAirport { get; set; }
        public DateTime Updated { get; set; }
        public AirlinerClass.ClassType PreferedClass { get; set; }
        public List<Airport> Route { get; set; }
        public int Factor { get; set; } //how many passengers does the passenger count for
        public Passenger(string id, PassengerType type, Airport homeAirport, AirlinerClass.ClassType preferedClass)
        {
            this.ID = id;
            this.CurrentType = type;
            this.PrimaryType = type;
            this.HomeAirport = homeAirport;
            this.CurrentAirport = homeAirport;
            this.PreferedClass = preferedClass;
       
        }
        //returns the next destination on the route for the passenger
        public Airport getNextRouteDestination(Airport currentDestination)
        {
            int index = this.Route.IndexOf(currentDestination);

            if (index >= 0 && index < this.Route.Count - 1)
                return this.Route[index + 1];
            else
                return null;
        }
    }
    //the list of passengers 
    public class Passengers
    {
        /*
        private static SQLiteConnection conn = new SQLiteConnection(@"Data Source=C:\bbm\theairline.s3db");  
        //adds a passenger to the database
        public static void AddPassenger(Passenger passenger)
        {
            string sqlQuery = "insert into  passengers (ID, currenttype, primarytype, homeairport, currentairport, preferedclass) values ('" + passenger.ID +", " + passenger.CurrentType.ToString()  + ", " + passenger.PrimaryType.ToString() + ", " + passenger.HomeAirport.Profile.IATACode + ", " + passenger.CurrentAirport.Profile.IATACode + ", " + passenger.PreferedClass.ToString() + "')";
            ExecuteQuery(sqlQuery);  
        }
        //clears the database of passengers
        public static void Clear()
        {
            string sqlQuery = "truncate table passengers";
            ExecuteQuery(sqlQuery);
        }
        //executes the query
        private static void ExecuteQuery(string txtQuery)
        {
            conn.Open();
            SQLiteCommand sql_cmd = conn.CreateCommand();
            sql_cmd.CommandText = txtQuery;
            sql_cmd.ExecuteNonQuery();
            conn.Close();
        }
        //counts the number of passengers
        public static int Count()
        {
            int rows = 0;

            string sqlQuery = "select count(ID) from passengers";
            conn.Open();
            SQLiteCommand sql_cmd = conn.CreateCommand();
            sql_cmd.CommandText = sqlQuery;

            rows = Convert.ToInt32(sql_cmd.ExecuteScalar());

            conn.Close();

            return rows;
        }
        //updates a passenger
        public static void UpdatePassenger(Passenger passenger)
        {
            string route = string.Join(",", from r in passenger.Route select r.Profile.IATACode);
            string sqlQuery = "update passengers set currenttype='" + passenger.CurrentType.ToString() + "', destination='"+ passenger.Destination.Profile.IATACode +"', currentairport='"+ passenger.CurrentAirport.Profile.IATACode + "', updated='" + passenger.Updated.ToLongDateString() +"', factor='" + passenger.Factor +"', route='" +route +  "' where ID='" + passenger.ID + "'";
            ExecuteQuery(sqlQuery);         
        }
        //returns a passenger with an id
        public static Passenger GetPassenger(string id)
        {
            conn.Open();
            string sqlQuery = "select * from passengers where ID='" + id + "'";

            SQLiteCommand sql_cmd = conn.CreateCommand();
            sql_cmd.CommandText = sqlQuery;

            SQLiteDataReader reader = sql_cmd.ExecuteReader();
            conn.Close();

            return GetPassenger(reader);
        }
        //returns a list of passengers 
        public static List<Passenger> GetPassengers(Predicate<Passenger> match)
        {
            
        }
        //returns all passengers 
        public static List<Passenger> GetAllPassengers()
        {
            List<Passenger> passengers = new List<Passenger>();

            conn.Open();

            try
            {
                string sqlQuery = "select * from passengers";

                SQLiteCommand command = new SQLiteCommand(sqlQuery, conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                    passengers.Add(GetPassenger(reader));
            }
            catch
            {
            }
            finally
            {
                conn.Close();
            }
            
            return passengers;
        }
        //reads and returns a passenger from a reader
        private static Passenger GetPassenger(SQLiteDataReader row)
        {
            string id = row["ID"].ToString();
            Passenger.PassengerType primaryType = (Passenger.PassengerType)Enum.Parse(typeof(Passenger.PassengerType), row["primarytype"].ToString());
            Passenger.PassengerType currentType = (Passenger.PassengerType)Enum.Parse(typeof(Passenger.PassengerType), row["currenttype"].ToString());
            Airport homeairport = Airports.GetAirport(row["homeairport"].ToString());
            Airport destination = Airports.GetAirport(row["destination"].ToString());
            Airport currentairport = Airports.GetAirport(row["currentairport"].ToString());
            DateTime updated = Convert.ToDateTime(row["updated"].ToString());
            int factor = Convert.ToInt16(row["factor"].ToString());
            AirlinerClass.ClassType preferedClass = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), row["preferedclass"].ToString());
            List<Airport> route = new List<Airport>();
            row["route"].ToString().Split(':').ToList().ForEach(a => route.Add(Airports.GetAirport(a)));

            Passenger passenger = new Passenger(id, primaryType, homeairport, preferedClass);
            passenger.CurrentAirport = currentairport;
            passenger.CurrentType = currentType;
            passenger.Destination = destination;
            passenger.Factor = factor;
            passenger.Updated = updated;
            passenger.Route = route;

            return passenger;
        }
      
        */
        private static Dictionary<string, Passenger> passengers = new Dictionary<string, Passenger>();
        //adds a passengers to the list
        public static void AddPassenger(Passenger passenger)
        {
           passengers.Add(passenger.ID,passenger);
        }
        //returns all passengers
        public static List<Passenger> GetAllPassengers()
        {
            return passengers.Values.ToList();
        }
        //returns the list of passengers
        public static List<Passenger> GetPassengers(Predicate<Passenger> match)
        {
            return passengers.Values.ToList().FindAll(match);
        }
        //clears the list of passengers
        public static void Clear() 
        {
            passengers.Clear();
        }
        //returns a passenger with an id
        public static Passenger GetPassenger(string id)
        {
            return passengers[id];
        }
        //counts all the pasengers
        public static long Count()
        {
            return passengers.Count;
        }
    
        //returns the max amount of passengers for the game
        public static long GetMaxPassengers()
        {
            return Airports.Count() * 10000;
        }
    }
}
