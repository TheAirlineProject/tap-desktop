using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace TheAirline.GraphicsModel.PageModel.GeneralModel
{
    //the page for the content
    public class PageContent : Page
    {
        private StackPanel panelContent;
        public PageContent()
        {
            Brush brush = new SolidColorBrush(Colors.DarkGray);
            brush.Opacity = 0.80;

            this.Background = brush;

            Border frameBorder = new Border();
            frameBorder.BorderBrush = Brushes.Gray;
            frameBorder.BorderThickness = new Thickness(2);

            Grid panelMain = new Grid();

            RowDefinition rowDefTop = new RowDefinition();
            rowDefTop.Height = new GridLength(90, GridUnitType.Star);
            panelMain.RowDefinitions.Add(rowDefTop);

            RowDefinition rowDefBottom = new RowDefinition();
            rowDefBottom.Height = new GridLength(10, GridUnitType.Star);
            panelMain.RowDefinitions.Add(rowDefBottom);


            panelContent = new StackPanel();
            panelContent.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            panelContent.Margin = new Thickness(10, 10, 10, 10);
          
            Grid.SetRow(panelContent, 0);
            panelMain.Children.Add(panelContent);

            frameBorder.Child = panelMain;

            this.Content = frameBorder;

        }
       
        //sets the content
        public void setContent(UIElement content)
        {
            panelContent.Children.Clear();

            panelContent.Children.Add(content);
        }
    }
}
