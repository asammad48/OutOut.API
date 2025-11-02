using OutOut.ViewModels.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOut.ViewModels.Requests.Categories
{
    public class UpdateCategoriesOrders
    {
        public List<UpdateCategoryOrder> UpdateCategoryOrders { get; set; }
    }

    public class UpdateCategoryOrder
    {
        [MongoId]
        public string Id { get; set; }

        public int Order { get; set; }

    }
}
