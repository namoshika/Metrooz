using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using SunokoLibrary.Web.GooglePlus;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class StreamErrorPanelViewModel : ViewModelBase
    {
        public StreamErrorPanelViewModel(StreamManager ownerModel)
        {
            _ownerStreamModel = ownerModel;
            ReconnectCommand = new RelayCommand(ReconnectCommand_Executed);
        }
        StreamManager _ownerStreamModel;
        public ICommand ReconnectCommand { get; private set; }
        void ReconnectCommand_Executed() { _ownerStreamModel.Reconnect(); }
    }
}
