using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TwitterAPI;
using TwitterAPI.JsonModels;

namespace TestSolution
{
    class Program
    {
        static void Main(string[] args)
        {
            TwitterUser User = new TwitterUser();

            Console.WriteLine("Для получения статистики введите имя аккаунта Twitter-пользователя и нажмите Enter.");
            string input = Console.ReadLine();
            string statistic = "";
            int count = 5;
            while (input != "")
            {
                if (Regex.IsMatch(input.Replace("@", ""), @"^([A-Za-z0-9_]){1,15}$"))
                {
                    try
                    {
                        var tweets = User.GetLastTweetsHack(input, ref count);
                        statistic = GetStatistic(tweets);
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("Пользователь не существует, его аккаунт закрыт или у него нет твиттов. Попробуйте еще раз.");
                        input = Console.ReadLine();
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine("Неправильно введено имя аккаунта (латинские буквы, цифры и знак '_' до 15 символов). Символ '@' игнорируется.");
                    input = Console.ReadLine();
                    continue;
                }
                string output = String.Format("{0}, статистика для последних {1} твитов: {2}", input, count, statistic);
                Console.WriteLine(output);

                Console.WriteLine("Если вы хотите запостить эту статистику к себе в твиттер, то введите -post. Для продолжения нажмите Enter.");
                if (Console.ReadLine() == "-post")
                    User.StatusUpdate(output);

                Console.WriteLine("Чтобы получить статистику по еще одному аккаунту, повторите ввод. Для выхода нажмите Enter.");
                input = Console.ReadLine();
            }
        }

        /// <summary>Возвращает частотность букв, используемых в твиттах.</summary>
        /// <param name="tweets">Твитты, полученные от <see cref="TwitterUser.GetLastTweets(string, int)"/></param>
        static string GetStatistic(params Tweet[] tweets)
        {
            SortedDictionary<char, int> alphabet = new SortedDictionary<char, int>();
            int totalCharQuantity = 0;
            foreach (var tweet in tweets)
            {
                string text = Regex.Replace(tweet.Text.ToLower(), @"[^a-zA-ZА-Яа-я]", "");
                totalCharQuantity += text.Length;
                foreach (char symbol in text)
                {
                    if (alphabet.Keys.Contains(symbol))
                        alphabet[symbol]++;
                    else
                        alphabet.Add(symbol, 1);
                }
            }

            string result = "{";
            foreach (var ch in alphabet)
            {
                string percent = (Convert.ToDouble(ch.Value) / totalCharQuantity).ToString();
                result += String.Format("{0}:{1}, ", ch.Key, percent.Substring(0, 6));
            }
            result = result.Substring(0, result.Length - 2) + "}";
            return result;
        }

        /// <summary>Возвращает частотность букв, используемых в твиттах.</summary>
        /// <param name="tweets">Твитты, полученные от <see cref="TwitterUser.GetLastTweetsHack(string, ref int)"/></param>
        static string GetStatistic(params string[] tweets)
        {
            SortedDictionary<char, int> alphabet = new SortedDictionary<char, int>();
            int totalCharQuantity = 0;
            foreach (var tweet in tweets)
            {
                string text = Regex.Replace(tweet.ToLower(), @"[^a-zA-ZА-Яа-я]", "");
                totalCharQuantity += text.Length;
                foreach (char symbol in text)
                {
                    if (alphabet.Keys.Contains(char.ToLower(symbol)))
                        alphabet[char.ToLower(symbol)]++;
                    else
                        alphabet.Add(char.ToLower(symbol), 1);
                }
            }

            string result = "{";
            foreach (var ch in alphabet)
            {
                string percent = (Convert.ToDouble(ch.Value) / totalCharQuantity).ToString();
                result += String.Format("{0}:{1}, ", ch.Key, percent.Substring(0, 6));
            }
            result = result.Substring(0, result.Length - 2) + "}";
            return result;
        }
    }
}
