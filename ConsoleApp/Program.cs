using OpenIddict.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Step 1: Get access token from AuthServer");
            Console.WriteLine("Press enter to continune.");
            Console.ReadLine();
            using var client = new HttpClient();
            var token = string.Empty;
            try
            {
                //取得Access Token
                token = await GetTokenAsync(client);
                Console.WriteLine("Access token: {0}", token);
                Console.WriteLine();
            }
            catch (HttpRequestException exception)
            {
                Console.WriteLine(BuildExceptionMessage(exception));
            }

            Console.WriteLine("Step 2: Call Weather API endpoint");
            Console.WriteLine("Press enter to continune.");
            Console.ReadLine();

            try
            {
                //呼叫 Weather API
                var resource = await GetResourceAsync(client, token);
                Console.WriteLine("API response from WeatherForecastController: {0}", resource);

            }
            catch (HttpRequestException exception)
            {
                Console.WriteLine(BuildExceptionMessage(exception));
            }

            Console.WriteLine("Press enter to finish.");
            Console.ReadLine();
        }

        private static string BuildExceptionMessage(HttpRequestException exception)
        {
            var builder = new StringBuilder();
            builder.AppendLine("+++++++++++++++++++++");
            builder.AppendLine(exception.Message);
            builder.AppendLine(exception.InnerException?.Message);
            builder.AppendLine("+++++++++++++++++++++");
            return builder.ToString();
        }

        public static async Task<string> GetTokenAsync(HttpClient client)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44337/connect/token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = "my-console-app",
                ["client_secret"] = "388D45FA-B36B-4988-BA59-B187D329C207"
            });

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            var payload = await response.Content.ReadFromJsonAsync<OpenIddictResponse>();

            if (!string.IsNullOrEmpty(payload.Error))
            {
                throw new InvalidOperationException("An error occurred while retrieving an access token.");
            }

            return payload.AccessToken;
        }

        public static async Task<string> GetResourceAsync(HttpClient client, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44358/weatherforecast");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
