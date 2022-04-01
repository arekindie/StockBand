﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Services;
using StockBand.ViewModel;
using System.Security.Claims;

namespace Stock_Band.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        public AccountController(IUserService userService)
        {
            _userService = userService;
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
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public IActionResult Create(Guid guid)
        {
            var verifyGuid = UniqueLinkService.VerifyLink(guid);
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
        public async Task<IActionResult> ChangePassword(ProfileEditUser dto)
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
    }  
}
