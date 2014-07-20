using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrooz.Models
{
    public class Activity : IDisposable
    {
        public Activity(ActivityInfo target)
        {
            Comments = new ObservableCollection<Comment>();
            CoreInfo = target;
        }
        IDisposable _commReciever;
        IDisposable _actvReciever;

        public ActivityInfo CoreInfo { get; private set; }
        public ObservableCollection<Comment> Comments { get; private set; }
        public async Task<bool> Activate()
        {
            try
            {
                await CoreInfo.UpdateGetActivityAsync(false, ActivityUpdateApiFlag.GetActivities);
                _actvReciever = CoreInfo.GetUpdatedActivity().Subscribe(CoreInfo_Refreshed);
                _commReciever = CoreInfo.GetComments(false, true).Subscribe(CoreInfo_RefreshComment);
                return true;
            }
            catch (FailToOperationException) { return false; }
        }
        public async Task<bool> CommentPost(string content)
        {
            try { return await CoreInfo.PostComment(content); }
            catch (FailToOperationException) { return false; }
        }
        public void Dispose()
        {
            if (_actvReciever != null)
                _actvReciever.Dispose();
            if (_commReciever != null)
                _commReciever.Dispose();
        }
        async void CoreInfo_Refreshed(ActivityInfo newValue)
        {
            await CoreInfo.UpdateGetActivityAsync(false, ActivityUpdateApiFlag.GetActivities).ConfigureAwait(false);
            Updated(this, new EventArgs());
        }
        void CoreInfo_RefreshComment(CommentInfo comment)
        {
            lock (Comments)
            {
                var item = Comments.FirstOrDefault(cmme => cmme.CommentInfo.Id == comment.Id);
                var itemIdx = Comments.IndexOf(item);
                switch (comment.Status)
                {
                    case PostStatusType.Removed:
                        if (item != null)
                            Comments.RemoveAt(itemIdx);
                        break;
                    case PostStatusType.First:
                    case PostStatusType.Edited:
                        var idx = 0;
                        while (idx < Comments.Count && comment.PostDate > Comments[idx].CommentInfo.PostDate) idx++;
                        if (item != null)
                        {
                            item.Refresh(comment);
                            Comments.Move(itemIdx, idx);
                        }
                        else
                            Comments.Insert(idx, new Comment(comment));
                        break;
                }
            }
        }

        public event EventHandler Updated = (sender, e) => { };
    }
}