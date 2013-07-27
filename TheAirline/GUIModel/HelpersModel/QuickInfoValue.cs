using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TheAirline.GUIModel.HelpersModel
{
    public class QuickInfoValue : DependencyObject
    {
        public static readonly DependencyProperty NameProperty =
                            DependencyProperty.Register("Name",
                            typeof(string), typeof(QuickInfoValue));


        [Category("Common Properties")]
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        //public string Name { get; set; }
        public UIElement Value { get; set; }
        public QuickInfoValue()
        {
        }
       
    }
}
