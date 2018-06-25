using System;
using System.Collections.Generic;
using System.Net.Http;

namespace TwitterTools
{
    /// <summary>Представляет сигнатуру, необходимую для Authorization-заголовка.</summary>
    internal class OauthSignature
    {
        public string OauthNonce { get; set; }
        public string TimeStamp { get; set; }
        public readonly string Signature;

        /// <summary>Инициализирует сигнатуру.</summary>
        /// <param name="request">Запрос, для которого формируется сигнатура.</param>
        /// <param name="user">Пользователь, для которого формируется сигнатура.</param>
        /// <param name="parameters">Дополнительные параметры запроса.</param>
        public OauthSignature(HttpRequestMessage request, UserData user, Dictionary<string, string> parameters = null)
        {
            OauthNonce = Tools.GetOauthNonce();
            TimeStamp = Tools.GetTimeStamp();

            SortedDictionary<string, string> sortedParams = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", user.OauthConsumerKey },
                { "oauth_signature_method", user.OauthSignatureMethod },
                { "oauth_version", user.OauthVersion },
                { "oauth_nonce", OauthNonce },
                { "oauth_timestamp", TimeStamp }
            };
            if (user.OauthAccessToken != null)
                sortedParams.Add("oauth_token", user.OauthAccessToken);
            if (parameters != null)
                foreach (var pair in parameters)
                    sortedParams.Add(pair.Key, Uri.EscapeDataString(pair.Value));

            string sign = "";
            foreach (var pair in sortedParams)
                sign += pair.Key + "=" + pair.Value + "&";
            sign = sign.Substring(0, sign.Length - 1);

            sign = request.Method.Method + "&" + Uri.EscapeDataString(request.RequestUri.ToString()) + "&"
                   + Uri.EscapeDataString(sign);

            Signature = Tools.EncryptMessage(sign, user.CompositeKey);
        }
    }
}
