using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metrooz.Models
{
    public class Comment
    {
        public Comment(CommentInfo info) { CommentInfo = info; }
        public CommentInfo CommentInfo { get; private set; }
        public void Refresh(CommentInfo info)
        {
            CommentInfo = info;
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