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
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirlinerConfiguration.xaml
    /// </summary>
    public partial class PopUpAirlinerConfiguration : PopUpWindow
    {
        public static int MaxSeats;
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
        
            DataTemplate dt = this.Resources["NewClassItem"] as DataTemplate;

   
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
            btnOk.Width = Double.NaN;
            btnOk.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.Content = "OK";
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = 16;
            btnCancel.Width = Double.NaN;
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
                lbClasses.Items.Add(new AirlinerClassItem(aClass, i == this.Classes.Count - 1 && i > 0));

                i++;
            }

            AirlinerClass.ClassType nextClass = this.Classes.Count < this.Classes[0].Airliner.Type.MaxAirlinerClasses ? this.Classes[this.Classes.Count - 1].Type + 1 : AirlinerClass.ClassType.Economy_Class;

            if (nextClass == AirlinerClass.ClassType.Business_Class)
            {
                MaxSeats = (int)(0.2 * Convert.ToDouble(this.Classes[0].Airliner.Type.MaxSeatingCapacity));


            }
            if (nextClass == AirlinerClass.ClassType.First_Class)
            {
                MaxSeats = (int)(0.1 * Convert.ToDouble(this.Classes[0].Airliner.Type.MaxSeatingCapacity));

            }
       
     
            lblNewClass.Visibility = this.Classes.Count < this.Classes[0].Airliner.Type.MaxAirlinerClasses ? Visibility.Visible : Visibility.Collapsed;
            lblNewClass.Content = nextClass;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            int seating = (int)((Button)sender).Tag;
            AirlinerClass.ClassType nextClass = this.Classes[this.Classes.Count - 1].Type + 1;
            AirlinerClass aClass = new AirlinerClass(this.Classes[0].Airliner, nextClass, seating);

            aClass.forceSetFacility(this.Classes[0].getFacility(AirlinerFacility.FacilityType.Audio));
            aClass.forceSetFacility(this.Classes[0].getFacility(AirlinerFacility.FacilityType.Seat));
            aClass.forceSetFacility(this.Classes[0].getFacility(AirlinerFacility.FacilityType.Video));


            this.Classes.Add(aClass);

            // chs, 2011-11-10 added so seat capacity is correctly calculated
            this.Classes[0].SeatingCapacity -= aClass.SeatingCapacity;
            this.Classes[0].RegularSeatingCapacity -= aClass.SeatingCapacity;

            showAirlinerClasses();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            AirlinerClass aClass = this.Classes[this.Classes.Count - 1];
            this.Classes.Remove(aClass);

            // chs, 2011-11-10 added so seat capacity is correctly calculated
            this.Classes[0].SeatingCapacity += aClass.RegularSeatingCapacity;
            this.Classes[0].RegularSeatingCapacity += aClass.RegularSeatingCapacity;

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
    public class SeatingCapacities : ObservableCollection<int>
    {
        public SeatingCapacities()
        {
           
            for (int i = 1; i < PopUpAirlinerConfiguration.MaxSeats; i++)
                Add(i + 1);
        }
    }
    //the converter for returning the amount of passengers
    public class NumberOfPassengersConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<int> list = new ObservableCollection<int>();
             for (int i = 1; i < PopUpAirlinerConfiguration.MaxSeats; i++)
                list.Add(i + 1);
           return list;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
   

}
    