using Livet.Behaviors.Messaging;
using Livet.Messaging;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interactivity;

namespace Metrooz.Controls
{
    public class MetroDialogMessageAction : InteractionMessageAction<FrameworkElement>
	{
        MahApps.Metro.Controls.MetroWindow _window;
        protected override void OnAttached()
        {
            base.OnAttached();
            _window = (MahApps.Metro.Controls.MetroWindow)Window.GetWindow(AssociatedObject);
        }
        protected override void InvokeAction(InteractionMessage message)
        {
            var confirmMessage = message as MetroDialogMessage;
            if (confirmMessage != null)
                confirmMessage.Response = _window.ShowMessageAsync(
                    confirmMessage.Title, confirmMessage.Message, confirmMessage.Style, confirmMessage.Settings);
        }
	}
    public class MetroDialogMessage : ResponsiveInteractionMessage<Task<MessageDialogResult>>
    {
        public MetroDialogMessage(string title, string message, string messageKey = null, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings setting = null)
            : base(messageKey)
        {
            Title = title;
            Message = message;
            Style = style;
            Settings = setting;
        }
        public string Title { get; set; }
        public string Message { get; set; }
        public MessageDialogStyle Style { get; set; }
        public MetroDialogSettings Settings { get; set; }
        protected override Freezable CreateInstanceCore()
        { return new MetroDialogMessage(Title, Message, MessageKey, Style, Settings); }
    }
}