using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpSingleItem.xaml
    /// </summary>
    public partial class PopUpSingleElement : PopUpWindow
    {
        private UIElement Element;
        public enum ButtonSelected { OK, Cancel }
        public static ButtonSelected ShowPopUp(string title, UIElement element)
        {
            PopUpSingleElement window = new PopUpSingleElement(title, element);
            window.ShowDialog();

            return window.Selected == null ? ButtonSelected.Cancel : (ButtonSelected)window.Selected;
        }
        public PopUpSingleElement(string title, UIElement element)
        {
            this.Element = element;

            InitializeComponent();

            this.Title = title;
          
            this.Width = 400;

            this.Height = 125;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            mainPanel.Children.Add(element);
            mainPanel.Children.Add(createButtonsPanel());

            this.Content = mainPanel;
        }
        //creates the buttons panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 10, 0, 0);

            Button btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "StandardButtonStyle");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
    
            buttonsPanel.Children.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.Uid = "101";
            btnCancel.SetResourceReference(Button.StyleProperty, "StandardButtonStyle");
            btnCancel.Height = Double.NaN;
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.Width = Double.NaN;
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnCancel.Content = Translator.GetInstance().GetString("General", btnCancel.Uid);
         
            buttonsPanel.Children.Add(btnCancel);


            return buttonsPanel;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = ButtonSelected.Cancel;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = ButtonSelected.OK;
            this.Close();
        }
    }
}
