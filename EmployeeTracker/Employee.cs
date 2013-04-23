using System;
using System.Text.RegularExpressions;

namespace EmployeeTracker
{
    public class Employee
    {
        readonly Regex _row2Regex = new Regex("\\<a href=\\\"/display/~(.*)\\\" class=\\\"confluence-userlink\\\" data-username=\\\"(.*)\\\" \\>(.*)\\</a\\>");
        readonly Regex _row2ErrorRegex = new Regex("\\<span class=\\\"error\\\"\\>&#91;~(.*)&#93;\\</span\\>");

        public Employee() { }

        public Employee(Match match)
        {
            var employeeId = match.Groups[1].ToString();
            var row2 = match.Groups[2].ToString();
            var row2Matches = _row2Regex.Match(row2);
            var username = row2Matches.Groups[3].ToString();
            if (string.IsNullOrEmpty(username))
            {
                Match match1 = _row2ErrorRegex.Match(row2);
                username = match1.Groups[1].ToString();
            }

            EmployeeId = int.Parse(employeeId);
            Name = username;
            LastSeen = DateTime.Now;
            FirstSeen = DateTime.Now;
        }

        public void MarkDeleted()
        {
            Deleted = DateTime.Now;
        }

        public bool IsDeleted
        {
            get { return Deleted != null; }
        }

        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public DateTime LastSeen { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime? Deleted { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, EmployeeId);
        }
    }
}