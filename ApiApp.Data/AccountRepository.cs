using ApiApp.Core.Interfaces;

namespace ApiApp.Data
{
    public class AccountRepository : IAccountRepository
    {
        public string GetHashedPassword(string username)
        {
            return "password";
        }
    }
}
