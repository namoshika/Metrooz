using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    public abstract class ViewModelBase : System.ComponentModel.INotifyPropertyChanged
    {
        public ViewModelBase(Dispatcher uiThreadDispatcher) { UiThreadDispatcher = uiThreadDispatcher; }
        protected Dispatcher UiThreadDispatcher { get; private set; }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            VerifyPropertyName(e.PropertyName);
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (GetType().GetProperty(propertyName) == null)
            {
                string msg = "Invalid property name: " + propertyName;
                throw new Exception(msg);
            }
        }
    }
}
