using System.Text.Json;
using WebApp.DbContexts;

namespace WebApp.Services.RconScanerService
{
    public static class MatchParser
    {
        public static ServerMatch ParseMatchStatistic(JsonDocument json, Server server)
        {
            if (json == null) return null;

            var resultPtr = json.RootElement.GetProperty("result");

            Map curentMap = new Map() { MapName = resultPtr.GetProperty("map_name").GetString() };

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

            foreach(var playerStat in resultPtr.GetProperty("player_stats").EnumerateArray())
            {
                var steaminfoPtr = playerStat.GetProperty("steaminfo").GetProperty("profile");

                SteamProfile steamProfile = new SteamProfile()
                {
                    SteamID64 = steaminfoPtr.GetProperty("steamid").GetUInt64(),
                    SteamName = steaminfoPtr.GetProperty("personaname").GetString(),
                    AvatarHash = steaminfoPtr.GetProperty("personaname").GetString(),

                    DeathByStats = new List<PersonalDeathByStat>(),
                    KillStats = new List<PersonalKillStat>()
                };

                PersonalMatchStat personalMatchStat = new PersonalMatchStat()
                {
                    Combat = playerStat.GetProperty("combat").GetUInt16(),
                    Deaths = playerStat.GetProperty("deaths").GetUInt16(),
                    DeathsByTK = playerStat.GetProperty("deaths_by_tk").GetUInt16(),
                    DeathsWithoutKillStreak = playerStat.GetProperty("deaths_without_kill_streak").GetUInt16(),
                    Defense = playerStat.GetProperty("defense").GetUInt16(),
                    Kills = playerStat.GetProperty("kills").GetUInt16(),
                    KillStreak = playerStat.GetProperty("kills_streak").GetUInt16(),
                    LongestLife = playerStat.GetProperty("longest_life_secs").GetUInt16(),
                    Offensive = playerStat.GetProperty("offense").GetUInt16(),
                    ShortestLife = playerStat.GetProperty("shortest_life_secs").GetUInt16(),
                    Support = playerStat.GetProperty("support").GetUInt16(),
                    TeamKills = playerStat.GetProperty("teamkills").GetUInt16(),
                    PlayTime = playerStat.GetProperty("time_seconds").GetUInt16(),


                    SteamProfile = steamProfile,
                    Match = curentMatch,


                    DeathByStats = steamProfile.DeathByStats,
                    DeathByWeaponStats = new List<PersonalDeathByWeaponStat>(),
                    KillStats = steamProfile.KillStats,
                    WeaponKillStats = new List<PersonalWeaponKillStat>(),
                };

                curentMatch.PersonalsMatchStat.Add(personalMatchStat);

                foreach(var killstat in playerStat.GetProperty("most_killed").EnumerateObject())
                {
                    personalMatchStat.KillStats.Add(
                        new PersonalKillStat()
                        {
                            Count = killstat.Value.GetUInt16(),
                            SteamProfile = new SteamProfile
                            {
                                SteamID64 = 0,
                                AvatarHash = string.Empty,
                                SteamName = killstat.Name
                            }
                        });

                }

                foreach (var deathStat in playerStat.GetProperty("death_by").EnumerateObject())
                {
                    personalMatchStat.DeathByStats.Add(
                        new PersonalDeathByStat()
                        {
                            Count = deathStat.Value.GetUInt16(),
                            SteamProfile = new SteamProfile
                            {
                                SteamID64 = 0,
                                AvatarHash = string.Empty,
                                SteamName = deathStat.Name
                            }
                        });
                }
                foreach (var weaponKills in playerStat.GetProperty("weapons").EnumerateObject())
                {
                    personalMatchStat.WeaponKillStats.Add(
                        new PersonalWeaponKillStat()
                        {
                            Count = weaponKills.Value.GetUInt16(),
                            Weapon = new Weapon()
                            {
                                WeaponName = weaponKills.Name,
                            }
                        });
                }

                foreach (var deathByWeapon in playerStat.GetProperty("death_by_weapons").EnumerateObject())
                {
                    personalMatchStat.DeathByWeaponStats.Add(
                        new PersonalDeathByWeaponStat()
                        {
                            Count = deathByWeapon.Value.GetUInt16(),
                            Weapon = new Weapon()
                            {
                                WeaponName = deathByWeapon.Name,
                            }
                        });
                }
            }

            return curentMatch;
        }
    }
}
