using SunokoLibrary.Threading;
using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrooz.Models
{
    using SunokoLibrary.Web.GooglePlus.Primitive;
    using SunokoLibrary.Web.GooglePlus.Utility;
    using SunokoLibrary.Threading;

    public class Account
    {
        public Account(IPlatformClientBuilder setting)
        {
            Builder = setting;
            Stream = new StreamManager(this);
            Notification = new NotificationManager(this);
        }
        readonly System.Threading.SemaphoreSlim _initSyncer = new System.Threading.SemaphoreSlim(1);

        public IPlatformClientBuilder Builder { get; private set; }
        public PlatformClient PlusClient { get; private set; }
        public ProfileInfo MyProfile { get; private set; }
        public StreamManager Stream { get; private set; }
        public NotificationManager Notification { get; private set; }

        public async Task<bool> Activate()
        {
            try
            {
                await _initSyncer.WaitAsync();
                //G+APIライブラリの初期化を行う
#if DEBUG
                PlusClient = await Builder.Build(new[] { new DefaultAccessor(new ApiWrapperWithLogger()) }).ConfigureAwait(false);
#else
                PlusClient = await Builder.Build(new[] { new DefaultAccessor(new ApiWrapper()) }).ConfigureAwait(false);
#endif
                MyProfile = await PlusClient.People.GetProfileOfMeAsync(false).ConfigureAwait(false);

                //各モジュールの初期化を行う
                await Notification.Activate();
                if (await Stream.Activate() == false)
                    return false;
                return true;
            }
            catch (FailToOperationException) { return false; }
            finally { _initSyncer.Release(); }
        }
        public async Task Deactivate()
        {
            try
            {
                await _initSyncer.WaitAsync().ConfigureAwait(false);

                //各モジュールを休止状態にする
                await Notification.Deactivate().ConfigureAwait(false);
                await Stream.Deactivate().ConfigureAwait(false);
                if (PlusClient != null)
                    PlusClient.Dispose();
            }
            finally { _initSyncer.Release(); }
        }
    }
}