﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace QQEgg_Backend.Models
{
    public partial class TPsiteRoom
    {
        public TPsiteRoom()
        {
            TCorderDetail = new HashSet<TCorderDetail>();
            TEvaluations = new HashSet<TEvaluations>();
        }

        public int RoomId { get; set; }
        public int? SiteId { get; set; }
        public int? CategoryId { get; set; }
        public decimal? HourPrice { get; set; }
        public decimal? DatePrice { get; set; }
        public int? Ping { get; set; }
        public string? Image { get; set; }
        public bool? Status { get; set; }
        public string? Description { get; set; }
        public string? RoomPassWork { get; set; }

        public virtual TCategory Category { get; set; }
        public virtual TPsite Site { get; set; }
        public virtual ICollection<TCorderDetail> TCorderDetail { get; set; }
        public virtual ICollection<TEvaluations> TEvaluations { get; set; }
    }
}