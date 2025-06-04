using System.ComponentModel.DataAnnotations;

namespace DeviceManager.API.Models;

public class EmployeeRequestDto
{
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Salary { get; set; }

    [Required]
    public int PositionId { get; set; }

    [Required]
    public int PersonId { get; set; }
} 