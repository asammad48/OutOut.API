using AutoMapper;
using OutOut.Constants.Errors;
using OutOut.Models.Exceptions;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.Events;
using OutOut.ViewModels.Responses.Offers;
using OutOut.ViewModels.Responses.TermsAndConditions;
using OutOut.ViewModels.Responses.VenueRequest;
using OutOut.ViewModels.Wrappers;
using System.Net;

namespace OutOut.Core.Services
{
    public class VenueRequestService
    {
        private readonly IVenueRequestRepository _venueRequestRepository;
        private readonly IMapper _mapper;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly ITermsAndConditionsRepository _termsAndConditionsRepository;
        private readonly VenueService _venueService;

        public VenueRequestService(IMapper mapper, IUserDetailsProvider userDetailsProvider, VenueService venueService, IVenueRequestRepository venueRequestRepository, ITermsAndConditionsRepository termsAndConditionsRepository)
        {
            _venueRequestRepository = venueRequestRepository;
            _mapper = mapper;
            _userDetailsProvider = userDetailsProvider;
            _venueService = venueService;
            _termsAndConditionsRepository = termsAndConditionsRepository;
        }

        public async Task<Page<VenueRequestSummaryDTO>> GetVenuesAwaitingApproval(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var venuesPage = await _venueRequestRepository.GetVenueRequests(paginationRequest, filterRequest);
            return _mapper.Map<Page<VenueRequestSummaryDTO>>(venuesPage);
        }

        public async Task<Page<VenueRequestSummaryDTO>> GetMyVenuesAwaitingApproval(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var venuesPage = await _venueRequestRepository.GetVenueRequests(paginationRequest, filterRequest, _userDetailsProvider.UserId);
            return _mapper.Map<Page<VenueRequestSummaryDTO>>(venuesPage);
        }

        public async Task<VenueRequestDTO> GetAwaitingVenueDetailsForAdmin(string requestId)
        {
            var user = _userDetailsProvider.User;
            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var venueRequest = await _venueRequestRepository.GetVenueRequestById(requestId);
            if (venueRequest == null)
                throw new OutOutException(ErrorCodes.RequestForApprovalNotFound);

            await _userDetailsProvider.ReInitialize();

            if (venueRequest.LastModificationRequest.CreatedBy != _userDetailsProvider.UserId && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            return _mapper.Map<VenueRequestDTO>(venueRequest);
        }

        public async Task<Page<OfferWithUsageResponse>> GetOffersPageInAwaitingVenue(string requestId, PaginationRequest paginationRequest)
        {
            var venueRequest = await _venueRequestRepository.GetVenueRequestById(requestId);
            if (venueRequest == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            await _userDetailsProvider.ReInitialize();

            if (venueRequest.LastModificationRequest.CreatedBy != _userDetailsProvider.UserId && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            var offers = await _venueRequestRepository.GetOffersByRequestId(requestId);;
            return _mapper.Map<Page<OfferWithUsageResponse>>(offers.GetPaged(paginationRequest));
        }

        public async Task<Page<EventSummaryResponse>> GetUpcomingEventsPageInAwaitingVenue(string requestId, PaginationRequest paginationRequest)
        {
            var venueRequest = await _venueRequestRepository.GetVenueRequestById(requestId);
            if (venueRequest == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            await _userDetailsProvider.ReInitialize();

            if (venueRequest.LastModificationRequest.CreatedBy != _userDetailsProvider.UserId && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            return await _venueService.PaginateUpcomingEventsInVenue(venueRequest.Venue.Events, paginationRequest);
        }

        public async Task<List<TermsAndConditionsResponse>> GetAwaitingVenueTermsAndConditions(string requestId)
        {
            var venueRequest = await _venueRequestRepository.GetVenueRequestById(requestId);
            if (venueRequest == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            await _userDetailsProvider.ReInitialize();

            if (venueRequest.LastModificationRequest.CreatedBy != _userDetailsProvider.UserId && !_userDetailsProvider.IsSuperAdmin)
                throw new OutOutException(ErrorCodes.YouDontHaveAccessToThisVenue, HttpStatusCode.Forbidden);

            var venueTermsAndConditions = await _termsAndConditionsRepository.GetVenueTermsAndConditions(venueRequest.Venue.SelectedTermsAndConditions);

            return _mapper.Map<List<TermsAndConditionsResponse>>(venueTermsAndConditions);
        }

        public async Task<bool> DeleteVenueRequest(string requestId)
        {
            var venueRequest = await _venueRequestRepository.GetVenueRequestById(requestId);
            if (venueRequest == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            if (venueRequest.LastModificationRequest.CreatedBy != _userDetailsProvider.UserId)
                throw new OutOutException(ErrorCodes.YouCannotDeleteThisRequest);

            return await _venueRequestRepository.DeleteVenueRequest(requestId);
        }
    }
}
