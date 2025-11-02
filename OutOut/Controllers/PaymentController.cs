using Microsoft.AspNetCore.Mvc;
using OutOut.Core.Services;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly EventBookingService _eventBookingService;

        public PaymentController(EventBookingService eventBookingService)
        {
            _eventBookingService = eventBookingService;
        }

        [Produces(typeof(OperationResult<string>))]
        [HttpGet]
        public async Task<IActionResult> Paid([FromQuery] string id)
        {
            var result = await _eventBookingService.HandleTelrBooking(id);
            return Ok(result);
        }

        [Produces(typeof(OperationResult<string>))]
        [HttpGet]
        public async Task<IActionResult> Cancelled([FromQuery] string id)
        {
            var result = await _eventBookingService.HandleTelrBooking(id);
            return Ok(result);
        }

        [Produces(typeof(OperationResult<string>))]
        [HttpGet]
        public async Task<IActionResult> Declined([FromQuery] string id)
        {
            var result = await _eventBookingService.HandleTelrBooking(id);
            return Ok(result);
        }

        [Produces(typeof(OperationResult<string>))]
        [HttpGet]
        public async Task<IActionResult> OnHold([FromQuery] string id)
        {
            var result = await _eventBookingService.HandleTelrBooking(id);
            return Ok(result);
        }
    }
}
