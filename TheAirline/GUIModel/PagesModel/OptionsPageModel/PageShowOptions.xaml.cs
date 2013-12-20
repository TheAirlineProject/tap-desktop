using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    /// <summary>
    /// Interaction logic for PageShowOptions.xaml
    /// </summary>
    public partial class PageShowOptions : Page
    {
        public OptionsMVVM Options { get; set; }
        public PageShowOptions()
        {
            this.Options = new OptionsMVVM();
            this.DataContext = this.Options;

            this.Loaded += PageShowOptions_Loaded;

            InitializeComponent();
        }

        private void PageShowOptions_Loaded(object sender, RoutedEventArgs e)
        {
            setIntevalValues();
        }
        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
             WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2952"), Translator.GetInstance().GetString("MessageBox", "2952", "message"), WPFMessageBoxButtons.YesNo);

             if (result == WPFMessageBoxResult.Yes)
             {
                 AppSettings.GetInstance().setLanguage((Language)cbLanguage.SelectedItem);

                 Settings.GetInstance().AirportCodeDisplay = rbIATA.IsChecked.Value ? Settings.AirportCode.IATA : Settings.AirportCode.ICAO;
                 Settings.GetInstance().MailsOnLandings = cbLandings.IsChecked.Value;
                 Settings.GetInstance().MailsOnBadWeather = cbWeather.IsChecked.Value;
                 Settings.GetInstance().MailsOnAirlineRoutes = cbAirlineDestinations.IsChecked.Value;
                 Settings.GetInstance().CurrencyShorten = cbShortenCurrency.IsChecked.Value;

                 if (this.Options.HourRoundEnabled)
                     Settings.GetInstance().MinutesPerTurn = (int)cbHours.SelectedItem;

                 GeneralHelpers.GameSpeedValue gameSpeed = (GeneralHelpers.GameSpeedValue)Enum.ToObject(typeof(GeneralHelpers.GameSpeedValue), (int)slGameSpeed.Value);
                 
                 Settings.GetInstance().setGameSpeed(gameSpeed);

                 var rbAutoSaves = UIHelpers.FindRBChildren(this, "AutoSave");

                 foreach (RadioButton rbInterval in rbAutoSaves)
                 { 
                     if (rbInterval.IsChecked.Value)
                         Settings.GetInstance().AutoSave = (Settings.Intervals)Enum.Parse(typeof(Settings.Intervals), rbInterval.Tag.ToString(), true);
                 }

                  var rbClearings = UIHelpers.FindRBChildren(this, "ClearStats");

                  foreach (RadioButton rbInterval in rbClearings)
                  {
                      if (rbInterval.IsChecked.Value)
                          Settings.GetInstance().ClearStats = (Settings.Intervals)Enum.Parse(typeof(Settings.Intervals), rbInterval.Tag.ToString(), true);
          
                  }
             }
        

        }
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            this.Options.undoChanges();
            setIntevalValues();

        }
        //sets the values of the interval types
        private void setIntevalValues()
        {
            var rbAutoSaves = UIHelpers.FindRBChildren(this, "AutoSave");

            foreach (RadioButton rbInterval in rbAutoSaves)
            {
                if (rbInterval.Tag.ToString() == this.Options.AutoSave.ToString())
                    rbInterval.IsChecked = true;
            }

            var rbClearings = UIHelpers.FindRBChildren(this, "ClearStats");

            foreach (RadioButton rbInterval in rbClearings)
            {
                if (rbInterval.Tag.ToString() == this.Options.ClearStats.ToString())
                    rbInterval.IsChecked = true;
            }
        }
    }
}
