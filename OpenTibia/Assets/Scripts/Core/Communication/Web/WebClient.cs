using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OpenTibiaUnity.Core.Communication.Web
{
    public enum RequestType
    {
        Login,
        CacheInfo,
        News,
        BoostedCreature,
        EventSchedule,
    }
    
    public abstract class WebClient
    {
        private static HttpClient s_HttpClient = null;

        public readonly int ClientVersion;
        public readonly int BuildVersion;

        public WebClient(int clientVersion, int buildVersion) {
            ClientVersion = clientVersion;
            BuildVersion = buildVersion;
        }

        protected async void Connect(string requestUri, RequestType type, Dictionary<string, string> requestData) {
            try {
                var request = CreateRequest(requestUri, type, requestData);
                var response = await SendAsync(request);
                OnResonseReceived(response);
            } catch (HttpRequestException e) {
                UnityEngine.Debug.Log("HttpRequestException: " + e);
                OnParsingFailed();
            } catch (Exception e) {
                UnityEngine.Debug.Log("Exception: " + e);
                OnParsingFailed();
            }
        }

        protected HttpRequestMessage CreateRequest(string requestUri, RequestType type, Dictionary<string, string> requestData) {
            string requestType = GetStringType(type);
            var requestStr = CreateRequestString(requestType, requestData);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(requestStr, Encoding.UTF8, "application/json");
            request.Headers.Add("User-Agent", "Mozilla/5.0");
            request.Headers.Add("Connection", "Keep-Alive");
            request.Headers.Add("Accept-Language", "en-US,*");
            return request;
        }

        protected async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) {
            if (s_HttpClient == null)
                s_HttpClient = new HttpClient();

            return await s_HttpClient.SendAsync(request);
        }

        protected async virtual void OnResonseReceived(HttpResponseMessage response) {
            string content = await response.Content.ReadAsStringAsync();

            OnResponseContentReceived(content);
        }

        protected virtual void OnResponseContentReceived(string content) {
            try {
                var jsonDeserializedObj = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                var @object = (JObject)jsonDeserializedObj;
                if (@object == null) {
                    OnParsingFailed();
                    return;
                }

                OnParsingSuccess(@object);
            } catch (Exception) {
                OnParsingFailed();
            }
        }

        protected abstract void OnParsingSuccess(JObject @object);
        protected abstract void OnParsingFailed();

        protected static string CreateRequestString(string requestType, Dictionary<string, string> requestData) {
            var @object = new JObject();
            @object["type"] = requestType;

            foreach (var pair in requestData)
                @object[pair.Key] = pair.Value;

            return @object.ToString();
        }

        protected static string GetStringType(RequestType type) {
            switch (type) {
                case RequestType.Login: return "login";
                case RequestType.CacheInfo: return "cacheinfo";
                case RequestType.News: return "news";
                case RequestType.BoostedCreature: return "boostedcreature";
                case RequestType.EventSchedule: return "eventschedule";
            }

            return string.Empty;
        }

        protected static string SafeString(JToken token, string @default = "") {
            return token != null ? token.ToString() : @default;
        }

        protected static int SafeInt(JToken token, int @default = 0) {
            if (token != null && int.TryParse(token.ToString(), out int result))
                return result;
            return @default;
        }

        protected static uint SafeUint(JToken token, uint @default = 0) {
            if (token != null && uint.TryParse(token.ToString(), out uint result))
                return result;
            return @default;
        }

        protected static bool SafeBool(JToken token, bool @default = false) {
            if (token == null)
                return @default;

            bool? res = (bool)token;
            return res.HasValue ? res.Value : @default;
        }

        public static bool operator !(WebClient instance) {
            return instance == null;
        }

        public static bool operator true(WebClient instance) {
            return !!instance;
        }

        public static bool operator false(WebClient instance) {
            return !instance;
        }
    }
}
