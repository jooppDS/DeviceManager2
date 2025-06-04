using System.ComponentModel.DataAnnotations;

namespace DeviceManager.API.DTOs;

public class AuthDto
{
    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string Password { get; set; }
}