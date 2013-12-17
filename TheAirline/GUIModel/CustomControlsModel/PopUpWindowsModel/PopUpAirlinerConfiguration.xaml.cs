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
using System.Collections.ObjectModel;
using System.ComponentModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GUIModel.HelpersModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirlinerConfiguration.xaml
    /// </summary>
    public partial class PopUpAirlinerConfiguration : PopUpWindow
    {
        public static int MaxSeats;
        public static List<AirlinerClass.ClassType> FreeClasses;
        private List<AirlinerClass> Classes;
        private ListBox lbClasses;
        private ContentControl lblNewClass;
        private AirlinerType Type;
        private AirlinerClass.ClassType CurrentClass;
        public static object ShowPopUp(Airliner airliner)
        {
            PopUpWindow window = new PopUpAirlinerConfiguration(airliner);
            window.ShowDialog();

            return window.Selected;
        }
          public static object ShowPopUp(AirlinerType airliner, List<AirlinerClass> classes)
        {
              
            PopUpWindow window = new PopUpAirlinerConfiguration(airliner,classes);
            window.ShowDialog();

            return window.Selected;
        }
       
        public PopUpAirlinerConfiguration(AirlinerType type, List<AirlinerClass> classes)
        {
            this.Classes = new List<AirlinerClass>(classes);
            
            this.Type = type;
          
            createPopUp();
        }
        public PopUpAirlinerConfiguration(Airliner airliner)
        {
          
            this.Classes = new List<AirlinerClass>();

            foreach (AirlinerClass aClass in airliner.Classes)
            {
                this.Classes.Add(aClass);
            }

            this.Type = airliner.Type;

            createPopUp();

        }
        //creates the pop up
        private void createPopUp()
        {
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
            btnOk.SetResourceReference(Button.StyleProperty, "StandardButtonStyle");
            btnOk.Height = 16;
            btnOk.Width = Double.NaN;
            btnOk.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.Content = "OK";
       
            panelButtons.Children.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.SetResourceReference(Button.StyleProperty, "StandardButtonStyle");
            btnCancel.Height = 16;
            btnCancel.Width = Double.NaN;
            btnCancel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.Content = "Cancel";
         
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

            FreeClasses = new List<AirlinerClass.ClassType>();

            int i = 0;
            foreach (AirlinerClass aClass in this.Classes)
            {
                lbClasses.Items.Add(new AirlinerClassItem(aClass, i == this.Classes.Count - 1 && i > 0));

                i++;
            }

            int maxCapacity = ((AirlinerPassengerType)this.Type).MaxSeatingCapacity;
        

            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                if (!this.Classes.Exists(c => c.Type == type) && ((int)type <= GameObject.GetInstance().GameTime.Year))
                    FreeClasses.Add(type);
            }
            
            AirlinerClass.ClassType nextClass = FreeClasses.Count > 0 ?  FreeClasses[0] : AirlinerClass.ClassType.Economy_Class;

            MaxSeats = maxCapacity - this.Classes.Count;

            lblNewClass.Visibility = this.Classes.Count < ((AirlinerPassengerType)this.Type).MaxAirlinerClasses && FreeClasses.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            lblNewClass.Content = nextClass;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            int seating = (int)((Button)sender).Tag;

              AirlinerClass aClass = new AirlinerClass(this.CurrentClass, seating);

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
        private void cbClasses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedItem!=null)
                this.CurrentClass = (AirlinerClass.ClassType)((ComboBox)sender).SelectedItem;
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
    //the converter for the "free" classes
    public class FreeClassesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<AirlinerClass.ClassType> list = new ObservableCollection<AirlinerClass.ClassType>();
            foreach (AirlinerClass.ClassType type in PopUpAirlinerConfiguration.FreeClasses)
                 list.Add(type);
                  
            return list;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
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
    