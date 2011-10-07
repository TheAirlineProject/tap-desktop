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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.Converters;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirlinerConfiguration.xaml
    /// </summary>
    public partial class PopUpAirlinerConfiguration : PopUpWindow
    {
        private List<AirlinerClass> Classes;
        private ListBox lbClasses;
        private ContentControl lblNewClass;
        public static object ShowPopUp(Airliner airliner)
        {
            PopUpWindow window = new PopUpAirlinerConfiguration(airliner);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpAirlinerConfiguration(Airliner airliner)
        {
            this.Classes = new List<AirlinerClass>();

            foreach (AirlinerClass aClass in airliner.Classes)
            {
                this.Classes.Add(aClass);
            }

            InitializeComponent();

            this.Title = "Airliner Configuration";

            this.Width = 400;

            this.Height = 150;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;


            StackPanel panelClasses = new StackPanel();

            lbClasses = new ListBox();
            lbClasses.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbClasses.ItemTemplate = this.Resources["ClassItem"] as DataTemplate;


            panelClasses.Children.Add(lbClasses);

            lblNewClass = new ContentControl();
            lblNewClass.Margin = new Thickness(0, 5, 0, 0);
            lblNewClass.ContentTemplate = this.Resources["NewClassItem"] as DataTemplate;
            lblNewClass.Content = AirlinerClass.ClassType.First_Class;

            panelClasses.Children.Add(lblNewClass);



            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);

            Button btnOk = new Button();
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = 16;
            btnOk.Width = 50;
            btnOk.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.Content = "OK";
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
         
            panelButtons.Children.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = 16;
            btnCancel.Width = 100;
            btnCancel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.Content = "Cancel";
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
           
            panelButtons.Children.Add(btnCancel);

            panelClasses.Children.Add(panelButtons);

            this.Content = panelClasses;

            showAirlinerClasses();

           

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = this.Classes;
            this.Close();
        }
        //shows the airliner classes
        private void showAirlinerClasses()
        {
            lbClasses.Items.Clear();

            int i = 0;
            foreach (AirlinerClass aClass in this.Classes)
            {
                lbClasses.Items.Add(new AirlinerClassItem(aClass,  i==this.Classes.Count-1 && i>0 ));

                i++;
            }

            AirlinerClass.ClassType nextClass = this.Classes.Count < this.Classes[0].Airliner.Type.MaxAirlinerClasses ? this.Classes[this.Classes.Count - 1].Type + 1 : AirlinerClass.ClassType.Economy_Class;
   
            lblNewClass.Visibility = this.Classes.Count < this.Classes[0].Airliner.Type.MaxAirlinerClasses  ? Visibility.Visible : Visibility.Collapsed;
            lblNewClass.Content = nextClass;
        }
       
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            int seating= (int)((Button)sender).Tag;
            AirlinerClass.ClassType nextClass = this.Classes[this.Classes.Count - 1].Type + 1;
            AirlinerClass aClass = new AirlinerClass(this.Classes[0].Airliner, nextClass, seating);

            aClass.forceSetFacility(this.Classes[0].getFacility(AirlinerFacility.FacilityType.Audio));
            aClass.forceSetFacility(this.Classes[0].getFacility(AirlinerFacility.FacilityType.Seat));
            aClass.forceSetFacility(this.Classes[0].getFacility(AirlinerFacility.FacilityType.Video));


            this.Classes.Add(aClass);

            this.Classes[0].SeatingCapacity -= aClass.SeatingCapacity;

            showAirlinerClasses();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            AirlinerClass aClass = this.Classes[this.Classes.Count - 1];
            this.Classes.Remove(aClass);

            this.Classes[0].SeatingCapacity += aClass.RegularSeatingCapacity;

            showAirlinerClasses();
        }
        //the class for an airliner class item
        private class AirlinerClassItem
        {
            public AirlinerClass AirlinerClass { get; set; }
            public Boolean CanDelete { get; set; }
            public AirlinerClassItem(AirlinerClass aClass, Boolean canDelete)
            {
                this.AirlinerClass = aClass;
                this.CanDelete = canDelete;
            }
        }


      
    }
    public class SeatingCapacities : List<int>
    {
        public SeatingCapacities()
        {
            for (int i = 1; i < 20; i++)
                Add(i + 1);
        }
    }
   
}
