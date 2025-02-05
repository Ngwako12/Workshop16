using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

using Workshop16.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Workshop16.Data;
using Workshop16.Services;

namespace Workshop16.Controllers
{
    [Authorize]
    public class CarServiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService; // For sending emails


        public CarServiceController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        private List<string> GetServiceTypes()
        {
            return new List<string> { "Service", "Tires", "Suspension", "Engine Repairs", "Electrical", "Sound Installation", "Towing", "Brakes" };
        }

        // 🚀 Require login to view bookings
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return View(await _context.CarServices.ToListAsync());
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                return View(await _context.CarServices.Where(cs => cs.CustomerId == userId).ToListAsync());
            }
        }

        // 🚀 View Service Details
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var carService = await _context.CarServices.FirstOrDefaultAsync(m => m.Id == id);
            if (carService == null) return NotFound();

            return View(carService);
        }

        //  Customers Can Book a Service (With Dropdown)
        [Authorize]
        public IActionResult Create()
        {
            var carService = new CarService
            {
                CustomerId = _userManager.GetUserId(User)
            };

            ViewBag.ServiceTypes = new SelectList(GetServiceTypes());
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("VehicleMake,Model,Year,Mileage,VIN,ServiceType,AppointmentDate")] CarService carService)
        {

            string userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User must be logged in to book a service.");
                return View(carService);
            }

            carService.CustomerId = userId;

            carService.Status = "Booked";
            //?
            ModelState.Remove("CustomerId");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(carService);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" Error saving CarService: " + ex.Message);
                    ModelState.AddModelError("", "An error occurred while saving the booking.");
                }

            }


            ViewBag.ServiceTypes = new SelectList(GetServiceTypes()); // Reload dropdown if validation fails
            return View(carService);

            

           

          
        }

        //  Only Admins Can Edit a Service
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var carService = await _context.CarServices.FindAsync(id);
            if (carService == null) return NotFound();

            ViewBag.ServiceTypes = new SelectList(GetServiceTypes(), carService.ServiceType);
            return View(carService);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,VehicleMake,Model,Year,Mileage,VIN,ServiceType,AppointmentDate,Status")] CarService carService)
        {
            if (id != carService.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(carService);
                    await _context.SaveChangesAsync();

                    if (carService.Status == "Done")
                    {
                        var customer = await _userManager.FindByIdAsync(carService.CustomerId);
                       /*/ var customer = await _userManager.Users
                           .Where(u => u.Id == carService.CustomerId)
                           .Select(u => new { u.Email, u.UserName })
                           .FirstOrDefaultAsync();
                       */

                        if (customer != null && !string.IsNullOrEmpty(customer.Email))
                        {
                            await _emailService.SendEmailAsync(customer.Email, "Your Car Service is Complete",
                                $"Dear {customer.UserName}, your car service is completed. You can collect your vehicle.");
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarServiceExists(carService.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ServiceTypes = new SelectList(GetServiceTypes(), carService.ServiceType);
            return View(carService);
        }

        // 🚀 Only Admins Can Delete a Service
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var carService = await _context.CarServices.FirstOrDefaultAsync(m => m.Id == id);
            if (carService == null) return NotFound();

            return View(carService);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
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
            return _context.CarServices.Any(e => e.Id == id);
        }
    }
}
