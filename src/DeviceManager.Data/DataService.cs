using System.Text.Json;
using DeviceManager.Data;
using DeviceManager.Lib.Data;
using DeviceManager.Lib.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DbContext = DeviceManager.Lib.Data.DbContext;



public class DataService : IDataService
{
    private readonly DbContext _context;

    public DataService(String connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        _context = new DbContext(optionsBuilder.Options);
    }

    public async Task<List<Device>> GetAllDevicesAsync()
    {
        return await _context.Devices
            .Include(d => d.DeviceType)
            .ToListAsync();
    }

    public async Task<Device?> GetDeviceByIdAsync(int id)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null) return null;

        var latestDeviceEmployee = await _context.DeviceEmployees
            .Where(de => de.DeviceId == id)
            .OrderByDescending(de => de.IssueDate)
            .FirstOrDefaultAsync();

        if (latestDeviceEmployee != null)
        {
            var employee = await _context.Employees
                .Include(e => e.Person)
                .FirstOrDefaultAsync(e => e.Id == latestDeviceEmployee.EmployeeId);

            if (employee != null)
            {
                var additionalProps = string.IsNullOrEmpty(device.AdditionalProperties)
                    ? new Dictionary<string, object>()
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(device.AdditionalProperties);

                additionalProps["currentEmployee"] = new
                {
                    id = employee.Id,
                    firstName = employee.Person.FirstName,
                    lastName = employee.Person.LastName
                };

                device.AdditionalProperties = JsonSerializer.Serialize(additionalProps);
            }
        }

        return device;
    }

    public async Task<Device> AddDeviceAsync(Device device)
    {
        if (device.DeviceType != null)
        {
            device.DeviceTypeId = await _context.DeviceTypes
                .Where(dt => dt.Name == device.DeviceType.Name)
                .Select(dt => (int?)dt.Id)
                .FirstOrDefaultAsync();
        }

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
        return device;
    }

    public async Task<List<Account>> GetAllAccountsAsync()
    {
        return await _context.Accounts.ToListAsync();
    }

    public async Task<Account?> GetAccountByIdAsync(int id)
    {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Account> AddAccountAsync(Account account)
    {
        // Validate username uniqueness
        var existingAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Username == account.Username);
        if (existingAccount != null)
            throw new InvalidOperationException("Username already exists.");

        // Validate employee exists
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == account.EmployeeId);
        if (employee == null)
            throw new InvalidOperationException("Employee not found.");

        // Validate role exists
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == account.RoleId);
        if (role == null)
            throw new InvalidOperationException("Role not found.");

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<Account?> AuthAsync(Account account, CancellationToken cancellationToken)
    {
        var foundAccount = await _context.Accounts.Include(u => u.Role).FirstOrDefaultAsync(u => String.Equals(u.Username, account.Username), cancellationToken);
        
        if (foundAccount == null) return foundAccount;

        var hasher = new PasswordHasher<Account>();
        
        var hashedPassword = hasher.VerifyHashedPassword(foundAccount, foundAccount.Password, account.Password);
        
        if(hashedPassword == PasswordVerificationResult.Success)
            return foundAccount;
        
        return null;
    }

    public async Task<bool> UpdateDeviceAsync(Device device)
    {
        var existingDevice = await _context.Devices.FindAsync(device.Id);
        if (existingDevice == null) return false;

        existingDevice.Name = device.Name;
        existingDevice.IsEnabled = device.IsEnabled;
        existingDevice.AdditionalProperties = device.AdditionalProperties;

        if (device.DeviceType != null)
        {
            existingDevice.DeviceTypeId = await _context.DeviceTypes
                .Where(dt => dt.Name == device.DeviceType.Name)
                .Select(dt => (int?)dt.Id)
                .FirstOrDefaultAsync();
        }

        await _context.SaveChangesAsync();
        return true;
    }
    
    
    public async Task<bool> UpdateAccountAsync(Account account)
    {
        var existingAccount = await _context.Accounts.FindAsync(account.Id);
        if (existingAccount == null)
            return false;

        // Validate username uniqueness if changed
        if (existingAccount.Username != account.Username)
        {
            var usernameExists = await _context.Accounts
                .AnyAsync(a => a.Username == account.Username);
            if (usernameExists)
                throw new InvalidOperationException("Username already exists.");
        }

        // Validate role exists
        var roleExists = await _context.Roles
            .AnyAsync(r => r.Id == account.RoleId);
        if (!roleExists)
            throw new InvalidOperationException("Role not found.");

        existingAccount.Username = account.Username;
        existingAccount.Password = account.Password; // Password should be pre-hashed
        existingAccount.EmployeeId = account.EmployeeId;
        existingAccount.RoleId = account.RoleId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteDeviceAsync(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null) return false;

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        return await _context.Employees
            .Include(e => e.Person)
            .ToListAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Person)
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
    
    
    public async Task<bool> DeleteAccountAsync(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null) return false;

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Account?> GetAccountByUsernameAsync(string username)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.Username == username);
    }


    public async Task<List<Device>> GetAllDevicesByAccount(Account account)
    {
        return await _context.Devices
            .Where(d => d.DeviceEmployees.Any(de => de.EmployeeId == account.EmployeeId))
            .ToListAsync();
    }
    
    public async Task<List<Device>> GetAllDevicesByAccountAndId(int id,Account account)
    {
        return await _context.Devices
            .Where(d => d.DeviceEmployees.Any(de => de.EmployeeId == account.EmployeeId) && d.Id == id)
            .ToListAsync();
    }
}
