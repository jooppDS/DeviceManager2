using System.ComponentModel.DataAnnotations;

namespace DeviceManager.API.Models;

public class AuthDto
{
    [Required]
    public String Username { get; set; }
    
    [Required]
    public String Password { get; set; }
}