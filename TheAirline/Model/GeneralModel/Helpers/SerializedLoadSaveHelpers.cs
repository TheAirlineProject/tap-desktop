
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
using TheAirline.Model.GeneralModel.Helpers.WorkersModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
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

           //Clearing stats because there is no need for saving those.
           if (name != "autosave")
           {
                Airports.GetAllAirports().ForEach(a => a.clearDestinationPassengerStatistics());
                Airports.GetAllAirports().ForEach(a => a.clearDestinationCargoStatistics());
                AirlineHelpers.ClearRoutesStatistics();
                AirlineHelpers.ClearAirlinesStatistics();
                AirportHelpers.ClearAirportStatistics();
           }
            
            SaveObject so = new SaveObject();
            Parallel.Invoke(() =>
            {
                so.airportsList = new List<Airport>();
                so.airportsList.AddRange(Airports.GetAllAirports());
            }, () =>
            {
                so.airlinesList = new List<Airline>();
                so.airlinesList.AddRange(Airlines.GetAllAirlines());
            }, () =>
            {
                so.airlinersList = new List<Airliner>();
                so.airlinersList.AddRange(Airliners.GetAllAirliners());
            }, () =>
            {
                so.calendaritemsList = new List<CalendarItem>();
                so.calendaritemsList.AddRange(CalendarItems.GetCalendarItems());
            }, () =>
            {
                so.configurationList = new List<Configuration>();
                so.configurationList.AddRange(Configurations.GetConfigurations());
            }, () =>
            {
                so.eventsList = new List<RandomEvent>();
                so.eventsList.AddRange(RandomEvents.GetEvents());
            }, () =>
            {
                so.allianceList = new List<Alliance>();
                so.allianceList.AddRange(Alliances.GetAlliances());
            }, () =>
            {
                so.Airportfacilitieslist = new List<AirportFacility>();
                so.Airportfacilitieslist.AddRange(AirportFacilities.GetFacilities());
            }, () =>
            {
                so.feeTypeslist = new List<FeeType>();
                so.feeTypeslist.AddRange(FeeTypes.GetTypes());
            }, () =>
            {
                so.advertisementTypeslist = new List<AdvertisementType>();
                so.advertisementTypeslist.AddRange(AdvertisementTypes.GetTypes());
            }, () =>
            {
                so.airlinerfacilitieslist = new List<AirlinerFacility>();
                so.airlinerfacilitieslist.AddRange(AirlinerFacilities.GetAllFacilities());
            }, () =>
            {
                so.routefacilitieslist = new List<RouteFacility>();
                so.routefacilitieslist.AddRange(RouteFacilities.GetAllFacilities());
            }, () =>
            {
                so.instance = GameObject.GetInstance();
                so.settings = Settings.GetInstance();
                so.savetype = "039";
                so.saveversionnumber = 1;
            });


            DataContractSerializer serializer = new DataContractSerializer(typeof(SaveObject), null, Int32.MaxValue, false, true, null);

            using (Stream stream = new FileStream(fileName, FileMode.Create))
            {
                using (DeflateStream compress = new DeflateStream(stream, CompressionLevel.Fastest))
                {
                    serializer.WriteObject(compress, so);
                }
            }
          
            sw.Stop();

        }
        //loads a game 
        public static void LoadGame(string file)
        {

            string fileName = AppSettings.getCommonApplicationDataPath() + "\\saves\\" + file + ".sav";

            DataContractSerializer serializer = new DataContractSerializer(typeof(SaveObject));
            SaveObject deserializedSaveObject;
            string loading;
            int version;

            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            {
                using (DeflateStream decompress = new DeflateStream(stream, CompressionMode.Decompress))
                {j

                    object o = serializer.ReadObject(decompress);
   
                }
            }

            loading = deserializedSaveObject.savetype;
            version = deserializedSaveObject.saveversionnumber;

            //Parrarel for loading the game
            Parallel.Invoke(() =>
            {
                Airlines.Clear();

                foreach (Airline airline in deserializedSaveObject.airlinesList)
                    Airlines.AddAirline(airline);
            },
            () =>
            {
                Airports.Clear();

                foreach (Airport airport in deserializedSaveObject.airportsList)
                {
                    airport.Statics = new AirportStatics(airport);
                    Airports.AddAirport(airport);
                }
            },
            () =>
            {
                Airliners.Clear();

                foreach (Airliner airliner in deserializedSaveObject.airlinersList)
                    Airliners.AddAirliner(airliner);
            },
            () =>
            {
                CalendarItems.Clear();

                foreach (CalendarItem item in deserializedSaveObject.calendaritemsList)
                    CalendarItems.AddCalendarItem(item);
            },
            () =>
            {
                Configurations.Clear();

                foreach (Configuration configuration in deserializedSaveObject.configurationList)
                    Configurations.AddConfiguration(configuration);
            },
            () =>
            {
                RandomEvents.Clear();

                foreach (RandomEvent e in deserializedSaveObject.eventsList)
                    RandomEvents.AddEvent(e);
            },
            () =>
            {
                Alliances.Clear();

                foreach (Alliance alliance in deserializedSaveObject.allianceList)
                    Alliances.AddAlliance(alliance);
            },
            () =>
            {
                AirportFacilities.Clear();

                foreach (AirportFacility facility in deserializedSaveObject.Airportfacilitieslist)
                    AirportFacilities.AddFacility(facility);
            },
            () =>
            {   //Do this only with new savegames for now
                if (loading == "new" || loading == "039")
                {
                    FeeTypes.Clear();

                    foreach (FeeType type in deserializedSaveObject.feeTypeslist)
                        FeeTypes.AddType(type);
                }
            },
            () =>
            {   //Do this only with new savegames for now
                if (loading == "new" || loading == "039")
                {
                    AdvertisementTypes.Clear();

                    foreach (AdvertisementType addtype in deserializedSaveObject.advertisementTypeslist)
                        AdvertisementTypes.AddAdvertisementType(addtype);
                }
            },
            () =>
            {   //Do this only with new savegames for now
                if (loading == "039")
                {
                    
                    AirlinerFacilities.Clear();

                    foreach (AirlinerFacility airlinerfas in deserializedSaveObject.airlinerfacilitieslist)
                        AirlinerFacilities.AddFacility(airlinerfas);
                }
            },
             () =>
             {   //Do this only with new savegames for now
                 if (loading == "new" || loading == "039")
                 {
                     RouteFacilities.Clear();

                     foreach (RouteFacility routefas in deserializedSaveObject.routefacilitieslist)
                         RouteFacilities.AddFacility(routefas);
                 }
             },
            () =>
            {
                if (loading == "039") { GameObject.SetInstance(deserializedSaveObject.instance); }
                Settings.SetInstance(deserializedSaveObject.settings);
            }); //close parallel.invoke

            //Maybe this helps? But i doubt this is the best way
            Action action = () =>
            {
                Stopwatch swPax = new Stopwatch();
                swPax.Start();

                PassengerHelpers.CreateDestinationDemand();

                //Console.WriteLine("Demand have been created in {0} ms.", swPax.ElapsedMilliseconds);
                swPax.Stop();
            };

            //Create some pilots for the game
            int pilotsPool = 100 * Airlines.GetAllAirlines().Count;
            GeneralHelpers.CreatePilots(pilotsPool);
            int instructorsPool = 75 * Airlines.GetAllAirlines().Count;
            GeneralHelpers.CreateInstructors(instructorsPool);

            //Start the game paused
            GameObjectWorker.GetInstance().startPaused();

            //Task is needed this unlocks the game agian.
            Task.Factory.StartNew(action);

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
        public Settings settings { get; set; }

        [DataMember]
        public string savetype { get; set; }

        [DataMember]
        public int saveversionnumber { get; set; }

        [DataMember]
        public List<Configuration> configurationList { get; set; }

        [DataMember]
        public List<RandomEvent> eventsList { get; set; }

        [DataMember]
        public List<Alliance> allianceList { get; set; }

        [DataMember]
        public List<AirportFacility> Airportfacilitieslist { get; set; }

        [DataMember]
        public List<FeeType> feeTypeslist { get; set; }

        [DataMember]
        public List<AdvertisementType> advertisementTypeslist { get; set; }

        [DataMember]
        public List<AirlinerFacility> airlinerfacilitieslist { get; set; }

        [DataMember]
        public List<RouteFacility> routefacilitieslist { get; set; }
    }
}