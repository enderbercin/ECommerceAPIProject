using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceProject.API.Entities
{
    [Table("Carts")]
    public class Cart
    {
        [Key]
        public int CardId { get; set; }
        public DateTime Date { get; set; }
        public bool IsClosed { get; set; }
        public int? AccountId { get; set; }
        public virtual Account Account { get; set; }
        public List<CartProduct> CartProducts { get; set; }
        public Cart()
        {
            CartProducts = new List<CartProduct>();
        }
    }
}
