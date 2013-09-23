
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the helper class for loading and saving using serialization
    public class SerializedLoadSaveHelpers
    {
        //deletes a saved game
        public static void DeleteSavedGame(string name)
        {
            string fileName = AppSettings.getCommonApplicationDataPath() + "\\saves\\" + name + ".sav";

            File.Delete(fileName);
        }
        //returns if a saved game exits
        public static Boolean SaveGameExists(string name)
        {
            string fileName = AppSettings.getCommonApplicationDataPath() + "\\saves\\" + name + ".sav";

            return File.Exists(fileName);
        }
        //saves a game
        public static void SaveGame(string name)
        {
            string fileName = AppSettings.getCommonApplicationDataPath() + "\\saves\\" + name + ".sav";

            Stopwatch sw = new Stopwatch();
            sw.Start();

            SaveObject so = new SaveObject();
            so.airportsList = new List<Airport>();
            so.airlinesList = new List<Airline>();
            so.airlinersList = new List<Airliner>();
            so.calendaritemsList = new List<CalendarItem>();
            so.configurationList = new List<Configuration>();
            so.eventsList = new List<RandomEvent>();

            so.airlinesList.AddRange(Airlines.GetAllAirlines());
            so.airportsList.AddRange(Airports.GetAllAirports());
            so.airlinersList.AddRange(Airliners.GetAllAirliners());
            so.calendaritemsList.AddRange(CalendarItems.GetCalendarItems());
            so.configurationList.AddRange(Configurations.GetConfigurations());
            so.eventsList.AddRange(RandomEvents.GetEvents());

            so.instance = GameObject.GetInstance();
        
            
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                NewLineChars = Environment.NewLine

            };
     DataContractSerializer serializer = new DataContractSerializer(typeof(SaveObject), null,
                      Int32.MaxValue,
                      false,
                      true,
                      null);
     using (Stream stream = new FileStream(fileName, FileMode.Create))
     {
         using (GZipStream compress = new GZipStream(stream, CompressionMode.Compress))
         {
             serializer.WriteObject(compress, so);

         }
     }
            /*
            using (var buffer = new FileStream(fileName, FileMode.Create))
            {
                var serializer = new DataContractSerializer(typeof(SaveObject), null,
                      Int32.MaxValue,
                      false,
                      true,
                      null); //new XmlSerializer(typeof(SaveObject));

                Stream stream = new GZipStream(buffer, CompressionMode.Compress);

                using (var writer = XmlWriter.Create(stream, settings))
                {
                    serializer.WriteObject(writer, so);

                }
                stream.Close();
            }
              
             * */
            /*
            using (var file = File.Create("c:\\bbm\\person.bin"))
            {
                Serializer.Serialize<SaveObject>(file, so);
            }*/
            sw.Stop();
          
        }
        //loads a game 
        public static void LoadGame(string file)
        {
            string fileName = AppSettings.getCommonApplicationDataPath() + "\\saves\\" + file + ".sav";

            DataContractSerializer serializer = new DataContractSerializer(typeof(SaveObject));
            SaveObject deserializedSaveObject;
            
            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            {
                using (GZipStream decompress = new GZipStream(stream, CompressionMode.Decompress))
                {
                    deserializedSaveObject =
                   (SaveObject)serializer.ReadObject(decompress);
                }
            }
            /*
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(SaveObject));

                Stream stream = new GZipStream(fs, CompressionMode.Decompress);
               
                XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
                quotas.MaxDepth = Int32.MaxValue;

                XmlDictionaryReader reader =
       XmlDictionaryReader.CreateTextReader(stream, quotas);
         
                // Deserialize the data and read it from the instance.
                deserializedSaveObject =
                    (SaveObject)ser.ReadObject(reader, true);
                
                reader.Close();
                stream.Close();
          

            }
            */
         

            Airlines.Clear();

            foreach (Airline airline in deserializedSaveObject.airlinesList)
                Airlines.AddAirline(airline);

            Airports.Clear();

            foreach (Airport airport in deserializedSaveObject.airportsList)
            {
                airport.Statics = new AirportStatics(airport);
                Airports.AddAirport(airport);
            }

            Airliners.Clear();

            foreach (Airliner airliner in deserializedSaveObject.airlinersList)
                Airliners.AddAirliner(airliner);

            CalendarItems.Clear();

            foreach (CalendarItem item in deserializedSaveObject.calendaritemsList)
                CalendarItems.AddCalendarItem(item);

            Configurations.Clear();

            foreach (Configuration configuration in deserializedSaveObject.configurationList)
                Configurations.AddConfiguration(configuration);

            RandomEvents.Clear();

            foreach (RandomEvent e in deserializedSaveObject.eventsList)
                RandomEvents.AddEvent(e);

            GameObject.SetInstance(deserializedSaveObject.instance);
        }
    }
    
    [DataContract(Name = "game")]
    public class SaveObject
    {
        [DataMember]
        public List<CalendarItem> calendaritemsList { get; set; }

        [DataMember]
        public List<Airliner> airlinersList { get; set; }

        [DataMember]
        public List<Airline> airlinesList { set; get; }

        [DataMember]
        public List<Airport> airportsList { set; get; }

        [DataMember]
        public GameObject instance { get; set; }

        [DataMember]
        public List<Configuration> configurationList { get; set; }

        [DataMember]
        public List<RandomEvent> eventsList { get; set; } 
       

    }
}
