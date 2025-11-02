using OutOut.Models.Models;
using System.Threading.Tasks;

namespace OutOut.Infrastructure.Services
{
    public interface INotificationHub
    {
        Task ReceiveNotification(Notification notification);
    }
}
