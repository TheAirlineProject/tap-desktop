using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using TheAirlineV2.Model.AirlinerModel;
using TheAirlineV2.GraphicsModel.PageModel.GeneralModel;
using System.Windows;
using TheAirlineV2.GraphicsModel.Converters;

namespace TheAirlineV2.GraphicsModel.PageModel.PageAirlinerModel.PanelAirlinersModel
{
    public abstract class PanelAirliner : ScrollViewer
    {
        protected PageAirliners ParentPage;
        private StackPanel panelAirliner;
        public PanelAirliner(PageAirliners parent)
        {
            //ScrollViewer scroller = new ScrollViewer();
            this.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.Height = GraphicsHelpers.GetContentHeight();
            
            panelAirliner = new StackPanel();
            panelAirliner.Orientation = Orientation.Vertical;
           

            this.Content = panelAirliner;

            this.ParentPage = parent;

            this.Margin = new Thickness(0, 0, 50, 0);
        }
        //clears the panel
        public void clearPanel()
        {
            panelAirliner.Children.Clear();
        }
        //adds an element to the panel
        public void addObject(UIElement element)
        {
            //DockPanel.SetDock(element, Dock.Top);
            panelAirliner.Children.Add(element);
        }
        //creates the quick info panel for a airliner type
        protected StackPanel createQuickInfoPanel(AirlinerType airliner)
        {
            StackPanel quickInfoPanel = new StackPanel();

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Airliner Type Specifications";

            quickInfoPanel.Children.Add(txtHeader);


            ListBox lbQuickInfo = new ListBox();
            lbQuickInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbQuickInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            quickInfoPanel.Children.Add(lbQuickInfo);

            lbQuickInfo.Items.Add(new QuickInfoValue("Name", UICreator.CreateTextBlock(airliner.Name)));

            ContentControl ccManufactorer = new ContentControl();
            ccManufactorer.SetResourceReference(ContentControl.ContentTemplateProperty, "ManufactorerCountryItem");
            ccManufactorer.Content = airliner.Manufacturer;
 
            lbQuickInfo.Items.Add(new QuickInfoValue("Manufactorer",ccManufactorer));
            lbQuickInfo.Items.Add(new QuickInfoValue("Body type", UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(airliner.Body, null, null, null).ToString())));
            
            string range =  string.Format("{0:0.##} {1}",new NumberToUnitConverter().Convert(airliner.Range),new StringToLanguageConverter().Convert("km."));
            lbQuickInfo.Items.Add(new QuickInfoValue("Range type", UICreator.CreateTextBlock(string.Format("{0} ({1})",new TextUnderscoreConverter().Convert(airliner.RangeType),range))));
            
            lbQuickInfo.Items.Add(new QuickInfoValue("Engine type", UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(airliner.Engine, null, null, null).ToString())));
   
            //lbQuickInfo.Items.Add(new QuickInfoValue("Manufactorer", UICreator.CreateTextBlock(airliner.Manufacturer.Name)));
            lbQuickInfo.Items.Add(new QuickInfoValue("Wingspan", UICreator.CreateTextBlock(string.Format("{0} m.",airliner.Wingspan))));
            lbQuickInfo.Items.Add(new QuickInfoValue("Length", UICreator.CreateTextBlock(string.Format("{0} m.", airliner.Length))));
            lbQuickInfo.Items.Add(new QuickInfoValue("Passengers", UICreator.CreateTextBlock(airliner.MaxSeatingCapacity.ToString())));
            lbQuickInfo.Items.Add(new QuickInfoValue("Max airliner classes", UICreator.CreateTextBlock(airliner.MaxAirlinerClasses.ToString())));
            
            string crewRequirements = string.Format("Cockpit: {0} Cabin: {1}", airliner.CockpitCrew, airliner.CabinCrew);
            lbQuickInfo.Items.Add(new QuickInfoValue("Crew requirements", UICreator.CreateTextBlock(crewRequirements)));
         
            //lbQuickInfo.Items.Add(new QuickInfoValue("Cockpit crew capacity", UICreator.CreateTextBlock(airliner.CockpitCrew.ToString())));
           // lbQuickInfo.Items.Add(new QuickInfoValue("Range",
            lbQuickInfo.Items.Add(new QuickInfoValue("Cruising speed", UICreator.CreateTextBlock(string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(airliner.CruisingSpeed),new StringToLanguageConverter().Convert("km/t")))));
            lbQuickInfo.Items.Add(new QuickInfoValue("Fuel Consumption", UICreator.CreateTextBlock(string.Format("{0:0.###} {1}", new FuelConsumptionToUnitConverter().Convert(airliner.FuelConsumption), new StringToLanguageConverter().Convert("l/seat/km")))));
            lbQuickInfo.Items.Add(new QuickInfoValue("Produced", UICreator.CreateTextBlock(airliner.Produced.ToString())));


            return quickInfoPanel;

        }
    }
}
