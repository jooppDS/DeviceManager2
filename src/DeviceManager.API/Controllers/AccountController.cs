using DeviceManager.API.DTOs;
using DeviceManager.API.Models;
using DeviceManager.Data;
using DeviceManager.Lib.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        
        private readonly IDataService _dataService;
        
        private readonly PasswordHasher<Account> _passwordHasher;

        public AccountController(IDataService dataService)
        {
            _passwordHasher = new PasswordHasher<Account>();
            _dataService = dataService;
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccounts()
        {
            try {
                var accounts = await _dataService.GetAllAccountsAsync();
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
            }
            
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("accounts/{id}")]
        public async Task<IActionResult> GetDevice(int id) {
            try {
                var account = await _dataService.GetAccountByIdAsync(id);
                return Ok(new {account.Username, account.Password});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost("accounts/admin")]
        public async Task<IActionResult> CreateAccount([FromBody] AccountDto account) {


            try
            {
                var newAccount = new Account
                {
                    Username = account.Username,
                    Password = account.Password,
                    EmployeeId = account.EmployeeId,
                    RoleId = account.RoleId
                };
                
                account.Password = _passwordHasher.HashPassword(newAccount, newAccount.Password);

                var createdAccount = await _dataService.AddAccountAsync(newAccount);
                return CreatedAtAction("GetAccount", new { id = createdAccount.Id }, createdAccount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
            }
            
            
        }   
        
        [Authorize(Roles = "Admin")]
        [HttpPut("accounts")]
        public async Task<IActionResult> UpdateAccount([FromBody] AccountDto account) {


            try
            {
                var changedAccount = new Account
                {
                    Username = account.Username,
                    Password = account.Password,
                    EmployeeId = account.EmployeeId,
                    RoleId = account.RoleId
                };
                
                account.Password = _passwordHasher.HashPassword(changedAccount, changedAccount.Password);

                var result = await _dataService.UpdateAccountAsync(changedAccount);
                if(!result)
                    throw new Exception("An unexpected error occurred.");
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
            }
            
            
        } 
        
        [Authorize(Roles = "Admin")]
        [HttpDelete("accounts/{id}")]
        public async Task<IActionResult> DeleteAccount(int id) {


            try
            {
               var result = await _dataService.DeleteAccountAsync(id);
               if(!result)
                   throw new Exception("An unexpected error occurred.");
               
               return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
            }
            
            
        } 


    }
}
