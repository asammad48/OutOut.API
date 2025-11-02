using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOut.Models.Wrappers
{
    public class CustomNotificationPage<T> : Page<T>
    {
        public long UnReadNotificationsCount { get; set; }
        public CustomNotificationPage(List<T> records, int pageNumber, int pageSize, long recordsTotalCount, long unReadNotificationsCount) : base(records, pageNumber, pageSize, recordsTotalCount)
        {
            UnReadNotificationsCount = unReadNotificationsCount;
        }
    }
}
