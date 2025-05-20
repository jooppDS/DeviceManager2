using DeviceManager.Lib.Models;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace DeviceManager.Data;

public class DataService : IDataService
{
    private readonly string _connectionString;

    public DataService(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Device operations
    public async Task<List<Device>> GetAllDevicesAsync()
    {
        var devices = new List<Device>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = new SqlCommand(@"
            SELECT Id, Name
            FROM Device", connection);
        
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            devices.Add(new Device
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }
        return devices;
    }

    public async Task<Device?> GetDeviceByIdAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = new SqlCommand(@"
            SELECT 
                d.Id,
                d.Name,
                dt.Name as DeviceTypeName,
                d.IsEnabled,
                d.AdditionalProperties,
                e.Id as EmployeeId,
                p.FirstName,
                p.LastName
            FROM Device d
            LEFT JOIN DeviceType dt ON d.DeviceTypeId = dt.Id
            LEFT JOIN (
                SELECT de.DeviceId, de.EmployeeId
                FROM DeviceEmployee de
                WHERE de.DeviceId = @Id
                AND de.IssueDate = (
                    SELECT MAX(IssueDate)
                    FROM DeviceEmployee
                    WHERE DeviceId = de.DeviceId
                )
            ) latest_de ON d.Id = latest_de.DeviceId
            LEFT JOIN Employee e ON latest_de.EmployeeId = e.Id
            LEFT JOIN Person p ON e.PersonId = p.Id
            WHERE d.Id = @Id", connection);
        
        command.Parameters.AddWithValue("@Id", id);
        
        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var additionalProperties = reader.IsDBNull(4) ? "{}" : reader.GetString(4);
            var employeeId = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5);
            var firstName = reader.IsDBNull(6) ? null : reader.GetString(6);
            var lastName = reader.IsDBNull(7) ? null : reader.GetString(7);

            var device = new Device
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                DeviceType = new DeviceType { Name = reader.GetString(2) },
                IsEnabled = reader.GetBoolean(3),
                AdditionalProperties = additionalProperties
            };

            // Store current employee info in AdditionalProperties
            var additionalProps = JsonSerializer.Deserialize<Dictionary<string, object>>(additionalProperties);
            if (employeeId.HasValue)
            {
                additionalProps["currentEmployee"] = new
                {
                    id = employeeId.Value,
                    firstName,
                    lastName
                };
            }
            device.AdditionalProperties = JsonSerializer.Serialize(additionalProps);

            return device;
        }
        return null;
    }

    public async Task<Device> AddDeviceAsync(Device device)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = new SqlCommand(@"
            INSERT INTO Device (Name, IsEnabled, AdditionalProperties, DeviceTypeId)
            OUTPUT INSERTED.Id
            VALUES (@Name, @IsEnabled, @AdditionalProperties, (SELECT Id FROM DeviceType WHERE Name = @DeviceTypeName))", connection);
        command.Parameters.AddWithValue("@Name", device.Name);
        command.Parameters.AddWithValue("@IsEnabled", device.IsEnabled);
        command.Parameters.AddWithValue("@AdditionalProperties", device.AdditionalProperties ?? "{}");
        command.Parameters.AddWithValue("@DeviceTypeName", device.DeviceType?.Name ?? (object)DBNull.Value);
        var newId = (int)await command.ExecuteScalarAsync();
        device.Id = newId;
        return device;
    }

    public async Task<bool> UpdateDeviceAsync(Device device)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = new SqlCommand(@"
            UPDATE Device
            SET Name = @Name, IsEnabled = @IsEnabled, AdditionalProperties = @AdditionalProperties, DeviceTypeId = (SELECT Id FROM DeviceType WHERE Name = @DeviceTypeName)
            WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", device.Id);
        command.Parameters.AddWithValue("@Name", device.Name);
        command.Parameters.AddWithValue("@IsEnabled", device.IsEnabled);
        command.Parameters.AddWithValue("@AdditionalProperties", device.AdditionalProperties ?? "{}");
        command.Parameters.AddWithValue("@DeviceTypeName", device.DeviceType?.Name ?? (object)DBNull.Value);
        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> DeleteDeviceAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = new SqlCommand("DELETE FROM Device WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", id);
        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        var employees = new List<Employee>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = new SqlCommand(@"
            SELECT e.Id, p.FirstName, p.LastName
            FROM Employee e
            JOIN Person p ON e.PersonId = p.Id", connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            employees.Add(new Employee
            {
                Id = reader.GetInt32(0),
                Person = new Person
                {
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2)
                }
            });
        }
        return employees;
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = new SqlCommand(@"
            SELECT e.Id, e.Salary, e.HireDate, p.Id as PersonId, p.FirstName, p.LastName, p.MiddleName, p.PassportNumber, p.PhoneNumber, p.Email, pos.Id as PositionId, pos.Name as PositionName
            FROM Employee e
            JOIN Person p ON e.PersonId = p.Id
            JOIN Position pos ON e.PositionId = pos.Id
            WHERE e.Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", id);
        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Employee
            {
                Id = reader.GetInt32(0),
                Salary = reader.GetDecimal(1),
                HireDate = reader.GetDateTime(2),
                Person = new Person
                {
                    Id = reader.GetInt32(3),
                    FirstName = reader.GetString(4),
                    LastName = reader.GetString(5),
                    MiddleName = reader.IsDBNull(6) ? null : reader.GetString(6),
                    PassportNumber = reader.GetString(7),
                    PhoneNumber = reader.GetString(8),
                    Email = reader.GetString(9)
                },
                Position = new Position
                {
                    Id = reader.GetInt32(10),
                    Name = reader.GetString(11)
                }
            };
        }
        return null;
    }
    
}
 