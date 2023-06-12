using System.Text.Json;
using WebApp.DbContexts;

namespace WebApp.Services.RconScanerService
{
    static class MatchParser
    {
        public static ServerMatch ParseMatchStatisticAndAddToContext(JsonDocument json, Server server, StatisticDbContext context)
        {
            if (json == null) return null;

            var resultPtr = json.RootElement.GetProperty("result");

            Map curentMap = GetMap(resultPtr.GetProperty("map_name").GetString());

            ServerMatch curentMatch = new ServerMatch()
            {
                ServerLocalMatchId = resultPtr.GetProperty("id").GetUInt32(),
                StartTime = resultPtr.GetProperty("start").GetDateTime(),
                EndTime = resultPtr.GetProperty("end").GetDateTime(),
                CreationTime = resultPtr.GetProperty("creation_time").GetDateTime(),

                Map = curentMap,
                Server = server,

                PersonalsMatchStat = new List<PersonalMatchStat>()
            };

            context.ServerMatches.Add(curentMatch);

            Dictionary<string, SteamProfile> localSteamProfile = new Dictionary<string, SteamProfile>();

            List<PersonalDeathByStat> personalDeathByStatsList = new List<PersonalDeathByStat>();
            List<PersonalDeathByWeaponStat> personalDeathByWeaponStatsList = new List<PersonalDeathByWeaponStat>();
            List<PersonalKillStat> personalKillStatsList = new List<PersonalKillStat>();
            List<PersonalWeaponKillStat> personalWeaponKillStatsList = new List<PersonalWeaponKillStat>();

            foreach (var playerStat in resultPtr.GetProperty("player_stats").EnumerateArray())
            {
                var steaminfoPtr = playerStat.GetProperty("steaminfo").GetProperty("profile");

                SteamProfile steamProfile = new SteamProfile()
                {
                    SteamID64 = ulong.Parse(steaminfoPtr.GetProperty("steamid").GetString()),
                    SteamName = steaminfoPtr.GetProperty("personaname").GetString(),
                    AvatarHash = steaminfoPtr.GetProperty("avatarhash").GetString(),

                    DeathByStats = new List<PersonalDeathByStat>(),
                    KillStats = new List<PersonalKillStat>()
                };
                string localNick = steamProfile.SteamName;

                steamProfile = GetSteamProfile(steamProfile);
                localSteamProfile.Add(playerStat.GetProperty("player").GetString(), steamProfile);

                Console.WriteLine(localNick);

                PersonalMatchStat personalMatchStat = new PersonalMatchStat()
                {
                    Deaths = playerStat.GetProperty("deaths").GetUInt16(),
                    DeathsByTK = playerStat.GetProperty("deaths_by_tk").GetUInt16(),
                    DeathsWithoutKillStreak = playerStat.GetProperty("deaths_without_kill_streak").GetUInt16(),
                    Kills = playerStat.GetProperty("kills").GetUInt16(),
                    KillStreak = playerStat.GetProperty("kills_streak").GetUInt16(),
                    LongestLife = playerStat.GetProperty("longest_life_secs").GetUInt16(),
                    ShortestLife = playerStat.GetProperty("shortest_life_secs").GetUInt16(),
                    TeamKills = playerStat.GetProperty("teamkills").GetUInt16(),
                    PlayTime = playerStat.GetProperty("time_seconds").GetInt16(),

                    //COMPATIBILITY WITH OLD API
                    Combat = playerStat.GetProperty("combat").ValueKind == JsonValueKind.Null ?
                            null :
                            playerStat.GetProperty("combat").GetUInt16(),
                    Defense = playerStat.GetProperty("defense").ValueKind == JsonValueKind.Null ?
                            null :
                            playerStat.GetProperty("defense").GetUInt16(),
                    Support = playerStat.GetProperty("support").ValueKind == JsonValueKind.Null ?
                            null :
                            playerStat.GetProperty("support").GetUInt16(),
                    Offensive = playerStat.GetProperty("offense").ValueKind == JsonValueKind.Null ?
                            null :
                            playerStat.GetProperty("offense").GetUInt16(),

                    SteamProfile = steamProfile,
                    Match = curentMatch,

                    DeathByStats = steamProfile.DeathByStats,
                    DeathByWeaponStats = new List<PersonalDeathByWeaponStat>(),
                    KillStats = steamProfile.KillStats,
                    WeaponKillStats = new List<PersonalWeaponKillStat>(),
                };
                curentMatch.PersonalsMatchStat.Add(personalMatchStat);

                context.SteamProfiles.Add(steamProfile);
                context.PersonalMatchStats.Add(personalMatchStat);

                foreach (var killstat in playerStat.GetProperty("most_killed").EnumerateObject())
                {
                    if (localSteamProfile.ContainsKey(killstat.Name))
                    {
                        personalKillStatsList.Add(
                            new PersonalKillStat()
                        {
                            Count = killstat.Value.GetUInt16(),
                            SteamProfile = localSteamProfile[killstat.Name]
                        });
                    }
                }

                foreach (var deathStat in playerStat.GetProperty("death_by").EnumerateObject())
                {
                    if (localSteamProfile.ContainsKey(deathStat.Name))
                    {
                        personalDeathByStatsList.Add(
                            new PersonalDeathByStat()
                        {
                            Count = deathStat.Value.GetUInt16(),
                            SteamProfile = localSteamProfile[deathStat.Name]
                        });
                    }

                }
                foreach (var weaponKills in playerStat.GetProperty("weapons").EnumerateObject())
                {
                    personalWeaponKillStatsList.Add(
                        new PersonalWeaponKillStat()
                        {
                            Count = weaponKills.Value.GetUInt16(),
                            Weapon = GetWeapon(weaponKills.Name)
                        });
                }

                //COMPATIBILITY WITH OLD API
                if (playerStat.GetProperty("death_by_weapons").ValueKind != JsonValueKind.Null)
                {
                    foreach (var deathByWeapon in playerStat.GetProperty("death_by_weapons").EnumerateObject())
                    {
                        personalDeathByWeaponStatsList.Add(
                            new PersonalDeathByWeaponStat()
                            {
                                Count = deathByWeapon.Value.GetUInt16(),
                                Weapon = GetWeapon(deathByWeapon.Name)
                            });
                    }
                }
            }

            context.SaveChanges();

            foreach (var personalStat in curentMatch.PersonalsMatchStat)
            {
                personalStat.DeathByStats = (from deathByStat in personalDeathByStatsList
                                             where deathByStat.MatchStat == personalStat
                                             select deathByStat).ToList();

                personalStat.KillStats = (from killStat in personalKillStatsList
                                          where killStat.MatchStat == personalStat
                                          select killStat).ToList();

                personalStat.WeaponKillStats = (from weaponKillStat in personalWeaponKillStatsList
                                                where weaponKillStat.MatchStat == personalStat
                                                select weaponKillStat).ToList();

                personalStat.DeathByWeaponStats = (from deathByWeaponStat in personalDeathByWeaponStatsList
                                                   where deathByWeaponStat.MatchStat == personalStat
                                                   select deathByWeaponStat).ToList();
            }

            context.SaveChanges();

            return curentMatch;

            Map GetMap(string mapName)
            {
                Map result = (from m in context.Maps
                              where m.MapName == mapName
                              select m).FirstOrDefault();
                if (result == null)
                {
                    result = (from m in context.Maps.Local
                              where m.MapName == mapName
                              select m).FirstOrDefault();

                    if (result == null)
                    {
                        result = new Map()
                        {
                            MapName = mapName
                        };
                        context.Maps.Add(result);
                    }
                }
                return result;
            }

            SteamProfile GetSteamProfile(SteamProfile profile)
            {
                SteamProfile result = (from s in context.SteamProfiles
                                       where s.SteamID64 == profile.SteamID64
                                       select s).FirstOrDefault();
                if (result == null)
                {
                    result = (from s in context.SteamProfiles.Local
                              where s.SteamID64 == profile.SteamID64
                              select s).FirstOrDefault();

                    if (result == null)
                    {
                        context.SteamProfiles.Add(profile);
                        result = profile;
                    }
                }

                result.SteamName = profile.SteamName;
                return result;
            }

            Weapon GetWeapon(string weaponName)
            {
                Weapon result = (from w in context.Weapons
                                 where w.WeaponName == weaponName
                                 select w).FirstOrDefault();

                if (result == null)
                {
                    result = (from m in context.Weapons.Local
                              where m.WeaponName == weaponName
                              select m).FirstOrDefault();
                    if (result == null)
                    {
                        result = new Weapon()
                        {
                            WeaponName = weaponName
                        };
                        context.Weapons.Add(result);
                    }
                }
                return result;
            }
        }
    }
}
