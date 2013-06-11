using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Navigation;
using TheAirline.Model.AirlinerModel;
using TheAirline.GraphicsModel.UserControlModel;
using TheAirline.Model.GeneralModel;
using System.Windows;

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
            sbFacilities.Content = Translator.GetInstance().GetString("PanelFleetAirliner","200");
            sbFacilities.IsSelected = this.Airliner.Airliner.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger;
            sbFacilities.Click += new System.Windows.RoutedEventHandler(sbFacilities_Click);
            sbFacilities.Visibility = this.Airliner.Airliner.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            buttonsPanel.Children.Add(sbFacilities);

            ucSelectButton sbRoute = new ucSelectButton();
            sbRoute.Content = Translator.GetInstance().GetString("PanelFleetAirliner","201");
            sbRoute.IsSelected = this.Airliner.Airliner.Type.TypeAirliner != AirlinerType.TypeOfAirliner.Passenger;
            sbRoute.Click += new System.Windows.RoutedEventHandler(sbRoute_Click);
            buttonsPanel.Children.Add(sbRoute);

            ucSelectButton sbTimeSlot = new ucSelectButton();
            sbTimeSlot.Content  = Translator.GetInstance().GetString("PanelFleetAirliner","202");
            sbTimeSlot.Visibility = System.Windows.Visibility.Collapsed;
            sbTimeSlot.Click += new System.Windows.RoutedEventHandler(sbTimeSlot_Click);
            buttonsPanel.Children.Add(sbTimeSlot);

            ucSelectButton sbStatistics = new ucSelectButton();
            sbStatistics.Content  = Translator.GetInstance().GetString("PanelFleetAirliner","203");
            sbStatistics.Click += new System.Windows.RoutedEventHandler(sbStatistics_Click);
            buttonsPanel.Children.Add(sbStatistics);

            ucSelectButton sbMaintenance = new ucSelectButton();
            sbMaintenance.Content = Translator.GetInstance().GetString("PanelFleetAirliner","205");
            sbMaintenance.Visibility = this.Airliner.Airliner.Airline.IsHuman ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            sbMaintenance.Click +=sbMaintenance_Click;
            buttonsPanel.Children.Add(sbMaintenance);

            ucSelectButton sbInsurance = new ucSelectButton();
            sbInsurance.Content = Translator.GetInstance().GetString("PanelFleetAirliner", "204");
            sbInsurance.Visibility = this.Airliner.Airliner.Airline.IsHuman ? Visibility.Visible : Visibility.Collapsed;
            sbInsurance.Click+=sbInsurance_Click;
            buttonsPanel.Children.Add(sbInsurance);

             this.Children.Add(buttonsPanel);

            frameContent = new Frame();
            frameContent.NavigationUIVisibility = NavigationUIVisibility.Hidden;

            if (this.Airliner.Airliner.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
                frameContent.Navigate(new PageFleetFacilities(this.Airliner));
            else
                frameContent.Navigate(new PageFleetRoute(this.Airliner));

            this.Children.Add(frameContent);

         }

        private void sbMaintenance_Click(object sender, RoutedEventArgs e)
        {
            frameContent.Navigate(new PageFleetMaintenance(this.Airliner));
        }

        private void sbInsurance_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageFleetInsurance(this.Airliner));
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
