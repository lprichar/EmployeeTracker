Summary
======

Don't you hate it when a co-worker leaves your company and you never get the chance to say goodbye?

EmployeeTracker solves the problem of Employers not notifying their employees of departures or new hires by monitoring 
a URL that contains a list of employees and employee id numbers.  

The software notifies you when employees are added to or removed from the list.  It also keeps track of 
the hire and departure dates of all employees and reports on some basic statistics like employee retention.

Finally EmployeeTracker stores all information in a .csv file so you can easly generate your own statistics
in a spreadsheet application.

Caveats
======

Currently EmployeeTracker assumes that the content is stored in Confluence and so it logs in to an employee 
list via Confluence's forms authentication.

Furthermore, the regular expressions that retrieves employee information will probably require some manual 
modification (in Program.cs).  If there is interest in this project I'll be happy to make this logic 
more generic.

Configuration
======

After downloading and compiling you may need to change the location of the page to retrieve employees from.  The setting is located in the App.config file.

Setup
======

EmployeeTracker is a command line app that must be run on a schedule (ideally daily).  After downloading and compiling add 
a schedule in windows:

1. Hit Windows-R
2. Type taskschd.msc
3. Click Action->Create Task
4. Give it a name
5. Under Triggers give add a schedule like Daily and Stop if task runs longer than 30 minutes
6. Under actions add "Start a program" and select C:\dev\EmployeeTracker\EmployeeTracker\bin\Debug\EmployeeTracker.exe.  Under arguments add your username and a space and your password.
