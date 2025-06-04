# DeviceManager API

## App Settings Configuration

Create a file named `appsettings.json` in the API project root with the following structure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<YOUR_CONNECTION_STRING_HERE>"
  },
  "Jwt": {
    "Key": "<YOUR_SECRET_KEY_HERE>",
    "Issuer": "<YOUR_ISSUER>",
    "Audience": "<YOUR_AUDIENCE>",
    "ValidityInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```



## Project Structure Explanation

The solution is split into three projects (API, Lib, Data) to enable reusability in potential future solutions. This structure also simplifies future deployments. Additionally, the project becomes more scalable. Modularity and dependency isolation are considered good programming practices.

