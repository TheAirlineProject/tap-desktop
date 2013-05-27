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
using TheAirline.Model.GeneralModel.Helpers;
using System.IO;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpLoad.xaml
    /// </summary>
    public partial class PopUpLoad : PopUpWindow
    {
        private ListBox lbSaves;
        private Button btnLoad, btnDelete;
        public static object ShowPopUp()
        {
            PopUpWindow window = new PopUpLoad();

            window.ShowDialog();

            return window.Selected == null ? null : window.Selected;
        }
        public PopUpLoad()
        {
            InitializeComponent();

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PopUpLoad", this.Uid);

            this.Width = 400;

            this.Height = 500;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            lbSaves = new ListBox();
            lbSaves.Height = 375;
            lbSaves.SelectionChanged += new SelectionChangedEventHandler(lbSaves_SelectionChanged);

      
            mainPanel.Children.Add(lbSaves);

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 10, 0, 0);

            mainPanel.Children.Add(panelButtons);

            btnLoad = new Button();
            btnLoad.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnLoad.Height = Double.NaN;
            btnLoad.Width = Double.NaN;
            btnLoad.Content = "Load";
            btnLoad.Click += new RoutedEventHandler(btnLoad_Click);
             btnLoad.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
             btnLoad.IsEnabled = false;

            panelButtons.Children.Add(btnLoad);

            Button btnCancel = new Button();
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = Double.NaN;
            btnCancel.Width = Double.NaN;
            btnCancel.Content = "Cancel";
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnCancel);

            btnDelete = new Button();
            btnDelete.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnDelete.Height = Double.NaN;
            btnDelete.Width = Double.NaN;
            btnDelete.Content = "Delete";
            btnDelete.Margin = new Thickness(5, 0, 0, 0);
            btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            btnDelete.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnDelete.IsEnabled = false;

            panelButtons.Children.Add(btnDelete);

            this.Content = mainPanel;

            showSaves();
        }
        //shows the list of saved files
        private void showSaves()
        {
            lbSaves.Items.Clear();

            foreach (string savedFile in LoadSaveHelpers.GetSavedGames())
                lbSaves.Items.Add(savedFile);

        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            LoadSaveHelpers.DeleteGame(lbSaves.SelectedItem as string);
            showSaves();

            btnLoad.IsEnabled = false; ;
            btnDelete.IsEnabled = false;

        }

        private void lbSaves_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnLoad.IsEnabled = true;
            btnDelete.IsEnabled = true;
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = lbSaves.SelectedItem != null ? ((string)lbSaves.SelectedItem) : null;
            this.Close();
    
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
    
        }
    }
}
