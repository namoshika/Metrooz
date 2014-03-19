using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GPlusBrowser.ViewModel
{
    public class DialogViewModel : ViewModelBase
    {
        public DialogViewModel(DialogMessage model, Action<DialogViewModel> callback)
        {
            _model = model;
            _callback = callback;
            MessageBoxText = model.Content;
            Caption = model.Caption;
            Button = model.Button;
            CommitCommand = new RelayCommand(Executed_CommitCommand);
        }
        DialogMessage _model;
        Action<DialogViewModel> _callback;
        string _messageBoxText;
        string _caption;
        System.Windows.MessageBoxButton _button;
        System.Windows.MessageBoxResult _result;

        public string MessageBoxText
        {
            get { return _messageBoxText; }
            set { Set(() => MessageBoxText, ref _messageBoxText, value); }
        }
        public string Caption
        {
            get { return _caption; }
            set { Set(() => Caption, ref _caption, value); }
        }
        public System.Windows.MessageBoxButton Button
        {
            get { return _button; }
            set { Set(() => Button, ref _button, value); }
        }
        public System.Windows.MessageBoxResult Result
        {
            get { return _result; }
            set { Set(() => Result, ref _result, value); }
        }
        public ICommand CommitCommand { get; private set; }

        void Executed_CommitCommand()
        {
            _callback(this);
            _model.Callback(Result);
        }
    }
}
