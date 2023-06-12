using System.Text.Json;
using WebApp.DbContexts;

namespace WebApp.Services.RconScanerService
{
    public class RconUpdaterService
    {
        public void UpdateStatisticDB()
        {
            using(StatisticDbContext context = new StatisticDbContext())
            {
                var servers = (from s in context.Servers
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
                            context.ServerMatches.RemoveRange(outdatedMatch);
                            context.SaveChanges();
                        }

                        uint LastDbMatchId = (from m in server.Matches
                                              select m.ServerLocalMatchId).Concat(new uint[] { 0 }).Max();

                        foreach (JsonDocument json in rconStat.GetLastMatches(lastServerLocalMatchId.Value - LastDbMatchId))
                        {
                            ServerMatch match = MatchParser.ParseMatchStatisticAndAddToContext(json, server, context);

                            context.SaveChanges();

                            Console.WriteLine($"MatchID {match.ServerLocalMatchId}/{lastServerLocalMatchId} saved" + "\n..");
                        }
                    }
                }
            }
        }
    }
}
