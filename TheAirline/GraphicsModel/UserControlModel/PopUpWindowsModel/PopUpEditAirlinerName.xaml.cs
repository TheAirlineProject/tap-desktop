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
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpEditAirlinerName.xaml
    /// </summary>
    public partial class PopUpEditAirlinerName : PopUpWindow
    {
        private TextBox txtName;
        private Button btnOk;
        private FleetAirliner Airliner;
        public static object ShowPopUp(FleetAirliner airliner)
        {
            PopUpWindow window = new PopUpEditAirlinerName(airliner);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpEditAirlinerName(FleetAirliner airliner)
        {
            this.Airliner = airliner;

            InitializeComponent();
            this.Uid = "1000";

            this.Title = Translator.GetInstance().GetString("PopUpEditAirlinerName", this.Uid);

            this.Width = 300;

            this.Height = 100;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);
            
            txtName = new TextBox();
            txtName.Background = Brushes.Transparent;
            txtName.Text = airliner.Name;
            txtName.TextChanged += new TextChangedEventHandler(txtName_TextChanged);
           
            mainPanel.Children.Add(txtName);
            
            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 10, 0, 0);

            mainPanel.Children.Add(panelButtons);

            btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
      
            panelButtons.Children.Add(btnOk);

            Button btnReset = new Button();
            btnReset.Uid = "108";
            btnReset.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnReset.Height = Double.NaN;
            btnReset.Width = Double.NaN;
            btnReset.Content = Translator.GetInstance().GetString("General", btnReset.Uid);
            btnReset.Click += new RoutedEventHandler(btnReset_Click);
            btnReset.Margin = new Thickness(5, 0, 0, 0);
            btnReset.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnReset);
            Button btnCancel = new Button();
            btnCancel.Uid = "101";
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = Double.NaN;
            btnCancel.Width = Double.NaN;
            btnCancel.Content = Translator.GetInstance().GetString("General", btnCancel.Uid);
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnCancel);
            this.Content = mainPanel;
            //mainPanel.Margin = new Thickness(10, 10, 10, 10);
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
              
            btnOk.IsEnabled = txtName.Text.Trim().Length > 0;
        }
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtName.Text = this.Airliner.Airliner.TailNumber;
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = txtName.Text.Trim();
            this.Close();

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();

        }
    }
}
