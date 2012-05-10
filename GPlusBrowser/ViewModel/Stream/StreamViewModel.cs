using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using SunokoLibrary.GooglePlus;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public abstract class StreamViewModel : ViewModelBase, IDisposable
    {
        public StreamViewModel(Dispatcher uiThreadDispatcher) : base(uiThreadDispatcher) { }
        int _order;
        string _name;

        public int Order
        {
            get { return _order; }
            set
            {
                _order = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Order"));
            }
        }
        public string CircleName
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("CircleName"));
            }
        }

        public abstract void Dispose();
    }
}