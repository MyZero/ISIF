using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISIF.Model
{
    [Serializable]
    public class UserQuery
    {
        [Prompt("Please enter your {&}")]
        [Optional]
        public string AccountName { get; set; }
        [Prompt("Please enter your {&}")]

        public string requester { get; set; }
        [Prompt("Please enter your {&}")]
        [Optional]
        public string AD { get; set; }
        [Prompt("Please enter your {&}")]
        [Optional]
        public string VPN { get; set; }
    }
}