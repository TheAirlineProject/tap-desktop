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

            InitializeComponent();

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
                 Settings.GetInstance().CurrencyShorten = cbShortenCurrency.IsChecked.Value;

                 if (this.Options.HourRoundEnabled)
                     Settings.GetInstance().MinutesPerTurn = (int)cbHours.SelectedItem;

                 GeneralHelpers.GameSpeedValue gameSpeed = (GeneralHelpers.GameSpeedValue)Enum.ToObject(typeof(GeneralHelpers.GameSpeedValue), (int)slGameSpeed.Value);
                 
                 GameTimer.GetInstance().setGameSpeed(gameSpeed);
             }
        

        }
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            this.Options.undoChanges();

        }
    }
}
