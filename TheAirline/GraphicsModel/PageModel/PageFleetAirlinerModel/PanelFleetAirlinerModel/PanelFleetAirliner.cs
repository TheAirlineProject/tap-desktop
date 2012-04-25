using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Navigation;
using TheAirline.Model.AirlinerModel;
using TheAirline.GraphicsModel.UserControlModel;

namespace TheAirline.GraphicsModel.PageModel.PageFleetAirlinerModel.PanelFleetAirlinerModel
{
    public class PanelFleetAirliner : StackPanel
    {
        private Frame frameContent;
        private FleetAirliner Airliner;
        public PanelFleetAirliner(FleetAirliner airliner)
        {
            this.Airliner = airliner;

            WrapPanel buttonsPanel = new WrapPanel();


            ucSelectButton sbFacilities = new ucSelectButton();
            sbFacilities.Content = "Facilities";
            sbFacilities.IsSelected = true;
            sbFacilities.Click += new System.Windows.RoutedEventHandler(sbFacilities_Click);
            buttonsPanel.Children.Add(sbFacilities);

            ucSelectButton sbRoute = new ucSelectButton();
            sbRoute.Content = "Route & Flight";
            sbRoute.Click += new System.Windows.RoutedEventHandler(sbRoute_Click);
            buttonsPanel.Children.Add(sbRoute);

            ucSelectButton sbTimeSlot = new ucSelectButton();
            sbTimeSlot.Content = "Timeslot";
            sbTimeSlot.Visibility = System.Windows.Visibility.Collapsed;
            sbTimeSlot.Click += new System.Windows.RoutedEventHandler(sbTimeSlot_Click);
            buttonsPanel.Children.Add(sbTimeSlot);


            ucSelectButton sbStatistics = new ucSelectButton();
            sbStatistics.Content = "Statistics";
            sbStatistics.Click += new System.Windows.RoutedEventHandler(sbStatistics_Click);
            buttonsPanel.Children.Add(sbStatistics);

            this.Children.Add(buttonsPanel);

            frameContent = new Frame();
            frameContent.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            frameContent.Navigate(new PageFleetFacilities(this.Airliner));

            this.Children.Add(frameContent);

         }

        private void sbTimeSlot_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageFleetTimeslot(this.Airliner));
        }

        private void sbFacilities_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageFleetFacilities(this.Airliner));
        }

        private void sbStatistics_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageFleetStatistics(this.Airliner));
        }

        private void sbRoute_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageFleetRoute(this.Airliner));
        }
    }
}
