using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Infrastructure.Events;
using TheAirline.Models.General;

namespace TheAirline.ViewModels
{
    [Export]
    public class MainWindowViewModel : BindableBase
    {
        private readonly DelegateCommand<object> _closeCommand;
        private readonly IEventAggregator _events;

        [ImportingConstructor]
        public MainWindowViewModel(IEventAggregator eventAggregator)
        {
            _events = eventAggregator;
            _closeCommand = new DelegateCommand<object>(CloseGame);
        }

        public void CloseGame(object ignored)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1003"), Translator.GetInstance().GetString("MessageBox", "1003", "message"),
                                                                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
                _events.GetEvent<CloseGameEvent>();
        }

        public ICommand CloseCommand => _closeCommand;
    }
}
