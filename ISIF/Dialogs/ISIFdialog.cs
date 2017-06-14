
using ISIF.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;

namespace ISIF.Dialogs
{
    [LuisModel("0a55e58f-ac75-44c6-8c3d-069a3cce4b26", "50f234d8ab7f4e5c88aa536006a44bcb")]
    [Serializable]
    public class ISIFdialog : LuisDialog<object>
    {
        
        public const string Entity_AccountName = "Entity_AccountName";
        public const string Entity_Type_VPN = "Type::VPN";
        public const string Entity_Type_AD = "Type::AD";


        [LuisIntent("ResetPassword")]
        public async Task ResetPassword(IDialogContext context, LuisResult result)
        {
        }

        [LuisIntent("CheckAccountStatus")]
        public async Task CheckAccountStatus(IDialogContext context, IAwaitable<IMessageActivity> activity,LuisResult result)
        {
            var message = await activity;
            await context.PostAsync($"Welcome to account status check service ! We are analyzing your message: '{message.Text}'...");
            var userquery = new UserQuery();
            EntityRecommendation info;
            if (result.TryFindEntity(Entity_Type_VPN, out info))
            {
                info.Type = "VPN";
                
            }
            else if (result.TryFindEntity(Entity_Type_AD, out info))
            {
                info.Type = "AD";
            }


            if(info.Type=="AD")
            {
                var CheckStatus = new FormDialog<UserQuery>(userquery, this.BuildADForm, FormOptions.PromptInStart, result.Entities);

                context.Call(CheckStatus, this.ResumeAfterBuildQueryForm);

            }
            else if(info.Type=="VPN")
            {
                var CheckStatus = new FormDialog<UserQuery>(userquery, this.BuildVPNForm, FormOptions.PromptInStart, result.Entities);

                context.Call(CheckStatus, this.ResumeAfterBuildQueryForm);
            }
            else
            {
                
            }

        }



        private IForm<UserQuery> BuildVPNForm()
        {
            OnCompletionAsyncDelegate<UserQuery> processQuery = async (context, state) =>
            {
                var message = "Checking for account status :";
                if (!string.IsNullOrEmpty(state.VPN))
                {
                    message += $" {state.VPN.ToUpper()}...";
                }

                await context.PostAsync(message);
            };
            return new FormBuilder<UserQuery>()
                .Field(nameof(UserQuery.VPN), (state) => string.IsNullOrEmpty(state.VPN))
                .Field(nameof(UserQuery.accountName), (state) => string.IsNullOrEmpty(state.accountName))
                .OnCompletion(processQuery)
                .Build();
        }

        private async Task ResumeAfterBuildQueryForm(IDialogContext context, IAwaitable<UserQuery> result)
        {
            try
            {
                var searchQuery = await result;

                var operInfo = await this.GetOptionsAsync(searchQuery);

                await context.PostAsync($"your account {operInfo.accountName } in {operInfo.domain} domain's status is.....");
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally { context.Done<object>(null); }
        }

        private async Task<OperationInfo> GetOptionsAsync(UserQuery searchQuery)
        {
            var info = new OperationInfo()
            {
                domain = $"{searchQuery.AD ?? searchQuery.VPN}",
                accountName = searchQuery.accountName
            };
            return info;
        }

        private IForm<UserQuery> BuildADForm()
        {
            OnCompletionAsyncDelegate<UserQuery> processQuery = async (context, state) =>
            {
                var message = "Checking for account status :";
                if (!string.IsNullOrEmpty(state.AD))
                {
                    message += $" {state.AD.ToUpper()}...";
                }               

                await context.PostAsync(message);
            };
            return new FormBuilder<UserQuery>()
                .Field(nameof(UserQuery.AD), (state) => string.IsNullOrEmpty(state.AD))
                .Field(nameof(UserQuery.accountName), (state) => string.IsNullOrEmpty(state.accountName))
                .OnCompletion(processQuery)
                .Build();
        }

        [LuisIntent("UnlockAccount")]
        public async Task UnlockAccount(IDialogContext context, LuisResult result)
        {
        }

    }
}