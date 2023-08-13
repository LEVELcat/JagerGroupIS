using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotApp.Modules.DiscordElectionNotificateClasses
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    internal class NotificationCycleModule : BaseCommandModule
    {
        private static ElectionNotificateClassCycle notificateClassCycle = new ElectionNotificateClassCycle();

        [Command("notification")]
        public async Task NotificationCycleInvoke(CommandContext ctx, string value)
        {
            if(value != null)
            {
                if(value == "start")
                {
                    notificateClassCycle?.StartCycles();
                    ctx.Message.DeleteAsync();
                }
                else if(value == "end")
                {
                    notificateClassCycle?.EndCycles();
                    ctx.Message.DeleteAsync();
                }
            } 
        }

    }
}
