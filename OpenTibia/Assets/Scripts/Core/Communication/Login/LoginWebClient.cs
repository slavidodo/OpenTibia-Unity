using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Login
{
    public enum LoginWebClientError
    {
        None = 0,
        Technical = 1,
        // 2 unknown
        Login = 3,
        // 4, 5 unknown
        Authentication = 6,
    }

    public class LoginWebClient : Web.WebClient
    {
        public class LoginErrorEvent : UnityEvent<string> { };
        public class LoginSuccessEvent : UnityEvent<PlayData> { };

        public string AccountIdentifier { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }

        public LoginErrorEvent onTechnicalError = new LoginErrorEvent();
        public LoginErrorEvent onLoginError = new LoginErrorEvent();
        public LoginErrorEvent onTokenError = new LoginErrorEvent();
        public LoginSuccessEvent onLoginSuccess = new LoginSuccessEvent();

        public LoginWebClient(int clientVersion, int buildVersion) : base(clientVersion, buildVersion) { }

        protected static string GetAccountIdentifierField() {
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameAccountEmailAddress))
                return "email";
            return "accountname";
        }

        public void LoadDataAsync(string requestURI) {
            var requestData = new Dictionary<string, string>();
            requestData.Add(GetAccountIdentifierField(), AccountIdentifier);
            requestData.Add("password", Password);
            if (Token.Length != 0)
                requestData.Add("token", Token);
            
            Connect(requestURI, Web.RequestType.Login, requestData);
        }

        protected override void OnParsingSuccess(JObject @object) {
            LoginWebClientError errorCode = LoginWebClientError.None;
            string errorMessage = string.Empty;
            foreach (var property in @object.Properties()) {
                if (property.Name == "errorCode")
                    errorCode = (LoginWebClientError)SafeInt(property.Value, 0);
                else if (property.Name == "errorMessage")
                    errorMessage = (string)property.Value;
            }

            if (errorCode == LoginWebClientError.Technical) {
                onTechnicalError.Invoke(errorMessage);
                return;
            } else if (errorCode == LoginWebClientError.Login) {
                onLoginError.Invoke(errorMessage);
                return;
            } else if (errorCode == LoginWebClientError.Authentication) {
                onTokenError.Invoke(errorMessage);
                return;
            } else if (errorCode != LoginWebClientError.None) {
                InvokeTechnicalParsingError();
                return;
            }

            var sessionObject = @object["session"] as JObject;
            var playDataObject = @object["playdata"] as JObject;
            if (sessionObject == null || playDataObject == null) {
                InvokeTechnicalParsingError();
                return;
            }

            var worldsArray = playDataObject["worlds"] as JArray;
            var charactersArray = playDataObject["characters"] as JArray;
            if (sessionObject == null || playDataObject == null) {
                InvokeTechnicalParsingError();
                return;
            }

            JToken sessionKeyToken, lastLoginTimeToken, statusToken;
            if (!sessionObject.TryGetValue("sessionkey", out sessionKeyToken)
                || !sessionObject.TryGetValue("lastlogintime", out lastLoginTimeToken)
                || !sessionObject.TryGetValue("status", out statusToken)) {
                InvokeTechnicalParsingError();
                return;
            }

            var playdata = new PlayData();
            playdata.Session = new PlayData.PlayDataSession {
                Key = SafeString(sessionKeyToken),
                Status = SafeString(statusToken),
                LastLoginTime = SafeUint(lastLoginTimeToken),
                PremiumUntil = SafeUint(sessionObject.GetValue("premiumuntil")),
                IsPremium = SafeBool(sessionObject.GetValue("ispremium")),
            };

            if (ClientVersion >= 1148) {
                playdata.Session.FpsTracking = (bool)sessionObject.GetValue("fpstracking");
                playdata.Session.IsReturner = (bool)sessionObject.GetValue("isreturner");
                playdata.Session.ReturnerNotification = (bool)sessionObject.GetValue("returnernotification");
                playdata.Session.ShowRewardNews = (bool)sessionObject.GetValue("showrewardnews");
            }

            if (ClientVersion >= 1149 && BuildVersion >= 5921) {
                playdata.Session.OptionTracking = (bool)sessionObject.GetValue("optiontracking");
            }

            List<int> worldIds = new List<int>();
            foreach (var worldObject in worldsArray.Children<JObject>()) {
                JToken idToken, nameToken, previewStateToken;
                if (!worldObject.TryGetValue("id", out idToken)
                    || !worldObject.TryGetValue("name", out nameToken)
                    || !worldObject.TryGetValue("previewstate", out previewStateToken))
                    continue;

                int worldId = SafeInt(idToken);
                if (worldIds.Contains(worldId))
                    continue;

                string externalAddress = string.Empty,
                    externalAddressProtected = string.Empty,
                    externalAddressUnprotected = string.Empty;

                int externalPort = 0,
                    externalPortProtected = 0,
                    externalPortUnprotected = 0;

                bool antiCheatProtection = true;

                if (ClientVersion >= 1148) {
                    JToken externalAddressProtectedToken, externalPortProtectedToken;
                    if (!worldObject.TryGetValue("externaladdressprotected", out externalAddressProtectedToken)
                        || !worldObject.TryGetValue("externalportprotected", out externalPortProtectedToken))
                        continue;

                    externalAddressProtected = SafeString(externalAddressProtectedToken);
                    externalPortProtected = SafeInt(externalPortProtectedToken);
                }

                if (ClientVersion >= 1149 && BuildVersion >= 5921) {
                    JToken externalAddressUnprotectedToken, externalPortUnprotectedToken;
                    if (!worldObject.TryGetValue("externaladdressunprotected", out externalAddressUnprotectedToken)
                        || !worldObject.TryGetValue("externalportunprotected", out externalPortUnprotectedToken))
                        continue;


                    externalAddressUnprotected = SafeString(externalAddressUnprotectedToken);
                    externalPortUnprotected = SafeInt(externalPortUnprotectedToken);
                } else {
                    JToken externalAddressToken, externalPortToken;
                    if (!worldObject.TryGetValue("externaladdress", out externalAddressToken)
                        || !worldObject.TryGetValue("externalport", out externalPortToken))
                        continue;

                    externalAddress = SafeString(externalAddressToken);
                    externalPort = SafeInt(externalPortToken);
                }

                if (ClientVersion >= 1148 && worldObject.TryGetValue("anticheatprotection", out JToken antiChearProtectionToken))
                    antiCheatProtection = SafeBool(antiChearProtectionToken);

                worldIds.Add(worldId);
                playdata.Worlds.Add(new PlayData.PlayDataWorld() {
                    Id = worldId,
                    PreviewState = SafeInt(previewStateToken),
                    ExternalPort = externalPort,
                    ExternalPortProtected = externalPortProtected,
                    ExternalPortUnprotected = externalPortUnprotected,

                    Name = SafeString(nameToken),
                    ExternalAddress = externalAddress,
                    ExternalAddressProtected = externalAddressProtected,
                    ExternalAddressUnprotected = externalAddressUnprotected,
                    AntiCheatProtection = antiCheatProtection,

                    // 12.00
                    PvpType = SafeInt(worldObject.GetValue("pvptype"), -1),

                    // 12.15
                    IsTournamentActive = SafeBool(worldObject.GetValue("istournamentactive")),
                    IsTournamentWorld = SafeBool(worldObject.GetValue("istournamentworld")),
                    RestrictStore = SafeBool(worldObject.GetValue("restrictedstore")),
                    CurrentTournamentPhase = SafeInt(worldObject.GetValue("currenttournamentphase"), -1),

                    // 12.20
                    IsMainCharacter = SafeBool(worldObject.GetValue("ismaincharacter")),
                });
            }

            foreach (var characterObject in charactersArray.Children<JObject>()) {
                JToken worldIdToken, nameToken;
                if (!characterObject.TryGetValue("worldid", out worldIdToken)
                    || !characterObject.TryGetValue("name", out nameToken))
                    continue;

                int worldId = SafeInt(worldIdToken);
                if (!worldIds.Contains(worldId))
                    continue;

                var character = new PlayData.PlayDataCharacter() {
                    WorldId = worldId,
                    Name = SafeString(nameToken),

                    // 11.00 (x?)
                    IsMale = SafeBool(characterObject.GetValue("ismale")),
                    Tutorial = SafeBool(characterObject.GetValue("tutorial")),

                    // 12.00
                    Level = SafeInt(characterObject.GetValue("level")),
                    OutfitId = SafeInt(characterObject.GetValue("outfitid")),
                    HeadColor = SafeInt(characterObject.GetValue("headcolor")),
                    TorsoColor = SafeInt(characterObject.GetValue("torsocolor")),
                    LegsColor = SafeInt(characterObject.GetValue("legscolor")),
                    DetailColor = SafeInt(characterObject.GetValue("detailcolor")),
                    AddonsFlags = SafeInt(characterObject.GetValue("addonsflags")),

                    Vocation = SafeString(characterObject.GetValue("vocation")),

                    IsHidden = SafeBool(characterObject.GetValue("ishidden")),

                    // 12.15
                    IsTournamentParticipant = SafeBool(characterObject.GetValue("istournamentparticipant")),
                    RemainingDailyTournamentPlaytime = SafeInt(characterObject.GetValue("remainingdailytournamentplaytime")),
                };

                playdata.Characters.Add(character);
            }

            onLoginSuccess.Invoke(playdata);
        }
        
        protected override void OnParsingFailed() {
            InvokeTechnicalParsingError();
        }

        private void InvokeTechnicalParsingError() {
            if (ClientVersion >= 1200)
                onTechnicalError.Invoke(TextResources.LOGIN_WEB_CLIENT_LOGIN_ERROR_TECHNICAL_V12);
            else
                onTechnicalError.Invoke(TextResources.LOGIN_WEB_CLIENT_LOGIN_ERROR_RESPONSE_PARSING);
        }
    }
}
