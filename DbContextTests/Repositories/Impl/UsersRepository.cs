using DbContextTests.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly MyContext db;

        public UsersRepository(MyContext db)
        {
            this.db = db;
        }

        public User Find(int userId)
        {
            return db.Users.Find(userId);
        }

        public void Update(User user)
        {
            // not sure if this is required - if the object is retrieved from the same context instance, the state should be correct,
            // unless change tracking is disabled
            db.Entry(user).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();
        }
    }
}
