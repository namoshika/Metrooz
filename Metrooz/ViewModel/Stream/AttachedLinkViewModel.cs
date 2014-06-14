using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Metrooz.ViewModel
{
    public class AttachedLinkViewModel : AttachedContentViewModel
    {
        public AttachedLinkViewModel(string ancourTitle, string ancourIntroductionText, Uri ancourUrl, ImageSource thumbnail)
        {
            _ancourTitle = ancourTitle;
            _ancourUrl = ancourUrl;
            _ancourIntroductionText = ancourIntroductionText;
            _hasThumb = thumbnail != null;
            _thumnailUrl = thumbnail;
            if (thumbnail != null)
            {
                _thumbWidth = thumbnail.Width;
                _thumbHeight = thumbnail.Height;
            }
        }
        bool _hasThumb;
        double _thumbWidth, _thumbHeight;
        string _ancourTitle;
        Uri _ancourUrl;
        ImageSource _thumnailUrl;
        string _ancourIntroductionText;

        public bool HasThumb
        {
            get { return _hasThumb; }
            set { Set(() => HasThumb, ref _hasThumb, value); }
        }
        public double ThumbWidth
        {
            get { return _thumbWidth; }
            set { Set(() => ThumbWidth, ref _thumbWidth, value); }
        }
        public double ThumbHeight
        {
            get { return _thumbHeight; }
            set { Set(() => ThumbHeight, ref _thumbHeight, value); }
        }
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
        public ImageSource ThumnailUrl
        {
            get { return _thumnailUrl; }
            set { Set(() => ThumnailUrl, ref _thumnailUrl, value); }
        }

        public static async Task<AttachedLinkViewModel> Create(AttachedLink model)
        {
            var thmbImg = model.ThumbnailUrl == null ? null : await DataCacheDictionary.DownloadImage(new Uri(model.ThumbnailUrl.Replace("$SIZE_SEGMENT", "s100"))).ConfigureAwait(false);
            return new AttachedLinkViewModel(model.Title, model.Summary, model.LinkUrl, thmbImg);
        }
    }
}