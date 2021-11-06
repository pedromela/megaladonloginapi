using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoginLib.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace LoginAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AuthenticationContext _context;
        private UserManager<ApplicationUser> _userManager;

        public PaymentsController(AuthenticationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpGet("package")]
        [Authorize]
        //GET : /api/Payments/package
        public async Task<ActionResult<SubscriptionPackage>> GetUserSubscriptionPackage()
        {
            string userId = User.Claims.First(c => c.Type == "UserID").Value;
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            SubscriptionPackage package = await _context.SubscriptionPackages.FindAsync(user.Package);
            return package;
        }

        // GET: api/Payments/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);


            if (payment == null)
            {
                return NotFound();
            }

            return payment;
        }

        // POST: api/Payments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Payment>> PostPayment(Payment payment)
        {
            string userId = User.Claims.First(c => c.Type == "UserID").Value;
            payment.UserId = userId;
            payment.ValidFrom = DateTime.Now;
            payment.ValidTo = DateTime.Now.AddYears(1);
            _context.Payments.Add(payment);

            var package = await _context.SubscriptionPackages.FindAsync(payment.Package);
            
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            user.Package = package.Name;

            _context.ApplicationUsers.Update(user);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPayment", new { id = payment.Id }, payment);
        }
    }
}
