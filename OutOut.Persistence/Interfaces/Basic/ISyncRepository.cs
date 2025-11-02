using OutOut.Models.EntityInterfaces;

namespace OutOut.Persistence.Interfaces.Basic
{
    public interface ISyncRepository<TOtherEntity> where TOtherEntity : INonSqlEntity
    {
        Task Sync(TOtherEntity oldOtherEntity, TOtherEntity otherEntity);
    }
}
