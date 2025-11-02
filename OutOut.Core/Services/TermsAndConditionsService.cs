using AutoMapper;
using MongoDB.Driver;
using OutOut.Constants;
using OutOut.Constants.Enums;
using OutOut.Constants.Errors;
using OutOut.Infrastructure.Services;
using OutOut.Models.Exceptions;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Requests.TermsAndConditions;
using OutOut.ViewModels.Responses.TermsAndConditions;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Core.Services
{
    public class TermsAndConditionsService
    {
        private readonly IMapper _mapper;
        private readonly ITermsAndConditionsRepository _termsAndConditionsRepo;
        private readonly IVenueRepository _venueRepository;
        private readonly NotificationComposerService _notificationComposerService;

        public TermsAndConditionsService(IMapper mapper,
                                         ITermsAndConditionsRepository termsAndConditionsRepo,
                                         IVenueRepository venueRepository,
                                         NotificationComposerService notificationComposerService)
        {
            _mapper = mapper;
            _termsAndConditionsRepo = termsAndConditionsRepo;
            _venueRepository = venueRepository;
            _notificationComposerService = notificationComposerService;
        }

        public async Task<Page<TermsAndConditionsResponse>> GetTermsAndConditionsPage(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var termsAndConditionsList = await _termsAndConditionsRepo.GetTermsAndConditionsPage(paginationRequest, filterRequest);
            return _mapper.Map<Page<TermsAndConditionsResponse>>(termsAndConditionsList);
        }

        public async Task<List<TermsAndConditionsResponse>> GetActiveTermsAndConditions()
        {
            var termsAndConditionsList = await _termsAndConditionsRepo.GetActiveTermsAndConditions();
            return _mapper.Map<List<TermsAndConditionsResponse>>(termsAndConditionsList);
        }

        public async Task<TermsAndConditionsResponse> GetTermAndCondition(string id)
        {
            var termsAndConditions = await _termsAndConditionsRepo.GetById(id);
            if (termsAndConditions == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            return _mapper.Map<TermsAndConditionsResponse>(termsAndConditions);
        }

        public async Task<TermsAndConditionsResponse> CreateTermsAndConditions(TermsAndConditionsRequest request)
        {
            var filters = Builders<TermsAndConditions>.Filter.Where(a => a.TermCondition.ToLower() == request.TermCondition.ToLower());

            var terms = await _termsAndConditionsRepo.Find(filters);
            if(terms != null && terms.Count > 0)
                throw new OutOutException(ErrorCodes.TermAndConditionAlreadyExists);

            var termsAndConditions = _mapper.Map<TermsAndConditions>(request);
            var result = await _termsAndConditionsRepo.Create(termsAndConditions);
            if (request.IsActive)
                await _notificationComposerService.SendSignalRNotification(NotificationAction.TermsAndConditions,
                                                                               $"Terms and conditions have been updated by Super Admin",
                                                                               result.Id,
                                                                               Roles.VenueAdmin);
            return _mapper.Map<TermsAndConditionsResponse>(result);
        }

        public async Task<TermsAndConditionsResponse> UpdateTermsAndConditions(string id, TermsAndConditionsRequest request)
        {
            var termsAndConditions = await _termsAndConditionsRepo.GetById(id);
            if (termsAndConditions == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            termsAndConditions = _mapper.Map(request, termsAndConditions);
            var result = await _termsAndConditionsRepo.Update(termsAndConditions);
            if (request.IsActive)
                await _notificationComposerService.SendSignalRNotification(NotificationAction.TermsAndConditions,
                                                                           $"Terms and conditions have been updated by Super Admin",
                                                                           result.Id,
                                                                           Roles.VenueAdmin);
            return _mapper.Map<TermsAndConditionsResponse>(result);
        }

        public async Task<bool> DeleteTermsAndConditions(string id)
        {
            var termsAndConditions = await _termsAndConditionsRepo.GetById(id);
            if (termsAndConditions == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            await _venueRepository.DeleteTermsAndConditions(id);

            return await _termsAndConditionsRepo.Delete(id);
        }
    }
}
