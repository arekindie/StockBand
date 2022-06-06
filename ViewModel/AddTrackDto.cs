﻿using StockBand.Data;
using System.ComponentModel.DataAnnotations;

namespace StockBand.ViewModel
{
    public class AddTrackDto
    {
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }
        [MaxLength(250)]
        public string Description { get; set; }
        [Required]
        public string TrackAccess { get; set; }
        [Required]
        public IFormFile File { get; set; }
        public string AlbumName { get; set; }
        [Display(Name = "Select to choose the album")]
        public bool IsAlbumSelectedToChoose { get; set; }
    }
}
