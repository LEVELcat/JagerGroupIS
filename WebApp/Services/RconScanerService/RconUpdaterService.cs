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
                        ServerMatch rawMatchStat = MatchParser.ParseMatchStatistic(json, server);

                        FixRawStatistic(rawMatchStat, context);

                        context.ServerMatches.Add(rawMatchStat);

                        context.SaveChanges();
                    }
                }
            }
        }

        private static void FixRawStatistic(ServerMatch serverMatch, StatisticDbContext context)
        {
            serverMatch.Map = GetMap(serverMatch.Map);

            var rawSteamProfiles = serverMatch.PersonalsMatchStat.Select(x => x.SteamProfile);

            Dictionary<string, SteamProfile> keyValues = new Dictionary<string, SteamProfile>();
            foreach (var profile in rawSteamProfiles)
            {
                keyValues.Add(profile.SteamName, GetSteamProfile(profile));
            }

            foreach (var personalStat in serverMatch.PersonalsMatchStat)
            {
                foreach (var weaponKillStat in personalStat.WeaponKillStats)
                    weaponKillStat.Weapon = GetWeapon(weaponKillStat.Weapon);

                foreach (var deathByWeapon in personalStat.DeathByWeaponStats)
                    deathByWeapon.Weapon = GetWeapon(deathByWeapon.Weapon);

                foreach (var killStat in personalStat.KillStats)
                    killStat.SteamProfile = keyValues[killStat.SteamProfile.SteamName];

                foreach (var deathByStat in personalStat.DeathByStats)
                    deathByStat.SteamProfile = keyValues[deathByStat.SteamProfile.SteamName];
            }

            SteamProfile GetSteamProfile(SteamProfile profile)
            {
                SteamProfile result = (from s in context.SteamProfiles.Concat(context.SteamProfiles.Local)
                                       where s.SteamID64 == profile.SteamID64
                                       select s).FirstOrDefault();

                if(result == null)
                {
                    context.SteamProfiles.Add(profile);
                    result = profile;
                }

                return profile;
            }

            Map GetMap(Map map)
            {
                Map result = (from m in context.Maps.Concat(context.Maps.Local)
                       where m.MapName == map.MapName
                       select m).FirstOrDefault();

                if(map == null)
                {
                    context.Maps.Add(map);
                    result = map;
                }
                return result;
            }

            Weapon GetWeapon(Weapon weapon)
            {
                Weapon result = (from w in context.Weapons.Concat(context.Weapons.Local)
                                 where w.WeaponName == weapon.WeaponName
                                 select w).FirstOrDefault();

                if(weapon == null)
                {
                    context.Weapons.Add(weapon);
                    result = weapon;
                }

                return weapon;
            }

        }



    }
}
