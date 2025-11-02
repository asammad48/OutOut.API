using OutOut.Models.Utils;

namespace OutOut.Core.Utils
{
    public class DefaultTimeProvider : ITimeProvider
    {
        public DateTime Now { get { return UAEDateTime.Now; } }
    }

    public interface ITimeProvider
    {
        DateTime Now { get; }
    }
}
