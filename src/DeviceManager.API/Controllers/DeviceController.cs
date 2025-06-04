using DeviceManager.Data;
using DeviceManager.Lib.Models;
using DeviceManager.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace DeviceManager.API.Controllers;


[Route("api")]
[ApiController]
public class DataController : ControllerBase
{
    private readonly IDataService _dataService;
    
    private readonly PasswordHasher<Account> _passwordHasher;

    public DataController(IDataService dataService)
    {
        _dataService = dataService;
        _passwordHasher = new PasswordHasher<Account>();
    }

    // GET: api/devices
    [HttpGet("devices")]
    public async Task<IActionResult> GetAllDevices()
    {
        try
        {
            var devices = await _dataService.GetAllDevicesAsync();
            var simplifiedDevices = devices.Select(d => new { d.Id, d.Name });
            return Ok(simplifiedDevices);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }
    
    // GET: api/devices/5
    [HttpGet("devices/{id}")]
    public async Task<IActionResult> GetDevice(int id)
    {
        try
        {
            var device = await _dataService.GetDeviceByIdAsync(id);
            if (device == null)
                return NotFound(new { error = "Device not found." });
            var additionalProps = JsonSerializer.Deserialize<Dictionary<string, object>>(device.AdditionalProperties ?? "{}");
            var response = new
            {
                deviceTypeName = device.DeviceType?.Name,
                isEnabled = device.IsEnabled,
                additionalProperties = additionalProps
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }

    // POST: api/devices
    [HttpPost("devices")]
    public async Task<IActionResult> CreateDevice([FromBody] DeviceRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var device = new Device
            {
                Name = dto.Name,
                DeviceType = new DeviceType { Name = dto.DeviceTypeName },
                IsEnabled = dto.IsEnabled,
                AdditionalProperties = dto.AdditionalProperties ?? "{}"
            };
            var createdDevice = await _dataService.AddDeviceAsync(device);
            return CreatedAtAction(nameof(GetDevice), new { id = createdDevice.Id }, createdDevice);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }
    
    [HttpPost("accounts")]
    public async Task<IActionResult> RegisterAccount([FromBody] AccountPostDto dto) {
        try
        {
            var account = new Account
            {
                Username = dto.Username,
                Password = dto.Password,
                EmployeeId = dto.EmployeeId,
                RoleId = 2
            };
            
            account.Password = _passwordHasher.HashPassword(account, account.Password);
            
            var createdAccount = await _dataService.AddAccountAsync(account);
            return CreatedAtAction("GetAccount",new {id = createdAccount.Id}, createdAccount);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
        
    }


    [HttpPost("auth")]
    public async Task<IActionResult> Authentication([FromBody] AuthDto dto)
    {

        try
        {
            
            var account = new Account()
            {
                Username = dto.Username,
                Password = dto.Password,
            };
            if(await _dataService.AuthAsync(account))
                return Ok(new { message = "Authentication successful." });
            
            return Unauthorized(new { message = "Invalid username or password." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
        
    }

    // PUT: api/devices/5
    [HttpPut("devices/{id}")]
    public async Task<IActionResult> UpdateDevice(int id, [FromBody] DeviceRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var device = new Device
            {
                Id = id,
                Name = dto.Name,
                DeviceType = new DeviceType { Name = dto.DeviceTypeName },
                IsEnabled = dto.IsEnabled,
                AdditionalProperties = dto.AdditionalProperties ?? "{}"
            };
            var success = await _dataService.UpdateDeviceAsync(device);
            if (!success)
                return NotFound(new { error = "Device not found." });
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }

    // DELETE: api/devices/5
    [HttpDelete("devices/{id}")]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        try
        {
            var success = await _dataService.DeleteDeviceAsync(id);
            if (!success)
                return NotFound(new { error = "Device not found." });
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }
}