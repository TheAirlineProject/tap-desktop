using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    //the mvvm class for use in the scenario editor
    public class ScenarioEditorMVVM
    {
        public double LastStartYear 
        { 
            get
            {
                return DateTime.Now.Year+1;
            }
            private set { ;} 
        }
        public ObservableCollection<int> StartYears 
        {
            get
            {
                var startYears = new ObservableCollection<int>();

                for (int i = 1960; i < DateTime.Now.Year + 1; i++)
                    startYears.Add(i);

                return startYears;
            }
            private set { ;} 
        }
    }
    //the mvvm object for a scenario
    public class ScenarioMVVM
    {
        public string Name { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
    }
}
