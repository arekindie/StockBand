﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.Services;
using StockBand.ViewModel;

namespace Stock_Band.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserLogService _userLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUniqueLinkService _uniqueLinkService;
        public AccountController(IUniqueLinkService uniqueLinkService, IUserService userService, IUserLogService userLogService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _userLogService = userLogService;
            _httpContextAccessor = httpContextAccessor;
            _uniqueLinkService = uniqueLinkService;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(UserLoginDto user)
        {
            if (!ModelState.IsValid)
                return View(user);
            var status = await _userService.LoginUserAsync(user);
            if (status)
                return RedirectToAction("index", "home");
            return View(user);
        }
        [HttpGet]
        public async Task<IActionResult> LogoutAsync()
        {
            var status = await _userService.LogoutUserAsync();
            if(status)
                return RedirectToAction("index", "home");
            return RedirectToAction("index", "home");
        }
        [HttpGet]
        [Route("account/create/{guid:Guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> Create(Guid guid)
        {
            var verifyGuid = await _uniqueLinkService.VerifyLink(guid);
            if (!verifyGuid)
            {
                TempData["Message"] = Message.Code01;
                return RedirectToAction("customexception", "exceptions");
            }
            if (User.Identity.IsAuthenticated)
            {
                TempData["Message"] = Message.Code02;
                return RedirectToAction("customexception", "exceptions");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/create/{guid:Guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAsync(Guid guid,CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);
            var status = await _userService.CreateUser(guid, dto);
            if (status)
                return RedirectToAction("login", "account");
            return View(dto);
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);
            var status = await _userService.ChangePasswordUser(dto);
            if (status)
            {
                await _userService.LogoutUserAsync();
                return RedirectToAction("index", "home");
            }
            return View(dto);
        }
        [HttpGet]
        public async Task<IActionResult> UserLog(int pageNumber=1,string search="")
        {
            if (pageNumber <= 0)
                return RedirectToAction("userlog", "account", new { pageNumber = 1 });
            var userLogs = _userLogService
                .GetAllUserLogsAsync()
                .OrderByDescending(x => x.CreatedDate)
                .Where(x => x.Action.Contains(search)
                || x.Guid.ToString().Contains(search)
                || x.CreatedDate.ToString().Contains(search))
                .Where(x => x.CreatedDate > DateTime.UtcNow.AddDays(-7));
            
            if (!userLogs.Any())
            {
                return View();
            }  
            var paginatedList = await PaginetedList<UserLog>.CreateAsync(userLogs.AsNoTracking(), pageNumber, 30);
            if (pageNumber > paginatedList.TotalPages)
                return RedirectToAction("userlog", "account", new { pageNumber = paginatedList.TotalPages });
            return View(paginatedList);
        }
        [HttpGet]
        public IActionResult UserSettings()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ChangeColor()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangeColor(ChangeColorDto userDto)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("changecolor", "account", userDto);
            var status = await _userService.ChangeUserColor(userDto);
            if (status)
            {
                return RedirectToAction("index", "home");
            }
            return RedirectToAction("changecolor", "account", userDto);
        }
        [HttpGet]
        public IActionResult ChangeTheme()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangeTheme(ChangeThemeDto userDto)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("changetheme", "account", userDto);
            var status = await _userService.ChangeUserTheme(userDto);
            if (status)
            {
                return RedirectToAction("index", "home");
            }
            return RedirectToAction("changetheme", "account", userDto);
        }
    }  
}
