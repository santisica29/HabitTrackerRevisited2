# Habit Logger - Console App 
Console based CRUD application to log habits. 
Developed using C# and SQLite.

## Requirements: 
- [x] This is an application where you’ll log occurrences of a habit.

- [x] This habit can't be tracked by time (ex. hours of sleep), only by quantity (ex. number of water glasses a day)

- [x] Users need to be able to input the date of the occurrence of the habit

- [x] The application should store and retrieve data from a real database

- [x] When the application starts, it should create a sqlite database, if one isn’t present.

- [x] It should also create a table in the database, where the habit will be logged.

- [x] The users should be able to insert, delete, update and view their logged habit.

- [x] You should handle all possible errors so that the application never crashes.

- [x] You can only interact with the database using ADO.NET. You can’t use mappers such as Entity Framework or Dapper.

- [x] Follow the DRY Principle, and avoid code repetition.

- [x] Your project needs to contain a Read Me file where you'll explain how your app works and tell a little bit about your thought progress. What was hard? What was easy? What have you learned? Here's a nice example:

- [x] Check for incorrect dates. What happens if a menu option is chosen that's not available? What happens if the users input a string instead of a number?

## Extra challenges
- [x] Use parametetrized queries to make app more secure 
 
- [x] To improve the user's experience, when asking for a date input, give the option to type a simple command to add today's date.

- [x] Let the users create their own habits to track. That will require that you let them choose the unit of measurement of each habit.

## Own challenges

- [x] Use DTOs to display information.

### How the App works
- SQLite connection
	- First you initialize the database with SQLite.
	- If no database exists, or the correct table does not exist they will be created on program start.

- Console based UI
	- Navegate the commands by typing the available options

- CRUD Db functions
	- Users can Create and Delete habits.
	- Users can Create, Read, Update or Delete habits logs.
	- Habits logs has to belong to a Habit.
	- If the user wants to create a log for a habit that doesn't exists he must create the habit first.

### Lessons Learned
- How to use Sqlite to store data.
- How ADO.NET works.
- How parameterized queries prevent SQL Injections.
- How to use foreign keys to chain two different tables.
- How to use DTOs to show the information.

### Project Info
The C# Academy [Habit Logger Project](https://www.thecsharpacademy.com/project/12/habit-logger).

### What's Next?
The C# Academy [Coding Tracker](https://www.thecsharpacademy.com/project/13/coding-tracker).

