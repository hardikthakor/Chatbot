using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using CallingApi;
using System.Net;

using Newtonsoft.Json;


namespace Catagory
{
    [Serializable]
    public class VoiceDialog : IDialog
    {
        public async Task StartAsync(IDialogContext context)
        {
            // await context.PostAsync("Welcome to the SJCET College Recommender BOT !!!");

           

        }

    }


}