using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using TheAirlineV2.GraphicsModel.UserControlModel;
using System.Windows.Navigation;

namespace TheAirlineV2.GraphicsModel.PageModel.PageAirlinesModel.PanelAirlinesModel
{
    public class PanelAirlines : StackPanel
    {
        private Frame frameContent;
        public PanelAirlines()
        {

            WrapPanel buttonsPanel = new WrapPanel();


            ucSelectButton sbStatistics = new ucSelectButton();
            sbStatistics.Content = "Statistics";
            //sbGates.Click += new System.Windows.RoutedEventHandler(sbGates_Click);
            sbStatistics.IsSelected = true;
            buttonsPanel.Children.Add(sbStatistics);

            //this.Children.Add(buttonsPanel);

            frameContent = new Frame();
            frameContent.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            frameContent.Navigate(new PageAirlinesStatistics());

            this.Children.Add(frameContent);
        }

    }
}
