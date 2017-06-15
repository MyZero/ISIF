
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

        public const string Entity_AccountName = "AccountName";
        public const string Entity_Type_VPN = "Type::VPN";
        public const string Entity_Type_AD = "Type::AD";
        string[] options = { "VPN", "SONY Account" };

        [LuisIntent("ResetPassword")]
        public async Task ResetPassword(IDialogContext context, LuisResult result)
        {
        }

        [LuisIntent("CheckAccountStatus")]
        public async Task CheckAccountStatus(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
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


            if (info != null && info.Type == "AD")
            {
                var CheckStatus = new FormDialog<UserQuery>(userquery, this.BuildADForm, FormOptions.PromptInStart, result.Entities);

                context.Call(CheckStatus, this.ResumeAfterBuildQueryForm);

            }
            else if (info != null && info.Type == "VPN")
            {
                var CheckStatus = new FormDialog<UserQuery>(userquery, this.BuildVPNForm, FormOptions.PromptInStart, result.Entities);

                context.Call(CheckStatus, this.ResumeAfterBuildQueryForm);
            }
            else
            {
                result.TryFindEntity(Entity_AccountName, out info);
                if (info != null && info.Type == Entity_AccountName)
                {
                    context.PrivateConversationData.SetValue(info.Type, info.Entity);

                    PromptDialog.Choice(context, AfterAccountAsync, options, "Which **domain** would you want to check?", "Sorry I don't understand that, please try again", promptStyle: PromptStyle.PerLine);
                }
                else
                {
                    PromptDialog.Text(context, AfterQueryAsync, "Could you please provide your ID?", "Sorry this is not a valid **Account ID**, please try again");
                }
            }

        }



        private async Task AfterQueryAsync(IDialogContext context, IAwaitable<string> result)
        {
            var ID = await result;
            if (Regex.IsMatch(ID, "^\\d{8}$|^\\d{10}$"))
            {
                context.PrivateConversationData.SetValue(Entity_AccountName, ID);
                PromptDialog.Choice(context, AfterAccountAsync, options, "Which **domain** would you want to check?", "Sorry I don't understand that, please try again", promptStyle: PromptStyle.PerLine);
            }
            else
            {
                PromptDialog.Text(context, AfterQueryAsync, "Could you please provide your ID?", "Sorry this is not a valid **Account ID**, please try again");
            }
        }

        private async Task AfterAccountAsync(IDialogContext context, IAwaitable<string> result)
        {
            var domain = await result;
            string accountName;
            context.PrivateConversationData.TryGetValue(Entity_AccountName, out accountName);
            if (domain == "VPN")
            {

                var message = "Checking for account status : VPN ID is:" + accountName;
                await context.PostAsync(message);

            }
            else if (domain == "SONY Account")
            {
                var message = "Checking for account status : SONY Account ID is:" + accountName;
                await context.PostAsync(message);
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
                .Field(nameof(UserQuery.AccountName), (state) => string.IsNullOrEmpty(state.AccountName))
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
                accountName = searchQuery.AccountName
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
                .Field(nameof(UserQuery.AccountName), (state) => string.IsNullOrEmpty(state.AccountName))
                .OnCompletion(processQuery)
                .Build();
        }

        [LuisIntent("UnlockAccount")]
        public async Task UnlockAccount(IDialogContext context, LuisResult result)
        {
        }

    }
}