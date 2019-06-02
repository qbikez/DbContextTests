using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests
{
    class UserTestData
    {
        public static void PrepareUser(int userId)
        {
            using (var db = new MyContext())
            {
                if (!db.Users.Any(u => u.Id == userId))
                {
                    db.Users.Add(new Model.User()
                    {
                        Id = userId,
                        UserName = $"testuser"
                    });
                    db.SaveChanges();
                }
            }
        }
    }
}
