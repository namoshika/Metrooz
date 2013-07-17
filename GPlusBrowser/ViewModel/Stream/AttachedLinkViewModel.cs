using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace GPlusBrowser.ViewModel
{
    public class AttachedLinkViewModel : ViewModelBase
    {
        public AttachedLinkViewModel(
            string ancourTitle, string ancourIntroductionText, Uri ancourFaviconUrl, Uri ancourUrl,
            Uri thumnailUrl, AccountViewModel topLevel, System.Windows.Threading.Dispatcher dispatcher)
            : base(dispatcher, topLevel)
        {
            AncourTitle = ancourTitle;
            AncourUrl = ancourUrl;
            AncourIntroductionText = ancourIntroductionText;
            topLevel.DataCacheDict.DownloadImage(thumnailUrl).ContinueWith(tsk => ThumnailUrl = tsk.Result);
            topLevel.DataCacheDict.DownloadImage(ancourFaviconUrl).ContinueWith(tsk => AncourFaviconUrl = tsk.Result);
        }
        string _ancourTitle;
        Uri _ancourUrl;
        ImageSource _ancourFaviconUrl;
        ImageSource _thumnailUrl;
        string _ancourIntroductionText;

        public bool ExistAncourIntroductionText { get; private set; }
        public bool ExistAncourFaviconUrl { get; private set; }
        public bool ExistThumnailUrl { get; private set; }
        public string AncourTitle
        {
            get { return _ancourTitle; }
            set
            {
                _ancourTitle = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AncourTitle"));
            }
        }
        public string AncourIntroductionText
        {
            get { return _ancourIntroductionText; }
            set
            {
                _ancourIntroductionText = value;
                ExistAncourIntroductionText = string.IsNullOrEmpty(value);
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AncourIntroductionText"));
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ExistAncourIntroductionText"));
            }
        }
        public ImageSource AncourFaviconUrl
        {
            get { return _ancourFaviconUrl; }
            set
            {
                _ancourFaviconUrl = value;
                ExistAncourFaviconUrl = value != null;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AncourFaviconUrl"));
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ExistAncourFaviconUrl"));
            }
        }
        public ImageSource ThumnailUrl
        {
            get { return _thumnailUrl; }
            set
            {
                _thumnailUrl = value;
                ExistThumnailUrl = value != null;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ThumnailUrl"));
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ExistThumnailUrl"));
            }
        }
        public Uri AncourUrl
        {
            get { return _ancourUrl; }
            set
            {
                _ancourUrl = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AncourUrl"));
            }
        }
    }
}