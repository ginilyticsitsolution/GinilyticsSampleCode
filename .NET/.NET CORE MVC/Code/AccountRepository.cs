using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WirelessSupport.Model.DataModel;
using WirelessSupport.Model.Enums;
using WirelessSupport.Model.ViewModel;
using WirelessSupport.Model.ViewModel.Enums;
using WirelessSupport.Repository.Repository.Contracts.ComapnyAccount;
using WirelessSupport.Repository.SetUp.Provider;
using WirelessSupport.Repository.SetUp.ServiceExtention.Configuration;

namespace WirelessSupport.Repository.Repository.RepositoryComapnyAccount
{
    public class AccountRepository : Repository<tbl_Account>, IAccountRepository
    {
        public readonly WirelessSupportContext _context;
        public readonly ApplicationDbContext _contextIdentity;
        private readonly UserManager<ApplicationUser> _userManager;
        int DeviceLeasePEmail = 0;
        public AccountRepository(WirelessSupportContext context, ApplicationDbContext contextIdentity, UserManager<ApplicationUser> userManager) : base(context)
        {
            _context = context;
            _contextIdentity = contextIdentity;
            _userManager = userManager;
        }

        public async Task<bool> DeleteAccount(tbl_Account account)
        {
            try
            {
                //_context.tbl_Account.Remove(account);
                account.IsDeleted = true;
                _context.tbl_Account.Update(account);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<tbl_Account> GetAccountById(int accountId)
        {
            try
            {
                return await _context.tbl_Account.Where(x => x.AccountId == accountId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<int> GetAccountIdByEmail(string Email)
        {
            try
            {
                return await _context.tbl_Account.Where(x => x.Email == Email && x.IsDeleted==false).Select(x=>x.AccountId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<string> GetAccountNameByEmail(string Email)
        {
            try
            {
                return await _context.tbl_Account.Where(x => x.Email == Email && x.IsDeleted == false).Select(x => x.AccountName).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> GetContactByAccountId(int Accountid)
        {
            try
            {
                return await _context.tbl_AccountContact.Where(x => x.AccountId == Accountid && x.IsPrimary == true).Select(x => x.ContactId).FirstOrDefaultAsync();
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

                var accountList = from account in _context.tbl_Account
                         .Where(x => x.AccountId == accountId).DefaultIfEmpty()

                                      //from industryType in _context.tbl_IndustryType
                                      //.Where(x => x.IndustryTypeId == account.IndustryTypeId).DefaultIfEmpty()

                                  from customerSource in _context.tbl_CustomerSource
                                  .Where(o => o.CustomerSourceId == account.CustomerSourceId).DefaultIfEmpty()

                                  from carrier in _context.tbl_AccountCarrier
                                  .Where(a => a.AccountId == accountId && a.IsPrimery == true).Take(1).DefaultIfEmpty()

                                  from Contact in _context.tbl_AccountContact
                                 .Where(a => a.AccountId == accountId && a.IsPrimary).Take(1).DefaultIfEmpty()

                                  from ContactName in _context.tbl_Contacts
                                 .Where(a => a.ContactId == Contact.ContactId).Take(1).DefaultIfEmpty()


                                  from accountAddress in _context.tbl_AccountAddress
                                  .Where(x => x.AccountId == accountId).DefaultIfEmpty()

                                  from carrierTittle in _context.Carriers
                                 .Where(a => a.CarrierId == carrier.CarrierId).DefaultIfEmpty()

                                      //from acountRep in _userManager.Users
                                      //.Where(p => p.Id == account.AccountRepId).DefaultIfEmpty()

                                      //        from CompanyAddress in _context.tbl_AccountAddress
                                      //.Where(a => a.AccountId == account.AccountId && a.AddressType == 1).DefaultIfEmpty()

                                  from city in _context.tbl_Cities
                                  .Where(x => x.Id == accountAddress.CityId).DefaultIfEmpty()

                                  from State in _context.tbl_States
                                 .Where(x => x.Id == accountAddress.CityId).DefaultIfEmpty()

                                  from settingStatus in _context.tbl_settingStatus
                                          .Where(x => x.StatusId == account.AccountStatusId).DefaultIfEmpty()

                                  select new Model.ViewModel.CompanyViewModel()
                                  {
                                      AccountId = account.AccountId,
                                      AccountName = account.AccountName,
                                      CurrentStatusId = account.CurrentStatusId,
                                      AccountRepId = account.AccountRepId,
                                      //AccountRep = acountRep.UserName,
                                      //IndustryType = industryType.IndustryType,
                                      CustomerSource = customerSource.CustomerSourceName,
                                      AccountRepOutside = account.AccountRepOutside,
                                      IsReferToLegal = account.IsReferToLegal,
                                      AccountType = account.AccountType,
                                      IsPartner = account.IsPartner,
                                      Description = account.Description,
                                      AccountImage = account.AccountImage,
                                      Website = account.Website,
                                      UserId = account.UserId,
                                      Fax = account.Fax,
                                      AnnualRevenueEst = account.AnnualRevenueEst,
                                      TaxId = account.TaxId,
                                      EmployeeCount = account.EmployeeCount,
                                      NoofDevices = account.NoofDevices,
                                      IndustryTypeId = account.IndustryTypeId,
                                      CustomerSourceId = account.CustomerSourceId,
                                      Activelines = account.Activelines,
                                      Phone = account.Phone,
                                      Email = account.Email,
                                      Lastpurcahse = account.Lastpurcahse,
                                      LastConnect = account.LastConnect,
                                      NextContactDate = account.NextContactDate,
                                      DateEntered = account.DateEntered,
                                      MainPhone = account.MainPhone,
                                      WebFormLink = account.WebFormLink,
                                      Carrier = carrierTittle != null ? carrierTittle.CarrierTitle : string.Empty,
                                      CarrierId = carrier != null ? carrier.CarrierId.Value : 0,
                                      //User = "Akshay",
                                      AccountOwnerId = account.AccountOwnerId,
                                      City = accountAddress != null ? accountAddress.CityName : string.Empty,
                                      //Street = CompanyAddress.Street,
                                      State = State != null ? State.StateName : string.Empty,
                                      ContactName = ContactName.FirstName + ContactName.LastName, // merging first name and last name
                                      AccountNumber = account.AccountNumber,
                                      Passcode = account.Passcode,
                                      //Zip = CompanyAddress.ZipPostalCode,
                                      AccountStatusId = settingStatus.StatusId,
                                      AccountStatusName = settingStatus.StatusTitle,
                                      Title = account.Title,
                                      AboutAccount = account.AboutAccount,
                                      Quality = account.Quality

                                  };
                var qry = (from account in accountList.AsEnumerable()
                           join acountRep in _userManager.Users on account.AccountRepId equals acountRep.Id into useraccount
                           from sub in useraccount.DefaultIfEmpty()


                           select new Model.ViewModel.CompanyViewModel()
                           {
                               AccountId = account.AccountId,
                               AccountName = account.AccountName,
                               CurrentStatusId = account.CurrentStatusId,
                               AccountRepId = account.AccountRepId,
                               AccountRep = sub?.UserName ?? string.Empty,
                               IndustryType = account.IndustryType,
                               CustomerSource = account.CustomerSource,
                               AccountRepOutside = account.AccountRepOutside,
                               IsReferToLegal = account.IsReferToLegal,
                               AccountType = account.AccountType,
                               IsPartner = account.IsPartner,
                               Description = account.Description,
                               AccountImage = account.AccountImage,
                               Website = account.Website,
                               UserId = account.UserId,
                               Fax = account.Fax,
                               AnnualRevenueEst = account.AnnualRevenueEst,
                               TaxId = account.TaxId,
                               EmployeeCount = account.EmployeeCount,
                               NoofDevices = account.NoofDevices,
                               IndustryTypeId = account.IndustryTypeId,
                               CustomerSourceId = account.CustomerSourceId,
                               Activelines = account.Activelines,
                               Phone = account.Phone,
                               Email = account.Email,
                               Lastpurcahse = account.Lastpurcahse,
                               LastConnect = account.LastConnect,
                               NextContactDate = account.NextContactDate,
                               DateEntered = account.DateEntered,
                               MainPhone = account.MainPhone,
                               WebFormLink = account.WebFormLink,
                               Carrier = account.Carrier,
                               CarrierId = account.CarrierId,

                               AccountOwnerId = account.AccountOwnerId,
                               City = account.City,
                               Street = account.Street,
                               State = account.State,
                               ContactName = account.ContactName, // merging first name and last name
                               AccountNumber = account.AccountNumber,
                               Passcode = account.Passcode,

                               Zip = account.Zip,
                               AccountStatusId = account.AccountStatusId,
                               AccountStatusName = account.AccountStatusName,
                               Title = account.Title,
                               AboutAccount = account.AboutAccount,
                               Quality = account.Quality,
                           });
                return qry.FirstOrDefault();


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<int[]> GetAccountCarrierIdAsync(int accountId)
        {
            try
            {
                var qry = _context.tbl_AccountCarrier.Where(x => x.AccountId == accountId && x.IsActive == true).Select(x => x.CarrierId);
                var carrierids = await qry.ToArrayAsync();
                return carrierids.OfType<int>().ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<tbl_AccountCarrier> GetAccountCarrierById(int accountId, int carrierId)
        {
            try
            {
                return await _context.tbl_AccountCarrier.Where(x => x.AccountId == accountId && x.CarrierId == carrierId).FirstOrDefaultAsync();
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

                var resultData = (from settingStatus in _context.tbl_settingStatus
                                  join source in _context.tbl_Source on settingStatus.SourceId equals source.SourceId
                                  where settingStatus.IsDeleted.Equals(false) && source.SourceEnum.Equals(ActivitySourceEnum.Account)
                                  select new SelectListItem
                                  {
                                      Text = settingStatus.StatusTitle,
                                      value = Convert.ToString(settingStatus.StatusId)
                                  });

                return resultData.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_AccountAddress> GetAccountAddressesById(int accountId)
        {
            try
            {
                return _context.tbl_AccountAddress.Where(x => x.AccountId == accountId).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeActivateAllCarrier(int accountId)
        {
            try
            {
                var carrierList = _context.tbl_AccountCarrier.Where(x => x.AccountId == accountId).ToList();
                carrierList.ForEach(x => x.IsActive = false);
                _context.tbl_AccountCarrier.UpdateRange(carrierList);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<bool> SaveOrUpdateAccount(tbl_Account account)
        {
            try
            {
                if (account.AccountId > 0)
                {
                    _context.tbl_Account.Update(account);
                }
                else
                {
                    _context.tbl_Account.Add(account);
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> SaveOrUpdateCarrier(List<tbl_AccountCarrier> carriers, bool isUpdated)
        {
            try
            {
                if (isUpdated)
                {
                    _context.tbl_AccountCarrier.UpdateRange(carriers);
                }
                else
                {
                    _context.tbl_AccountCarrier.AddRange(carriers);
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> SaveOrUpdateAddresses(List<tbl_AccountAddress> accountAddressList, bool isUpdated)
        {
            try
            {
                if (isUpdated)
                {
                    _context.tbl_AccountAddress.UpdateRange(accountAddressList);
                }
                else
                {
                    _context.tbl_AccountAddress.AddRange(accountAddressList);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<int> AddAccount(Model.ViewModel.CompanyViewModel model)
        {
            try
            {
                tbl_Account objAccount = new tbl_Account
                {
                    AccountId = model.AccountId,
                    AccountName = model.AccountName,
                    CurrentStatusId = model.CurrentStatusId,
                    AccountRepId = model.AccountRepId,
                    AccountRepOutside = model.AccountRepOutside,
                    IsReferToLegal = model.IsReferToLegal,
                    AccountType = model.AccountType,
                    IsPartner = model.IsPartner,
                    Description = model.Description,
                    AccountImage = model.AccountImage,
                    Website = model.Website,
                    UserId = model.UserId,
                    Fax = model.Fax,
                    AnnualRevenueEst = model.AnnualRevenueEst,
                    TaxId = model.TaxId,
                    EmployeeCount = model.EmployeeCount,
                    NoofDevices = model.NoofDevices,
                    IndustryTypeId = model.IndustryTypeId,
                    CustomerSourceId = model.CustomerSourceId,
                    Activelines = model.Activelines,
                    Lastpurcahse = model.Lastpurcahse,
                    Phone = model.Phone,
                    Email = model.Email,
                    LastConnect = model.LastConnect,
                    NextContactDate = model.NextContactDate,
                    DateEntered = model.DateEntered,
                    MainPhone = model.MainPhone,
                    WebFormLink = model.WebFormLink,
                    AccountOwnerId = model.AccountOwnerId,
                    AccountNumber = model.AccountNumber,
                    Passcode = model.Passcode

                    //CarrierId = model.CarrierId
                };

                tbl_AccountAddress objAccountAddress = new tbl_AccountAddress
                {
                    AccountId = model.AccountId,
                    Street = model.CompanyAddresses.Street,
                    // City = model.CompanyAddresses.City,
                    //State = model.CompanyAddresses.State,
                    AreaCode = model.CompanyAddresses.AreaCode,
                    ZipPostalCode = model.CompanyAddresses.ZipPostalCode


                };


                if (objAccount.AccountId == 0) { await _context.tbl_Account.AddAsync(objAccount); }
                else
                {
                    var entity = await _context.tbl_Account.FindAsync(model.AccountId);
                    if (entity != null)
                    {
                        entity.AccountId = model.AccountId;
                        entity.AccountName = model.AccountName;
                        entity.CurrentStatusId = model.CurrentStatusId;
                        entity.AccountRepId = model.AccountRepId;
                        entity.AccountRepOutside = model.AccountRepOutside;
                        entity.IsReferToLegal = model.IsReferToLegal;
                        entity.AccountType = model.AccountType;
                        entity.IsPartner = model.IsPartner;
                        entity.Description = model.Description;
                        entity.AccountImage = model.AccountImage;
                        entity.Website = model.Website;
                        entity.UserId = model.UserId;
                        entity.Fax = model.Fax;
                        entity.AnnualRevenueEst = model.AnnualRevenueEst;
                        entity.TaxId = model.TaxId;
                        entity.EmployeeCount = model.EmployeeCount;
                        entity.NoofDevices = model.NoofDevices;
                        entity.IndustryTypeId = model.IndustryTypeId;
                        entity.CustomerSourceId = model.CustomerSourceId;
                        entity.Activelines = model.Activelines;
                        entity.Phone = model.Phone;
                        entity.Email = model.Email;
                        entity.Lastpurcahse = model.Lastpurcahse;
                        entity.LastConnect = model.LastConnect;
                        entity.NextContactDate = model.NextContactDate;
                        entity.DateEntered = model.DateEntered;
                        entity.MainPhone = model.MainPhone;
                        entity.WebFormLink = model.WebFormLink;
                        entity.AccountOwnerId = model.AccountOwnerId;
                        //entity.CarrierId = model.CarrierId;
                        entity.AccountNumber = model.AccountNumber;
                        entity.Passcode = model.Passcode;


                        await _context.tbl_Account.AddAsync(entity);
                    }
                }
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Model.ViewModel.CompanyViewModel GetAccountInformation(int accountId)
        {
            try
            {
                var accountList = from account in _context.tbl_Account
                                         .Where(x => x.AccountId == accountId).DefaultIfEmpty()

                                  from activity in _context.tbl_Activity
                                  .Where(a => a.SourceId == account.AccountId).DefaultIfEmpty()

                                  from activitytype in _context.tbl_ActivityType
                                  .Where(a => a.ActivityId == activity.ActivityId).DefaultIfEmpty()

                                  from status in _context.tbl_settingStatus
                                 .Where(x => x.StatusId == activity.StatusId).DefaultIfEmpty()

                                  from source in _context.tbl_Source
                                  .Where(x => x.SourceId == activity.SourceId).DefaultIfEmpty()

                                  from industryType in _context.tbl_IndustryType
                                  .Where(x => x.IndustryTypeId == account.IndustryTypeId).DefaultIfEmpty()

                                  from customerSource in _context.tbl_CustomerSource
                                  .Where(o => o.CustomerSourceId == account.CustomerSourceId).DefaultIfEmpty()

                                      // from carrier in _context.Carriers
                                      //.Where(o => o.CarrierId == account.CarrierId).DefaultIfEmpty()

                                      //from acountRep in _userManager.Users
                                      //.Where(p => p.Id == account.AccountRepId).DefaultIfEmpty()

                                  select new Model.ViewModel.CompanyViewModel()
                                  {
                                      AccountId = account.AccountId,
                                      AccountName = account.AccountName,
                                      CurrentStatusId = account.CurrentStatusId,
                                      AccountRepId = account.AccountRepId,
                                      //AccountRep = acountRep.UserName,
                                      IndustryType = industryType.IndustryType,
                                      CustomerSource = customerSource.CustomerSourceName,
                                      AccountRepOutside = account.AccountRepOutside,
                                      IsReferToLegal = account.IsReferToLegal,
                                      AccountType = account.AccountType,
                                      IsPartner = account.IsPartner,
                                      Description = account.Description,
                                      AccountImage = account.AccountImage,
                                      Website = account.Website,
                                      UserId = account.UserId,
                                      Fax = account.Fax,
                                      AnnualRevenueEst = account.AnnualRevenueEst,
                                      TaxId = account.TaxId,
                                      EmployeeCount = account.EmployeeCount,
                                      NoofDevices = account.NoofDevices,
                                      IndustryTypeId = account.IndustryTypeId,
                                      CustomerSourceId = account.CustomerSourceId,
                                      Activelines = account.Activelines,
                                      Phone = account.Phone,
                                      Email = account.Email,
                                      Lastpurcahse = account.Lastpurcahse,
                                      LastConnect = account.LastConnect,
                                      NextContactDate = account.NextContactDate,
                                      DateEntered = account.DateEntered,
                                      MainPhone = account.MainPhone,
                                      WebFormLink = account.WebFormLink,
                                      //Carrier = carrier.CarrierTitle,
                                      //CarrierId = account.CarrierId,
                                      User = "Akshay",
                                      AccountOwnerId = account.AccountOwnerId,
                                      SourceName = source.Name,
                                      Status = status.StatusTitle,
                                      ActivityName = activitytype.Type
                                  };

                var AccountInformation = from account in accountList.AsEnumerable()
                                         join acountRep in _userManager.Users on account.AccountRepId equals acountRep.Id into useraccount
                                         from sub in useraccount.DefaultIfEmpty()

                                         select new Model.ViewModel.CompanyViewModel()
                                         {
                                             AccountId = account.AccountId,
                                             AccountName = account.AccountName,
                                             CurrentStatusId = account.CurrentStatusId,
                                             AccountRepId = account.AccountRepId,
                                             AccountRep = sub?.UserName ?? string.Empty,
                                             IndustryType = account.IndustryType,
                                             CustomerSource = account.CustomerSource,
                                             AccountRepOutside = account.AccountRepOutside,
                                             IsReferToLegal = account.IsReferToLegal,
                                             AccountType = account.AccountType,
                                             IsPartner = account.IsPartner,
                                             Description = account.Description,
                                             AccountImage = account.AccountImage,
                                             Website = account.Website,
                                             UserId = account.UserId,
                                             Fax = account.Fax,
                                             AnnualRevenueEst = account.AnnualRevenueEst,
                                             TaxId = account.TaxId,
                                             EmployeeCount = account.EmployeeCount,
                                             NoofDevices = account.NoofDevices,
                                             IndustryTypeId = account.IndustryTypeId,
                                             CustomerSourceId = account.CustomerSourceId,
                                             Activelines = account.Activelines,
                                             Phone = account.Phone,
                                             Email = account.Email,
                                             Lastpurcahse = account.Lastpurcahse,
                                             LastConnect = account.LastConnect,
                                             NextContactDate = account.NextContactDate,
                                             DateEntered = account.DateEntered,
                                             MainPhone = account.MainPhone,
                                             WebFormLink = account.WebFormLink,
                                             //Carrier = carrier.CarrierTitle,
                                             //CarrierId = account.CarrierId,

                                             AccountOwnerId = account.AccountOwnerId,
                                             SourceName = account.SourceName,
                                             Status = account.Status,
                                             ActivityName = account.ActivityName
                                         };

                //var AccountList = PagingList<Model.ViewModel.CompanyViewModel>.CreateAsync(qry, pageNo, pageSize, sortField, sortOrder);
                return AccountInformation.FirstOrDefault();


                //return await _context.tbl_Account.Where(x => x.Account == true).ToListAsync();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Task<List<tbl_Account>> GetAccounts()
        {
            throw new NotImplementedException();
        }
        public async Task<bool> IsAccountRepresentative(string Id)
        {
            return _userManager.Users.Where(x => x.Id == Id && x.Active == true).Select(x => x.IsAccountRep).FirstOrDefault();
        }
        public async Task<List<Model.ViewModel.CompanyViewModel>> GetGlobalFilerAccounts(string globalFilter)
        {

            try
            {
                var accountList = (from account in _context.tbl_Account.Where(a => a.IsActive == true && (a.AccountName.Contains(globalFilter)
                          || a.Email.Contains(globalFilter) || a.Phone.Contains(globalFilter) || globalFilter == null)
                          /*&& (companyViewModel.CarrierId == a.CarrierId || companyViewModel.CarrierId == null)*/)
                                   from industryType in _context.tbl_IndustryType
                                   .Where(x => x.IndustryTypeId == account.IndustryTypeId).DefaultIfEmpty()

                                   from customerSource in _context.tbl_CustomerSource
                                   .Where(o => o.CustomerSourceId == account.CustomerSourceId).DefaultIfEmpty()

                                   join carrier in _context.tbl_AccountCarrier on account.AccountId equals carrier.AccountId

                                   //from acountRep in userlist.to
                                   //.Where(p => p.Id == account.AccountRepId)

                                   select new Model.ViewModel.CompanyViewModel()
                                   {
                                       AccountId = account.AccountId,
                                       AccountName = account.AccountName,
                                       CurrentStatusId = account.CurrentStatusId,
                                       AccountRepId = account.AccountRepId,
                                       //AccountRep = acountRep.UserName,
                                       IndustryType = industryType.IndustryType,
                                       CustomerSource = customerSource.CustomerSourceName,
                                       AccountRepOutside = account.AccountRepOutside,
                                       IsReferToLegal = account.IsReferToLegal,
                                       AccountType = account.AccountType,
                                       IsPartner = account.IsPartner,
                                       Description = account.Description,
                                       AccountImage = account.AccountImage,
                                       Website = account.Website,
                                       UserId = account.UserId,
                                       Fax = account.Fax,
                                       AnnualRevenueEst = account.AnnualRevenueEst,
                                       TaxId = account.TaxId,
                                       EmployeeCount = account.EmployeeCount,
                                       NoofDevices = account.NoofDevices,
                                       IndustryTypeId = account.IndustryTypeId,
                                       CustomerSourceId = account.CustomerSourceId,
                                       Activelines = account.Activelines,
                                       Phone = account.Phone,
                                       Email = account.Email,
                                       Lastpurcahse = account.Lastpurcahse,
                                       LastConnect = account.LastConnect,
                                       NextContactDate = account.NextContactDate,
                                       DateEntered = account.DateEntered,
                                       MainPhone = account.MainPhone,
                                       CreatedDate = account.CreatedDate,
                                       WebFormLink = account.WebFormLink,
                                       AccountOwnerId = account.AccountOwnerId,
                                       //CarrierId = account.CarrierId,

                                       ModifiedDate = account.ModifiedDate



                                   }).Distinct();

                var qry = (from account in accountList.AsEnumerable()
                           join acountRep in _userManager.Users on account.AccountRepId equals acountRep.Id into useraccount
                           from sub in useraccount.DefaultIfEmpty()


                           select new Model.ViewModel.CompanyViewModel()
                           {
                               AccountId = account.AccountId,
                               AccountName = account.AccountName,
                               CurrentStatusId = account.CurrentStatusId,
                               AccountRepId = account.AccountRepId,
                               AccountRep = sub?.UserName ?? string.Empty,
                               IndustryType = account.IndustryType,
                               CustomerSource = account.CustomerSource,
                               AccountRepOutside = account.AccountRepOutside,
                               IsReferToLegal = account.IsReferToLegal,
                               AccountType = account.AccountType,
                               IsPartner = account.IsPartner,
                               Description = account.Description,
                               AccountImage = account.AccountImage,
                               Website = account.Website,
                               UserId = account.UserId,
                               Fax = account.Fax,
                               AnnualRevenueEst = account.AnnualRevenueEst,
                               TaxId = account.TaxId,
                               EmployeeCount = account.EmployeeCount,
                               NoofDevices = account.NoofDevices,
                               IndustryTypeId = account.IndustryTypeId,
                               CustomerSourceId = account.CustomerSourceId,
                               Activelines = account.Activelines,
                               Phone = account.Phone,
                               Email = account.Email,
                               Lastpurcahse = account.Lastpurcahse,
                               LastConnect = account.LastConnect,
                               NextContactDate = account.NextContactDate,
                               DateEntered = account.DateEntered,
                               MainPhone = account.MainPhone,
                               CreatedDate = account.CreatedDate,
                               WebFormLink = account.WebFormLink,
                               AccountOwnerId = account.AccountOwnerId,
                               //CarrierId = account.CarrierId,
                               ModifiedDate = account.ModifiedDate
                           });

                return qry.OrderBy(x => x.CreatedDate).ToList();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> SaveNextContactDate(DateTime? NextContactDate, int accountId)
        {
            try
            {
                var GetAccount = _context.tbl_Account.Where(x => x.AccountId == accountId).FirstOrDefault();
                if (GetAccount != null)
                {
                    GetAccount.NextContactDate = NextContactDate;
                    _context.tbl_Account.Update(GetAccount);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public int QuotationListCount()
        {

            var ticketcount = (from a in _context.Quotations.Where(x => x.IsActive == true)

                               select a.QuotationId).Count();
            return ticketcount;
        }

        public Task<bool> UpdateDevicestatus(string id, DateTime changedate)
        {
            try
            {
                var intids = id.Split(',').Select(int.Parse).ToList();

                var data = _context.tbl_AccountDeviceLease.Where(x => intids.Contains(x.AccountDeviceLeaseId)).ToList();


                data.ForEach(f =>
                {
                    f.LineStatusIsActive = false;
                    f.ReturnDate = changedate;
                });

                _context.UpdateRange(data);
                _context.SaveChanges();

                return null;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public dynamic GetExport(int AccountId)
        {
            try
            {

                var list = _context.Accountdevicelease.FromSqlInterpolated($"exec SP_GetAccountDeviceLeaseDetails {AccountId}").ToList();

                
                return list;
                
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public async Task<ShippingAddressViewModel> GetshippingAddres(int id)
        {
            ShippingAddressViewModel viewModel = new ShippingAddressViewModel();

            var shippingaddress = await (from Addres in _context.tbl_AccountAddress
                                         where Addres.AccountAddressId == id
                                         && Addres.AddressType == Convert.ToInt32(GlobalEnums.AddressType.Shipping)
                                         let city = _context.tbl_Cities.Where(w => w.Id == Addres.CityId).Select(s => s.CityName).FirstOrDefault()
                                         let state = _context.tbl_States.Where(w => w.Id == Addres.StateId).Select(s => s.StateCode).FirstOrDefault()
                                         select new ShippingAddressViewModel()
                                         {
                                             StreetAddress = Addres.Street,
                                             City = Addres.CityName,
                                             State = state,
                                             ZipCode = Addres.ZipPostalCode,
                                         }).FirstOrDefaultAsync();



            return shippingaddress;
        }
        public async Task<string> GetShippingServiceCode(int id)
        {
            try
            {
                var servicecode = await _context.ShipingMethod.Where(w => w.IsDeleted == false && w.ShippingMethodId == id).Select(s => s.ServiceCode).FirstOrDefaultAsync();

                return servicecode;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public bool AddRepairLabelData(AddClientRepairsLabelViewModel model, AddDeviceLeaseLabelViewModel devicemodel)
        {
            try
            {
                if (model != null)
                {
                    tblClientRepairsLabel addreplbl = new tblClientRepairsLabel()
                    {
                        DeviceRepId = model.DeviceRepId,
                        LabelResult = model.LabelResult,
                        LabelDate = model.LabelDate,
                        CreatedBy = model.CreatedBy,
                        CreatedOn = model.CreatedOn,
                        LabelType = model.LabelType,


                    };

                    _context.tblClientRepairsLabel.Add(addreplbl);
                    _context.SaveChanges();
                }
                if (devicemodel != null)
                    AddDeviceLeaseLabelData(devicemodel);
                return true;
            }
            catch (Exception ex)
            {
                //throw ex;
                return false;
            }

        }
        public bool AddDeviceLeaseLabelData(AddDeviceLeaseLabelViewModel model)
        {
            try
            {
                tbl_DeviceLeasePEmail addreplbl = new tbl_DeviceLeasePEmail()
                {
                    deviceLeaseId = model.deviceLeaseId,
                    AccountId = model.AccountId,
                    DocumentPath = model.DocumentPath,
                    EmailContent = model.EmailContent,
                    EmailSubject = model.EmailSubject,
                    EmailTo = model.EmailTo,
                    FileName = model.FileName,
                    ReturnLabelDate = model.ReturnLabelDate,
                    ReturnLabelResult = model.ReturnLabelResult,
                    ShippingTrackingNumber = model.ShippingTrackingNumber,
                    UniqueFileName = model.UniqueFileName,
                    CreatedBy = model.CreatedBy,
                    CreatedDate = model.CreatedDate,
                    ShippingAddressId = model.ShippingAddressId

                };

                _context.tbl_DeviceLeasePEmail.Add(addreplbl);
                _context.SaveChanges();

                DeviceLeasePEmail = addreplbl.ID;
                return true;
            }
            catch (Exception ex)
            {
                //throw ex;
                return false;
            }

        }
        public bool UpdateDeviceLeaseLabelData(AddDeviceLeaseLabelViewModel model)
        {
            try
            {

                if (DeviceLeasePEmail > 0)
                {
                    var data = _context.tbl_DeviceLeasePEmail.Where(x => x.ID == DeviceLeasePEmail).FirstOrDefault();

                    //tbl_DeviceLeasePEmail addreplbl = new tbl_DeviceLeasePEmail()
                    //{
                    //    deviceLeaseId = model.deviceLeaseId,
                    //    AccountId = model.AccountId,
                    //    DocumentPath = model.DocumentPath,
                    //    EmailContent = model.EmailContent,
                    //    EmailSubject = model.EmailSubject,
                    //    EmailTo = model.EmailTo,
                    //    FileName = model.FileName,
                    //    ReturnLabelDate = model.ReturnLabelDate,
                    //    ReturnLabelResult = model.ReturnLabelResult,
                    //    ShippingTrackingNumber = model.ShippingTrackingNumber,
                    //    UniqueFileName = model.UniqueFileName,
                    //    CreatedBy = model.CreatedBy,
                    //    CreatedDate = model.CreatedDate,

                    //};
                    data.EmailTo = model.EmailTo;
                    data.EmailContent = model.EmailContent;
                    data.EmailSubject = model.EmailSubject;
                    _context.tbl_DeviceLeasePEmail.Update(data);
                    _context.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                //throw ex;
                return false;
            }

        }

        public PagingList<AddDeviceLeaseLabeResponse> GetDeviceLeasePreviousData(int AccountId, int pageNo, int pageSize, string sortField, string sortOrder)
        {
            AddDeviceLeaseLabeResponse viewModel = new AddDeviceLeaseLabeResponse();

            var Data = (from device in _context.tbl_DeviceLeasePEmail
                        where device.AccountId == AccountId
                        select new AddDeviceLeaseLabeResponse()
                        {
                            Id = device.ID,
                            deviceLeaseId = device.deviceLeaseId,
                            ShippingTrackingNumber = device.ShippingTrackingNumber,
                            ReturnLabelDate = device.ReturnLabelDate!=null? device.ReturnLabelDate:null,
                            Error = device.ReturnLabelResult,
                            Tomail = device.EmailTo,
                            LabelLink = device.DocumentPath != null ? device.DocumentPath : "",
                        }).AsEnumerable()
                               .Select((a, serialNumber) => new AddDeviceLeaseLabeResponse()
                               {
                                   Id = a.Id,
                                   deviceLeaseId = a.deviceLeaseId,
                                   ShippingTrackingNumber = a.ShippingTrackingNumber,
                                   ReturnLabelDate = a.ReturnLabelDate,
                                   Error = a.Error,
                                   Tomail = a.Tomail,
                                   LabelLink = a.LabelLink,
                                   SerialNumber = serialNumber + 1
                               }).ToList();
            viewModel.TotalCount = Data.Count();

            var requestList = PagingList<AddDeviceLeaseLabeResponse>.CreateAsync(Data, pageNo, pageSize, sortField, sortOrder);
            return requestList;
        }

        public tbl_DeviceLeasePEmail GetDeviceLeaseLabelData(int id)
        {

            var leasedata = _context.tbl_DeviceLeasePEmail.Where(w => w.ID == id).FirstOrDefault();

            return leasedata;

        }

        public async Task<bool> GetAccountName(string AccountName)
        {
            try
            {
                var value = await _context.tbl_Account.Where(x => x.AccountName == AccountName).FirstOrDefaultAsync();
                if (value == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
