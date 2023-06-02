using System.Text.Json;
using WebApp.DbContexts;

namespace WebApp.Services.RconScanerService
{
    public class RconUpdaterService
    {
        public void UpdateStatisticDB()
        {
            StatisticDbContext context = WebApp.Application.Services.GetService<StatisticDbContext>();


            foreach (var server in context.Servers)
            {
                if (server != null && server.ServerIsTracking == true)
                {
                    RconStatGetter rconStat = new RconStatGetter(server.RconURL);

                    uint? lastLocalMatchId = rconStat.GetLastMatchId;
                    if (lastLocalMatchId < 0) continue;

                    var outdatedMatch = from m in context.ServerMatches
                                        where m.ServerLocalMatchId > lastLocalMatchId
                                        select m;
                    if (outdatedMatch.Count() > 0)
                    {
                        context.ServerMatches.RemoveRange(outdatedMatch);
                        context.SaveChanges();
                    }

                    uint LastDbMatchId = (from m in context.ServerMatches
                                          where m.ServerID == server.ID
                                          select m.ServerLocalMatchId).Max();

                    foreach(JsonDocument json in rconStat.GetLastMatches(LastDbMatchId - lastLocalMatchId.Value))
                    {


                    }
                }
            }
        }
    }
}
