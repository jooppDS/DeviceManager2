using DeviceManager.Data;
using DeviceManager.Lib.Models;
using DeviceManager.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Collections.Generic;
using DeviceManager.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DeviceManager.API.Controllers;


[Route("api")]
[ApiController]
public class DeviceController : ControllerBase
{
    private readonly IDataService _dataService;
    
    public DeviceController(IDataService dataService)
    {
        _dataService = dataService;
    }

    // GET: api/devices
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    [HttpGet("devices/{id:int}")]
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
    [Authorize(Roles = "Admin")]
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
    

    // PUT: api/devices/5
    [Authorize(Roles = "Admin")]
    [HttpPut("devices/{id:int}")]
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
    [Authorize(Roles = "Admin")]
    [HttpDelete("devices/{id:int}")]
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

    [Authorize(Roles = "Admin,User")]
    [HttpGet("devices/me")]
    public async Task<IActionResult> SeeMyDevice() {
        var username = User.Identity?.Name;
        if (username == null)
            return Unauthorized();

        var account = await _dataService.GetAccountByUsernameAsync(username);

        if (account == null)
            return NotFound();
        
        var devices = await _dataService.GetAllDevicesByAccount(account);
        
        
        return Ok(devices);
    }
    
    [Authorize(Roles = "User,Admin")]
    [HttpPut("devices/me/{id:int}")]
    public async Task<IActionResult> UpdatePersonalDevice(int id, [FromBody] DeviceRequestDto dto)
    {
        var username = User.Identity?.Name;
        if (username == null)
            return Unauthorized();

        
        var account = await _dataService.GetAccountByUsernameAsync(username);

        if (account == null)
            return NotFound();

        var device = await _dataService.GetAllDevicesByAccountAndId(id,account);

        if (device == null)
            return Forbid();

        var updatedDevice = new Device
        {
            Id = id,
            Name = dto.Name,
            DeviceType = new DeviceType { Name = dto.DeviceTypeName },
            IsEnabled = dto.IsEnabled,
            AdditionalProperties = dto.AdditionalProperties ?? "{}"
        };
        var success = await _dataService.UpdateDeviceAsync(updatedDevice);
        if (!success)
            return NotFound(new { error = "Device not found." });

        return Ok(new { message = "Device updated." });
    }
}