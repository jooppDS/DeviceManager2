using DeviceManager.API.DTOs;
using DeviceManager.Data;
using DeviceManager.Lib.Models;
using DeviceManager.Lib.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IDataService _dataService;
        private readonly PasswordHasher<Account> _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthController(IDataService dataService, ITokenService tokenService)
        {
            _dataService = dataService;
            _passwordHasher = new PasswordHasher<Account>();
            _tokenService = tokenService;
        }

        [HttpPost("accounts")]
        public async Task<IActionResult> RegisterAccount([FromBody] AccountDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

               
                var existingAccount = await _dataService.GetAccountByUsernameAsync(dto.Username);
                if (existingAccount != null)
                    return BadRequest(new { error = "Username already exists." });

                
                var employee = await _dataService.GetEmployeeByIdAsync(dto.EmployeeId);
                if (employee == null)
                    return BadRequest(new { error = "Employee not found." });

                var account = new Account
                {
                    Username = dto.Username,
                    Password = dto.Password,
                    EmployeeId = dto.EmployeeId,
                    RoleId = 2 
                };

                account.Password = _passwordHasher.HashPassword(account, account.Password);

                var createdAccount = await _dataService.AddAccountAsync(account);
                return CreatedAtAction("GetAccount", new { id = createdAccount.Id }, new { 
                    id = createdAccount.Id,
                    username = createdAccount.Username,
                    employeeId = createdAccount.EmployeeId
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { error = "Database error occurred.", details = "Please try again later." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", details = "Please try again later." });
            }
        }

        [HttpPost("auth")]
        public async Task<IActionResult> Authentication([FromBody] AuthDto dto, CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var account = new Account
                {
                    Username = dto.Username,
                    Password = dto.Password,
                };

                var foundAccount = await _dataService.AuthAsync(account, cancellationToken);
                if (foundAccount == null)
                    return Unauthorized(new { error = "Invalid username or password." });

                var token = new
                {
                    accessToken = _tokenService.GenerateToken(foundAccount.Username, foundAccount.Role.Name),
                };

                return Ok(token);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", details = "Please try again later." });
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("accounts/me")]
        public async Task<IActionResult> GetOwnAccount()
        {
            try
            {
                var username = User.Identity?.Name;
                if (username == null)
                    return Unauthorized(new { error = "User not authenticated." });

                var account = await _dataService.GetAccountByUsernameAsync(username);
                if (account == null)
                    return NotFound(new { error = "Account not found." });

                return Ok(new { 
                    username = account.Username, 
                    employeeId = account.EmployeeId,
                    role = account.Role.Name
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", details = "Please try again later." });
            }
        }

        [Authorize(Roles = "User")]
        [HttpPut("accounts/me")]
        public async Task<IActionResult> UpdateOwnAccount([FromBody] AccountDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var username = User.Identity?.Name;
                if (username == null)
                    return Unauthorized(new { error = "User not authenticated." });

                var account = await _dataService.GetAccountByUsernameAsync(username);
                if (account == null)
                    return NotFound(new { error = "Account not found." });

            
                if (dto.Username != username)
                {
                    var existingAccount = await _dataService.GetAccountByUsernameAsync(dto.Username);
                    if (existingAccount != null)
                        return BadRequest(new { error = "Username already exists." });
                }

                var updatedAccount = new Account
                {
                    Id = account.Id,
                    Username = dto.Username,
                    Password = _passwordHasher.HashPassword(account, dto.Password),
                    EmployeeId = account.EmployeeId,
                    RoleId = account.RoleId 
                };

                var result = await _dataService.UpdateAccountAsync(updatedAccount);
                if (!result)
                    return BadRequest(new { error = "Failed to update account." });

                return Ok(new { message = "Account updated successfully." });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { error = "Database error occurred.", details = "Please try again later." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", details = "Please try again later." });
            }
        }
    }
}
