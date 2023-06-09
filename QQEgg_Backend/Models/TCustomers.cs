﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace QQEgg_Backend.Models
{
    public partial class TCustomers
    {
        public TCustomers()
        {
            TCorders = new HashSet<TCorders>();
            TEvaluations = new HashSet<TEvaluations>();
        }

        public int CustomerId { get; set; }
        public string Name { get; set; }
        public bool? Sex { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public DateTime? Birth { get; set; }
        public string CreditCard { get; set; }
        public int? CreditPoints { get; set; }
        public bool? BlackListed { get; set; }

        public virtual ICollection<TCorders> TCorders { get; set; }
        public virtual ICollection<TEvaluations> TEvaluations { get; set; }
    }
}