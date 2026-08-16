using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Portal_1._0.Data;
using Student_Portal_1._0.Models;
using Student_Portal_1._0.Models.ViewModels;

namespace Student_Portal_1._0.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _context;

        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment webHostEnvironment,
            AppDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var targetUser = await _userManager.FindByNameAsync(model.StudentId)
                       ?? await _userManager.FindByEmailAsync(model.StudentId)
                       ?? await _userManager.Users
                           .Include(u => u.StudentInfo)
                           .FirstOrDefaultAsync(u => u.StudentInfo != null && u.StudentInfo.StudentId.ToString() == model.StudentId);

            if (targetUser != null)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(
                    targetUser.UserName!,
                    model.Password,
                    isPersistent: false,
                    lockoutOnFailure: false
                );

                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (int.TryParse(model.StudentId, out int parsedStudentIdValue))
            {
                bool studentInfoExists = await _context.studentInfos.AnyAsync(s => s.StudentId == parsedStudentIdValue);
                if (studentInfoExists)
                {
                    ModelState.AddModelError("StudentId", "This Student ID is already registered.");
                    return View(model);
                }
            }

            var matchedEmailUser = await _userManager.FindByEmailAsync(model.Email);
            if (matchedEmailUser != null)
            {
                ModelState.AddModelError("Email", "This Email is already in use.");
                return View(model);
            }

            string? uploadedFileName = null;
            string? savedFilePath = null;

            if (model.ImageFile != null)
            {
                string targetFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "students");

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                uploadedFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ImageFile.FileName)}";
                savedFilePath = Path.Combine(targetFolder, uploadedFileName);

                using (var fileStream = new FileStream(savedFilePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }
            }

            string profileImagePath = uploadedFileName != null
                ? $"/images/students/{uploadedFileName}"
                : "/images/default-user.png";

            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var newUser = new ApplicationUser
                {
                    UserName = model.StudentId,
                    Email = model.Email,
                    PhoneNumber = model.Phone
                };

                var createResult = await _userManager.CreateAsync(newUser, model.Password);

                if (!createResult.Succeeded)
                {
                    if (savedFilePath != null && System.IO.File.Exists(savedFilePath))
                    {
                        System.IO.File.Delete(savedFilePath);
                    }

                    foreach (var identityError in createResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, identityError.Description);
                    }

                    return View(model);
                }

                await _userManager.AddToRoleAsync(newUser, "Student");

                if (int.TryParse(model.StudentId, out int finalParsedId))
                {
                    var newStudentInfo = new StudentInfo
                    {
                        StudentId = finalParsedId,
                        Name = model.Name,
                        Email = model.Email,
                        Department = model.Department,
                        Semester = model.Semester,
                        Phone = model.Phone,
                        Gender = model.Gender,
                        Address = model.Address,
                        Image = profileImagePath,
                        ApplicationUserId = newUser.Id
                    };

                    _context.studentInfos.Add(newStudentInfo);
                    await _context.SaveChangesAsync();
                }

                await dbTransaction.CommitAsync();

                TempData["SuccessMessage"] = "Registration successful! Please log in with your credentials.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();

                if (savedFilePath != null && System.IO.File.Exists(savedFilePath))
                {
                    System.IO.File.Delete(savedFilePath);
                }

                ModelState.AddModelError(string.Empty, "An error occurred while creating your account. Please try again.");
                return View(model);
            }
        }
    }
}