# DeviceManager API

## App Settings Configuration

Create a file named `appsettings.json` in the API project root with the following structure:

```
{
  "ConnectionStrings": {
    "DefaultConnection": "<YOUR_CONNECTION_STRING_HERE>"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```
**Do NOT commit your real connection string to version control!**

## Project Structure Explanation

_Explain here why you chose to split (or not split) your code into several projects._

## Running the Application

1. Restore NuGet packages: `dotnet restore`
2. Update `appsettings.json` with your connection string.
3. Build and run the API: `dotnet run --project DeviceManager.API` 