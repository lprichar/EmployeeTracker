Summary
======

Don't you hate it when an Employee leaves your company and you don't get the chance to say goodbye?

EmployeeTracker monitors a URL that contains a list of employees and employee id numbers and notifies you
when an employee is added to or removed from the list.

EmployeeTracker also keeps track of the hire dates and departure dates of all employees and reports on some statistics like employee retention.

Currently EmployeeTracker assumes that the content is stored in Confluence and so it logs in to an employee list via Confluences form authentication.

Configuration
======

After downloading and compiling you may need to change the location of the page to retrieve employees from.  The setting is located in the App.config file.

You may also need to fiddle with the Regular expressions that retrieve employee information.  This currently must be done in code (in Program.cs).

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