using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApp.DbContexts;

namespace WebApp.Services.RconScanerService
{
    public class RconUpdaterService
    {
        public async Task UpdateStatisticDB(CancellationTokenSource CancellationTokenSource)
        {
            using(StatisticDbContext Context = WebApp.Application.Services.GetService<StatisticDbContext>())
            {
                if (Context == null) return;

                CancellationToken token = CancellationTokenSource.Token;

                var servers = (from s in Context.Servers
                               select s).ToList();

                foreach (var server in servers)
                {
                    if (server != null && server.ServerIsTracking == true)
                    {
                        RconStatGetter rconStat = new RconStatGetter(server.RconURL);

                        uint? lastServerLocalMatchId = rconStat.GetLastMatchId;
                        if (lastServerLocalMatchId < 0) continue;

                        var outdatedMatch = (from m in server.Matches
                                             where m.ServerLocalMatchId > lastServerLocalMatchId
                                             select m).ToList();

                        if (outdatedMatch.Count() > 0)
                        {
                            Context.ServerMatches.RemoveRange(outdatedMatch);
                            Context.SaveChanges();
                        }

                        uint LastDbMatchId = (from m in server.Matches
                                              select m.ServerLocalMatchId).Concat(new uint[] { 0 }).Max();

                        foreach (JsonDocument json in rconStat.GetLastMatches(lastServerLocalMatchId.Value - LastDbMatchId))
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            if (token.IsCancellationRequested) break;

                            using(StatisticDbContext localContext = WebApp.Application.Services.GetService<StatisticDbContext>())
                            {
                                var localServer = localContext.Servers.FirstOrDefault(x => x.ID == server.ID);

                                ServerMatch match = MatchParser.ParseMatchStatisticAndAddToContext(json, localServer, localContext);

                                await localContext.SaveChangesAsync();

                                Console.WriteLine($"MatchID {match.ServerLocalMatchId}/{lastServerLocalMatchId} saved" + "\n..");
                            }
                        }
                    }

                    if (token.IsCancellationRequested) break;
                }
            }

            

        }
    }
}
