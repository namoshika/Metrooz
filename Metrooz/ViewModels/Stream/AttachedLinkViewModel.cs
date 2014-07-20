using Livet;
using Livet.Commands;
using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Metrooz.ViewModels
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
            set
            {
                _hasThumb = value;
                RaisePropertyChanged(() => HasThumb);
            }
        }
        public double ThumbWidth
        {
            get { return _thumbWidth; }
            set
            {
                _thumbWidth = value;
                RaisePropertyChanged(() => ThumbWidth);
            }
        }
        public double ThumbHeight
        {
            get { return _thumbHeight; }
            set
            {
                _thumbHeight = value;
                RaisePropertyChanged(() => ThumbHeight);
            }
        }
        public string AncourTitle
        {
            get { return _ancourTitle; }
            set
            {
                _ancourTitle = value;
                RaisePropertyChanged(() => AncourTitle);
            }
        }
        public string AncourIntroductionText
        {
            get { return _ancourIntroductionText; }
            set
            {
                _ancourIntroductionText = value;
                RaisePropertyChanged(() => AncourIntroductionText);
            }
        }
        public Uri AncourUrl
        {
            get { return _ancourUrl; }
            set
            {
                _ancourUrl = value;
                RaisePropertyChanged(() => AncourUrl);
            }
        }
        public ImageSource ThumnailUrl
        {
            get { return _thumnailUrl; }
            set
            {
                _thumnailUrl = value;
                RaisePropertyChanged(() => ThumnailUrl);
            }
        }

        public static async Task<AttachedLinkViewModel> Create(AttachedLink model)
        {
            var thmbImg = model.ThumbnailUrl == null ? null : await DataCacheDictionary.DownloadImage(new Uri(model.ThumbnailUrl.Replace("$SIZE_SEGMENT", "s100"))).ConfigureAwait(false);
            return new AttachedLinkViewModel(model.Title, model.Summary, model.LinkUrl, thmbImg);
        }
    }
}