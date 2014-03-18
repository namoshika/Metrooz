using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SunokoLibrary.Web.GooglePlus;
using SunokoLibrary.Web.GooglePlus.Primitive;

namespace GPlusBrowser.ViewModel
{
    public abstract class NotificationViewModel : ViewModelBase
    {
        public NotificationViewModel() { }
        ImageSource _displayIconUrl;
        public ImageSource DisplayIconUrl
        {
            get { return _displayIconUrl; }
            set { Set(() => DisplayIconUrl, ref _displayIconUrl, value); }
        }
    }
}
