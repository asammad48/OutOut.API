using AutoMapper;
using OutOut.Constants.Errors;
using OutOut.Models.Exceptions;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.EventRequest;
using OutOut.ViewModels.Wrappers;
using System.Net;

namespace OutOut.Core.Services
{
    public class EventRequestService
    {
        private readonly IEventRequestRepository _eventRequestRepository;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly IMapper _mapper;

        public EventRequestService(IEventRequestRepository eventRequestRepository, IUserDetailsProvider userDetailsProvider, IMapper mapper)
        {
            _eventRequestRepository = eventRequestRepository;
            _userDetailsProvider = userDetailsProvider;
            _mapper = mapper;
        }

        public async Task<Page<EventRequestSummaryDTO>> GetEventsAwaitingApproval(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var result = await _eventRequestRepository.GetAllEventsRequests(paginationRequest, filterRequest);
            return _mapper.Map<Page<EventRequestSummaryDTO>>(result);
        }

        public async Task<Page<EventRequestSummaryDTO>> GetMyEventsAwaitingApproval(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var result = await _eventRequestRepository.GetAllEventsRequests(paginationRequest, filterRequest, _userDetailsProvider.UserId);
            return _mapper.Map<Page<EventRequestSummaryDTO>>(result);
        }

        public async Task<EventRequestDTO> GetEventAwaitingApprovalDetailsForAdmin(string requestId)
        {
            var eventResult = await _eventRequestRepository.GetEventRequestById(requestId);
            if (eventResult == null)
                throw new OutOutException(ErrorCodes.RequestForApprovalNotFound);

            await _userDetailsProvider.ReInitialize();

            if (eventResult.LastModificationRequest.CreatedBy != _userDetailsProvider.UserId && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisEvent, HttpStatusCode.Forbidden);

            return _mapper.Map<EventRequestDTO>(eventResult);
        }

        public async Task<bool> DeleteEventRequest(string requestId)
        {
            var eventRequest = await _eventRequestRepository.GetEventRequestById(requestId);
            if (eventRequest == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            if (eventRequest.LastModificationRequest.CreatedBy != _userDetailsProvider.UserId)
                throw new OutOutException(ErrorCodes.YouCannotDeleteThisRequest);

            return await _eventRequestRepository.DeleteEventRequest(requestId);
        }
    }
}
