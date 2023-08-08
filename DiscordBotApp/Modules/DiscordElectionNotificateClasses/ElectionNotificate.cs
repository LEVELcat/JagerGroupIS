using DbLibrary.JagerDsModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotApp.Modules.DiscordElectionNotificateClasses
{
    public class ElectionNotificate
    {

        public async Task SendNotificationToPeopleAsync(CancellationTokenSource CancellationTokenSource)
        {
            CancellationToken token = CancellationTokenSource.Token;
            var logger = DiscordApp.DiscordBot.Client.Logger;

            logger.LogDebug("Получение списка актуальных голосовалок");

            Election[] elections = null;
            //TimeSpan

            //using (JagerDbContext electionContext = new JagerDbContext())
            //{
            //    elections = (from el in electionContext.Elections
            //                 where el.EndTime.Value.Subtract(new TimeSpan) )



            //    await electionContext.DisposeAsync();
            //} 




        }



    }
}
