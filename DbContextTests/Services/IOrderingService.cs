namespace DbContextTests.Services
{
    interface IOrderingService
    {
        void MakeOrder(string itemName, int userId);
        void SetUserAddress(int userId, global::MyContext.Model.Address newAddress);
        void SetUserPreferences(int userId, string favoriteItem);
    }
}