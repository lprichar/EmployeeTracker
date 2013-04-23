using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EmployeeTracker
{
    public class EmployeeStore
    {
        private const string FileName = "EmployeeList.csv";
        
        public IEnumerable<Employee> GetPreviousEmployees()
        {
            if (!File.Exists(FileName)) return Enumerable.Empty<Employee>();
            var previousEmployeesStr = File.ReadAllText(FileName);
            var previousEmployeesList = previousEmployeesStr.Split('\n');
            return previousEmployeesList.Select(i => new Employee(i));
        }

        public void SaveEmployees(IEnumerable<Employee> employees)
        {
            IEnumerable<string> employeesAsString = employees.Select(i => i.ToString());
            File.WriteAllText(FileName, string.Join("\n", employeesAsString));
        }
    }
}