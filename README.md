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


## Project Structure Explanation

The solution is split into three projects (API, Lib, Data) to enable reusability in potential future solutions. This structure also simplifies future deployments. Additionally, the project becomes more scalable. Modularity and dependency isolation are considered good programming practices.
