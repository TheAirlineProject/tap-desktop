using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TheAirline.GUIModel.HelpersModel
{
    public class QuickInfoValue : FrameworkElement
    {
        public static readonly DependencyProperty TextProperty =
                            DependencyProperty.Register("Text",
                            typeof(string), typeof(QuickInfoValue));

        [Category("Common Properties")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        //public string Name { get; set; }
        public UIElement Value { get; set; }
        public QuickInfoValue()
        {
               
        }

       
    }
}
