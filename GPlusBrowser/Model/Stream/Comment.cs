using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SunokoLibrary.GooglePlus;

namespace GPlusBrowser.Model
{
    public class Comment
    {
        public Comment(CommentInfo info) { Refresh(info); }
        public CommentInfo CommentInfo { get; private set; }
        public Uri OwnerIcon { get; private set; }
        public string OwnerName { get; private set; }
        public string CommentContent { get; private set; }
        public StyleElement CommentContentElement { get; private set; }

        public void Refresh(CommentInfo info)
        {
            CommentInfo = info;
            OwnerIcon = new Uri(info.Owner.IconImageUrlText.Replace("$SIZE_SEGMENT", "s25-c-k"));
            OwnerName = info.Owner.Name;
            CommentContent = info.Html;
            CommentContentElement = info.ParsedContent;

            OnRefreshed(new EventArgs());
        }

        public event EventHandler Refreshed;
        protected virtual void OnRefreshed(EventArgs e)
        {
            if (Refreshed != null)
                Refreshed(this, e);
        }
    }
}
