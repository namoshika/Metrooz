﻿using GalaSoft.MvvmLight;
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
}
