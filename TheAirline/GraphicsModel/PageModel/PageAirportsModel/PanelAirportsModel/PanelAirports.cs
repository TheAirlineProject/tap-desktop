using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Navigation;
using TheAirline.GraphicsModel.UserControlModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirportsModel.PanelAirportsModel
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
            sbSearch.Uid = "201";
            sbSearch.Content = Translator.GetInstance().GetString("PanelAirports", sbSearch.Uid);
            sbSearch.IsSelected = true;
            sbSearch.Click += new System.Windows.RoutedEventHandler(sbSearch_Click);
            buttonsPanel.Children.Add(sbSearch);

            ucSelectButton sbExtendedSearch = new ucSelectButton();
            sbExtendedSearch.Uid = "203";
            sbExtendedSearch.Content = Translator.GetInstance().GetString("PanelAirports", sbExtendedSearch.Uid);
            sbExtendedSearch.Click += new System.Windows.RoutedEventHandler(sbExtendedSearch_Click);
            buttonsPanel.Children.Add(sbExtendedSearch);

            ucSelectButton sbStatistics = new ucSelectButton();
            sbStatistics.Uid = "202";
            sbStatistics.Content = Translator.GetInstance().GetString("PanelAirports", sbStatistics.Uid);
            sbStatistics.Click += new System.Windows.RoutedEventHandler(sbStatistics_Click);
            buttonsPanel.Children.Add(sbStatistics);

            this.Children.Add(buttonsPanel);

            frameContent = new Frame();
            frameContent.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            frameContent.Navigate(new PageSearchAirports(this.ParentPage));

            this.Children.Add(frameContent);
        }

        private void sbExtendedSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            frameContent.Navigate(new PageExtendedSearchAirports(this.ParentPage));
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
