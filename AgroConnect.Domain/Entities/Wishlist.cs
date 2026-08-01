using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroConnect.Domain.Entities
{
    public class Wishlist
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser? ApplicationUser { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
