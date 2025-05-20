using System.ComponentModel.DataAnnotations;

namespace DeviceManager.API.Models;

public class DeviceRequestDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Required]
    public string DeviceTypeName { get; set; } = null!;

    [Required]
    public bool IsEnabled { get; set; }

    public string? AdditionalProperties { get; set; }
} 