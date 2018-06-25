using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace TwitterTools
{
    static class Tools
    {
        /// <summary>Хэширует сообщение методом <see cref="HMACSHA1"/></summary>
        /// <param name="message">Хэшируемое сообщение.</param>
        /// <param name="key">Ключ шифрования.</param>
        /// <returns></returns>
        public static string EncryptMessage(string message, string key)
        {
            string result;
            using (HMACSHA1 hasher = new HMACSHA1(Encoding.ASCII.GetBytes(key)))
            {
                result = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(message)));
            }
            return result;
        }

        /// <summary>Формирует ключ, используемый в дальнейшем для вычисления oauth_token.</summary>
        /// <param name="consumerSecret">oauth_consumer_secret</param>
        /// <param name="tokenSecret">oauth_token_secret</param>
        /// <returns></returns>
        public static string GetCompositeKey(string consumerSecret, string tokenSecret = null)
        {
            if (tokenSecret == null)
                return Uri.EscapeDataString(consumerSecret);
            else
                return Uri.EscapeDataString(consumerSecret) + "&" + Uri.EscapeDataString(tokenSecret);
        }

        /// <summary>Возвращает текущий TimeStamp в виде строки.</summary>
        public static string GetTimeStamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        }

        /// <summary>Возвращает случайно(псевдо) сгенерированную строку.</summary>
        public static string GetOauthNonce()
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(DateTime.Now.Ticks.ToString()));
        }

        /// <summary>Формирует и устанавливает Authorization-заголовок для запроса.</summary>
        /// <param name="request">Запрос, в который будет установлен заголовок.</param>
        /// <param name="user">Пользователь, для которого формируется запрос.</param>
        /// <param name="addParameters">Дополнительные параметры, передаваемые в запросе.</param>
        public static void SetAuthHeader(HttpRequestMessage request, UserData user, Dictionary<string, string> addParameters = null)
        {
            OauthSignature signature = new OauthSignature(request, user, addParameters);

            var sortedParams = new SortedDictionary<string, string>()
            {
                { "oauth_consumer_key", user.OauthConsumerKey },
                { "oauth_nonce", signature.OauthNonce },
                { "oauth_timestamp", signature.TimeStamp },
                { "oauth_signature_method", user.OauthSignatureMethod },
                { "oauth_signature", signature.Signature },
                { "oauth_version", user.OauthVersion }
            };
            if (user.OauthAccessToken != null)
                sortedParams.Add("oauth_token", user.OauthAccessToken);
            if (addParameters != null)
                foreach (var pair in addParameters)
                    sortedParams.Add(pair.Key, pair.Value);

            string header = "";
            foreach (var pair in sortedParams)
                header += pair.Key + "=" + "\"" + Uri.EscapeDataString(pair.Value) + "\"" + ", ";
            header = header.Substring(0, header.Length - 2);

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", header);
        }

        /// <summary>Возвращает подстроки исходного текста.</summary>
        /// <param name="text">Текст.</param>
        /// <param name="startText">Строка, с которой будет начинаться поиск.</param>
        /// <param name="endText">Строка, на которой будет заканчиваться поиск.</param>
        /// <param name="count">Количество извлекаемых подстрок.</param>
        public static string[] CutSubstrings(string text, string startText, string endText, ref int count)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < count; i++)
            {
                int startIndex = text.IndexOf(startText) + startText.Length;
                if (startIndex == -1)
                {
                    count = i;
                    break;
                }
                int length = text.IndexOf(endText, startIndex) - startIndex;

                string substr = text.Substring(startIndex, length);
                text = text.Substring(startIndex);
                if (substr == "")
                {
                    i--;
                    continue;
                }
                else
                    result.Add(substr);
            }
            return result.ToArray();
        }
    }
}
