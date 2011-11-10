using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Navigation;
using TheAirline.GraphicsModel.UserControlModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
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
            sbGates.Uid = "201";
            // chs, 2011-27-10 changed for the possibility of purchasing a terminal

            sbGates.Content = Translator.GetInstance().GetString("PanelAirport", sbGates.Uid);
            sbGates.Click += new System.Windows.RoutedEventHandler(sbGates_Click);
            sbGates.IsSelected = true;
            buttonsPanel.Children.Add(sbGates);

            ucSelectButton sbFacilities = new ucSelectButton();
            sbFacilities.Uid = "202";
            sbFacilities.Content = Translator.GetInstance().GetString("PanelAirport", sbFacilities.Uid);
            sbFacilities.Click += new System.Windows.RoutedEventHandler(sbFacilities_Click);
            buttonsPanel.Children.Add(sbFacilities);

            ucSelectButton sbStatistics = new ucSelectButton();
            sbStatistics.Uid = "203";
            sbStatistics.Content = Translator.GetInstance().GetString("PanelAirport", sbStatistics.Uid);
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
