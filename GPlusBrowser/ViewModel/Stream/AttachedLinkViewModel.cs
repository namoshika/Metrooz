using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
            string ancourTitle, string ancourIntroductionText, Uri ancourFaviconUrl, Uri ancourUrl, Uri thumnailUrl)
        {
            AncourTitle = ancourTitle;
            AncourUrl = ancourUrl;
            AncourIntroductionText = ancourIntroductionText;
            DataCacheDictionary.DownloadImage(thumnailUrl).ContinueWith(tsk => ThumnailUrl = tsk.Result);
            DataCacheDictionary.DownloadImage(ancourFaviconUrl).ContinueWith(tsk => AncourFaviconUrl = tsk.Result);
        }
        string _ancourTitle;
        Uri _ancourUrl;
        ImageSource _ancourFaviconUrl;
        ImageSource _thumnailUrl;
        string _ancourIntroductionText;

        public string AncourTitle
        {
            get { return _ancourTitle; }
            set { Set(() => AncourTitle, ref _ancourTitle, value); }
        }
        public string AncourIntroductionText
        {
            get { return _ancourIntroductionText; }
            set { Set(() => AncourIntroductionText, ref _ancourIntroductionText, value); }
        }
        public ImageSource AncourFaviconUrl
        {
            get { return _ancourFaviconUrl; }
            set { Set(() => AncourFaviconUrl, ref _ancourFaviconUrl, value); }
        }
        public ImageSource ThumnailUrl
        {
            get { return _thumnailUrl; }
            set { Set(() => ThumnailUrl, ref _thumnailUrl, value); }
        }
        public Uri AncourUrl
        {
            get { return _ancourUrl; }
            set { Set(() => AncourUrl, ref _ancourUrl, value); }
        }
    }
}