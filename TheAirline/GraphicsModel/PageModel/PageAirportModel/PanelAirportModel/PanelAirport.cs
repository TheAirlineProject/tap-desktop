using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Navigation;
using TheAirline.GraphicsModel.UserControlModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using System.Diagnostics;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    public class PanelAirport : StackPanel
    {
        private Frame frameContent;
        private Airport Airport;
        public PanelAirport(Airport airport)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            this.Airport = airport;

            WrapPanel buttonsPanel = new WrapPanel();


            ucSelectButton sbGates = new ucSelectButton();
            sbGates.Uid = "201";
            sbGates.Content = Translator.GetInstance().GetString("PanelAirport", sbGates.Uid);
            sbGates.Click += new System.Windows.RoutedEventHandler(sbGates_Click);
            sbGates.IsSelected = true;
            buttonsPanel.Children.Add(sbGates);

            ucSelectButton sbRunways = new ucSelectButton();
            sbRunways.Uid = "205";
            sbRunways.Content = Translator.GetInstance().GetString("PanelAirport", sbRunways.Uid);
            sbRunways.Click += new System.Windows.RoutedEventHandler(sbRunways_Click);
            buttonsPanel.Children.Add(sbRunways);

            ucSelectButton sbFacilities = new ucSelectButton();
            sbFacilities.Uid = "202";
            sbFacilities.Content = Translator.GetInstance().GetString("PanelAirport", sbFacilities.Uid);
            sbFacilities.Click += new System.Windows.RoutedEventHandler(sbFacilities_Click);
            buttonsPanel.Children.Add(sbFacilities);

            ucSelectButton sbWeather = new ucSelectButton();
            sbWeather.Uid = "208";
            sbWeather.Content = Translator.GetInstance().GetString("PanelAirport", sbWeather.Uid);
            sbWeather.Click += new System.Windows.RoutedEventHandler(sbWeather_Click);
            buttonsPanel.Children.Add(sbWeather);

            ucSelectButton sbStatistics = new ucSelectButton();
            sbStatistics.Uid = "203";
            sbStatistics.Content = Translator.GetInstance().GetString("PanelAirport", sbStatistics.Uid);
            sbStatistics.Click += new System.Windows.RoutedEventHandler(sbStatistics_Click);
            buttonsPanel.Children.Add(sbStatistics);

            ucSelectButton sbTraffic = new ucSelectButton();
            sbTraffic.Uid = "207";
            sbTraffic.Content = Translator.GetInstance().GetString("PanelAirport", sbTraffic.Uid);
            sbTraffic.Click += new System.Windows.RoutedEventHandler(sbTraffic_Click);
            buttonsPanel.Children.Add(sbTraffic);

            ucSelectButton sbFlights = new ucSelectButton();
            sbFlights.Uid = "204";
            sbFlights.Content = Translator.GetInstance().GetString("PanelAirport", sbFlights.Uid);
            sbFlights.Click += new System.Windows.RoutedEventHandler(sbFlights_Click);
            buttonsPanel.Children.Add(sbFlights);

            ucSelectButton sbDistances = new ucSelectButton();
            sbDistances.Uid = "206";
            sbDistances.Content = Translator.GetInstance().GetString("PanelAirport",sbDistances.Uid);
            sbDistances.Click += new System.Windows.RoutedEventHandler(sbDistances_Click);
            buttonsPanel.Children.Add(sbDistances);

            this.Children.Add(buttonsPanel);

            frameContent = new Frame();
            frameContent.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            frameContent.Navigate(new PageAirportGates(this.Airport));

            this.Children.Add(frameContent);

            sw.Stop();
            PerformanceCounters.AddPerformanceCounter(new PagePerformanceCounter("PanelAirport", GameObject.GetInstance().GameTime, sw.ElapsedMilliseconds));
        }

        private void sbWeather_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirportWeather(this.Airport));
        }

        private void sbTraffic_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirportTraffic(this.Airport));
        }

        private void sbDistances_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirportDistances(this.Airport));
        }

        private void sbRunways_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirportRunways(this.Airport));
        }

        private void sbFlights_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirportFlights(this.Airport));
        }

        private void sbStatistics_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirportStatistics(this.Airport));
        }

        private void sbFacilities_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirportFacilities(this.Airport));
        }

        private void sbGates_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirportGates(this.Airport));
        }
    }
}
