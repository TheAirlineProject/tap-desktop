using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    //the class for the names
    public class Names
    {
        private static Names Instance;
        private Dictionary<Country, List<string>> FirstNames;
        private Dictionary<Country, List<string>> LastNames;
        private Random rnd;
        public static Names GetInstance()
        {
            if (Instance == null)
                Instance = new Names();

            return Instance;
        }
        private Names()
        {
            rnd = new Random();

            this.FirstNames = new Dictionary<Country, List<string>>();
            this.LastNames = new Dictionary<Country, List<string>>();

            setupNames();
        }
        //returns a random first name from a country
        public string getRandomFirstName(Country country)
        {
            if (!this.FirstNames.ContainsKey(country))
            {
                var countries = this.FirstNames.Select(n => n.Key);
                country = countries.ElementAt(rnd.Next(countries.Count()));
            }
         
            return getRandomElement(this.FirstNames[country]);
        }
        //returns a random last name
        public string getRandomLastName(Country country)
        {
            if (!this.LastNames.ContainsKey(country))
            {
                var countries = this.FirstNames.Select(n => n.Key);
                country = countries.ElementAt(rnd.Next(countries.Count()));
            }
       
            return getRandomElement(this.LastNames[country]);
        }
        //returns a random element from a list
        private string getRandomElement(List<string> list)
        {
            return list[rnd.Next(list.Count)];
        }
        //setup the names
        private void setupNames()
        {
            DirectoryInfo dir = new DirectoryInfo(AppSettings.getDataPath() + "\\addons\\names");
            
            foreach (FileInfo file in dir.GetFiles("*.names"))
            {
                List<Country> countries = new List<Country>();

                System.IO.StreamReader reader =
   new System.IO.StreamReader(file.FullName, System.Text.Encoding.GetEncoding("iso-8859-1"));
                
                //first line is the attributes for the file. Format: [TYPE=F(irstnames)||L(astnames)][COUNTRIES=110,111....]
                string[] attributes = reader.ReadLine().Split(new string[]{"["},StringSplitOptions.RemoveEmptyEntries);
                Boolean isFirstname = attributes[0].Substring(attributes[0].IndexOf("=") + 1,1) == "F";

                string attrCountries = attributes[1].Substring(attributes[1].IndexOf("=") + 1);

                foreach (string country in attrCountries.Split(','))
                {
                    countries.Add(Countries.GetCountry(country.Replace(']', ' ').Trim()));
                }

                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstname)
                    {
                        foreach (Country country in countries)
                        {
                            if (!this.FirstNames.ContainsKey(country))
                                this.FirstNames.Add(country, new List<string>());
                            this.FirstNames[country].Add(line);
                        }
                    }
                    else
                    {
                        foreach (Country country in countries)
                        {
                            if (!this.LastNames.ContainsKey(country))
                                this.LastNames.Add(country, new List<string>());
                            this.LastNames[country].Add(line);
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
    }
}
