using System.ComponentModel.DataAnnotations;

namespace DeviceManager.API.DTOs;

public class AccountDto
{
    [Required]
    [RegularExpression(@"^[^\d][\w]*$", ErrorMessage = "Username must not start with a number.")]
    public required string Username { get; set; }

    [Required]
    [MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).+$", ErrorMessage = "Password must have at least one lowercase, one uppercase, one number, and one symbol.")]
    public required string Password { get; set; }

    
    public int EmployeeId { get; set; }
    
    [Required]
    [Range(1, 2, ErrorMessage = "RoleId must be 1 (Admin) or 2 (User).")]
    public int RoleId { get; set; }
}