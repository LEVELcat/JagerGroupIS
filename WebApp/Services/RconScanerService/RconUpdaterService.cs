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
                            ServerMatch rawMatchStat = MatchParser.ParseMatchStatistic(json, server);

                            FixRawStatistic(rawMatchStat, context);

                            context.ServerMatches.Add(rawMatchStat);

                            context.SaveChanges();

                            Console.WriteLine($"MatchID {rawMatchStat.ServerLocalMatchId}/{lastServerLocalMatchId} saved");
                        }
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
                SteamProfile? result = (from s in context.SteamProfiles
                                       where s.SteamID64 == profile.SteamID64
                                       select s).FirstOrDefault();
                if(result == null)
                {
                    result = (from s in context.SteamProfiles.Local
                              where s.SteamID64 == profile.SteamID64
                              select s).FirstOrDefault();

                    if(result == null)
                    {
                        context.SteamProfiles.Add(profile);
                        result = profile;
                    }
                }

                return profile;
            }

            Map GetMap(Map map)
            {
                Map? result = (from m in context.Maps
                       where m.MapName == map.MapName
                       select m).FirstOrDefault();
                if(result == null)
                {
                    result = (from m in context.Maps.Local
                              where m.MapName == map.MapName
                              select m).FirstOrDefault();

                    if(result == null)
                    {
                        context.Maps.Add(map);
                        result = map;
                    }
                }
                return result;
            }

            Weapon GetWeapon(Weapon weapon)
            {
                Weapon? result = (from w in context.Weapons
                                 where w.WeaponName == weapon.WeaponName
                                 select w).FirstOrDefault();

                if(result == null)
                {
                    result = (from m in context.Weapons.Local
                              where m.WeaponName == weapon.WeaponName
                              select m).FirstOrDefault();
                    if (result == null)
                    {
                        context.Weapons.Add(weapon);
                        result = weapon;
                    }
                }
                return weapon;
            }
        }
    }
}
