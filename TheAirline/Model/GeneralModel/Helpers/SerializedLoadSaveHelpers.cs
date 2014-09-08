namespace TheAirline.Model.GeneralModel.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading.Tasks;

    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel.Helpers.WorkersModel;

    //the helper class for loading and saving using serialization
    public class SerializedLoadSaveHelpers
    {
        //deletes a saved game

        #region Public Methods and Operators

        public static void DeleteSavedGame(string name)
        {
            string fileName = AppSettings.getCommonApplicationDataPath() + "\\saves\\" + name + ".sav";

            File.Delete(fileName);
        }

        //returns if a saved game exits

        //loads a game 
        public static void LoadGame(string file)
        {
            string fileName = AppSettings.getCommonApplicationDataPath() + "\\saves\\" + file + ".sav";

            var deserializedSaveObject = FileSerializer.Deserialize<SaveObject>(fileName);

            /*
          
           DataContractSerializer serializer = new DataContractSerializer(typeof(SaveObject));
           SaveObject deserializedSaveObject;
           string loading;
           int version;

           using (FileStream stream = new FileStream(fileName, FileMode.Open))
           {
               using (DeflateStream decompress = new DeflateStream(stream, CompressionMode.Decompress))
               {
                   deserializedSaveObject = (SaveObject)serializer.ReadObject(decompress);
               }
           }

           loading = deserializedSaveObject.savetype;
           version = deserializedSaveObject.saveversionnumber;
           */
            //Parrarel for loading the game
            Parallel.Invoke(
                () =>
                {
                    Airlines.Clear();

                    foreach (Airline airline in deserializedSaveObject.airlinesList)
                    {
                        Airlines.AddAirline(airline);
                    }
                },
                () =>
                {
                    Setup.LoadAirports();
                    
                    List<Airport> airports = Airports.GetAllAirports();

                    Airports.Clear();

                    foreach (Airport airport in deserializedSaveObject.airportsList)
                    {
                        airport.Statics = new AirportStatics(airport);
                        Airports.AddAirport(airport);
                    }

                    foreach (string iata in deserializedSaveObject.airportsfromstringList)
                    {
                        Airport airport = airports.FirstOrDefault(a => a.Profile.IATACode == iata);

                        if (airport != null)
                        {
                            Airports.AddAirport(airport);
                        }
                    }
                },
                () =>
                {
                    Airliners.Clear();

                    foreach (Airliner airliner in deserializedSaveObject.airlinersList)
                    {
                        Airliners.AddAirliner(airliner);
                    }
                },
                () =>
                {
                    CalendarItems.Clear();

                    foreach (CalendarItem item in deserializedSaveObject.calendaritemsList)
                    {
                        CalendarItems.AddCalendarItem(item);
                    }
                },
                () =>
                {
                    Configurations.Clear();

                    foreach (Configuration configuration in deserializedSaveObject.configurationList)
                    {
                        Configurations.AddConfiguration(configuration);
                    }
                },
                () =>
                {
                    RandomEvents.Clear();

                    foreach (RandomEvent e in deserializedSaveObject.eventsList)
                    {
                        RandomEvents.AddEvent(e);
                    }
                },
                () =>
                {
                    Alliances.Clear();

                    foreach (Alliance alliance in deserializedSaveObject.allianceList)
                    {
                        Alliances.AddAlliance(alliance);
                    }
                },
                () =>
                {
                    AirportFacilities.Clear();

                    foreach (AirportFacility facility in deserializedSaveObject.Airportfacilitieslist)
                    {
                        AirportFacilities.AddFacility(facility);
                    }
                },
                () =>
                {
                    FeeTypes.Clear();

                    foreach (FeeType type in deserializedSaveObject.feeTypeslist)
                    {
                        FeeTypes.AddType(type);
                    }
                },
                () =>
                {
                    AdvertisementTypes.Clear();

                    foreach (AdvertisementType addtype in deserializedSaveObject.advertisementTypeslist)
                    {
                        AdvertisementTypes.AddAdvertisementType(addtype);
                    }
                },
                () =>
                {
                    AirlinerFacilities.Clear();

                    foreach (AirlinerFacility airlinerfas in deserializedSaveObject.airlinerfacilitieslist)
                    {
                        AirlinerFacilities.AddFacility(airlinerfas);
                    }
                },
                () =>
                {
                    RouteFacilities.Clear();

                    foreach (RouteFacility routefas in deserializedSaveObject.routefacilitieslist)
                    {
                        RouteFacilities.AddFacility(routefas);
                    }
                },
                () =>
                {
                    GameObject.SetInstance(deserializedSaveObject.instance);
                    Settings.SetInstance(deserializedSaveObject.settings);
                },
                () =>
                {
                    if (deserializedSaveObject.airlinefacilitieslist != null)
                    {
                        AirlineFacilities.Clear();

                        foreach (AirlineFacility airlinefac in deserializedSaveObject.airlinefacilitieslist)
                        {
                            AirlineFacilities.AddFacility(airlinefac);
                        }
                    }
                }); //close parallel.invoke

            //for 0.3.9.2 and the issue with no saved facilities on a route classes
            IEnumerable<RouteClassConfiguration> emptyRouteClassesFacilities =
                Configurations.GetConfigurations(Configuration.ConfigurationType.Routeclasses)
                    .SelectMany(c => ((RouteClassesConfiguration)c).Classes.Where(cl => cl.Facilities == null));

            foreach (RouteClassConfiguration rClassConfiguration in emptyRouteClassesFacilities)
            {
                rClassConfiguration.Facilities = new List<RouteFacility>();
            }

        
            Setup.SetupLoadedGame();
            //Maybe this helps? But i doubt this is the best way
            Action action = () =>
            {
                var swPax = new Stopwatch();
                swPax.Start();

                PassengerHelpers.CreateDestinationDemand();

                //Console.WriteLine("Demand have been created in {0} ms.", swPax.ElapsedMilliseconds);
                swPax.Stop();

              };

           
            //create some pilots for the game
            int pilotsPool = 100 * Airlines.GetAllAirlines().Count;
            GeneralHelpers.CreatePilots(pilotsPool);
            int instructorsPool = 75 * Airlines.GetAllAirlines().Count;
            GeneralHelpers.CreateInstructors(instructorsPool);

            //creates some airliners for the game
            AirlinerHelpers.CreateStartUpAirliners();

            //Start the game paused
            GameObjectWorker.GetInstance().startPaused();

            //Task is needed this unlocks the game agian.
            Task.Factory.StartNew(action);
        }

        public static void SaveGame(string name)
        {
            var sw = new Stopwatch();
            sw.Start();

            var so = new SaveObject();
            Parallel.Invoke(
                () =>
                {
                    so.airportsList = new List<Airport>();
                    so.airportsfromstringList = new List<string>();

                    IEnumerable<Airport> airportsInUse =
                        Airports.GetAllAirports()
                            .Where(
                                a =>
                                    Airlines.GetAllAirlines().Exists(al => al.Airports.Contains(a))
                                    || a.hasAirlineFacility());
                    so.airportsList.AddRange(airportsInUse);

                    foreach (Airport airport in Airports.GetAirports(a => !airportsInUse.Contains(a)))
                    {
                        so.airportsfromstringList.Add(airport.Profile.IATACode);
                    }
                },
                () =>
                {
                    so.airlinesList = new List<Airline>();
                    so.airlinesList.AddRange(Airlines.GetAllAirlines());
                },
                () =>
                {
                    so.airlinersList = new List<Airliner>();
                    so.airlinersList.AddRange(Airliners.GetAllAirliners().Where(a => a.Airline != null));
                },
                () =>
                {
                    so.calendaritemsList = new List<CalendarItem>();
                    so.calendaritemsList.AddRange(CalendarItems.GetCalendarItems());
                },
                () =>
                {
                    so.configurationList = new List<Configuration>();
                    so.configurationList.AddRange(Configurations.GetConfigurations());
                },
                () =>
                {
                    so.eventsList = new List<RandomEvent>();
                    so.eventsList.AddRange(RandomEvents.GetEvents());
                },
                () =>
                {
                    so.allianceList = new List<Alliance>();
                    so.allianceList.AddRange(Alliances.GetAlliances());
                },
                () =>
                {
                    so.Airportfacilitieslist = new List<AirportFacility>();
                    so.Airportfacilitieslist.AddRange(AirportFacilities.GetFacilities());
                },
                () =>
                {
                    so.feeTypeslist = new List<FeeType>();
                    so.feeTypeslist.AddRange(FeeTypes.GetTypes());
                },
                () =>
                {
                    so.advertisementTypeslist = new List<AdvertisementType>();
                    so.advertisementTypeslist.AddRange(AdvertisementTypes.GetTypes());
                },
                () =>
                {
                    so.airlinerfacilitieslist = new List<AirlinerFacility>();
                    so.airlinerfacilitieslist.AddRange(AirlinerFacilities.GetAllFacilities());
                },
                () =>
                {
                    so.routefacilitieslist = new List<RouteFacility>();
                    so.routefacilitieslist.AddRange(RouteFacilities.GetAllFacilities());
                },
                () =>
                {
                    so.instance = GameObject.GetInstance();
                    so.settings = Settings.GetInstance();
                },
                () =>
                {
                    so.airlinefacilitieslist = new List<AirlineFacility>();
                    so.airlinefacilitieslist.AddRange(AirlineFacilities.GetFacilities());
                });

            string fileName = AppSettings.getCommonApplicationDataPath() + "\\saves\\" + name + ".sav";

            FileSerializer.Serialize(fileName, so);

            sw.Stop();
            Console.WriteLine("Saving: {0} ms", sw.ElapsedMilliseconds);

            //Clearing stats because there is no need for saving those.
            if (name != "autosave")
            {
                Airports.GetAllAirports().ForEach(a => a.clearDestinationPassengerStatistics());
                Airports.GetAllAirports().ForEach(a => a.clearDestinationCargoStatistics());
                AirlineHelpers.ClearRoutesStatistics();
                AirlineHelpers.ClearAirlinesStatistics();
                AirportHelpers.ClearAirportStatistics();
            }
            /*
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
            */
            /*
            DataContractSerializer serializer = new DataContractSerializer(typeof(SaveObject), null, Int32.MaxValue, false, true, null);

            using (Stream stream = new FileStream(fileName, FileMode.Create))
            {
                using (DeflateStream compress = new DeflateStream(stream, CompressionLevel.Fastest))
                {
                    serializer.WriteObject(compress, so);
                }
            }
          */
        }

        public static Boolean SaveGameExists(string name)
        {
            string fileName = AppSettings.getCommonApplicationDataPath() + "\\saves\\" + name + ".sav";

            return File.Exists(fileName);
        }

        #endregion
    }

    [Serializable]
    public class SaveObject
    {
        #region Public Properties

        [Versioning("airportfacilities")]
        public List<AirportFacility> Airportfacilitieslist { get; set; }

        [Versioning("advertisementtypes")]
        public List<AdvertisementType> advertisementTypeslist { get; set; }

        [Versioning("airlinefacilities")]
        public List<AirlineFacility> airlinefacilitieslist { get; set; }

        [Versioning("airlinerfacilities")]
        public List<AirlinerFacility> airlinerfacilitieslist { get; set; }

        [Versioning("airliners")]
        public List<Airliner> airlinersList { get; set; }

        [Versioning("airlines")]
        public List<Airline> airlinesList { set; get; }

        [Versioning("airports")]
        public List<Airport> airportsList { set; get; }

        [Versioning("airportsfromstrings")]
        public List<string> airportsfromstringList { get; set; }

        [Versioning("alliances")]
        public List<Alliance> allianceList { get; set; }

        [Versioning("calendaritems")]
        public List<CalendarItem> calendaritemsList { get; set; }

        [Versioning("configurations")]
        public List<Configuration> configurationList { get; set; }

        [Versioning("events")]
        public List<RandomEvent> eventsList { get; set; }

        [Versioning("feetypes")]
        public List<FeeType> feeTypeslist { get; set; }

        [Versioning("instance")]
        public GameObject instance { get; set; }

        [Versioning("routefacilities")]
        public List<RouteFacility> routefacilitieslist { get; set; }

        public string savetype { get; set; }

        public int saveversionnumber { get; set; }

        [Versioning("settings")]
        public Settings settings { get; set; }

        #endregion
    }

    public static class FileSerializer
    {
        #region Public Methods and Operators

        public static T Deserialize<T>(string filename)
        {
            T objectToSerialize = default(T);

            Stream stream = null;

            try
            {
                stream = File.Open(filename, FileMode.Open);

                var bFormatter = new BinaryFormatter();

                objectToSerialize = (T)bFormatter.Deserialize(stream);
            }

            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }

            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            return objectToSerialize;
        }

        public static void Serialize(string filename, object objectToSerialize)
        {
            if (objectToSerialize == null)
            {
                throw new ArgumentNullException("objectToSerialize cannot be null");
            }

            Stream stream = null;

            try
            {
                stream = File.Open(filename, FileMode.Create);

                var bFormatter = new BinaryFormatter();

                bFormatter.Serialize(stream, objectToSerialize);

                /*
                 var serializer = new DataContractSerializer(objectToSerialize.GetType(), null, 
        0x7FFF /*maxItemsInObjectGraph, */
                // false /*ignoreExtensionDataObject*/, 
                // true /*preserveObjectReferences : this is where the magic happens */, 
                // null /*dataContractSurrogate*/);
                //serializer.WriteObject(stream, objectToSerialize);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        #endregion
    }
}