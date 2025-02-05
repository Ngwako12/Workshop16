using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Workshop16.Models;
using Workshop16.Services; // Import email service
using Workshop16.Data;

namespace Workshop16.Controllers
{
    [Authorize(Roles = "Admin")] // Restrict access to Admins only
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService; // Email Service

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // GET: Admin (View all service bookings)
        public async Task<IActionResult> Index()
        {
            return _context.CarServices != null
                ? View(await _context.CarServices.ToListAsync())
                : Problem("Entity set 'ApplicationDbContext.CarServices' is null.");
        }

        // GET: Admin/Details/5 (View service details)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.CarServices == null)
            {
                return NotFound();
            }

            var carService = await _context.CarServices.FindAsync(id);
            if (carService == null)
            {
                return NotFound();
            }

            return View(carService);
        }

        // GET: Admin/Edit/5 (Update service status)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.CarServices == null)
            {
                return NotFound();
            }

            var carService = await _context.CarServices.FindAsync(id);
            if (carService == null)
            {
                return NotFound();
            }

            return View(carService);
        }
        
        // POST: Admin/Edit/5 (Save updates)
        [HttpPost]
        [ValidateAntiForgeryToken]

       

        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,VehicleMake,Model,Year,Mileage,VIN,ServiceType,AppointmentDate,Status")] CarService carService)
        {
            if (id != carService.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(carService);
                    await _context.SaveChangesAsync();

                    // ✅ Send email when service is completed
                    if (string.Equals(carService.Status, "Done", StringComparison.OrdinalIgnoreCase))
                    {
                        var customer = await _userManager.FindByIdAsync(carService.CustomerId);
                        if (customer != null)
                        {
                            await _emailService.SendEmailAsync(
                                customer.Email,
                                "Service Completed",
                                $"Hello {customer.UserName},<br/>Your vehicle ({carService.VehicleMake} {carService.Model}) is now serviced and ready for pickup.<br/><br/>Thank you!"
                            );
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarServiceExists(carService.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(carService);
        }

        // GET: Admin/Delete/5 (Confirm deletion)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.CarServices == null)
            {
                return NotFound();
            }

            var carService = await _context.CarServices.FindAsync(id);
            if (carService == null)
            {
                return NotFound();
            }

            return View(carService);
        }

        // POST: Admin/Delete/5 (Delete service)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.CarServices == null)
            {
                return Problem("Entity set 'ApplicationDbContext.CarServices' is null.");
            }

            var carService = await _context.CarServices.FindAsync(id);
            if (carService != null)
            {
                _context.CarServices.Remove(carService);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }



        private bool CarServiceExists(int id)
        {
            return (_context.CarServices?.Any(e => e.Id == id)).GetValueOrDefault();
        }

    }
}
