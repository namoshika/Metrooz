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
            set
            {
                _ownerName = value;
                RaisePropertyChanged(() => OwnerName);
            }
        }
        public Uri OwnerProfileUrl
        {
            get { return _ownerProfileUrl; }
            set
            {
                _ownerProfileUrl = value;
                RaisePropertyChanged(() => OwnerProfileUrl);
            }
        }
        public Uri OwnerActivityUrl
        {
            get { return _ownerActivityUrl; }
            set
            {
                _ownerActivityUrl = value;
                RaisePropertyChanged(() => OwnerActivityUrl);
            }
        }
        public ImageSource OwnerIcon
        {
            get { return _ownerIcon; }
            set
            {
                _ownerIcon = value;
                RaisePropertyChanged(() => OwnerIcon);
            }
        }
        public StyleElement PostContentInline
        {
            get { return _postContentInline; }
            set
            {
                _postContentInline = value;
                RaisePropertyChanged(() => PostContentInline);
            }
        }
        public object AttachedContent
        {
            get { return _attachedContent; }
            set
            {
                _attachedContent = value;
                RaisePropertyChanged(() => AttachedContent);
            }
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
