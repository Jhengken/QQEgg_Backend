﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace QQEgg_Backend.Models
{
    public partial class TCorderDetail
    {
        public int OrderId { get; set; }
        public int? RoomId { get; set; }
        public int? CouponId { get; set; }
        public int? Price { get; set; }

        public virtual TCoupons Coupon { get; set; }
        public virtual TCorders Order { get; set; }
        public virtual TPsiteRoom Room { get; set; }
    }
}