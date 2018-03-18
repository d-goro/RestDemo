using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;
using ServiceStack;

namespace RestConsumer
{
    class Program
    {
        public const string Url = "http://api.openweathermap.org/data/2.5/find?q={0}&units=metric&appid=78ddad61cb484c82ae1e04ac12531fc8";
        public const string UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";

        static void Main(string[] args)
        {
            Console.WriteLine("Fetching the list of Weather data for specified city releases and their IDs and main description of weather");
            Console.WriteLine();
            Console.Write("Specify city name: ");
            var city = Console.ReadLine();
            var query = string.Format(Url, city);
            Console.WriteLine();
            Console.WriteLine("Select way of query:");
            Console.WriteLine("1. ServiceStack");
            Console.WriteLine("2. RestSharp");
            Console.WriteLine("3. HttpClient");
            Console.WriteLine("4. WebClient");
            Console.Write("Selection: ");

            Task<string> task = null;
            switch (Console.ReadLine())
            {
                case "1":
                    task = QueryWithServiceStack(query);
                    break;
                case "2":
                    task = QueryWithRestSharp(query);
                    break;
                case "3":
                    task = QueryWithHttpClient(query);
                    break;
                case "4":
                    task = QueryWithWebClient(query);
                    break;

            }

            Console.WriteLine("Waiting for response...");
            Console.WriteLine();
            task.Wait(4000);
            var response = task.Result;

            var token = JArray.Parse(JObject.Parse(response).GetValue("list").ToString());
            foreach (var child in token.Children<JObject>())
            {
                Console.WriteLine($"Name:{child.GetValue("name")}, " +
                                  $"ID:{child.GetValue("id")}, " +
                                  $"Weather: {child.GetValue("main").ToObject<Dictionary<string, string>>()["temp"]} Celcius ," +
                                  $"{child.GetValue("weather").First.ToObject<Dictionary<string, string>>()["main"]}");
            }

        }

        static async Task<string> QueryWithServiceStack(string url)
        {
            var response = await url.GetJsonFromUrlAsync(webRequest => { webRequest.UserAgent = UserAgent; });
            return response;
        }

        static async Task<string> QueryWithRestSharp(string url)
        {
            var client = new RestClient(url);

            var response = await client.ExecuteGetTaskAsync(new RestRequest()).ContinueWith(t => t.Result.Content);
            return response;
        }

        static async Task<string> QueryWithHttpClient(string url)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                var response = await httpClient.GetStringAsync(new Uri(url));

                return response;
            }
        }

        static async Task<string> QueryWithWebClient(string url)
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("User-Agent", UserAgent);
                var response = await webClient.DownloadStringTaskAsync(new Uri(url));
                return response;
            }
        }
    }
}
