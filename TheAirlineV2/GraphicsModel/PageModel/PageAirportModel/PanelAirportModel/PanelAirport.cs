using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using TheAirlineV2.GraphicsModel.UserControlModel;
using System.Windows.Navigation;
using TheAirlineV2.Model.AirportModel;

namespace TheAirlineV2.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    public class PanelAirport : StackPanel
    {
        private Frame frameContent;
        private Airport Airport;
        public PanelAirport(Airport airport)
        {
            this.Airport = airport;

            WrapPanel buttonsPanel = new WrapPanel();


            ucSelectButton sbGates = new ucSelectButton();
            sbGates.Content = "Gates";
            sbGates.Click += new System.Windows.RoutedEventHandler(sbGates_Click);
            sbGates.IsSelected = true;
            buttonsPanel.Children.Add(sbGates);

            ucSelectButton sbFacilities = new ucSelectButton();
            sbFacilities.Content = "Facilities";
            sbFacilities.Click += new System.Windows.RoutedEventHandler(sbFacilities_Click);
            buttonsPanel.Children.Add(sbFacilities);

            ucSelectButton sbStatistics = new ucSelectButton();
            sbStatistics.Content = "Statistics";
            sbStatistics.Click += new System.Windows.RoutedEventHandler(sbStatistics_Click);
            buttonsPanel.Children.Add(sbStatistics);


            this.Children.Add(buttonsPanel);

            frameContent = new Frame();
            frameContent.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            frameContent.Navigate(new PageAirportGates(this.Airport));

            this.Children.Add(frameContent);
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
