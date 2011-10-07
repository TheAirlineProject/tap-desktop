using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Globalization;
using TheAirlineV2.Model.GeneralModel;

namespace TheAirlineV2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            //FrameworkElement.LanguageProperty.
            /*
            string cultureInfo;
            if (GameObject.GetInstance().Language == null)
                cultureInfo = "da";
            else
                cultureInfo = GameObject.GetInstance().Language.CultureInfo;
            
           

            FrameworkElement.LanguageProperty.OverrideMetadata(

                typeof(FrameworkElement),

                new FrameworkPropertyMetadata(

                    XmlLanguage.GetLanguage(new CultureInfo(cultureInfo).IetfLanguageTag)));//(CultureInfo.CurrentCulture.IetfLanguageTag)));
            */
        }
    }
}
