# UqamAppNotificationService

This app is a worker service whose role is to send notifications when notes app about a user change

## Project Structure
The main entry of the project is the Worker class,
that class contain the main loop that fetch the data from the uqam api and
compare it to the local data on the computer

## Getting started?

Follow theses steps to configure the project

1. Download the dotnet sdk https://dotnet.microsoft.com/en-us/download

1. Copy the content of `appsettings.Development.json` to `appsettings.json` and change the `PermanentCode` and `Password` to the corresponding values. 
`ATTENTION: Be sure that the appsettings.json be on the .gitignore, this file must not be on the repos`

1. Execute this command to run the project 
```bash
    dotnet run
```
