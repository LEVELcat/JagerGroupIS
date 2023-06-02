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


                    DeathByStats = new List<PersonalDeathByStat>(),
                    DeathByWeaponStats = new List<PersonalDeathByWeaponStat>(),
                    KillStats = new List<PersonalKillStat>(),
                    WeaponKillStats = new List<PersonalWeaponKillStat>(),

                };



            }


            return curentMatch;
        }
    }
}
