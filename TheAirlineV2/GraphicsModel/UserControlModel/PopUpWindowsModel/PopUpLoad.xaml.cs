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
using TheAirlineV2.Model.GeneralModel.Helpers;

namespace TheAirlineV2.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpLoad.xaml
    /// </summary>
    public partial class PopUpLoad : PopUpWindow
    {
        private ListBox lbSaves;
        private Button btnLoad;
        public static object ShowPopUp()
        {
            PopUpWindow window = new PopUpLoad();

            window.ShowDialog();

            return window.Selected == null ? null : window.Selected;
        }
        public PopUpLoad()
        {
            InitializeComponent();

            this.Title = "Load a saved game";

            this.Width = 400;

            this.Height = 500;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            lbSaves = new ListBox();
            //lbSaves.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbSaves.Height = 375;
            lbSaves.DisplayMemberPath = "Key";
            lbSaves.SelectionChanged += new SelectionChangedEventHandler(lbSaves_SelectionChanged);

            foreach (KeyValuePair<string, string> savedFile in LoadSaveHelpers.GetSavedGames())
                lbSaves.Items.Add(savedFile);

            mainPanel.Children.Add(lbSaves);

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 10, 0, 0);

            mainPanel.Children.Add(panelButtons);

            btnLoad = new Button();
            btnLoad.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnLoad.Height = 20;
            btnLoad.Width = 80;
            btnLoad.Content = "Load";
            btnLoad.Click += new RoutedEventHandler(btnLoad_Click);
             btnLoad.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
             btnLoad.IsEnabled = false;

            panelButtons.Children.Add(btnLoad);

            Button btnCancel = new Button();
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = 20;
            btnCancel.Width = 80;
            btnCancel.Content = "Cancel";
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnCancel);



            this.Content = mainPanel;
        }

        private void lbSaves_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnLoad.IsEnabled = true;
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = lbSaves.SelectedItem != null ? ((KeyValuePair<string,string>)lbSaves.SelectedItem).Value : null;
            this.Close();
    
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
    
        }
    }
}
