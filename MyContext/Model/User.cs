using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Model
{
    public class User
    {
        [Key()]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string UserName { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public int OrdersCount { get; set; }

        public void IncreaseOrdersCount()
        {
            // is it better to allow external code to modify OrdersCount directory?
            // if yes, then who should do it:
            // a) anyone?
            // b) OrderingService?
            // c) OrdersRepository?
            // d) OrdersManager?

            OrdersCount++;
        }
    }
}
