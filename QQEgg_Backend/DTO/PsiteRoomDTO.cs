﻿using QQEgg_Backend.Models;

namespace QQEgg_Backend.DTO
{
    public class PsiteRoomDTO
    {
        public int RoomId { get; set; }        
        public int? CategoryId { get; set; }
        public decimal? HourPrice { get; set; }
        public decimal? DatePrice { get; set; }
        public int? Ping { get; set; }
        public string? Image { get; set; }
        public bool? Status { get; set; }
        public string? RoomDescription { get; set; }        
        
    }
}