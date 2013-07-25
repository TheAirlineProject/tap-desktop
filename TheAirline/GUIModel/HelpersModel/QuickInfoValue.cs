using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TheAirline.GUIModel.HelpersModel
{
    public class QuickInfoValue
    {
        public string Name { get; set; }
        public UIElement Value { get; set; }
        public QuickInfoValue()
        {
        }
        public QuickInfoValue(string name, UIElement value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
