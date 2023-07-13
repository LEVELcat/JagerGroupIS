using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using DbLibrary.StatisticModel;
using Microsoft.AspNetCore.Hosting.Server;

namespace WebApp.Services.RconScanerService
{
    static class MatchParser
    {
        public static async Task<ServerMatch> ParseMatchStatisticAndAddToContext(JsonDocument json, ushort serverGroupID, StatisticDbContext context, CancellationTokenSource cancellationTokenSource)
        {
            ILogger logger = WebApp.AppLogger;

            CancellationToken token = cancellationTokenSource.Token;

            if (json == null)
            {
                logger.LogDebug("Пустой json");
                return null;
            }

            var resultPtr = json.RootElement.GetProperty("result");

            Map curentMap = await GetMap(resultPtr.GetProperty("map_name").GetString());

            Server curentServer = await GetServer(resultPtr.GetProperty("server_number").GetByte(), serverGroupID);

            logger.LogDebug("Создание экземпляра матча");
            ServerMatch curentMatch = new ServerMatch()
            {
                ServerLocalMatchId = resultPtr.GetProperty("id").GetUInt32(),
                StartTime = resultPtr.GetProperty("start").GetDateTime(),
                EndTime = resultPtr.GetProperty("end").GetDateTime(),
                CreationTime = resultPtr.GetProperty("creation_time").GetDateTime(),

                Map = curentMap,
                Server = curentServer,

                PersonalsMatchStat = new List<PersonalMatchStat>()
            };
            logger.LogDebug("Добавление экземпляра матча в контекст");
            context.ServerMatches.Add(curentMatch);

            Dictionary<string, SteamProfile> localSteamProfile = new Dictionary<string, SteamProfile>();

            logger.LogDebug("Старт цикла перебора игроков");
            foreach (var playerStat in resultPtr.GetProperty("player_stats").EnumerateArray())
            {
                if (playerStat.GetProperty("steaminfo").ValueKind == JsonValueKind.Null)
                {
                    logger.LogDebug("Отсуствует steam профиль");
                    continue;
                } 

                var steaminfoPtr = playerStat.GetProperty("steaminfo").GetProperty("profile");

                logger.LogDebug("Создание экземпляра профиля");
                SteamProfile steamProfile = new SteamProfile()
                {
                    SteamID64 = ulong.Parse(steaminfoPtr.GetProperty("steamid").GetString()),
                    SteamName = steaminfoPtr.GetProperty("personaname").GetString(),
                    AvatarHash = steaminfoPtr.GetProperty("avatarhash").GetString(),
                    ProfileUpdtaded = curentMatch.CreationTime,

                    DeathByStats = new List<PersonalDeathByStat>(),
                    KillStats = new List<PersonalKillStat>()
                };
                string localNick = playerStat.GetProperty("player").GetString();

                steamProfile = await GetSteamProfile(steamProfile);

                logger.LogDebug("Добавление профиля в локальный словарь");
                localSteamProfile.Add(localNick, steamProfile);

                logger.LogDebug("Создание экземпляра персональной статистики");
                PersonalMatchStat personalMatchStat = new PersonalMatchStat()
                {
                    Deaths = playerStat.GetProperty("deaths").GetUInt16(),
                    DeathsByTK = playerStat.GetProperty("deaths_by_tk").GetUInt16(),
                    DeathsWithoutKillStreak = playerStat.GetProperty("deaths_without_kill_streak").GetUInt16(),
                    Kills = playerStat.GetProperty("kills").GetUInt16(),
                    KillStreak = playerStat.GetProperty("kills_streak").GetUInt16(),
                    LongestLife = playerStat.GetProperty("longest_life_secs").GetInt32(),
                    ShortestLife = playerStat.GetProperty("shortest_life_secs").GetInt32(),
                    TeamKills = playerStat.GetProperty("teamkills").GetUInt16(),
                    PlayTime = playerStat.GetProperty("time_seconds").GetInt32(),

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

                if(curentMatch.PersonalsMatchStat == null)
                {
                    logger.LogDebug("Добавление пустого массива к коллекции матчей");
                    curentMatch.PersonalsMatchStat = new List<PersonalMatchStat>();
                }

                logger.LogDebug("Присоединения персональной статистики к матчу");
                curentMatch.PersonalsMatchStat.Add(personalMatchStat);
            }

            List<Weapon> localWeaponList = new List<Weapon>();

            logger.LogDebug("Старт цикла перебора статистик по убийствам и оружию");
            foreach (var playerStat in resultPtr.GetProperty("player_stats").EnumerateArray())
            {
                string localNick = playerStat.GetProperty("player").GetString();

                logger.LogDebug("Проверка наличия никннейма в локальном словаре");
                if (localSteamProfile.ContainsKey(localNick))
                {
                    logger.LogDebug("Получение персональой статистики из коллекции");
                    PersonalMatchStat? personalMatchStat = curentMatch.PersonalsMatchStat
                        .FirstOrDefault(p => p.SteamProfile == localSteamProfile[localNick]);

                    if(personalMatchStat != null)
                    {
                        logger.LogDebug("Старт цикла перебора статистики по убийствам");
                        foreach (var killstat in playerStat.GetProperty("most_killed").EnumerateObject())
                        {
                            logger.LogDebug("Проверка на наличие имени в локальных профилях");
                            if (localSteamProfile.ContainsKey(killstat.Name))
                            {
                                if (personalMatchStat.KillStats == null)
                                {
                                    logger.LogDebug("Добавление пустого массива в список убийств");
                                    personalMatchStat.KillStats = new List<PersonalKillStat>();
                                }

                                logger.LogDebug("Присоединение статистики по убийствам к коллекции убийств");
                                personalMatchStat.KillStats.Add(new PersonalKillStat()
                                {
                                    Count = killstat.Value.GetUInt16(),
                                    SteamProfile = localSteamProfile[killstat.Name],
                                    PersonalMatchStat = personalMatchStat
                                });
                            }
                        }

                        logger.LogDebug("Старт цикла перебора статистики по смертям");
                        foreach (var deathStat in playerStat.GetProperty("death_by").EnumerateObject())
                        {
                            logger.LogDebug("Проверка на наличие имени в локальных профилях");
                            if (localSteamProfile.ContainsKey(deathStat.Name))
                            {
                                if (personalMatchStat.DeathByStats == null)
                                {
                                    logger.LogDebug("Добавление пустого массива в список смертей");
                                    personalMatchStat.DeathByStats = new List<PersonalDeathByStat>();
                                }

                                logger.LogDebug("Присоединение статистики по смертям к коллекции смертей");
                                personalMatchStat.DeathByStats.Add(new PersonalDeathByStat()
                                {
                                    Count = deathStat.Value.GetUInt16(),
                                    SteamProfile = localSteamProfile[deathStat.Name],
                                    PersonalMatchStat = personalMatchStat
                                });
                            }
                        }

                        logger.LogDebug("Старт цикла перебора статистики по убийствам с оружия");

                        foreach(var weaponKills in playerStat.GetProperty("weapons").EnumerateObject())
                        {
                            if(personalMatchStat.WeaponKillStats == null)
                            {
                                logger.LogDebug("Добавление пустого массива в список убийств из оружия");
                                personalMatchStat.WeaponKillStats = new List<PersonalWeaponKillStat>();
                            }

                            logger.LogDebug("Присоединение статистики по убийствам с оружия к коллекции убийств с оружием");
                            personalMatchStat.WeaponKillStats.Add(new PersonalWeaponKillStat()
                            {
                                Count = weaponKills.Value.GetUInt16(),
                                Weapon = await GetWeapon(weaponKills.Name),
                                PersonalMatchStat = personalMatchStat
                            });
                        }

                        //COMPATIBILITY WITH OLD API

                        if (playerStat.GetProperty("death_by_weapons").ValueKind != JsonValueKind.Null)
                        {
                            logger.LogDebug("Старт цикла перебора статистики по смертям от оружия");

                            foreach(var deathsByWeapon in playerStat.GetProperty("death_by_weapons").EnumerateObject())
                            {
                                if (personalMatchStat.DeathByWeaponStats == null)
                                {
                                    logger.LogDebug("Добавление пустого массива в коллекцию смертей от оружия");
                                    personalMatchStat.DeathByWeaponStats = new List<PersonalDeathByWeaponStat>();
                                }

                                logger.LogDebug("Присоединение статистики по смертям от оружия к коллекции смертей от оружия");
                                personalMatchStat.DeathByWeaponStats.Add(new PersonalDeathByWeaponStat()
                                {
                                    Count = deathsByWeapon.Value.GetUInt16(),
                                    Weapon = await GetWeapon(deathsByWeapon.Name),
                                    PersonalMatchStat = personalMatchStat
                                });
                            }
                        }
                        else
                            logger.LogDebug("Отсуствует список смертей от оружия");

                    }
                }
            }

            logger.LogDebug("Возврат матча");
            return curentMatch;

            async Task<Map> GetMap(string mapName)
            {
                logger.LogDebug("Получение карты из БД");
                Map? result = await context.Maps.FirstOrDefaultAsync(m => m.MapName == mapName);

                if (result == null)
                {
                    logger.LogDebug("Добавление карты в контекст");
                    result = new Map() { MapName = mapName };

                    await context.Maps.AddAsync(result);
                }
                return result;
            }

            async Task<SteamProfile> GetSteamProfile(SteamProfile profile)
            {
                logger.LogDebug("Получение профиля из БД");
                SteamProfile? result = await context.SteamProfiles.FirstOrDefaultAsync(s => s.SteamID64 == profile.SteamID64);

                if(result == null)
                {
                    logger.LogDebug("Добавление профиля в контекст");
                    result = profile;
                    await context.SteamProfiles.AddAsync(profile);
                }
                else 
                {
                    if (result.ProfileUpdtaded == null)
                        result.ProfileUpdtaded = DateTime.UtcNow;

                    if(profile.ProfileUpdtaded > result.ProfileUpdtaded)
                    {
                        if (result.SteamName != profile.SteamName)
                        {
                            logger.LogDebug("Изменение имени в контексте");
                            result.SteamName = profile.SteamName;
                        }

                        if (result.AvatarHash != profile.AvatarHash)
                        {
                            logger.LogDebug("Изменение хэша аватара в контексте");
                            result.AvatarHash = profile.AvatarHash;
                        }

                        result.ProfileUpdtaded = result.ProfileUpdtaded;

                    }

                    
                }
                return result;
            }

            async Task<Weapon> GetWeapon(string weaponName)
            {
                logger.LogDebug("Получение оружия из БД");
                Weapon? result = await context.Weapons.FirstOrDefaultAsync(x => x.WeaponName == weaponName);

                if (result == null)
                {
                    logger.LogDebug("Получение оружия из локальных данных");
                    result = localWeaponList.FirstOrDefault(x => x.WeaponName == weaponName);

                    if(result == null)
                    {
                        logger.LogDebug("Добавление оружия в контекст");
                        result = new Weapon() { WeaponName = weaponName };

                        localWeaponList.Add(result);

                        await context.Weapons.AddAsync(result);
                    }
                }
                return result;
            }

            async Task<Server> GetServer(byte serverNumber, ushort serverGroupID)
            {
                logger.LogDebug("Получение сервера из БД");

                Server? result = await context.Servers.FirstOrDefaultAsync(s => s.ServerGroupID == serverGroupID && s.ServerNumber == serverNumber);

                if(result == null)
                {
                    logger.LogDebug("Добавление сервера в БД");

                    result = new Server()
                    {
                        ServerGroupID = serverGroupID,
                        ServerNumber = serverNumber,
                        ServerName = null,
                        ServerTypeID = 1
                    };
                    await context.Servers.AddAsync(result);
                }
                return result;
            }
        }
    }
}
