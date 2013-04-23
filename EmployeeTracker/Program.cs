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
            var deletedEmployees = DiffEmployees(previousEmployees.Where(i => !i.IsDeleted), currentEmployees).ToList();

            if (!currentEmployees.Any())
            {
                Console.WriteLine("Invalid password or some other error");
                return;
            }

            PrintNewEmployees(newEmployees);
            IdentifyDeletedEmployees(deletedEmployees, previousEmployees);
            UpdateLastSeenDates(previousEmployees);
            
            var newEmployeeList = previousEmployees.Union(newEmployees).ToList();
            PrintStats(newEmployeeList);
            employeeStore.SaveEmployees(newEmployeeList);
        }

        private static void PrintStats(List<Employee> newEmployeeList)
        {
            var lastFiveDeletedEmployees = newEmployeeList.Where(i => i.IsDeleted).OrderByDescending(i => i.Deleted).Take(5);
            var lastFiveAddedEmployees = newEmployeeList.Where(i => !i.IsDeleted).OrderByDescending(i => i.FirstSeen).Take(5);
            WriteSectionHeader("Recently Departed:");
            foreach (var employee in lastFiveDeletedEmployees)
            {
                Console.WriteLine("{0} ({1}) left {2:g}", employee.Name, employee.EmployeeId, employee.Deleted);
            }
            Console.WriteLine();

            WriteSectionHeader("Recently Hired:");
            foreach (var employee in lastFiveAddedEmployees)
            {
                Console.WriteLine("{0} ({1}) hired {2:g}", employee.Name, employee.EmployeeId, employee.FirstSeen);
            }
            Console.WriteLine();

            WriteSectionHeader("Stats");
            int activeEmployeeCount = newEmployeeList.Count(i => !i.IsDeleted);
            int deletedEmployeeCount = newEmployeeList.Count(i => i.IsDeleted);
            var churn = activeEmployeeCount == 0 ? 0 : deletedEmployeeCount / activeEmployeeCount;
            Console.WriteLine("Current Active Employees: " + activeEmployeeCount);
            Console.WriteLine("Known Deleted Employees: " + deletedEmployeeCount);
            Console.WriteLine("Churn: {0:p}", churn);
            Console.WriteLine();
        }

        private static void PrintNewEmployees(List<Employee> newEmployees)
        {
            WriteSectionHeader("Newly Hired Employees:");
            foreach (var newEmployee in newEmployees)
            {
                Console.WriteLine(newEmployee);
            }
            Console.WriteLine();
        }

        private static void WriteSectionHeader(string header)
        {
            Console.WriteLine("-----------------------------");
            Console.WriteLine(header);
            Console.WriteLine("-----------------------------");
        }
        
        private static void IdentifyDeletedEmployees(IList<Employee> deletedEmployees, IList<Employee> previousEmployees)
        {
            WriteSectionHeader("Newly Departed Employees:");
            foreach (var deletedEmployee in deletedEmployees)
            {
                Console.WriteLine(deletedEmployee);
                var previouslyDeletedEmployee = previousEmployees.First(i => i.EmployeeId == deletedEmployee.EmployeeId);
                previouslyDeletedEmployee.MarkDeleted();
            }
            Console.WriteLine();
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
