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
using TheAirlineV2.GraphicsModel.PageModel.GeneralModel;
using System.IO;
using TheAirlineV2.Model.GeneralModel.Helpers;

namespace TheAirlineV2.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpSave.xaml
    /// </summary>
    public partial class PopUpSave : PopUpWindow
    {
        private TextBox txtName;
        private Button btnSave;
        public static object ShowPopUp()
        {
            PopUpWindow window = new PopUpSave();

            window.ShowDialog();

            return window.Selected == null ? null : window.Selected;
        }
        public PopUpSave()
        {
            InitializeComponent();

            this.Title = "Save game";

            this.Width = 400;

            this.Height = 500;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            ListBox lbSaves = new ListBox();
            lbSaves.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbSaves.Height = 375;
            lbSaves.DisplayMemberPath = "Key";
            lbSaves.SelectionChanged += new SelectionChangedEventHandler(lbSaves_SelectionChanged);
            //lbSaves.SelectedValuePath = "Name";

            foreach (KeyValuePair<string, string> savedFile in LoadSaveHelpers.GetSavedGames())
                lbSaves.Items.Add(savedFile);

            mainPanel.Children.Add(lbSaves);

            txtName = new TextBox();
            txtName.Margin = new Thickness(0, 10, 0, 0);
            txtName.Background = Brushes.Transparent;
            txtName.TextChanged += new TextChangedEventHandler(txtName_TextChanged);
            mainPanel.Children.Add(txtName);

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 10, 0, 0);

            mainPanel.Children.Add(panelButtons);

            btnSave = new Button();
            btnSave.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSave.Height = 20;
            btnSave.Width = 80;
            btnSave.Content = "Save";
            btnSave.Click += new RoutedEventHandler(btnSave_Click);
            btnSave.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnSave.IsEnabled = false;

            panelButtons.Children.Add(btnSave);

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

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnSave.IsEnabled = txtName.Text.Length > 0;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (txtName.Text.Length > 0)
                 this.Selected = txtName.Text;
            else
                this.Selected = null;
            this.Close();
       
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
       
        }

        private void lbSaves_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtName.Text = ((KeyValuePair<string, string>)((ListBox)sender).SelectedItem).Key;
        }
    }
}
