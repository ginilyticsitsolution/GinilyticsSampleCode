using FinnlyS.Client.State;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinnlyS.Client.Services.Lists;
using FinnlyS.Client.Services.Registration;
using FinnlyS.Shared.Lists;
using FinnlyS.Shared.Registration;
using FinnlyS.Shared;
using System.Collections.ObjectModel;
using Telerik.Blazor.Components;
using FinnlyS.Client.Services.Site;
using FinnlyS.Shared.Site;
using FinnlyS.Client.Services.Account;
using FinnlyS.Shared.Account;
using FinnlyS.Client.Services.Financial;
using FinnlyS.Shared.Financial;
using FinnlyS.Client.Services.Facility;
using FinnlyS.Shared.Facility;
using FinnlyS.Shared.Event;
using FinnlyS.Shared.Helper;

namespace FinnlyS.Client.Pages.Registration
{
    public class ActivityBase : ComponentBase
    {
        [Inject] public cGlobal _global { get; set; }
        [Inject] public iACT_SiteResidentTypeService _residentTypeService { get; set; }
        [Inject] public iACT_SiteSkillLevelService _siteSkillLevelService { get; set; }
        [Inject] public iACT_SiteSkillLevelValueService _siteSkillLevelValueService { get; set; }
        [Inject] public iFAC_FacilityService _facilityService { get; set; }
        [Inject] public iFAC_EventTypeService _eventTypeService { get; set; }
        [Inject] public iFIN_FeeService _feeService { get; set; }
        [Inject] public iFIN_ItemService _itemService { get; set; }
        [Inject] public iFIN_TaxLocationService _taxLocationService { get; set; }
        [Inject] public iLST_AccrualEarnedOptionService _accrualEarnedOptionService { get; set; }
        [Inject] public iLST_ActivityAdditionalFeeTypeService _activityAdditionalFeeTypeService { get; set; }
        [Inject] public iLST_ActivityDiscountTypeService _activityDiscountTypeService { get; set; }
        [Inject] public iLST_ActivityFeeTypeService _activityFeeTypeService { get; set; }
        [Inject] public iLST_SkillMatchService _skillMatchService { get; set; }
        [Inject] public iREG_ActivityGroupService _activityGroupService { get; set; }
        [Inject] public iLST_ActivityRegistrationTypeService _activityRegistrationTypeService { get; set; }
        [Inject] public iREG_ActivityService _activityService { get; set; }
        [Inject] public iREG_ActivityTypeService _activityTypeService { get; set; }
        [Inject] public iSIT_GenderService _genderService { get; set; }
        [Inject] public iFIN_PaymentMethodService _paymentMethodService { get; set; }
        [Inject] public iREG_SeasonService _seasonService { get; set; }
        //For Create Duplicate Activity
        [Inject] public iACT_AccountService _accountService { get; set; }
        //[Inject] public iREG_ActivityService _activityService { get; set; }
        [Inject] public iREG_ActivityAccrualScheduleService _activityAccrualScheduleService { get; set; }
        [Inject] public iREG_ActivityAdditionalFeeService _activityAdditionalFeeService { get; set; }
        [Inject] public iREG_ActivityCustomQuestionService _activityCustomQuestionService { get; set; }
        [Inject] public iREG_ActivityFeeService _activityFeeService { get; set; }
        [Inject] public iREG_ActivityDiscountService _activityDiscountService { get; set; }
        [Inject] public iREG_ActivityPrerequisiteService _activityPrerequisiteService { get; set; }
        [Inject] public iREG_SessionService _sessionService { get; set; }
        [Inject] public iREG_SessionEventService _sessionEventService { get; set; }
        [Inject] public iREG_RegistrationService _registrationService { get; set; }
        [Inject] public iREG_ActivityContractService _activityContractService { get; set; }


        protected ErrorNotification ErrorNotificationRef { get; set; }
        protected bool Loading { get; set; } = true;
        public List<cACT_SiteResidentTypeModel> ResidentTypeList { get; set; }
        public List<cACT_SiteSkillLevelModel> SkillLevelList { get; set; }
        public List<cACT_SiteSkillLevelValueModel> SkillValueList { get; set; }
        public List<cFAC_FacilityModel> FacilityList { get; set; }
        public List<cFAC_EventTypeModel> EventTypeList { get; set; }
        public List<cFIN_FeeModel> FeeList { get; set; }
        public List<cFIN_ItemModel> ItemList { get; set; }
        public List<cFIN_TaxLocationModel> TaxLocationList { get; set; }
        public List<cLST_AccrualEarnedOptionModel> AccrualEarnedOptionList { get; set; }
        public List<cLST_ActivityAdditionalFeeTypeModel> ActivityAdditionalFeeTypeList { get; set; }
        public List<cLST_ActivityDiscountTypeModel> ActivityDiscountTypeList { get; set; }
        public List<cLST_ActivityFeeTypeModel> ActivityFeeTypeList { get; set; }
        public List<cLST_SkillMatchModel> SkillMatchList { get; set; }
        public List<cREG_ActivityGroupModel> ActivityGroupList { get; set; }
        public List<cREG_SeasonModel> ActivitySeasonList { get; set; }
        protected cACT_AddressModel GuestAddress { get; set; } = new cACT_AddressModel();
        public List<cLST_ActivityRegistrationTypeModel> ActivityRegistrationTypeList { get; set; }
        private List<cREG_ActivityModel> _activityList;
        public List<cREG_ActivityTypeModel> ActivityTypeList { get; set; }
        public List<cSIT_GenderModel> GenderList { get; set; }
        public ObservableCollection<cREG_ActivityModel> ObsActivityList { get; set; } = new ObservableCollection<cREG_ActivityModel>();
        public cREG_ActivityModel WorkingActivity { get; set; }
        public cREG_ActivityModel DeleteActivityItem { get; set; }
        public cREG_ActivityModel DuplicateActivityItem { get; set; }
        public bool ShowDuplicate { get; set; }
        public bool ShowDuplicateMessage { get; set; }
        protected bool ShowActivityConflicts { get; set; }
        public bool ShowDeleteConfirm { get; set; }
        public bool ShowEdit { get; set; }
        public bool CanEdit { get; set; }
        public bool ShowSessionInfo { get; set; }
        protected bool ShowCheckin { get; set; }
        public string Action { get; set; }
        protected List<cREG_RegistrationModel> RegistrationList { get; set; } = new List<cREG_RegistrationModel>();
        protected bool ShowSell { get; set; }
        protected bool ShowPromote { get; set; }
        protected long? SelectedActivityFee { get; set; }
        protected List<cFIN_PaymentMethodModel> PaymentMethodList { get; set; }
        protected long? FilterActivitySeasonId { get; set; }
        protected long? FilterActivityTypeId { get; set; }
        protected DateTime FilterStartDate { get; set; } = DateTime.Now;
        protected DateTime FilterEndDate { get; set; } = DateTime.Now.AddMonths(3);

        //TODO List For Duplicate.
        protected List<cREG_SessionModel> SessionList { get; set; }
        protected List<cREG_SessionEventModel> SessionEventList { get; set; }
        protected List<cAAA_ConflictItem> RecurringConflictList { get; set; }
        protected List<cREG_ActivityCustomQuestionModel> ActivityCustomQuestionList { get; set; }
        protected List<cREG_ActivityFeeModel> ActivityFeeList { get; set; }
        protected ObservableCollection<cREG_ActivityFeeModel> ObsActivityFeeList { get; set; } = new ObservableCollection<cREG_ActivityFeeModel>();
        protected List<cREG_ActivityDiscountModel> ActivityDiscountList { get; set; }
        protected List<cREG_ActivityPrerequisiteModel> PrequisiteList { get; set; }
        protected List<cREG_ActivityContractModel> ContractList { get; set; }
        protected ObservableCollection<cREG_ActivityDiscountModel> ObsActivityDiscountList { get; set; } = new ObservableCollection<cREG_ActivityDiscountModel>();
        protected List<cREG_ActivityAdditionalFeeModel> ActivityAdditionalFeeList { get; set; }
        protected ObservableCollection<cREG_ActivityAdditionalFeeModel> ObsActivityAdditionalFeeList { get; set; } = new ObservableCollection<cREG_ActivityAdditionalFeeModel>();
        protected List<cREG_ActivityAccrualScheduleModel> ActivityAccrualScheduleList { get; set; }
        protected ObservableCollection<cREG_ActivityAccrualScheduleModel> ObsActivityAccrualScheduleList { get; set; } = new ObservableCollection<cREG_ActivityAccrualScheduleModel>();
        protected List<cREG_RegistrationModel> StoredRegistrationList { get; set; }
        public static bool IsDuplicateResultSuccess { get; set; }

        //Todo List for SessionEvent 
        protected bool SelectSessionWeek { get; set; } = false;
        public int result;
        protected int DdlValue { get; set; }
        public class MyDdlModel
        {
            public int MyValueField { get; set; }
            public string MyTextField { get; set; }
        }

        public class GetSessionDateModel
        {
            public DateTime SessionStartDate { get; set; }
            public DateTime SessionEndDate { get; set; }
        }
        public List<GetSessionDateModel> getSessionDateList { get; set; } = new List<GetSessionDateModel>();
        public IEnumerable<MyDdlModel> myDdlData = Enumerable.Range(1, 60).Select(x => new MyDdlModel { MyTextField = " " + x, MyValueField = x });
        public static string ActivityStartDate { get; set; }
        public static string ActivityEndDate { get; set; }
        //
        //Todo End 

        protected override async Task OnInitializedAsync()
        {
            IsDuplicateResultSuccess = false;
            if (_global.FinnlyAdmin || _global.FacilitySiteAdminIdList.Contains(_global.SiteId.Value))
            {
                CanEdit = true;
            }
            else
            {
                CanEdit = false;
            }
            Task<cActionResult<List<cACT_SiteResidentTypeModel>>> residencyTask = _residentTypeService.GetList(_global.SiteId.Value);
            Task<cActionResult<List<cACT_SiteSkillLevelModel>>> skillLevelTask = _siteSkillLevelService.GetList(_global.SiteId.Value);
            Task<cActionResult<List<cACT_SiteSkillLevelValueModel>>> skillLevelValueTask = _siteSkillLevelValueService.GetList(_global.SiteId.Value);
            Task<cActionResult<List<cFAC_FacilityModel>>> facilityTask = _facilityService.GetListBySiteId(_global.SiteId.Value);
            Task<cActionResult<List<cFAC_EventTypeModel>>> eventTypeTask = _eventTypeService.GetList(_global.SiteId.Value);
            Task<cActionResult<List<cFIN_FeeModel>>> feeTask = _feeService.GetList(_global.SiteId.Value);
            Task<cActionResult<List<cFIN_ItemModel>>> itemTask = _itemService.GetList(_global.SiteId.Value);
            Task<cActionResult<List<cFIN_TaxLocationModel>>> taxLocationTask = _taxLocationService.GetList(_global.SiteId.Value);
            Task<cActionResult<List<cLST_AccrualEarnedOptionModel>>> accrualOptionTask = _accrualEarnedOptionService.GetList();
            Task<cActionResult<List<cLST_ActivityAdditionalFeeTypeModel>>> additionalFeeTypeTask = _activityAdditionalFeeTypeService.GetList();
            Task<cActionResult<List<cLST_SkillMatchModel>>> skillMatchTask = _skillMatchService.GetList();
            Task<cActionResult<List<cLST_ActivityDiscountTypeModel>>> discountTask = _activityDiscountTypeService.GetList();
            Task<cActionResult<List<cLST_ActivityFeeTypeModel>>> feeTypeTask = _activityFeeTypeService.GetList();
            Task<cActionResult<List<cREG_ActivityGroupModel>>> activityGroupTask = _activityGroupService.GetList(_global.SiteId.Value);
            Task<cActionResult<List<cREG_SeasonModel>>> activitySeasonTask = _seasonService.GetList(_global.SiteId.Value);
            Task<cActionResult<List<cLST_ActivityRegistrationTypeModel>>> regTypeTask = _activityRegistrationTypeService.GetList();
            Task<cActionResult<List<cREG_ActivityTypeModel>>> activityTypeTask = _activityTypeService.GetList(_global.SiteId.Value);
            Task<cActionResult<List<cSIT_GenderModel>>> genderTask = _genderService.GetList(_global.SiteId.Value);
            Task<cActionResult<List<cFIN_PaymentMethodModel>>> paymentMethodTask = _paymentMethodService.GetList(_global.SiteId.Value);
            await Task.WhenAll(residencyTask,
                               skillLevelTask,
                               skillLevelValueTask,
                               facilityTask,
                               eventTypeTask,
                               feeTask,
                               itemTask,
                               taxLocationTask,
                               accrualOptionTask,
                               additionalFeeTypeTask,
                               skillMatchTask,
                               discountTask,
                               feeTypeTask,
                               activityGroupTask,
                               activitySeasonTask,
                               regTypeTask,
                               activityTypeTask,
                               genderTask,
                               paymentMethodTask);
            if (!residencyTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the residency list.");
                return;
            }
            if (!skillLevelTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the skill level list.");
                return;
            }
            if (!skillLevelValueTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the skill value list.");
                return;
            }
            if (!facilityTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the facility list.");
                return;
            }
            if (!eventTypeTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the event type list.");
                return;
            }
            if (!feeTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the fee list.");
                return;
            }
            if (!itemTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the item list.");
                return;
            }
            if (!taxLocationTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the tax location list.");
                return;
            }
            if (!accrualOptionTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the accrual earned option list.");
                return;
            }
            if (!additionalFeeTypeTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the activity additional fee list.");
                return;
            }
            if (!skillMatchTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the activity additional fee list.");
                return;
            }
            if (!discountTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the activity discount list.");
                return;
            }
            if (!feeTypeTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the activity fee list.");
                return;
            }
            if (!activityGroupTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the activity group list.");
                return;
            }
            if (!activitySeasonTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the activity season list.");
                return;
            }
            if (!regTypeTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the activity registration type list.");
                return;
            }
            if (!activityTypeTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the activity type list.");
                return;
            }
            if (!genderTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the gender list.");
                return;
            }
            if (!paymentMethodTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the payment method list.");
                return;
            }
            ResidentTypeList = residencyTask.Result.Result;
            SkillLevelList = skillLevelTask.Result.Result;
            SkillValueList = skillLevelValueTask.Result.Result;
            FacilityList = facilityTask.Result.Result;
            EventTypeList = eventTypeTask.Result.Result;
            FeeList = feeTask.Result.Result;
            ItemList = itemTask.Result.Result;
            TaxLocationList = taxLocationTask.Result.Result;
            AccrualEarnedOptionList = accrualOptionTask.Result.Result;
            ActivityAdditionalFeeTypeList = additionalFeeTypeTask.Result.Result;
            SkillMatchList = skillMatchTask.Result.Result;
            ActivityDiscountTypeList = discountTask.Result.Result;
            ActivityFeeTypeList = feeTypeTask.Result.Result;
            ActivityGroupList = activityGroupTask.Result.Result;
            ActivitySeasonList = activitySeasonTask.Result.Result;
            ActivityRegistrationTypeList = regTypeTask.Result.Result;
            ActivityTypeList = activityTypeTask.Result.Result;
            GenderList = genderTask.Result.Result;
            PaymentMethodList = paymentMethodTask.Result.Result;

            bool activitySuccess = await LoadActivity();
            if (activitySuccess)
            {
                Loading = false;
                StateHasChanged();
            }
        }

        private async Task<bool> LoadActivity()
        {
            cActionResult<List<cREG_ActivityModel>> activityAction = await _activityService.GetList(_global.SiteId.Value);
            if (!activityAction.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the activity list.");
                return false;
            }
            _activityList = activityAction.Result;
            ObsActivityList.Clear();
            foreach (cREG_ActivityModel activity in _activityList)
            {
                AssignDisplays(activity);
            }
            ApplyFilter();
            return true;
        }

        private void AssignDisplays(cREG_ActivityModel Activity)
        {
            cLST_ActivityRegistrationTypeModel regitrationType = ActivityRegistrationTypeList.FirstOrDefault(item => item.ActivityRegistrationTypeId == Activity.ActivityRegistrationTypeId);
            Activity.DisplayActivityRegistrationType = regitrationType != null ? regitrationType.Name : "";
            cFAC_FacilityModel facility = FacilityList.FirstOrDefault(item => item.FacilityId == (Activity.DefaultFacilityId ?? 0));
            Activity.DisplayFacility = facility != null ? facility.FacilityName : Activity.DefaultLocationText;
            var activityType = ActivityTypeList.FirstOrDefault(item => item.ActivityTypeId == Activity.ActivityTypeId);
            Activity.DisplayActivityType = activityType != null ? activityType.Name : "";
        }

        // *********************************
        // Filters
        // *********************************
        protected void FilterActivityTypeChanged()
        {
            ApplyFilter();
        }

        protected void FilterStartDateChanged()
        {
            if (FilterStartDate > FilterEndDate)
            {
                FilterEndDate = FilterStartDate;
            }
            ApplyFilter();
        }

        protected void FilterEndDateChanged()
        {
            if (FilterEndDate < FilterStartDate)
            {
                FilterStartDate = FilterEndDate;
            }
            ApplyFilter();
        }

        protected void ApplyFilter()
        {
            ObsActivityList.Clear();
            foreach (cREG_ActivityModel activity in _activityList)
            {
                bool goodItem = true;
                if (FilterActivitySeasonId.HasValue && activity.ActivitySeasonId != FilterActivitySeasonId.Value)
                {
                    goodItem = false;
                }
                if (FilterActivityTypeId.HasValue && activity.ActivityTypeId != FilterActivityTypeId.Value)
                {
                    goodItem = false;
                }
                if (activity.ActivityStartDate > FilterEndDate || activity.ActivityEndDate < FilterStartDate)
                {
                    goodItem = false;
                }
                if (goodItem)
                {
                    ObsActivityList.Add(activity);
                }
            }
        }
        protected void CreateActivity()
        {

            WorkingActivity = new cREG_ActivityModel()
            {
                ActivityId = -1,
                AccrualEarnedOptionId = 1,
                ActivityStartDate = null,
                ActivityEndDate = null,
                ActivityGroupId = null,
                ActivityRegistrationTypeId = 1,
                ActivityTypeId = 0,
                Code = "",
                DefaultFacilityId = null,
                Description = "",
                GenderId = null,
                MinBirthDate = null,
                MaxBirthDate = null,
                MaxEnroll = null,
                Name = "",
                OnlineViewStartDate = null,
                OnlineViewEndDate = null,
                PurchaseStartDate = null,
                PurchaseEndDate = null,
                OnlinePurchaseStartDate = null,
                OnlinePurchaseEndDate = null,
                SiteId = _global.SiteId.Value
            };
            Action = "Add";
            ShowEdit = true;
            _global.IsVisible = false;
        }

        protected void EditActivity(GridCommandEventArgs args)
        {
            WorkingActivity = (cREG_ActivityModel)args.Item;
            Action = "Edit";
            ShowEdit = true;
            _global.IsVisible = true;
        }

        protected void EditCompleted(bool Save)
        {
            if (IsDuplicateResultSuccess)
            {
                DuplicateCompleted(true);
            }
            else
            {
                AssignDisplays(WorkingActivity);
                if (Save && Action == "Add")
                {
                    _activityList.Add(WorkingActivity);
                    ObsActivityList.Add(WorkingActivity);
                }

                ShowEdit = false;
            }
        }

        protected void SetupActivityDelete(GridCommandEventArgs args)
        {
            DeleteActivityItem = (cREG_ActivityModel)args.Item;
            ShowDeleteConfirm = true;
        }

        protected async void DeleteActivity()
        {
            cActionResult<bool> deleteAction = await _activityService.Delete(DeleteActivityItem);
            if (deleteAction.Success)
            {
                _activityList.Remove(DeleteActivityItem);
                ObsActivityList.Remove(DeleteActivityItem);
            }
            ShowDeleteConfirm = false;
            StateHasChanged();
        }

        protected void SetupActivityDuplicate(GridCommandEventArgs args)
        {
            DuplicateActivityItem = (cREG_ActivityModel)args.Item;
            ShowDuplicate = true;
        }

        protected async void DuplicateActivity()
        {
            //DuplicateActivityItem = (cREG_ActivityModel)args.Item;
            Action = "Duplicate";
            ShowDuplicate = false;
            ShowDuplicateMessage = true;
            SelectSessionWeek = false;
            //TODO : Create Duplicate Activity.
            Task<cActionResult<List<cREG_ActivityFeeModel>>> feeTask = _activityFeeService.GetList(DuplicateActivityItem.ActivityId);
            Task<cActionResult<List<cREG_ActivityCustomQuestionModel>>> customQuestionTask = _activityCustomQuestionService.GetList(DuplicateActivityItem.ActivityId);
            Task<cActionResult<List<cREG_ActivityDiscountModel>>> discountTask = _activityDiscountService.GetList(DuplicateActivityItem.ActivityId);
            Task<cActionResult<List<cREG_ActivityPrerequisiteModel>>> prereqTask = _activityPrerequisiteService.GetListByActivityId(DuplicateActivityItem.ActivityId);
            Task<cActionResult<List<cREG_ActivityAdditionalFeeModel>>> additionalFeeTask = _activityAdditionalFeeService.GetList(DuplicateActivityItem.ActivityId);
            Task<cActionResult<List<cREG_ActivityAccrualScheduleModel>>> accrualScheduleOptionTask = _activityAccrualScheduleService.GetList(DuplicateActivityItem.ActivityId);
            Task<cActionResult<List<cREG_SessionModel>>> sessionTask = _sessionService.GetList(DuplicateActivityItem.ActivityId);
            Task<cActionResult<List<cREG_SessionEventModel>>> sessionEventTask = _sessionEventService.GetList(DuplicateActivityItem.ActivityId);
            Task<cActionResult<cACT_AccountModel>> accountTask = _accountService.Get(DuplicateActivityItem.AccountId);
            Task<cActionResult<List<cREG_RegistrationModel>>> registrationTask = _registrationService.GetList(DuplicateActivityItem.ActivityId);
            Task<cActionResult<List<cREG_ActivityContractModel>>> contractTask = _activityContractService.GetList(DuplicateActivityItem.ActivityId);
            await Task.WhenAll(feeTask,
                               customQuestionTask,
                               discountTask,
                               prereqTask,
                               additionalFeeTask,
                               accrualScheduleOptionTask,
                               sessionTask,
                               sessionEventTask,
                               accountTask,
                               registrationTask,
                               contractTask);
            if (!feeTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the activity fee list.");
                return;
            }
            if (!customQuestionTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the question list.");
                return;
            }
            if (!discountTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the activity discount list.");
                return;
            }
            if (!prereqTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the prerequisites list.");
                return;
            }
            if (!contractTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the activity contract list.");
                return;
            }
            if (!additionalFeeTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the activity additional fee list.");
                return;
            }
            if (!accrualScheduleOptionTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the activity accrual schedule option list.");
                return;
            }
            if (!sessionTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the session list.");
                return;
            }
            if (!sessionEventTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the session event list.");
                return;
            }
            if (!accountTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the account.");
                return;
            }
            if (!registrationTask.Result.Success)
            {
                ErrorNotificationRef.ShowError("Failed to load the registration list.");
                return;
            }
            DuplicateActivityItem.DisplayAccount = accountTask.Result.Result.AccountName;
            ActivityCustomQuestionList = customQuestionTask.Result.Result;
            foreach (cREG_ActivityCustomQuestionModel question in ActivityCustomQuestionList)
            {
                question.ActivityCustomQuestionId = -1;
            }
            ActivityFeeList = feeTask.Result.Result;
            foreach (cREG_ActivityFeeModel fee in ActivityFeeList)
            {
                fee.ActivityFeeId = -1;
                if (fee.SessionId == null)
                    fee.Old_SessionId = null;
                else
                {
                    fee.Old_SessionId = fee.SessionId;
                }
            }

            ActivityDiscountList = discountTask.Result.Result;
            foreach (cREG_ActivityDiscountModel discount in ActivityDiscountList)
            {
                discount.ActivityDiscountId = -1;
                if (discount.SessionId == null)
                    discount.Old_SessionId = null;
                else
                {
                    discount.Old_SessionId = discount.SessionId;
                }
            }

            ActivityAdditionalFeeList = additionalFeeTask.Result.Result;
            foreach (cREG_ActivityAdditionalFeeModel addFee in ActivityAdditionalFeeList)
            {
                addFee.ActivityAdditionalFeeId = -1;
                if (addFee.SessionId == null)
                    addFee.Old_SessionId = null;
                else
                {
                    addFee.Old_SessionId = addFee.SessionId;
                }
            }
            ActivityAccrualScheduleList = accrualScheduleOptionTask.Result.Result;
            foreach (cREG_ActivityAccrualScheduleModel schedule in ActivityAccrualScheduleList)
            {
                schedule.ActivityAccrualScheduleId = -1;
            }
            PrequisiteList = prereqTask.Result.Result;
            foreach (cREG_ActivityPrerequisiteModel prerequisite in PrequisiteList)
            {
                prerequisite.ActivityPrerequisiteId = -1;
            }
            ContractList = contractTask.Result.Result;
            foreach (cREG_ActivityContractModel contract in ContractList)
            {
                contract.ActivityContractId = -1;
            }
            SessionEventList = sessionEventTask.Result.Result;
            StoredRegistrationList = registrationTask.Result.Result;
            //CalculatePercentage();
            SessionList = sessionTask.Result.Result;
            if (SessionList.Count > 0)
            {
                ActivityStartDate = Convert.ToString(DuplicateActivityItem.ActivityStartDate);
                ActivityEndDate = Convert.ToString(DuplicateActivityItem.ActivityEndDate);
                ShowDuplicateMessage = false;
                SelectSessionWeek = true;
                StateHasChanged();
                return;
            }
            else
            {
                foreach (cREG_SessionModel session in SessionList)
                {
                    session.Old_SessionId = session.SessionId;
                    session.SessionId = -1;
                }

                foreach (cREG_SessionEventModel sessionEvent in SessionEventList)
                {
                    sessionEvent.SessionEventId = -1;
                    //sessionEvent.SessionId = -1;
                }

                foreach (cREG_RegistrationModel registration in StoredRegistrationList)
                {
                    registration.RegistrationId = -1;
                }
                DuplicateActivityItem.ActivityId = -1;
                SaveDuplicateActivity(true);
            }
        }

        protected async void SaveDuplicateActivity(bool Save)
        {
            var savePayload = new cREG_ActivitySave()
            {
                Activity = DuplicateActivityItem,
                FeeList = ActivityFeeList,
                DiscountList = ActivityDiscountList,
                AdditionalFeeList = ActivityAdditionalFeeList,
                AccrualScheduleList = ActivityAccrualScheduleList,
                SessionList = SessionList,
                SessionEventList = SessionEventList,
                PrerequisiteList = PrequisiteList,
                ContractList = ContractList,
                ActivityCustomQuestionList = ActivityCustomQuestionList
            };
            //TODO : Add DateTime For Copy Data.
            savePayload.Activity.Name = DuplicateActivityItem.Name + "_" + System.DateTime.Now.ToString();
            //
            cActionResult<cREG_ActivitySaveResult> saveAction = await _activityService.Copy(savePayload);
            if (!saveAction.Success)
            {
                ErrorNotificationRef.ShowError("Save failed.");
                ShowDuplicateMessage = false;
                ShowDuplicate = false;
                getSessionDateList = new List<GetSessionDateModel>();
                StateHasChanged();
                return;
            }
            //else
            //StateHasChanged();
            if (!saveAction.Result.Success)
            {
                RecurringConflictList = saveAction.Result.ConflictList;
                foreach (cAAA_ConflictItem conflict in RecurringConflictList)
                {
                    var sourceFacility = FacilityList.FirstOrDefault(item => item.FacilityId == conflict.SourceEvent.FacilityId);
                    var conflictFacility = FacilityList.FirstOrDefault(item => item.FacilityId == conflict.ConflictEvent.FacilityId);
                    conflict.SourceEvent.DisplayFacilityName = sourceFacility != null ? sourceFacility.FacilityName : "";
                    conflict.ConflictEvent.DisplayFacilityName = conflictFacility != null ? conflictFacility.FacilityName : "";
                }
                ShowActivityConflicts = true;
                ShowDuplicateMessage = false;
                SelectSessionWeek = false;
                ShowDuplicate = false;
                getSessionDateList = new List<GetSessionDateModel>();
                StateHasChanged();
                //return;
            }
            await DuplicateCompleted(true);
        }

        public void GetSelectedSessionWeek()
        {
            ShowDuplicateMessage = false;
            // extract the data item from the data source by using the value
            MyDdlModel selectedItem = myDdlData.Where(d => d.MyValueField == DdlValue).FirstOrDefault();
            if (selectedItem != null)
            {
                result = selectedItem.MyValueField;
                var days = (result * 7);

                if (DuplicateActivityItem.ActivityStartDate != null)
                    DuplicateActivityItem.ActivityStartDate = Convert.ToDateTime(DuplicateActivityItem.ActivityStartDate).AddDays(days);

                if (DuplicateActivityItem.ActivityEndDate != null)
                    DuplicateActivityItem.ActivityEndDate = Convert.ToDateTime(DuplicateActivityItem.ActivityEndDate).AddDays(days);

                if (DuplicateActivityItem.PurchaseStartDate != null)
                    DuplicateActivityItem.PurchaseStartDate = Convert.ToDateTime(DuplicateActivityItem.PurchaseStartDate).AddDays(days);

                if (DuplicateActivityItem.PurchaseEndDate != null)
                    DuplicateActivityItem.PurchaseEndDate = Convert.ToDateTime(DuplicateActivityItem.PurchaseEndDate).AddDays(days);

                if (DuplicateActivityItem.OnlineViewStartDate != null)
                    DuplicateActivityItem.OnlineViewStartDate = Convert.ToDateTime(DuplicateActivityItem.OnlineViewStartDate).AddDays(days);

                if (DuplicateActivityItem.OnlineViewEndDate != null)
                    DuplicateActivityItem.OnlineViewEndDate = Convert.ToDateTime(DuplicateActivityItem.OnlineViewEndDate).AddDays(days);

                if (DuplicateActivityItem.OnlinePurchaseStartDate != null)
                    DuplicateActivityItem.OnlinePurchaseStartDate = Convert.ToDateTime(DuplicateActivityItem.OnlinePurchaseStartDate).AddDays(days);

                if (DuplicateActivityItem.OnlinePurchaseEndDate != null)
                    DuplicateActivityItem.OnlinePurchaseEndDate = Convert.ToDateTime(DuplicateActivityItem.OnlinePurchaseEndDate).AddDays(days);

                foreach (cREG_SessionModel session in SessionList)
                {
                    session.Old_SessionId = session.SessionId;
                    session.SessionId = -1;
                    session.StartDate = session.StartDate.AddDays(result * 7);
                    session.EndDate = session.EndDate.AddDays(result * 7);
                }

                foreach (cREG_SessionEventModel sessionEvent in SessionEventList)
                {
                    sessionEvent.SessionEventId = -1;
                    //sessionEvent.SessionId = -1;
                    sessionEvent.StartTime = sessionEvent.StartTime.AddDays(result * 7);
                    sessionEvent.EndTime = sessionEvent.EndTime.AddDays(result * 7);
                }

                foreach (cREG_RegistrationModel registration in StoredRegistrationList)
                {
                    registration.RegistrationId = -1;
                }

                DuplicateActivityItem.ActivityId = -1;
                ShowDuplicateMessage = true;
                SelectSessionWeek = false;
                StateHasChanged();
                SaveDuplicateActivity(true);
            }
            else
            {
                ShowDuplicateMessage = false;
                SelectSessionWeek = false;
                StateHasChanged();
            }
            SelectSessionWeek = false;
            //ShowDuplicateMessage = false;
            StateHasChanged();
        }

        protected async Task DuplicateCompleted(bool Save)
        {
            SelectSessionWeek = false;
            ShowDuplicateMessage = false;
            ShowDuplicate = false;
            ShowEdit = false;
            if (Save)
            {
                await LoadActivity();
            }
            getSessionDateList = new List<GetSessionDateModel>();
            StateHasChanged();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            // execute conditionally for loading data, otherwise this will load
            // every time the page refreshes
            LoadActivity();
        }

        protected void EditActivityAfterDuplicate()
        {
            Action = "Edit";
            ShowEdit = true;
        }

        protected void SellActivity(GridCommandEventArgs args)
        {
            WorkingActivity = (cREG_ActivityModel)args.Item;
            GuestAddress = new cACT_AddressModel();
            ShowSell = true;
        }

        protected void PromoteActivity(GridCommandEventArgs args)
        {
            WorkingActivity = (cREG_ActivityModel)args.Item;
            ShowPromote = true;
        }

        protected void PromoteCompleted(bool Save)
        {
            ShowPromote = false;
        }

        protected void InfoActivity(GridCommandEventArgs args)
        {
            WorkingActivity = (cREG_ActivityModel)args.Item;
            ShowSessionInfo = true;
        }

        protected async Task RegistrationCompleted(bool Save)
        {
            ShowSell = false;
            if (Save)
            {
                await LoadActivity();
            }
        }

        protected void SessionCompleted(bool Save)
        {
            ShowSessionInfo = false;
        }

        protected void CheckInSession(GridCommandEventArgs args)
        {
            WorkingActivity = (cREG_ActivityModel)args.Item;
            ShowCheckin = true;
        }
    }
}
