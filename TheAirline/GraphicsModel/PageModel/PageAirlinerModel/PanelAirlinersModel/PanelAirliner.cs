using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlinerModel.PanelAirlinersModel
{
    public abstract class PanelAirliner : ScrollViewer
    {
        protected PageAirliners ParentPage;
        private StackPanel panelAirliner;
        public PanelAirliner(PageAirliners parent)
        {
            this.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.Height = GraphicsHelpers.GetContentHeight()-100;

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
            panelAirliner.Children.Add(element);
        }

        //creates the quick info panel for a airliner type
        public static StackPanel createQuickInfoPanel(AirlinerType airliner)
        {
            StackPanel quickInfoPanel = new StackPanel();

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PanelAirliner", txtHeader.Uid);

            quickInfoPanel.Children.Add(txtHeader);

            if (airliner.Image != null)
            {
                Image imgAirlinerMap = new Image();
                imgAirlinerMap.Width = GraphicsHelpers.GetContentWidth()-100;
                imgAirlinerMap.Source = new BitmapImage(new Uri(airliner.Image, UriKind.RelativeOrAbsolute));
                RenderOptions.SetBitmapScalingMode(imgAirlinerMap, BitmapScalingMode.HighQuality);

                quickInfoPanel.Children.Add(imgAirlinerMap);
            }

            ListBox lbQuickInfo = new ListBox();
            lbQuickInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbQuickInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            quickInfoPanel.Children.Add(lbQuickInfo);

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner", "1002"), UICreator.CreateTextBlock(airliner.Name)));

            ContentControl ccManufactorer = new ContentControl();
            ccManufactorer.SetResourceReference(ContentControl.ContentTemplateProperty, "ManufactorerCountryItem");
            ccManufactorer.Content = airliner.Manufacturer;

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner", "1003"), ccManufactorer));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner", "1004"), UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(airliner.Body, null, null, null).ToString())));

            string range = string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(airliner.Range), new StringToLanguageConverter().Convert("km."));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner", "1005"), UICreator.CreateTextBlock(string.Format("{1} ({0})", new TextUnderscoreConverter().Convert(airliner.RangeType), range))));

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner", "1006"), UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(airliner.Engine, null, null, null).ToString())));

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner", "1007"), UICreator.CreateTextBlock(new NumberMeterToUnitConverter().Convert(airliner.Wingspan).ToString())));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner", "1008"), UICreator.CreateTextBlock(new NumberMeterToUnitConverter().Convert(airliner.Length).ToString())));
            if (airliner.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
            {
                lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner", "1009"), UICreator.CreateTextBlock(((AirlinerPassengerType)airliner).MaxSeatingCapacity.ToString())));
                lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner", "1010"), UICreator.CreateTextBlock(((AirlinerPassengerType)airliner).MaxAirlinerClasses.ToString())));
            }
            if (airliner.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo)
            {
                
                lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner","1017"),UICreator.CreateTextBlock(new CargoSizeConverter().Convert(airliner).ToString())));
            }

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner", "1015"), UICreator.CreateTextBlock(new NumberMeterToUnitConverter().Convert(airliner.MinRunwaylength).ToString())));

            if (airliner.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
            {
                string crewRequirements = string.Format("Cockpit: {0} Cabin: {1}", airliner.CockpitCrew, ((AirlinerPassengerType)airliner).CabinCrew);
                lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner", "1011"), UICreator.CreateTextBlock(crewRequirements)));
            }
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner", "1012"), UICreator.CreateTextBlock(string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(airliner.CruisingSpeed), new StringToLanguageConverter().Convert("km/t")))));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner", "1013"), UICreator.CreateTextBlock(string.Format("{0:0.###} {1}", new FuelConsumptionToUnitConverter().Convert(airliner.FuelConsumption), new StringToLanguageConverter().Convert("l/seat/km")))));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1016"), UICreator.CreateTextBlock(string.Format("{0:0.#} {1}", new FuelUnitGtLConverter().Convert(airliner.FuelCapacity), new StringToLanguageConverter().Convert("gallons")))));


            string produced = string.Format("{0}-{1}", airliner.Produced.From > GameObject.GetInstance().GameTime ? "?" : airliner.Produced.From.ToShortDateString(), airliner.Produced.To > GameObject.GetInstance().GameTime ? "?" : airliner.Produced.To.ToShortDateString());

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelAirliner", "1014"), UICreator.CreateTextBlock(produced)));

            return quickInfoPanel;
        }

      
    }
}
