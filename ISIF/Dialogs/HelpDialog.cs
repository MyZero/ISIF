using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ISIF.Dialogs
{
    public class HelpDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("This is the Help Dialog.<br/> You can check account status here.<br/> Reply with anything to return to prior dialog.");

            context.Wait(this.MessageReceived);
        }

        private async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if ((message.Text != null) && (message.Text.Trim().Length > 0))
            {
                context.Done<object>(null);
            }
            else
            {
                context.Fail(new Exception("Message was not a string or was an empty string."));
            }
        }
    }
}