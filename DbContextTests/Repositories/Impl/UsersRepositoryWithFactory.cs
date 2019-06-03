using DbContextTests.Infrastructure;
using DbContextTests.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace DbContextTests.Repositories
{
    public class UsersRepositoryWithFactory : IUsersRepository
    {
        private readonly IContextFactory<MyContext> contextFactory;

        public UsersRepositoryWithFactory(IContextFactory<MyContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public User Find(int userId)
        {
            using (var db = contextFactory.Create())
            {
                return db.Users.Include(u => u.UserPreferences).SingleOrDefault(u => u.Id == userId);
            }
        }

        public void Update(User user)
        {
            using (var db = contextFactory.Create())
            {
                // not sure if this is required - if the object is retrieved from the same context instance, the state should be correct,
                // unless change tracking is disabled
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();
            }
        }
    }
}
