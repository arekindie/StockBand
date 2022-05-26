﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly ITrackService _trackService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public LibraryController(ITrackService trackService, IConfiguration configuration, IMapper mapper)
        {
            _trackService = trackService;
            _configuration = configuration;
            _mapper = mapper;
        }
        [HttpGet]
        [Route("library/edittrack/{guid:Guid}")]
        public async Task<IActionResult> EditTrack(Guid guid)
        {
            var track = await _trackService.GetTrack(guid);
            if (track is null)
                return RedirectToAction("badrequestpage", "exceptions");

            var viewModel = _mapper.Map<EditTrackDto>(track);
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("library/edittrack/{guid:Guid}")]
        public async Task<IActionResult> EditTrack(Guid guid, EditTrackDto track)
        {
            if (!ModelState.IsValid)
                return View(track);
            var status = await _trackService.EditTrack(guid, track);
            if(status)
                return RedirectToAction("track", "library", new { guid = guid });
            return View("track", track);
        }

        [HttpGet]
        public IActionResult AddTrack()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddTrack(AddTrackDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);
            var status = await _trackService.AddTrack(dto);
            if (status)
                return RedirectToAction("track", "library",new {guid = await _trackService.GetGuidTrackByTitle(dto.Title)});
            return View(dto);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("library/track/{guid:Guid}")]
        public async Task <IActionResult> Track(Guid guid)
        {
            var track = await _trackService.GetTrack(guid);
            if(track is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if (!_trackService.VerifyAccessTrack(track))
            {
                return RedirectToAction("forbidden", "exceptions");
            }
            return View(track);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("library/stream/{guid:Guid}")]
        public async Task<IActionResult> Stream(Guid guid)
        {
            var track = await _trackService.GetTrack(guid);
            if (track is null)
            {
                return RedirectToAction("badrequestpage", "exceptions");
            }
            if (!_trackService.VerifyAccessTrack(track))
            {
                return RedirectToAction("forbidden", "exceptions");
            }
            Response.Headers.Remove("Cache-Control");
            Response.Headers.Add("Accept-Ranges", "bytes");
            var fileStream = new FileStream($"{_configuration["TrackFilePath"]}{track.Guid}.{track.Extension}", FileMode.Open, FileAccess.Read, FileShare.Read, 1024);
            return File(fileStream, "audio/mp3");
        }
    }
}
