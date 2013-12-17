using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;

namespace TheAirline.GraphicsModel.UserControlModel.MessageBoxModel
{
    public enum WPFMessageBoxResult
    {
        Yes,
        No,
        Ok,
        Cancel,
        Close,
        Exit,
        Continue
    }
    public enum WPFMessageBoxButtons
    {
        YesNo,
        Ok,
        ContinueExit
    }
    public class MessageBoxViewModel : INotifyPropertyChanged
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

        public string Message
        {
            get { return ___Message; }
            set
            {
                if (___Message != value)
                {
                    ___Message = value;
                    NotifyPropertyChange("Message");
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
                        ___View.Result = WPFMessageBoxResult.Yes;
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
                        ___View.Result = WPFMessageBoxResult.No;
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
                        ___View.Result = WPFMessageBoxResult.Ok;
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
                        ___View.Result = WPFMessageBoxResult.Continue;
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
                        ___View.Result = WPFMessageBoxResult.Exit;
                        ___View.Close();
                    });
                return ___ExitCommand;
            }
        }





        public MessageBoxViewModel(WPFMessageBox view,
            string title, string message, WPFMessageBoxButtons buttons)
        {
            Title = title;
            Message = message;

            ___View = view;

            setButtonsVisibility(buttons);
        }

        public void NotifyPropertyChange(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        private void setButtonsVisibility(WPFMessageBoxButtons buttonOption)
        {
            switch (buttonOption)
            {
                case WPFMessageBoxButtons.YesNo:
                    OkVisibility = CancelVisibility = ContinueExitVisibility = Visibility.Collapsed;
                    break;
                case WPFMessageBoxButtons.Ok:
                    YesNoVisibility = CancelVisibility = ContinueExitVisibility = Visibility.Collapsed;
                    break;
                case WPFMessageBoxButtons.ContinueExit:
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
        string ___Message;


        WPFMessageBox ___View;

    }
}
