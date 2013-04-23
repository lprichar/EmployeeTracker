using System;
using System.Collections.Generic;
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
            string username = args[0];
            string password = args[1];
            
            EmployeeStore employeeStore = new EmployeeStore();
            var currentEmployees = GetCurrentEmployees(username, password).Result.ToList();
            var previousEmployees = employeeStore.GetPreviousEmployees();
            var newEmployees = DiffEmployees(currentEmployees, previousEmployees).ToList();
            var deletedEmployees = DiffEmployees(previousEmployees, currentEmployees).ToList();

            if (!currentEmployees.Any())
            {
                Console.WriteLine("Invalid password or some other error");
                return;
            }

            PrintNewEmployees(newEmployees);
            IdentifyDeletedEmployees(deletedEmployees, previousEmployees);
            UpdateLastSeenDates(previousEmployees);
            SaveEmployeesList(employeeStore, previousEmployees, newEmployees);
        }

        private static void SaveEmployeesList(EmployeeStore employeeStore, IList<Employee> previousEmployees, IList<Employee> newEmployees)
        {
            employeeStore.SaveEmployees(previousEmployees.Union(newEmployees));
        }

        private static void PrintNewEmployees(List<Employee> newEmployees)
        {
            if (newEmployees.Any())
            {
                Console.WriteLine("New Employees:");
            }
            foreach (var newEmployee in newEmployees)
            {
                Console.WriteLine(newEmployee);
            }
        }

        private static void IdentifyDeletedEmployees(IList<Employee> deletedEmployees, IList<Employee> previousEmployees)
        {
            if (deletedEmployees.Any())
            {
                Console.WriteLine("Deleted Employees:");
            }
            foreach (var deletedEmployee in deletedEmployees)
            {
                Console.WriteLine(deletedEmployee);
                var previouslyDeletedEmployee = previousEmployees.First(i => i.EmployeeId == deletedEmployee.EmployeeId);
                previouslyDeletedEmployee.MarkDeleted();
            }
        }

        private static void UpdateLastSeenDates(IList<Employee> previousEmployees)
        {
            foreach (var previousEmployee in previousEmployees)
            {
                if (!previousEmployee.IsDeleted)
                {
                    previousEmployee.LastSeen = DateTime.Now;
                }
            }
        }

        private static IEnumerable<Employee> DiffEmployees(IEnumerable<Employee> currentEmployees, IEnumerable<Employee> previousEmployees)
        {
            var previousEmployeesDict = previousEmployees.ToDictionary(i => i.EmployeeId, i => i.Name);
            return currentEmployees.Where(current => !previousEmployeesDict.ContainsKey(current.EmployeeId));
        }

        private static async Task<IEnumerable<Employee>> GetCurrentEmployees(string username, string password)
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
