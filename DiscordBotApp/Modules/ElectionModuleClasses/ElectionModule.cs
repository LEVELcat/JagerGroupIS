using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using DSharpPlus.Interactivity;
using System.Net.Http.Headers;
using System.Collections;

namespace DiscordBotApp.Modules.ElectionModuleClasses
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    internal partial class ElectionModule : BaseCommandModule
    {
        [Command("event")]
        public async Task EventCommandInvoke(CommandContext ctx)
        {
            new ElectionFactory().CreateElectionAsync(ctx);
        }
    }
}
