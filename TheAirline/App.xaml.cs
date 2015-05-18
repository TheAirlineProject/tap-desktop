using System;
using NLog;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.General;

/*!
 * /brief Namespace of the project
 */

namespace TheAirline
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static App()
        {
            AppSettings.Init();


            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += currentDomain_UnhandledException;
        }

        private static void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //var l_CurrentStack = new System.Diagnostics.StackTrace(true);

            //var file = new StreamWriter(AppSettings.GetCommonApplicationDataPath() + "\\theairline.log");
            //file.WriteLine("{0}: {1} {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), e.ExceptionObject);
            //file.WriteLine("---------GAME INFORMATION----------");
            //file.Write("Gametime: {0}, human airline: {1}", GameObject.GetInstance().GameTime.ToShortDateString(), GameObject.GetInstance().HumanAirline.Profile.Name);
            //file.Close();

            Logger.Fatal("Unhandled Exception", e.ExceptionObject);
        }
    }
}