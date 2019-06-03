namespace DbContextTests.Services
{
    interface IOrderingService
    {
        void MakeOrder(string itemName, int userId);
    }
}