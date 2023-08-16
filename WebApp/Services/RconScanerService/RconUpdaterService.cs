using DbLibrary.StatisticModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WebApp.Services.RconScanerService
{
    public class RconUpdaterService
    {
        public async Task UpdateStatisticDB(CancellationTokenSource CancellationTokenSource)
        {
#if DEBUG
            try
            {
#endif


                CancellationToken token = CancellationTokenSource.Token;
                ILogger logger = WebApp.AppLogger;

                logger.LogDebug("Создание serverContext");

                ServerGroup[] serverGroups = null;
                using (StatisticDbContext serverContext = new StatisticDbContext())
                {
                    serverGroups = await serverContext.ServerGroups.AsNoTracking().ToArrayAsync();

                    logger.LogInformation("Список серверов получен");

                    logger.LogDebug("Освобождение ресурсов serverContext");
                    await serverContext.DisposeAsync();
                }

                foreach (var serverGroup in serverGroups)
                {
                    logger.LogInformation($"Начато сканирование серверов {serverGroup.ServerGroupName}");

                    if (serverGroup != null && serverGroup.IsTracking == true)
                    {
                        RconStatGetter rconStat = new RconStatGetter(serverGroup.RconURL);

                        logger.LogDebug("Получение последнего ID матча");

                        uint? lastRconServerMatchID = rconStat.GetLastMatchId;
                        if (lastRconServerMatchID < 0) continue;

                        logger.LogDebug("Создание outdatedMatchContext");

                        uint lastDbMatchId = 0;
                        using (StatisticDbContext lastMatchContext = new StatisticDbContext())
                        {
                            logger.LogDebug("Получение ID последнего матча");

                            var servers = await (from s in lastMatchContext.Servers
                                                 where s.ServerGroupID == serverGroup.ID
                                                 select s.ID).ToArrayAsync();

                            var matchesID = await (from m in lastMatchContext.ServerMatches
                                                   where servers.Contains(m.ServerID)
                                                   orderby m.ID
                                                   select m.ServerLocalMatchId).ToArrayAsync();

                            var lastMatchID = matchesID.LastOrDefault();


                            if (lastRconServerMatchID < lastMatchID)
                                lastDbMatchId = 0;
                            else
                                lastDbMatchId = lastMatchID;

                            logger.LogDebug("Освобождение ресурсов outdatedMatchContext");
                            await lastMatchContext.DisposeAsync();
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
                                    ServerMatch serverMatch = await MatchParser.ParseMatchStatisticAndAddToContext(json, serverGroup.ID, matchContext, CancellationTokenSource);
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
#if DEBUG
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());
            }
#endif

        }
    }
}
