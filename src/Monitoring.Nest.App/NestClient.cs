using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Monitoring.Nest.App.Model;
using Newtonsoft.Json;

namespace Monitoring.Nest.App
{
    public class NestClient
    {
        readonly string _email;
        readonly string _password;
        readonly JsonSerializerSettings _serializerSettings;
        readonly string _loginUrl = "https://home.nest.com";
        readonly IFlurlClient _flurl = new FlurlClient();
        readonly Random _rand = new Random();
        readonly DateTimeOffset _unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        Session _session = null;
        string _sessionId = null;


        public NestClient(string email, string password, JsonSerializerSettings serializerSettings)
        {
            _email = email;
            _password = password;
            _serializerSettings = serializerSettings;

            _flurl.Configure(settings =>
            {
                settings.JsonSerializer = new NewtonsoftJsonSerializer(_serializerSettings);
            });
        }

        public async Task<UserData> GetUserDataAsync()
        {
            await EnsureLoggedInAsync();

            var endpoint = _loginUrl.AppendPathSegment("api/0.1/")
                .AppendPathSegment("user")
                .AppendPathSegment(_session.Userid)
                .AppendPathSegment("app_launch");
            return await _flurl.Request(endpoint)
                .WithHeader("Authorization", "Basic " + _session.AccessToken)
                .PostJsonAsync(new { KnownBucketTypes = GetKnownBucketTypes(), KnownBucketVersions = new string[0] })
                .ReceiveJson<UserData>();
        }

        public async Task<IEnumerable<ObjectData>> SubscribeAsync(IEnumerable<ObjectHeader> objectHeadersToSubscribe)
        {
            await EnsureLoggedInAsync();
            EnsureSessionIdExists();

            string objectDataStr = null;
            try
            {
                var endpoint = _session.Urls.TransportUrl.AppendPathSegment("v5")
                    .AppendPathSegment("subscribe");
                objectDataStr = await _flurl.Request(endpoint)
                    .WithHeader("Authorization", "Basic " + _session.AccessToken)
                    .WithTimeout(30)
                    .PostJsonAsync(new { Objects = objectHeadersToSubscribe.ToList(), Session = _sessionId, Timeout = 30 })
                    .ReceiveString();
            }
            catch (OperationCanceledException) { }

            if (string.IsNullOrEmpty(objectDataStr))        //This may be null because the endpoint failed, or because the endpoint returned no new data
                return new ObjectData[0];

            var objectDatas = JsonConvert.DeserializeObject<ObjectDatas>(objectDataStr, _serializerSettings);
            return objectDatas.Objects;
        }

        public void EnsureSessionIdExists()
        {
            if (_sessionId != null)
                return;

            int date = (int)(DateTimeOffset.Now - _unixEpoch).TotalMilliseconds;
            string random = _rand.Next().ToString("D5").Substring(5);
            _sessionId = $"{_session.Userid}.{random}.{date}";
        }

        async Task EnsureLoggedInAsync()
        {
            if (_session != null)
                return;
            _session = await SessionLoginAsync();
        }

        async Task<Session> SessionLoginAsync()
        {
            string endpoint = _loginUrl
                .AppendPathSegment("session");
            return await _flurl.Request(endpoint)
                .PostJsonAsync(new { Email = _email, Password = _password })
                .ReceiveJson<Session>();
        }

        IEnumerable<string> GetKnownBucketTypes()
        {
            return new[]
            {
                "buckets",
                "delayed_topaz",
                "demand_charge",
                "demand_charge_event",
                "demand_response",
                "device",
                "device_alert_dialog",
                "geofence_info",
                "link",
                "message",
                "message_center",
                "metadata",
                "occupancy",
                "quartz",
                "safety",
                "rate_plan",
                "rcs_settings",
                "safety_summary",
                "schedule",
                "shared",
                "structure",
                "structure_history",
                "structure_metadata",
                "topaz",
                "topaz_resource",
                "tou",
                "track",
                "trip",
                "tuneups",
                "user",
                "user_alert_dialog",
                "user_settings",
                "utility",
                "where",
                "widget_track",
                "kryptonite",
            };
        }
    }
}
