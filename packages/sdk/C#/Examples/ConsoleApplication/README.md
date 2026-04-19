# Example Console Application

This console application demonstrates how to use the generated Agent Protocol API client SDK to interact with the Agent Tasks API. The application provides a user-friendly command-line interface for performing various operations with agent tasks, steps, and artifacts.

## Features

- Create new agent tasks
- List all existing tasks with pagination
- View detailed information about a specific task
- List steps for a task with pagination
- Execute task steps
- View detailed information about a specific step
- List artifacts for a task with pagination
- Upload new artifacts to a task
- Download existing artifacts

## Project Structure

```
ConsoleApplication/
├── Program.cs                     # Main application entry point and UI
├── Models/
│   └── AgentTaskModels.cs         # Domain models for tasks, steps, and artifacts
├── Services/
│   └── AgentTaskService.cs        # Service layer for API interactions
├── appsettings.json               # Application configuration
└── ConsoleApplication.csproj     # Project file with dependencies
```

## Prerequisites

- .NET 8.0 SDK
- Access to an Agent Protocol compatible API
- The generated API client SDK (referenced in the project)

## Setup

1. Clone this repository
2. Update the `appsettings.json` file with your API credentials:

   ```json
   {
     "ApiSettings": {
       "BaseUrl": "https://api.example.com",
       "ApiKey": "your-api-key-here",
       "DefaultTimeout": 30
     }
   }
   ```

3. Build the project:

   ```
   dotnet build
   ```

## Running the Application

```bash
dotnet run
```
