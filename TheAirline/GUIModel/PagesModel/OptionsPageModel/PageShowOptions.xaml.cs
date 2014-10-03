using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageShowOptions.xaml
    /// </summary>
    public partial class PageShowOptions : Page
    {
        #region Constructors and Destructors

        public PageShowOptions()
        {
            this.Options = new OptionsMVVM();
            this.DataContext = this.Options;

            this.Loaded += this.PageShowOptions_Loaded;

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public OptionsMVVM Options { get; set; }

        #endregion

        #region Methods

        private void PageShowOptions_Loaded(object sender, RoutedEventArgs e)
        {
            this.setIntevalValues();
        }

        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2952"),
                Translator.GetInstance().GetString("MessageBox", "2952", "message"),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                AppSettings.GetInstance().SetLanguage((Language)this.cbLanguage.SelectedItem);

                Settings.GetInstance().AirportCodeDisplay = this.rbIATA.IsChecked.Value
                    ? Settings.AirportCode.IATA
                    : Settings.AirportCode.ICAO;
                Settings.GetInstance().MailsOnLandings = this.cbLandings.IsChecked.Value;
                Settings.GetInstance().MailsOnBadWeather = this.cbWeather.IsChecked.Value;
                Settings.GetInstance().MailsOnAirlineRoutes = this.cbAirlineDestinations.IsChecked.Value;
                Settings.GetInstance().CurrencyShorten = this.cbShortenCurrency.IsChecked.Value;

                if (this.Options.HourRoundEnabled)
                {
                    Settings.GetInstance().MinutesPerTurn = (int)this.cbHours.SelectedItem;
                }

                var gameSpeed =
                    (GeneralHelpers.GameSpeedValue)
                        Enum.ToObject(typeof(GeneralHelpers.GameSpeedValue), (int)this.slGameSpeed.Value);

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
            this.Options.undoChanges();
            this.setIntevalValues();
        }

        //sets the values of the interval types
        private void setIntevalValues()
        {
            List<RadioButton> rbAutoSaves = UIHelpers.FindRBChildren(this, "AutoSave");

            foreach (RadioButton rbInterval in rbAutoSaves)
            {
                if (rbInterval.Tag.ToString() == this.Options.AutoSave.ToString())
                {
                    rbInterval.IsChecked = true;
                }
            }

            List<RadioButton> rbClearings = UIHelpers.FindRBChildren(this, "ClearStats");

            foreach (RadioButton rbInterval in rbClearings)
            {
                if (rbInterval.Tag.ToString() == this.Options.ClearStats.ToString())
                {
                    rbInterval.IsChecked = true;
                }
            }
        }

        #endregion
    }
}