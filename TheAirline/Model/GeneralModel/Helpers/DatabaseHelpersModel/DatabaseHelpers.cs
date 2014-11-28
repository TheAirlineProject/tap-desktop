using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.Helpers.DatabaseHelpersModel.SQLite;

namespace TheAirline.Model.GeneralModel.Helpers.DatabaseHelpersModel
{
    //the class for the database helpers
    public class DatabaseHelpers
    {
        private static SQLiteConnection db;
        private static string DatabaseName = "c:\\bbm\\test.db";
        private static List<object> objects = new List<object>();
        //creates the database
        public static void SetupDatabase()
        {
            objects = new List<object>();
            db = new SQLiteConnection(DatabaseName,true);

            db.DropTable<AirportDestinationDemand>();
            db.CreateTable<AirportDestinationDemand>();

        }
        //closes the database
        public static void CloseDatabase()
        {
            db.Close();
            
            File.Delete(DatabaseName);
        }
        //returns the connection
        public static SQLiteConnection GetConnection()
        {
            if (db == null)
                SetupDatabase();

            return db;
        }
        
        //insert an object in the database
        public static void AddObject(object o)
        {
            /*
            if (db == null)
                SetupDatabase();
            
            db.Insert(o);
             * */
            objects.Add(o);
        }
        //commites the changes to the db
        public static void CommitToDatabase()
        {
            var os = new List<object>(objects);

            db.InsertAll(os);

          
            
            objects.Clear();

        }
        //updates an object
        public static void UpdateObject(object o)
        {
            db.Update(o);
        }
        //specific methods for the classes
        public static AirportDestinationDemand GetDemand(Airport airport,Airport demand)
        {
            if (db == null)
                SetupDatabase();

            return db.Query<AirportDestinationDemand>("select * from AirportDestinationDemand where destination = ?1 and airport = ?2", demand.Profile.IATACode,airport.Profile.IATACode).FirstOrDefault();
        }
       
    }
}
