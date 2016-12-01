namespace ApiApp.Core.Interfaces
{
    public interface IAccountRepository
    {
        string GetHashedPassword(string username);
    }
}
