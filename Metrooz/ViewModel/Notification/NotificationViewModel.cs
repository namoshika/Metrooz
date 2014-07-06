using GalaSoft.MvvmLight;
using SunokoLibrary.Web.GooglePlus;
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

namespace Metrooz.ViewModel
{
    public abstract class NotificationViewModel : ViewModelBase
    {
        public NotificationViewModel(DateTime insertTime) { _insertTime = insertTime; }
        DateTime _insertTime;
        ImageSource _displayIconUrl;
        public bool IsEnableInsertAnime
        {
            get
            {
                var aa = DateTime.UtcNow - _insertTime < TimeSpan.FromMilliseconds(1000);
                return aa;
            }
        }
        public ImageSource DisplayIconUrl
        {
            get { return _displayIconUrl; }
            set { Set(() => DisplayIconUrl, ref _displayIconUrl, value); }
        }
    }
    public class UnknownTypeNotificationViewModel : NotificationViewModel
    {
        public UnknownTypeNotificationViewModel(NotificationInfo info, DateTime insertTime) : base(insertTime)
        { _model = info; }
        NotificationInfo _model;
    }
}
