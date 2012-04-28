using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using SunokoLibrary.GooglePlus;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class CircleSelecterViewModel : ViewModelBase
    {
        public CircleSelecterViewModel(CircleInfo info, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _circleModel = info;
            _circleName = info.Name;
        }
        bool _isSelected;
        string _circleName;
        CircleInfo _circleModel;
        System.Windows.Input.ICommand _clipCommand;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsSelected"));
            }
        }
        public CircleInfo Circle
        {
            get { return _circleModel; }
            set
            {
                _circleModel = value;
                CircleName = value.Name;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Circle"));
            }
        }
        public string CircleName
        {
            get { return _circleName; }
            set
            {
                _circleName = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("CircleName"));
            }
        }
        public System.Windows.Input.ICommand ClipCommand
        {
            get { return _clipCommand; }
            set
            {
                _clipCommand = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ClipCommand"));
            }
        }
    }
}