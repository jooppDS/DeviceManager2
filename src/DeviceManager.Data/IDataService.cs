using DeviceManager.Lib.Models;

namespace DeviceManager.Data;

public interface IDataService
{
    Task<List<Device>> GetAllDevicesAsync();
    Task<Device?> GetDeviceByIdAsync(int id);
    Task<Device> AddDeviceAsync(Device device);
    Task<bool> UpdateDeviceAsync(Device device);
    Task<bool> DeleteDeviceAsync(int id);
    
    Task<List<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(int id);
    
    Task<List<Account>> GetAllAccountsAsync();
    
    Task<Account?> GetAccountByIdAsync(int id);
    Task<bool> UpdateAccountAsync(Account account);

    Task<bool> DeleteAccountAsync(int id);
    Task<Account> AddAccountAsync(Account account);
    Task<Account?> GetAccountByUsernameAsync(string username);
    Task<Account?> AuthAsync(Account account, CancellationToken cancellationToken);

    Task<List<Device>> GetAllDevicesByAccount(Account account);

    Task<List<Device>> GetAllDevicesByAccountAndId(int id, Account account);

}
