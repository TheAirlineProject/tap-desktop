using SharpRaven;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the logger for the game
    public class TAPLogger
    {
        private static TAPLogger Logger;
        private RavenClient ravenClient;
        //logs an event
        public static void LogEvent(string exception, string location)
        {
            if (Logger == null)
                Logger = new TAPLogger();

            Logger.logEvent(exception, location);

        }
        private TAPLogger()
        {
            try
            {
                ravenClient = new RavenClient(new Dsn("https://8200a0f6a6584bd286d721942b02a0ef:220ce0211c044f12976d2401d1bfe8af@app.getsentry.com/31831"));
            }
            catch
            {
                ravenClient = null;
            }
        }
        private void logEvent(string exception, string location)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();

            messages.Add("Version", TheAirline.Properties.Settings.Default.Version);
            messages.Add("Human", GameObject.GetInstance().HumanAirline.Profile.Name);
            messages.Add("Gametime", GameObject.GetInstance().GameTime.ToShortDateString());
            messages.Add("Location", location);

            if (ravenClient != null)
                ravenClient.CaptureMessage(exception, SharpRaven.Data.ErrorLevel.Error, messages);
            else
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(AppSettings.getCommonApplicationDataPath() + "\\theairline.log");

                foreach (string message in messages.Keys)
                    file.WriteLine(message + ": " + messages[message]);

                file.Write(exception);
                           file.Close();
            }
        }

    }

}
