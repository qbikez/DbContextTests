using DbContextTests.Model;

namespace DbContextTests.Repositories
{
    public interface IOrdersRepository
    {
        void Add(Order order);
    }
}