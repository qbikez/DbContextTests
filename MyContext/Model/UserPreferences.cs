using DbContextTests.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyContext.Model
{
    public class UserPreferences
    {
        [Key, ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public string FavoriteProduct { get; set; }
    }
}
