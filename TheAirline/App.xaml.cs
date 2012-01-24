using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Resources;
using System.Globalization;
using TheAirline.Model.GeneralModel;

/*!
 * /brief Namespace of the project
 */
namespace TheAirline
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            AppSettings.Init();
            
        }
    }
}
