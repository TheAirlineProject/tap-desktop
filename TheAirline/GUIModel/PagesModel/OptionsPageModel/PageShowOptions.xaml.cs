using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Models.General;

namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    /// <summary>
    ///     Interaction logic for PageShowOptions.xaml
    /// </summary>
    public partial class PageShowOptions : Page
    {
        #region Constructors and Destructors

        public PageShowOptions()
        {
            Options = new OptionsMVVM();
            DataContext = Options;

            Loaded += PageShowOptions_Loaded;

            InitializeComponent();
        }

        #endregion

        #region Public Properties

        public OptionsMVVM Options { get; set; }

        #endregion

        #region Methods

        private void PageShowOptions_Loaded(object sender, RoutedEventArgs e)
        {
            setIntevalValues();
        }

        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2952"),
                Translator.GetInstance().GetString("MessageBox", "2952", "message"),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                AppSettings.GetInstance().SetLanguage((Language)cbLanguage.SelectedItem);

                Settings.GetInstance().AirportCodeDisplay = rbIATA.IsChecked.Value
                    ? Settings.AirportCode.IATA
                    : Settings.AirportCode.ICAO;
                Settings.GetInstance().MailsOnLandings = cbLandings.IsChecked.Value;
                Settings.GetInstance().MailsOnBadWeather = cbWeather.IsChecked.Value;
                Settings.GetInstance().MailsOnAirlineRoutes = cbAirlineDestinations.IsChecked.Value;
                Settings.GetInstance().CurrencyShorten = cbShortenCurrency.IsChecked.Value;

                if (Options.HourRoundEnabled)
                {
                    Settings.GetInstance().MinutesPerTurn = (int)cbHours.SelectedItem;
                }

                var gameSpeed =
                    (GeneralHelpers.GameSpeedValue)
                        Enum.ToObject(typeof(GeneralHelpers.GameSpeedValue), (int)slGameSpeed.Value);

                Settings.GetInstance().SetGameSpeed(gameSpeed);

                List<RadioButton> rbAutoSaves = UIHelpers.FindRBChildren(this, "AutoSave");

                foreach (RadioButton rbInterval in rbAutoSaves)
                {
                    if (rbInterval.IsChecked.Value)
                    {
                        Settings.GetInstance().AutoSave =
                            (Settings.Intervals)Enum.Parse(typeof(Settings.Intervals), rbInterval.Tag.ToString(), true);
                    }
                }

                List<RadioButton> rbClearings = UIHelpers.FindRBChildren(this, "ClearStats");

                foreach (RadioButton rbInterval in rbClearings)
                {
                    if (rbInterval.IsChecked.Value)
                    {
                        Settings.GetInstance().ClearStats =
                            (Settings.Intervals)Enum.Parse(typeof(Settings.Intervals), rbInterval.Tag.ToString(), true);
                    }
                }
            }
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            Options.undoChanges();
            setIntevalValues();
        }

        //sets the values of the interval types
        private void setIntevalValues()
        {
            List<RadioButton> rbAutoSaves = UIHelpers.FindRBChildren(this, "AutoSave");

            foreach (RadioButton rbInterval in rbAutoSaves)
            {
                if (rbInterval.Tag.ToString() == Options.AutoSave.ToString())
                {
                    rbInterval.IsChecked = true;
                }
            }

            List<RadioButton> rbClearings = UIHelpers.FindRBChildren(this, "ClearStats");

            foreach (RadioButton rbInterval in rbClearings)
            {
                if (rbInterval.Tag.ToString() == Options.ClearStats.ToString())
                {
                    rbInterval.IsChecked = true;
                }
            }
        }

        #endregion
    }
}