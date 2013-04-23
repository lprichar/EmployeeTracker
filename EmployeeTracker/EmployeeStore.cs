using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;

namespace EmployeeTracker
{
    public class EmployeeStore
    {
        private const string FileName = "EmployeeList.csv";
        
        public IEnumerable<Employee> GetPreviousEmployees()
        {
            if (!File.Exists(FileName)) return Enumerable.Empty<Employee>();
            var csv = new CsvReader(new StreamReader(FileName));
            return csv.GetRecords<Employee>();
        }

        public void SaveEmployees(IEnumerable<Employee> employees)
        {
            using (var csv = new CsvWriter(new StreamWriter(FileName)))
            {
                csv.WriteRecords(employees);
            }
        }
    }
}