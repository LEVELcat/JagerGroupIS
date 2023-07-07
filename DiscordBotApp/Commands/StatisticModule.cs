using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DbLibrary.StatisticModel;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotApp.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    internal class StatisticModule : BaseCommandModule
    {
        [Command("stat")]
        public async Task GetStatistic(CommandContext ctx, params string[] values)
        {
            MakeResponse(ctx, values);
        }

        private async void MakeResponse(CommandContext ctx, params string[] values)
        {
            DiscordMessageBuilder message;

            DiscordChannel channel = ctx.Channel;

            if (ulong.TryParse(values[0], out ulong steam64ID) == true)
            {
                message = await GetPersonalStatMessage(steam64ID);
                ctx.RespondAsync(message);
            }
        }

        private async Task<DiscordMessageBuilder> GetPersonalStatMessage(ulong Steam64ID)
        {
            DiscordMessageBuilder discordMessage = new DiscordMessageBuilder() { Content = "Что то пошло не так"};

            using (StatisticDbContext dbContext = new StatisticDbContext())
            {
                try
                {
                    SteamProfile steamProfile = await dbContext.SteamProfiles.SingleOrDefaultAsync(s => s.SteamID64 == Steam64ID);

                    if (steamProfile == null)
                    {
                        discordMessage = new DiscordMessageBuilder() { Content = "Пользователь с таким Steam64ID на найден" };
                    }
                    else
                    {
                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

                        var statistic = (from stats in steamProfile.MatchStats
                                         select new
                                         {
                                             PlayTime = stats.PlayTime,
                                             Kills = stats.Kills,
                                             Deaths = stats.Deaths,
                                             Combat = stats.Combat,
                                             Offensive = stats.Offensive,
                                             Defense = stats.Defense,
                                             Support = stats.Support
                                         }).ToArray();

                        embedBuilder.AddField("Игрок", steamProfile.SteamName, true)
                                    .AddField("Матчей", steamProfile.MatchStats.Count().ToString(), true)
                                    .AddField("Среднее время игры", statistic.Average(m => m.PlayTime) is double num ? $"{(num / 60).ToString("0.0")} Минут" : "Нет данных", true)
                                    .AddField("Всего убийств", statistic.Sum(m => m.Kills) is int num1? num1.ToString() : "Нет данных", true)
                                    .AddField("Всего смертей", statistic.Sum(m => m.Deaths) is int num2? num2.ToString() : "Нет данных", true)
                                    .AddField("Среднее очков атаки", statistic.Average(m => m.Combat) is double num3? num3.ToString("0.0") : "Нет данных", true)
                                    .AddField("Среднее очков наступления", statistic.Average(m => m.Offensive) is double num4? num4.ToString("0.0") : "Нет данных", true)
                                    .AddField("Среднее очков защиты", statistic.Average(m => m.Defense) is double num5 ? num5.ToString("0.0") : "Нет данных", true)
                                    .AddField("Среднее очков поддержки", statistic.Average(m => m.Support) is double num6 ? num6.ToString("0.0") : "Нет данных", true)
                                    ;

                        embedBuilder.WithImageUrl($"https://avatars.steamstatic.com/{steamProfile.AvatarHash}_full.jpg");

                        discordMessage.Content = string.Empty;
                        discordMessage.AddEmbed(embedBuilder);
                    }
                }
                catch (Exception ex)
                {
                    discordMessage = new DiscordMessageBuilder() { Content = ex.Message };
                }
                dbContext.DisposeAsync();
            }

            return discordMessage;
        }

    }


}
