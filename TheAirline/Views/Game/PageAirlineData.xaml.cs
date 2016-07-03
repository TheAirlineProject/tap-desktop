using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.CustomControlsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.ObjectsModel;
using TheAirline.GUIModel.PagesModel.GamePageModel;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;
using TheAirline.Views.Airline;

namespace TheAirline.Views.Game
{
    /// <summary>
    ///     Interaction logic for PageAirlineData.xaml
    /// </summary>
    [Export("PageAirlineData")]
    public partial class PageAirlineData
    {
    //    #region Fields

    //    private readonly StartDataObject StartData;

    //    #endregion

    //    #region Constructors and Destructors

    //    public PageAirlineData(StartDataObject startData)
    //    {
    //        AllTimeZones = TimeZones.GetTimeZones();
    //        AllAirports = new ObservableCollection<Airport>();
    //        StartData = startData;

    //        GameObject.GetInstance().GameTime = new DateTime(StartData.Year, 1, 1);

    //        InitializeComponent();

    //        List<Models.Airlines.Airline> airlines =
    //            Airlines.GetAirlines(
    //                airline =>
    //                    (airline.Profile.Country.Region == StartData.Region
    //                     || (StartData.Region.Uid == "100" && StartData.Continent.Uid == "100")
    //                     || (StartData.Region.Uid == "100"
    //                         && StartData.Continent.HasRegion(airline.Profile.Country.Region)))
    //                    && airline.Profile.Founded <= StartData.Year
    //                    && airline.Profile.Folded > StartData.Year).OrderBy(a => a.Profile.Name).ToList();

    //        cbAirline.ItemsSource = airlines;
    //    }

    //    #endregion

    //    #region Public Properties

    //    public ObservableCollection<Airport> AllAirports { get; set; }

    //    public List<GameTimeZone> AllTimeZones { get; set; }

    //    #endregion

    //    #region Methods

    //    private void btnBack_Click(object sender, RoutedEventArgs e)
    //    {
    //        var frmContent = UIHelpers.FindChild<Frame>((Page)Tag, "frmContent");

    //        frmContent.Navigate(new PageStartData { Tag = Tag });
    //    }

    //    private void btnCreateGame_Click(object sender, RoutedEventArgs e)
    //    {
    //        StartData.Airline = (Models.Airlines.Airline)cbAirline.SelectedItem;
    //        StartData.Airport = (Airport)cbAirport.SelectedItem;
    //        StartData.CEO = txtCEO.Text;
    //        StartData.HomeCountry = (Country)cbCountry.SelectedItem;
    //        StartData.TimeZone = (GameTimeZone)cbTimeZone.SelectedItem;
    //        StartData.LocalCurrency = cbLocalCurrency.IsChecked.Value
    //                                       && StartData.HomeCountry.HasLocalCurrency;

    //        if (StartData.SelectedCountries != null)
    //        {
    //            PageNavigator.NavigateTo(new PageSelectAirports(StartData));
    //        }
    //        else if (!StartData.RandomOpponents)
    //        {
    //            PageNavigator.NavigateTo(new PageSelectOpponents(StartData));
    //        }
    //        else
    //        {
    //            var scCreating = UIHelpers.FindChild<SplashControl>(this, "scCreating");

    //            scCreating.Visibility = Visibility.Visible;

    //            var bgWorker = new BackgroundWorker();
    //            bgWorker.DoWork += (y, x) => { GameObjectHelpers.CreateGame(StartData); };
    //            bgWorker.RunWorkerCompleted += (y, x) =>
    //            {
    //                scCreating.Visibility = Visibility.Collapsed;

    //                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

    //                PageNavigator.ClearNavigator();
    //            };
    //            bgWorker.RunWorkerAsync();
    //        }
    //    }

    //    private void btnLoadAirline_Click(object sender, RoutedEventArgs e)
    //    {
    //        string directory = AppSettings.GetCommonApplicationDataPath() + "\\custom airlines";

    //        var dlg = new OpenFileDialog();

    //        dlg.DefaultExt = ".xml";
    //        dlg.Filter = "Airline XMLs (.xml)|*.xml";
    //        dlg.InitialDirectory = Path.GetFullPath(directory);
    //        dlg.Multiselect = false;

    //        bool? result = dlg.ShowDialog();

    //        if (result == true)
    //        {
    //            string path = dlg.FileName;

    //            Models.Airlines.Airline airline = Setup.LoadAirline(path);

    //            string imagePath = string.Format("{0}\\{1}.png", directory, airline.Profile.IATACode);

    //            if (File.Exists(imagePath))
    //            {
    //                airline.Profile.AddLogo(new AirlineLogo(imagePath));
    //            }
    //            else
    //            {
    //                airline.Profile.AddLogo(
    //                    new AirlineLogo(AppSettings.GetDataPath() + "\\graphics\\airlinelogos\\default.png"));
    //            }

    //            if (Airlines.GetAirline(airline.Profile.IATACode) != null)
    //            {
    //                Airlines.RemoveAirlines(a => a.Profile.IATACode == airline.Profile.IATACode);
    //            }

    //            Airlines.AddAirline(airline);

    //            List<Models.Airlines.Airline> airlines =
    //                Airlines.GetAirlines(
    //                    a =>
    //                        (a.Profile.Country.Region == StartData.Region
    //                         || (StartData.Region.Uid == "100" && StartData.Continent.Uid == "100")
    //                         || (StartData.Region.Uid == "100"
    //                             && StartData.Continent.HasRegion(a.Profile.Country.Region)))
    //                        && a.Profile.Founded <= StartData.Year && a.Profile.Folded > StartData.Year)
    //                    .OrderBy(a => a.Profile.Name)
    //                    .ToList();

    //            cbAirline.ItemsSource = airlines;

    //            WPFMessageBox.Show(
    //                Translator.GetInstance().GetString("MessageBox", "2409"),
    //                Translator.GetInstance().GetString("MessageBox", "2409", "message"),
    //                WPFMessageBoxButtons.Ok);

    //            if (cbAirline.Items.Contains(airline))
    //            {
    //                cbAirline.SelectedItem = airline;
    //            }
    //        }
    //    }

    //    private void btnStartMenu_Click(object sender, RoutedEventArgs e)
    //    {
    //        //PageNavigator.NavigateTo(new PageStartMenu());
    //    }

    //    private void cbAirline_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //    {
    //        var airline = (Models.Airlines.Airline)cbAirline.SelectedItem;

    //        AllAirports.Clear();

    //        foreach (
    //            Airport airport in
    //                Airports.GetAllActiveAirports()
    //                    .Where(a => airline.Profile.Countries.Contains(a.Profile.Country))
    //                    .OrderBy(a => a.Profile.Name))
    //        {
    //            AllAirports.Add(airport);
    //        }

    //        if (AllAirports.Contains(airline.Profile.PreferedAirport))
    //        {
    //            cbAirport.SelectedItem = airline.Profile.PreferedAirport;
    //        }
    //        else
    //        {
    //            cbAirport.SelectedIndex = 0;
    //        }
    //    }

    //    #endregion
    }
}