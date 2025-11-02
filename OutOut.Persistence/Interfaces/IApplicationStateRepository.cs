namespace OutOut.Persistence.Interfaces
{
    public interface IApplicationStateRepository
    {
        Task<string> GetByKey(string key);
        Task SetByKey(string key, string value);
    }
}
