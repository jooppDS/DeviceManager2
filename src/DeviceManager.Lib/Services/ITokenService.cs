namespace DeviceManager.Lib.Services;

public interface ITokenService
{
    string  GenerateToken(string Username, string Role);
}