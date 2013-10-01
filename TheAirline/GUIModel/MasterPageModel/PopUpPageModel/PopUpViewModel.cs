using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;

namespace TheAirline.GUIModel.MasterPageModel.PopUpPageModel
{
    public enum WPFPopUpResult
    {
        Yes,
        No,
        Ok,
        Cancel,
        Close,
        Exit,
        Continue
    }
    public enum WPFPopUpButtons
    {
        YesNo,
        Ok,
        ContinueExit
    }
    public class PopUpViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title
        {
            get { return ___Title; }
            set
            {
                if (___Title != value)
                {
                    ___Title = value;
                    NotifyPropertyChange("Title");
                }
            }
        }

        public DataTemplate Content
        {
            get { return ___Content; }
            set
            {
                if (___Content != value)
                {
                    ___Content = value;
                    NotifyPropertyChange("Content");
                }
            }
        }
        public Visibility ContinueExitVisibility
        {
            get { return ___ContinueExitVisibility; }
            set
            {
                if (___ContinueExitVisibility != value)
                {
                    ___ContinueExitVisibility = value;
                    NotifyPropertyChange("ContinueExitVisibility");
                }
            }
        }

        public Visibility YesNoVisibility
        {
            get { return ___YesNoVisibility; }
            set
            {
                if (___YesNoVisibility != value)
                {
                    ___YesNoVisibility = value;
                    NotifyPropertyChange("YesNoVisibility");
                }
            }
        }

        public Visibility CancelVisibility
        {
            get { return ___CancelVisibility; }
            set
            {
                if (___CancelVisibility != value)
                {
                    ___CancelVisibility = value;
                    NotifyPropertyChange("CancelVisibility");
                }
            }
        }

        public Visibility OkVisibility
        {
            get { return ___OKVisibility; }
            set
            {
                if (___OKVisibility != value)
                {
                    ___OKVisibility = value;
                    NotifyPropertyChange("OkVisibility");
                }
            }
        }

        public ICommand YesCommand
        {
            get
            {
                if (___YesCommand == null)
                    ___YesCommand = new DelegateCommand(() =>
                    {
                        ___View.Result = WPFPopUpResult.Yes;
                        ___View.Close();
                    });
                return ___YesCommand;
            }
        }

        public ICommand NoCommand
        {

            get
            {
                if (___NoCommand == null)
                    ___NoCommand = new DelegateCommand(() =>
                    {
                        ___View.Result = WPFPopUpResult.No;
                        ___View.Close();
                    });
                return ___NoCommand;
            }
        }

        public ICommand OkCommand
        {
            get
            {
                if (___OKCommand == null)
                    ___OKCommand = new DelegateCommand(() =>
                    {
                        ___View.Result = WPFPopUpResult.Ok;
                        ___View.Close();
                    });
                return ___OKCommand;
            }
        }
        public ICommand ContinueCommand
        {
            get
            {
                if (___ContinueCommand == null)
                    ___ContinueCommand = new DelegateCommand(() =>
                    {
                        ___View.Result = WPFPopUpResult.Continue;
                        ___View.Close();
                    });
                return ___ContinueCommand;
            }
        }
        public ICommand ExitCommand
        {
            get
            {
                if (___ExitCommand == null)
                    ___ExitCommand = new DelegateCommand(() =>
                    {
                        ___View.Result = WPFPopUpResult.Exit;
                        ___View.Close();
                    });
                return ___ExitCommand;
            }
        }





        public PopUpViewModel(WPFRegularPopUp view,
            string title, DataTemplate content, WPFPopUpButtons buttons)
        {
            Title = title;
            Content = content;

            ___View = view;

            setButtonsVisibility(buttons);
        }

        public void NotifyPropertyChange(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        private void setButtonsVisibility(WPFPopUpButtons buttonOption)
        {
            switch (buttonOption)
            {
                case WPFPopUpButtons.YesNo:
                    OkVisibility = CancelVisibility = ContinueExitVisibility = Visibility.Collapsed;
                    break;
                case WPFPopUpButtons.Ok:
                    YesNoVisibility = CancelVisibility = ContinueExitVisibility = Visibility.Collapsed;
                    break;
                case WPFPopUpButtons.ContinueExit:
                    OkVisibility = CancelVisibility = YesNoVisibility = Visibility.Collapsed;
                    break;
                default:
                    OkVisibility = CancelVisibility = YesNoVisibility = Visibility.Collapsed;
                    break;
            }
        }


        Visibility ___ContinueExitVisibility;
        Visibility ___YesNoVisibility;
        Visibility ___OKVisibility;
        Visibility ___CancelVisibility;

        ICommand ___YesCommand;
        ICommand ___NoCommand;
        ICommand ___OKCommand;
        ICommand ___ContinueCommand;
        ICommand ___ExitCommand;



        string ___Title;
        DataTemplate ___Content;


        WPFRegularPopUp ___View;

    }
}
