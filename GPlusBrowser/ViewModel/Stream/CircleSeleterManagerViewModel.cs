using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using SunokoLibrary.GooglePlus;

namespace GPlusBrowser.ViewModel
{
    using GPlusBrowser.Model;

    public class CircleSeleterManagerViewModel : ViewModelBase
    {
        public CircleSeleterManagerViewModel(StreamManager manager, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _manager = manager;
            _manager.Initialized += _manager_Initialized;
            _manager.ChangedSelectedCircleIndex += _manager_ChangedSelectedCircleIndex;
            _selectedCircleIndex = -1;
            _displayCircleSelecter = false;
            Items = new ObservableCollection<CircleSelecterViewModel>();
        }

        StreamManager _manager;
        bool _displayCircleSelecter;
        int _selectedCircleIndex;
        public ObservableCollection<CircleSelecterViewModel> Items { get; set; }
        public bool DisplayCircleSelecter
        {
            get { return _displayCircleSelecter; }
            set
            {
                _displayCircleSelecter = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("DisplayCircleSelecter"));
            }
        }
        public int SelectedCircleIndex
        {
            get { return _selectedCircleIndex; }
            set
            {
                if (_selectedCircleIndex >= 0)
                    Items[_selectedCircleIndex].IsSelected = false;
                if (value >= 0)
                    Items[value].IsSelected = true;
                _selectedCircleIndex = value;
                _manager.SelectedCircleIndex = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SelectedCircleIndex"));
            }
        }

        void _manager_ChangedSelectedCircleIndex(object sender, EventArgs e)
        {
            UiThreadDispatcher.BeginInvoke((Action)(() =>
            {
                if (SelectedCircleIndex != _manager.SelectedCircleIndex)
                    SelectedCircleIndex = _manager.SelectedCircleIndex;
            }), DispatcherPriority.ContextIdle);
        }
        void _manager_Initialized(object sender, EventArgs e)
        {
            SelectedCircleIndex = -1;
            Items.ClearAsync(UiThreadDispatcher);
            foreach (CircleInfo item in _manager.CircleStreams.Select(strm => (CircleInfo)strm.Reader))
                Items.AddAsync(new CircleSelecterViewModel(item, UiThreadDispatcher), UiThreadDispatcher);
        }
    }
    public class circleSelecterBoolToLeftDouble : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.FirstOrDefault(val => val == System.Windows.DependencyProperty.UnsetValue) != null)
                return 0;

            var leftOffset = (double)values.First(val => val is double);
            var display = (bool)values.First(val => val is bool);
            return display ? 0 : -leftOffset;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
}