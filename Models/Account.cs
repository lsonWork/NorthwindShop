using System;
using System.Collections.Generic;

namespace Assignment.Models
{
    public partial class Account
    {
        public Account()
        {
            Carts = new HashSet<Cart>();
        }

        public int AccountId { get; set; }
        public string? CustomerId { get; set; }
        public int? EmployeeId { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int? Role { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }
    }
}
