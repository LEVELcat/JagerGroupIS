using System.Collections;
using System.Text.Json;

namespace WebApp.Services.RconScanerService
{
    public class RconStatGetter
    {
        string baseURL = string.Empty;

        public RconStatGetter(string BaseURL)
        {
            baseURL = BaseURL;
        }

        public RconStatGetterCollection GetAllMatch() =>
           new RconStatGetterCollection(baseURL);
        public RconStatGetterCollection GetLastMatches(int Count)
        {
            if (Count <= 0) return null;

            var rcon = new RconStatGetterCollection(baseURL);
            int lastMatchID = Convert.ToInt32(rcon.GetLastMatchId().Value);
            return new RconStatGetterCollection(baseURL, lastMatchID - (Count - 1), lastMatchID);
        }
        public RconStatGetterCollection GetLastMatches(uint Count) => GetLastMatches(Convert.ToInt32(Count));


        public RconStatGetterCollection GetRangeMatch(int FromMatchId, int ToMatchId) =>
           new RconStatGetterCollection(baseURL, FromMatchId, ToMatchId);

        public uint? GetLastMatchId => new RconStatGetterCollection(baseURL).GetLastMatchId();

        public class RconStatGetterCollection : IEnumerator, IEnumerable
        {
            private const string SeverInfoPath = "/api/get_scoreboard_maps";
            private const string MatсhStatPath = "/api/get_map_scoreboard?map_id=";

            int matchId = 0;
            uint lastMatch = 0;

            int defaultMatchId = 0;
            uint lastMatchFromServer = 0;

            string baseURL = string.Empty;

            RconStatGetterCollection() { }
            internal RconStatGetterCollection(string baseURL)
            {
                this.baseURL = baseURL;
                lastMatchFromServer = GetLastMatchId().Value;
                lastMatch = lastMatchFromServer;
                matchId = 0;
                defaultMatchId = matchId;
            }

            internal RconStatGetterCollection(string baseURL, int FromMatchId, int ToMatchId) : this(baseURL)
            {
                matchId = FromMatchId - 1;

                if (matchId < 0)
                    matchId = 0;

                lastMatch = Convert.ToUInt32(ToMatchId);
                defaultMatchId = matchId;
            }

            IEnumerator IEnumerable.GetEnumerator() => this;

            object IEnumerator.Current => Current;

            public JsonDocument Current
            {
                get
                {
                    try
                    {
                        if (matchId <= 0 || matchId > lastMatchFromServer)
                            throw new IndexOutOfRangeException();
                        return GetCurrentMatchStat();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message, ex);
                    }
                }
            }

            public bool MoveNext()
            {
                matchId++;
                return (matchId <= lastMatch);
            }

            public void Reset()
            {
                matchId = defaultMatchId;
            }

            public uint? GetLastMatchId()
            {
                uint result = 0;
                string json = String.Empty;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(baseURL);

                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync(SeverInfoPath).Result;

                if (response.IsSuccessStatusCode)
                {
                    json = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    Console.Write($"{(int)response.StatusCode} {response.ReasonPhrase}");
                }
                client.Dispose();

                if (json == null || json == String.Empty) return null;
                else
                {
                    using (JsonDocument document = JsonDocument.Parse(json))
                    {
                        var res = document.RootElement.GetProperty("result");
                        var maps = res.GetProperty("maps");

                        if (maps.GetArrayLength() > 0)
                        {
                            var item = maps[0];

                            result = item.GetProperty("id").GetUInt32();
                        }
                    }
                }
                return result;
            }

            JsonDocument GetCurrentMatchStat()
            {
                string result = String.Empty;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(baseURL);

                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync(MatсhStatPath + matchId).Result;

                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                    client.Dispose();
                    return JsonDocument.Parse(result);
                }
                else
                {
                    Console.WriteLine($"{(int)response.StatusCode} ({response.ReasonPhrase})");
                    client.Dispose();
                    return null;
                }
            }
        }
    }
}
