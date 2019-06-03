using DbContextTests.Model;

namespace DbContextTests.Repositories
{
    public interface IUsersRepository
    {
        User Find(int userId);
        void Update(User user);
    }
}