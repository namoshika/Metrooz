using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using SunokoLibrary.Web.GooglePlus;
using SunokoLibrary.Web.GooglePlus.Primitive;
using System.Reactive.Linq;

namespace Metrooz.Model
{
    public class Activity : IDisposable
    {
        public Activity(ActivityInfo target)
        {
            Comments = new ObservableCollection<Comment>();
            CoreInfo = target;
            Initialize();
        }
        IDisposable _commReciever;
        IDisposable _actvReciever;

        public ActivityInfo CoreInfo { get; private set; }
        public ObservableCollection<Comment> Comments { get; private set; }
        public void Initialize()
        {
            _actvReciever = CoreInfo.GetUpdatedActivity().Subscribe(CoreInfo_Refreshed);
            _commReciever = CoreInfo.GetComments(false, true).Subscribe(CoreInfo_RefreshComment);
        }
        public async Task<bool> CommentPost(string content)
        {
            try { return await CoreInfo.PostComment(content).ConfigureAwait(false); }
            catch (FailToOperationException)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
                return false;
            }
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
            OnUpdated(new EventArgs());
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

        public event EventHandler Updated;
        protected virtual void OnUpdated(EventArgs e)
        {
            if (Updated != null)
                Updated(this, e);
        }
    }
}