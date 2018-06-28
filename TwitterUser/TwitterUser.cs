using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using TwitterTools;
using System.Threading;

namespace TwitterAPI
{
    /// <summary>Представляет методы для взаимодействия с API.</summary>
    public class TwitterUser
    {
        private UserData user;
        private HttpClient Client;

        /// <summary>Ициниализация, с авторизацией пользователя.</summary>
        public TwitterUser()
        {
            user = new UserData();
            Client = new HttpClient();
        }
        /// <summary>Инициализация с уже известными данными о пользователе.</summary>
        /// <param name="accessToken">oauth_token</param>
        /// <param name="accessTokenSecret">oauth_token_secret</param>
        /// <param name="userID">user_id</param>
        /// <param name="userName">screen_name</param>
        public TwitterUser(string accessToken, string accessTokenSecret, string userID, string userName)
        {
            user = new UserData(accessToken, accessTokenSecret, userID, userName);
            Client = new HttpClient();
        }

        /// <summary>Авторизовывает пользователя, на основе ПИН-кода</summary>
        private void AuthoriseMe()
        {
            // Получаем временные ключи
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.twitter.com/oauth/request_token");
            Tools.SetAuthHeader(request, user, new Dictionary<string, string>() { { "oauth_callback", "oob" } });

            HttpResponseMessage response = Client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("Сервер вернул ошибку: " + result);

            MatchCollection tokens = Regex.Matches(result, "oauth_token=([^&]+)&oauth_token_secret=([^&]+)&oauth_callback_confirmed=true");
            user = new UserData(tokens[0].Groups[1].Value, tokens[0].Groups[2].Value);
            // Посылаем пользователя за пин-кодом
            System.Diagnostics.Process.Start("https://api.twitter.com/oauth/authorize?oauth_token=" + user.OauthAccessToken);

            Console.WriteLine("Введите пин-код, полученный на сайте Twitter и нажмите Enter");
            string pin = Console.ReadLine();
            // Если пин-код верный то получаем настоящие ключи, если нет, то мучаем его пока он не введет верный
            while (true)
            {
                request = new HttpRequestMessage(HttpMethod.Post, "https://api.twitter.com/oauth/access_token");
                Tools.SetAuthHeader(request, user, new Dictionary<string, string>() { { "oauth_verifier", pin } });
                response = Client.SendAsync(request).Result;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine("Пин-код введен неправильно! Я закрываюсь!");
                    // Тут должно быть зацикливание, но я его еще не придумал
                    Thread.Sleep(3000);
                    Environment.Exit(0);
                }
                else
                    break;
            }

            result = response.Content.ReadAsStringAsync().Result;

            MatchCollection data = Regex.Matches(result, @"oauth_token=([^&]+)&oauth_token_secret=([^&]+)&user_id=(\d+)&screen_name=(.+)");
            user = new UserData(data[0].Groups[1].Value, data[0].Groups[2].Value, data[0].Groups[3].Value, data[0].Groups[4].Value);
        }

        /// <summary>Отправка твитта в авторизованный аккаунт.</summary>
        /// <param name="message">Текст твитта.</param>
        public void StatusUpdate(string message)
        {
            if (user.UserName == null)
                AuthoriseMe();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.twitter.com/1.1/statuses/update.json");
            Tools.SetAuthHeader(request, user, new Dictionary<string, string>() { { "status", message } });

            string postBody = "status=" + Uri.EscapeDataString(message);
            request.Content = new StringContent(postBody, Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpResponseMessage response = Client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            if (response.StatusCode != HttpStatusCode.OK)
                if (result.Contains("\"code\":186"))
                {
                    StatusUpdate(message.Substring(0, 270));
                    StatusUpdate(message.Substring(270));
                    Console.WriteLine("Т.к. длинна твитта была слишком большая, было отправлено 2 твитта.");
                }
                else if (result.Contains("\"code\":187"))
                    Console.WriteLine("У вас уже есть такой твитт!");
                else
                    throw new Exception("Сервер вернул ошибку: " + result);
            else
                Console.WriteLine("Твитт успешно отправлен к вам на страницу.");

        }

        /// <summary>Грязный хак. Парсим html твиттера.</summary>
        /// <param name="userName">Пользователь, чьи твитты будут читаться.</param>
        /// <param name="count">Количество последних твиттов.</param>
        public string[] GetLastTweets(string userName, ref int count)
        {
            HttpResponseMessage response = Client.GetAsync("https://twitter.com/" + userName).Result;
            string pageText = response.Content.ReadAsStringAsync().Result;

            Regex pattern = new Regex(@"\s<div class=""js-tweet-text-container"">\s(.*)\s</div>\s");
            Match match = pattern.Match(pageText);
            List<string> tweets = new List<string>();
            while (match.Success)
            {
                tweets.Add(Regex.Replace(match.Value, @"<[^>]+>", ""));
                match = match.NextMatch();
                if (tweets.Count == count)
                    break;
            }
            if (tweets.Count == 0)
                throw new ArgumentException("Такого пользователя не существует, его аккаунт закрыт, либо у него нет ни одного твитта");

            return tweets.ToArray();
        }
    }
}
