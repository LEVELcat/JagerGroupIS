using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;

namespace DiscordBotApp.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    internal class StatisticModule : BaseCommandModule
    {
        [Command("stat")]
        public async Task GetStatistic(CommandContext ctx, params string[] values)
        {
            
        }

    }


}
