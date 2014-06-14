using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Threading.Tasks;

namespace Metrooz.ViewModel
{
    public class AttachedYouTubeViewModel : AttachedLinkViewModel
    {
        public AttachedYouTubeViewModel(string ancourTitle, string ancourIntroductionText, Uri ancourUrl, ImageSource thumbnail)
            : base(ancourTitle, ancourIntroductionText, ancourUrl, thumbnail) { }
        public static async Task<AttachedLinkViewModel> Create(AttachedYouTube model)
        {
            var thmbImg = model.ThumbnailUrl == null ? null : await DataCacheDictionary
                .DownloadImage(new Uri(model.ThumbnailUrl.Replace("$SIZE_SEGMENT", string.Format("w{0}-h{1}-n", model.ThumbnailWidth, model.ThumbnailHeight))))
                .ConfigureAwait(false);
            return new AttachedYouTubeViewModel(model.Title, model.Summary, model.LinkUrl, thmbImg);
        }
    }
}