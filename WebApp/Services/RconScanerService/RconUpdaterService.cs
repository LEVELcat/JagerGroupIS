using DbLibrary.StatisticModel;
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
            ILogger logger = WebApp.AppLogger;

            logger.LogDebug("Создание serverContext");

            Server[] servers = null;
            using (StatisticDbContext serverContext = new StatisticDbContext())
            {
                servers = await (from s in serverContext.Servers
                                 select s).AsNoTracking().ToArrayAsync();

                logger.LogInformation("Список серверов получен");

                logger.LogDebug("Освобождение ресурсов serverContext");
                await serverContext.DisposeAsync();
            }

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

                    uint lastDbMatchId = 0;
                    using (StatisticDbContext outdatedMatchContext = new StatisticDbContext()) 
                    {
                        logger.LogDebug("Получение списка матчей которые нужно удалить");

                        var outdatedMatch = await (from m in outdatedMatchContext.ServerMatches
                                                   where m.ServerID == server.ID && m.ServerLocalMatchId > lastRconServerMatchID
                                                   select m).AsNoTracking().ToArrayAsync();

                        if (outdatedMatch.Length != 0)
                        {
                            logger.LogDebug("Удаление лишних матчей");
                            outdatedMatchContext.ServerMatches.RemoveRange(outdatedMatch);
                            await outdatedMatchContext.SaveChangesAsync();
                        }

                        logger.LogDebug("Получение ID последнего матча");
                        lastDbMatchId = (await (from m in outdatedMatchContext.ServerMatches
                                                where m.ServerID == server.ID
                                                select m.ServerLocalMatchId).ToListAsync()).Max() is uint number ? number : 0;

                        logger.LogDebug("Освобождение ресурсов outdatedMatchContext");
                        await outdatedMatchContext.DisposeAsync();
                    }

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

                        using (StatisticDbContext matchContext = new StatisticDbContext())
                        {
                            try
                            {
                                logger.LogDebug("Создание matchContext");

                                logger.LogDebug("Парсинг json'а и запись в контекст");
                                ServerMatch serverMatch = await MatchParser.ParseMatchStatisticAndAddToContext(json, server.ID, matchContext, CancellationTokenSource);
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
                            matchContext.DisposeAsync();
                        }

                        if (token.IsCancellationRequested)
                        {
                            logger.LogInformation("Отмена цикла запросов по причине получения токена отмены");
                            break;
                        }

                        logger.LogDebug("Освобождение ресурсов matchContext");
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
