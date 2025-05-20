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
  
   
}
