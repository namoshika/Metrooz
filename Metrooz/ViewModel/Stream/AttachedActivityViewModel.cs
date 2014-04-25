using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SunokoLibrary.Web.GooglePlus;
using SunokoLibrary.Web.GooglePlus.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Metrooz.ViewModel
{
    public class AttachedActivityViewModel : AttachedContentViewModel
    {
        public AttachedActivityViewModel(string ownerName, Uri ownerProfileUrl, Uri ownerActivityUrl,
            ImageSource ownerIcon, StyleElement postContentInline, object attachedContent)
        {
            _ownerName = ownerName;
            _ownerProfileUrl = ownerProfileUrl;
            _ownerActivityUrl = ownerActivityUrl;
            _ownerIcon = ownerIcon;
            _postContentInline = postContentInline;
            _attachedContent = attachedContent;
        }
        string _ownerName;
        Uri _ownerProfileUrl;
        Uri _ownerActivityUrl;
        ImageSource _ownerIcon;
        StyleElement _postContentInline;
        object _attachedContent;

        public string OwnerName
        {
            get { return _ownerName; }
            set { Set(() => OwnerName, ref _ownerName, value); }
        }
        public Uri OwnerProfileUrl
        {
            get { return _ownerProfileUrl; }
            set { Set(() => OwnerProfileUrl, ref _ownerProfileUrl, value); }
        }
        public Uri OwnerActivityUrl
        {
            get { return _ownerActivityUrl; }
            set { Set(() => OwnerActivityUrl, ref _ownerActivityUrl, value); }
        }
        public ImageSource OwnerIcon
        {
            get { return _ownerIcon; }
            set { Set(() => OwnerIcon, ref _ownerIcon, value); }
        }
        public StyleElement PostContentInline
        {
            get { return _postContentInline; }
            set { Set(() => PostContentInline, ref _postContentInline, value); }
        }
        public object AttachedContent
        {
            get { return _attachedContent; }
            set { Set(() => AttachedContent, ref _attachedContent, value); }
        }

        public async static Task<AttachedActivityViewModel> Create(AttachedPost model)
        {
            var ownerIcon = await DataCacheDictionary.DownloadImage(new Uri(model.OwnerIconUrl.Replace("$SIZE_SEGMENT", "s25-c-k"))).ConfigureAwait(false);
            var attachedContent = model.AttachedContent != null ? await AttachedContentViewModel.Create(model.AttachedContent).ConfigureAwait(false) : null;
            return new AttachedActivityViewModel(model.OwnerName, null,
                model.LinkUrl, ownerIcon, model.ParsedText, attachedContent);
        }
    }
}
