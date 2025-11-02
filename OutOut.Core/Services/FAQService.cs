using AutoMapper;
using OutOut.Constants.Errors;
using OutOut.Models.Exceptions;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces;
using OutOut.ViewModels.Requests.FAQs;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Responses.FAQs;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Core.Services
{
    public class FAQService
    {
        private readonly IMapper _mapper;
        private readonly IFAQRepository _faqRepository;
        private const string KEY_FAQ = "FAQ Number";
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public FAQService(IMapper mapper, IFAQRepository faqRepo)
        {
            _mapper = mapper;
            _faqRepository = faqRepo;
        }

        public async Task<Page<FAQResponse>> GetFAQPage(PaginationRequest paginationRequest, FAQFilterationRequest filterRequest)
        {
            var faqList = await _faqRepository.GetFAQPage(paginationRequest, filterRequest);
            return _mapper.Map<Page<FAQResponse>>(faqList);
        }

        public async Task<Page<FAQResponse>> GetAllFAQ(PaginationRequest paginationRequest, SearchFilterationRequest searchFilterRequest)
        {
            var faqList = await _faqRepository.GetAllFAQ(paginationRequest, searchFilterRequest);
            return _mapper.Map<Page<FAQResponse>>(faqList);
        }

        public async Task<FAQResponse> GetFAQ(string id)
        {
            var faq = await _faqRepository.GetById(id);
            if (faq == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            return _mapper.Map<FAQResponse>(faq);
        }

        public async Task<FAQResponse> CreateFAQ(FAQRequest request)
        {
            var faq = _mapper.Map<FAQ>(request);
            faq.QuestionNumber = await _faqRepository.GenerateLastIncrementalNumber(KEY_FAQ);

            var faqResult = await _faqRepository.Create(faq);
            return _mapper.Map<FAQResponse>(faqResult);
        }

        public async Task<int> GetNextQuestionNumber() => await _faqRepository.GetNextIncrementalNumber(KEY_FAQ);

        public async Task<FAQResponse> UpdateFAQ(string id, FAQRequest request)
        {
            var faq = await _faqRepository.GetById(id);
            if (faq == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            faq = _mapper.Map(request, faq);
            faq = await _faqRepository.Update(faq);
            return _mapper.Map<FAQResponse>(faq);
        }

        public async Task<bool> DeleteFAQ(string id)
        {
            bool result;
            try
            {
                await semaphore.WaitAsync();

                var faq = await _faqRepository.GetById(id);
                if (faq == null)
                    throw new OutOutException(ErrorCodes.RequestNotFound);

                result = await _faqRepository.Delete(id) && await _faqRepository.ResetQuestionNumbers(faq.QuestionNumber);
                await _faqRepository.SubtractOneIncrementalNumber(KEY_FAQ);
            }
            finally
            {
                semaphore.Release();
            }

            return result;
        }
    }
}
