using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunokoLibrary.Web.GooglePlus;
using SunokoLibrary.Web.GooglePlus.Primitive;

namespace GPlusBrowser.ViewModel
{
    public abstract class AttachedContentViewModel : ViewModelBase
    {
        public async static Task<AttachedContentViewModel> Create(IAttachable model)
        {
            switch (model.Type)
            {
                case ContentType.Album:
                    var attachedAlbum = (AttachedAlbum)model;
                    return await AttachedAlbumViewModel.Create(attachedAlbum);
                case ContentType.Image:
                    var attachedImage = (AttachedImage)model;
                    return await AttachedImageViewModel.Create(attachedImage);
                case ContentType.Link:
                case ContentType.InteractiveLink:
                    var attachedLink = (AttachedLink)model;
                    return await AttachedLinkViewModel.Create(
                        attachedLink.Title,
                        string.IsNullOrEmpty(attachedLink.Summary) ? null : attachedLink.Summary.Trim('\n', '\r', ' '),
                        attachedLink.LinkUrl, attachedLink.FaviconUrl, attachedLink.OriginalThumbnailUrl);
                case ContentType.Reshare:
                    var attachedActivity = (AttachedPost)model;
                    return await AttachedActivityViewModel.Create(attachedActivity);
                default:
                    return null;
            }
        }
    }
}
