using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TheAirline.Model.GeneralModel
{
    /*
    //the generator for generating a name for a fleet airliner
    public class AirlinerNameGenerator
    {
        private static AirlinerNameGenerator Instance;

        private List<string> Names;
        private AirlinerNameGenerator()
        {
         
            createNames();

            shuffleNames();


        }
        //returns the game instance
        public static AirlinerNameGenerator GetInstance()
        {
            if (Instance == null)
                Instance = new AirlinerNameGenerator();
            return Instance;
        }
        //returns the list of current possible names
        public List<string> getNames()
        {
            return this.Names;
        }
        //returns the next name from the list
        public string getNextName()
        {
            Random rnd = new Random();

            int value = rnd.Next(this.Names.Count);

            string name = this.Names[value];

            this.Names.RemoveAt(value);

            return name;
        }
        //removes a name from the list
        public void removeName(string name)
        {
            this.Names.Remove(name);
        }
        //shuffle the list of names
        private void shuffleNames()
        {
            for (int i = 0; i < 1000; i++)
            {
                Random rnd = new Random();

                int value = rnd.Next(this.Names.Count);

                this.Names.Insert(0, this.Names[value]);

                this.Names.RemoveAt(value + 1);
            }
        }
        //creates the list of names
        private void createNames()
        {
            this.Names = new List<string>();

            StreamReader re = new StreamReader(Setup.getDataPath() + "\\fleetairlinernames.txt",Encoding.UTF8);//File.OpenText(dataPath+ "\\fleetairlinernames.txt");
            
            
            string name = null;
            while ((name = re.ReadLine()) != null)
            {
                this.Names.Add(name);
            }
            re.Close();

         


        }
       
    }*
     * */
}
