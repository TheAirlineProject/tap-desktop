using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using TheAirlineV2.Model.AirlineModel;
using TheAirlineV2.GraphicsModel.UserControlModel;
using System.Windows.Navigation;
using TheAirlineV2.Model.GeneralModel;

namespace TheAirlineV2.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    public class PanelAirline : StackPanel
    {
        private Frame frameContent;
        private Airline Airline;
        public PanelAirline(Airline airline)
        {
            this.Airline = airline;

            WrapPanel buttonsPanel = new WrapPanel();

            ucSelectButton sbFleet = new ucSelectButton();
            sbFleet.Content = "Fleet";
            sbFleet.IsSelected = true;
            sbFleet.Click += new System.Windows.RoutedEventHandler(sbFleet_Click);
            buttonsPanel.Children.Add(sbFleet);

            ucSelectButton sbDestinations = new ucSelectButton();
            sbDestinations.Content = "Destinations and Routes";
            sbDestinations.Click += new System.Windows.RoutedEventHandler(sbDestinations_Click);
            buttonsPanel.Children.Add(sbDestinations);

            ucSelectButton sbFacilities = new ucSelectButton();
            sbFacilities.Content = "Facilities";
            sbFacilities.Click += new System.Windows.RoutedEventHandler(sbFacilities_Click);
            buttonsPanel.Children.Add(sbFacilities);

            ucSelectButton sbStatistics = new ucSelectButton();
            sbStatistics.Click += new System.Windows.RoutedEventHandler(sbStatistics_Click);
            sbStatistics.Content = "Statistics";
            buttonsPanel.Children.Add(sbStatistics);

            ucSelectButton sbFinances = new ucSelectButton();
            sbFinances.Content = "Finances";
            sbFinances.Click += new System.Windows.RoutedEventHandler(sbFinances_Click);
            buttonsPanel.Children.Add(sbFinances);

            ucSelectButton sbWages = new ucSelectButton();
            sbWages.Content = "Wages and Fees";
            sbWages.Visibility = this.Airline.IsHuman ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            sbWages.Click += new System.Windows.RoutedEventHandler(sbWages_Click);
            buttonsPanel.Children.Add(sbWages);

            this.Children.Add(buttonsPanel);

            frameContent = new Frame();
            frameContent.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            frameContent.Navigate(new PageAirlineFleet(this.Airline));

            this.Children.Add(frameContent);
        }

        private void sbWages_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirlineWages(this.Airline));
        }

        private void sbFinances_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirlineFinances(this.Airline));
        }

        private void sbFacilities_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirlineFacilities(this.Airline));
        }

        private void sbStatistics_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirlineStatistics(this.Airline));
        }

        private void sbDestinations_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirlineDestinations(this.Airline));
        }

        private void sbFleet_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirlineFleet(this.Airline));
        }
    }

    }

