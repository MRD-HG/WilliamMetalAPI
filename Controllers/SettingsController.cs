using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WilliamMetalAPI.Data;
using WilliamMetalAPI.Models;

namespace WilliamMetalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly WilliamMetalContext _db;

        public SettingsController(WilliamMetalContext db)
        {
            _db = db;
        }

        [HttpGet("company")]
        public async Task<IActionResult> GetCompany()
        {
            var s = await _db.CompanySettings.AsNoTracking().OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync();
            if (s == null) return NotFound(new { success = false, message = "Company settings not found." });
            return Ok(new { success = true, data = s });
        }

        [HttpPut("company")]
        public async Task<IActionResult> UpdateCompany([FromBody] CompanySettings input)
        {
            var s = await _db.CompanySettings.OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync();
            if (s == null)
            {
                input.Id = Guid.NewGuid().ToString();
                input.UpdatedAt = DateTime.UtcNow;
                _db.CompanySettings.Add(input);
            }
            else
            {
                s.Name = input.Name;
                s.Address = input.Address;
                s.Phone = input.Phone;
                s.Email = input.Email;
                s.TaxRate = input.TaxRate;
                s.Currency = input.Currency;
                s.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
