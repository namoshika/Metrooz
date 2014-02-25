﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace GPlusBrowser.ViewModel
{
    public class AttachedLinkViewModel : AttachedContentViewModel
    {
        public AttachedLinkViewModel(string ancourTitle, string ancourIntroductionText, Uri ancourUrl, BitmapImage ancourFaviconUrl, BitmapImage thumnailUrl)
        {
            _ancourTitle = ancourTitle;
            _ancourUrl = ancourUrl;
            _ancourIntroductionText = ancourIntroductionText;
            _thumnailUrl = thumnailUrl;
            _ancourFaviconUrl = ancourFaviconUrl;
        }
        string _ancourTitle;
        Uri _ancourUrl;
        BitmapImage _ancourFaviconUrl;
        BitmapImage _thumnailUrl;
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
        public Uri AncourUrl
        {
            get { return _ancourUrl; }
            set { Set(() => AncourUrl, ref _ancourUrl, value); }
        }
        public BitmapImage AncourFaviconUrl
        {
            get { return _ancourFaviconUrl; }
            set { Set(() => AncourFaviconUrl, ref _ancourFaviconUrl, value); }
        }
        public BitmapImage ThumnailUrl
        {
            get { return _thumnailUrl; }
            set { Set(() => ThumnailUrl, ref _thumnailUrl, value); }
        }

        public static async Task<AttachedLinkViewModel> Create(string ancourTitle, string ancourIntroductionText, Uri ancourUrl, Uri ancourFaviconUrl, Uri thumnailUrl)
        {
            var aa = await DataCacheDictionary.DownloadImage(ancourFaviconUrl).ConfigureAwait(false);
            var bb = await DataCacheDictionary.DownloadImage(thumnailUrl).ConfigureAwait(false);
            return new AttachedLinkViewModel(ancourTitle, ancourIntroductionText, ancourUrl, aa, bb);
        }
    }
}