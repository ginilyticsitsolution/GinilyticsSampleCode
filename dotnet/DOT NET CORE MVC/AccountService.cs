using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WirelessSupport.Model.DataModel;
using WirelessSupport.Model.ViewModel;
using WirelessSupport.Model.ViewModel.Enums;
using WirelessSupport.Model.ViewModel.CompanyAccountViewmodel;
using WirelessSupport.Repository.Repository.Contracts.ComapnyAccount;
using WirelessSupport.Service.Services.Contracts.CompanyAccount;
using Microsoft.Extensions.Logging;
using WirelessSupport.Repository.Repository.Contracts;
using WirelessSupport.Model.Enums;
using WirelessSupport.Service.Helper;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using MapsterMapper;
using WirelessSupport.Service.Extensions;
using Dapper;
using WirelessSupport.Repository.SetUp.Provider;
using System.Data;
using WirelessSupport.Service.Services.Contracts;

namespace WirelessSupport.Service.Services.CompanyAccountServices
{
    public class AccountService : IComapnyAccount
    {
        private IAccountRepository _accountRepository;
        private IRepository<object> _repository;
        private IAccountRepRepository _accountRepRepository;
        private IAccountLines _accountLinesRepository;
        private IDivisionRepository _divisionRepository;
        private IAccountDeviceLease _accountDeviceLease;
        private IAccountAddressRepository _accountAddress;
        private IAccountCarrierRepository _accountCarrierRepository;
        private IDropdownRepository _dropdownRepository;
        private IAccountNotesRepository _accountNotesRepository;
        private IAccountAddedServiceRepository _accountAddedServiceRepository;
        private IAccountFeatures _accountFeatures;
        private readonly ISettingStatusRepository _settingStatusRepository;
        private ILogger<AccountService> _logger;
        private IDocumentRepository _documentsRepository;
        private ServiceResult _serviceResult;
        private ServiceResult _onErrorServiceResult;
        private readonly EmailHelper _emailHelper;
        private readonly int _pageSize = 10;
        private readonly int _pageSizeDas = 5;
        private readonly IMapper _mapper;
        private readonly IKeyValueRepository _keyValueRepository;
        public readonly WirelessSupportContext _context;
        private readonly IEmailService _emailService;

        public AccountService(IOptions<AppSettings> appSettings, IAccountRepository accountRepository, IAccountAddressRepository accountAddress, IAccountLines accountLinesRepository,
            IAccountNotesRepository accountNotesRepository, IAccountCarrierRepository accountCarrierRepository, IDivisionRepository divisionRepository,
            IAccountRepRepository accountRepRepository, IAccountFeatures accountFeatures, IDocumentRepository documentRepository,
            IAccountAddedServiceRepository accountAddedServiceRepository, IRepository<object> repository, IDropdownRepository dropdownRepository, IAccountDeviceLease accountDeviceLease,
            ILogger<AccountService> logger, ISettingStatusRepository settingStatusRepository, IMapper _mapper, IKeyValueRepository keyValueRepository, WirelessSupportContext context,
            IEmailService emailService)
        {
            _accountRepository = accountRepository;
            _accountRepRepository = accountRepRepository;
            _accountNotesRepository = accountNotesRepository;
            _accountFeatures = accountFeatures;
            _accountAddress = accountAddress;
            _accountAddedServiceRepository = accountAddedServiceRepository;
            _accountLinesRepository = accountLinesRepository;
            _accountCarrierRepository = accountCarrierRepository;
            _divisionRepository = divisionRepository;
            _documentsRepository = documentRepository;
            _dropdownRepository = dropdownRepository;
            _accountDeviceLease = accountDeviceLease;
            _settingStatusRepository = settingStatusRepository;
            _repository = repository;
            _logger = logger;
            _serviceResult = new ServiceResult { Message = "Success", Status = true };
            _onErrorServiceResult = new ServiceResult { Message = "Error", Status = false };
            _emailHelper = new EmailHelper(appSettings);
            this._mapper = _mapper;
            _keyValueRepository = keyValueRepository;
            _context = context;
            _emailService = emailService;
        }

        public async Task<PagingList<CompanyViewModel>> GetAccountList(string userId, int accountId, int pageNo, string sortField, string sortOrder, CompanyViewModel companyViewModel, int pageSize = 10)
        {
            try
            {
                pageNo = pageNo == 0 ? 1 : pageNo;
                companyViewModel.MyAccount = false;

                var Accounts = await this.GetAccounts(companyViewModel.MyAccount, userId, companyViewModel.Filters, companyViewModel.AdvanceSearchKey, companyViewModel.AdvanceSearchValue, companyViewModel.NextContactDate, companyViewModel.AccountStatusId, companyViewModel.CarrierId, companyViewModel.AccountRepId, companyViewModel.AccountsWithSale);
                companyViewModel.ALLAccountTotalCount = Accounts.Count();
                var AccountList = new List<CompanyViewModel>();
                var AccountRepoList = //_accountRepository.Get_AcountList(pageNo, _pageSize, sortField, sortOrder, companyViewModel);
                    PagingList<Model.ViewModel.CompanyViewModel>.CreateAsync(Accounts, pageNo,pageSize, sortField, sortOrder);
                int Count = Accounts.Count();

                foreach (var item in AccountRepoList)
                {
                    AccountList.Add(new CompanyViewModel
                    {
                        AccountId = item.AccountId,
                        AccountName = item.AccountName,
                        CurrentStatusId = item.CurrentStatusId,
                        AccountRepId = item.AccountRepId,
                        AccountRep = item.AccountRep,
                        IndustryType = item.IndustryType,
                        CustomerSource = item.CustomerSource,
                        AccountRepOutside = item.AccountRepOutside,
                        IsReferToLegal = item.IsReferToLegal,
                        AccountType = item.AccountType,
                        IsPartner = item.IsPartner,
                        Description = item.Description,
                        AccountImage = item.AccountImage,
                        Website = item.Website,
                        UserId = item.UserId,
                        Fax = item.Fax,
                        AnnualRevenueEst = item.AnnualRevenueEst,
                        TaxId = item.TaxId,
                        EmployeeCount = item.EmployeeCount,
                        NoofDevices = item.NoofDevices,
                        IndustryTypeId = item.IndustryTypeId,
                        CustomerSourceId = item.CustomerSourceId,
                        Activelines = item.Activelines,
                        Phone = item.Phone,
                        Email = item.Email,
                        LastConnect = item.LastConnect,
                        Lastpurcahse = item.Lastpurcahse,
                        NextContactDate = item.NextContactDate,
                        DateEntered = item.DateEntered,
                        MainPhone = item.MainPhone,
                        WebFormLink = item.WebFormLink,
                        AccountOwnerId = item.AccountOwnerId,
                        CarrierId = item.CarrierId,
                        Carrier = item.Carrier,
                        User = item.User,
                        ContactName = item.ContactName,
                        ModifiedDate = item.ModifiedDate,
                        ALLAccountTotalCount = Count,
                        MyAccountTotalCount = Count,
                        ModifiedBy = item.ModifiedBy,
                        AboutAccount = item.AboutAccount,


                    });


                }
                var list = PagingList<CompanyViewModel>.ConvertListAsync(AccountList.AsQueryable(), pageNo, AccountRepoList.TotalPages, sortField, sortOrder);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<CompanyViewModel>> GetAllAccountForExport(string userId, CompanyViewModel companyViewModel)
        {
            try
            {
                //pageNo = pageNo == 0 ? 1 : pageNo;
                companyViewModel.MyAccount = false;

                var Accounts = await this.GetAccounts(companyViewModel.MyAccount, userId, companyViewModel.Filters, companyViewModel.AdvanceSearchKey, companyViewModel.AdvanceSearchValue, companyViewModel.NextContactDate, companyViewModel.AccountStatusId, companyViewModel.CarrierId);
                companyViewModel.ALLAccountTotalCount = Accounts.Count();
                var AccountList = new List<CompanyViewModel>();
                //var AccountRepoList = //_accountRepository.Get_AcountList(pageNo, _pageSize, sortField, sortOrder, companyViewModel);
                //    List<Model.ViewModel.CompanyViewModel>.CreateAsync(Accounts);
                int Count = Accounts.Count();

                foreach (var item in Accounts)
                {
                    AccountList.Add(new CompanyViewModel
                    {
                        AccountId = item.AccountId,
                        AccountName = item.AccountName,
                        CurrentStatusId = item.CurrentStatusId,
                        AccountRepId = item.AccountRepId,
                        AccountRep = item.AccountRep,
                        IndustryType = item.IndustryType,
                        CustomerSource = item.CustomerSource,
                        AccountRepOutside = item.AccountRepOutside,
                        IsReferToLegal = item.IsReferToLegal,
                        AccountType = item.AccountType,
                        IsPartner = item.IsPartner,
                        Description = item.Description,
                        AccountImage = item.AccountImage,
                        Website = item.Website,
                        UserId = item.UserId,
                        Fax = item.Fax,
                        AnnualRevenueEst = item.AnnualRevenueEst,
                        TaxId = item.TaxId,
                        EmployeeCount = item.EmployeeCount,
                        NoofDevices = item.NoofDevices,
                        IndustryTypeId = item.IndustryTypeId,
                        CustomerSourceId = item.CustomerSourceId,
                        Activelines = item.Activelines,
                        Phone = item.Phone,
                        Email = item.Email,
                        LastConnect = item.LastConnect,
                        Lastpurcahse = item.Lastpurcahse,
                        NextContactDate = item.NextContactDate,
                        DateEntered = item.DateEntered,
                        MainPhone = item.MainPhone,
                        WebFormLink = item.WebFormLink,
                        AccountOwnerId = item.AccountOwnerId,
                        CarrierId = item.CarrierId,
                        Carrier = item.Carrier,
                        User = item.User,
                        ContactName = item.ContactName,
                        ModifiedDate = item.ModifiedDate,
                        ALLAccountTotalCount = Count,
                        MyAccountTotalCount = Count,
                        ModifiedBy = item.ModifiedBy,
                        AboutAccount = item.AboutAccount,


                    });


                }

                return AccountList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<CompanyViewModel>> GetAccountsForExport(string UserId, CompanyViewModel companyViewModel)
        {
            try
            {
                //pageNo = pageNo == 0 ? 1 : pageNo;
                companyViewModel.MyAccount = true;
                await GetDataTableList(companyViewModel);
                var AccountsList = await this.GetAccounts(companyViewModel.MyAccount, UserId, companyViewModel.Filters, companyViewModel.AdvanceSearchKey, companyViewModel.AdvanceSearchValue);

                companyViewModel.MyAccountTotalCount = AccountsList.Count();

                var AccountList = new List<CompanyViewModel>();
                //var AccountRepoList = PagingList<Model.ViewModel.CompanyViewModel>.CreateAsync(AccountsList, pageNo, _pageSize, sortField, sortOrder);
                int Count = AccountsList.Count();
                //var AccountRepoList = await _accountRepository.Get_MyAcountList(UserId, companyViewModel);

                foreach (var item in AccountsList)
                {
                    AccountList.Add(new CompanyViewModel
                    {
                        AccountId = item.AccountId,
                        AccountName = item.AccountName,
                        CurrentStatusId = item.CurrentStatusId,
                        AccountRepId = item.AccountRepId,
                        AccountRep = item.AccountRep,
                        IndustryType = item.IndustryType,
                        CustomerSource = item.CustomerSource,
                        AccountRepOutside = item.AccountRepOutside,
                        IsReferToLegal = item.IsReferToLegal,
                        AccountType = item.AccountType,
                        IsPartner = item.IsPartner,
                        Description = item.Description,
                        AccountImage = item.AccountImage,
                        Website = item.Website,
                        UserId = item.UserId,
                        Fax = item.Fax,
                        AnnualRevenueEst = item.AnnualRevenueEst,
                        TaxId = item.TaxId,
                        EmployeeCount = item.EmployeeCount,
                        NoofDevices = item.NoofDevices,
                        IndustryTypeId = item.IndustryTypeId,
                        CustomerSourceId = item.CustomerSourceId,
                        Activelines = item.Activelines,
                        Phone = item.Phone,
                        Email = item.Email,
                        LastConnect = item.LastConnect,
                        Lastpurcahse = item.Lastpurcahse,
                        NextContactDate = item.NextContactDate,
                        DateEntered = item.DateEntered,
                        MainPhone = item.MainPhone,
                        WebFormLink = item.WebFormLink,
                        AccountOwnerId = item.AccountOwnerId,
                        CarrierId = item.CarrierId,
                        Carrier = item.Carrier,
                        User = item.User,
                        ContactName = item.ContactName,
                        ModifiedDate = item.ModifiedDate,
                        MyAccountTotalCount = Count,
                        ALLAccountTotalCount = Count,
                        ModifiedBy = item.ModifiedBy,
                        AboutAccount = item.AboutAccount,
                        AccountIds = item.AccountIds,

                    });


                }
                //var AList = PagingList<CompanyViewModel>.ConvertListAsync(AccountList.AsQueryable(), pageNo, AccountRepoList.TotalPages, sortField, sortOrder);
                return AccountList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<CompanyViewModel> GetDataTableList(CompanyViewModel advanceSearchModel)
        {
            try
            {

                CompanyViewModel dt1 = new CompanyViewModel();
                DataTable dt = new DataTable();
                dt.Columns.Add("Id", typeof(int));
                dt.Columns.Add("AdvanceSearchKey", typeof(string));
                dt.Columns.Add("AdvanceSearchValue", typeof(string));


                return dt1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<PagingList<CompanyViewModel>> GetMyAccountList(string UserId, CompanyViewModel companyViewModel, int pageNo, string sortField, string sortOrder, int pageSize = 10)
        {
            try
            {
                pageNo = pageNo == 0 ? 1 : pageNo;
                companyViewModel.MyAccount = true;
                await GetDataTableList(companyViewModel);
                var AccountsList = await this.GetAccounts(companyViewModel.MyAccount, UserId, companyViewModel.Filters, companyViewModel.AdvanceSearchKey, companyViewModel.AdvanceSearchValue, companyViewModel.NextContactDate, companyViewModel.AccountStatusId, companyViewModel.CarrierId, companyViewModel.AccountRepId, companyViewModel.AccountsWithSale);

                companyViewModel.MyAccountTotalCount = AccountsList.Count();

                var AccountList = new List<CompanyViewModel>();
                var AccountRepoList = PagingList<Model.ViewModel.CompanyViewModel>.CreateAsync(AccountsList, pageNo, pageSize, sortField, sortOrder);
                int Count = AccountsList.Count();
                //var AccountRepoList = await _accountRepository.Get_MyAcountList(UserId, companyViewModel);

                foreach (var item in AccountRepoList)
                {
                    AccountList.Add(new CompanyViewModel
                    {
                        AccountId = item.AccountId,
                        AccountName = item.AccountName,
                        CurrentStatusId = item.CurrentStatusId,
                        AccountRepId = item.AccountRepId,
                        AccountRep = item.AccountRep,
                        IndustryType = item.IndustryType,
                        CustomerSource = item.CustomerSource,
                        AccountRepOutside = item.AccountRepOutside,
                        IsReferToLegal = item.IsReferToLegal,
                        AccountType = item.AccountType,
                        IsPartner = item.IsPartner,
                        Description = item.Description,
                        AccountImage = item.AccountImage,
                        Website = item.Website,
                        UserId = item.UserId,
                        Fax = item.Fax,
                        AnnualRevenueEst = item.AnnualRevenueEst,
                        TaxId = item.TaxId,
                        EmployeeCount = item.EmployeeCount,
                        NoofDevices = item.NoofDevices,
                        IndustryTypeId = item.IndustryTypeId,
                        CustomerSourceId = item.CustomerSourceId,
                        Activelines = item.Activelines,
                        Phone = item.Phone,
                        Email = item.Email,
                        LastConnect = item.LastConnect,
                        Lastpurcahse = item.Lastpurcahse,
                        NextContactDate = item.NextContactDate,
                        DateEntered = item.DateEntered,
                        MainPhone = item.MainPhone,
                        WebFormLink = item.WebFormLink,
                        AccountOwnerId = item.AccountOwnerId,
                        CarrierId = item.CarrierId,
                        Carrier = item.Carrier,
                        User = item.User,
                        ContactName = item.ContactName,
                        ModifiedDate = item.ModifiedDate,
                        MyAccountTotalCount = Count,
                        ALLAccountTotalCount = Count,
                        ModifiedBy = item.ModifiedBy,
                        AboutAccount = item.AboutAccount,
                        AccountIds = item.AccountIds,

                    });


                }
                var AList = PagingList<CompanyViewModel>.ConvertListAsync(AccountList.AsQueryable(), pageNo, AccountRepoList.TotalPages, sortField, sortOrder);
                return AList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<List<SelectListItem>> GetAccountSelectListItem(string userId)
        {
            try
            {


                var p = new DynamicParameters();
                p.Add("@UserId", dbType: System.Data.DbType.String, value: userId);
                //p.Add("@ColourCode", dbType: System.Data.DbType.String, value: request.param.StatusColour);
                var result = await _accountRepository.GetQuery<CompanyViewModel>("sp_GetAccountsByUserId", p, commandType: System.Data.CommandType.StoredProcedure).ConfigureAwait(false);

                var selectListItems = new List<SelectListItem>();
                var AccountList = new List<CompanyViewModel>();

                result.ToList().ForEach(x => selectListItems.Add(new SelectListItem() { Text = x.AccountName, value = Convert.ToString(x.AccountId) }));
                return selectListItems;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<int> GetAccountIdByEmail(string Email)
        {
            var AccountId = await _accountRepository.GetAccountIdByEmail(Email);
            return AccountId;
        }
        public async Task<int> GetContactByAccountId(int accountId)
        {
            var AccountId = await _accountRepository.GetContactByAccountId(accountId);
            return AccountId;
        }
        public async Task<string> GetAccountNameByEmail(string Email)
        {
            var AccountId = await _accountRepository.GetAccountNameByEmail(Email);
            return AccountId;
        }
        public async Task<CompanyViewModel> GetAccountById(int accountId)
        {
            try
            {
                var Accountobj = new CompanyViewModel();
                if (accountId > 0)
                {
                    var account = await _accountRepository.GetAccountById(accountId);
                    Accountobj.AccountId = account.AccountId;
                    Accountobj.AccountName = account.AccountName;
                    Accountobj.CurrentStatusId = account.CurrentStatusId;
                    Accountobj.AccountRepId = account.AccountRepId;
                    Accountobj.AccountRepOutside = account.AccountRepOutside;
                    Accountobj.IsReferToLegal = account.IsReferToLegal;
                    Accountobj.AccountType = account.AccountType;
                    Accountobj.IsPartner = account.IsPartner;
                    Accountobj.Description = account.Description;
                    Accountobj.AccountImage = account.AccountImage;
                    Accountobj.Website = account.Website;
                    Accountobj.UserId = account.UserId;
                    Accountobj.Fax = account.Fax;
                    Accountobj.AnnualRevenueEst = account.AnnualRevenueEst;
                    Accountobj.TaxId = account.TaxId;
                    Accountobj.EmployeeCount = account.EmployeeCount;
                    Accountobj.NoofDevices = account.NoofDevices;
                    Accountobj.IndustryTypeId = account.IndustryTypeId;
                    Accountobj.CustomerSourceId = account.CustomerSourceId;
                    Accountobj.Activelines = account.Activelines;
                    Accountobj.Phone = account.Phone;
                    Accountobj.Email = account.Email;
                    Accountobj.LastConnect = account.LastConnect;
                    Accountobj.Lastpurcahse = account.Lastpurcahse;
                    Accountobj.NextContactDate = account.NextContactDate;
                    Accountobj.DateEntered = account.DateEntered;
                    Accountobj.MainPhone = account.MainPhone;
                    Accountobj.WebFormLink = account.WebFormLink;
                    Accountobj.AccountOwnerId = account.AccountOwnerId;
                    Accountobj.AccountNumber = account.AccountNumber;
                    Accountobj.Passcode = account.Passcode;
                    Accountobj.AboutAccount = account.AboutAccount;

                    //Accountobj.CarrierId = account.CarrierId;
                    //Accountobj.Carrier = account.ca;


                }

                return Accountobj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<CompanyViewModel> GetAccountInfoById(int accountId)
        {
            try
            {
                var Accountobj = new CompanyViewModel();
                if (accountId > 0)
                {
                    var account = await _accountRepository.GetAccountInfoById(accountId);
                    Accountobj.AccountId = account.AccountId;
                    Accountobj.AccountName = account.AccountName;
                    Accountobj.AccountStatusName = account.AccountStatusName;
                    Accountobj.AccountStatusId = account.AccountStatusId;
                    Accountobj.CurrentStatusId = account.CurrentStatusId;
                    Accountobj.AccountRepId = account.AccountRepId;
                    Accountobj.AccountRepOutside = account.AccountRepOutside;
                    Accountobj.IsReferToLegal = account.IsReferToLegal;
                    Accountobj.AccountType = account.AccountType;
                    Accountobj.IsPartner = account.IsPartner;
                    Accountobj.Description = account.Description;
                    Accountobj.AccountImage = account.AccountImage;
                    Accountobj.Website = account.Website;
                    Accountobj.UserId = account.UserId;
                    Accountobj.Fax = account.Fax;
                    Accountobj.AnnualRevenueEst = account.AnnualRevenueEst;
                    Accountobj.TaxId = account.TaxId;
                    Accountobj.EmployeeCount = account.EmployeeCount;
                    Accountobj.NoofDevices = account.NoofDevices;
                    Accountobj.IndustryTypeId = account.IndustryTypeId;
                    Accountobj.CustomerSourceId = account.CustomerSourceId;
                    Accountobj.Activelines = account.Activelines;
                    Accountobj.Phone = account.Phone;
                    Accountobj.Email = account.Email;
                    Accountobj.LastConnect = account.LastConnect;
                    Accountobj.Lastpurcahse = account.Lastpurcahse;
                    Accountobj.NextContactDate = account.NextContactDate;
                    Accountobj.DateEntered = account.DateEntered;
                    Accountobj.MainPhone = account.MainPhone;
                    Accountobj.WebFormLink = account.WebFormLink;
                    Accountobj.AccountOwnerId = account.AccountOwnerId;
                    Accountobj.CarrierId = account.CarrierId;
                    Accountobj.State = account.State;
                    Accountobj.City = account.City;
                    Accountobj.Street = account.Street;
                    Accountobj.Zip = account.Zip;
                    Accountobj.IndustryType = account.IndustryType;
                    Accountobj.AccountRep = account.AccountRep;
                    Accountobj.CustomerSource = account.CustomerSource;
                    Accountobj.Carrier = account.Carrier;
                    Accountobj.ContactName = account.ContactName;
                    Accountobj.AccountNumber = account.AccountNumber;
                    Accountobj.Passcode = account.Passcode;
                    Accountobj.Title = account.Title;
                    Accountobj.AboutAccount = account.AboutAccount;
                    Accountobj.Quality = account.Quality;
                    Accountobj.CarrierIds = await _accountRepository.GetAccountCarrierIdAsync(accountId);

                    //Accountobj.Carrier = account.ca;

                }

                ////get carriers based upon account it 
                //var carriers = _accountRepository.GetAccountCarrier(accountId);
                //Accountobj.CarrierIds = new List<int>();
                //foreach (var carrier in carriers)
                //{
                //    Accountobj.CarrierIds.Add(carrier.CarrierId);
                //}



                ////get addresses based upon accountId
                Accountobj.CompanyAddresses = new CompanyAddressViewModel();
                Accountobj.CompanyAddresses.CityList = Accountobj.CompanyAddresses.CityList;
                Accountobj.CompanyAddresses.StateList = Accountobj.CompanyAddresses.StateList;




                Accountobj.ShippingAddresses = new CompanyAddressViewModel();


                Accountobj.ShippingAddresses.CityList = Accountobj.ShippingAddresses.CityList;
                Accountobj.ShippingAddresses.StateList = Accountobj.ShippingAddresses.StateList;

                Accountobj.BillingAddresses = new CompanyAddressViewModel();


                Accountobj.BillingAddresses.StateList = Accountobj.BillingAddresses.StateList;
                Accountobj.BillingAddresses.CityList = Accountobj.BillingAddresses.CityList;
                var addresses = _accountRepository.GetAccountAddressesById(accountId);
                foreach (var address in addresses)
                {
                    if (address.AddressType == Convert.ToInt32(GlobalEnums.AddressType.Company))
                    {
                        Accountobj.CompanyAddresses.Street = address.Street;
                        Accountobj.CompanyAddresses.AccountAddressId = address.AccountAddressId;
                        Accountobj.CompanyAddresses.CityId = address.CityId;
                        Accountobj.CompanyAddresses.CityName = address.CityName;
                        Accountobj.CompanyAddresses.StateId = address.StateId;
                        Accountobj.CompanyAddresses.AreaCode = address.AreaCode;
                        Accountobj.CompanyAddresses.ZipPostalCode = address.ZipPostalCode;
                        Accountobj.CompanyAddresses.Country = address.Country;
                    }
                    if (address.AddressType == Convert.ToInt32(GlobalEnums.AddressType.Billing))
                    {
                        Accountobj.BillingAddresses.Street = address.Street;
                        Accountobj.BillingAddresses.AccountAddressIdBill = address.AccountAddressId;
                        Accountobj.BillingAddresses.CityId = address.CityId;
                        Accountobj.BillingAddresses.CityName = address.CityName;
                        Accountobj.BillingAddresses.StateId = address.StateId;
                        Accountobj.BillingAddresses.AreaCode = address.AreaCode;
                        Accountobj.BillingAddresses.ZipPostalCode = address.ZipPostalCode;
                        Accountobj.BillingAddresses.Country = address.Country;
                    }
                    if (address.AddressType == Convert.ToInt32(GlobalEnums.AddressType.Shipping))
                    {
                        Accountobj.ShippingAddresses.Street = address.Street;
                        Accountobj.ShippingAddresses.AccountAddressIdShip = address.AccountAddressId;
                        Accountobj.ShippingAddresses.CityId = address.CityId;
                        Accountobj.ShippingAddresses.CityName = address.CityName;
                        Accountobj.ShippingAddresses.StateId = address.StateId;
                        Accountobj.ShippingAddresses.AreaCode = address.AreaCode;
                        Accountobj.ShippingAddresses.ZipPostalCode = address.ZipPostalCode;
                        Accountobj.ShippingAddresses.Country = address.Country;
                    }
                }




                return Accountobj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<SelectListItem>> GetAccountStatusList()
        {
            try
            {
                List<SelectListItem> statusList = new List<SelectListItem>();
                statusList = await _accountRepository.GetAccountStatusList();
                return statusList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ServiceResult> DeleteAccount(int accountId)
        {
            try
            {
                if (accountId > 0)
                {
                    var account = await _accountRepository.GetAccountById(accountId);
                    if (account != null)
                    {
                        account.IsActive = false;
                        await _accountRepository.DeleteAccount(account);

                    }
                }
                return _serviceResult;
            }
            catch (Exception ex)
            {
                _onErrorServiceResult.Message = ex.Message;
                return _onErrorServiceResult;
            }
        }

        public async Task<ServiceResult> AssignAccount(int accountId, string radioButtonValue, string assignUserId)
        {
            try
            {
                if (accountId > 0)
                {
                    var account = await _accountRepository.GetAccountById(accountId);
                    if (account != null)
                    {
                        if (radioButtonValue == "Representative")
                        {
                            account.AccountRepId = assignUserId;
                        }
                        else if (radioButtonValue == "Owner")
                        {
                            account.AccountOwnerId = assignUserId;
                        }

                        await _accountRepository.SaveOrUpdateAccount(account);

                    }
                }
                return _serviceResult;
            }
            catch (Exception ex)
            {
                _onErrorServiceResult.Message = ex.Message;
                return _onErrorServiceResult;
            }
        }

        public async Task<ServiceResult> AddAccount(CompanyViewModel accountViewModel)
        {
            try
            {
                if (accountViewModel.AccountId > 0)
                {
                    return await UpdateAccount(accountViewModel);
                }
                else
                {
                    return await SaveAccount(accountViewModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ServiceResult> SaveAccount(CompanyViewModel accountViewModel)
        {
            try
            {
                var account = new tbl_Account
                {
                    AccountId = accountViewModel.AccountId,
                    AccountName = accountViewModel.AccountName,
                    CurrentStatusId = accountViewModel.CurrentStatusId,
                    AccountRepId = accountViewModel.AccountRepId,
                    AccountRepOutside = accountViewModel.AccountRepOutside,
                    IsReferToLegal = accountViewModel.IsReferToLegal,
                    AccountType = accountViewModel.AccountType,
                    IsPartner = accountViewModel.IsPartner,
                    Description = accountViewModel.Description,
                    AccountImage = accountViewModel.AccountImage,
                    Website = accountViewModel.Website,
                    UserId = accountViewModel.UserId,
                    Fax = accountViewModel.Fax,
                    AnnualRevenueEst = accountViewModel.AnnualRevenueEst,
                    TaxId = accountViewModel.TaxId,
                    EmployeeCount = accountViewModel.EmployeeCount,
                    NoofDevices = accountViewModel.NoofDevices,
                    IndustryTypeId = accountViewModel.IndustryTypeId,
                    CustomerSourceId = accountViewModel.CustomerSourceId,
                    Activelines = accountViewModel.Activelines,
                    Phone = accountViewModel.Phone,
                    Email = accountViewModel.Email,
                    LastConnect = DateTime.UtcNow,
                    Lastpurcahse = accountViewModel.Lastpurcahse,
                    NextContactDate = accountViewModel.NextContactDate,
                    DateEntered = accountViewModel.DateEntered,
                    MainPhone = accountViewModel.MainPhone,
                    WebFormLink = accountViewModel.WebFormLink,
                    AccountOwnerId = accountViewModel.AccountOwnerId,
                    AccountNumber = accountViewModel.AccountNumber,
                    Passcode = accountViewModel.Passcode,
                    IsActive = true,
                    IsDeleted = false,
                    AccountStatusId = accountViewModel.AccountStatusId,
                    CreatedDate = DateTime.UtcNow,
                    AboutAccount = accountViewModel.AboutAccount,


                };
                await _accountRepository.SaveOrUpdateAccount(account);

                //// insert update carrier 
                List<tbl_AccountCarrier> carriers = new List<tbl_AccountCarrier>();
                if (accountViewModel.CarrierIds != null)
                {
                    var index = 0;
                    foreach (var item in accountViewModel.CarrierIds)
                    {
                        tbl_AccountCarrier accountCarrier = new tbl_AccountCarrier
                        {
                            AccountId = account.AccountId,
                            CarrierId = item,
                            CreatedDate = DateTime.UtcNow,
                            IsActive = true,
                            IsDeleted = false,
                        };

                        if (index == 0)
                        {
                            accountCarrier.IsPrimery = true;
                        }
                        index++;
                        carriers.Add(accountCarrier);
                    }
                }

                bool isUpdated = false;
                if (accountViewModel.AccountId > 0)
                {
                    isUpdated = true;
                }
                await _accountRepository.SaveOrUpdateCarrier(carriers, isUpdated);

                ////add address AccountAddress where address type 1--> company 2-->billing 3-->shipping
                List<tbl_AccountAddress> accountAddressList = new List<tbl_AccountAddress>();
                tbl_AccountAddress accountAddress = null;

                ///company address
                accountAddress = new tbl_AccountAddress
                {
                    Street = accountViewModel.CompanyAddresses.Street,
                    CityName = accountViewModel.CompanyAddresses.CityName,
                    AccountId = account.AccountId,
                    StateId = accountViewModel.CompanyAddresses.StateId,
                    AreaCode = accountViewModel.CompanyAddresses.AreaCode,
                    ZipPostalCode = accountViewModel.CompanyAddresses.ZipPostalCode,
                    Country = accountViewModel.CompanyAddresses.Country,
                    AddressType = Convert.ToInt32(GlobalEnums.AddressType.Company),
                };
                accountAddressList.Add(accountAddress);

                //billing Address
                accountAddress = new tbl_AccountAddress
                {
                    Street = accountViewModel.BillingAddresses.Street,
                    CityName = accountViewModel.BillingAddresses.CityName,
                    AccountId = account.AccountId,
                    StateId = accountViewModel.BillingAddresses.StateId,
                    AreaCode = accountViewModel.BillingAddresses.AreaCode,
                    ZipPostalCode = accountViewModel.BillingAddresses.ZipPostalCode,
                    Country = accountViewModel.BillingAddresses.Country,
                    AddressType = Convert.ToInt32(GlobalEnums.AddressType.Billing),
                };
                accountAddressList.Add(accountAddress);

                ///shipping address
                accountAddress = new tbl_AccountAddress
                {
                    Street = accountViewModel.ShippingAddresses.Street,
                    CityName = accountViewModel.ShippingAddresses.CityName,
                    AccountId = account.AccountId,
                    StateId = accountViewModel.ShippingAddresses.StateId,
                    AreaCode = accountViewModel.ShippingAddresses.AreaCode,
                    ZipPostalCode = accountViewModel.ShippingAddresses.ZipPostalCode,
                    Country = accountViewModel.ShippingAddresses.Country,
                    AddressType = Convert.ToInt32(GlobalEnums.AddressType.Shipping),
                };
                accountAddressList.Add(accountAddress);

                await _accountRepository.SaveOrUpdateAddresses(accountAddressList, false);
                _serviceResult.Status = true;
                _serviceResult.Result = "Account Created Successfully!";
                return _serviceResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ServiceResult> UpdateAccount(CompanyViewModel accountViewModel)
        {
            try
            {
                var account = await _accountRepository.GetAccountById(accountViewModel.AccountId);
                account.AccountId = accountViewModel.AccountId;
                account.AccountName = accountViewModel.AccountName;
                account.CurrentStatusId = accountViewModel.CurrentStatusId;
                account.AccountRepId = accountViewModel.AccountRepId;
                account.AccountRepOutside = accountViewModel.AccountRepOutside;
                account.IsReferToLegal = accountViewModel.IsReferToLegal;
                account.AccountType = accountViewModel.AccountType;
                account.IsPartner = accountViewModel.IsPartner;
                account.Description = accountViewModel.Description;
                account.AccountImage = accountViewModel.AccountImage;
                account.Website = accountViewModel.Website;
                account.UserId = accountViewModel.UserId;
                account.Fax = accountViewModel.Fax;
                account.AnnualRevenueEst = accountViewModel.AnnualRevenueEst;
                account.TaxId = accountViewModel.TaxId;
                account.EmployeeCount = accountViewModel.EmployeeCount;
                account.NoofDevices = accountViewModel.NoofDevices;
                account.IndustryTypeId = accountViewModel.IndustryTypeId;
                account.CustomerSourceId = accountViewModel.CustomerSourceId;
                account.Activelines = accountViewModel.Activelines;
                account.Phone = accountViewModel.Phone;
                account.Email = accountViewModel.Email;
                account.LastConnect = DateTime.UtcNow;
                account.Lastpurcahse = accountViewModel.Lastpurcahse;
                account.NextContactDate = accountViewModel.NextContactDate;
                account.DateEntered = accountViewModel.DateEntered;
                account.MainPhone = accountViewModel.MainPhone;
                account.WebFormLink = accountViewModel.WebFormLink;
                account.AccountOwnerId = accountViewModel.AccountOwnerId;
                account.AccountStatusId = accountViewModel.AccountStatusId;
                account.ModifiedDate = DateTime.UtcNow;
                account.AccountNumber = accountViewModel.AccountNumber;
                account.Passcode = accountViewModel.Passcode;
                account.ModifiedBy = accountViewModel.UserId;
                account.AboutAccount = accountViewModel.AboutAccount;


                await _accountRepository.SaveOrUpdateAccount(account);
                await _accountRepository.DeActivateAllCarrier(account.AccountId);


                // insert update carrier 
                List<tbl_AccountCarrier> addCarriers = new List<tbl_AccountCarrier>();
                List<tbl_AccountCarrier> updateCarriers = new List<tbl_AccountCarrier>();
                if (accountViewModel != null && accountViewModel.CarrierIds != null)
                {
                    //var allcarrier = await _accountRepository.Getcarrierprimery(accountViewModel.AccountId).
                    foreach (var item in accountViewModel.CarrierIds)
                    {
                        var carrier = await _accountRepository.GetAccountCarrierById(accountViewModel.AccountId, item);
                        if (carrier != null)
                        {
                            carrier.IsActive = true;
                            updateCarriers.Add(carrier);
                        }
                        else
                        {
                            tbl_AccountCarrier accountCarrier = new tbl_AccountCarrier
                            {
                                AccountId = account.AccountId,
                                CarrierId = item,
                                CreatedDate = DateTime.UtcNow,
                                IsActive = true
                            };
                            addCarriers.Add(accountCarrier);
                        }
                    }
                }

                bool isUpdated = false;
                if (updateCarriers.Count > 0)
                {
                    #region Update Code For primary
                    var primaryreccount = updateCarriers.Where(w => w.IsActive == true && w.IsPrimery).Count();

                    if (primaryreccount <= 0)
                    {
                        var crindex = 0;

                        var notprrec = updateCarriers.Where(w => w.IsActive == true && !w.IsPrimery).ToList();

                        notprrec.ForEach(f =>
                        {
                            if (crindex == 0)
                            {
                                f.IsPrimery = true;
                                crindex++;
                            }
                        });
                    }
                    #endregion
                    isUpdated = true;
                    await _accountRepository.SaveOrUpdateCarrier(updateCarriers, isUpdated);
                }
                if (addCarriers.Count > 0)
                {

                    if (updateCarriers.Count <= 0)
                    {
                        var crindex = 0;
                        foreach (var item in addCarriers)
                        {
                            if (crindex == 0)
                            {
                                item.IsPrimery = true;
                                crindex++;
                            }
                        }
                    }

                    isUpdated = false;
                    await _accountRepository.SaveOrUpdateCarrier(addCarriers, isUpdated);
                }

                ////update address AccountAddress where address type 1--> company 2-->billing 3-->shipping
                accountViewModel.CompanyAddresses.AddressType = 1;
                
               var addresses = _accountRepository.GetAccountAddressesById(accountViewModel.AccountId);
                List<tbl_AccountAddress> accountAddressList = new List<tbl_AccountAddress>();
                if (accountViewModel.CompanyAddresses.AccountAddressId == 0)
                {
                    tbl_AccountAddress accountAddress = null;
                    accountAddress = new tbl_AccountAddress
                    {
                        Street = accountViewModel.CompanyAddresses.Street,
                        CityName = accountViewModel.CompanyAddresses.CityName,
                        AccountId = account.AccountId,
                        StateId = accountViewModel.CompanyAddresses.StateId,
                        AreaCode = accountViewModel.CompanyAddresses.AreaCode,
                        ZipPostalCode = accountViewModel.CompanyAddresses.ZipPostalCode,
                        Country = accountViewModel.CompanyAddresses.Country,
                        AddressType = 1
                    };
                    accountAddressList.Add(accountAddress);
                
                   
                }
                if (accountViewModel.BillingAddresses.AccountAddressIdBill == 0)
                {
                    tbl_AccountAddress accountAddress = null;
                    accountAddress = new tbl_AccountAddress
                    {
                        Street = accountViewModel.CompanyAddresses.Street,
                        CityName = accountViewModel.CompanyAddresses.CityName,
                        AccountId = account.AccountId,
                        StateId = accountViewModel.CompanyAddresses.StateId,
                        AreaCode = accountViewModel.CompanyAddresses.AreaCode,
                        ZipPostalCode = accountViewModel.CompanyAddresses.ZipPostalCode,
                        Country = accountViewModel.CompanyAddresses.Country,
                        AddressType = 2
                    };
                    accountAddressList.Add(accountAddress);
                }
                if (accountViewModel.ShippingAddresses.AccountAddressIdShip == 0)
                {
                    tbl_AccountAddress accountAddress = null;
                    accountAddress = new tbl_AccountAddress
                    {
                        Street = accountViewModel.CompanyAddresses.Street,
                        CityName = accountViewModel.CompanyAddresses.CityName,
                        AccountId = account.AccountId,
                        StateId = accountViewModel.CompanyAddresses.StateId,
                        AreaCode = accountViewModel.CompanyAddresses.AreaCode,
                        ZipPostalCode = accountViewModel.CompanyAddresses.ZipPostalCode,
                        Country = accountViewModel.CompanyAddresses.Country,
                        AddressType = 3
                    };
                    accountAddressList.Add(accountAddress);
                }
                foreach (var address in addresses)
                {
                    if (address.AddressType == Convert.ToInt32(GlobalEnums.AddressType.Company))
                    {
                        address.Street = accountViewModel.CompanyAddresses.Street;
                        address.CityName = accountViewModel.CompanyAddresses.CityName;
                        address.AccountId = account.AccountId;
                        address.StateId = accountViewModel.CompanyAddresses.StateId;
                        address.AreaCode = accountViewModel.CompanyAddresses.AreaCode;
                        address.ZipPostalCode = accountViewModel.CompanyAddresses.ZipPostalCode;
                        address.Country = accountViewModel.CompanyAddresses.Country;
                        accountAddressList.Add(address);
                    }
                    if (address.AddressType == Convert.ToInt32(GlobalEnums.AddressType.Billing))
                    {
                        address.Street = accountViewModel.BillingAddresses.Street;
                        address.CityName = accountViewModel.BillingAddresses.CityName;
                        address.AccountId = account.AccountId;
                        address.StateId = accountViewModel.BillingAddresses.StateId;
                        address.AreaCode = accountViewModel.BillingAddresses.AreaCode;
                        address.ZipPostalCode = accountViewModel.BillingAddresses.ZipPostalCode;
                        address.Country = accountViewModel.BillingAddresses.Country;
                        accountAddressList.Add(address);
                    }
                    if (address.AddressType == Convert.ToInt32(GlobalEnums.AddressType.Shipping))
                    {
                        address.Street = accountViewModel.ShippingAddresses.Street;
                        address.CityName = accountViewModel.ShippingAddresses.CityName;
                        address.AccountId = account.AccountId;
                        address.StateId = accountViewModel.ShippingAddresses.StateId;
                        address.AreaCode = accountViewModel.ShippingAddresses.AreaCode;
                        address.ZipPostalCode = accountViewModel.ShippingAddresses.ZipPostalCode;
                        address.Country = accountViewModel.ShippingAddresses.Country;
                        accountAddressList.Add(address);
                    }
                }
                await _accountRepository.SaveOrUpdateAddresses(accountAddressList, true);
                _serviceResult.Status = true;
                _serviceResult.Result = "Account Updated Successfully!";
                return _serviceResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<IEnumerable<CompanyViewModel>> GetAccounts(bool MyAccountOnly, string UserId, string Filters, string AdvanceSearchKey = null, string AdvanceSearchValue = null, DateTime? NextContactDate = null, int AccountStatusId = 0, int CarrierId = 0, string AccountRepId = null, bool accountWithSale = false)
        {
            List<CompanyViewModel> accounts = new List<CompanyViewModel>();

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", dbType: System.Data.DbType.String, value: UserId);
            parameters.Add("@NextContactDate", dbType: System.Data.DbType.DateTime, value: NextContactDate);
            parameters.Add("@AccountStatusId", dbType: System.Data.DbType.Int32, value: AccountStatusId);
            parameters.Add("@CarrierId", dbType: System.Data.DbType.Int32, value: CarrierId);
            parameters.Add("@Filters", dbType: System.Data.DbType.String, value: Filters);
            //parameters.Add("@AdvanceSearchField", dbType: System.Data.DbType.String, value: AdvanceSearchKey);
            parameters.Add("@@AdvanceSearch", dbType: System.Data.DbType.String, value: AdvanceSearchValue);
            parameters.Add("@AccountRepId", dbType: System.Data.DbType.String, value: AccountRepId);
            parameters.Add("@onlyMyAccounts", dbType: System.Data.DbType.Boolean, value: MyAccountOnly);
            var data = await _accountRepository.GetQuery<CompanyViewModel>("sp_GetMyAcountList", parameters, commandType: System.Data.CommandType.StoredProcedure).ConfigureAwait(false);

            foreach (var item in data)
            {
                var fname = _context.tbl_Contacts.Where(x => x.IsPrimary == true && x.AccountId == item.AccountId).Select(s => s.FirstName).FirstOrDefault();
                var lname = _context.tbl_Contacts.Where(x => x.IsPrimary == true && x.AccountId == item.AccountId).Select(s => s.LastName).FirstOrDefault();
                var firstname = fname + ' ' + lname;
                //item.ContactName = firstname != null ? firstname : string.Empty;

                if (accountWithSale == true)
                {
                    var order = _context.tbl_Order.Where(x => x.AccountId == item.AccountId && x.IsDeleted == false).FirstOrDefault();
                    if (order == null)
                        continue;
                }

                accounts.Add(new CompanyViewModel
                {
                    AccountId = item.AccountId,
                    AccountName = item.AccountName,
                    CurrentStatusId = item.CurrentStatusId,
                    AccountRepId = item.AccountRepId,
                    AccountRep = item.AccountRep,
                    IndustryType = item.IndustryType,
                    CustomerSource = item.CustomerSource,
                    AccountRepOutside = item.AccountRepOutside,
                    IsReferToLegal = item.IsReferToLegal,
                    AccountType = item.AccountType,
                    IsPartner = item.IsPartner,
                    Description = item.Description,
                    AccountImage = item.AccountImage,
                    Website = item.Website,
                    UserId = item.UserId,
                    Fax = item.Fax,
                    AnnualRevenueEst = item.AnnualRevenueEst,
                    TaxId = item.TaxId,
                    EmployeeCount = item.EmployeeCount,
                    NoofDevices = item.NoofDevices,
                    IndustryTypeId = item.IndustryTypeId,
                    CustomerSourceId = item.CustomerSourceId,
                    Activelines = item.Activelines,
                    Phone = item.Phone,
                    Email = item.Email,
                    LastConnect = item.LastConnect,
                    Lastpurcahse = item.Lastpurcahse,
                    NextContactDate = item.NextContactDate,
                    DateEntered = item.DateEntered,
                    MainPhone = item.MainPhone,
                    WebFormLink = item.WebFormLink,
                    AccountOwnerId = item.AccountOwnerId,
                    CarrierId = item.CarrierId,
                    Carrier = item.Carrier,
                    User = item.User,
                    ContactName = (firstname != null ? firstname : string.Empty),    //item.ContactName,
                    ModifiedDate = item.ModifiedDate,
                    //MyAccountTotalCount = Count,
                    //ALLAccountTotalCount = Count,
                    ModifiedBy = item.ModifiedBy,
                    AboutAccount = item.AboutAccount,
                    AccountIds = item.AccountIds,

                });

            }



            return accounts;
        }



        public async Task<int> LastModifiedAccountId(List<CompanyViewModel> accountList, string userId)
        {
            return accountList.Where(x => x.ModifiedDate != null && x.ModifiedBy == userId).OrderByDescending(x => x.ModifiedDate).Select(x => x.AccountId).FirstOrDefault();
        }
        public CompanyViewModel GetAccountInformation(int accountId)
        {
            try
            {
                var Accountobj = new CompanyViewModel();
                if (accountId > 0)
                {
                    var account = _accountRepository.GetAccountInformation(accountId);
                    Accountobj.AccountId = account.AccountId;
                    Accountobj.AccountName = account.AccountName;
                    Accountobj.CurrentStatusId = account.CurrentStatusId;
                    Accountobj.AccountRepId = account.AccountRepId;
                    Accountobj.AccountRepOutside = account.AccountRepOutside;
                    Accountobj.IsReferToLegal = account.IsReferToLegal;
                    Accountobj.AccountType = account.AccountType;
                    Accountobj.IsPartner = account.IsPartner;
                    Accountobj.Description = account.Description;
                    Accountobj.AccountImage = account.AccountImage;
                    Accountobj.Website = account.Website;
                    Accountobj.UserId = account.UserId;
                    Accountobj.Fax = account.Fax;
                    Accountobj.AnnualRevenueEst = account.AnnualRevenueEst;
                    Accountobj.TaxId = account.TaxId;
                    Accountobj.EmployeeCount = account.EmployeeCount;
                    Accountobj.NoofDevices = account.NoofDevices;
                    Accountobj.IndustryTypeId = account.IndustryTypeId;
                    Accountobj.CustomerSourceId = account.CustomerSourceId;
                    Accountobj.Activelines = account.Activelines;
                    Accountobj.Phone = account.Phone;
                    Accountobj.Email = account.Email;
                    Accountobj.LastConnect = account.LastConnect;
                    Accountobj.Lastpurcahse = account.Lastpurcahse;
                    Accountobj.NextContactDate = account.NextContactDate;
                    Accountobj.DateEntered = account.DateEntered;
                    Accountobj.MainPhone = account.MainPhone;
                    Accountobj.WebFormLink = account.WebFormLink;
                    Accountobj.AccountOwnerId = account.AccountOwnerId;
                    Accountobj.CarrierId = account.CarrierId;
                    Accountobj.ContactName = account.ContactName;
                    Accountobj.Status = account.Status;
                    Accountobj.ActivityName = account.ActivityName;
                }
                return Accountobj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //IAccount Rep
        public async Task<List<AccountRepViewmodel>> GetAccountRepList()
        {
            try
            {
                var accountRepList = new List<AccountRepViewmodel>();
                var accountRep = await _accountRepRepository.GetAccountRep();
                foreach (var item in accountRep)
                {
                    accountRepList.Add(new AccountRepViewmodel
                    {
                        AccountRepId = item.AccountRepId,
                        AccountRepName = item.AccountRepName


                    });
                }
                return accountRepList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<UserAccountRepViewmodel>> GetUserAccountRepList()
        {
            try
            {
                var accountRepList = new List<UserAccountRepViewmodel>();
                var accountRep = await _accountRepRepository.GetUserAccountRep();
                foreach (var item in accountRep)
                {
                    accountRepList.Add(new UserAccountRepViewmodel
                    {
                        AccountRepId = item.Id,
                        AccountRepName = item.UserName


                    });
                }
                return accountRepList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<AccountRepViewmodel> GetAccountRepById(int accountrepId)
        {
            try
            {
                var accountRepobj = new AccountRepViewmodel();
                if (accountrepId > 0)
                {
                    var accountRep = await _accountRepRepository.GetAccountRepById(accountrepId);
                    accountRepobj.AccountRepName = accountRep.AccountRepName;

                }
                return accountRepobj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Carrier



        public PagingList<DivisionViewModel> GetDivision(int accountId, int divisionId, DivisionViewModel divisionViewModel, int pageNo, string sortField, string sortOrder)
        {
            try
            {
                pageNo = pageNo == 0 ? 1 : pageNo;
                var Divisionlist = new List<DivisionViewModel>();
                var Division = _divisionRepository.GetDivision(accountId, divisionViewModel, pageNo, _pageSize, sortField, sortOrder);
                foreach (var item in Division)
                {
                    Divisionlist.Add(new DivisionViewModel
                    {
                        DivisionId = item.DivisionId,
                        AccountId = item.AccountId,
                        DivisionName = item.DivisionName,
                        ShipingAddress = item.ShipingAddress,
                        StatusId = item.StatusId,
                        CityName = item.City,
                        StateId = item.State,
                        ZipPostalCode = item.ZipCode
                    });
                }
                var list = PagingList<DivisionViewModel>.ConvertListAsync(Divisionlist.AsQueryable(), pageNo, Division.TotalPages, sortField, sortOrder);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<DivisionViewModel> GetDivisionById(int divisionId)
        {
            try
            {
                var divisionObj = new DivisionViewModel();
                if (divisionId > 0)
                {
                    var division = await _divisionRepository.GetDivisionById(divisionId);
                    divisionObj.DivisionId = division.DivisionId;
                    divisionObj.DivisionName = division.DivisionName;
                    divisionObj.AccountId = division.AccountId;
                    divisionObj.ShipingAddress = division.ShipingAddress;
                    divisionObj.StatusId = division.StatusId;
                    divisionObj.Street = division.Street;
                    divisionObj.CityName = division.City;
                    divisionObj.Country = division.Country;
                    divisionObj.StateId = division.State;
                    divisionObj.ZipPostalCode = division.ZipCode;
                    divisionObj.AreaCode = division.AreaCode;

                }
                return divisionObj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<AjaxResponseModel> AddDivision(DivisionViewModel divisionView)
        {
            try
            {
                if (divisionView.DivisionId > 0)
                {
                    return await UpdateDivision(divisionView);
                }
                else
                {
                    return await SaveDivision(divisionView);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<AjaxResponseModel> SaveDivision(DivisionViewModel divisionView)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                //var shippingAddress = string.Format("{0}{1}{2}{3}{4}{5}", divisionView.Street, divisionView.CityName, divisionView.StateName, divisionView.Country, divisionView.AreaCode, divisionView.ZipPostalCode);

                var DivisionObj = new tbl_Division
                {
                    DivisionId = divisionView.DivisionId,
                    DivisionName = divisionView.DivisionName,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false,
                    //ShipingAddress = shippingAddress,
                    AccountId = divisionView.AccountId,
                    StatusId = divisionView.StatusId,
                    State = divisionView.StateId,
                    Street = divisionView.Street,
                    City = divisionView.CityName,
                    Country = divisionView.Country,
                    AreaCode = divisionView.AreaCode,
                    ZipCode = divisionView.ZipPostalCode,
                };
                await _divisionRepository.SaveOrUpdateDivision(DivisionObj);
                var accounttoupdate = _accountRepository.GetAccountById(divisionView.AccountId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                response.Message = "Division Added Sucessfully";
                // return _serviceResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<AjaxResponseModel> UpdateDivision(DivisionViewModel divisionView)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                var division = await _divisionRepository.GetDivisionById(divisionView.DivisionId);
                division.AccountId = divisionView.AccountId;
                division.DivisionName = divisionView.DivisionName;
                division.ShipingAddress = divisionView.ShipingAddress;
                division.StatusId = divisionView.StatusId;
                division.State = divisionView.StateId;
                division.Street = divisionView.Street;
                division.City = divisionView.CityName;
                division.Country = divisionView.Country;
                division.AreaCode = divisionView.AreaCode;
                division.ZipCode = divisionView.ZipPostalCode;
                division.ModifiedDate = DateTime.Now;

                await _divisionRepository.SaveOrUpdateDivision(division);
                var accounttoupdate = _accountRepository.GetAccountById(divisionView.AccountId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                response.Message = "Division Updated Sucessfully";
                //return _serviceResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<ServiceResult> DeleteDivision(int divisionId)
        {
            try
            {
                if (divisionId > 0)
                {
                    var division = await _divisionRepository.GetDivisionById(divisionId);
                    if (division != null)
                    {
                        await _divisionRepository.DeleteDivision(division);

                    }
                }
                return _serviceResult;
            }
            catch (Exception ex)
            {
                _onErrorServiceResult.Message = ex.Message;
                return _onErrorServiceResult;
            }
        }

        //End Carrier

        //***************************************************************Account Notes *****************************************************************************************************//

        public async Task<List<NotesViewModel>> GetNotesList(int accountId, NotesViewModel noteViewModel, int noteType)
        {
            try
            {
                var Notelist = new List<NotesViewModel>();
                var Note = await _accountNotesRepository.GetNotes(accountId, noteViewModel, noteType);
                foreach (var item in Note)
                {
                    Notelist.Add(new NotesViewModel
                    {
                        NoteID = item.NoteID,
                        NoteSource = item.NoteSource,
                        SourceId = item.SourceId,
                        Notedescription = item.Notedescription,
                        Notetitle = item.Notetitle,
                        Time = item.Time,
                        CreatedDate = item.CreatedDate,
                        CreatedBy = item.CreatedBy,
                        IsImportant = Convert.ToBoolean(item.IsImportant)
                    });
                }

                return Notelist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public async Task<List<NotesViewModel>> GetActivityNotesList(int ActivitySourceId, NotesViewModel noteViewModel, string NoteSource)
        //{
        //    try
        //    {
        //        var Notelist = new List<NotesViewModel>();
        //        var Note = await _accountNotesRepository.GetActivityNotesList(ActivitySourceId, noteViewModel, NoteSource);
        //        foreach (var item in Note)
        //        {
        //            Notelist.Add(new NotesViewModel
        //            {
        //                NoteID = item.NoteID,
        //                NoteSource = item.NoteSource,
        //                SourceId = item.SourceId,
        //                Notedescription = item.Notedescription,
        //                Notetitle = item.Notetitle,
        //                Time = item.Time,
        //                CreatedDate = item.CreatedDate,
        //                CreatedBy = item.CreatedBy
        //            });
        //        }

        //        return Notelist;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public async Task<NotesViewModel> GetNoteById(int noteId)
        {
            try
            {
                var carrierObj = new NotesViewModel();
                if (noteId > 0)
                {
                    var carrier = await _accountNotesRepository.GetNoteById(noteId);
                    carrierObj.NoteID = carrier.NoteID;
                    carrierObj.NoteSource = carrier.NoteSource;
                    carrierObj.SourceId = carrier.SourceId;
                    carrierObj.Notedescription = carrier.Notedescription;
                    carrierObj.Notetitle = carrier.Notetitle;
                    carrierObj.IsImportant = (bool)carrier.IsImportant;
                    carrierObj.Time = carrier.Time;
                    carrierObj.CreatedDate = carrier.CreatedDate;
                    carrierObj.CreatedBy = carrier.CreatedBy;
                }
                return carrierObj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AjaxResponseModel> AddNotes(NotesViewModel noteViewModel)
        {
            try
            {
                if (noteViewModel.NoteID > 0)
                {
                    return await UpdateNote(noteViewModel);
                }
                else
                {
                    return await SaveNote(noteViewModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<AjaxResponseModel> SaveNote(NotesViewModel noteViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                var NoteObj = new tbl_Notes
                {
                    NoteSource = noteViewModel.NoteSource,
                    SourceId = noteViewModel.SourceId,
                    Notetitle = noteViewModel.Notetitle,
                    Notedescription = noteViewModel.Notedescription,
                    CreatedBy = noteViewModel.CreatedBy,
                    Time = DateTime.Now.TimeOfDay,
                    IsDeleted = false,
                    IsAccountMemo = noteViewModel.IsAccountMemo,
                    IsUrgent = noteViewModel.IsUrgent,
                    IsImportant = noteViewModel.IsImportant,
                    CreatedDate = DateTime.Now,
                };
                await _accountNotesRepository.SaveOrUpdateNote(NoteObj);
                if (noteViewModel.NoteSource == "Account")
                {
                    var accounttoupdate = _accountRepository.GetAccountById(noteViewModel.AccountId).Result;
                    accounttoupdate.LastConnect = DateTime.UtcNow;
                    await _accountRepository.UpdateAsync(accounttoupdate);
                }
                response.Message = "Notes Added Sucessfully";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<AjaxResponseModel> UpdateNote(NotesViewModel noteViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                var carrier = await _accountNotesRepository.GetNoteById(noteViewModel.NoteID);

                carrier.NoteSource = noteViewModel.NoteSource;
                carrier.SourceId = noteViewModel.SourceId;
                carrier.Notetitle = noteViewModel.Notetitle;
                carrier.Notedescription = noteViewModel.Notedescription;
                carrier.ModifiedDate = DateTime.Now;
                carrier.IsDeleted = false;
                carrier.IsAccountMemo = noteViewModel.IsAccountMemo;
                carrier.IsUrgent = noteViewModel.IsUrgent;
                carrier.IsImportant = noteViewModel.IsImportant;

                await _accountNotesRepository.SaveOrUpdateNote(carrier);
                var accounttoupdate = _accountRepository.GetAccountById(noteViewModel.AccountId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                response.Message = "Notes Added Sucessfully";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<ServiceResult> DeleteNote(int NoteID)
        {
            try
            {
                if (NoteID > 0)
                {
                    var note = await _accountNotesRepository.GetNoteById(NoteID);
                    if (note != null)
                    {
                        await _accountNotesRepository.DeleteNotes(note);

                    }
                }
                return _serviceResult;
            }
            catch (Exception ex)
            {
                _onErrorServiceResult.Message = ex.Message;
                return _onErrorServiceResult;
            }
        }

        public async Task<List<SelectListItem>> GetAllSource(ActivitySourceEnum? activitySourceEnum)
        {
            try
            {
                var accounts = await _accountNotesRepository.GetAllSource(activitySourceEnum);

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<SelectListItem>> GetAllAccounts()
        {
            try
            {
                //var accounts = await _accountNotesRepository.GetAllAccounts();
                var accounts = (await _keyValueRepository.GetKeyValue<tbl_Account, SelectListItem>(x => x.IsDeleted != true, (x => new SelectListItem { Text = x.AccountName, value = x.AccountId.ToString() })).ConfigureAwait(false)).ToList();

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void UserAccountAssign(UserAccountAssignViewModel userAccountList)
        {
            _accountNotesRepository.UserAccountAssign(userAccountList);

            _accountNotesRepository.SaveAllAccountAssign(userAccountList.UserId);
        }

        public async Task<List<SelectListItem>> GetUserAccountAssign(string userId)
        {
            return await _accountNotesRepository.GetUserAccountAssign(userId);
        }

        public async Task<List<UnAssignedListViewModel>> GetFilteredUserAccount(int isAssignedAccountsOnly, int accountStatusId, string accountRepId, string UserId, string CurrentUserId)
        {
            try
            {
                var p = new DynamicParameters();
                p.Add("@IsUnassignedOnly", dbType: DbType.Int32, value: isAssignedAccountsOnly);
                p.Add("@AccountStatusId", dbType: DbType.Int32, value: accountStatusId);
                p.Add("@AccountRepId", dbType: DbType.String, value: accountRepId);
                p.Add("@UserID", dbType: DbType.String, value: UserId);
                p.Add("@CurrentUserId", dbType: DbType.String, value: CurrentUserId);
                var result = (List<UnAssignedListViewModel>)await _accountRepository.GetQuery<UnAssignedListViewModel>("usp_GetAllUnassignedAccounts", p, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<SelectListItem>> GetAllContacts()
        {
            try
            {
                var accounts = await _accountNotesRepository.GetAllContacts();

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<List<SelectListItem>> GetAllContactsByAccount(int accountId)
        {
            try
            {
                var accounts = await _accountNotesRepository.GetAllContactsByAccount(accountId);

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<List<SelectListItem>> GetAllContactsByAccounts(int accountId)
        {
            try
            {
                var accounts = await _accountNotesRepository.GetAllContactsByAccounts(accountId);

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<SelectListItem>> GetActivityType()
        {
            try
            {
                var accounts = await _accountNotesRepository.GetActivityType();

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<SelectListItem>> GetStatus()
        {
            try
            {
                var accounts = await _accountNotesRepository.GetStatus();

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }

        //*************************************************************** End Account Notes *****************************************************************************************************//


        //***************************************************************  Account Carrier *****************************************************************************************************//

        public async Task<List<AccountCarrierViewModel>> GetAccountCarrierList(int accountId)
        {
            try
            {
                var AccountList = new List<AccountCarrierViewModel>();
                var AccountRepoList = await _accountCarrierRepository.GetAcountCarrierList(accountId);
                foreach (var item in AccountRepoList)
                {
                    AccountList.Add(new AccountCarrierViewModel
                    {
                        AccountCarrierId = item.AccountCarrierId,
                        AccountId = item.AccountId,
                        CarrierId = item.CarrierId,
                        CarrierTittle = item.CarrierTittle,
                        Autopay = item.Autopay,
                        BillingCycle = item.BillingCycle,
                        PaperLessBilling = item.PaperLessBilling,
                        OnlineBillingLoginDetails = item.OnlineBillingLoginDetails,
                        Discount = item.Discount,
                        IsPrimery = item.IsPrimery
                    });


                }
                return AccountList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<AccountCarrierViewModel> GetAccountCarrierById(int accountCarrierId)
        {
            try
            {
                var Accountobj = new AccountCarrierViewModel();
                if (accountCarrierId > 0)
                {
                    var account = await _accountCarrierRepository.GetAccountCarrierById(accountCarrierId);
                    Accountobj.AccountId = account.AccountId;
                    Accountobj.AccountCarrierId = account.AccountCarrierId;


                    Accountobj.Autopay = account.Autopay;
                    Accountobj.BillingCycle = account.BillingCycle;
                    Accountobj.PaperLessBilling = account.PaperLessBilling;
                    Accountobj.OnlineBillingLoginDetails = account.OnlineBillingLoginDetails;
                    Accountobj.Discount = account.Discount;
                    Accountobj.CarrierId = account.CarrierId;
                    Accountobj.IsPrimery = account.IsPrimery;
                }


                return Accountobj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ServiceResult> DeleteAccountCarrier(int accountCarrierId)
        {
            try
            {
                if (accountCarrierId > 0)
                {
                    var account = await _accountCarrierRepository.GetAccountCarrierById(accountCarrierId);
                    if (account != null)
                    {
                        await _accountCarrierRepository.DeleteAccountCarrier(account);

                    }
                }
                return _serviceResult;
            }
            catch (Exception ex)
            {
                _onErrorServiceResult.Message = ex.Message;
                return _onErrorServiceResult;
            }
        }

        public async Task<AjaxResponseModel> AddAccountCarrier(AccountCarrierViewModel accountCarrierViewModel)
        {
            try
            {
                if (accountCarrierViewModel.AccountCarrierId > 0)
                {
                    return await UpdateAccountCarrier(accountCarrierViewModel);
                }
                else
                {
                    return await SaveAccountCarrier(accountCarrierViewModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AjaxResponseModel> SaveAccountCarrier(AccountCarrierViewModel accountCarrierViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                if (accountCarrierViewModel.IsPrimery)
                {
                    _accountCarrierRepository.UpdatePrimery(accountCarrierViewModel.AccountId);
                }

                var check = _accountCarrierRepository.GetAccountCarrierByAccountId(accountCarrierViewModel.AccountId, accountCarrierViewModel.CarrierId);
                if (check == true)
                {
                    response.flag = false;
                    response.Message = "Carrier already Exist";
                }
                else
                {
                    var account = new tbl_AccountCarrier
                    {
                        AccountId = accountCarrierViewModel.AccountId,
                        CarrierId = accountCarrierViewModel.CarrierId,
                        AccountCarrierId = accountCarrierViewModel.AccountCarrierId,
                        Autopay = accountCarrierViewModel.Autopay,
                        BillingCycle = (int)accountCarrierViewModel.BillingCycle,
                        PaperLessBilling = accountCarrierViewModel.PaperLessBilling,
                        OnlineBillingLoginDetails = accountCarrierViewModel.OnlineBillingLoginDetails,
                        Discount = accountCarrierViewModel.Discount,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now,
                        IsPrimery = accountCarrierViewModel.IsPrimery
                    };

                    await _accountCarrierRepository.SaveOrUpdateAccountCarrier(account);

                    response.Message = "Carrier saved sucessfully";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<AjaxResponseModel> UpdateAccountCarrier(AccountCarrierViewModel accountCrrierViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                if (accountCrrierViewModel.IsPrimery)
                {
                    _accountCarrierRepository.UpdatePrimery(accountCrrierViewModel.AccountId);
                }
                var account = await _accountCarrierRepository.GetAccountCarrierById(accountCrrierViewModel.AccountCarrierId);
                account.AccountId = accountCrrierViewModel.AccountId;
                account.CarrierId = accountCrrierViewModel.CarrierId;
                account.AccountCarrierId = accountCrrierViewModel.AccountCarrierId;
                account.Autopay = accountCrrierViewModel.Autopay;
                account.BillingCycle = (int)accountCrrierViewModel.BillingCycle;
                account.PaperLessBilling = accountCrrierViewModel.PaperLessBilling;
                account.OnlineBillingLoginDetails = accountCrrierViewModel.OnlineBillingLoginDetails;
                account.Discount = accountCrrierViewModel.Discount;
                account.ModifiedDate = DateTime.Now;
                account.IsPrimery = accountCrrierViewModel.IsPrimery;
                //account.ModifiedBy = accountViewModel.UserId;


                await _accountCarrierRepository.SaveOrUpdateAccountCarrier(account);
                var accounttoupdate = _accountRepository.GetAccountById(accountCrrierViewModel.AccountId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                response.Message = "Carrier Updated Sucessfully";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        //*************************************************************** End Account Carrier *****************************************************************************************************//

        //***************************************************************  Account Address *****************************************************************************************************//

        public async Task<List<CompanyAddressViewModel>> GetAccountAddressList(int accountId)
        {
            try
            {
                var AccountAddress = new List<CompanyAddressViewModel>();
                var AccountAddressList = await _accountAddress.GetAcountAddressList(accountId);
                foreach (var item in AccountAddressList)
                {
                    AccountAddress.Add(new CompanyAddressViewModel
                    {
                        AccountAddressId = item.AccountAddressId,
                        AccountId = item.AccountId,
                        Street = item.Street,
                        StateId = item.StateId,
                        CityId = item.CityId,
                        Country = item.Country,
                        AddressType = item.AddressType,
                        AreaCode = item.AreaCode,
                        StateName = item.StateName,
                        CityName = item.CityName,
                        ZipPostalCode = item.ZipPostalCode,
                        IsDefault = item.IsDefault,
                        ISSameAsShiping = item.ISSameAsShiping

                    });


                }
                return AccountAddress;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<AccountCarrierViewModel>> GetAccountCarrier(int accountId)
        {
            try
            {
                var AccountCarrier = new List<AccountCarrierViewModel>();
                var AccountCarrierList = await _accountAddress.GetAccountCarrier(accountId);
                foreach (var item in AccountCarrierList)
                {
                    AccountCarrier.Add(new AccountCarrierViewModel
                    {
                        IsPrimery = item.IsPrimery,
                        CarrierTittle = item.CarrierTittle,
                        CarrierId = item.CarrierId
                    });

                }
                return AccountCarrier;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public async Task<List<CompanyViewModel>> GetAccountDetail(int accountId)
        {
            try
            {
                var AccountCarrier = new List<CompanyViewModel>();
                var AccountCarrierList = await _accountAddress.GetAccountDetail(accountId);

                return AccountCarrierList;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<CompanyAddressViewModel> GetAccountAddressById(int accountAddressId)
        {
            try
            {
                var Accountobj = new CompanyAddressViewModel();
                if (accountAddressId > 0)
                {
                    var account = await _accountAddress.GetAccountAddressById(accountAddressId);
                    Accountobj.AccountId = account.AccountId;
                    Accountobj.AccountAddressId = account.AccountAddressId;


                    Accountobj.CityId = account.CityId;
                    Accountobj.CityName = account.CityName;
                    Accountobj.Street = account.Street;
                    Accountobj.StateId = account.StateId;
                    Accountobj.Country = account.Country;
                    Accountobj.AddressType = account.AddressType;
                    Accountobj.AreaCode = account.AreaCode;
                    Accountobj.ZipPostalCode = account.ZipPostalCode;
                    Accountobj.IsDefault = account.IsDefault;
                    // Accountobj.CityName = account.cit;
                    // Accountobj.ShippingAddresses = new CompanyAddressViewModel();
                    Accountobj.CityList = Accountobj.CityList;
                    Accountobj.StateList = Accountobj.StateList;
                    Accountobj.ISSameAsShiping = account.ISSameAsShiping;
                    //Accountobj.BillingAddresses = new CompanyAddressViewModel();
                }


                return Accountobj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ServiceResult> DeleteAccountAddress(int accountAddressId)
        {
            try
            {
                if (accountAddressId > 0)
                {
                    var account = await _accountAddress.GetAccountAddressById(accountAddressId);
                    if (account != null)
                    {
                        await _accountAddress.DeleteAccountAddress(account);

                    }
                }
                return _serviceResult;
            }
            catch (Exception ex)
            {
                _onErrorServiceResult.Message = ex.Message;
                return _onErrorServiceResult;
            }
        }
        //public async Task<ServiceResult> Deletedivisionid(int divisionid)
        //{
        //    try
        //    {
        //        if (divisionid > 0)
        //        {
        //            var account = await _accountAddress.GetDeletedivisionBYid(divisionid);
        //            if (account != null)
        //            {
        //                await _accountAddress.DeleteAccountAddress(account);

        //            }
        //        }
        //        return _serviceResult;
        //    }
        //    catch (Exception ex)
        //    {
        //        _onErrorServiceResult.Message = ex.Message;
        //        return _onErrorServiceResult;
        //    }
        //}

        public async Task<AjaxResponseModel> AddAccountAddress(CompanyAddressViewModel accountAddressViewModel)
        {
            try
            {
                if (accountAddressViewModel.AccountAddressId > 0)
                {
                    return await UpdateAccountAddress(accountAddressViewModel);
                }
                else
                {
                    return await SaveAccountAddress(accountAddressViewModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AjaxResponseModel> SaveAccountAddress(CompanyAddressViewModel accountAddressViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                if (accountAddressViewModel.IsDefault)
                {
                    _accountCarrierRepository.UpdateDefaultAddress(accountAddressViewModel.AccountId, accountAddressViewModel.AddressType);
                }
                var account = new tbl_AccountAddress
                {
                    AccountId = accountAddressViewModel.AccountId,
                    AccountAddressId = accountAddressViewModel.AccountAddressId,
                    Street = accountAddressViewModel.Street,
                    CityName = accountAddressViewModel.CityName,
                    StateId = accountAddressViewModel.StateId,
                    Country = accountAddressViewModel.Country,
                    AreaCode = accountAddressViewModel.AreaCode,
                    ISSameAsShiping = accountAddressViewModel.ISSameAsShiping,
                    ZipPostalCode = accountAddressViewModel.ZipPostalCode,
                    IsDefault = accountAddressViewModel.IsDefault,
                    AddressType = accountAddressViewModel.AddressType,
                    IsDeleted = false,
                    //CreatedDate = DateTime.Now
                };

                await _accountAddress.SaveOrUpdateAccountAddress(account);
                var accounttoupdate = _accountRepository.GetAccountById(accountAddressViewModel.AccountId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                response.Message = "Address Save Sucessfully";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<AjaxResponseModel> UpdateAccountAddress(CompanyAddressViewModel accountAddressViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                if (accountAddressViewModel.IsDefault)
                {
                    _accountCarrierRepository.UpdateDefaultAddress(accountAddressViewModel.AccountId, accountAddressViewModel.AddressType);
                }
                var account = await _accountAddress.GetAccountAddressById(accountAddressViewModel.AccountAddressId);
                account.AccountId = accountAddressViewModel.AccountId;
                account.AccountAddressId = accountAddressViewModel.AccountAddressId;
                account.Street = accountAddressViewModel.Street;
                account.CityName = accountAddressViewModel.CityName;
                account.StateId = accountAddressViewModel.StateId;
                account.Country = accountAddressViewModel.Country;
                account.AreaCode = accountAddressViewModel.AreaCode;
                account.AreaCode = accountAddressViewModel.AreaCode;
                account.ISSameAsShiping = accountAddressViewModel.ISSameAsShiping;
                account.IsDefault = accountAddressViewModel.IsDefault;
                account.AddressType = accountAddressViewModel.AddressType;
                //account.ModifiedDate = DateTime.Now;
                //account.ModifiedBy = accountAddressViewModel.UserId;


                await _accountAddress.SaveOrUpdateAccountAddress(account);
                var accounttoupdate = _accountRepository.GetAccountById(accountAddressViewModel.AccountId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                response.Message = "Address Updated Sucessfully";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        //*************************************************************** End Account Address *****************************************************************************************************//

        public async Task<ServiceResult> AddActivity(ActivityViewModel activityViewModel)
        {
            try
            {
                if (activityViewModel.ActivityId > 0)
                {
                    return await UpdateActivity(activityViewModel);
                }
                else
                {
                    return await SaveActivity(activityViewModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ServiceResult> ActivityCancel(ActivityCancelViewModel activityViewModel)
        {
            try
            {
                return await CancelActivity(activityViewModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<ServiceResult> SaveActivity(ActivityViewModel activityViewModel)
        {
            try
            {
                var ActivityObj = new tbl_Activity
                {
                    SourceId = activityViewModel.SourceId.GetValueOrDefault(),
                    StatusId = activityViewModel.StatusId.GetValueOrDefault(),
                    StartDate = activityViewModel.StartDate,
                    StartTime = activityViewModel.StartTime,
                    EndDate = activityViewModel.EndDate,
                    EndTime = activityViewModel.EndTime,
                    ActivitySourceId = int.Parse(activityViewModel.SourceName),
                    ActivityName = activityViewModel.ActivityName,
                    ActivityTriggerId = activityViewModel.ActivityTriggerId.GetValueOrDefault(),
                    IsDeleted = false,
                    CreatedBy = activityViewModel.CreatedBy,
                    CreatedDate = DateTime.Now,
                    ActivityTypeId = activityViewModel.ActivityTypeId.GetValueOrDefault(),
                    UserId = activityViewModel.UserId,
                    Description = activityViewModel.Description,
                };
                await _accountNotesRepository.SaveOrUpdateActivity(ActivityObj);
                if (activityViewModel.SourceId == 3)
                {
                    var accounttoupdate = _accountRepository.GetAccountById(ActivityObj.ActivitySourceId).Result;
                    accounttoupdate.LastConnect = DateTime.UtcNow;
                    await _accountRepository.UpdateAsync(accounttoupdate);
                }
                return _serviceResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ServiceResult> UpdateActivity(ActivityViewModel activityViewModel)
        {
            try
            {
                var activity = await _accountNotesRepository.GetActivityById(activityViewModel.ActivityId);

                activity.SourceId = activityViewModel.SourceId.GetValueOrDefault();
                activity.StatusId = activityViewModel.StatusId.GetValueOrDefault();
                //activity.ContactId = activityViewModel.ContactId.GetValueOrDefault();
                activity.StartDate = activityViewModel.StartDate;
                activity.StartTime = activityViewModel.StartTime;
                activity.EndDate = activityViewModel.EndDate;
                activity.EndTime = activityViewModel.EndTime;
                activity.ActivitySourceId = int.Parse(activityViewModel.SourceName);
                activity.ActivityName = activityViewModel.ActivityName;
                activity.ModifiedDate = DateTime.Now;
                activity.ModifiedBy = activityViewModel.ModifiedBy;
                activity.IsDeleted = false;
                activity.UserId = activityViewModel.UserId;
                activity.Description = activityViewModel.Description;

                await _accountNotesRepository.SaveOrUpdateActivity(activity);
                var accounttoupdate = _accountRepository.GetAccountById(activity.ActivitySourceId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                return _serviceResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<ServiceResult> CancelActivity(ActivityCancelViewModel activityViewModel)
        {
            try
            {
                var activity = await _accountNotesRepository.GetcancelActivityById(activityViewModel.activityid, activityViewModel.sourceid);

                activity.IsCancel = true;
                activity.CancelReason = activityViewModel.cencelreson;
                await _accountNotesRepository.SaveOrUpdateActivity(activity);
                return _serviceResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ActivityViewModel> GetActivityById(int activityId)
        {
            try
            {
                var activityObj = new ActivityViewModel();
                if (activityId > 0)
                {
                    var activity = await _accountNotesRepository.GetActivityById(activityId);
                    activityObj.ActivityId = activity.ActivityId;
                    activityObj.ActivityName = activity.ActivityName;
                    activityObj.SourceId = activity.SourceId;
                    activityObj.ActivityTriggerId = activity.ActivityTriggerId;
                    activityObj.ActivityTypeId = activity.ActivityTypeId;
                    activityObj.ActivitySourceId = activity.ActivitySourceId;
                    activityObj.DeletedDate = activity.DeletedDate;
                    activityObj.EndDate = activity.EndDate;
                    activityObj.EndTime = activity.EndTime;
                    activityObj.IsDeleted = activity.IsDeleted;
                    activityObj.ModifiedBy = activity.ModifiedBy;
                    activityObj.ModifiedDate = activity.ModifiedDate;
                    activityObj.StartDate = activity.StartDate;
                    activityObj.StartTime = activity.StartTime;
                    activityObj.StatusId = activity.StatusId;
                    activityObj.UserId = activity.UserId;
                    activityObj.Description = activity.Description;
                    activityObj.CreatedDate = activity.CreatedDate;
                    activityObj.CreatedBy = activity.CreatedBy;
                }
                return activityObj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<ActivityListByDate>> GetActivityList(SearchActivityListViewModel searchActivity)
        {
            try
            {
                var Activitylist = new List<ActivityListByDate>();
                var activities = await _accountNotesRepository.GetActivity(searchActivity.Accounttypeid, searchActivity.SearchStartdate, searchActivity.SearchEnddate, searchActivity.activityStatus, searchActivity.activityTrigger, searchActivity.userId, searchActivity.IsAdmin, searchActivity.IsMissed);

                if (activities == null)
                    return Activitylist;

                var typelist = activities?.Select(s => new
                {
                    StartDate = s.StartDate
                }).Distinct().ToList();

                if (typelist == null)
                    return Activitylist;

                foreach (var item in typelist)
                {
                    var Activity = new ActivityListByDate();
                    var activitylist = new List<ActivityListByTypeViewModel>();

                    Activity.StartDate = item.StartDate;

                    var accgroupbydate = activities.Where(w => w.StartDate == item.StartDate)
                                        .Select(s => new
                                        {
                                            activitytype = s.ActivityTypeName.Trim(),
                                            activitytypeid = s.ActivityTypeId,
                                            ActivityTypeEnum = s.ActivityTypeEnum
                                        })
                                        .Distinct().ToList();

                    foreach (var titem in accgroupbydate)
                    {
                        var acdata = new ActivityListByTypeViewModel();

                        acdata.ActivityType = titem.activitytype;
                        acdata.ActivityTypeId = titem.activitytypeid;
                        acdata.ActivityTypeEnum = titem.ActivityTypeEnum;

                        acdata.Activities = activities.Where(d => d.StartDate == item.StartDate &&
                                                                d.ActivityTypeId == titem.activitytypeid)
                            .Select(item => new ActivityViewModel
                            {
                                ActivityId = item.ActivityId,
                                ActivityTypeId = item.ActivityTypeId,
                                ActivityTypeEnum = item.ActivityTypeEnum,
                                StartDate = item.StartDate,
                                StartTime = item.StartTime,
                                SourceName = item.SourceName,
                                ActivityName = item.ActivityName,
                                SourceId = item.SourceId,
                                ActivityTypeName = item.ActivityTypeName,
                                StatusName = item.StatusName,
                                EndDate = item.EndDate,
                                EndTime = item.EndTime,
                                UserId = item.UserId,
                                UserName = item.UserName,
                                Description = item.Description
                            }).ToList();

                        activitylist.Add(acdata);
                    }

                    Activity.ActivitiesData = activitylist;//.OrderByDescending(o => o.StartDate).ToList();

                    Activitylist.Add(Activity);
                }

                return Activitylist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<EventActivityListViewModel>> GetEventActivityList(string userId, bool IsAdmin)
        {
            var neweventlist = new List<EventActivityListViewModel>();
            try
            {

                var EventActivities = await _accountNotesRepository.GetActivity(null, null, null, null, null, userId, IsAdmin);

                neweventlist = (from a in EventActivities
                                let startdate = new DateTime(a.StartDate.Year, a.StartDate.Month, a.StartDate.Day)
                                let enddate = new DateTime(a.EndDate.Year, a.EndDate.Month, a.EndDate.Day)
                                select new EventActivityListViewModel
                                {
                                    activityid = a.ActivityId,
                                    appointment = string.Format("{0} - {1} - {2}{3}", a.SourceName, a.ActivityTypeName, a.ActivityName, ((a.ActivityTriggerId.HasValue && a.ActivityTriggerId.Value > 0) ? (" - " + a.ActivityTriggerId.ToString()) : string.Empty)),
                                    title = a.ActivityName,
                                    description = a.ActivityName,
                                    startdate = startdate + a.StartTime,
                                    enddate = enddate + a.EndTime,
                                    backgroundColor = (a.SourceEnum == ActivitySourceEnum.Account ? "#3888AC"
                                                        : ((a.SourceEnum == ActivitySourceEnum.Lead) ? "#5F8575" :
                                                            ((a.SourceEnum == ActivitySourceEnum.Opportunities) ? "#ff7f50" : "#808080"))),
                                    textColor = (a.SourceEnum == ActivitySourceEnum.Account ? "#FFFFFF"
                                                        : ((a.SourceEnum == ActivitySourceEnum.Lead) ? "#FFFFFF" :
                                                            ((a.SourceEnum == ActivitySourceEnum.Opportunities) ? "#FFFFFF" : "#FFFFFF")))
                                }).ToList();

                return neweventlist;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<List<SelectListItem>> GetSourceById(int id, string userid = null)
        {
            try
            {
                var accounts = await _accountNotesRepository.GetSourceById(id, userid);

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }
        //***************************************************************  Account Services -- AccountLines *****************************************************************************************************//

        public PagingList<AccountLineViewModel> GetAccountLinesList(int pageNo, string sortField, string sortOrder, int accountId, AccountLineViewModel accountLineViewModel)
        {
            try
            {
                pageNo = pageNo == 0 ? 1 : pageNo;
                var AccountFeaturesList = new List<AccountLineViewModel>();
                var AccountRepoList = _accountLinesRepository.GetAcountLinesList(pageNo, _pageSize, sortField, sortOrder, accountId, accountLineViewModel);
                foreach (var item in AccountRepoList)
                {
                    AccountFeaturesList.Add(new AccountLineViewModel
                    {
                        AccountLinesId = item.AccountLinesId,
                        AccountId = item.AccountId,
                        CarrierId = item.CarrierId,
                        CarrierTittle = item.CarrierTittle,
                        ContractDate = item.ContractDate,
                        ContractId = item.ContractId,
                        Mobile = item.Mobile,
                        MEDI_IMEI = item.MEDI_IMEI,
                        SIM = item.SIM,
                        DepartmentId = item.DepartmentId,
                        UserName = item.UserName,
                        LineStatusIsActive = item.LineStatusIsActive,
                        LineStatusIsUnActive = item.LineStatusIsUnActive,
                        ActiveRep = item.ActiveRep,

                        Make = item.Make,
                        MakeId = item.MakeId,
                        Modal = item.Modal,
                        ModalId = item.ModalId,
                        RatePlanId = item.RatePlanId,
                        RatePlanName = item.RatePlanName,
                        MontlyRate = item.MontlyRate,
                        FeatureId = item.FeatureId,
                        FeatureName = item.FeatureName,
                        ActType = item.ActType,

                        ActNote = item.ActNote


                    });


                }
                var list = PagingList<AccountLineViewModel>.ConvertListAsync(AccountFeaturesList.AsQueryable(), pageNo, AccountRepoList.TotalPages, sortField, sortOrder);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<AccountLineViewModel> GetAccountLinesById(int accountLineId)
        {
            try
            {
                var Accountobj = new AccountLineViewModel();
                if (accountLineId > 0)
                {
                    var account = await _accountLinesRepository.GetAccountLineById(accountLineId);
                    Accountobj.AccountId = account.AccountId;
                    Accountobj.AccountLinesId = account.AccountLinesId;
                    Accountobj.CarrierId = account.CarrierId;
                    Accountobj.FeatureId = account.FeatureId;
                    Accountobj.ContractId = account.ContractId;
                    Accountobj.Mobile = account.Mobile;
                    Accountobj.MEDI_IMEI = account.MEDI_IMEI;
                    Accountobj.SIM = account.SIM;
                    Accountobj.DepartmentId = account.DepartmentId;
                    Accountobj.UserName = account.UserName;
                    Accountobj.LineStatusIsActive = account.LineStatusIsActive;
                    //Accountobj.LineStatusIsUnActive = account.LineStatusIsUnActive;
                    Accountobj.ActiveRep = account.ActiveRep;
                    Accountobj.ModalId = account.ModalId;
                    Accountobj.MakeId = account.MakeId;
                    Accountobj.RatePlanId = account.RatePlanId;
                    Accountobj.MontlyRate = account.MontlyRate;
                    Accountobj.FeatureId = account.FeatureId;
                    Accountobj.ActType = account.ActType;

                    Accountobj.ActNote = account.ActNote;
                    Accountobj.ContractDate = account.ContractDate;
                }


                return Accountobj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ServiceResult> DeleteAccountLines(int accountLineId)
        {
            try
            {
                if (accountLineId > 0)
                {
                    var account = await _accountLinesRepository.GetAccountLineById(accountLineId);
                    if (account != null)
                    {
                        await _accountLinesRepository.DeleteAccountLine(account);

                    }
                }
                return _serviceResult;
            }
            catch (Exception ex)
            {
                _onErrorServiceResult.Message = ex.Message;
                return _onErrorServiceResult;
            }
        }

        public async Task<AjaxResponseModel> AddAccountLine(AccountLineViewModel accountLinesViewModel)
        {
            try
            {
                if (accountLinesViewModel.AccountLinesId > 0)
                {
                    return await UpdateAccountLine(accountLinesViewModel);
                }
                else
                {
                    return await SaveAccountLine(accountLinesViewModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AjaxResponseModel> SaveAccountLine(AccountLineViewModel accountLinesViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                var accountLines = new AccountLines
                {
                    AccountId = accountLinesViewModel.AccountId,
                    CarrierId = accountLinesViewModel.CarrierId,
                    ContractDate = accountLinesViewModel.ContractDate,
                    Mobile = accountLinesViewModel.Mobile,
                    ContractId = accountLinesViewModel.ContractId,
                    MEDI_IMEI = accountLinesViewModel.MEDI_IMEI,
                    SIM = accountLinesViewModel.SIM,
                    DepartmentId = accountLinesViewModel.DepartmentId,
                    UserName = accountLinesViewModel.UserName,
                    LineStatusIsActive = accountLinesViewModel.LineStatusIsActive,
                    // LineStatusIsUnActive = accountLinesViewModel.LineStatusIsUnActive,
                    ActiveRep = accountLinesViewModel.ActiveRep,

                    MakeId = accountLinesViewModel.MakeId,
                    ModalId = accountLinesViewModel.ModalId,
                    RatePlanId = accountLinesViewModel.RatePlanId,
                    MontlyRate = accountLinesViewModel.MontlyRate,
                    FeatureId = accountLinesViewModel.FeatureId,
                    ActType = accountLinesViewModel.ActType,

                    AccountLinesId = accountLinesViewModel.AccountLinesId,
                    ActNote = accountLinesViewModel.ActNote,
                    IsActive = true,
                    CreatedDate = DateTime.Now




                };



                await _accountLinesRepository.SaveOrUpdateAccountLines(accountLines);
                var accounttoupdate = _accountRepository.GetAccountById(accountLinesViewModel.AccountId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                response.Message = "Lines saved Sucessfully";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<AjaxResponseModel> UpdateAccountLine(AccountLineViewModel accountLinesViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                var account = await _accountLinesRepository.GetAccountLineById(accountLinesViewModel.AccountLinesId);
                account.AccountId = accountLinesViewModel.AccountId;
                account.CarrierId = accountLinesViewModel.CarrierId;
                account.ContractDate = accountLinesViewModel.ContractDate;
                account.Mobile = accountLinesViewModel.Mobile;
                account.MEDI_IMEI = accountLinesViewModel.MEDI_IMEI;
                account.SIM = accountLinesViewModel.SIM;
                account.DepartmentId = accountLinesViewModel.DepartmentId;
                account.UserName = accountLinesViewModel.UserName;
                if (accountLinesViewModel.LineStatusIsUnActive == 1)
                {
                    account.LineStatusIsActive = true;
                }
                else
                {
                    account.LineStatusIsActive = false;
                }
                // account.LineStatusIsUnActive = accountLinesViewModel.LineStatusIsUnActive;
                account.ActiveRep = accountLinesViewModel.ActiveRep;

                account.MakeId = accountLinesViewModel.MakeId;
                account.ModalId = accountLinesViewModel.ModalId;
                account.RatePlanId = accountLinesViewModel.RatePlanId;
                account.MontlyRate = accountLinesViewModel.MontlyRate;
                account.FeatureId = accountLinesViewModel.FeatureId;
                account.ActType = accountLinesViewModel.ActType;

                account.ActNote = accountLinesViewModel.ActNote;

                account.ModifiedDate = DateTime.Now;
                //account.ModifiedBy = accountViewModel.UserId;


                await _accountLinesRepository.SaveOrUpdateAccountLines(account);
                var accounttoupdate = _accountRepository.GetAccountById(accountLinesViewModel.AccountId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                response.Message = "Line Updated sucessfully";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        //*************************************************************** End Account Carrier *****************************************************************************************************//

        //*************************************************************** Account Feature *****************************************************************************************************//

        public PagingList<AccountFeatureViewModel> GetAccountFeatureList(int pageNo, string sortField, string sortOrder, int accountId, AccountFeatureViewModel featuresViewModel)
        {
            try
            {
                pageNo = pageNo == 0 ? 1 : pageNo;
                var AccountFeaturesList = new List<AccountFeatureViewModel>();
                var AccountRepoList = _accountFeatures.GetAcountFeaturesList(pageNo, _pageSize, sortField, sortOrder, accountId, featuresViewModel);
                foreach (var item in AccountRepoList)
                {
                    AccountFeaturesList.Add(new AccountFeatureViewModel
                    {
                        AccountId = item.AccountId,
                        AccountFeatureId = item.AccountFeatureId,
                        Date = item.Date,
                        Mobile = item.Mobile,
                        FeatureName = item.FeatureName,
                        FeatureCost = item.FeatureCost,
                        CarrierTittle = item.CarrierTittle,
                        AccountRepName = item.AccountRepName,
                        ActiveRepId = item.ActiveRepId,
                        DeviceId = item.DeviceId,
                        AccountRepId = item.AccountRepId



                    });


                }
                var list = PagingList<AccountFeatureViewModel>.ConvertListAsync(AccountFeaturesList.AsQueryable(), pageNo, AccountRepoList.TotalPages, sortField, sortOrder);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AccountFeatureViewModel> GetAccountFeatureById(int accountFeatureId)
        {
            try
            {
                var Accountobj = new AccountFeatureViewModel();
                if (accountFeatureId > 0)
                {

                    var account = await _accountFeatures.GetAccountFeatureById(accountFeatureId);
                    string datevar = Convert.ToDateTime(account.Date).ToShortDateString();
                    Accountobj.AccountId = account.AccountId;

                    Accountobj.AccountFeatureId = account.AccountFeatureId;
                    Accountobj.Date = Convert.ToDateTime(datevar).Date;
                    Accountobj.Mobile = account.Mobile;
                    Accountobj.FeatureId = account.FeaturesId;
                    Accountobj.FeatureCost = account.FeatureCost;
                    Accountobj.CarrierId = account.CarrierId;
                    Accountobj.ActiveRepId = account.ActiveRepId;
                    Accountobj.DeviceId = account.DeviceId;
                    Accountobj.FeatureId = account.FeaturesId;


                }


                return Accountobj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ServiceResult> DeleteAccountFeature(int accountFeaturesId)
        {
            try
            {
                if (accountFeaturesId > 0)
                {
                    var account = await _accountFeatures.GetAccountFeatureById(accountFeaturesId);
                    if (account != null)
                    {
                        await _accountFeatures.DeleteAccountFeature(account);

                    }
                }
                return _serviceResult;
            }
            catch (Exception ex)
            {
                _onErrorServiceResult.Message = ex.Message;
                return _onErrorServiceResult;
            }
        }

        public async Task<AjaxResponseModel> AddAccountFeatures(AccountFeatureViewModel accountFeatureViewModel)
        {
            try
            {
                if (accountFeatureViewModel.AccountFeatureId > 0)
                {
                    return await UpdateAccountFeature(accountFeatureViewModel);
                }
                else
                {
                    return await SaveAccountFeature(accountFeatureViewModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AjaxResponseModel> SaveAccountFeature(AccountFeatureViewModel accountFeatureViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                var accountLines = new AccountFeatures
                {
                    AccountId = accountFeatureViewModel.AccountId,
                    CarrierId = accountFeatureViewModel.CarrierId,
                    Date = Convert.ToDateTime(accountFeatureViewModel.Date).Date,
                    Mobile = accountFeatureViewModel.Mobile,
                    FeaturesId = accountFeatureViewModel.FeatureId,
                    FeatureCost = accountFeatureViewModel.FeatureCost,
                    ActiveRepId = accountFeatureViewModel.ActiveRepId,
                    DeviceId = accountFeatureViewModel.DeviceId,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };

                await _accountFeatures.SaveOrUpdateAccountFeatures(accountLines);
                var accounttoupdate = _accountRepository.GetAccountById(accountFeatureViewModel.AccountId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                response.Message = "Account Feature Saved Sucessfully";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<AjaxResponseModel> UpdateAccountFeature(AccountFeatureViewModel accountFeatureViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                var account = await _accountFeatures.GetAccountFeatureById(accountFeatureViewModel.AccountFeatureId);
                account.AccountId = accountFeatureViewModel.AccountId;
                account.CarrierId = accountFeatureViewModel.CarrierId;
                account.Date = Convert.ToDateTime(accountFeatureViewModel.Date).Date;
                account.Mobile = accountFeatureViewModel.Mobile;
                account.FeaturesId = accountFeatureViewModel.FeatureId;
                account.FeatureCost = accountFeatureViewModel.FeatureCost;
                account.DeviceId = accountFeatureViewModel.DeviceId;
                account.ActiveRepId = accountFeatureViewModel.ActiveRepId;


                account.ModifiedDate = DateTime.Now;
                //account.ModifiedBy = accountViewModel.UserId;


                await _accountFeatures.SaveOrUpdateAccountFeatures(account);
                var accounttoupdate = _accountRepository.GetAccountById(accountFeatureViewModel.AccountId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                response.Message = "Feature Updated sucessfully";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<List<SelectListItem>> GetFeaturesDropdown()
        {
            try
            {
                //var accounts = await _accountNotesRepository.GetAllAccounts();
                var Features = (await _keyValueRepository.GetKeyValue<Feature, SelectListItem>(x => x.IsDeleted != true, (x => new SelectListItem { Text = x.FeatureDetails, value = x.FeaturesId.ToString() })).ConfigureAwait(false)).ToList();

                return Features;
            }
            catch (Exception)
            {
                return null;
            }
        }

        //*************************************************************** End Account Feature *****************************************************************************************************//

        //*****************************************************************************Urgent Notes for Account Deatil Page ***********************************************************************//

        public async Task<List<NotesViewModel>> GetUrgentNotesList(int accountId)
        {
            try
            {
                var Notelist = new List<NotesViewModel>();
                var Note = await _accountNotesRepository.GetUrgentNotes(accountId);
                foreach (var item in Note)
                {
                    Notelist.Add(new NotesViewModel
                    {
                        NoteID = item.NoteID,
                        NoteSource = item.NoteSource,
                        SourceId = item.SourceId,
                        Notedescription = item.Notedescription,
                        Notetitle = item.Notetitle,
                        Time = item.Time,
                        CreatedDate = item.CreatedDate,
                        CreatedBy = item.CreatedBy,
                        IsDeleted = false
                    });
                }

                return Notelist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //************************************************************************** End Urgent Notes for Account Deatil Page ***********************************************************************//


        //************************************************************************** Added Services for Account Deatil Page ***********************************************************************//


        public async Task<List<AccountAddedServicesViewModel>> GetAccountAddedServicesList(int accountId)
        {
            try
            {
                var AccountList = new List<AccountAddedServicesViewModel>();
                var AccountRepoList = await _accountAddedServiceRepository.GetAcountAddedServicesList(accountId);
                foreach (var item in AccountRepoList)
                {
                    AccountList.Add(new AccountAddedServicesViewModel
                    {
                        AccountAddedServiceId = item.AccountAddedServiceId,
                        AccountId = item.AccountId,
                        Noserver = item.Noserver,
                        AuditMyBill = item.AuditMyBill,
                        WirelessSupportComplete = item.WirelessSupportComplete,
                        WirelessSupportCompleteMDM = item.WirelessSupportCompleteMDM
                    });


                }
                return AccountList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<AccountAddedServicesViewModel> GetAccountAddedServiceById(int accountAddedServicesId)
        {
            try
            {
                var Accountobj = new AccountAddedServicesViewModel();
                if (accountAddedServicesId > 0)
                {
                    var account = await _accountAddedServiceRepository.GetAccountAddedServicesById(accountAddedServicesId);
                    Accountobj.AccountId = account.AccountId;
                    Accountobj.AccountAddedServiceId = account.AccountAddedServiceId;
                    Accountobj.Noserver = account.Noserver;
                    Accountobj.AuditMyBill = account.AuditMyBill;
                    Accountobj.WirelessSupportComplete = account.WirelessSupportComplete;
                    Accountobj.WirelessSupportCompleteMDM = account.WirelessSupportCompleteMDM;

                }


                return Accountobj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ServiceResult> DeleteAccountAddedServices(int accountAddedServicesId)
        {
            try
            {
                if (accountAddedServicesId > 0)
                {
                    var account = await _accountAddedServiceRepository.GetAccountAddedServicesById(accountAddedServicesId);
                    if (account != null)
                    {
                        await _accountAddedServiceRepository.DeleteAccountAddedServices(account);

                    }
                }
                return _serviceResult;
            }
            catch (Exception ex)
            {
                _onErrorServiceResult.Message = ex.Message;
                return _onErrorServiceResult;
            }
        }

        public async Task<AjaxResponseModel> AddAccountAddedServices(AccountAddedServicesViewModel accountAddedServicesViewModel)
        {
            try
            {
                if (accountAddedServicesViewModel.AccountAddedServiceId > 0)
                {
                    return await UpdateAccountAddedServices(accountAddedServicesViewModel);
                }
                else
                {
                    return await SaveAccountAddedServices(accountAddedServicesViewModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AjaxResponseModel> SaveAccountAddedServices(AccountAddedServicesViewModel accountAddedServicesViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                var accountLines = new AccountAddedServices
                {
                    AccountId = accountAddedServicesViewModel.AccountId,
                    AccountAddedServiceId = accountAddedServicesViewModel.AccountAddedServiceId,
                    AuditMyBill = accountAddedServicesViewModel.AuditMyBill,
                    Noserver = accountAddedServicesViewModel.Noserver,
                    WirelessSupportComplete = accountAddedServicesViewModel.WirelessSupportComplete,
                    WirelessSupportCompleteMDM = accountAddedServicesViewModel.WirelessSupportCompleteMDM,

                    IsActive = true,
                    CreatedDate = DateTime.Now




                };



                await _accountAddedServiceRepository.SaveOrUpdateAccountAddedServices(accountLines);
                response.Message = "Service Added Sucessfully";
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return response;
        }

        public async Task<AjaxResponseModel> UpdateAccountAddedServices(AccountAddedServicesViewModel accountAddedServicesViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                var account = await _accountAddedServiceRepository.GetAccountAddedServicesById(accountAddedServicesViewModel.AccountAddedServiceId);
                account.AccountId = accountAddedServicesViewModel.AccountId;
                account.Noserver = accountAddedServicesViewModel.Noserver;
                account.AuditMyBill = accountAddedServicesViewModel.AuditMyBill;
                account.WirelessSupportComplete = accountAddedServicesViewModel.WirelessSupportComplete;
                account.WirelessSupportCompleteMDM = accountAddedServicesViewModel.WirelessSupportCompleteMDM;
                account.AccountAddedServiceId = accountAddedServicesViewModel.AccountAddedServiceId;



                account.ModifiedDate = DateTime.Now;
                //account.ModifiedBy = accountViewModel.UserId;


                await _accountAddedServiceRepository.SaveOrUpdateAccountAddedServices(account);
                response.Message = " Service Updated Sucessfully";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        //*************************************************************** End Account AddedServices *****************************************************************************************************//

        //*****************************************************************************Dropdown section  for Account Deatil Services ***********************************************************************//

        public async Task<List<DepartmentViewModel>> GetDepartmentList()
        {
            try
            {
                var Notelist = new List<DepartmentViewModel>();
                var Note = await _dropdownRepository.GetDepartments();
                foreach (var item in Note)
                {
                    Notelist.Add(new DepartmentViewModel
                    {
                        DepartmentId = item.DepartmentId,
                        DepartmentName = item.DepartmentName

                    });
                }

                return Notelist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<LeasetermViewModel>> GetLeaseTermList()
        {
            try
            {
                var Notelist = new List<LeasetermViewModel>();
                var Note = await _dropdownRepository.GetLeaseTerms();
                foreach (var item in Note)
                {
                    Notelist.Add(new LeasetermViewModel
                    {
                        LeaseTermId = item.LeaseTermId,
                        LeaseTermName = item.LeaseTermName

                    });
                }

                return Notelist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AccountActivityViewModel> GetAccountActivityById(int activityid)
        {
            try
            {
                var activityModel = new AccountActivityViewModel();
                if (activityid > 0)
                {
                    activityModel = await _accountNotesRepository.GetAccountActivityById(activityid);
                }
                return activityModel;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<List<ModelsViewM>> GetModelList()
        {
            try
            {
                var Notelist = new List<ModelsViewM>();
                var Note = await _dropdownRepository.GetModel();
                foreach (var item in Note)
                {
                    Notelist.Add(new ModelsViewM
                    {
                        ModelId = item.ModelId,
                        ModelName = item.ModelName

                    });
                }
                return Notelist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Documents

        public async Task<List<DocumentsViewModel>> GetDocumentsList()
        {
            try
            {
                var documentList = new List<DocumentsViewModel>();
                var documentsList = await _documentsRepository.GetDocumentList();
                foreach (var item in documentsList)
                {
                    documentList.Add(new DocumentsViewModel
                    {
                        OrderId = item.OrderId,
                        DocumentName = item.DocumentName,
                        DocumentType = item.DocumentType,
                        Description = item.Description,
                        FilePath = item.FilePath,
                        FilesPath = item.FilesPath,
                    });
                }
                return documentList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<DocumentsViewModel>> GetDocumentList(int accountid, DocumentsViewModel documentsViewModel)
        {
            try
            {
                var documentList = new List<DocumentsViewModel>();
                var documentsList = await _documentsRepository.GetDocumentList();
                foreach (var item in documentsList)
                {
                    documentList.Add(new DocumentsViewModel
                    {
                        OrderId = item.OrderId,
                        DocumentName = item.DocumentName,
                        DocumentType = item.DocumentType,
                        Description = item.Description,
                        FilePath = item.FilePath,
                        FilesPath = item.FilesPath,
                    });
                }
                return documentList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public PagingList<DocumentsViewModel> GetDocumentListPaging(int accountId, int pageNo, string sortField, string sortOrder)
        {
            try

            {

                pageNo = pageNo == 0 ? 1 : pageNo;
                var DocumentList = new List<DocumentsViewModel>();
                var document = _documentsRepository.GetDocuments(accountId, pageNo, _pageSize, sortField, sortOrder);
                foreach (var item in document)
                {
                    DocumentList.Add(new DocumentsViewModel
                    {
                        DocumentId = item.DocumentId,
                        AccountId = item.AccountId,
                        DocumentName = item.DocumentName,
                        DocumentType = item.DocumentType,
                        Description = item.Description,
                        DateUpload = item.DateUpload,
                        FilesPath = item.FilesPath,
                        SourceEnum = item.SourceEnum,
                        SourceId = item.SourceId,
                    });
                }
                var list = PagingList<DocumentsViewModel>.ConvertListAsync(DocumentList.AsQueryable(), pageNo, document.TotalPages, sortField, sortOrder);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<DocumentsViewModel> GetDocumentById(int DocumentId)
        {
            try
            {
                var Documentobj = new DocumentsViewModel();
                if (DocumentId > 0)
                {
                    var document = await _documentsRepository.GetDocumentsById(DocumentId);
                    Documentobj.DocumentId = document.DocumentId;
                    Documentobj.OrderId = document.OrderId;
                    Documentobj.DocumentName = document.DocumentName;
                    Documentobj.DocumentType = document.DocumentType;
                    Documentobj.Description = document.Description;
                    Documentobj.DateUpload = DateTime.UtcNow;
                    Documentobj.FilePath = document.FilePath;
                }
                return Documentobj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AjaxResponseModel> AddDocument(DocumentsViewModel documentViewModel)
        {
            try
            {
                if (documentViewModel.DocumentId > 0)
                {
                    return await UpdateDocument(documentViewModel);
                }
                else
                {
                    return await SaveDocument(documentViewModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AjaxResponseModel> SaveDocument(DocumentsViewModel documentViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                var document = new Documents { IsDeleted = false };
                foreach (var item in documentViewModel.FilesPath)
                {
                    if (item != null)
                    {
                        document.FilePath = item.FileName;
                    }
                }
                var documentObj = new Documents
                {
                    DocumentId = documentViewModel.DocumentId,
                    DocumentName = documentViewModel.DocumentName,
                    DocumentType = documentViewModel.DocumentType,
                    Description = documentViewModel.Description,
                    SourceEnum = documentViewModel.SourceEnum,
                    SourceId = documentViewModel.SourceId,
                    FilePath = document.FilePath,
                    DateUpload = DateTime.UtcNow,
                    IsDeleted = false,
                };
                await _documentsRepository.SaveOrUpdateDocument(documentObj);
                var accounttoupdate = _accountRepository.GetAccountById(documentViewModel.AccountId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                response.Message = "Documents saved successfully";

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<AjaxResponseModel> UpdateDocument(DocumentsViewModel documentViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };

            try
            {
                var document = await _documentsRepository.GetDocumentsById(documentViewModel.DocumentId);
                document.DocumentId = documentViewModel.DocumentId;
                document.DocumentName = documentViewModel.DocumentName;
                document.DocumentType = documentViewModel.DocumentType;
                document.Description = documentViewModel.Description;
                document.FilePath = documentViewModel.FilePath;
                document.DateUpload = DateTime.UtcNow;
                document.ModifiedDate = DateTime.UtcNow;
                document.SourceEnum = documentViewModel.SourceEnum;
                document.SourceId = documentViewModel.SourceId;

                await _documentsRepository.SaveOrUpdateDocument(document);
                var accounttoupdate = _accountRepository.GetAccountById(documentViewModel.AccountId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ServiceResult> DeleteDocument(int DocumentId)
        {
            try
            {
                if (DocumentId > 0)
                {
                    var document = await _documentsRepository.GetDocumentsById(DocumentId);
                    if (document != null)
                    {
                        await _documentsRepository.DeleteDocument(document);

                    }
                }
                return _serviceResult;
            }
            catch (Exception ex)
            {
                _onErrorServiceResult.Message = ex.Message;
                return _onErrorServiceResult;
            }
        }

        //End Documents--------------->


        public async Task<AccountActiviryViewModel> GetActivitiesByAcId(int? acsourceid,
            int? accounttypeid,
            DateTime? startdate,
            DateTime? enddate,
            int? activityStatus,
            int? activityTrigger,
            string userId,
            bool IsAdmin,
            ActivitySourceEnum? activitysource = null)
        {
            try
            {
                var carrierObj = new AccountActiviryViewModel();
                if (acsourceid.HasValue && acsourceid.Value > 0)
                {
                    var Activitylist = new List<ActivityListByDate>();
                    var activities = await _accountNotesRepository.GetActivity(accounttypeid, startdate, enddate, activityStatus, activityTrigger, userId, IsAdmin, activitytypeenum: activitysource, acsourceid: acsourceid);

                    //activities = activities?.Where(w => w.ActivitySourceId == accountid).ToList();

                    var typelist = activities?.Select(s => new
                    {
                        StartDate = s.StartDate
                    }).Distinct().ToList();

                    if (typelist == null)
                        return carrierObj;

                    foreach (var item in typelist)
                    {
                        var Activity = new ActivityListByDate();
                        var activitylist = new List<ActivityListByTypeViewModel>();

                        Activity.StartDate = item.StartDate;


                        var accGroupByDate = activities.Where(w => w.StartDate == item.StartDate)
                                            .Select(s => new
                                            {
                                                activitytype = s.ActivityTypeName.Trim(),
                                                activitytypeid = s.ActivityTypeId,
                                                ActivityTypeEnum = s.ActivityTypeEnum
                                            })
                                            .Distinct().ToList();

                        foreach (var titem in accGroupByDate)
                        {
                            var acdata = new ActivityListByTypeViewModel();

                            acdata.ActivityType = titem.activitytype;
                            acdata.ActivityTypeId = titem.activitytypeid;
                            acdata.ActivityTypeEnum = titem.ActivityTypeEnum;

                            acdata.Activities = activities.Where(d => d.StartDate == item.StartDate &&
                                                                    d.ActivityTypeId == titem.activitytypeid)
                                .Select(item => new ActivityViewModel
                                {
                                    ActivityId = item.ActivityId,
                                    StartDate = item.StartDate,
                                    StartTime = item.StartTime,
                                    SourceName = item.SourceName,
                                    ActivityName = item.ActivityName,
                                    SourceId = item.SourceId,
                                    ActivityTypeId = item.ActivityTypeId,
                                    ActivityTypeName = item.ActivityTypeName,
                                    ActivityTriggerId = item.ActivityTriggerId,
                                    StatusName = item.StatusName,
                                    CreatedBy = item.CreatedByName,
                                    CreatedByName = item.CreatedByName,
                                    EndDate = item.EndDate,
                                    EndTime = item.EndTime,
                                    ActivityTypeEnum = item.ActivityTypeEnum,
                                    IsCancel = item.IsCancel,
                                    CancelReason = item.CancelReason,
                                    UserName = item.UserName,
                                    Description = item.Description
                                }).ToList();

                            activitylist.Add(acdata);
                        }

                        Activity.ActivitiesData = activitylist;//.OrderByDescending(o => o.StartDate).ToList();

                        Activitylist.Add(Activity);
                    }


                    carrierObj.Activities = Activitylist;

                }
                return carrierObj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> SendActivityEmail(SendEmailRequest request)
        {
            try
            {
                _emailHelper.SendEmail(request.toemail, request.emailbody, request.emailsubj);

                var isadded = await _accountNotesRepository.AddEventActivityHistory(request);

                return isadded;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<List<RatePlanViewModel>> GetRatePlanList()
        {
            try
            {
                var Notelist = new List<RatePlanViewModel>();
                var Note = await _dropdownRepository.GetRatePlan();
                foreach (var item in Note)
                {
                    Notelist.Add(new RatePlanViewModel
                    {
                        ServiceId = item.ServiceId,
                        RatePlansName = item.RatePlansName

                    });
                }

                return Notelist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<FeaturesViewModel>> GetFeaturesList()
        {
            try
            {
                var Notelist = new List<FeaturesViewModel>();
                var Note = await _dropdownRepository.GetFeature();
                foreach (var item in Note)
                {
                    Notelist.Add(new FeaturesViewModel
                    {
                        FeaturesId = item.FeaturesId,
                        FeatureDetails = item.FeatureDetails

                    });
                }

                return Notelist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<CarrierContractViewModel>> GetContractList()
        {
            try
            {
                var Notelist = new List<CarrierContractViewModel>();
                var Note = await _dropdownRepository.GetContract();
                foreach (var item in Note)
                {
                    Notelist.Add(new CarrierContractViewModel
                    {
                        ContractManageId = item.ContractManageId,
                        ContractName = item.ContractName

                    });
                }

                return Notelist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<DeviceMakeViewModel>> GetMakeList()
        {
            try
            {
                var Notelist = new List<DeviceMakeViewModel>();
                var Note = await _dropdownRepository.GetMake();
                foreach (var item in Note)
                {
                    Notelist.Add(new DeviceMakeViewModel
                    {
                        DeviceMakeId = item.DeviceMakeId,
                        Name = item.Name

                    });
                }

                return Notelist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        //************************************************************************** END Dropdown section  for Account Deatil Services ***********************************************************************//

        public async Task<List<EmailActivityList>> GetEmailActivityList(int activityid)
        {
            try
            {
                var activitydata = await _accountNotesRepository.GetEmailActivityList(activityid);

                return activitydata;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public PagingList<AccountDeviceLeaseViewModel> GetAccountDeviceLeaseList(int pageNo, string sortField, string sortOrder, int accountId, AccountDeviceLeaseViewModel accountDeviceLeaseViewModel)
        {
            try
            {
                pageNo = pageNo == 0 ? 1 : pageNo;
                var AccountFeaturesList = new List<AccountDeviceLeaseViewModel>();
                var AccountRepoList = _accountDeviceLease.GetAcountDeviceLeaseList(pageNo, _pageSize, sortField, sortOrder, accountId, accountDeviceLeaseViewModel);
                foreach (var item in AccountRepoList)
                {
                    AccountFeaturesList.Add(new AccountDeviceLeaseViewModel
                    {
                        AccountDeviceLeaseId = item.AccountDeviceLeaseId,
                        AccountId = item.AccountId,
                        CarrierId = item.CarrierId,
                        CarrierTittle = item.CarrierTittle,
                        LeaseDate = item.LeaseDate,
                        LeaseTermName = item.LeaseTermName,
                        Mobile = item.Mobile,
                        MEDI_IMEI = item.MEDI_IMEI,
                        SIM = item.SIM,
                        LeaseNotes = item.LeaseNotes,
                        UserName = item.UserName,
                        LineStatusIsActive = item.LineStatusIsActive,
                        ReturnDate = item.ReturnDate,
                        //LineStatusIsActive = item.LineStatusIsActive,
                        AccountRepId = item.AccountRepId,

                        Make = item.Make,
                        MakeId = item.MakeId,
                        Modal = item.Modal,
                        ModalId = item.ModalId,

                        MontlyRate = item.MontlyRate,


                    });


                }
                var list = PagingList<AccountDeviceLeaseViewModel>.ConvertListAsync(AccountFeaturesList.AsQueryable(), pageNo, AccountRepoList.TotalPages, sortField, sortOrder);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<AccountDeviceLeaseViewModel> GetAccountDeviceLeaseById(int accountDeviceLeaseId)
        {
            try
            {
                var Accountobj = new AccountDeviceLeaseViewModel();
                if (accountDeviceLeaseId > 0)
                {
                    var account = await _accountDeviceLease.GetAccountDeviceLeaseById(accountDeviceLeaseId);
                    Accountobj.AccountId = account.AccountId;
                    Accountobj.AccountDeviceLeaseId = account.AccountDeviceLeaseId;
                    Accountobj.CarrierId = account.CarrierId;
                    Accountobj.LeaseTermId = account.LeaseTermId;
                    Accountobj.LineStatusIsActive = account.LineStatusIsActive;
                    if(Accountobj.LineStatusIsActive)
                    {
                        Accountobj.CheckLineStatusIsActive = 1;
                    }
                    else
                    {
                        Accountobj.CheckLineStatusIsActive = 0;
                    }
                    Accountobj.Mobile = account.Mobile;
                    Accountobj.MEDI_IMEI = account.MEDI_IMEI;
                    Accountobj.SIM = account.SIM;
                    Accountobj.ReturnDate = account.ReturnDate;

                    Accountobj.UserName = account.UserName;
                    Accountobj.LineStatusIsActive = account.LineStatusIsActive;

                    Accountobj.AccountRepId = account.ActiveRep;
                    Accountobj.ModalId = account.ModalId;
                    Accountobj.MakeId = account.MakeId;

                    Accountobj.MontlyRate = account.MontlyRate;


                    Accountobj.LeaseNotes = account.LeaseNotes;
                    Accountobj.LeaseDate = account.LeaseDate;
                }


                return Accountobj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public AccountDeviceLeaseViewModel GetAccountDeviceLeaseByIds(int accountDeviceLeaseId)
        {
            try
            {
                var Accountobj = new AccountDeviceLeaseViewModel();
                if (accountDeviceLeaseId > 0)
                {
                    var account = _accountDeviceLease.GetAccountDeviceLeaseById(accountDeviceLeaseId).Result;
                    Accountobj.AccountId = account.AccountId;
                    Accountobj.AccountDeviceLeaseId = account.AccountDeviceLeaseId;
                    Accountobj.CarrierId = account.CarrierId;
                    Accountobj.LeaseTermId = account.LeaseTermId;
                    Accountobj.LineStatusIsActive = account.LineStatusIsActive;
                    Accountobj.Mobile = account.Mobile;
                    Accountobj.MEDI_IMEI = account.MEDI_IMEI;
                    Accountobj.SIM = account.SIM;
                    Accountobj.ReturnDate = account.ReturnDate;

                    Accountobj.UserName = account.UserName;
                    Accountobj.LineStatusIsActive = account.LineStatusIsActive;

                    Accountobj.ActiveRep = account.ActiveRep;
                    Accountobj.ModalId = account.ModalId;
                    Accountobj.MakeId = account.MakeId;

                    Accountobj.MontlyRate = account.MontlyRate;


                    Accountobj.LeaseNotes = account.LeaseNotes;
                    Accountobj.LeaseDate = account.LeaseDate;
                }


                return Accountobj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ServiceResult> DeleteAccountDeviceLease(int accountDeviceLease)
        {
            try
            {
                if (accountDeviceLease > 0)
                {
                    var account = await _accountDeviceLease.GetAccountDeviceLeaseById(accountDeviceLease);
                    if (account != null)
                    {
                        await _accountDeviceLease.DeleteAccountDeviceLease(account);

                    }
                }
                return _serviceResult;
            }
            catch (Exception ex)
            {
                _onErrorServiceResult.Message = ex.Message;
                return _onErrorServiceResult;
            }
        }

        public async Task<AjaxResponseModel> AddAccounDeviceLease(AccountDeviceLeaseViewModel accountDeviceLeaseViewModel)
        {
            try
            {
                if (accountDeviceLeaseViewModel.AccountDeviceLeaseId > 0)
                {
                    return await UpdateAccountDeviceLease(accountDeviceLeaseViewModel);
                }
                else
                {
                    return await SaveAccountDeviceLease(accountDeviceLeaseViewModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AjaxResponseModel> SaveAccountDeviceLease(AccountDeviceLeaseViewModel accountDeviceLeaseViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                var accountLines = new AccountDeviceLease
                {
                    AccountId = accountDeviceLeaseViewModel.AccountId,
                    CarrierId = accountDeviceLeaseViewModel.CarrierId,
                    LeaseDate = accountDeviceLeaseViewModel.LeaseDate,
                    Mobile = accountDeviceLeaseViewModel.Mobile,

                    MEDI_IMEI = accountDeviceLeaseViewModel.MEDI_IMEI,
                    SIM = accountDeviceLeaseViewModel.SIM,

                    UserName = accountDeviceLeaseViewModel.UserName,
                    ReturnDate = accountDeviceLeaseViewModel.ReturnDate,
                    LineStatusIsActive = accountDeviceLeaseViewModel.LineStatusIsActive,
                    ActiveRep = accountDeviceLeaseViewModel.AccountRepId,

                    MakeId = accountDeviceLeaseViewModel.MakeId,
                    ModalId = accountDeviceLeaseViewModel.ModalId,
                    MontlyRate = accountDeviceLeaseViewModel.MontlyRate,
                    LeaseTermId = accountDeviceLeaseViewModel.LeaseTermId,

                    LeaseNotes = accountDeviceLeaseViewModel.LeaseNotes,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreateBy = accountDeviceLeaseViewModel.UserID




                };



                await _accountDeviceLease.SaveOrUpdateAccountDeviceLease(accountLines);
                var accounttoupdate = _accountRepository.GetAccountById(accountDeviceLeaseViewModel.AccountId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                response.Message = "DeviceLease Created Sucessfully";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<AjaxResponseModel> UpdateAccountDeviceLease(AccountDeviceLeaseViewModel accountDeviceLeaseViewModel)
        {
            var response = new AjaxResponseModel() { flag = true };
            try
            {
                var account = await _accountDeviceLease.GetAccountDeviceLeaseById(accountDeviceLeaseViewModel.AccountDeviceLeaseId);
                account.AccountId = accountDeviceLeaseViewModel.AccountId;
                account.CarrierId = accountDeviceLeaseViewModel.CarrierId;
                account.LeaseDate = accountDeviceLeaseViewModel.LeaseDate;
                account.Mobile = accountDeviceLeaseViewModel.Mobile;
                account.MEDI_IMEI = accountDeviceLeaseViewModel.MEDI_IMEI;
                account.SIM = accountDeviceLeaseViewModel.SIM;

                account.UserName = accountDeviceLeaseViewModel.UserName;
                account.LineStatusIsActive = accountDeviceLeaseViewModel.LineStatusIsActive;
                if(accountDeviceLeaseViewModel.CheckLineStatusIsActive==1)
                {
                    account.LineStatusIsActive = true;
                }
                else
                {
                    account.LineStatusIsActive = false;
                }

                account.MakeId = accountDeviceLeaseViewModel.MakeId;
                account.ModalId = accountDeviceLeaseViewModel.ModalId;
                account.LeaseTermId = accountDeviceLeaseViewModel.LeaseTermId;
                account.MontlyRate = accountDeviceLeaseViewModel.MontlyRate;
                account.ReturnDate = accountDeviceLeaseViewModel.ReturnDate;
                account.ActiveRep = accountDeviceLeaseViewModel.AccountRepId;


                account.LeaseNotes = accountDeviceLeaseViewModel.LeaseNotes;

                account.ModifiedDate = DateTime.Now;
                account.ModifiedBy = accountDeviceLeaseViewModel.UserID;


                await _accountDeviceLease.SaveOrUpdateAccountDeviceLease(account);
                var accounttoupdate = _accountRepository.GetAccountById(accountDeviceLeaseViewModel.AccountId).Result;
                accounttoupdate.LastConnect = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(accounttoupdate);
                response.Message = "Device Lease Updated Sucessfully";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        //*************************************************************** End Account Carrier *****************************************************************************************************//



        public async Task<List<string>> GetAutoCompleteResult(bool showMyAccountsOnly, string userId, string filter)
        {
            //return await _accountRepository.GetAutoCompleteResult(filter);

            var AccountRepoList = await this.GetAccounts(showMyAccountsOnly, userId, filter);
            var AccountList = new List<string>();

            foreach (var item in AccountRepoList)
            {
                AccountList.Add(item.AccountName);
            }
            return AccountList;
        }

        public async Task<AccountLineViewModel> GetAccountLinesInfoById(int accountLinesId)
        {
            try
            {
                var Accountobj = new AccountLineViewModel();
                if (accountLinesId > 0)
                {
                    var account = await _accountLinesRepository.GetAccountLineInfoById(accountLinesId);
                    Accountobj.AccountId = account.AccountId;
                    Accountobj.AccountLinesId = account.AccountLinesId;
                    Accountobj.CarrierId = account.CarrierId;
                    Accountobj.FeatureId = account.FeatureId;
                    Accountobj.ContractId = account.ContractId;
                    Accountobj.Mobile = account.Mobile;
                    Accountobj.MEDI_IMEI = account.MEDI_IMEI;
                    Accountobj.SIM = account.SIM;
                    Accountobj.DepartmentId = account.DepartmentId;
                    Accountobj.DepartMentName = account.DepartMentName;
                    Accountobj.UserName = account.UserName;
                    Accountobj.LineStatusIsActive = account.LineStatusIsActive;
                    if (account.LineStatusIsActive)
                    {
                        Accountobj.LineStatusIsUnActive = 1;
                    }
                    else
                    {
                        Accountobj.LineStatusIsUnActive = 0;
                    }
                    Accountobj.ActiveRep = account.ActiveRep;
                    Accountobj.Modal = account.Modal;
                    Accountobj.ModalId = account.ModalId;
                    Accountobj.Make = account.Make;
                    Accountobj.MakeId = account.MakeId;
                    Accountobj.RatePlanName = account.RatePlanName;
                    Accountobj.RatePlanId = account.RatePlanId;
                    Accountobj.MontlyRate = account.MontlyRate;
                    Accountobj.FeatureName = account.FeatureName;
                    Accountobj.FeatureId = account.FeatureId;
                    Accountobj.ActType = account.ActType;
                    Accountobj.ActiveRep = account.ActiveRep;

                    Accountobj.ActNote = account.ActNote;
                    Accountobj.ContractDate = account.ContractDate;



                }

                return Accountobj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<bool> AddPhoneHistory(PhoneHistoryRequest request)
        {
            try
            {
                return await _accountNotesRepository.AddPhoneHistory(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<PhoneActivityList>> GetAllPhoneHistoryList(int activityid)
        {
            try
            {
                return await _accountNotesRepository.GetAllPhoneHistoryList(activityid);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> IsAccountRepresentative(string Id)
        {
            return await _accountRepository.IsAccountRepresentative(Id);
        }
        public async Task<List<Model.ViewModel.CompanyViewModel>> GetGlobalFilerAccounts(string globalFilter)
        {
            return await _accountRepository.GetGlobalFilerAccounts(globalFilter);
        }

        public SaveResponseDetails SaveActivityNotes(ActivityNotesviewModel request)
        {
            var response = _accountNotesRepository.SaveActivityNotes(request);

            return response;
        }
        public async Task<List<ActivityNotesviewModel>> GetActivityNotesList(int activityId)
        {
            try
            {
                var Notelist = new List<ActivityNotesviewModel>();
                var Note = await _accountNotesRepository.GetActivityNotesList(activityId);
                foreach (var item in Note)
                {
                    Notelist.Add(new ActivityNotesviewModel
                    {
                        Id = item.Id,
                        NoteTitle = item.NoteTitle,
                        NoteDescription = item.NoteDescription,
                        CreatedOn = item.CreatedOn,
                        CreatedBy = item.CreatedBy
                    });
                }

                return Notelist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteActivityNotes(int noteId)
        {
            var response = await _accountNotesRepository.DeleteActivityNotes(noteId);

            return response;
        }
        public async Task<List<SelectListItem>> GetFilterStatus()
        {
            try
            {
                var accounts = await _accountNotesRepository.GetFilterStatus();

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<SelectListItem>> GetUser()
        {
            try
            {
                var accounts = await _accountNotesRepository.GetUser();

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public async Task<CompanyViewModel> GetAccountDetailsById(int accountId)
        {
            List<Expression<Func<tbl_Account, object>>> incluceExpression = new List<Expression<Func<tbl_Account, object>>>();
            List<Expression<Func<tbl_AccountContact, object>>> incluceExpressionContact = new List<Expression<Func<tbl_AccountContact, object>>>();
            incluceExpressionContact.Add(x => x.tbl_Contact);
            var account = (await _accountRepository.GetAsync(x => x.AccountId == accountId && x.IsDeleted != true, null, incluceExpression, true).ConfigureAwait(false)).FirstOrDefault();
            var dataContacts = (await _keyValueRepository.GetKeyValue<tbl_AccountContact, SelectListItem<string, string>>(x => x.IsDeleled != true && x.AccountId == accountId, (x => new SelectListItem<string, string> { Text = $"{x.tbl_Contact.FirstName} {x.tbl_Contact.FirstName}", Value = x.tbl_Contact.Email }), incluceExpressionContact).ConfigureAwait(false));
            var data = _mapper.Map<CompanyViewModel>(account);
            data.ContactName = dataContacts.FirstOrDefault().Text;
            data.Email = dataContacts.FirstOrDefault().Value;
            return data;

        }

        public async Task<List<NotificationActivity>> GetReminderActivity(string UserId, DateTime datetime)
        {
            var response = await _accountNotesRepository.GetReminderActivity(UserId, datetime);

            return response;
        }

        public async Task<int> ActivityListCount(bool IsAdmin, string UserId)
        {
            var response = await _accountNotesRepository.GetActivityListCount(IsAdmin, UserId);

            return response;


        }
        public async Task<int> ActivityNotificationListCount(bool IsAdmin, string UserId)
        {
            var response = await _accountNotesRepository.ActivityNotificationListCount(IsAdmin, UserId);

            return response;


        }

        public async Task<List<ActivityStatusHistoryList>> GetAllActivityHistoryById(int activityid)
        {
            var response = await _accountNotesRepository.GetAllActivityHistoryById(activityid);

            return response;
        }

        public async Task<bool> AddActivityStatusHistory(ActivityStatusHistoryRequest request)
        {
            try
            {
                return await _accountNotesRepository.AddActivityStatusHistory(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<SelectListItem>> BindAllContactDatasByAccount(int accountId)
        {
            try
            {
                var accounts = await _accountNotesRepository.BindAllContactDatasByAccount(accountId);

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public PagingList<Dashbordactivityrequest> GetDashbordActivityListPaging(int pageNo, DashbordActivityViewModel dashbordModel, bool fromPaging, string sortField, string sortOrder)
        {

            pageNo = pageNo == 0 ? 1 : pageNo;

            var request = _accountNotesRepository.GetDashbordActivityListPaging(pageNo, _pageSizeDas, sortField, sortOrder, dashbordModel);

            return request;
        }
        public async Task<string> Binddeviceworkphone(int accountId)
        {
            try
            {
                var accounts = await _accountNotesRepository.Binddeviceworkphone(accountId);

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<OrderAddViewModel> Binddevicepasstax(int accountId)
        {
            try
            {
                var accounts = await _accountNotesRepository.Binddevicepasstax(accountId);

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<List<SelectListItem>> GetAccountFeature()
        {
            try
            {
                var accounts = await _accountNotesRepository.GetAccountFeature();

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<List<SelectListItemOrder>> GetRatePlan()
        {
            try
            {
                var accounts = await _accountNotesRepository.GetRatePlan();

                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<tbl_NotificationViewModel>> GetAllUnReadNotification(string userId)
        {
            try
            {
                var unreadnotifications = await _accountNotesRepository.GetAllUnReadNotification(userId);

                return unreadnotifications;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> SaveNotifyAsRead(int id, string userid)
        {
            try
            {
                var updateddata = await _accountNotesRepository.SaveNotifyAsRead(id, userid);

                return updateddata;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public PagingList<tbl_NotificationtReqaust> GetAllNotificationByUser(int pageNo, string sortField, string sortOrder, tbl_NotificationtReqaust requestViewModel, string userid)
        {
            try
            {

                pageNo = pageNo == 0 ? 1 : pageNo;

                var request = _accountNotesRepository.GetAllNotificationByUser(pageNo, _pageSize, sortField, sortOrder, requestViewModel, userid);

                return request;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> SaveNextContactDate(string strNextContactDate, int accountId)
        {
            try
            {

                if (strNextContactDate != null)
                {
                    var NextContactDate = Convert.ToDateTime(strNextContactDate);
                    await _accountRepository.SaveNextContactDate(NextContactDate, accountId);
                }
                else
                {
                    await _accountRepository.SaveNextContactDate(null, accountId);
                }

                return true;
            }
            catch (Exception ex)
            {
                _onErrorServiceResult.Message = ex.Message;
                return true;
            }
        }

        public int QuotationListCount()
        {
            return _accountRepository.QuotationListCount();
        }

        public Task<bool> UpdateDevicestatus(string id, DateTime changedate)
        {
            try
            {
                var result = _accountRepository.UpdateDevicestatus(id.ToString(), changedate);
                return result;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public dynamic GetExport(int AccountId)
        {
            try
            {

                //  pageNo = pageNo == 0 ? 1 : pageNo;

                var request = _accountRepository.GetExport(AccountId);

                return request;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public PagingList<AddDeviceLeaseLabeResponse> GetDeviceLeasePreviousData(int AccountId, int pageNo, string sortField, string sortOrder)
        {
            try
            {

                pageNo = pageNo == 0 ? 1 : pageNo;

                //_pageSize = 5;
                var request = _accountRepository.GetDeviceLeasePreviousData(AccountId, pageNo, _pageSizeDas, sortField, sortOrder);

                return request;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SaveAttachMentViewModel> GenerateLabelAsync(int ShippingAddress)
        {

            SaveAttachMentViewModel modelresponse = new SaveAttachMentViewModel();
            try
            {
                int ShippingMethod = 32;
                modelresponse.ShippingAddressData = await _accountRepository.GetshippingAddres(ShippingAddress);
                modelresponse.ShippingServiceCode = await _accountRepository.GetShippingServiceCode(ShippingMethod);
                return modelresponse;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public bool AddRepairLabelData(AddClientRepairsLabelViewModel model, AddDeviceLeaseLabelViewModel devicemodel)
        {
            var response = _accountRepository.AddRepairLabelData(model, devicemodel);

            return response;
        }

        public async Task<NotificationemailTicketModel> NotificationToCostomer(AccountDeviceLeaseViewModel request, string loginuser, string fullAddress)
        {
            NotificationemailTicketModel respons = new NotificationemailTicketModel();
            try
            {
                Dictionary<string, string> objEmailTokens = new Dictionary<string, string>();

                List<Dictionary<string, string>> listEmailTokens = new List<Dictionary<string, string>>();
                if (request.CreatedBy == null)
                {
                    request.CreatedBy = loginuser;
                }
                var userSignature = _settingStatusRepository.GetSignature(request.CreatedBy);

                objEmailTokens.Add("%CONTACTNAME%", request.AccountName);
                objEmailTokens.Add("%Name%", loginuser);
                objEmailTokens.Add("%TicketID%", request.AccountDeviceLeaseId.ToString());
                //objEmailTokens.Add("%ReplacementCost%", txtEstimateCost.Text);
                objEmailTokens.Add("%ShippingAddress%", fullAddress);
                objEmailTokens.Add("%ShippingMethod%", "UPS_Ground_Mail");
                objEmailTokens.Add("%ShippingTracking%", "");

                objEmailTokens.Add("%MULTIPLE%", "");



                var sendToMailAddress = request.ToEmail;
                var sendToName = loginuser;
                var emailsubject = request.EmailTitle;
                var eamil = _emailService.SendOrderEmail(sendToMailAddress, objEmailTokens, listEmailTokens, emailsubject, null, request.attachments.ToArray(), null, null, null);

                AddDeviceLeaseLabelViewModel DeviceModel = new AddDeviceLeaseLabelViewModel
                {

                    EmailContent = "",
                    EmailSubject = emailsubject,
                    EmailTo = request.ToEmail,


                };

                _accountRepository.UpdateDeviceLeaseLabelData(DeviceModel);

                respons.success = true;
                respons.message = "Email send";
                return respons;
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }


        public tbl_DeviceLeasePEmail GetDeviceLeaseLabelData(int id)
        {
            var response = _accountRepository.GetDeviceLeaseLabelData(id);

            return response;
        }

        public async Task<bool> FindAcoountEmail(string AccountName)
        {
            try
            {

                if (AccountName != null)
                {
                    bool IsEmail = await _accountRepository.GetAccountName(AccountName);
                    return IsEmail;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}