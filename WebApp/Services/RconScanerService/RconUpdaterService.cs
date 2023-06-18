using System.Data.Entity;
using System.Text.Json;
using WebApp.DbContexts;

namespace WebApp.Services.RconScanerService
{
    public class RconUpdaterService
    {
        public async Task UpdateStatisticDB(CancellationTokenSource CancellationTokenSource)
        {
            CancellationToken token = CancellationTokenSource.Token;

            Server[] servers = null;

            using (StatisticDbContext Context = new StatisticDbContext())
            {
                if (Context == null) return;



                servers = (from s in Context.Servers.AsNoTracking()
                           select s).ToArray();
            }

            if (servers == null) return;

            foreach (var server in servers)
            {
                if (server != null && server.ServerIsTracking == true)
                {
                    RconStatGetter rconStat = new RconStatGetter(server.RconURL);

                    uint? lastServerLocalMatchId = rconStat.GetLastMatchId;
                    if (lastServerLocalMatchId < 0) continue;

                    uint LastDbMatchId = 0;

                    using (StatisticDbContext deleteContext = new StatisticDbContext())
                    {
                        var outdatedMatch = (from m in deleteContext.ServerMatches
                                             where m.ServerID == server.ID && m.ServerLocalMatchId > lastServerLocalMatchId
                                             select m).ToArray();

                        if (outdatedMatch.Count() > 0)
                        {
                            deleteContext.ServerMatches.RemoveRange(outdatedMatch);
                            deleteContext.SaveChanges();



                        }

                        LastDbMatchId = (from m in deleteContext.ServerMatches.AsNoTracking()
                                         where m.ServerID == server.ID
                                         select m.ServerLocalMatchId).ToArray().Concat(new uint[] { 0 }).Max();

                        deleteContext.DisposeAsync();
                    }

                    foreach (JsonDocument json in rconStat.GetLastMatches(lastServerLocalMatchId.Value - LastDbMatchId))
                    {
                        GC.Collect();

                        if (token.IsCancellationRequested) break;

                        using (StatisticDbContext localContext = new StatisticDbContext())
                        {
                            var localServer = localContext.Servers.AsNoTracking().FirstOrDefault(x => x.ID == server.ID);

                            ServerMatch match = null;
                            try
                            {
                                match = MatchParser.ParseMatchStatisticAndAddToContext(json, localServer, localContext);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                Console.WriteLine(ex.Message);
                            }
                            await localContext.SaveChangesAsync();

                            Console.WriteLine($"MatchID {match.ServerLocalMatchId}/{lastServerLocalMatchId} saved" + "\n..");

                            localContext.Dispose();
                        }
                    }
                }
                if (token.IsCancellationRequested) break;
            }
        }
    }
}
