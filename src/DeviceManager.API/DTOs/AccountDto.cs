using System.ComponentModel.DataAnnotations;

namespace DeviceManager.API.Models;

public class AccountPostDto
{
    [Required]
    [RegularExpression(@"^[^\d][\w]*$", ErrorMessage = "Username must not start with a number.")]
    public string Username { get; set; }

    [Required]
    [MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).+$", ErrorMessage = "Password must have at least one lowercase, one uppercase, one number, and one symbol.")]
    public string Password { get; set; }

    
    public int EmployeeId { get; set; }
    
    public int RoleId { get; set; }
}