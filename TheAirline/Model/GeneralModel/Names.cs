using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel.CountryModel;

namespace TheAirline.Model.GeneralModel
{
    //the class for the names
    public class Names
    {
        #region Static Fields

        private static Names _instance;

        #endregion

        #region Fields

        private readonly Dictionary<Country, List<string>> _firstNames;

        private readonly Dictionary<Country, List<string>> _lastNames;

        private readonly Random _rnd;

        #endregion

        #region Constructors and Destructors

        private Names()
        {
            _rnd = new Random();

            _firstNames = new Dictionary<Country, List<string>>();
            _lastNames = new Dictionary<Country, List<string>>();

            SetupNames();
        }

        #endregion

        #region Public Methods and Operators

        public static Names GetInstance()
        {
            return _instance ?? (_instance = new Names());
        }

        //returns a random first name from a country
        public string GetRandomFirstName(Country country)
        {
            if (!_firstNames.ContainsKey(country))
            {
                IEnumerable<Country> countries = _firstNames.Select(n => n.Key);
                var enumerable = countries as Country[] ?? countries.ToArray();
                country = enumerable.ElementAt(_rnd.Next(enumerable.Count()));
            }

            return GetRandomElement(_firstNames[country]);
        }

        //returns a random last name
        public string GetRandomLastName(Country country)
        {
            if (!_lastNames.ContainsKey(country))
            {
                IEnumerable<Country> countries = _firstNames.Select(n => n.Key);
                var enumerable = countries as Country[] ?? countries.ToArray();
                country = enumerable.ElementAt(_rnd.Next(enumerable.Count()));
            }

            return GetRandomElement(_lastNames[country]);
        }

        #endregion

        #region Methods

        private string GetRandomElement(List<string> list)
        {
            return list[_rnd.Next(list.Count)];
        }

        //setup the names
        private void SetupNames()
        {
            var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\addons\\names");

            foreach (FileInfo file in dir.GetFiles("*.names"))
            {
                var countries = new List<Country>();

                var reader = new StreamReader(file.FullName, Encoding.GetEncoding("iso-8859-1"));

                //first line is the attributes for the file. Format: [TYPE=F(irstnames)||L(astnames)][COUNTRIES=110,111....]
                var readLine = reader.ReadLine();
                if (readLine != null)
                {
                    string[] attributes = readLine.Split(new[] {"["}, StringSplitOptions.RemoveEmptyEntries);
                    Boolean isFirstname = attributes[0].Substring(attributes[0].IndexOf("=", StringComparison.Ordinal) + 1, 1) == "F";

                    string attrCountries = attributes[1].Substring(attributes[1].IndexOf("=", StringComparison.Ordinal) + 1);

                    foreach (string country in attrCountries.Split(','))
                    {
                        countries.Add(Countries.GetCountry(country.Replace(']', ' ').Trim()));
                    }

                    string line;

                    while ((line = readLine) != null)
                    {
                        if (isFirstname)
                        {
                            foreach (Country country in countries)
                            {
                                if (!_firstNames.ContainsKey(country))
                                {
                                    _firstNames.Add(country, new List<string>());
                                }
                                _firstNames[country].Add(line);
                            }
                        }
                        else
                        {
                            foreach (Country country in countries)
                            {
                                if (!_lastNames.ContainsKey(country))
                                {
                                    _lastNames.Add(country, new List<string>());
                                }
                                _lastNames[country].Add(line);
                            }
                        }
                    }
                }

                reader.Close();
            }

            /*
            var reader = new StreamReader(File.OpenRead(AppSettings.getDataPath() + "\\names.csv"));

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                if (values.Length > 1)
                {
                    if (values[0].Replace('\'',' ').Trim().Length > 1)
                        this.FirstNames.Add(values[0]);

                    if (values[1].Replace('\'',' ').Trim().Length > 1)
                        this.LastNames.Add(values[1]);
                }
            }
             * */
        }

        #endregion

        //returns a random element from a list
    }
}