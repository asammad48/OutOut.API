using AutoMapper;
using OutOut.Constants.Enums;
using OutOut.Constants.Errors;
using OutOut.Models.Exceptions;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Requests.CustomersSupport;
using OutOut.ViewModels.Requests.ManageAdminDashboard;
using OutOut.ViewModels.Responses.CustomersSupport;
using OutOut.ViewModels.Wrappers;
using System.Net;

namespace OutOut.Core.Services
{
    public class CustomerSupportService
    {
        private readonly IMapper _mapper;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly ICustomerSupportRepository _customerSupportRepository;

        public CustomerSupportService(IMapper mapper,
                                      IUserDetailsProvider userDetailsProvider,
                                      ICustomerSupportRepository customerSupportRepository)
        {
            _mapper = mapper;
            _userDetailsProvider = userDetailsProvider;
            _customerSupportRepository = customerSupportRepository;
        }

        public async Task<CustomerSupportResponse> PostNewRequest(CustomerSupportRequest request)
        {
            var currentUser = _userDetailsProvider.User;
            if (currentUser == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Unauthorized);

            var message = _mapper.Map<CustomerSupportMessage>(request);
            message.FullName = string.IsNullOrEmpty(message.FullName) ? currentUser.FullName : message.FullName;
            message.PhoneNumber = string.IsNullOrEmpty(message.PhoneNumber) ? currentUser.PhoneNumber : message.PhoneNumber;
            message.Email = currentUser.Email;
            message.CreatedBy = _userDetailsProvider.UserId;

            var result = await _customerSupportRepository.Create(message);

            return _mapper.Map<CustomerSupportResponse>(result);
        }

        public async Task<Page<CustomerSupportResponse>> GetCustomerServiceRequestsPage(PaginationRequest paginationRequest, FilterationRequest filterRequest)
        {
            var result = await _customerSupportRepository.GetAllCustomerServices(paginationRequest, filterRequest);
            return _mapper.Map<Page<CustomerSupportResponse>>(result);
        }

        public async Task<CustomerSupportResponse> GetCustomerServiceRequest(string id)
        {
            var request = await _customerSupportRepository.GetById(id);
            if (request == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            return _mapper.Map<CustomerSupportResponse>(request);
        }

        public async Task<bool> ResolveCustomerServiceRequest(string id)
        {
            var request = await _customerSupportRepository.GetById(id);
            if (request == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            return await _customerSupportRepository.UpdateStatus(id, CustomerSupportStatus.Resolved);
        }

        public async Task<bool> RejectCustomerServiceRequest(string id)
        {
            var request = await _customerSupportRepository.GetById(id);
            if (request == null)
                throw new OutOutException(ErrorCodes.RequestNotFound);

            return await _customerSupportRepository.UpdateStatus(id, CustomerSupportStatus.Rejected);
        }
    }
}
