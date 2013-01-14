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
        private List<string> FirstNames;
        private List<string> LastNames;
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
            this.FirstNames = new List<string>();
            this.LastNames = new List<string>();

            setupNames();
        }
        //returns a random first name
        public string getRandomFirstName()
        {
            return this.FirstNames[rnd.Next(this.FirstNames.Count)];
        }
        //returns a random last name
        public string getRandomLastName()
        {
            return this.LastNames[rnd.Next(this.LastNames.Count)];
        }
        //setup the names
        private void setupNames()
        {
            var reader = new StreamReader(File.OpenRead(AppSettings.getDataPath() + "\\names.csv"));

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                if (values[0].Trim().Length > 1)
                    this.FirstNames.Add(values[0]);

                if (values[1].Trim().Length > 1)
                    this.LastNames.Add(values[1]);
            }
        }
    }
}
