# Ocelot Editor

A Windows desktop tool built with WPF and MVVM that helps you load, inspect, edit, validate, and save `ocelot.json` files for the Ocelot API Gateway.

## Features

- Strongly-typed editor for routes, downstream hosts, authentication scopes, and global configuration
- JSON parsing/serialization powered by Newtonsoft.Json
- Validation for required route fields before saving
- Add, duplicate, delete, and reorder routes
- Built-in status bar showing the active file path and save status

## Getting started

1. Install the .NET 8 SDK with Windows Desktop support.
2. Restore dependencies and build:
   ```bash
   dotnet restore
   dotnet build
   ```
3. Run the application:
   ```bash
   dotnet run
   ```

Use the **Open...** command to load an existing `ocelot.json`, make your changes, then choose **Save** or **Save As...** to persist them.
