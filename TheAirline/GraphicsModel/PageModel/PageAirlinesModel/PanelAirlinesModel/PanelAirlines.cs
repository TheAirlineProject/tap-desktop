using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Navigation;
using TheAirline.GraphicsModel.UserControlModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlinesModel.PanelAirlinesModel
{
    public class PanelAirlines : StackPanel
    {
        private Frame frameContent;
        public PanelAirlines()
        {

            WrapPanel buttonsPanel = new WrapPanel();

            // chs, 2011-18-10 added for different views / statistics for airlines
            ucSelectButton sbFlights = new ucSelectButton();
            sbFlights.Content = "Flights";
            sbFlights.IsSelected = true;
            sbFlights.Click += new System.Windows.RoutedEventHandler(sbFlights_Click);
            buttonsPanel.Children.Add(sbFlights);

            ucSelectButton sbFinancial = new ucSelectButton();
            sbFinancial.Content = "Financial";
            sbFinancial.Click += new System.Windows.RoutedEventHandler(sbFinancial_Click);
            buttonsPanel.Children.Add(sbFinancial);

            ucSelectButton sbFleet = new ucSelectButton();
            sbFleet.Content = "Fleet";
            sbFleet.Click += new System.Windows.RoutedEventHandler(sbFleet_Click);
            buttonsPanel.Children.Add(sbFleet);

            this.Children.Add(buttonsPanel);

            frameContent = new Frame();
            frameContent.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            frameContent.Navigate(new PageAirlinesStatistics());

            this.Children.Add(frameContent);
        }

        private void sbFleet_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirlinesExtendedStatistics(PageAirlinesExtendedStatistics.ViewType.Fleet));
        }

        private void sbFinancial_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirlinesExtendedStatistics(PageAirlinesExtendedStatistics.ViewType.Financial));
        }

        private void sbFlights_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirlinesStatistics());
        }

    }
}
