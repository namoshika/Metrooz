using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPlusBrowser.ViewModel
{
    public class AttachedLinkViewModel : ViewModelBase
    {
        public AttachedLinkViewModel(
            string ancourTitle, string ancourIntroductionText, Uri ancourFaviconUrl, Uri ancourUrl,
            Uri thumnailUrl, System.Windows.Threading.Dispatcher dispatcher) : base(dispatcher)
        {
            AncourTitle = ancourTitle;
            AncourUrl = ancourUrl;
            ThumnailUrl = thumnailUrl;
            AncourFaviconUrl = ancourFaviconUrl;
            AncourIntroductionText = ancourIntroductionText;
        }
        string _ancourTitle;
        Uri _ancourUrl;
        Uri _thumnailUrl;
        Uri _ancourFaviconUrl;
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
        public Uri AncourFaviconUrl
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
        public Uri AncourUrl
        {
            get { return _ancourUrl; }
            set
            {
                _ancourUrl = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AncourUrl"));
            }
        }
        public Uri ThumnailUrl
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
    }
}
