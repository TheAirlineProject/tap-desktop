using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.GraphicsModel.PageModel.GeneralModel
{
    //the class for the warnings in the game
    public class Warnings
    {
        private static Dictionary<string,string> warnings = new Dictionary<string,string>();
        //adds a warning to the list
        public static void AddWarning(string title, string text)
        {
            warnings.Add(title, text);
        }
        //clears the list of warnings
        public static void Clear()
        {
            warnings.Clear();
        }
        //returns all warnings
        public static Dictionary<string, string> GetWarnings()
        {
            return warnings;
        }

    }
}
