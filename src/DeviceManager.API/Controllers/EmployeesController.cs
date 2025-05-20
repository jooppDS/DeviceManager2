using DeviceManager.Data;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.API.Controllers;

[Route("api/employees")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly IDataService _dataService;

    public EmployeesController(IDataService dataService)
    {
        _dataService = dataService;
    }

    // GET: api/employees
    [HttpGet]
    public async Task<IActionResult> GetAllEmployees()
    {
        try
        {
            var employees = await _dataService.GetAllEmployeesAsync();
            var result = employees.Select(e => new {
                id = e.Id,
                fullName = e.Person != null ? $"{e.Person.FirstName} {e.Person.LastName}" : null
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }

    // GET: api/employees/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployeeById(int id)
    {
        try
        {
            var employee = await _dataService.GetEmployeeByIdAsync(id);
            if (employee == null)
                return NotFound(new { error = "Employee not found." });
            var result = new {
                id = employee.Id,
                person = employee.Person,
                salary = employee.Salary,
                position = new { id = employee.Position?.Id, name = employee.Position?.Name },
                hireDate = employee.HireDate
            };
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }
} 