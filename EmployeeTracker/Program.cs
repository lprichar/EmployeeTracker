using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace EmployeeTracker
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                VerifyEmployees(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
            Console.WriteLine("Press Any Key");
            Console.ReadKey();
        }

        private static void VerifyEmployees(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("usage: EmployeeTracker.exe username password");
                return;
            }
            var employees = GetEmployees(args[0], args[1]).Result.ToList();
            if (!employees.Any())
            {
                Console.WriteLine("Invalid password or some other error");
                return;
            }
            File.WriteAllText("out.html", string.Join("\n", employees.Select(i => i.ToString())));
        }

        private static async Task<IEnumerable<Employee>> GetEmployees(string username, string password)
        {
            var htmlResult = await GetEmployeeNumbersPageFromMobius(username, password);
            var matchCollection = Regex.Matches(htmlResult, "\\<td class=\\\"confluenceTd\\\"\\>\\<p\\>(.*)\\<\\/p\\>\\<\\/td\\>\n\\<td class=\\\"confluenceTd\\\"\\>\\<p\\>(.*)\\<\\/p\\>\\<\\/td\\>\n");
            
            return matchCollection.Cast<Match>().Select(i => new Employee(i));
        }

        private static async Task<string> GetEmployeeNumbersPageFromMobius(string username, string password)
        {
            var baseAddress = new Uri("https://mobius.nearinfinity.com");

            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler {CookieContainer = cookieContainer})
            using (var client = new HttpClient(handler) {BaseAddress = baseAddress})
            {
                var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("os_username", username),
                        new KeyValuePair<string, string>("os_password", password),
                        new KeyValuePair<string, string>("os_cookie", "true"),
                        new KeyValuePair<string, string>("login", "Log In"),
                        new KeyValuePair<string, string>("os_destination", "/display/HR/Employee+Numbers")
                    });
                var result = client.PostAsync("/dologin.action", content).Result;
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStringAsync();
            }
        }
    }
}
