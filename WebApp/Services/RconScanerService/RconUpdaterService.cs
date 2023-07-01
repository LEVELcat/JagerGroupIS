using DbLibrary.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WebApp.Services.RconScanerService
{
    public class RconUpdaterService
    {
        public async Task UpdateStatisticDB(CancellationTokenSource CancellationTokenSource)
        {
            CancellationToken token = CancellationTokenSource.Token;
            ILogger logger = WebApp.Application.Services.GetService<ILogger>();

            logger.LogDebug("Создание serverContext");
            using StatisticDbContext serverContext = new StatisticDbContext();

            logger.LogInformation("Получение списка серверов");

            Server[] servers = await (from s in serverContext.Servers
                                select s).AsNoTracking().ToArrayAsync();

            logger.LogInformation("Список серверов получен");

            logger.LogDebug("Освобождение ресурсов serverContext");
            await serverContext.DisposeAsync();

            foreach (var server in servers)
            {
                logger.LogInformation($"Начато сканирование {server.Description}");

                if(server != null && server.ServerIsTracking == true)
                {
                    RconStatGetter rconStat = new RconStatGetter(server.RconURL);

                    logger.LogDebug("Получение последнего ID матча");

                    uint? lastRconServerMatchID = rconStat.GetLastMatchId;
                    if (lastRconServerMatchID < 0) continue;

                    logger.LogDebug("Создание outdatedMatchContext");
                    using StatisticDbContext outdatedMatchContext = new StatisticDbContext();

                    logger.LogDebug("Получение списка матчей которые нужно удалить");

                    var outdatedMatch = await (from m in outdatedMatchContext.ServerMatches
                                               where m.ServerID == server.ID && m.ServerLocalMatchId > lastRconServerMatchID
                                               select m).AsNoTracking().ToArrayAsync();

                    if(outdatedMatch.Length != 0)
                    {
                        logger.LogDebug("Удаление лишних матчей");
                        outdatedMatchContext.ServerMatches.RemoveRange(outdatedMatch);
                        await outdatedMatchContext.SaveChangesAsync();
                    }


                    logger.LogDebug("Получение ID последнего матча");

                    uint lastDbMatchId = (await (from m in outdatedMatchContext.ServerMatches
                                          where m.ServerID == server.ID
                                          select m.ServerLocalMatchId).ToListAsync()).Max() is uint number ? number : 0;

                    logger.LogDebug("Освобождение ресурсов outdatedMatchContext");
                    await outdatedMatchContext.DisposeAsync();


                    logger.LogInformation("Старт цикла запросов из RCON API");

                    foreach (JsonDocument json in rconStat.GetLastMatches(lastRconServerMatchID.Value - lastDbMatchId))
                    {
                        logger.LogDebug("Старт сборки мусора");
                        GC.Collect();

                        if (token.IsCancellationRequested)
                        {
                            logger.LogInformation("Отмена цикла запросов по причине получения токена отмены");
                            break;
                        }

                        logger.LogDebug("Создание matchContext");
                        using StatisticDbContext matchContext = new StatisticDbContext();

                        logger.LogDebug("Получение сервера");
                        var localServer = matchContext.Servers.AsNoTracking().SingleOrDefault(x => x.ID == server.ID);

                        ServerMatch serverMatch = null;

                        try
                        {
                            logger.LogDebug("Парсинг json'а и запись в контекст");
                            serverMatch = await MatchParser.ParseMatchStatisticAndAddToContext(json, localServer, matchContext, CancellationTokenSource);
                            logger.LogInformation("Парсинг json'а и запись в контекст завершен");

                            logger.LogInformation($"MatchID {serverMatch?.ServerLocalMatchId}/{lastRconServerMatchID} saved");


                            logger.LogDebug("Сохранение контекста");
                            await matchContext.SaveChangesAsync();
                            logger.LogInformation("Сохранение контекста завершено");
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex.ToString());
                            logger.LogError(ex.Message);
                        }

                        if (token.IsCancellationRequested)
                        {
                            logger.LogInformation("Отмена цикла запросов по причине получения токена отмены");
                            break;
                        }

                        logger.LogDebug("Освобождение ресурсов matchContext");
                        matchContext.Dispose();
                    }
                }
                if (token.IsCancellationRequested)
                {
                    logger.LogInformation("Отмена цикла проверки серверов по причине получения токена отмены");
                    break;
                }
            }
        }
    }
}
