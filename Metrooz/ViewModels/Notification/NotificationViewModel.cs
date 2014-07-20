using Livet;
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

namespace Metrooz.ViewModels
{
    public abstract class NotificationViewModel : ViewModel
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
            set
            {
                _displayIconUrl = value;
                RaisePropertyChanged(() => DisplayIconUrl);
            }
        }
    }
    public class OtherTypeNotificationViewModel : NotificationViewModel
    {
        public OtherTypeNotificationViewModel(NotificationInfo info, DateTime insertTime, string message, bool isError)
            : base(insertTime)
        {
            _model = info;
            _message = message;
            _isError = isError;
        }
        bool _isError;
        string _message;
        NotificationInfo _model;
        public bool IsError { get { return _isError; } }
        public string Message { get { return _message; } }
    }
}
