using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using OutOut.Constants.Enums;
using OutOut.Constants.Extensions;
using OutOut.Core.Services;
using OutOut.Helpers.Attributes;
using OutOut.Infrastructure.Services;
using OutOut.Models;
using OutOut.Models.Models;
using OutOut.ViewModels.Responses.DeveloperTools;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [DevelopmentOnly]
    public class DeveloperToolsController : ControllerBase
    {
        private readonly IHubContext<NotificationHub, INotificationHub> _hub;
        private readonly CustomerService _customerService;
        private readonly AuthService _authService;
        public DeveloperToolsController(CustomerService customerService, AuthService authService, IHubContext<NotificationHub, INotificationHub> hub, IOptions<AppSettings> appSetting)
        {
            _customerService = customerService;
            _authService = authService;
            _hub = hub;
        }

        #region Enums

        private readonly string[] CONSTANT_ENUMS_NAMESPACE = new string[] { "OutOut.Constants.Enums" };
        private readonly string[] FILTERATION_ENUMS_NAMESPACES = new string[] { "OutOut.ViewModels.Enums" };

        [HttpGet]
        public IActionResult SystemEnums()
        {
            var constantsEnumsTypes = GetEnumTypesInNamespaces(CONSTANT_ENUMS_NAMESPACE);
            var filterationEnumsTypes = GetEnumTypesInNamespaces(FILTERATION_ENUMS_NAMESPACES);

            return Ok(SuccessHelper.Wrap(new SystemEnumsResponse
            {
                ConstantEnums = InflateEnumViewModel(constantsEnumsTypes),
                FilterationEnums = InflateEnumViewModel(filterationEnumsTypes),
            }));
        }

        private IEnumerable<Type> GetEnumTypesInNamespaces(string[] namespaces)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                   .Where(t => namespaces.Contains(t.Namespace) && t.IsEnum);

        }
        private List<EnumResponse> InflateEnumViewModel(IEnumerable<Type> enumTypes)
        {
            List<EnumResponse> returnViewModel = new List<EnumResponse>();

            foreach (Type type in enumTypes)
            {
                var currentEnum = new EnumResponse { EnumName = type.Name };

                foreach (var enumValue in Enum.GetValues(type))
                {
                    currentEnum.EnumValues.Add(((Enum)enumValue).ToDisplayName(), (int)enumValue);
                }

                returnViewModel.Add(currentEnum);
            }

            return returnViewModel;
        }

        #endregion

        [Produces(typeof(OperationResult<bool>))]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var result = await _customerService.DeleteUser(email);
            return Ok(SuccessHelper.Wrap(result));
        }

        [HttpPost]
        public async Task<IActionResult> VerifyAndAssignSuperAdminRole(string email)
        {
            await _authService.VerifyAndAssignSuperAdminRole(email);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> PushNotificationToSuperAdmins()
        {
            var notification = new Notification("userId",
                                                 NotificationAction.VenueRequestDetails,
                                                 "123",
                                                 $"New Venue Added");
            await _hub.Clients.Group("44b920e4-8697-42e7-804c-efa5ffbb04ad").ReceiveNotification(notification);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> PushNotificationToVenueAdmins()
        {
            var notification = new Notification("userId",
                                                NotificationAction.NewVenueBooking,
                                                "123",
                                                $"New Booking");
            await _hub.Clients.Group("ae2fc349-92c1-4c43-8d41-952628f1da03").ReceiveNotification(notification);
            return Ok();
        }
    }
}
