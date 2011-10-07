using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using TheAirlineV2.GraphicsModel.UserControlModel;
using System.Windows.Navigation;

namespace TheAirlineV2.GraphicsModel.PageModel.PageAirportsModel.PanelAirportsModel
{
    public class PanelAirports : StackPanel
    {
        private Frame frameContent;
        private PageAirports ParentPage;
        public PanelAirports(PageAirports parent)
        {
            this.ParentPage = parent;

            WrapPanel buttonsPanel = new WrapPanel();


            ucSelectButton sbSearch = new ucSelectButton();
            sbSearch.Content = "Search";
            sbSearch.IsSelected = true;
            sbSearch.Click += new System.Windows.RoutedEventHandler(sbSearch_Click);
            buttonsPanel.Children.Add(sbSearch);

            ucSelectButton sbStatistics = new ucSelectButton();
            sbStatistics.Content = "Statistics";
            sbStatistics.Click += new System.Windows.RoutedEventHandler(sbStatistics_Click);
            buttonsPanel.Children.Add(sbStatistics);

            this.Children.Add(buttonsPanel);

            frameContent = new Frame();
            frameContent.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            frameContent.Navigate(new PageSearchAirports(this.ParentPage));

            this.Children.Add(frameContent);
        }

        private void sbStatistics_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageAirportsStatistics());
        }

        private void sbSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageSearchAirports(this.ParentPage));
        }
    }
}
