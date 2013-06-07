using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Navigation;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    public class PanelAirline : StackPanel
    {
        private Frame frameContent;
        private Airline Airline;
        private StandardPage PageParent;
        public PanelAirline(Airline airline,StandardPage parent)
        {
            this.Airline = airline;
            this.PageParent = parent;

            WrapPanel buttonsPanel = new WrapPanel();

            ucSelectButton sbFleet = new ucSelectButton();
            sbFleet.Uid = "1001";
            sbFleet.Content = Translator.GetInstance().GetString("PanelAirline", sbFleet.Uid);
            sbFleet.IsSelected = true;
            sbFleet.Click += new System.Windows.RoutedEventHandler(sbFleet_Click);
            buttonsPanel.Children.Add(sbFleet);
                 
            ucSelectButton sbDestinations = new ucSelectButton();
            sbDestinations.Uid = "1002";
            sbDestinations.Content = Translator.GetInstance().GetString("PanelAirline", sbDestinations.Uid);
            sbDestinations.Click += new System.Windows.RoutedEventHandler(sbDestinations_Click);
            buttonsPanel.Children.Add(sbDestinations);

            ucSelectButton sbPilots = new ucSelectButton();
            sbPilots.Uid = "1008";
            sbPilots.Content = Translator.GetInstance().GetString("PanelAirline", sbPilots.Uid);
            sbPilots.Click += sbPilots_Click;
            buttonsPanel.Children.Add(sbPilots);

            ucSelectButton sbStatistics = new ucSelectButton();
            sbStatistics.Uid = "1004";
            sbStatistics.Click += new System.Windows.RoutedEventHandler(sbStatistics_Click);
            sbStatistics.Content = Translator.GetInstance().GetString("PanelAirline", sbStatistics.Uid);
            buttonsPanel.Children.Add(sbStatistics);

            ucSelectButton sbFinances = new ucSelectButton();
            sbFinances.Uid = "1005";
            sbFinances.Content = Translator.GetInstance().GetString("PanelAirline", sbFinances.Uid);
            sbFinances.Click += new System.Windows.RoutedEventHandler(sbFinances_Click);
            buttonsPanel.Children.Add(sbFinances);

            ucSelectButton sbSubsidiary = new ucSelectButton();
            sbSubsidiary.Uid = "1007";
            sbSubsidiary.Content = Translator.GetInstance().GetString("PanelAirline", sbSubsidiary.Uid);
            sbSubsidiary.Click += new System.Windows.RoutedEventHandler(sbSubsidiary_Click);
            sbSubsidiary.Visibility = this.Airline is SubsidiaryAirline ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            buttonsPanel.Children.Add(sbSubsidiary);

            ucSelectButton sbWages = new ucSelectButton();
            sbWages.Uid = "1006";
            sbWages.Content = Translator.GetInstance().GetString("PanelAirline", sbWages.Uid);
            sbWages.Visibility = this.Airline.IsHuman ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            sbWages.Click += new System.Windows.RoutedEventHandler(sbWages_Click);
            buttonsPanel.Children.Add(sbWages);

            ucSelectButton sbInsurances = new ucSelectButton();
            sbInsurances.Uid = "1009";
            sbInsurances.Content = Translator.GetInstance().GetString("PanelAirline", sbInsurances.Uid);
            sbInsurances.Visibility = this.Airline.IsHuman ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            sbInsurances.Click += sbInsurances_Click;
            buttonsPanel.Children.Add(sbInsurances);

            this.Children.Add(buttonsPanel);

            frameContent = new Frame();
            frameContent.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            frameContent.Navigate(new PageAirlineFleet(this.Airline));

            this.Children.Add(frameContent);
        }

        private void sbInsurances_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirlineInsurances(this.Airline));
        }

        private void sbPilots_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirlinePilots(this.Airline));
        }

        private void sbSubsidiary_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirlineSubsidiaries(this.Airline,this.PageParent));
        }

     
        private void sbWages_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirlineWages(this.Airline));
        }

        private void sbFinances_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirlineFinances(this.Airline));
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

