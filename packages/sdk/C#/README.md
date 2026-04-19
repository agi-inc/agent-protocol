# OpenAPI Client Generator

A .NET console application that generates C# client libraries from OpenAPI JSON specifications using NSwag. Built with modern .NET practices including dependency injection, logging, and service-oriented architecture.

## Features

- Generates C# client code from OpenAPI specifications
- Well-structured application using dependency injection
- Comprehensive logging
- Service-oriented architecture for better maintainability
- Command-line interface with multiple options
- Centralized package version management using Directory.Packages.props
- Shared build properties using Directory.Build.props

## Requirements

- .NET 8.0 SDK

## Installation

1. Clone or download this repository
2. Navigate to the project directory
3. Build the project:

   ```
   dotnet build
   ```

## Testing

The solution includes a comprehensive test suite covering unit, integration, and command-line tests:

1. Run all tests:

   ```
   dotnet test
   ```

The test project demonstrates different

## Usage

Run the application using `dotnet run` with the required parameters:

```
dotnet run -- -i <path-to-openapi-json> [-o <output-directory>] [-n <namespace>]
```

Or build and run the executable:

```
dotnet build
dotnet run -- -i <path-to-openapi-json> -o <output-directory>
```

### Parameters

- `-i, --input`: (Required) Path to the OpenAPI JSON file
- `-o, --output`: (Optional) Output directory for the generated C# client code
- `-n, --namespace`: (Optional) Namespace for the generated client (default: "GeneratedApiClient")

### Examples

Generate a client with default settings:

```
dotnet run -- -i ./my-api-spec.json
```

Specify an output directory and namespace:

```
dotnet run -- -i ./my-api-spec.json -o ./MyClientLibrary -n MyCompany.ApiClient
```

## Installing as a Global Tool

You can also install this application as a global .NET tool:

```
dotnet pack
dotnet tool install --global --add-source ./bin/Debug OpenApiClientGenerator
```

After installation, you can use it directly:

```
openapi-generator -i ./my-api-spec.json -o ./ClientLibrary
```
