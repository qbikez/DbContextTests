using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Model
{
    public class Order
    {
        [Key()]
        public int Id { get; set; }
        public string Item { get; set; }
        public int SequenceNo { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Obsolete("For EF only", true)]
        public Order()
        {
            // for EF
            // this has to be public IF we want LazyLoading
        }

        public Order(string itemName)
        {
            Item = itemName;
        }

        public Order(string itemName, int userId)
        {
            Item = itemName;
            UserId = userId;
        }
    }
}
