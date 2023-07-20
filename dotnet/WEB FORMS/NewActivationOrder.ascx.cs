using AjaxControlToolkit;
using Classes;
using Classes.ShipEngine;
using ClosedXML.Excel;
using Newtonsoft.Json.Linq;
using Sindhu;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

public partial class Admin_UserControls_NewActivationOrder : System.Web.UI.UserControl
{
    ManageMasters objManageMasters;
    AdminOrders objOrders;
    SiteAttachments objAttach;
    //Admin_UserControls_AccessoryOrder AccessoryOrderContol = new Admin_UserControls_AccessoryOrder();

    AdminUserLogin adminUserLogin = new AdminUserLogin();

    const string adminorders = "adminorders";
    const string manageMasters = "objManageMasters";
    const string siteattach = "siteattachments";
    const string OTNewActivationOrder = "NewActivationOrder";
    const string OTNewActivationOrderLineItem = "NewActivationOrderLineItem";
    const string OTUpgradeOrder = "UpgradeOrder";
    const string OTGPSOrder = "GPSOrder";
    const string ClientId = "ClientId";
    const string OrderId = "OrderId";
    const string AdminTaskId = "id";
    const string PageName = "pg";
    const string TaskId = "TaskId";
    string accessoryUploads = "/Uploads/Order/NewAccessory/";
    string attachedLabelFile = "";
    bool isSalesUser = false;

    //const string OTNewAccessoryOrder = "NewAccessoryOrder";
    const string OTNewAccessoryOrderSub = "Device";

    protected void Page_Load(object sender, EventArgs e)
    {



        #region Check if the Session for Logged In user is Available


        if (AppHelper.CheckSessionEmpty())

        {
            Response.Redirect(Common.GetBaseURL + "admin/default.aspx", true);
        }
        //var hideAccessoryGridWeight = orderAccessoryOrder.GetAccessoryWeighttotalRow();
        #endregion

        lblMsg.Text = "";
        LoadPageControls();
        SetTriggers();



        #region set controls visibility and state
        int multiCheckCount = 0;
        foreach (GridViewRow item in grdOrderInfo.Rows)
        {
            CheckBox chk = item.FindControl("chkOrderDetail") as CheckBox;
            if (chk != null && chk.Checked)
            {
                multiCheckCount++;
            }
        }
        if (multiCheckCount > 1)
        {
            btnUpdateMultiDetails.Visible = true;
        }

        if (rptLine.Items.Count > 0)
        {
            HiddenField hdnOrderDetailId = rptLine.Items[0].FindControl("hdnOrderDetailId") as HiddenField;
            if (hdnOrderDetailId != null)
            {
                int orderDetailId = 0;
                int.TryParse(hdnOrderDetailId.Value, out orderDetailId);
                if (orderDetailId > 0)
                {
                    btnAddNewLine.Visible = true;
                    btnAddNewLine.Text = "Update";
                }
            }

        }
        #endregion
        orderAccessoryOrder.ParentFile = Request.Url.ToString();
        // var tt = orderAccessoryOrder.GetAccessoryWeighttotalRow();

        if (!Page.IsPostBack)
        {

            if (Request.QueryString["som"] != null)
            {
                SetMessageBig("Order submitted successfully");
            }
            if (Request.QueryString["so"] != null)
            {
                SetMessage("Saved successfully");
            }
            lblOrderStatus.Text = AppConstants.NEW;
            int queryClientId = GetQueryId(ClientId);

            if (queryClientId != 0)
            {
                LoadOrderRelated();
            }
            else
            {
                lblMsg.Text = "Please select Company name to continue.";
            }
            if (string.IsNullOrEmpty(txtOrderEmails.Text))
                txtOrderEmails.Text = hdnSalesEmail.Value;

        }

        if (rdoclientStatus.SelectedValue != null && rdoclientStatus.SelectedValue == "0")
        {
            //OrderActivationGain.Visible=true;
            OrderActivationGain.Attributes["style"] = "display:block";
        }
        else
        {
            //OrderActivationGain.Visible = false;
            OrderActivationGain.Attributes["style"] = "display:none";
        }
        var hideAccessoryGridWeight = orderAccessoryOrder.GetAccessoryWeighttotalRow();
        //var p = orderAccessoryOrder.GetAccessoryWeighttotalRow();


    }

    private void LoadTaskValues(string type, ref string department, ref string make, ref string model, ref string username, ref string areacode)
    {
        AdminTasks objTasks = new AdminTasks();

        objTasks.SPType = AppConstants.OPEN;
        objTasks.AdminTaskId = GetQueryId(TaskId) == 0 ? GetQueryId(AdminTaskId) : GetQueryId(TaskId);
        objTasks.TaskHeading = "";
        objTasks.TaskStatus = 0;
        objTasks.TaskAssignedRep = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);
        DataSet ds = objTasks.GetAllTasks();

        if (objTasks.AdminTaskId > 0)
        {
            DataTable dt = ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                string sourceFrom = Convert.ToString(dt.Rows[0]["SourceFrom"]);
                hdnOrderStatusSub.Value = Convert.ToString(dt.Rows[0]["orderStatusSub"]);

                string taskDetails = Convert.ToString(dt.Rows[0]["TaskDetailsUser"]);
                if (taskDetails != "")
                {
                    if (sourceFrom == AppConstants.AMAZONDSP)
                    {

                    }
                    else
                    {
                        string[] arr = taskDetails.Split(new string[] { "\r\n", "<br />", "<br>" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string s in arr)
                        {
                            if (s.ToLower().StartsWith("department"))
                            {
                                department = s;
                            }
                            if (s.ToLower().StartsWith("make") && make == "")
                            {
                                make = s.Split(':').Length > 1 ? s.Split(':')[1].Trim() : "";
                            }
                            if (s.ToLower().StartsWith("make") && make != "")
                            {
                                model = s.Split(':').Length > 1 ? s.Split(':')[1].Trim() : "";
                            }
                            if (s.ToLower().StartsWith("new username"))
                            {
                                username = s.Split(':').Length > 1 ? s.Split(':')[1].Trim() : "";
                            }

                            if (s.ToLower().StartsWith("request area code"))
                            {
                                areacode = s.Split(':').Length > 1 ? s.Split(':')[1].Trim() : "";
                            }
                        }
                    }
                }
            }
        }
    }

    private void LoadOrderRelated()
    {
        GetOrderNo();
        EnableDisableControls();
        LoadCarrier(ddlCarrier);
        BindDropDownShippingMethod(hdnClientID.Value);

        LoadOrder();
        LoadAttachments();

    }

    private bool LoadPageControls(bool hide = false)
    {
        string pageName = Request.Url.AbsolutePath;
        string sot = CommonMethods.GetOrderSubType(true);
        hdnOrderTypeSub.Value = sot;
        if (pageName.ToLower().Contains("admindashordertasks") || hide)
        {
            divButtonArea.Visible = false;
            btnAddNewLine.Visible = false;
            btnCopytoNewLine.Visible = false;
            txtNoOfCopy.Visible = false;
            divAttachment.Visible = false;
            divButtonAreaClose.Visible = hide;
            try
            {
                grdOrderInfo.Columns[11].Visible = true;
                grdOrderInfo.Columns[12].Visible = true;
            }
            catch (Exception ex)
            {

            }
            return true;
        }
        return false;
    }

    private bool LoadPageControlsSubOrder(string subType, HtmlGenericControl divManageLines, out bool disableControls, bool hide = false)
    {
        disableControls = true;
        bool isSubOrder = false;
        string pageName = Request.Url.AbsolutePath;
        string sot = CommonMethods.GetOrderSubType(true);
        hdnOrderTypeSub.Value = sot;

        DataTable dt = CommonMethods.GetMasterAdmin();
        if (dt != null && dt.Rows.Count > 0)
        {
            string query = "OrderType = '" + AppConstants.TLNewActivationOrder + "' And OrderSubType = '" + subType + "' And CarrierId = '" + ddlCarrier.SelectedValue + "'";
            DataRow[] drList = dt.Select(query);
            if (drList != null && drList.Length > 0)
            {
                if (subType == AppConstants.TLNewActivationOrderWSFF)
                {
                    if (sot == subType && subType == AppConstants.TLNewActivationOrderWSFF && (hdnOrderStatusSub.Value == "" || hdnOrderStatusSub.Value == AppConstants.SAVEORDER))
                        hdnSalesEmail.Value = Conversion.ParseString(drList[0]["EmailAddress"]);
                    else
                        hdnSalesEmail.Value = Conversion.ParseString(drList[0]["OrderDeskEmailAddress"]);
                }
                else if (sot == subType)
                {
                    hdnSalesEmail.Value = Conversion.ParseString(drList[0]["OrderDeskEmailAddress"]);
                }
            }
        }

        if (subType == AppConstants.TLNewActivationOrderWSFF)
        {
            if ((pageName.ToLower().Contains("manageordersnewact") || pageName.ToLower().Contains("admindashordertasks"))
                && hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderWSFF)
            {
                isSubOrder = true;
                disableControls = false;
                isSubOrder = true;

                if (lblOrderStatus.Text == AppConstants.NEW || lblOrderStatus.Text == AppConstants.SAVEORDER)
                {
                    divManageLines.Visible = false;
                    pnlShippingSales.Visible = false;
                }
            }
        }
        else if (subType == AppConstants.TLNewActivationOrderCOD)
        {
            if ((pageName.ToLower().Contains("manageordersnewact") || pageName.ToLower().Contains("admindashordertasks"))
                && hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderCOD)
            {
                isSubOrder = true;
                ForCOD(true);
            }
        }
        else if (subType == AppConstants.TLNewActivationOrderCFF)
        {
            if ((pageName.ToLower().Contains("manageordersnewact") || pageName.ToLower().Contains("admindashordertasks"))
                && hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderCFF)
            {
                if (lblOrderStatus.Text == AppConstants.NEW || lblOrderStatus.Text == AppConstants.SAVEORDER)
                {
                    pnlShippingSales.Visible = false;
                }
            }
        }
        return isSubOrder;
    }

    private void ForCOD(bool enable)
    {
        pnlShippingOption.Visible = true;
        pnlCOD.Visible = false;
        pnlShippingSales.Visible = true;
        pnlShippingAddress.Visible = true;
        pnlShippingOptionDDL.Visible = false;
        rfvShippingAddress.Enabled = false;
    }

    protected void btnSaveOrder_Click(object sender, EventArgs e)
    {
        UploadFile(false);
        SaveOrder(AppConstants.SAVEORDER);
    }

    private void SaveOrderMethod(string buttonCall = "", DataTable dt = null)
    {
        Initialize(adminorders);
        objOrders.ParentAdminTaskId = objOrders.AdminTaskId;
        objOrders.AdminTaskId = AssignModelValue(AppConstants.SAVEANDCLOSE, objOrders.AdminTaskId);
        SaveOrder(buttonCall, "1");
        if (buttonCall == AppConstants.NEWLINE && hdnOrderId.Value != "" && hdnOrderId.Value != "0")
        {
            SaveNewOrder(dt);
        }
    }

    private int AssignModelValue(string ActionType, Int64 parentAdminTaskId)
    {

        //LoadClientData(clientID);
        AdminTasks objTasks = new AdminTasks();
        #region Assign Value for model
        int adminTaskId = 0;

        objTasks.SPType = adminTaskId == 0 ? "INSERTCONVERSION" : AppConstants.UPDATE;
        objTasks.AdminTaskId = adminTaskId;
        objTasks.ActionType = ActionType;
        objTasks.RequestID = objTasks.GetNextTaskRequestID();

        objTasks.TaskHeading = txtCompanyName.Text.Trim() + " - " + OTNewActivationOrder.SeparateCapitals() + "-" + CommonMethods.GetOrderSubType(false) + " #" + lblOrderNo.Text;

        objTasks.Invoice = (this.Page.Parent.FindControl("txtInvoice") as TextBox).Text;

        objTasks.ClientID = Conversion.ParseInt(hdnClientID.Value);

        objTasks.TaskCompanyName = txtCompanyName.Text;

        objTasks.TaskContact = txtSpoc.Text;
        objTasks.EmailAddress = txtSpocEmail.Text;
        objTasks.TaskDetailsUser = "";
        objTasks.TaskDetailsResponse = "";
        objTasks.TaskAssignedRep = Convert.ToInt32(Session[AppConstants.SessionSaleRepID]);
        objTasks.TaskStatus = 16;

        objTasks.ShippingTrackingNumber = txtShippingTrackingNumber.Text;
        objTasks.ShippingMethodId = ddlShippingMethod.SelectedValue == "" ? 0 : Conversion.ParseInt(ddlShippingMethod.SelectedValue);
        objTasks.ShippingOptionName = ddlShippingOptionLabel.SelectedValue;

        objTasks.ViewingAdminId = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);
        //objTasks.TaskNotes = txtTaskNotes.Text.Trim();
        objTasks.TaskCreatedDateTime = DateTime.Now;
        objTasks.TaskCreatedBy = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);

        objTasks.TaskModifiedBy = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);

        objTasks.IsActive = 1;
        objTasks.IsDeleted = 0;
        objTasks.ParentAdminTaskId = parentAdminTaskId;

        adminTaskId = objTasks.SaveAdminTask();

        #endregion
        return adminTaskId;
    }

    protected void btnSubmitOrder_Click(object sender, EventArgs e)
    {
        string msg = ValidateOrderAndAttachment();
        if (string.IsNullOrEmpty(msg))
            SaveOrder(AppConstants.SUBMITORDER, "");
        else
            SetMessage(msg);
    }

    private string ValidateOrderAndAttachment()
    {
        string msg = "";

        if (rptLine.Items.Count > 5 && grdAttachments.Rows.Count == 0 && Request.QueryString["sot"] != null && Request.QueryString["sot"].ToLower() == AppConstants.TLNewActivationOrderWSFF.ToLower())
        {
            msg = "Please attach a file";
        }
        /*else if(grdAttachments.Rows.Count == 0 && ddlCarrier.SelectedItem.Text.ToUpper().Trim() == "VERIZON")
        {
            msg = "The Verizon Order Desk requires you to attach the Verizon order Form for all orders. Orders cannot be submitted unless this attachment is added";
        }*/
        return msg;
    }

    public string ValidateShippingTracking(string msg)
    {
        if (CommonMethods.GetOrderSubType(true) == AppConstants.TLNewActivationOrderCFF || CommonMethods.GetOrderSubType(true) == AppConstants.TLNewActivationOrderWSFF)
        {
            if (grdShippingLabel.Rows.Count == 0 && ddlShippingOptionLabel.SelectedValue != "" && txtShippingTrackingNumber.Text.Trim() == "")
            {
                msg = "Please create Shipping Label or enter Shipping Tracking Number.";
                grdShippingLabel.Focus();
            }
        }

        if (msg == "" && CommonMethods.GetOrderSubType(true) == AppConstants.TLNewActivationOrderWSFF)
        {
            if (string.IsNullOrEmpty(txtShippingTrackingNumber.Text))
            {
                msg = "Please enter Shipping Tracking number.";
                txtShippingTrackingNumber.Focus();
            }
            else if (ddlShippingMethod.SelectedIndex == 0)
            {
                msg = "Please select Shipping Method.";
                ddlShippingMethod.Focus();
            }
            else if (grdShippingLabel.Rows.Count == 0 && txtShippingTrackingNumber.Text.Trim() == "")
            {
                msg = "Please create Shipping Label or enter Shipping Tracking Number.";
                grdShippingLabel.Focus();
            }
        }

        if (lblOrderStatus.Text == AppConstants.SUBMITORDER)
        {
            if (CommonMethods.GetOrderSubType(true) != AppConstants.TLNewActivationOrderCOD && (ddlShippingAddress.SelectedItem.Text.ToLower().IndexOf("ws fulfillment") > -1
               && ((ddlShippingOption.SelectedItem.Text.ToLower().IndexOf("in person") == -1)
               || ddlShippingMethod.SelectedItem.Text.ToLower().IndexOf("in person") == -1)))
            {
                msg = "Address: WS FULFILLMENT CENTER can be selected only with Option: IN PERSON PICKUP and Method: IN PERSON DELIVERY.";
                ddlShippingAddress.Focus();
            }
            else if (txtShippingTrackingNumber.Visible == true
                && string.IsNullOrEmpty(txtShippingTrackingNumber.Text)
                && ddlShippingMethod.SelectedItem.Text.ToLower().IndexOf("in person") == -1)
            {
                msg = "Please enter Shipping Tracking number.";
                txtShippingTrackingNumber.Focus();
            }
            else if (txtShippingTrackingNumber.Visible == true
                && !string.IsNullOrEmpty(txtShippingTrackingNumber.Text)
                && txtShippingTrackingNumber.Text.Trim().Length < 18
                && ddlShippingMethod.SelectedItem.Text.ToLower().IndexOf("ups") != -1)
            {
                msg = "Please enter valid Shipping Tracking number.";
                txtShippingTrackingNumber.Focus();
            }
        }

        return msg;
    }

    protected void btnClose_Click(object sender, EventArgs e)
    {
        //Response.Redirect(Common.GetBaseURL + "Admin/admindash.aspx.aspx?ClientId=" + GetQueryId("ClientId"));
        Response.Redirect(Common.GetBaseURL + "Admin/admindash.aspx");
    }

    private void LoadCarrier(DropDownList ddlCarrier)
    {
        CommonMethods.LoadCarrier(ddlCarrier, objManageMasters);
    }

    private void LoadFeatures(DropDownList ddlFeature, int carrierId)
    {
        CommonMethods.LoadFeatures(ddlFeature, carrierId, objManageMasters);
    }

    private void LoadContract(DropDownList ddlContract)
    {
        CommonMethods.BindContract(ddlContract, ddlCarrier);
    }

    private void BindDropDownRateplan(string CarrierId, DropDownList ddldivRatePlan)
    {
        CommonMethods.BindRatePlanDDL(ddldivRatePlan, CarrierId, objManageMasters);
    }

    private DataTable BindModel(int MakeId, DropDownList ddlModel)
    {
        return CommonMethods.BindModel(MakeId, Conversion.ParseInt(ddlCarrier.SelectedValue), ddlModel, "ModelId");
    }

    private int GetQueryId(string querystring)
    {
        int returnValue = 0;
        if (querystring == ClientId && Request.QueryString[querystring] != null)
        {
            hdnClientID.Value = Convert.ToString(Request.QueryString[querystring]);
            Int32.TryParse(hdnClientID.Value, out returnValue);
        }

        if (querystring == OrderId)
        {
            if (Request.QueryString[querystring] != null)
                returnValue = Convert.ToInt32(Request.QueryString[querystring] == "" ? "0" : Request.QueryString[querystring]);
            if (returnValue == 0)
                returnValue = hdnOrderId.Value == "" ? 0 : Convert.ToInt32(hdnOrderId.Value);
        }
        if (querystring == AdminTaskId && Request.QueryString[querystring] != null)
        {
            returnValue = Convert.ToInt32(Request.QueryString[querystring] == "" ? "0" : Request.QueryString[querystring]);
        }
        if (querystring == TaskId && Request.QueryString[querystring] != null)
        {
            returnValue = Convert.ToInt32(Request.QueryString[querystring] == "" ? "0" : Request.QueryString[querystring]);
        }
        if (querystring == AdminTaskId && Request.QueryString[querystring] != null)
        {
            returnValue = Convert.ToInt32(Request.QueryString[querystring] == "" ? "0" : Request.QueryString[querystring]);
        }

        return returnValue;
    }

    private void LoadClientData(int clientId)
    {
        if (clientId == 0)
            clientId = GetQueryId(ClientId);
        if (clientId > 0)
        {
            ManageMasters objManageMasters = new ManageMasters();
            objManageMasters.ClientId = clientId;
            DataTable dt = objManageMasters.BusinessClient_GetById();
            if (dt != null)
            {
                LoadClientDetails(objManageMasters, dt);
            }
        }
    }

    private void LoadClientDetails(ManageMasters objManageMasters, DataTable dt)
    {
        txtCompanyName.Text = Convert.ToString(dt.Rows[0]["CompanyName"]);
        hdnWorkPhone.Value = Convert.ToString(dt.Rows[0]["WorkPhone"]);
        string carrierId = Convert.ToString(dt.Rows[0]["Currently_Using_CarrierId"]);
        if (carrierId.Trim() != "" && carrierId.Trim() != string.Empty)
        {
            try
            {
                ddlCarrier.SelectedValue = Convert.ToString(dt.Rows[0]["Currently_Using_CarrierId"]);
            }
            catch (Exception ex)
            {
                Initialize(manageMasters);
                int Id = Convert.ToInt32(dt.Rows[0]["Currently_Using_CarrierId"]);
                objManageMasters.Id = Id;
                string Name = objManageMasters.GetCarrierName();
                ddlCarrier.Items.Add(new ListItem(Name + "(Deleted)", Id.ToString()));
                ddlCarrier.SelectedValue = Id.ToString();
            }
        }

        txtAccountNo.Text = Convert.ToString(dt.Rows[0]["Account"]);
        txtTaxID.Text = Convert.ToString(dt.Rows[0]["SocialTaxNo"]);
        txtPassCode.Text = Convert.ToString(dt.Rows[0]["Passcode"]);
        txtSpoc.Text = Convert.ToString(dt.Rows[0]["Contact1"]);
        txtSpocEmail.Text = Convert.ToString(dt.Rows[0]["Email1"]);

        BindDropDownShippingAddress();
        ViewState["DTShippingOption"] = CommonMethods.BindDropDownShippingOption(AppConstants.MPSHIPOPTION, ddlCarrier, ddlShippingOption);
    }

    private void BindDropDownShippingAddress()
    {
        Initialize(manageMasters);
        objManageMasters.ClientId = GetQueryId(ClientId);
        DataTable dt = new DataTable();
        dt = objManageMasters.GetShippingStateAddress();
        ViewState["DTShippingAddress"] = dt;
        LoadShippingAddress(dt);
        //if (ddlShippingAddress.Items.Count == 0)
        //{
        //    rfvShippingAddress.Enabled = false;
        //}
    }

    private void LoadShippingAddress(DataTable dt)
    {
        ddlShippingAddress.DataSource = dt;
        ddlShippingAddress.DataTextField = "ShippingAddress";
        ddlShippingAddress.DataValueField = "ShippingId";


        try
        {
            if (dt.Rows.Count > 0)
            {
                using (DataTable dtNew = new DataView(dt, "IsDefault=1 or IsDefault=True", "", DataViewRowState.CurrentRows).ToTable())
                {
                    if (dtNew.Rows.Count > 0)
                    {
                        int shippingId = 0;
                        int.TryParse(dtNew.Rows[0]["ShippingId"].ToString(), out shippingId);
                        if (shippingId > 0)
                        {
                            ddlShippingAddress.SelectedValue = shippingId.ToString();
                            // dt.Rows[0]["ShippingAddress"] = dtNew.Rows[0]["ShippingAddress"] + "(Default)";

                        }
                    }
                }
            }
            //using (DataTable dtNew1 = new DataView(dt, "IsDefault=1 or IsDefault=True", "", DataViewRowState.CurrentRows).ToTable())
            //{
            //    if (dtNew1.Rows.Count > 0)
            //    {
            //        dtNew1.Rows[0]["ShippingAddress"] = dtNew1.Rows[0]["ShippingAddress"];
            //        dt.Rows[0]["ShippingAddress"] = dtNew1.Rows[0]["ShippingAddress"] + "(Default)";

            //    }

            //}
        }



        catch (Exception ex)
        {

        }
        ddlShippingAddress.DataBind();
        ddlShippingAddress.Items.Insert(0, new ListItem("-Select-", "0"));
    }

    private void GetOrderNo()

    {
        Initialize(adminorders);
        objOrders.SPType = adminorders;
        lblOrderNo.Text = objOrders.GetNextID();
    }

    private void Initialize(string className)
    {
        if (className == adminorders)
        {
            if (objOrders == null)
            {
                objOrders = new AdminOrders();
            }
        }
        if (className == manageMasters)
        {
            if (objManageMasters == null)
            {
                objManageMasters = new ManageMasters();
            }
        }
        if (className == siteattach)
        {
            if (objAttach == null)
            {
                objAttach = new SiteAttachments();
            }
        }
    }

    private void EnableDisableControls()
    {
        txtCompanyName.Enabled = false;
        ddlCarrier.Enabled = false;
        txtAccountNo.Enabled = false;
        txtPassCode.Enabled = false;
        txtTaxID.Enabled = false;

    }

    #region Dropdown- Model
    protected void ddlMake_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddlMake = (DropDownList)sender;

        if (ddlMake != null)
        {
            int index = ddlMake.ClientID.LastIndexOf('_') + 1;
            string rowIndex = ddlMake.ClientID.Substring(index);
            Repeater rpt = hdnIsMultiEditBtn.Value == "1" ? rptLineItems : rptLine;
            foreach (RepeaterItem item in rpt.Items)
            {
                DropDownList ddlPortIn = item.FindControl("ddlPortIn") as DropDownList;
                if (ddlPortIn != null)
                {
                    HtmlGenericControl divAreaCode = item.FindControl("divAreaCode") as HtmlGenericControl;
                    HtmlGenericControl divMobile = item.FindControl("divMobile") as HtmlGenericControl;
                    HtmlGenericControl divManageLines = item.FindControl("divManageLines") as HtmlGenericControl;
                    RequiredFieldValidator rftxtSimID = item.FindControl("rftxtSimID") as RequiredFieldValidator;
                    FilteredTextBoxExtender FilteredTextBoxExtender6 = item.FindControl("FilteredTextBoxExtender6") as FilteredTextBoxExtender;

                    AjaxControlToolkit.MaskedEditValidator maskValidator = item.FindControl("MaskedEditValidator1") as AjaxControlToolkit.MaskedEditValidator;
                    Label lblItemOrderStatus = item.FindControl("lblOrderStatus") as Label;
                    SetDIVShow(ddlPortIn, divAreaCode, divMobile, maskValidator, lblItemOrderStatus.Text, divManageLines);

                    CheckForCODOrder(divManageLines, rftxtSimID, FilteredTextBoxExtender6);
                }

                if (item.ItemIndex == Convert.ToInt32(rowIndex))
                {
                    DropDownList ddlModel = item.FindControl("ddlModel") as DropDownList;
                    if (ddlModel != null && ddlMake.SelectedIndex > 0)
                    {
                        DataTable dtModelResult = BindModel(Convert.ToInt32(ddlMake.SelectedValue), ddlModel);
                        if (dtModelResult != null)
                        {
                            ViewState["ddlModelForMake" + ddlMake.SelectedValue] = dtModelResult;
                        }
                    }
                    else
                    {
                        ddlModel.Items.Clear();
                    }
                }


            }
        }
    }
    #endregion

    public string SaveOnly()
    {
        SaveOrder(AppConstants.SAVEONLY);
        return string.Empty;
    }

    private void SaveOrder(string buttonCall, string conversion = "")
    {
        try
        {
            //int totalActivationGain = 0;
            int newNetagain;
            int RefreshNetGain;
            lblMsg.Text = "";
            var email = string.Join(",", CommonMethods.CheckExcludedEmails(txtOrderEmails.Text));
            if (email != null && email.Count() > 0)
            {
                lblMsg.Text = "Email(s) " + string.Join(",", email) + "- not allowed in Order Emails.";
                return;
            }
            //if (rdoclientStatus.SelectedValue!=null && rdoclientStatus.SelectedValue=="0" && buttonCall == "SAVEORDER")
            //{
            if (rdoclientStatus.SelectedValue != null && rdoclientStatus.SelectedValue == "0" && (buttonCall == "SAVEORDER" || buttonCall == AppConstants.SUBMITORDER || buttonCall == AppConstants.SAVEANDCLOSE))
            {
                int.TryParse(txnetActivationGain.Text, out newNetagain);
                int.TryParse(txtRefreshGain.Text, out RefreshNetGain);
                if (grdOrderInfo.Rows.Count != newNetagain + RefreshNetGain)
                {
                    lblMsg.Text = "The number of lines in the order does not match the total in the Activation Gain section. Please correct the applicable section.";
                    return;
                }

            }

            Initialize(adminorders);

            objOrders.OrderId = (hdnOrderId.Value == "" || hdnOrderId.Value == "0") ? GetQueryId(OrderId) : Convert.ToInt32(hdnOrderId.Value);
            objOrders.SPType = objOrders.OrderId == 0 ? AppConstants.INSERT : AppConstants.UPDATE;
            objOrders.OrderType = OTNewActivationOrder;
            objOrders.OrderSubType = CommonMethods.GetOrderSubType(false);
            if (rdoInstallMDM.SelectedValue == "1")
                objOrders.InstallMDM = true;
            if (rdoInstallMDM.SelectedValue == "0")
                objOrders.InstallMDM = false;
            objOrders.ShippingMethodId = Conversion.ParseInt(ddlShippingMethod.SelectedValue);
            objOrders.ShippingTrackingNumber = txtShippingTrackingNumber.Text.Trim();
            objOrders.OrderNo = lblOrderNo.Text;
            objOrders.ClientID = GetQueryId(ClientId);
            objOrders.ShippingAddressId = Convert.ToInt32(ddlShippingAddress.SelectedValue);
            objOrders.ShippingOptionId = Convert.ToInt32(ddlShippingOption.SelectedValue);
            objOrders.IsShippingRequired = chkNoShippingRequired.Checked == true ? true : false;
            objOrders.SPOC = txtSpoc.Text.Trim();
            objOrders.SPOCEmail = txtSpocEmail.Text.Trim();
            objOrders.OrderStatus = buttonCall == AppConstants.SUBMITACTIVATION ? AppConstants.SUBMITORDER : buttonCall;
            objOrders.OrderStatusSub = buttonCall;
            objOrders.IsActive = 1;
            objOrders.IsDeleted = 0;
            objOrders.AssignedRep = Session[AppConstants.SessionSaleRepID].ToString() != "" ? Convert.ToInt32(Session[AppConstants.SessionSaleRepID]) : 0;
            objOrders.CreatedDate = DateTime.Now;
            objOrders.CreatedBy = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);
            objOrders.IsUrgent = chkMarkUrgent.Checked ? true : false;
            objOrders.OrderInstructions = txtOrderInstructions.Text.Trim();
            objOrders.OrderEmails = txtOrderEmails.Text.Trim();
            objOrders.AssociatedUserId = Conversion.ParseInt(hdnAssociatedUserId.Value);
            objOrders.IsConversion = conversion;
            objOrders.NewNetGain = Convert.ToDecimal(txnetActivationGain.Text.Trim() == "" ? null : txnetActivationGain.Text.Trim());
            objOrders.RefreshNetGain = Convert.ToDecimal(txtRefreshGain.Text.Trim() == "" ? null : txtRefreshGain.Text.Trim());
            objOrders.OrderBy = Convert.ToString(txtOredrBy.Text.Trim());




            //objOrders.CustomerStatus = Convert.ToInt32(CustomerStatusDropDownList.SelectedValue);
            if (rdoclientStatus.SelectedValue == "1")
                objOrders.ClientStatus = true;
            if (rdoclientStatus.SelectedValue == "0")
                objOrders.ClientStatus = false;

            //if (rdogain.SelectedValue == "1")
            //    objOrders.ActivationGain = true;

            //if (rdogain.SelectedValue == "0")


            objOrders.ActivationGain = false;

            objOrders.PickupFirstName = "";// txtPickupFirstName.Text.Trim();
            objOrders.PickupLastName = "";//txtPickupLastName.Text.Trim();
            objOrders.ShippingOptionName = ddlShippingOptionLabel.SelectedValue;
            // objOrders.TotalActivationGain = totalActivationGain;
            objOrders.DimensionsBoxHeight = Convert.ToInt32(txtBoxHight.Text.Trim() == "" ? null : txtBoxHight.Text.Trim());
            objOrders.DimensionsBoxLenght = Convert.ToInt32(txtBoxLength.Text.Trim() == "" ? null : txtBoxLength.Text.Trim());
            objOrders.DimensionsBoxWidth = Convert.ToInt32(txtBoxWidth.Text.Trim() == "" ? null : txtBoxWidth.Text.Trim());
            objOrders.ShippingLabelWeight = Convert.ToDecimal(txtDeviceWeight.Text.Trim() == "" ? null : txtDeviceWeight.Text.Trim());




            ////if (txtPickedUpOn.Text != "")
            //{
            //    objOrders.PickedUpOn = txtPickedUpOn.Text;
            //}
            int savedOrderId = objOrders.SaveOrder();
            if (savedOrderId > 0)
            {
                hdnOrderId.Value = savedOrderId.ToString();
                objOrders.OrderId = savedOrderId;

                string accessoryOrder = orderAccessoryOrder.SaveOrderDetailFull(buttonCall, hdnOrderId.Value);


                if (buttonCall != AppConstants.SAVEANDCLOSE && buttonCall != AppConstants.CREATEMANAGELINES && buttonCall != AppConstants.INSERTFILE)
                    SaveOrderDetail(buttonCall);

                if (buttonCall == AppConstants.SUBMITORDER)
                {
                    hdnOrderStatusSub.Value = buttonCall;

                    //if (hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderWSFF)
                    //    EmailProcess(hdnSalesEmail.Value, "Order Desk", hdnOrderTypeSub.Value, AppConstants.TitleORDERNEWACTIVATIONORDERLINEITEM);
                    //else
                    string emailTitle = "";
                    if (hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderCFF)
                    {
                        emailTitle = AppConstants.TitleORDERNEWACTIVATIONORDERCFF;
                    }
                    if (hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderWSFF)
                    {
                        emailTitle = AppConstants.TitleORDERNEWACTIVATIONORDERWSFF;
                    }
                    if (hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderCOD)
                    {
                        emailTitle = AppConstants.TitleORDERNEWACTIVATIONORDERCOD;
                    }
                    Response.Write("hdnSalesEmail.Value = " + hdnSalesEmail.Value + Environment.NewLine);
                    string orderEmail = hdnSalesEmail.Value;
                    try
                    {
                        //    lblMsg.Text += "Before = " + hdnSalesEmail.Value;
                        //    lblMsg.Text += "hdnOrderTypeSub = " + hdnOrderTypeSub.Value;
                        //    LoadPageControlsSubOrder(hdnOrderTypeSub.Value, null, out disableControls, false);
                        //    lblMsg.Text += "After = " + hdnSalesEmail.Value;
                        if (string.IsNullOrEmpty(txtOrderEmails.Text))
                            orderEmail = CommonMethods.GetMasterAdminEmailAddress(hdnOrderTypeSub.Value, ddlCarrier.SelectedValue,
                            ((hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderCFF || hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderCOD) ? "" : "EmailAddress"));
                        else
                            orderEmail = txtOrderEmails.Text;
                    }
                    catch (Exception ex)
                    {
                        Response.Write("SaveOrder(string buttonCall) LoadPageControlsSubOrder: " + "hdnSalesEmail.Value:" + hdnSalesEmail.Value + " Message: "
                            + ex.Message.ToString() + " StackTrace: " + ex.StackTrace.ToString() + Environment.NewLine);
                        lblMsg.Text = "SaveOrder(string buttonCall) LoadPageControlsSubOrder: " + "hdnSalesEmail.Value:" + hdnSalesEmail.Value + " Message: "
                            + ex.Message.ToString() + " StackTrace: " + ex.StackTrace.ToString();
                    }
                    //Response.Write("hdnSalesEmail.Value = " + hdnSalesEmail.Value + Environment.NewLine);
                    EmailProcess(orderEmail, "Order Desk", emailTitle, AppConstants.TitleORDERNEWACTIVATIONORDERLINEITEM, "submit order");
                    Response.Redirect("ManageOrdersNewAct.aspx?ClientId=" + GetQueryId(ClientId) + "&OrderId=" + objOrders.OrderId
                        + "&id=" + GetQueryId(TaskId)
                        + "&sot=" + hdnOrderTypeSub.Value + GetDeleteLineIndex() + "&som=1");
                }
                else if (buttonCall == AppConstants.SAVEORDER)
                {
                    SetMessage("Saved successfully");
                }
            }
        }
        catch (Exception ex)
        {
            lblMsg.Text = "SaveOrder(string buttonCall): " + ex.Message.ToString();
        }
    }

    public bool EmailProcess(string clientEmail, string contactName, string emailTitle, string emailSubTitle, string buttonCall = "")
    {
        bool flag = false;
        Initialize(manageMasters);
        Dictionary<string, string> objEmailTokens;
        List<Dictionary<string, string>> listEmailTokens;
        EmailContentFormation(clientEmail, contactName, emailSubTitle, out objEmailTokens, out listEmailTokens, buttonCall);
        DataTable dataTable = new DataTable();
        string attachmentList = "";
        if (buttonCall != "Send Order Conf To Client")
        {
            for (int i = 0; i < grdAttachments.Rows.Count; i++)
            {
                CheckBox chkFileChecked = grdAttachments.Rows[i].FindControl("chkAttachment") as CheckBox;

                {
                    Literal UniqueFileName = grdAttachments.Rows[i].FindControl("ltrUniqueFileName") as Literal;
                    attachmentList += Server.MapPath("~/Uploads/AdminTasks/") + UniqueFileName.Text + ",";
                }
            }
        }
        SendEmail objEmail = new SendEmail();
        string heading = string.Empty;
        if (buttonCall == "Send Order Conf To Client" || (buttonCall == "submit order" && listEmailTokens.Count >= 5))
        {
            string strBody = @"<table style='font-size:14px;font-family:Arial, Helvetica, sans-serif;border:1px'>
            <tr style='font-weight:400;'><td style='background:'#507CD1'><b>Mobile Number</b></td><td><b>User Name</b></td><td><b>Contract</b></td><td><b>Rate Plan</b></td><td><b>Make</b></td><td><b>Model</b></td><td><b>MEID/ IMEI</b></td><td><b>Sim ID</b></td></tr>#CONTENT#</table>";

            DataRow dataRow = dataTable.NewRow();

            AppHelper.FormDataTable(dataTable, new string[] {
                "MOBILENO" , "USERNAME" , "CONTRACT" ,"RATEPLANVALUE", "MAKE", "MODEL", "IMEIMEIDVALUE", "SIMVALUE"
            });
            string content = "";
            foreach (Dictionary<string, string> token in listEmailTokens)
            {
                content += "<tr>";
                content += "<td>'" + token["%MOBILENO%"] + "</td>";
                content += "<td>" + token["%USERNAME%"] + "</td>";
                content += "<td>" + token["%CONTRACT%"] + "</td>";
                content += "<td>" + token["%RATEPLANVALUE%"] + "</td>";// 
                content += "<td>" + token["%MAKE%"] + "</td>";
                content += "<td>" + token["%MODEL%"] + "</td>";
                content += "<td>'" + token["%IMEIMEIDVALUE%"] + "</td>";
                content += "<td>'" + token["%SIMVALUE%"] + "</td>";
                content += "</tr>";

                try
                {
                    dataRow = dataTable.NewRow();
                    dataRow["MOBILENO"] = token["%MOBILENO%"].GetStringFromObject();
                    dataRow["USERNAME"] = token["%USERNAME%"].GetStringFromObject();
                    dataRow["CONTRACT"] = token["%CONTRACT%"].GetStringFromObject();
                    dataRow["RATEPLANVALUE"] = token["%RATEPLANVALUE%"].GetStringFromObject();
                    dataRow["MAKE"] = token["%MAKE%"].GetStringFromObject();
                    dataRow["MODEL"] = token["%MODEL%"].GetStringFromObject();
                    dataRow["IMEIMEIDVALUE"] = token["%IMEIMEIDVALUE%"].GetStringFromObject();
                    dataRow["SIMVALUE"] = token["%SIMVALUE%"].GetStringFromObject();
                    dataTable.Rows.Add(dataRow);
                }
                catch (Exception)
                {

                }
            }
            bool isSuccess = true;
            try
            {
                heading = txtCompanyName.Text.Trim() + "-" + lblOrderNo.Text + ".xlsx";
                string fullPath = AppHelper.CreateDirectory(Server.MapPath("~/files/AdminOrders/")) + heading;

                ExcelHelper excelHelper = new ExcelHelper();
                excelHelper.DownloadExcel(dataTable, fullPath, null, null);

                attachmentList += fullPath + ",";
            }
            catch (Exception ex)
            {
                isSuccess = false;
            }

            if (isSuccess == false)
            {
                strBody = strBody.Replace("#CONTENT#", content);

                heading = txtCompanyName.Text.Trim() + "-" + lblOrderNo.Text + ".xls";

                byte[] inputstrngBytes = Encoding.UTF8.GetBytes(strBody);
                MemoryStream contentStream = new MemoryStream(inputstrngBytes);
                System.Net.Mail.Attachment attFile = new System.Net.Mail.Attachment(contentStream, heading);

                objEmail.AttachmentListObject = new List<System.Net.Mail.Attachment>();
                objEmail.AttachmentListObject.Add(attFile);
            }
        }
        string[] att = attachmentList != "" ? attachmentList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : null;
        //Response.Write(listEmailTokens.Count);

        if (string.IsNullOrEmpty(clientEmail))
        {
            //string emailAddress = AppHelper.GetSupportEmail(Convert.ToInt32(ddlCarrier.SelectedValue), AppConstants.OrderTypeLines);
            string emailAddress = string.Empty; // AppHelper.GetSupportEmail(Convert.ToInt32(ddlCarrier.SelectedValue), AppConstants.OrderTypeLines);
            if (string.IsNullOrEmpty(txtOrderEmails.Text))
                emailAddress = CommonMethods.GetMasterAdminEmailAddress(hdnOrderTypeSub.Value, ddlCarrier.SelectedValue,
                    ((hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderCFF || hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderCOD) ? "" : "EmailAddress"));
            else
                emailAddress = txtOrderEmails.Text;
            objEmail.PrimaryId = hdnOrderId.Value;
            objEmail.ModuleName = AppConstants.ORDER;

            if (!string.IsNullOrEmpty(emailAddress))
                flag = objEmail.SendUserEmail(emailAddress, objEmailTokens, listEmailTokens, OTNewActivationOrder, OTNewActivationOrderLineItem
                , att, AppConstants.SUPPORT, "");
        }
        else
        {
            //if (!string.IsNullOrEmpty(hdnSalesEmail.Value))
            //{
            //    flag = objEmail.SendOrderEmail(hdnSalesEmail.Value, objEmailTokens, listEmailTokens, emailTitle, emailSubTitle, att, AppConstants.SUPPORT, "");
            //}
            //else
            {
                objEmail.PrimaryId = hdnOrderId.Value;
                objEmail.ModuleName = AppConstants.ORDER;
                //clientEmail = "nautnajarlura@gmail.com";
                flag = objEmail.SendOrderEmail(clientEmail, objEmailTokens, listEmailTokens, emailTitle, emailSubTitle, att, AppConstants.SUPPORT, "");
            }
        }
        return flag;
    }

    private void EmailContentFormation(string clientEmail, string contactName, string emailSubTitle, out Dictionary<string, string> objEmailTokens
        , out List<Dictionary<string, string>> listEmailTokens, string buttonCall = "")
    {
        objEmailTokens = new Dictionary<string, string>();

        #region Order Detail
        string url = Common.GetBaseURL + "/Admin/ManageOrdersNewAct.aspx?ClientId=" + AppHelper.GetValueFromQueryString("ClientId", this.Context) + "&OrderId=" + AppHelper.GetValueFromQueryString("OrderId", this.Context);
        string OrderNumber = "<a target='_blank' href ='" + url + "'" + ">" + lblOrderNo.Text + " </a> ";

        string heading = txtCompanyName.Text.Trim() + " - " + OTNewActivationOrder.SeparateCapitals() + "-" + CommonMethods.GetOrderSubType(false) + " #" + lblOrderNo.Text;
        if (chkMarkUrgent.Checked)
        {
            objEmailTokens.Add("%Subject%", "[URGENT]" + heading);
            objEmailTokens.Add("%URGENT%", "BLOCK");
        }
        else
        {
            objEmailTokens.Add("%Subject%", heading);
            objEmailTokens.Add("%URGENT%", "NONE");
        }

        objEmailTokens.Add("%NEWACTIVATION%", "BLOCK");
        objEmailTokens.Add("%UPGRADE%", "NONE");
        objEmailTokens.Add("%CONTACTNAME%", contactName);
        objEmailTokens.Add("%ORDERTYPE%", OTNewActivationOrder.SeparateCapitals());
        objEmailTokens.Add("%ORDERNUMBER%", OrderNumber);
        objEmailTokens.Add("%COMPANYNAME%", txtCompanyName.Text.Trim());
        objEmailTokens.Add("%CARRIER%", ddlCarrier.SelectedItem.Text);
        objEmailTokens.Add("%ACCOUNT%", txtAccountNo.Text.Trim());
        objEmailTokens.Add("%PASSCODE%", txtPassCode.Text.Trim());
        objEmailTokens.Add("%TAXID%", txtTaxID.Text.Trim());
        objEmailTokens.Add("%WORKPHONE%", hdnWorkPhone.Value.Trim());

        if (hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderCFF)
        {
            string OrderNumberActivation = "<a  > " + lblOrderNo.Text + " </a> ";
            objEmailTokens.Remove("%ORDERNUMBER%");
            objEmailTokens.Add("%ORDERNUMBER%", OrderNumberActivation);

            if (!string.IsNullOrEmpty(txtShippingTrackingNumber.Text))
                if (!objEmailTokens.ContainsKey("%GREETINGS%"))
                    objEmailTokens.Add("%GREETINGS%", "Great News! Your order has been shipped and you should receive it shortly.");
            else
                objEmailTokens.Add("%GREETINGS%", "Great News! Your order has been submitted for processing.");
            objEmailTokens.Add("%CFF%", "block");
            objEmailTokens.Add("%COD%", "none");
            objEmailTokens.Add("%WSFF%", "none");

            if (!objEmailTokens.ContainsKey("%SHIPPINGTRACKING%"))
                objEmailTokens.Add("%SHIPPINGTRACKING%", txtShippingTrackingNumber.Text);
            if (!objEmailTokens.ContainsKey("%SHIPPINGMETHOD%") && ddlShippingMethod.SelectedIndex > 0)
                objEmailTokens.Add("%SHIPPINGMETHOD%", ddlShippingMethod.SelectedItem.Text);

            objEmailTokens.Add("%NOSALES%", "block");
            objEmailTokens.Add("%SALES%", "none");

            if (!objEmailTokens.ContainsKey("%SHIPPINGOPTION%"))
                objEmailTokens.Add("%SHIPPINGOPTION%", ddlShippingOption.SelectedItem.Text);

            if (chkNoShippingRequired.Checked)
            {
                objEmailTokens.Add("%NOSHIPPING%", "No");
                objEmailTokens.Add("%NOCFF%", "block");
            }
            else
            {
                objEmailTokens.Add("%NOCFF%", "none");
            }
        }
        else if (hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderWSFF)
        {
            //string OrderNumberActivation = "<a  > " + lblOrderNo.Text + " </a> ";
            //objEmailTokens.Remove("%ORDERNUMBER%");
            //objEmailTokens.Add("%ORDERNUMBER%", OrderNumberActivation);
            objEmailTokens.Add("%CFF%", "none");
            objEmailTokens.Add("%COD%", "none");
            objEmailTokens.Add("%WSFF%", "block");
            if (!objEmailTokens.ContainsKey("%GREETINGS%"))
                objEmailTokens.Add("%GREETINGS%", "Great News! Your order has been completed and shipped.");
            if (!objEmailTokens.ContainsKey("%SHIPPINGTRACKING%"))
                objEmailTokens.Add("%SHIPPINGTRACKING%", txtShippingTrackingNumber.Text);

            string salesNoSales = "";
            if (hdnOrderStatusSub.Value == AppConstants.SUBMITORDER)
            {
                if (buttonCall == AppConstants.SUBMITACTIVATION)
                {
                    salesNoSales = @"<tr>"
+ "<td colspan=\"2\"><span lang=\"EN-IN\" style=\"color: #696969; font-size:14px; font-weight: normal; margin-left: 0px;\">Dear Order Desk</span></td>"
                    + "</tr><tr>"
                        + "<td colspan=\"2\">"
                            + "<span lang=\"EN-IN\" style=\"color: #696969; font-size:14px; font-weight: normal; margin-left: 0px;\">"
                                + "Below are the details of an order. Please process and email back the order information."
                                + "For larger orders, please email back an excel spreadsheet indication the matching mobile numbers, imei and sim information."
                            + "</span>"
                        + "</td>"
                    + "</tr>";

                    objEmailTokens.Add("%NOSALES%", salesNoSales);
                    objEmailTokens.Add("%SALES%", "none");

                    if (!objEmailTokens.ContainsKey("%SHIPPINGADDRESS%"))
                        objEmailTokens.Add("%SHIPPINGADDRESS%", string.Empty);
                }
                else
                {
                    salesNoSales = @"<tr>"
                        + "<td colspan=\"2\"><span lang=\"EN-IN\" style=\"color: #696969; font-size:14px; font-weight: normal; margin-left: 0px;\">Dear Fulfillment Team</span></td>"
                    + "</tr>"
                    + "<tr>"
                     + "<td colspan=\"2\">"
                     + "       <span lang=\"EN-IN\" style=\"color: #696969; font-size:14px; font-weight: normal; margin-left: 0px;\">"
                        + "        Below are the order details. Please help process the order and submit to Order Desk."
                          + "  </span>"
                        + "</td>"
                    + "</tr>";
                    objEmailTokens.Add("%NOSALES%", salesNoSales);
                    objEmailTokens.Add("%SALES%", "block");
                }
            }
            else
            {
                salesNoSales = @"<tr>"
+ "<td colspan=\"2\"><span lang=\"EN-IN\" style=\"color: #696969; font-size:14px; font-weight: normal; margin-left: 0px;\">Dear Order Desk</span></td>"
                    + "</tr><tr>"
                        + "<td colspan=\"2\">"
                            + "<span lang=\"EN-IN\" style=\"color: #696969; font-size:14px; font-weight: normal; margin-left: 0px;\">"
                                + "Below are the details of an order. Please process and email back the order information."
                                + "For larger orders, please email back an excel spreadsheet indication the matching mobile numbers, imei and sim information."
                            + "</span>"
                        + "</td>"
                    + "</tr>";

                objEmailTokens.Add("%NOSALES%", salesNoSales);
                objEmailTokens.Add("%SALES%", "none");

            }
        }
        else if (hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderCOD)
        { 
            string OrderNumberActivation = "<a  > " + lblOrderNo.Text + " </a> ";
            objEmailTokens.Remove("%ORDERNUMBER%");
            objEmailTokens.Add("%ORDERNUMBER%", OrderNumberActivation);
            objEmailTokens.Add("%CFF%", "none");
            objEmailTokens.Add("%COD%", "block");
            if (!objEmailTokens.ContainsKey("%GREETINGS%"))
                objEmailTokens.Add("%GREETINGS%", "Great News! Your order has been completed and activated.");
            objEmailTokens.Add("%WSFF%", "none");
            if (!objEmailTokens.ContainsKey("%SHIPPINGTRACKING%"))
                objEmailTokens.Add("%SHIPPINGTRACKING%", "");

            objEmailTokens.Add("%NOSALES%", "block");
            objEmailTokens.Add("%SALES%", "none");
        }
        else
        {
            objEmailTokens.Add("%CFF%", "none");
            objEmailTokens.Add("%COD%", "none");
            objEmailTokens.Add("%WSFF%", "none");
            objEmailTokens.Add("%NOSALES%", "block");
            objEmailTokens.Add("%SALES%", "none");
            if (!objEmailTokens.ContainsKey("%SHIPPINGOPTION%"))
                objEmailTokens.Add("%SHIPPINGOPTION%", "");
        }

        if (ddlShippingAddress != null && ddlShippingAddress.SelectedValue != "0")
        {
            objEmailTokens.Add("%DISPLAY%", "block");
            if (!objEmailTokens.ContainsKey("%SHIPPINGADDRESS%"))
                objEmailTokens.Add("%SHIPPINGADDRESS%", ddlShippingAddress.SelectedItem.Text);
        }
        else
        {
            objEmailTokens.Add("%DISPLAY%", "none");
            if (!objEmailTokens.ContainsKey("%SHIPPINGADDRESS%"))
                objEmailTokens.Add("%SHIPPINGADDRESS%", "");
        }
        if (!objEmailTokens.ContainsKey("%SHIPPINGOPTION%"))
            objEmailTokens.Add("%SHIPPINGOPTION%", ddlShippingOption.SelectedItem.Text);

        if (!objEmailTokens.ContainsKey("%SHIPPINGMETHOD%") && ddlShippingMethod != null && ddlShippingMethod.SelectedIndex > 0)
            objEmailTokens.Add("%SHIPPINGMETHOD%", ddlShippingMethod.SelectedItem.Text);

        string instructions = txtOrderInstructions.Text.Trim();
        if (ddlCarrier != null && ddlCarrier.SelectedItem.Text.ToUpper().Trim() == "VERIZON")
        {
            objEmailTokens.Add("%VERIZON%", "block");

            string verizonInstruction = "<br/><span lang=\"EN-IN\" style=\"color: #FF0000;font-size: 14px;font-weight: bold;margin-left: 0px;\">" +
                "<p><p>Please process the order below following the instructions and details.</p>"
                //+"<p>-Be Sure to pay attention to the Tab “Order Details”, this will provide you with all the order details</p><p>-Pay special attention to the User names, rate plans, shipping address and port information. If you’re not clear on anything reply to this email and ask.</p><p>-After you complete the order, reply to <u>ALL</u> to this email and attached the Order Form with the “VZ AGENT completed order form” Tab filled out all the way.  You may have to wait until the order ships before you can obtain all the information to fill in on this tab. <u>Failure to fill out this tab completely and replying to all will result in no compensation paid.</u></p>"
                + "<p>-Please monitor the order and make sure the SPOC accepts the order. If there is a problem, let the client or us know.</p></p></span>";

            instructions = instructions + verizonInstruction;
        }
        else
        {
            objEmailTokens.Add("%VERIZON%", "none");
        }

        if (!objEmailTokens.ContainsKey("%SHIPPINGMETHOD%"))
            objEmailTokens.Add("%SHIPPINGMETHOD%", string.Empty);
        objEmailTokens.Add("%SPOC%", txtSpoc.Text.Trim());
        objEmailTokens.Add("%SPOCEMAIL%", txtSpocEmail.Text.Trim());
        objEmailTokens.Add("%Name%", adminUserLogin.UserID);
        if (buttonCall == "Send Order Conf To Client")
        {
            string mailclientInstructiobns = "<br/><span lang=\"EN-IN\" style=\"font-size: 14px;font-weight: bold;margin-left: 0px;\">" +
                "<p><p> <h3>Please See Attached Excle File For Your Records </h3></p>";
            objEmailTokens.Add("%ORDERINSTRUCTIONS%", mailclientInstructiobns);
        }
        else
            objEmailTokens.Add("%ORDERINSTRUCTIONS%", instructions);

        if (AppConstants.SUBMITORDER == hdnOrderStatusSub.Value || emailSubTitle == AppConstants.TitleORDERNEWACTIVATIONORDERLINEITEM)
        {
            if (hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderWSFF)
            {
                if (buttonCall == "" || (emailSubTitle == AppConstants.TitleORDERNEWACTIVATIONORDERLINEITEM && buttonCall == AppConstants.SAVEANDCLOSE))
                {
                    if (rdoInstallMDM.SelectedValue == "1")
                        objEmailTokens.Add("%INSTALLMDM%", "<b>Install MDM</b>: Yes");
                    else if (rdoInstallMDM.SelectedValue == "0")
                        objEmailTokens.Add("%INSTALLMDM%", "<b>Install MDM</b>: No");
                    else
                        objEmailTokens.Add("%INSTALLMDM%", "");
                }
                else
                {
                    objEmailTokens.Add("%INSTALLMDM%", "");
                }
                if (buttonCall != AppConstants.SUBMITACTIVATION)
                {
                    string fullillmentInstruction = "<br/><span lang=\"EN-IN\" style=\"color: #FF0000;font-size: 14px;font-weight: bold;margin-left: 0px;\">" +
                "<p><p>Fullfillment Team, Please Open This order and Add Equipment Information So It Can Be Sent To Order Desk For Processing.</p>"
                //+"<p>-Be Sure to pay attention to the Tab “Order Details”, this will provide you with all the order details</p><p>-Pay special attention to the User names, rate plans, shipping address and port information. If you’re not clear on anything reply to this email and ask.</p><p>-After you complete the order, reply to <u>ALL</u> to this email and attached the Order Form with the “VZ AGENT completed order form” Tab filled out all the way.  You may have to wait until the order ships before you can obtain all the information to fill in on this tab. <u>Failure to fill out this tab completely and replying to all will result in no compensation paid.</u></p>"
                + "<p></p></p></span>";
                    objEmailTokens.Remove("%ORDERINSTRUCTIONS%");
                    objEmailTokens.Add("%ORDERINSTRUCTIONS%", fullillmentInstruction);

                }
            }
            else
                objEmailTokens.Add("%INSTALLMDM%", "");

            if (!objEmailTokens.ContainsKey("%NOSALES%"))
                objEmailTokens.Add("%NOSALES%", "none");
            if (!objEmailTokens.ContainsKey("%SALES%"))
                objEmailTokens.Add("%SALES%", "block");
        }
        else
        {
            objEmailTokens.Add("%INSTALLMDM%", "");

            if (!objEmailTokens.ContainsKey("%NOSALES%"))
                objEmailTokens.Add("%NOSALES%", "block");
            if (!objEmailTokens.ContainsKey("%SALES%"))
                objEmailTokens.Add("%SALES%", "none");
        }
        #endregion

        listEmailTokens = new List<Dictionary<string, string>>();

        //foreach (RepeaterItem item in rptLine.Items)
        //{
        //    #region LineItem
        //    Dictionary<string, string> objLineItemTokens = new Dictionary<string, string>();

        //    Label lblItemOrderStatus = item.FindControl("lblOrderStatus") as Label;

        //    if (lblItemOrderStatus.Text != AppConstants.NEW)
        //    {
        //        DropDownList ddlPortIn = item.FindControl("ddlPortIn") as DropDownList;
        //        TextBox txtMobileNo = item.FindControl("txtMobileNo") as TextBox;
        //        TextBox txtAreaCode = item.FindControl("txtAreaCode") as TextBox;
        //        DropDownList ddlContract = item.FindControl("ddlContract") as DropDownList;
        //        TextBox txtUserName = item.FindControl("txtUserName") as TextBox;
        //        DropDownList ddlMake = item.FindControl("ddlMake") as DropDownList;
        //        DropDownList ddlModel = item.FindControl("ddlModel") as DropDownList;
        //        TextBox txtActNotes = item.FindControl("txtActNotes") as TextBox;
        //        TextBox txtLineNotes = item.FindControl("txtLineNotes") as TextBox;
        //        TextBox txtMEIDIMEI = item.FindControl("txtMEIDIMEI") as TextBox;
        //        TextBox txtSimID = item.FindControl("txtSimID") as TextBox;
        //        DropDownList ddldivRatePlan = item.FindControl("ddldivRatePlan") as DropDownList;
        //        DropDownList ddlFeatures = item.FindControl("ddlFeatures") as DropDownList;


        //        objLineItemTokens.Add("%NUM%", (item.ItemIndex + 1).ToString());

        //        if (ddlPortIn != null && ddlPortIn.SelectedItem != null)
        //        {
        //            string portInTemplate = "<tr><td><span lang='EN-IN' style='color: #696969; font-size:14px; font-weight: normal; margin-left: 5px;font-style: normal;'>Port In:<span>&nbsp; </span>" + ddlPortIn.SelectedItem.Text + "</span></td></tr>";
        //            objLineItemTokens.Add("%PORTIN%", portInTemplate);
        //        }
        //        else
        //            objLineItemTokens.Add("%PORTIN%", "");

        //        if (!string.IsNullOrEmpty(clientEmail))
        //        {
        //            objLineItemTokens.Add("%MOBILENO%", AppHelper.FormatMaskedMobileTextBox(txtMobileNo, this.Request));
        //        }
        //        else
        //        {
        //            if (ddlPortIn.SelectedItem.Value == "0")
        //            {
        //                objLineItemTokens.Add("%PORTTYPE%", "Area Code");
        //                objLineItemTokens.Add("%MOBILENO%", AppHelper.FormatMaskedMobileTextBox(txtAreaCode, this.Request));
        //            }
        //            else
        //            {
        //                objLineItemTokens.Add("%PORTTYPE%", "Mobile no");
        //                objLineItemTokens.Add("%MOBILENO%", AppHelper.FormatMaskedMobileTextBox(txtMobileNo, this.Request));
        //            }
        //        }

        //        objLineItemTokens.Add("%CONTRACT%", ddlContract.SelectedItem.Text);

        //        if (ddldivRatePlan != null && ddldivRatePlan.SelectedItem != null)
        //        {
        //            string ratePlanTemplate = @"<tr><td><span lang='EN-IN' style='color: #696969; font-size:14px; font-weight: normal; margin-left: 5px;font-style: normal;'>Rate plan:<span>&nbsp; </span>" + ddldivRatePlan.SelectedItem.Text + "</span></td></tr>";
        //            objLineItemTokens.Add("%RATEPLAN%", ratePlanTemplate);
        //            if (ddldivRatePlan.SelectedIndex > 0)
        //                objLineItemTokens.Add("%RATEPLANVALUE%", ddldivRatePlan.SelectedItem.Text);
        //            else
        //                objLineItemTokens.Add("%RATEPLANVALUE%", string.Empty);
        //        }
        //        else
        //        {
        //            objLineItemTokens.Add("%RATEPLAN%", string.Empty);
        //            objLineItemTokens.Add("%RATEPLANVALUE%", string.Empty);
        //        }
        //        objLineItemTokens.Add("%USERNAME%", txtUserName.Text);
        //        if (ddlMake != null && ddlMake.SelectedItem != null)
        //            objLineItemTokens.Add("%MAKE%", ddlMake.SelectedItem.Text);
        //        else
        //            objLineItemTokens.Add("%MAKE%", string.Empty);
        //        if (ddlModel != null && ddlModel.SelectedItem != null)
        //            objLineItemTokens.Add("%MODEL%", ddlModel.SelectedItem.Text);
        //        else
        //            objLineItemTokens.Add("%MODEL%", string.Empty);


        //        if (ddlFeatures != null && ddlFeatures.SelectedItem != null)
        //            objLineItemTokens.Add("%FEATURE%", ddlFeatures.SelectedItem.Text);
        //        else
        //            objLineItemTokens.Add("%FEATURE%", string.Empty);

        //        objLineItemTokens.Add("%IMEIMEID%", txtMEIDIMEI.Text == "" ? "" : "<tr><td><span lang=\"EN-IN\" style=\"color: #696969; font-size:14px; font-weight: normal; margin-left: 5px;font-style: normal;\"><span style=\"\">IMEI/MEID:&nbsp; </span>" + txtMEIDIMEI.Text + "</span></td></tr>");
        //        objLineItemTokens.Add("%SIM%", txtMEIDIMEI.Text == "" ? "" : "<tr><td><span lang=\"EN-IN\" style=\"color: #696969; font-size:14px; font-weight: normal; margin-left: 5px;font-style: normal;\"><span style=\"\">SIM:&nbsp; </span>" + txtSimID.Text + "</span></td></tr>");
        //        //objLineItemTokens.Add("%LINENOTES%", txtLineNotes.Text == "" ? "" : "<tr><td><span lang=\"EN-IN\" style=\"color: #696969; font-size:14px; font-weight: normal; margin-left: 5px;font-style: normal;\"><span style=\"\">Line notes/Instruction:&nbsp; </span>" + txtLineNotes.Text + "</span></td></tr>");

        //        if (buttonCall == "Send Order Conf To Client")
        //        {
        //            objLineItemTokens.Add("%LINENOTES%", "");
        //            if (!objLineItemTokens.ContainsKey("%ACTNOTES%"))
        //                objLineItemTokens.Add("%ACTNOTES%", "");
        //        }
        //        else
        //            objLineItemTokens.Add("%LINENOTES%", txtLineNotes.Text == "" ? "" : "<tr><td><span lang=\"EN-IN\" style=\"color: #696969; font-size:14px; font-weight: normal; margin-left: 5px;font-style: normal;\"><span style=\"\">Line notes/Instruction:&nbsp; </span>" + txtLineNotes.Text + "</span></td></tr>");

        //        //objLineItemTokens.Add("%LINENOTES%", txtLineNotes.Text);
        //        objLineItemTokens.Add("%IMEIMEIDVALUE%", txtMEIDIMEI.Text);
        //        objLineItemTokens.Add("%SIMVALUE%", txtSimID.Text);

        //        listEmailTokens.Add(objLineItemTokens);
        //    }
        //    #endregion
        //}

        if (ViewState["rptLine"] != null)
        {
            using (DataTable dt = (DataTable)ViewState["rptLine"])
            {
                if (dt.Rows.Count > 0)
                {
                    int rowCount = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        rowCount++;
                        #region LineItem
                        Dictionary<string, string> objLineItemTokens = new Dictionary<string, string>();

                        string OrderStatus = dr["OrderStatus"].ToString();
                        string OrderDetailId = dr["OrderDetailId"].ToString();
                        string MobileNo = dr["MobileNo"].ToString();
                        string AreadCode = dr["MobileNo"].ToString();
                        string UserName = dr["UserName"].ToString();

                        string LineNotesInstruction = dr["LineNotesInstruction"].ToString();
                        string ActOrderInternalNotes = dr["ActOrderInternalNotes"].ToString();
                        string MonthlyCost = dr["MonthlyCost"].ToString();
                        string MEID_IMEI = dr["MEID_IMEI"].ToString();
                        string SIMID = dr["SIMID"].ToString();
                        string Port = dr["Port"].ToString();
                        string Contract = dr["Contract"].ToString();
                        string Make = dr["Make"].ToString();
                        string Model = dr["Model"].ToString();
                        string RatePlans = dr["RatePlans"].ToString();
                        string Features = dr["Features"].ToString();

                        if (OrderStatus != AppConstants.NEW)
                        {
                            objLineItemTokens.Add("%NUM%", rowCount.ToString());

                            if (!string.IsNullOrEmpty(Port))
                            {
                                string portInTemplate = "<tr><td><span lang='EN-IN' style='color: #696969; font-size:14px; font-weight: normal; margin-left: 5px;font-style: normal;'>Port In:<span>&nbsp; </span>" + Port + "</span></td></tr>";
                                objLineItemTokens.Add("%PORTIN%", portInTemplate);
                            }
                            else
                                objLineItemTokens.Add("%PORTIN%", "");

                            if (!string.IsNullOrEmpty(clientEmail))
                            {
                                objLineItemTokens.Add("%MOBILENO%", MobileNo);
                            }
                            else
                            {
                                if (Port == "No")
                                {
                                    objLineItemTokens.Add("%PORTTYPE%", "Area Code");
                                    objLineItemTokens.Add("%MOBILENO%", AreadCode);
                                }
                                else
                                {
                                    objLineItemTokens.Add("%PORTTYPE%", "Mobile no");
                                    objLineItemTokens.Add("%MOBILENO%", MobileNo);
                                }
                            }

                            objLineItemTokens.Add("%CONTRACT%", Contract);
                           
                                if (!string.IsNullOrEmpty(RatePlans))
                                {
                                    string ratePlanTemplate = @"<tr><td><span lang='EN-IN' style='color: #696969; font-size:14px; font-weight: normal; margin-left: 5px;font-style: normal;'>Rate Plan:<span>&nbsp; </span>" + RatePlans + "</span></td></tr>";
                                    objLineItemTokens.Add("%RATEPLAN%", ratePlanTemplate);
                                    if (!string.IsNullOrEmpty(RatePlans))
                                        objLineItemTokens.Add("%RATEPLANVALUE%", RatePlans);
                                    else
                                        objLineItemTokens.Add("%RATEPLANVALUE%", string.Empty);
                                }
                                else
                                {
                                    objLineItemTokens.Add("%RATEPLAN%", string.Empty);
                                    objLineItemTokens.Add("%RATEPLANVALUE%", string.Empty);
                                }
                            
                            objLineItemTokens.Add("%USERNAME%", UserName);
                            if (!string.IsNullOrEmpty(Make))
                                objLineItemTokens.Add("%MAKE%", Make);
                            else
                                objLineItemTokens.Add("%MAKE%", string.Empty);

                            if (!string.IsNullOrEmpty(Model))
                                objLineItemTokens.Add("%MODEL%", Model);
                            else
                                objLineItemTokens.Add("%MODEL%", string.Empty);


                            if (!string.IsNullOrEmpty(Features))
                                objLineItemTokens.Add("%FEATURE%", Features);
                            else
                                objLineItemTokens.Add("%FEATURE%", string.Empty);

                            objLineItemTokens.Add("%IMEIMEID%", MEID_IMEI == "" ? "" : "<tr><td><span lang=\"EN-IN\" style=\"color: #696969; font-size:14px; font-weight: normal; margin-left: 5px;font-style: normal;\"><span style=\"\">IMEI/MEID:&nbsp; </span>" + MEID_IMEI + "</span></td></tr>");
                            objLineItemTokens.Add("%SIM%", SIMID == "" ? "" : "<tr><td><span lang=\"EN-IN\" style=\"color: #696969; font-size:14px; font-weight: normal; margin-left: 5px;font-style: normal;\"><span style=\"\">SIM:&nbsp; </span>" + SIMID + "</span></td></tr>");
                            //objLineItemTokens.Add("%LINENOTES%", txtLineNotes.Text == "" ? "" : "<tr><td><span lang=\"EN-IN\" style=\"color: #696969; font-size:14px; font-weight: normal; margin-left: 5px;font-style: normal;\"><span style=\"\">Line notes/Instruction:&nbsp; </span>" + txtLineNotes.Text + "</span></td></tr>");
                            if(MEID_IMEI == "")
                            {
                                objLineItemTokens.Remove("%IMEIMEID%");
                                objLineItemTokens.Add("%IMEIMEID%", "");
                            }
                            if (buttonCall == "Send Order Conf To Client")
                            {
                                string OrderNumberActivation = "<a  > " + lblOrderNo.Text + " </a> ";
                                objEmailTokens.Remove("%ORDERNUMBER%");
                                objEmailTokens.Add("%ORDERNUMBER%", OrderNumberActivation);
                                objLineItemTokens.Add("%LINENOTES%", "");
                                if (!objLineItemTokens.ContainsKey("%ACTNOTES%"))
                                    objLineItemTokens.Add("%ACTNOTES%", "");
                            }
                            else
                                objLineItemTokens.Add("%LINENOTES%", LineNotesInstruction == "" ? "" : "<tr><td><span lang=\"EN-IN\" style=\"color: #696969; font-size:14px; font-weight: normal; margin-left: 5px;font-style: normal;\"><span style=\"\">Line notes/Instruction:&nbsp; </span>" + LineNotesInstruction + "</span></td></tr>");

                            //objLineItemTokens.Add("%LINENOTES%", txtLineNotes.Text);
                            objLineItemTokens.Add("%IMEIMEIDVALUE%", MEID_IMEI);
                            objLineItemTokens.Add("%SIMVALUE%", SIMID);

                            listEmailTokens.Add(objLineItemTokens);
                        }
                        #endregion

                    }
                }
            }
        }
    }

    private string SaveOrderDetail(string buttonCall)
    {
        try
        {
            string retMsg = "";
            string pageName = Request.Url.AbsolutePath;
            if (rptLine.Items.Count > 0)
            {
                Initialize(adminorders);
                int lastItem = 0;

                DataTable dtRptLine = null;
                try
                {
                    dtRptLine = ViewState["rptLine"] as DataTable;
                }
                catch (Exception ex)
                {
                }
                DataTable dtForMail = null;
                Repeater rpt = hdnIsMultiEditBtn.Value == "1" ? rptLineItems : rptLine;
                foreach (RepeaterItem item in rpt.Items)
                {
                    if (lastItem == rptLine.Items.Count - 1 || (buttonCall == AppConstants.SAVEORDER || buttonCall == AppConstants.SAVEANDCLOSE
                        || buttonCall == AppConstants.SAVEONLY
                        || buttonCall == AppConstants.CREATEMANAGELINES || buttonCall == AppConstants.SUBMITACTIVATION || buttonCall == AppConstants.SAVEDEVICELEASE))
                    {
                        #region Save Order Line
                        HiddenField hdnOrderDetailId = item.FindControl("hdnOrderDetailId") as HiddenField;
                        TextBox txtMobileNo = item.FindControl("txtMobileNo") as TextBox;
                        TextBox txtAreaCode = item.FindControl("txtAreaCode") as TextBox;
                        TextBox txtUserName = item.FindControl("txtUserName") as TextBox;
                        TextBox txtLineNotes = item.FindControl("txtLineNotes") as TextBox;
                        TextBox txtActNotes = item.FindControl("txtActNotes") as TextBox;
                        TextBox txtMonthlyCost = item.FindControl("txtMonthlyCost") as TextBox;

                        TextBox txtMEIDIMEI = item.FindControl("txtMEIDIMEI") as TextBox;
                        TextBox txtSimID = item.FindControl("txtSimID") as TextBox;

                        DropDownList ddlPortIn = item.FindControl("ddlPortIn") as DropDownList;
                        DropDownList ddlContract = item.FindControl("ddlContract") as DropDownList;
                        DropDownList ddlMake = item.FindControl("ddlMake") as DropDownList;
                        DropDownList ddlModel = item.FindControl("ddlModel") as DropDownList;
                        DropDownList ddldivRatePlan = item.FindControl("ddldivRatePlan") as DropDownList;
                        DropDownList ddlFeatures = item.FindControl("ddlFeatures") as DropDownList;

                        Label lblItemOrderStatus = item.FindControl("lblOrderStatus") as Label;
                        Label lblErrorMessage = item.FindControl("lblErrorMessage") as Label;

                        bool toSave = false;
                        objOrders.OrderId = hdnOrderId.Value == "" ? 0 : Convert.ToInt32(hdnOrderId.Value);
                        objOrders.OrderDetailId = hdnOrderDetailId.Value == "" ? 0 : Convert.ToInt32(hdnOrderDetailId.Value);
                        objOrders.SPType = objOrders.OrderDetailId == 0 ? AppConstants.INSERT : AppConstants.UPDATE;

                        if (buttonCall != AppConstants.CREATEMANAGELINES && buttonCall != AppConstants.SAVEANDCLOSE
                            && buttonCall != AppConstants.SAVEDEVICELEASE && buttonCall != AppConstants.SAVEONLY)
                            objOrders.MobileNo = AppHelper.FormatMaskedMobileTextBox(ddlPortIn.SelectedValue == "1" ? txtMobileNo : txtAreaCode, this.Request);
                        else
                            objOrders.MobileNo = AppHelper.FormatMaskedMobileTextBox(txtMobileNo) != "" ? AppHelper.FormatMaskedMobileTextBox(txtMobileNo) : AppHelper.FormatMaskedMobileTextBox(txtMobileNo, this.Request);
                        if (pageName.ToLower().Contains("admindashordertasks"))
                        {
                            objOrders.MobileNo = AppHelper.FormatMaskedMobileTextBox(txtMobileNo);
                        }
                        objOrders.PortIn = ddlPortIn.SelectedValue;
                        objOrders.ContractId = Convert.ToInt32(ddlContract.SelectedValue);
                        if (objOrders.ContractId > 0)
                        {
                            toSave = true;
                        }
                        objOrders.UserName = txtUserName.Text.Trim();
                        objOrders.MakeId = ddlMake.SelectedValue == "" ? 0 : Convert.ToInt32(ddlMake.SelectedValue);
                        objOrders.ModelId = ddlModel.SelectedValue == "" ? 0 : Convert.ToInt32(ddlModel.SelectedValue);
                        objOrders.MEID_IMEI = txtMEIDIMEI != null ? (txtMEIDIMEI.Text == "0" ? "" : txtMEIDIMEI.Text) : "";
                        objOrders.SIMID = txtSimID != null ? (txtSimID.Text == "0" ? "" : txtSimID.Text) : "";
                        objOrders.LineNotesInstruction = txtLineNotes.Text.Trim();
                        objOrders.ActOrderInternalNotes = txtActNotes.Text.Trim();
                        objOrders.ResponseToOrderDesk = "";
                        objOrders.CarrierId = ddlCarrier.SelectedValue == "" ? 0 : Convert.ToInt32(ddlCarrier.SelectedValue);
                        objOrders.RatePlanId = ddldivRatePlan.SelectedValue == "" ? 0 : Convert.ToInt32(ddldivRatePlan.SelectedValue);
                        objOrders.FeatureId = ddlFeatures.SelectedValue == "" ? 0 : Convert.ToInt32(ddlFeatures.SelectedValue);
                        objOrders.MonthlyCost = Conversion.ParseDecimal(txtMonthlyCost.Text);
                        objOrders.DeviceType = "";
                        objOrders.DeviceID = "";
                        objOrders.ServiceDetails = "";
                        objOrders.OrderStatus = AppHelper.GetOrderStatus(buttonCall, objOrders.OrderDetailId);
                        objOrders.ClientID = Convert.ToInt32(AppHelper.GetValueFromQueryString(ClientId, this.Context));
                        objOrders.IsActive = 1;
                        objOrders.IsDeleted = 0;
                        objOrders.CreatedDate = DateTime.Now;
                        objOrders.CreatedBy = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);
                        objOrders.OrderBy = txtOredrBy.Text.Trim();
                        if (objOrders.MobileNo.Length > 3)
                            retMsg = CheckOrderForMobileNo(objOrders.MobileNo, buttonCall);

                        if (toSave && retMsg == "")
                        {
                            bool isChanged = CompareItem(dtRptLine, hdnOrderDetailId, txtUserName, txtLineNotes, txtActNotes, txtMonthlyCost
                                , txtMEIDIMEI, txtSimID, ddlPortIn, ddlContract, ddlMake, ddlModel);

                            if (buttonCall == AppConstants.SAVEORDER && objOrders.OrderStatus != lblItemOrderStatus.Text)
                            { isChanged = true; }

                            if (isChanged)
                            {
                                int noOfCopy = 0;
                                int.TryParse(txtNoOfCopy.Text, out noOfCopy);
                                if (noOfCopy == 0)
                                {
                                    noOfCopy = 1;
                                }
                                for (int i = 1; i <= noOfCopy; i++)
                                {
                                    if (i > 1)
                                    {
                                        objOrders.OrderDetailId = 0;
                                    }
                                    int orderdetailId = objOrders.SaveOrderDetail(out retMsg);
                                    lblErrorMessage.Text = retMsg;
                                    if (orderdetailId > 0)
                                    {

                                        if (string.IsNullOrEmpty(retMsg))
                                        {
                                            AppHelper.SetMessage("Successfully saved details.", lblMsg);
                                            AppHelper.SetMessage("Successfully saved details.", lblMsgDown);
                                        }
                                    }
                                    else
                                    {
                                        AppHelper.SetMessage(retMsg, lblMsg);
                                        AppHelper.SetMessage(retMsg, lblMsgDown);
                                    }
                                }
                            }

                            if (buttonCall == AppConstants.SAVEDEVICELEASE)
                                retMsg = SaveOrderDeviceLease(buttonCall, Convert.ToInt32(objOrders.OrderDetailId), item.ItemIndex);
                        }
                        else if (retMsg != "")
                        {
                            AppHelper.SetMessage(retMsg, lblMsg);
                            AppHelper.SetMessage(retMsg, lblMsgDown);
                        }

                        #endregion
                    }

                    if (lastItem != rptLine.Items.Count - 1)
                    {
                        lastItem += 1;
                    }
                }
                ViewState["dtForMail"] = dtForMail;
                if (buttonCall == AppConstants.NEWLINE || hdnIsMultiEditBtn.Value == "1")
                {
                    LoadOrderRelated();
                }
                else if (buttonCall != AppConstants.SUBMITORDER && buttonCall != AppConstants.SUBMITACTIVATION
                    && buttonCall != AppConstants.CREATEMANAGELINES && string.IsNullOrEmpty(retMsg)
                && buttonCall != AppConstants.SAVEANDCLOSE && buttonCall != AppConstants.TASKRESPONSE && buttonCall != AppConstants.SAVEDEVICELEASE
                && buttonCall != AppConstants.SAVEONLY)
                {
                    Response.Redirect("ManageOrdersNewAct.aspx?ClientId=" + GetQueryId(ClientId) + "&OrderId=" + objOrders.OrderId
                        + "&sot=" + hdnOrderTypeSub.Value + "&id=" + GetQueryId(TaskId)
                        + (buttonCall == AppConstants.SAVEORDER ? "&so=1" : "")
                        + (buttonCall != AppConstants.NEWLINE ? GetDeleteLineIndex() : ""));
                }
            }

            return retMsg;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }



    private void LoadOrder()
    {
        try
        {
            DataSet dsData = LoadOrderOnLoad();
            if (dsData.Tables.Count > 0)
            {
                DataTable dtOrder = dsData.Tables[0];
                if (dtOrder != null && dtOrder.Rows.Count > 0)
                {
                    hdnOrderId.Value = Convert.ToString(dtOrder.Rows[0]["OrderId"]);
                    lblOrderNo.Text = Convert.ToString(dtOrder.Rows[0]["OrderNumber"]);
                    lblOrderStatus.Text = Convert.ToString(dtOrder.Rows[0]["OrderStatus"]);
                    hdnOrderStatusSub.Value = Convert.ToString(dtOrder.Rows[0]["OrderStatusSub"]);

                    LoadClientDetails(objManageMasters, dtOrder);

                    ddlShippingAddress.SelectedValue = Convert.ToString(dtOrder.Rows[0]["ShippingAddressId"]);
                    ddlShippingOption.SelectedValue = Convert.ToString(dtOrder.Rows[0]["ShippingOptionId"]);
                    chkNoShippingRequired.Checked = Conversion.ParseDBNullBool(dtOrder.Rows[0]["IsShippingRequired"].GetStringFromObject()) ? true : false;
                    ShippingRequired();
                    txtSpoc.Text = Convert.ToString(dtOrder.Rows[0]["Spoc"]);
                    txtSpocEmail.Text = Convert.ToString(dtOrder.Rows[0]["SpocEmail"]);
                    string installMDM = Convert.ToString(dtOrder.Rows[0]["InstallMDM"]);
                    if (installMDM != "")
                        rdoInstallMDM.SelectedValue = installMDM == "True" ? "1" : "0";
                    txtShippingTrackingNumber.Text = Convert.ToString(dtOrder.Rows[0]["ShippingTrackingNumber"]);
                    ddlShippingMethod.SelectedValue = Convert.ToString(dtOrder.Rows[0]["ShippingMethod"]);
                    txtOrderInstructions.Text = Convert.ToString(dtOrder.Rows[0]["OrderInstructions"]);
                    txtOrderEmails.Text = Convert.ToString(dtOrder.Rows[0]["OrderEmails"]);
                    chkMarkUrgent.Checked = dtOrder.Rows[0]["IsUrgent"] != null ? Conversion.ParseBool(dtOrder.Rows[0]["IsUrgent"]) : false;
                    txtAssociatedName.Text = Convert.ToString(dtOrder.Rows[0]["AssociatedUserFullName"]);
                    txtAssociatedEmail.Text = Convert.ToString(dtOrder.Rows[0]["EmailAddress"]);
                    //txtOredrBy.Text = Convert.ToString(dtOrder.Rows[0]["OrderBy"]);
                    txtBoxHight.Text = Convert.ToString(dtOrder.Rows[0]["DimensionsBoxHeight"]);
                    if (txtBoxHight.Text == "")
                    {
                        txtBoxHight.Text = "5";
                    }
                    txtBoxLength.Text = Convert.ToString(dtOrder.Rows[0]["DimensionsBoxLenght"]);
                    if (txtBoxLength.Text == "")
                    {
                        txtBoxLength.Text = "5";
                    }
                    txtBoxWidth.Text = Convert.ToString(dtOrder.Rows[0]["DimensionsBoxWidth"]);
                    if (txtBoxWidth.Text == "")
                    {
                        txtBoxWidth.Text = "5";
                    }
                    // txtDeviceWeight.Text= Convert.ToString(dtOrder.Rows[0]["ShippingLabelWeight"]);
                    txnetActivationGain.Text = Convert.ToString(dtOrder.Rows[0]["NewNetGain"]);
                    txtRefreshGain.Text = Convert.ToString(dtOrder.Rows[0]["RefreshNetGain"]);
                    txtOredrBy.Text = Convert.ToString(dtOrder.Rows[0]["OrderBy"]);
                    //int newNetGain = Convert.ToInt32(txnetActivationGain.Text);
                    //int RefreshNetGain = Convert.ToInt32(txnetActivationGain.Text);

                    string oclientSTatus = Convert.ToString(dtOrder.Rows[0]["ClientStatus"]);
                    if (oclientSTatus != "")
                        rdoclientStatus.SelectedValue = oclientSTatus == "True" ? "1" : "0";

                    string rdoGain = Convert.ToString(dtOrder.Rows[0]["ActivationGain"]);
                    //if (rdoGain != "")
                    //    rdogain.SelectedValue = rdoGain == "True" ? "1" : "0";

                    //int totalActivationGain = 0;
                    //int.TryParse(dtOrder.Rows[0]["TotalActivationGain"].ToString(), out totalActivationGain);
                    //if (totalActivationGain > 0)
                    //{
                    //    TotalActivationGain.Text = totalActivationGain.ToString();
                    //}

                    try
                    {
                        var option = Convert.ToString(dtOrder.Rows[0]["ShippingOptionName"]);
                        ddlShippingOptionLabel.SelectedValue = option;
                    }
                    catch (Exception ex)
                    {
                        ddlShippingOptionLabel.SelectedValue = Convert.ToString(dtOrder.Rows[0]["ShippingOptionLabel"]);
                    }


                    if (lblOrderStatus.Text == AppConstants.SAVEORDER)
                    {
                        btnSubmitOrder.Visible = true;
                    }
                    if (lblOrderStatus.Text == AppConstants.SUBMITORDER)
                    {
                        btnSaveOrder.Visible = false;
                        btnAddNewLine.Visible = false;
                        btnCopytoNewLine.Visible = false;
                        txtNoOfCopy.Visible = false;
                        pnlShippingInfo.Visible = true;
                    }
                    if (lblOrderStatus.Text == AppConstants.CREATEMANAGELINES)
                    {
                        btnSaveOrder.Visible = false;
                        btnSubmitOrder.Visible = false;
                        btnAddNewLine.Visible = false;
                        btnCopytoNewLine.Visible = false;
                        txtNoOfCopy.Visible = false;
                        pnlInstallMDM.Enabled = false;
                        pnlShippingInfo.Visible = true;

                        btnCreateShippingLabel.Enabled = false;
                    }
                    if (Convert.ToString(dtOrder.Rows[0]["StatusName"]) == AppConstants.CLOSED
                        || Convert.ToString(dtOrder.Rows[0]["StatusName"]) == "TASK-CLOSED")
                    {
                        btnSaveOrder.Visible = false;
                        btnSubmitOrder.Visible = false;
                        pnlInstallMDM.Enabled = false;
                        btnCreateShippingLabel.Enabled = false;
                        LoadPageControls(true);

                        if (dtOrder.Rows[0]["OrderTypeSub"].GetStringFromObject() == AppConstants.TLNewActivationOrderWSFulfillment
                            || dtOrder.Rows[0]["OrderTypeSub"].GetStringFromObject() == AppConstants.TLNewActivationOrderCarrierFulfillment)
                        {
                            pnlShippingSales.Visible = true;
                        }
                    }
                    //btnClose.Style.Add("float", "right");
                    //btnSaveOrder.Style.Add("float", "right");
                }
                else
                {
                    LoadClientData(GetQueryId(ClientId));
                }

                int queryTaskId = GetQueryId(TaskId);
                if (queryTaskId > 0)
                {
                    GetTaskValues();
                }
                else
                {
                    DataTable dtOrderDetails = dsData.Tables.Count > 1 ? dsData.Tables[1] : null;

                    if (dtOrderDetails != null)
                    {
                        LoadOrderDetails(dtOrderDetails);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            lblMsg.Text = "LoadOrder() : " + ex.Message.ToString();
        }
    }

    private DataSet LoadOrderOnLoad()
    {
        Initialize(adminorders);
        objOrders.SPType = LoadPageControls() ? AppConstants.TASKVIEW : AppConstants.OPEN;
        objOrders.OrderId = GetQueryId("OrderId");
        objOrders.OrderDetailId = 0;
        objOrders.ClientId = GetQueryId(ClientId);
        objOrders.AdminTaskId = GetQueryId(TaskId);

        DataSet dsData = objOrders.GetOrders();
        return dsData;
    }

    private void LoadOrderDetails(DataTable dtOrderDetails)
    {
        try
        {
            if (dtOrderDetails != null)
            {
                var co = AppHelper.GetValueFromQueryString("co", HttpContext.Current);
                if (co.GetStringFromObject() != "")
                {
                    var dtOD = dtOrderDetails.AsEnumerable().Where(d => d["OrderId"].GetStringFromObject() != "0");
                    if (dtOD != null && dtOD.Count() > 0)
                        dtOrderDetails = dtOD.CopyToDataTable();
                }
                hdnItemsCount.Value = dtOrderDetails.Rows.Count.ToString();

                rptLine.DataSource = dtOrderDetails.AsEnumerable().Where(d => d["OrderDetailId"].GetStringFromObject() == "0").CopyToDataTable();
                rptLine.DataBind();
                ViewState["rptLine"] = dtOrderDetails;
                if (dtOrderDetails.Rows.Count > 0)
                {
                    dtOrderDetails.Rows.RemoveAt(dtOrderDetails.Rows.Count - 1);
                }

                grdOrderInfo.DataSource = dtOrderDetails;
                grdOrderInfo.DataBind();


                SetTriggers();
            }
        }
        catch (Exception ex)
        {
            lblMsg.Text = "LoadOrderDetails : " + ex.Message.ToString();
        }
    }

    private void SetTriggers()
    {
        foreach (RepeaterItem item in rptLine.Items)
        {
            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
            {
                DropDownList ddlMake = item.FindControl("ddlMake") as DropDownList;
                DropDownList ddlModel = item.FindControl("ddlModel") as DropDownList;
                DropDownList ddldivRatePlan = item.FindControl("ddldivRatePlan") as DropDownList;

                if (this.Page.Controls[0].FindControl("ContentPlaceHolder1") != null
                   && this.Page.Controls[0].FindControl("ContentPlaceHolder1").FindControl("sc") != null)
                {
                    ScriptManager scriptManager = this.Page.Controls[0].FindControl("ContentPlaceHolder1").FindControl("sc") as ScriptManager;
                    scriptManager.RegisterAsyncPostBackControl(ddlMake);
                    scriptManager.RegisterAsyncPostBackControl(ddlModel);
                    scriptManager.RegisterAsyncPostBackControl(ddldivRatePlan);
                }

            }
        }

    }

    protected void rptLine_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
        {
            DropDownList ddlPortIn = e.Item.FindControl("ddlPortIn") as DropDownList;
            DropDownList ddlContract = e.Item.FindControl("ddlContract") as DropDownList;

            DropDownList ddlMake = e.Item.FindControl("ddlMake") as DropDownList;
            DropDownList ddlModel = e.Item.FindControl("ddlModel") as DropDownList;
            DropDownList ddldivRatePlan = e.Item.FindControl("ddldivRatePlan") as DropDownList;

            DropDownList ddlFeatures = e.Item.FindControl("ddlFeatures") as DropDownList;

            HtmlGenericControl divAreaCode = e.Item.FindControl("divAreaCode") as HtmlGenericControl;
            HtmlGenericControl divMobile = e.Item.FindControl("divMobile") as HtmlGenericControl;
            HtmlGenericControl divManageLines = e.Item.FindControl("divManageLines") as HtmlGenericControl;

            TextBox txtMEIDIMEI = e.Item.FindControl("txtMEIDIMEI") as TextBox;
            FilteredTextBoxExtender FilteredTextBoxExtender1 = e.Item.FindControl("FilteredTextBoxExtender1") as FilteredTextBoxExtender;
            RequiredFieldValidator rftxtMEIDIMEI = e.Item.FindControl("rftxtMEIDIMEI") as RequiredFieldValidator;
            TextBox txtSimID = e.Item.FindControl("txtSimID") as TextBox;
            FilteredTextBoxExtender FilteredTextBoxExtender6 = e.Item.FindControl("FilteredTextBoxExtender6") as FilteredTextBoxExtender;
            RequiredFieldValidator rftxtSimID = e.Item.FindControl("rftxtSimID") as RequiredFieldValidator;


            Button btnDeleteLine = e.Item.FindControl("btnDeleteLine") as Button;
            Label lblItemOrderStatus = e.Item.FindControl("lblOrderStatus") as Label;
            //HiddenField hdnOrderStatusSub = e.Item.FindControl("hdnOrderStatusSub") as HiddenField;
            //var hdnOrderStatusSub = Page.Parent.FindControl("hdnOrderStatusSub");

            AjaxControlToolkit.MaskedEditValidator maskValidator = e.Item.FindControl("MaskedEditValidator1") as AjaxControlToolkit.MaskedEditValidator;

            DataRowView item = e.Item.DataItem as DataRowView;
            var deviceType = Convert.ToString((item["DeviceType"])).ToLower();
            SimValidation(rftxtSimID, deviceType);

            if (ddlContract != null) LoadContract(ddlContract);
            if (ddlMake != null) AppHelper.GetCarrierMakeModel(ddlCarrier, ddlMake, objManageMasters);
            if (ddldivRatePlan != null) BindDropDownRateplan(ddlCarrier.SelectedValue, ddldivRatePlan);


            string portIn = Convert.ToString((item["PortIn"]));
            if (ddlPortIn != null)
            {
                ddlPortIn.SelectedValue = (portIn == "True" || portIn == "1") ? "1" : "0";

                SetDIVShow(ddlPortIn, divAreaCode, divMobile, maskValidator, lblItemOrderStatus.Text, divManageLines);
            }
            string contractId = Convert.ToString((item["ContractId"]));
            if (!string.IsNullOrEmpty(contractId))
            {
                ddlContract.SelectedValue = contractId;
            }
            if (ddlFeatures != null) LoadFeatures(ddlFeatures, Convert.ToInt32(ddlCarrier.SelectedValue));
            string featureId = Convert.ToString((item["FeatureId"]));
            if (!string.IsNullOrEmpty(featureId))
            {
                ddlFeatures.SelectedValue = featureId;
            }

            string rateplanId = Convert.ToString((item["rateplanId"]));
            if (!string.IsNullOrEmpty(rateplanId))
            {
                ddldivRatePlan.SelectedValue = rateplanId;
            }

            string makeId = Convert.ToString((item["MakeId"]));
            if (!string.IsNullOrEmpty(makeId) && makeId != "0")
            {
                ddlMake.SelectedValue = makeId;
                BindModel(Convert.ToInt16(makeId), ddlModel);

                string modelId = Convert.ToString((item["ModelId"]));
                ddlModel.SelectedValue = modelId;
            }
            divManageLines.Visible = false;
            if (LoadPageControls())
            {
                divManageLines.Visible = true;
            }


            int orderDetailsCount = 0;
            Int32.TryParse(hdnItemsCount.Value, out orderDetailsCount);
            btnDeleteLine.Visible = true;
            //if (orderDetailsCount > 1)
            //    if (orderDetailsCount - 1 == e.Item.ItemIndex && orderDetailsCount - 1 != 0
            //    && ((hdnOrderStatusSub.Value == AppConstants.SAVEORDER || hdnOrderStatusSub.Value == AppConstants.NEWLINE)
            //    && (lblItemOrderStatus.Text == AppConstants.NEW || lblItemOrderStatus.Text == AppConstants.SAVEORDER)))
            if (orderDetailsCount == 1)
            {
                btnDeleteLine.Visible = false;
            }

            if (GetDeleteLineIndex() == "&d=" + e.Item.ItemIndex.ToString())
            {
                e.Item.Visible = false;
            }

            if (lblOrderStatus.Text == AppConstants.SUBMITORDER || lblOrderStatus.Text == AppConstants.LINECREATED)
            {
                ddlMake.Enabled = false;
                ddlModel.Enabled = false;
            }

            #region Load details from Task
            int queryTaskId = GetQueryId(TaskId);
            if (queryTaskId == 0) queryTaskId = GetQueryId(AdminTaskId);

            if (queryTaskId > 0)
            {
                string department = "", make = "", model = "", username = "", areacode = "";
                if (lblOrderStatus.Text == AppConstants.SAVED)
                {
                    GetTaskValues();
                }
                /*
                 * LoadTaskValues("", ref department, ref make, ref model, ref username, ref areacode);

                if (make != "")
                {
                    ddlMake.SelectedValue = ddlMake.Items.FindByText(make).Value;

                    //ddlModel = e.Item.FindControl("ddlModel") as DropDownList;

                    BindModel(Convert.ToInt16(ddlMake.SelectedValue), ddlModel);
                    ddlModel.SelectedValue = ddlModel.Items.FindByText(model).Value;

                    TextBox txtLineNotes = e.Item.FindControl("txtLineNotes") as TextBox;
                    TextBox txtAreaCode = e.Item.FindControl("txtAreaCode") as TextBox;
                    TextBox txtUserName = e.Item.FindControl("txtUserName") as TextBox;

                    txtLineNotes.Text = department;
                    txtAreaCode.Text = areacode;
                    txtUserName.Text = username;
                }
                */
            }
            #endregion

            bool disableControls = false;
            bool enableFields = disableControls;
            bool isWSFF = LoadPageControlsSubOrder(AppConstants.TLNewActivationOrderWSFF, divManageLines, out disableControls, false);

            if (isWSFF)
            {
                isSalesUser = CommonMethods.CheckIfSalesUser();

                if ((isSalesUser && lblOrderStatus.Text == AppConstants.SUBMITORDER) || hdnOrderStatusSub.Value == AppConstants.SUBMITACTIVATION
                     || hdnOrderStatusSub.Value == AppConstants.SAVEANDCLOSE || hdnOrderStatusSub.Value == AppConstants.SAVEONLY)
                {
                    enableFields = true;
                    divManageLines.Visible = enableFields;
                    pnlShippingOption.Visible = enableFields;
                    pnlInstallMDM.Visible = true;
                    rdoInstallMDM.Enabled = false;
                    pnlShippingSales.Visible = enableFields;
                }
                else
                {
                    //enableFields = disableControls;// Based on Login user
                    //divManageLines.Visible = enableFields;
                    //pnlShippingSales.Visible = enableFields;
                    //txtMEIDIMEI.Enabled = enableFields;
                    //FilteredTextBoxExtender1.Enabled = enableFields;
                    //rftxtMEIDIMEI.Enabled = enableFields;
                    //txtSimID.Enabled = enableFields;
                    //FilteredTextBoxExtender6.Enabled = enableFields;
                    //rftxtSimID.Visible = enableFields;
                }
                if (hdnOrderStatusSub.Value == AppConstants.SUBMITORDER || hdnOrderStatusSub.Value == AppConstants.SUBMITACTIVATION)
                {
                    rdoInstallMDM.Enabled = false;
                }

                if (hdnOrderStatusSub.Value == "" || hdnOrderStatusSub.Value == AppConstants.SAVEORDER)
                {
                    rdoInstallMDM.Enabled = true;
                }
            }
            else
            {
                pnlInstallMDM.Visible = false;
            }


            if (hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderCFF
            && (lblOrderStatus.Text == AppConstants.SUBMITORDER || lblOrderStatus.Text == AppConstants.SAVEORDER
            || hdnOrderStatusSub.Value == AppConstants.SAVEANDCLOSE || hdnOrderStatusSub.Value == "" || hdnOrderStatusSub.Value == AppConstants.SAVEONLY))
            {
                rdoInstallMDM.Enabled = true;
                pnlInstallMDM.Visible = true;
                enableFields = true;
                pnlShippingSales.Visible = enableFields;

                if (hdnOrderStatusSub.Value == AppConstants.SUBMITORDER || hdnOrderStatusSub.Value == AppConstants.SUBMITACTIVATION)
                {
                    rdoInstallMDM.Enabled = false;
                }
            }

            disableControls = CheckForCODOrder(divManageLines, rftxtSimID, FilteredTextBoxExtender6);
        }
    }

    private void GetTaskValues()
    {
        ClientDashboard objClient = new ClientDashboard();
        objClient.WSUPDeviceBatchId = 0;
        objClient.AdminTaskId = GetQueryId(TaskId);
        objClient.ClientId = Conversion.ParseInt(Session[AppConstants.SessionClientId]);
        DataSet ds = objClient.Client_GetWSUPClientPhoneDetails();
        //Order created. Redirect to order page.        
        if (ds != null && ds.Tables.Count > 3 && ds.Tables[3].Rows.Count > 0)
        {
            hdnOrderId.Value = Conversion.ParseString(ds.Tables[3].Rows[0]["OrderId"]);
            string subActionType = Conversion.ParseString(ds.Tables[2].Rows[0]["SubActionType"]);
            if (ds.Tables[2].Rows.Count == 1)
                ddlShippingAddress.SelectedValue = Conversion.ParseString(ds.Tables[2].Rows[0]["ShippingAddressId"]);
            if (subActionType == AppConstants.ActionNEWLINEONEXISTINGDEVICE)
            {
                hdnOrderTypeSub.Value = AppConstants.TLNewActivationOrderCOD;
            }

            if (subActionType == AppConstants.ActionNEWLINEONEXISTINGDEVICE || subActionType == AppConstants.ActionDEVICEUPGRADE)
            {
                hdnOrderTypeSub.Value = AppConstants.TLNewActivationOrderCOD;
            }
            if (subActionType == AppConstants.ActionNEWLINEANDDEVICE)
            {
                hdnOrderTypeSub.Value = AppConstants.TLNewActivationOrderCFF;
            }

            if (hdnOrderId.Value != "" && hdnOrderId.Value != "0")
            {
                Response.Redirect("ManageOrdersNewAct.aspx?ClientId=" + GetQueryId(ClientId) + "&OrderId=" + hdnOrderId.Value
                    + "&id=" + GetQueryId(TaskId) + "&sot=" + hdnOrderTypeSub.Value + "&co=1");
            }
        }
        //Create new order for conversion
        if (ds != null && ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
        {
            hdnAssociatedUserId.Value = Conversion.ParseString(ds.Tables[2].Rows[0]["CreatedBy"]);
            string subActionType = Conversion.ParseString(ds.Tables[2].Rows[0]["SubActionType"]);
            if (ds.Tables[2].Rows.Count == 1)
                ddlShippingAddress.SelectedValue = Conversion.ParseString(ds.Tables[2].Rows[0]["ShippingAddressId"]);
            if (subActionType == AppConstants.ActionNEWLINEONEXISTINGDEVICE || subActionType == AppConstants.ActionDEVICEUPGRADE)
            {
                hdnOrderTypeSub.Value = AppConstants.TLNewActivationOrderCOD;
            }
            if (subActionType == AppConstants.ActionNEWLINEANDDEVICE)
            {
                hdnOrderTypeSub.Value = AppConstants.TLNewActivationOrderCFF;
            }
            SaveOrderMethod(AppConstants.NEWLINE, ds.Tables[2]);

            Response.Redirect("ManageOrdersNewAct.aspx?ClientId=" + GetQueryId(ClientId) + "&OrderId=" + hdnOrderId.Value
                + "&id=" + GetQueryId(TaskId) + "&sot=" + hdnOrderTypeSub.Value + "&co=1");
        }
    }
    private bool CheckForCODOrder(HtmlGenericControl divManageLines, RequiredFieldValidator rftxtSimID, FilteredTextBoxExtender filteredTextBoxExtender)
    {
        bool disableControls;
        bool isCOD = LoadPageControlsSubOrder(AppConstants.TLNewActivationOrderCOD, divManageLines, out disableControls, false);
        if (isCOD)
        {
            pnlCOD.Visible = true;
            divManageLines.Visible = true;
            divManageLines.Style.Add("display", "block");
            //rftxtSimID.Visible = false;
            rftxtSimID.Enabled = false;
            filteredTextBoxExtender.Enabled = false;

            ForCOD(true);
        }

        return disableControls;
    }

    private void SetDIVShow(DropDownList ddlPortIn, HtmlGenericControl divAreaCode, HtmlGenericControl divMobile
        , AjaxControlToolkit.MaskedEditValidator maskValidator, string rowStatus, HtmlGenericControl divManageLines)
    {
        if (divAreaCode != null && divMobile != null)
        {
            if (ddlPortIn.SelectedValue == "0")
            {
                divAreaCode.Style.Add("display", "block");
                divMobile.Style.Add("display", "none");
                divManageLines.Style.Add("display", "none");
            }
            if (ddlPortIn.SelectedValue == "1" || LoadPageControls())
            {
                divAreaCode.Style.Add("display", "none");
                divMobile.Style.Add("display", "block");
                divManageLines.Style.Add("display", "block");
            }

            if (lblOrderStatus.Text == AppConstants.SUBMITORDER)
            {

            }
            //if (rowStatus == AppConstants.NEW)
            //    maskValidator.Enabled = true;
            //else
            if (maskValidator != null)
                maskValidator.Enabled = false;
        }
    }

    protected void rptLine_ItemCreated(object sender, RepeaterItemEventArgs e)
    {
        DropDownList ddlMake = (DropDownList)e.Item.FindControl("ddlMake");
        ddlMake.SelectedIndexChanged += ddlMake_SelectedIndexChanged;
    }

    protected void btnAddNewLine_Click(object sender, EventArgs e)
    {
        txtNoOfCopy.Text = "1";
        AddNewLine();
        btnAddNewLine.Text = "Add";
    }

    private void AddNewLine()
    {
        try
        {
            hdnDeleteLine.Value = "";
            SaveItem(AppConstants.NEWLINE);
            grdAttachments.Focus();
        }
        catch (Exception ex)
        {
            //lblMsg.Text = "AddNewLine() " + ex.Message.ToString();
        }
    }

    private string SaveItem(string buttoncall)
    {
        string validationMessage = CheckForDuplicateMobileNo("");
        if (string.IsNullOrEmpty(validationMessage))
        {
            if (hdnOrderId.Value == "" || hdnOrderId.Value == "0")
            {
                SaveOrder(buttoncall);
            }
            if (hdnOrderId.Value != "" && hdnOrderId.Value != "0")
            {
                //if (buttoncall == AppConstants.NEWLINE)
                //    SaveOrderDetailFull(buttoncall);
                //else
                SaveOrderDetail(buttoncall);
                if (lblMsg.Text == "" && lblMsgDown.Text == "")
                    LoadOrder();
            }
        }
        else
        {
            SetMessage(validationMessage);
        }
        return validationMessage;
    }

    protected void Paging_Click(object sender, CommandEventArgs e)
    {

    }


    protected void btnUpdateMultiDetails_Click(object sender, EventArgs e)
    {
        DataTable dt = (DataTable)ViewState["rptLine"];
        DataTable dtNew = dt.Clone();
        hdnIsMultiEditBtn.Value = "1";
        //if (dt.Rows.Count>0)
        //{
        //    rptLineItems.DataSource = dt;
        //    rptLineItems.DataBind();
        //}
        mdlMultiItemUpdateForm.Show();
        //return;
        //DropDownList ddlContract, ddlMake, ddlModel, ddldivRatePlan, ddlFeatures, ddlPortIn;
        //TextBox txtMobileNo, txtMEIDIMEI, txtSimID, txtUserName, txtActNotes, txtLineNotes, txtMonthlyCost, txtAreaCode;
        //GetControlsFromOrderGrid(rptLine.Items[0], out ddlContract, out ddlMake, out ddlModel, out ddldivRatePlan, out ddlFeatures, out txtMobileNo, out txtMEIDIMEI, out txtSimID, out txtUserName, out txtActNotes, out txtLineNotes, out txtMonthlyCost, out ddlPortIn, out txtAreaCode);


        //check for checked row
        foreach (GridViewRow item in grdOrderInfo.Rows)
        {
            CheckBox chk = item.FindControl("chkOrderDetail") as CheckBox;
            if (chk != null && chk.Checked)
            {
                HiddenField hdnOrderDetailId = item.FindControl("hdnOrderDetailId") as HiddenField;
                foreach (DataRow dr in dt.Rows)
                {

                    if (dr["OrderDetailId"].ToString() == hdnOrderDetailId.Value)
                    {
                        //if (ddlPortIn.SelectedValue != dr["PortIn"].ToString())
                        //{
                        //    dr["PortIn"] = ddlPortIn.SelectedValue;
                        //}

                        //string strMobileNo=AppHelper.FormatMaskedMobileTextBox(ddlPortIn.SelectedValue == "1" ? txtMobileNo : txtAreaCode, this.Request);
                        //if (strMobileNo!="")
                        //{
                        //    dr["MobileNo"] = strMobileNo;
                        //}

                        //if(txtUserName.Text.Trim()!="")
                        //{
                        //    dr["UserName"] = txtUserName.Text.Trim();
                        //}

                        //if (ddlContract.SelectedValue!="0" && ddlContract.SelectedValue!=null)
                        //{
                        //    dr["ContractId"] = ddlContract.SelectedValue;
                        //}

                        //if (ddldivRatePlan.SelectedValue!="0")
                        //{
                        //    dr["RatePlanId"] = ddldivRatePlan.SelectedValue;
                        //}

                        //if (ddlMake.SelectedValue != "0")
                        //{
                        //    dr["MakeId"] = ddlMake.SelectedValue;
                        //}

                        //if (ddlModel.SelectedValue != "0" && ddlModel.SelectedValue!="")
                        //{
                        //    dr["ModelId"] = ddlModel.SelectedValue;
                        //}

                        //if (ddlFeatures.SelectedValue != "0")
                        //{
                        //    dr["FeatureId"] = ddlFeatures.SelectedValue;
                        //}

                        //double monthlyCost = 0;
                        //Double.TryParse(txtMonthlyCost.Text, out monthlyCost);
                        //if (txtMonthlyCost.Text.Trim() != "" && monthlyCost>0)
                        //{
                        //    dr["MonthlyCost"] = txtMonthlyCost.Text.Trim();
                        //}

                        //if (txtActNotes.Text.Trim() != "")
                        //{
                        //    dr["ActOrderInternalNotes"] = txtActNotes.Text.Trim();
                        //}

                        //if (txtLineNotes.Text.Trim() != "")
                        //{
                        //    dr["LineNotesInstruction"] = txtLineNotes.Text.Trim();
                        //}
                        dtNew.ImportRow(dr);
                    }
                }
                Label lblItemOrderStatus = item.FindControl("lblOrderStatus") as Label;
                Label lblErrorMessage = item.FindControl("lblErrorMessage") as Label;
            }
        }

        if (dtNew.Rows.Count > 0)
        {
            rptLineItems.DataSource = dtNew;
            rptLineItems.DataBind();
        }

        //if (dtNew.Rows.Count > 0)
        //{
        //    string retMsg = "";
        //    foreach (DataRow dr in dtNew.Rows)
        //    {
        //        Initialize(adminorders);
        //        objOrders.OrderId = long.Parse(dr["OrderId"].ToString());
        //        objOrders.OrderDetailId = long.Parse(dr["OrderDetailId"].ToString());
        //        objOrders.SPType = AppConstants.UPDATE;
        //        objOrders.MobileNo = dr["MobileNo"].ToString();

        //        objOrders.PortIn = dr["PortIn"].ToString();
        //        objOrders.ContractId = int.Parse(dr["ContractId"].ToString());
        //        objOrders.UserName = dr["UserName"].ToString();
        //        objOrders.MakeId = int.Parse(dr["MakeId"].ToString());
        //        objOrders.ModelId = int.Parse(dr["ModelId"].ToString());
        //        objOrders.MEID_IMEI = dr["MEID_IMEI"].ToString();
        //        objOrders.SIMID = dr["SIMID"].ToString();
        //        objOrders.LineNotesInstruction = dr["LineNotesInstruction"].ToString();
        //        objOrders.ActOrderInternalNotes = dr["ActOrderInternalNotes"].ToString();
        //        objOrders.ResponseToOrderDesk = dr["ResponseToOrderDesk"].ToString();
        //        objOrders.CarrierId = int.Parse(dr["CarrierId"].ToString());
        //        objOrders.RatePlanId = int.Parse(dr["RatePlanId"].ToString());
        //        objOrders.FeatureId = int.Parse(dr["FeatureId"].ToString());
        //        objOrders.MonthlyCost = decimal.Parse(dr["MonthlyCost"].ToString());
        //        objOrders.DeviceType = dr["DeviceType"].ToString();
        //        objOrders.DeviceID = dr["DeviceID"].ToString();
        //        objOrders.ServiceDetails = dr["ServiceDetails"].ToString();
        //        objOrders.OrderStatus = dr["OrderStatus"].ToString();
        //        //objOrders.ClientID = int.Parse(dr["ClientID"].ToString());
        //        objOrders.IsActive = 1;
        //        objOrders.IsDeleted = 0;
        //        objOrders.CreatedDate = DateTime.Parse(dr["CreatedDate"].ToString());
        //        objOrders.CreatedBy = int.Parse(dr["CreatedBy"].ToString());
        //        int orderDetailId = objOrders.SaveOrderDetail(out retMsg);
        //        AppHelper.SetMessage("Successfully saved details.", lblMsg);
        //        AppHelper.SetMessage("Successfully saved details.", lblMsgDown);
        //    }
        //    LoadOrder();
        //}
    }

    #region grdAttachments Events

    /// <summary>
    /// For Grid Commands Edit and delete phone data
    /// </summary>
    protected void grdAttachments_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Edit")
        {
            int adminTaskAttachmentId = 0;
            GridView gvPhone = (GridView)sender;
            DataKey key = gvPhone.SelectedDataKey;
            int.TryParse(e.CommandArgument.ToString(), out adminTaskAttachmentId);
        }
        if (e.CommandName == "DeleteData")
        {
            int adminTaskAttachmentId = 0;
            GridView gvPhone = (GridView)sender;
            DataKey key = gvPhone.SelectedDataKey;
            int.TryParse(e.CommandArgument.ToString(), out adminTaskAttachmentId);

            Initialize(siteattach);
            objAttach.SPType = AppConstants.DELETE;
            objAttach.PrimaryId = GetQueryId(OrderId);
            objAttach.AttachmentId = adminTaskAttachmentId;
            objAttach.TaskCreatedBy = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);
            objAttach.CreatedDate = DateTime.Now;
            objAttach.DeletedDate = DateTime.Now;
            LoadAttachments(objAttach.SaveAdminAttachment(out adminTaskAttachmentId));
        }

    }

    /// <summary>
    /// Phone Details
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void grdAttachments_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lnkDeleteAttachment = e.Row.FindControl("lnkDeleteAttachment") as LinkButton;
            if (hdnViewPermission.Value == "1" && hdnDeletePermission.Value == "0")
                lnkDeleteAttachment.Visible = false;
        }
        if (e.Row.RowType == DataControlRowType.Pager)
        {
            Sindhu.PagingControl Paging1 = (Sindhu.PagingControl)e.Row.FindControl("PagingPhone");
            Label lblPagingSummary = (Label)e.Row.FindControl("lblPagingSummaryPhone");
            Paging1.PageSize = grdAttachments.PageSize;
            try
            {
                //Paging1.TotalRecord = Convert.ToInt32(hidTotalRecordsPhone.Value);
            }
            catch
            {
                Paging1.TotalRecord = 1;
            }
            Paging1.CurrentPage = grdAttachments.PageIndex + 1;
            Paging1.DataLoad();

            //lblPagingSummary.Text = "Total records : " + hidTotalRecordsPhone.Value + ", Page " + Paging1.CurrentPage.ToString() + " of " + Paging1.TotalPages.ToString() + "";
            int TotalPages = Paging1.TotalPages;
            Paging1.Visible = true;
            if (TotalPages <= 1)
            {
                Paging1.Visible = false;
            }
        }
    }
    protected void grdAttachments_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }
    protected void grdAttachments_OnRowUpdating(object sender, GridViewUpdateEventArgs e)
    {

    }

    protected void grdAttachments_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        grdAttachments.PageIndex = e.NewPageIndex;
        //LoadProposalAttachments(ProposalSaveType());
    }

    private void LoadAttachments(DataTable dtData = null)
    {
        SiteAttachments objAttachment = new SiteAttachments();
        objAttachment.PrimaryId = GetQueryId(OrderId);
        objAttachment.SPType = AppConstants.SELECT;
        objAttachment.ModuleName = OTNewActivationOrder;

        int result = 0;
        if (dtData == null)
            grdAttachments.DataSource = objAttachment.SaveAdminAttachment(out result);
        else
            grdAttachments.DataSource = dtData;
        grdAttachments.DataBind();

        SiteAttachments objAttachments = new SiteAttachments();
        objAttachments.SPType = AppConstants.SELECT;
        FormAttachmentModel(Convert.ToInt32(objAttachment.PrimaryId), objAttachments, string.Empty, string.Empty, OTNewActivationOrder, OTNewActivationOrder);
        BindDataAttachment(grdShippingLabel, objAttachments);

    }

    private void LoadAttachments(GridView grd, string grd1)
    {
        SiteAttachments objAttach = new SiteAttachments();
        try
        {

            FormAttachmentModel(0, objAttach, "", "", AppConstants.ORDERACCESSORY, string.Empty);

            objAttach.SPType = AppConstants.SELECT;
            objAttach.PrimaryId = AppHelper.GetTaskIdFromQueryString("OrderId", this.Context);
            objAttach.AttachmentId = 0;
            int res = 0;
            DataTable dtData = objAttach.SaveAdminAttachment(out res);
            if (grd == null || grd.ID == "grdShippingLabel")
                BindGrid(grdShippingLabel, dtData);
            if (grd == null || grd.ID == "grdAttachments")
                BindGrid(grdAttachments, dtData);
        }
        catch (Exception ex)
        {
        }
    }

    #endregion

    protected void btnAttachFile_Click(object sender, EventArgs e)
    {
        try
        {
            UploadFile(true);
        }
        catch (Exception ex)
        {
            SetMessage(ex.Message);
        }
    }

    private void UploadFile(bool showNoFile = true)
    {
        Int64 PrimaryTaskId = ValidateAndSave(AppConstants.SAVEANDUPLOAD);
        if (GetQueryId(OrderId) == 0)
        {
            SaveOrder(AppConstants.INSERTFILE);
        }
        if (PrimaryTaskId == 0)
        {
            SetMessage("Could not save the details.");
        }
        else if (fUAttachment.HasFile && fUAttachment.PostedFile.ContentLength > 0)
        {
            SiteAttachments objAttach = new SiteAttachments();

            string imagePath = Server.MapPath("~/Uploads/AdminTasks");
            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }
            string postedFileName = AppHelper.FormatUploadFileName(fUAttachment.PostedFile.FileName);
            string filename = Path.GetFileNameWithoutExtension(postedFileName) + "_" + Guid.NewGuid().ToString().Replace("-", "").ToString()
                + Path.GetExtension(postedFileName);
            fUAttachment.SaveAs(imagePath + "/" + filename);

            //flag = "true";

            objAttach.SPType = AppConstants.INSERT;
            objAttach.LocalPath = imagePath + "/" + filename;
            objAttach.DocumentPath = Common.GetBaseURL + "Uploads/AdminTasks/" + filename;
            objAttach.UniqueFileName = filename;
            objAttach.FileName = Path.GetFileName(postedFileName);
            objAttach.TaskCreatedBy = Conversion.ParseInt(HttpContext.Current.Session[AppConstants.SessionAdminUserId]);
            objAttach.PrimaryId = GetQueryId(OrderId);
            objAttach.IsDeleted = 0;
            objAttach.ModuleName = OTNewActivationOrder;
            objAttach.CreatedDate = DateTime.Now;
            objAttach.DeletedDate = DateTime.Now;
            objAttach.UserId = Conversion.ParseInt(HttpContext.Current.Session[AppConstants.SessionAdminUserId]);

            int attachmentId = 0;
            DataTable dtData = objAttach.SaveAdminAttachment(out attachmentId);
            if (dtData != null)
            {
                SetMessage("File saved!");
                LoadAttachments(dtData);
            }
            else
            {
                SetMessage("File not saved!");
            }
        }
        else
        {
            if (showNoFile)
                SetMessage("Please select a file.");
        }
    }

    private Int64 ValidateAndSave(string buttonName)
    {
        lblMsg.Text = "";
        lblMsgDown.Text = "";

        return 1;
    }

    private void ShowAlert(string message)
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), "", "javascript:alert('" + message + "');", true);
    }

    private void SetMessage(string message)
    {
        lblMsg.Text = Common.DisplayMessage(message, Common.Enum_MessageType.ConfirmMessage, 5, Common.Enum_TableStyle.None);
        lblMsgDown.Text = Common.DisplayMessage(message, Common.Enum_MessageType.ConfirmMessage, 5, Common.Enum_TableStyle.None);
        ShowAlert(message);
    }

    private void SetMessageBig(string message)
    {
        lblMsg.Text = Common.DisplayMessageBig(message, Common.Enum_MessageType.ConfirmMessage, 5, Common.Enum_TableStyle.None);
        lblMsgDown.Text = Common.DisplayMessageBig(message, Common.Enum_MessageType.ConfirmMessage, 5, Common.Enum_TableStyle.None);
        ShowAlert(message);
    }

    public string SaveToManageLines(bool validate, string buttonCall, Button btn = null)
    {
        ManageMasters objManageMasters = new ManageMasters();

        String clientId = "";
        string validationMessage = "";

        if (btn != null && btn.Text == "Close & Add To Manage Lines")
            validationMessage = ValidateShippingTracking(validationMessage);
        if (string.IsNullOrEmpty(validationMessage))
        {
            #region Loop
            foreach (RepeaterItem item in rptLine.Items)
            {
                DropDownList ddlContract, ddlMake, ddlModel, ddldivRatePlan, ddlFeatures;
                TextBox txtMobileNo, txtMEIDIMEI, txtSimID, txtUserName, txtActNotes;
                GetControlsFromRepeater(item, out ddlContract, out ddlMake, out ddlModel, out ddldivRatePlan, out ddlFeatures, out txtMobileNo, out txtMEIDIMEI, out txtSimID, out txtUserName, out txtActNotes);

                if (validate)
                {
                    txtMobileNo.MaxLength = 10;
                    MaskedEditValidator MaskedEditValidator1 = item.FindControl("MaskedEditValidator1") as MaskedEditValidator;
                    MaskedEditValidator1.Enabled = true;

                    validationMessage += ValidateFields(ddlContract, ddlMake, ddlModel, txtMobileNo, txtMEIDIMEI, txtSimID, txtUserName, txtActNotes, item.ItemIndex + 1);
                    string mob = AppHelper.FormatMaskedMobileTextBox(txtMobileNo, this.Request);
                    validationMessage += CheckForDuplicateMobileNo(mob);
                }
                bool showValidation = false;
                if (string.IsNullOrEmpty(validationMessage) || buttonCall == AppConstants.SUBMITACTIVATION)
                {
                    // Save in Order Table
                    validationMessage += SaveOrderDetail(buttonCall);

                    // Save in Client Phone numbers table
                    if (string.IsNullOrEmpty(validationMessage) && buttonCall != AppConstants.SAVEANDCLOSE
                        && buttonCall != AppConstants.SUBMITACTIVATION && buttonCall != AppConstants.TASKORDRESPONSE)
                    {
                        int result = SaveManageLines(objManageMasters, ddlContract, ddlMake, ddlModel, txtMobileNo, txtMEIDIMEI, txtSimID, txtUserName, txtActNotes, ddldivRatePlan, ddlFeatures);
                        clientId += result.ToString() + ",";
                    }
                    else
                    {
                        showValidation = true;
                    }
                }
                else
                {
                    showValidation = true;
                    if (buttonCall == AppConstants.CREATEMANAGELINES)
                    {
                        Initialize(adminorders);
                        objOrders.SPType = AppConstants.STATUS;
                        objOrders.OrderStatus = AppConstants.SUBMITORDER;
                        objOrders.OrderId = GetQueryId(OrderId);
                        objOrders.CreatedDate = DateTime.Now;
                        objOrders.SaveOrder();
                    }
                }
                if (buttonCall == AppConstants.SUBMITACTIVATION)
                    showValidation = false;
                if (showValidation)
                {
                    validationMessage = string.Join(Environment.NewLine, validationMessage.Split(',').ToList().Distinct().ToList());
                    SetMessage(validationMessage);
                }
            }

            #endregion
            if (string.IsNullOrEmpty(validationMessage))
            {
                SaveOrder(buttonCall);
            }
        }
        // Save Task Status as Closed
        if (string.IsNullOrEmpty(validationMessage) && buttonCall != AppConstants.TASKRESPONSE && buttonCall != AppConstants.TASKCOMPRESPONSE && buttonCall != AppConstants.SUBMITACTIVATION)
        {
            validationMessage = "Saved! Line(s) created successfully.";
        }

        return validationMessage;
    }

    public string SaveToManageLinesV1(bool validate, string buttonCall, Button btn = null)
    {
        ManageMasters objManageMasters = new ManageMasters();

        String clientId = "";
        string validationMessage = "";

        if (btn != null && btn.Text == "Close & Add To Manage Lines")
            validationMessage = ValidateShippingTracking(validationMessage);
        if (string.IsNullOrEmpty(validationMessage))
        {
            #region Loop
            if (ViewState["rptLine"] != null)
            {
                using (DataTable dt = (DataTable)ViewState["rptLine"])
                {
                    int rowNo = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        rowNo++;
                        int ContactId, MakeId, ModelId, RatePlanId, FeatureId, PortId;
                        string MobileNo, MEIDIMEI, SimID, UserName, ActNotes, LineNotes, MonthlyCost, AreaCode;
                        GetControlsFromOrderDataRow(dr, out ContactId, out MakeId, out ModelId, out RatePlanId, out FeatureId, out MobileNo, out MEIDIMEI, out SimID, out UserName, out ActNotes, out LineNotes, out MonthlyCost, out PortId, out AreaCode);
                        if (validate)
                        {
                            validationMessage += ValidateOrderGridFields(ContactId, MakeId, ModelId, MobileNo, MEIDIMEI, SimID, UserName, ActNotes, rowNo, PortId);
                            validationMessage += CheckForDuplicateMobileNo(MobileNo);
                        }
                        bool showValidation = false;
                        if (string.IsNullOrEmpty(validationMessage) || buttonCall == AppConstants.SUBMITACTIVATION)
                        {
                            // Save in Order Table
                            //validationMessage += SaveOrderDetail(buttonCall);

                            // Save in Client Phone numbers table
                            if (string.IsNullOrEmpty(validationMessage) && buttonCall != AppConstants.SAVEANDCLOSE
                                && buttonCall != AppConstants.SUBMITACTIVATION && buttonCall != AppConstants.TASKORDRESPONSE)
                            {
                                int result = SaveManageLinesV1(objManageMasters, ContactId, MakeId, ModelId, MobileNo, MEIDIMEI, SimID, UserName, ActNotes, RatePlanId, FeatureId);
                                clientId += result.ToString() + ",";
                            }
                            else
                            {
                                showValidation = true;
                            }
                        }
                        else
                        {
                            showValidation = true;
                            if (buttonCall == AppConstants.CREATEMANAGELINES)
                            {
                                Initialize(adminorders);
                                objOrders.SPType = AppConstants.STATUS;
                                objOrders.OrderStatus = AppConstants.SUBMITORDER;
                                objOrders.OrderId = GetQueryId(OrderId);
                                objOrders.CreatedDate = DateTime.Now;
                                objOrders.SaveOrder();
                            }
                        }
                        if (buttonCall == AppConstants.SUBMITACTIVATION)
                            showValidation = false;
                        if (showValidation)
                        {
                            validationMessage = string.Join(Environment.NewLine, validationMessage.Split(',').ToList().Distinct().ToList());
                            SetMessage(validationMessage);
                        }
                    }
                }
            }



            #endregion
            if (string.IsNullOrEmpty(validationMessage))
            {
                SaveOrder(buttonCall);
            }
        }
        // Save Task Status as Closed
        if (string.IsNullOrEmpty(validationMessage) && buttonCall != AppConstants.TASKRESPONSE && buttonCall != AppConstants.TASKCOMPRESPONSE && buttonCall != AppConstants.SUBMITACTIVATION)
        {
            validationMessage = "Saved! Line(s) created successfully.";
        }

        return validationMessage;
    }


    private static void GetControlsFromOrderDataRow(DataRow dr, out int ContractId, out int MakeId, out int ModelId, out int RatePlanId, out int FeatureId, out string MobileNo, out string MEIDIMEI, out string SimID, out string UserName, out string ActNotes, out string LineNotes, out string MonthlyCost, out int PortIn, out string AreaCode)
    {
        int.TryParse(dr["PortIn"].ToString(), out PortIn);
        int.TryParse(dr["ContractId"].ToString(), out ContractId);
        int.TryParse(dr["MakeId"].ToString(), out MakeId);
        int.TryParse(dr["ModelId"].ToString(), out ModelId);
        int.TryParse(dr["RatePlanId"].ToString(), out RatePlanId);
        int.TryParse(dr["FeatureId"].ToString(), out FeatureId);
        AreaCode = dr["MobileNo"].ToString();
        MobileNo = dr["MobileNo"].ToString();
        MEIDIMEI = dr["MEID_IMEI"].ToString();
        SimID = dr["SIMID"].ToString();
        UserName = dr["UserName"].ToString();
        ActNotes = dr["ActOrderInternalNotes"].ToString();
        MonthlyCost = dr["MonthlyCost"].ToString();
        LineNotes = dr["LineNotesInstruction"].ToString();
    }


    private int SaveManageLinesV1(ManageMasters objManageMasters, int ContractId, int MakeId, int ModelId
            , string MobileNo, string MEIDIMEI, string SimID, string UserName, string ActNotes, int RatePlanId, int FeatureId)
    {
        hdnClientID.Value = AppHelper.GetValueFromQueryString(ClientId, this.Context);
        objManageMasters.ClientId = Convert.ToInt32(hdnClientID.Value);
        objManageMasters.Mobile = MobileNo;
        objManageMasters.CarrierID = Convert.ToInt32(ddlCarrier.SelectedValue);
        objManageMasters.Sim_ESN = MEIDIMEI.Trim();
        objManageMasters.SimID = SimID.Trim();
        // objManageMasters.IMEI_SN = txtdivImei.Text.Trim();
        objManageMasters.RatePlanId = RatePlanId;
        objManageMasters.ContractId = ContractId;
        objManageMasters.AccountTypeId = Convert.ToInt32(0);
        objManageMasters.FeatureId = FeatureId;
        objManageMasters.SaleRepId = Convert.ToInt32(Session[AppConstants.SessionSaleRepID]);
        objManageMasters.SalesRepId = Convert.ToInt32(Session[AppConstants.SessionSaleRepID]);
        objManageMasters.ActType = 1;
        objManageMasters.ContactDate = DateTime.Now.ToShortDateString();
        objManageMasters.Status = Convert.ToInt32(1);
        objManageMasters.InsertDate = DateTime.Now;
        objManageMasters.ExpiryDate = AppHelper.GetExpiryDate(ContractId.ToString(), DateTime.Now.ToShortDateString());
        objManageMasters.MakeId = MakeId;
        objManageMasters.ModelId = ModelId;
        objManageMasters.UserName = UserName;
        objManageMasters.Department = "";
        objManageMasters.ActNotes = ActNotes.Trim();
        try
        {
            TextBox txtActivationDate = this.Parent.FindControl("txtActivationDate") as TextBox;
            if (txtActivationDate != null && txtActivationDate.Text != "")
            {
                objManageMasters.ContactDate = txtActivationDate.Text;
            }
        }
        catch (Exception ex)
        {
        }

        try
        {
            DropDownList dropDownList = this.Parent.FindControl("ddlRep") as DropDownList;
            objManageMasters.SaleRepId = Convert.ToInt32(dropDownList.SelectedValue);
            objManageMasters.SalesRepId = Convert.ToInt32(dropDownList.SelectedValue);
        }
        catch (Exception ex)
        {
        }



        int result = objManageMasters.BusinessClientPhoneNumber_Insert();

        return result;
    }

    private string ValidateOrderGridFields(int ContractId, int MakeId, int ModelId
        , string MobileNo, string MEIDIMEI, string SimID, string UserName, string ActNotes, int index, int portNo, string buttonCall = "")
    {
        string validationString = "";

        if (ContractId == 0)
        {
            validationString += "Please select contract.<br/>";
        }

        if (MobileNo != null
        && (MobileNo == "" || MobileNo.Trim().Length != 10))
        {

            validationString += "Please enter mobile no.<br/>";
        }
        if (MakeId == 0)
        {
            validationString += "Please select Make.<br/>";
        }
        if (MEIDIMEI == "" && buttonCall != AppConstants.SAVEANDCLOSE)
        {
            validationString += "Please enter MEID/IMEI.<br/>";
        }
        if (SimID == "" && buttonCall != AppConstants.SAVEANDCLOSE)
        {
            validationString += "Please enter SIM ID.<br/>";
        }
        if (!string.IsNullOrEmpty(validationString))
        {
            validationString = "Line " + index.ToString() + " has issues. " + Environment.NewLine + validationString;
        }

        return validationString;
    }
    private static void GetControlsFromRepeater(RepeaterItem item, out DropDownList ddlContract, out DropDownList ddlMake, out DropDownList ddlModel, out DropDownList ddldivRatePlan, out DropDownList ddlFeatures, out TextBox txtMobileNo, out TextBox txtMEIDIMEI, out TextBox txtSimID, out TextBox txtUserName, out TextBox txtActNotes)
    {
        DropDownList ddlPortIn = item.FindControl("ddlPortIn") as DropDownList;
        ddlContract = item.FindControl("ddlContract") as DropDownList;
        ddlMake = item.FindControl("ddlMake") as DropDownList;
        ddlModel = item.FindControl("ddlModel") as DropDownList;
        ddldivRatePlan = item.FindControl("ddldivRatePlan") as DropDownList;
        ddlFeatures = item.FindControl("ddlFeatures") as DropDownList;
        HtmlGenericControl divAreaCode = item.FindControl("divAreaCode") as HtmlGenericControl;
        HtmlGenericControl divMobile = item.FindControl("divMobile") as HtmlGenericControl;
        HtmlGenericControl divManageLines = item.FindControl("divManageLines") as HtmlGenericControl;

        txtMobileNo = item.FindControl("txtMobileNo") as TextBox;
        txtMEIDIMEI = item.FindControl("txtMEIDIMEI") as TextBox;
        txtSimID = item.FindControl("txtSimID") as TextBox;
        txtUserName = item.FindControl("txtUserName") as TextBox;
        txtActNotes = item.FindControl("txtActNotes") as TextBox;
    }

    private static void GetControlsFromOrderGrid(RepeaterItem item, out DropDownList ddlContract, out DropDownList ddlMake, out DropDownList ddlModel, out DropDownList ddldivRatePlan, out DropDownList ddlFeatures, out TextBox txtMobileNo, out TextBox txtMEIDIMEI, out TextBox txtSimID, out TextBox txtUserName, out TextBox txtActNotes, out TextBox txtLineNotes, out TextBox txtMonthlyCost, out DropDownList ddlPortIn, out TextBox txtAreaCode)
    {
        ddlPortIn = item.FindControl("ddlPortIn") as DropDownList;
        ddlContract = item.FindControl("ddlContract") as DropDownList;
        ddlMake = item.FindControl("ddlMake") as DropDownList;
        ddlModel = item.FindControl("ddlModel") as DropDownList;
        ddldivRatePlan = item.FindControl("ddldivRatePlan") as DropDownList;
        ddlFeatures = item.FindControl("ddlFeatures") as DropDownList;
        HtmlGenericControl divAreaCode = item.FindControl("divAreaCode") as HtmlGenericControl;
        HtmlGenericControl divMobile = item.FindControl("divMobile") as HtmlGenericControl;
        HtmlGenericControl divManageLines = item.FindControl("divManageLines") as HtmlGenericControl;

        txtMobileNo = item.FindControl("txtMobileNo") as TextBox;
        txtMEIDIMEI = item.FindControl("txtMEIDIMEI") as TextBox;
        txtSimID = item.FindControl("txtSimID") as TextBox;
        txtUserName = item.FindControl("txtUserName") as TextBox;
        txtActNotes = item.FindControl("txtActNotes") as TextBox;
        txtMonthlyCost = item.FindControl("txtMonthlyCost") as TextBox;
        txtLineNotes = item.FindControl("txtLineNotes") as TextBox;
        txtAreaCode = item.FindControl("txtAreaCode") as TextBox;
    }

    private string ValidateFields(DropDownList ddlContract, DropDownList ddlMake, DropDownList ddlModel
        , TextBox txtMobileNo, TextBox txtMEIDIMEI, TextBox txtSimID, TextBox txtUserName, TextBox txtActNotes, int index, string buttonCall = "")
    {
        string validationString = "";
        if (ddlContract != null && ddlContract.SelectedIndex == 0)
        {
            validationString += "Please select contract.<br/>";
        }
        if (txtMobileNo != null
            && (AppHelper.FormatMaskedMobileTextBox(txtMobileNo) == "" || AppHelper.FormatMaskedMobileTextBox(txtMobileNo).Length != 10))
        {
            validationString += "Please enter mobile no.<br/>";
        }
        if (ddlMake != null && ddlMake.SelectedIndex == 0)
        {
            validationString += "Please select Make.<br/>";
        }
        if (txtMEIDIMEI != null && txtMEIDIMEI.Text == "" && buttonCall != AppConstants.SAVEANDCLOSE)
        {
            validationString += "Please enter MEID/IMEI.<br/>";
        }
        if (txtSimID != null && txtSimID.Text == "" && buttonCall != AppConstants.SAVEANDCLOSE)
        {
            validationString += "Please enter SIM ID.<br/>";
        }
        if (!string.IsNullOrEmpty(validationString))
        {
            validationString = "Line " + index.ToString() + " has issues. " + Environment.NewLine + validationString;
        }

        return validationString;
    }

    private int SaveManageLines(ManageMasters objManageMasters, DropDownList ddlContract, DropDownList ddlMake, DropDownList ddlModel
        , TextBox txtMobileNo, TextBox txtMEIDIMEI, TextBox txtSimID, TextBox txtUserName, TextBox txtActNotes, DropDownList ddldivRatePlan, DropDownList ddlFeature)
    {
        hdnClientID.Value = AppHelper.GetValueFromQueryString(ClientId, this.Context);
        objManageMasters.ClientId = Convert.ToInt32(hdnClientID.Value);
        objManageMasters.Mobile = AppHelper.FormatMaskedMobileTextBox(txtMobileNo, this.Request);
        objManageMasters.CarrierID = Convert.ToInt32(ddlCarrier.SelectedValue);
        objManageMasters.Sim_ESN = txtMEIDIMEI.Text.Trim();
        objManageMasters.SimID = txtSimID.Text.Trim();
        // objManageMasters.IMEI_SN = txtdivImei.Text.Trim();
        objManageMasters.RatePlanId = Convert.ToInt32(ddldivRatePlan.SelectedValue);
        objManageMasters.ContractId = Convert.ToInt32(ddlContract.SelectedValue);
        objManageMasters.AccountTypeId = Convert.ToInt32(0);
        objManageMasters.FeatureId = Convert.ToInt32(ddlFeature.SelectedValue);
        objManageMasters.SaleRepId = Convert.ToInt32(Session[AppConstants.SessionSaleRepID]);
        objManageMasters.SalesRepId = Convert.ToInt32(Session[AppConstants.SessionSaleRepID]);
        objManageMasters.ActType = 1;
        objManageMasters.ContactDate = DateTime.Now.ToShortDateString();
        objManageMasters.Status = Convert.ToInt32(1);
        objManageMasters.InsertDate = DateTime.Now;
        objManageMasters.ExpiryDate = AppHelper.GetExpiryDate(ddlContract.SelectedValue, DateTime.Now.ToShortDateString());
        objManageMasters.MakeId = Convert.ToInt32(ddlMake.SelectedValue);
        objManageMasters.ModelId = Convert.ToInt32(ddlModel.SelectedItem.Value);
        objManageMasters.UserName = txtUserName.Text.Trim();
        objManageMasters.Department = "";
        objManageMasters.ActNotes = txtActNotes.Text.Trim();
        try
        {
            TextBox txtActivationDate = this.Parent.FindControl("txtActivationDate") as TextBox;
            if (txtActivationDate != null && txtActivationDate.Text != "")
            {
                objManageMasters.ContactDate = txtActivationDate.Text;
            }
        }
        catch (Exception ex)
        {
        }

        try
        {
            DropDownList dropDownList = this.Parent.FindControl("ddlRep") as DropDownList;
            objManageMasters.SaleRepId = Convert.ToInt32(dropDownList.SelectedValue);
            objManageMasters.SalesRepId = Convert.ToInt32(dropDownList.SelectedValue);
        }
        catch (Exception ex)
        {
        }



        int result = objManageMasters.BusinessClientPhoneNumber_Insert();

        return result;
    }

    private string CheckForDuplicateMobileNo(string mobileNo, string buttonCall = "")
    {
        string message = "";

        message = CheckOrderForMobileNo(mobileNo, buttonCall);
        List<string> listMobileNo = new List<string>();
        List<string> listAreaCode = new List<string>();
        if (string.IsNullOrEmpty(message))
        {
            foreach (RepeaterItem item in rptLine.Items)
            {
                TextBox txtMobileNo = item.FindControl("txtMobileNo") as TextBox;
                TextBox txtAreaCode = item.FindControl("txtAreaCode") as TextBox;
                DropDownList ddlPortIn = item.FindControl("ddlPortIn") as DropDownList;
                if (txtMobileNo != null && AppHelper.FormatMaskedMobileTextBox(txtMobileNo).Count() > 3)
                {
                    var num = AppHelper.FormatMaskedMobileTextBox(txtMobileNo);
                    if (!string.IsNullOrEmpty(num) && ddlPortIn.SelectedValue == "1")
                        listMobileNo.Add(num);
                }
                if (txtAreaCode != null)
                {
                    var num = AppHelper.FormatMaskedMobileTextBox(txtAreaCode);
                    if (!string.IsNullOrEmpty(num) && ddlPortIn.SelectedValue == "0")
                        listAreaCode.Add(num);
                }
            }

            if (listAreaCode.Count > 0 && listMobileNo.Count == 0)
            {
                message = "";
            }
            else
            {

                if (!String.IsNullOrEmpty(mobileNo))
                {
                    var count = listMobileNo.Where(d => d == mobileNo).Count();
                    if (count > 1)
                    {
                        message = mobileNo + " is already available in the list." + Environment.NewLine;
                    }
                }
                else
                {
                    var duplicates = listMobileNo.GroupBy(s => s).SelectMany(grp => grp.Skip(1));

                    listMobileNo = listMobileNo.Distinct().ToList();
                    if (rptLine.Items.Count > 0 && duplicates.Count() > 0)
                    {
                        message = "There are duplicate items in the list. Please enter distinct mobile numbers." + Environment.NewLine;
                    }
                }
            }
        }
        return message;
    }

    protected void btnDeleteLine_Click(object sender, EventArgs e)
    {
        DeleteLine(sender);
        /*
        int selectedIndex = 0;
        Int32.TryParse(((Button)sender).CommandArgument, out selectedIndex);
        if (selectedIndex > -1)
        {
            RepeaterItem item = rptLine.Items[selectedIndex];
            if (item != null)
            {
                item.Visible = false;
                hdnDeleteLine.Value = selectedIndex.ToString();

                HiddenField hdnClientPhoneId = item.FindControl("hdnClientPhoneId") as HiddenField;
                DropDownList ddlContract = item.FindControl("ddlContract") as DropDownList;
                DropDownList ddlMake = item.FindControl("ddlMake") as DropDownList;
                DropDownList ddlModel = item.FindControl("ddlModel") as DropDownList;
                DropDownList ddldivRatePlan = item.FindControl("ddldivRatePlan") as DropDownList;
                DropDownList ddlFeatures = item.FindControl("ddlFeatures") as DropDownList;

                TextBox txtAreaCode = item.FindControl("txtAreaCode") as TextBox;
                TextBox txtMobileNo = item.FindControl("txtMobileNo") as TextBox;
                TextBox txtUserName = item.FindControl("txtUserName") as TextBox;
                TextBox txtActNotes = item.FindControl("txtActNotes") as TextBox;
                TextBox txtLineNotes = item.FindControl("txtLineNotes") as TextBox;

                RequiredFieldValidator rfddlContract = item.FindControl("rfddlContract") as RequiredFieldValidator;
                RequiredFieldValidator rftxtUserName = item.FindControl("rftxtUserName") as RequiredFieldValidator;
                RequiredFieldValidator rfddldivRatePlan = item.FindControl("rfddldivRatePlan") as RequiredFieldValidator;
                RequiredFieldValidator rfddlMake = item.FindControl("rfddlMake") as RequiredFieldValidator;
                RequiredFieldValidator rfddlModel = item.FindControl("rfddlModel") as RequiredFieldValidator;
                RequiredFieldValidator rftxtMEIDIMEI = item.FindControl("rftxtMEIDIMEI") as RequiredFieldValidator;
                RequiredFieldValidator rftxtSimID = item.FindControl("rftxtSimID") as RequiredFieldValidator;
                RequiredFieldValidator rftxtActNotes = item.FindControl("rftxtActNotes") as RequiredFieldValidator;
                RequiredFieldValidator rftxtLineNotes = item.FindControl("rftxtLineNotes") as RequiredFieldValidator;

                MaskedEditValidator MaskedEditValidator1 = item.FindControl("MaskedEditValidator1") as MaskedEditValidator;
                Label lblItemOrderStatus = item.FindControl("lblOrderStatus") as Label;

                if (txtAreaCode != null) txtAreaCode.Text = "";
                if (ddlContract != null) ddlContract.SelectedIndex = 0; rfddlContract.Enabled = false;
                if (ddlMake != null) ddlMake.SelectedIndex = 0; rfddlMake.Enabled = false;
                if (ddlModel != null && ddlModel.Items.Count > 0) ddlModel.SelectedIndex = 0; rfddlModel.Enabled = false;
                if (ddldivRatePlan != null) ddldivRatePlan.SelectedIndex = 0; rfddldivRatePlan.Enabled = false;
                if (txtMobileNo != null) txtMobileNo.Text = ""; txtMobileNo.Enabled = false; MaskedEditValidator1.Enabled = false;
                if (txtUserName != null) txtUserName.Text = ""; rftxtUserName.Enabled = false;
                if (txtActNotes != null) txtActNotes.Text = ""; rftxtActNotes.Enabled = false;
                if (txtLineNotes != null) txtLineNotes.Text = ""; rftxtLineNotes.Enabled = false;
                if (lblItemOrderStatus != null) lblItemOrderStatus.Text = AppConstants.DELETED;
                if (ddlFeatures != null) ddlFeatures.SelectedIndex = 0;

                if (selectedIndex > 1
                    && (hdnOrderStatusSub.Value == AppConstants.NEWLINE || hdnOrderStatusSub.Value == AppConstants.SAVEORDER))
                {
                    item = rptLine.Items[selectedIndex - 1];
                    Button btnDeleteLine = item.FindControl("btnDeleteLine") as Button;
                    if (btnDeleteLine != null)
                        btnDeleteLine.Visible = true;
                }
            }
        }
        */
    }

    private string CheckOrderForMobileNo(string mobileNo, string buttonCall = "")
    {
        string returnMessage = "";
        if (!string.IsNullOrEmpty(mobileNo))
        {
            Initialize(manageMasters);
            objManageMasters.ClientId = Convert.ToInt32(hdnClientID.Value == "" ? "0" : hdnClientID.Value);
            objManageMasters.MobileNo = mobileNo;
            objManageMasters.SPType = buttonCall;
            int adminTaskId = 0;
            Int32.TryParse(GetQueryId(AdminTaskId).ToString(), out adminTaskId);
            DataSet dtSet = objManageMasters.CheckOrderForMobileNoDS(adminTaskId);
            DataTable dtMobile = dtSet.Tables.Count > 0 ? dtSet.Tables[0] : null;
            if (dtMobile != null && dtMobile.Rows.Count > 0)
            {
                returnMessage = "Mobile no : " + mobileNo + " already exists for the client.";
            }
            if (string.IsNullOrEmpty(returnMessage))
            {
                dtMobile = dtSet.Tables.Count > 1 ? dtSet.Tables[1] : null;
                if (dtMobile != null && dtMobile.Rows.Count > 0)
                {
                    returnMessage = "Mobile no : " + mobileNo + " already exists.";
                }
            }
        }
        return returnMessage;
    }

    private string ValidateDeviceLease(int index, TextBox txtMob, TextBox txtUser, TextBox txtIMEI, TextBox txtSIM)
    {
        string result = "";
        string mob = "";
        if (txtMob != null)
        {
            mob = AppHelper.FormatMaskedMobileTextBox(txtMob);
        }
        if (mob.Length != 10)
        {
            result += "Mobile no is invalid at Line " + (index).ToString();
        }
        else if (txtUser != null && txtUser.Text == "")
        {
            result += "Username is required at Line " + (index).ToString();
        }
        else if (txtIMEI != null && txtIMEI.Text == "")
        {
            result += "MEID/IMEI is required at Line " + (index).ToString();
        }
        else if (txtSIM != null && txtSIM.Text == "")
        {
            result += "Sim ID is required at Line " + (index).ToString();
        }

        return result;
    }

    private string GetDeleteLineIndex()
    {
        string index = "";
        if (hdnDeleteLine.Value != "")
        {
            index = "&d=" + hdnDeleteLine.Value;
        }
        else if (Request.QueryString["d"] != null && !string.IsNullOrEmpty(Request.QueryString["d"]))
        {
            index = "&d=" + Convert.ToString(Request.QueryString["d"]);
        }
        return index;
    }


    protected void ddldivRatePlan_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddldivRatePlan = (DropDownList)sender;

        if (ddldivRatePlan != null)
        {
            int index = ddldivRatePlan.ClientID.LastIndexOf('_') + 1;
            string rowIndex = ddldivRatePlan.ClientID.Substring(index);

            int.TryParse(rowIndex, out index);
            //var item = rptLine.Items[index];
            Repeater rpt = hdnIsMultiEditBtn.Value == "1" ? rptLineItems : rptLine;
            var item = rpt.Items[index];
            TextBox txtMonthlyCost = item.FindControl("txtMonthlyCost") as TextBox;
            DropDownList ddlRatePlan = item.FindControl("ddldivRatePlan") as DropDownList;
            DropDownList ddlPortIn = item.FindControl("ddlPortIn") as DropDownList;
            RequiredFieldValidator rftxtSimID = item.FindControl("rftxtSimID") as RequiredFieldValidator;
            FilteredTextBoxExtender FilteredTextBoxExtender6 = item.FindControl("FilteredTextBoxExtender6") as FilteredTextBoxExtender;
            if (ddlRatePlan != null)
            {
                txtMonthlyCost.Text = GetRatePlan(Convert.ToInt32(ddlRatePlan.SelectedValue));

                HtmlGenericControl divMobile = item.FindControl("divMobile") as HtmlGenericControl;
                HtmlGenericControl divAreaCode = item.FindControl("divAreaCode") as HtmlGenericControl;
                HtmlGenericControl divManageLines = item.FindControl("divManageLines") as HtmlGenericControl;

                SetDIVShow(ddlPortIn, divAreaCode, divMobile, null, "", divManageLines);
                CheckForCODOrder(divManageLines, rftxtSimID, FilteredTextBoxExtender6);
            }

        }
    }

    private string GetRatePlan(int RatePlanId)
    {
        string monthlyCost = "";
        if (objManageMasters == null) objManageMasters = new ManageMasters();
        objManageMasters.RatePlanId = RatePlanId;
        DataTable dt = objManageMasters.GetRatePlanFeatures();
        if (dt.Rows.Count > 0)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                monthlyCost = Convert.ToString(dt.Rows[i]["MonthlyCost"]);
            }
        }
        return monthlyCost;
    }

    protected void ddlModel_SelectedIndexChanged(object sender, EventArgs e)
    {

        DropDownList ddlModel = (DropDownList)sender;

        if (ddlModel != null)
        {
            int index = ddlModel.ClientID.LastIndexOf('_') + 1;
            string rowIndex = ddlModel.ClientID.Substring(index);
            Repeater rpt = hdnIsMultiEditBtn.Value == "1" ? rptLineItems : rptLine;
            RepeaterItem item = rpt.Items[Convert.ToInt16(rowIndex)];
            if (item != null)
            {
                DropDownList ddlMake = item.FindControl("ddlMake") as DropDownList;
                RequiredFieldValidator rftxtSimID = item.FindControl("rftxtSimID") as RequiredFieldValidator;
                if (ddlMake != null)
                {
                    GetDeviceTypeName(ddlModel, ddlMake, rftxtSimID);
                }
            }
        }
    }

    private void GetDeviceTypeName(DropDownList ddlModel, DropDownList ddlMake, RequiredFieldValidator rftxtSimID)
    {
        objManageMasters = new ManageMasters();
        objManageMasters.MakeId = Conversion.ParseInt(ddlMake.SelectedValue);
        objManageMasters.ModelId = Conversion.ParseInt(ddlModel.SelectedValue);
        DataSet dsData = objManageMasters.BusinessAdmin_GetModel();
        if (dsData != null && dsData.Tables.Count > 1 && dsData.Tables[1].Rows.Count > 0)
        {
            var deviceType = Conversion.ParseString(dsData.Tables[1].Rows[0]["DeviceTypeName"]).ToLower();
            SimValidation(rftxtSimID, deviceType);
        }
    }

    private static void SimValidation(RequiredFieldValidator rftxtSimID, string deviceType)
    {
        if (deviceType == "basic phone")
        {
            rftxtSimID.Enabled = false;
        }
        else
            rftxtSimID.Enabled = true;
    }

    protected void btnCopytoNewLine_Click_1(object sender, EventArgs e)
    {
        AddNewLine();
        txtNoOfCopy.Text = "1";
        copyLastItem();
    }

    protected void copyLastItem()
    {
        RepeaterItem item = rptLine.Items[rptLine.Items.Count - 2 > 0 ? rptLine.Items.Count - 2 : 0];
        if (item != null)
        {
            #region Source Order Line
            HiddenField hdnOrderDetailId = item.FindControl("hdnOrderDetailId") as HiddenField;
            TextBox txtMobileNo = item.FindControl("txtMobileNo") as TextBox;
            TextBox txtAreaCode = item.FindControl("txtAreaCode") as TextBox;
            TextBox txtUserName = item.FindControl("txtUserName") as TextBox;
            TextBox txtLineNotes = item.FindControl("txtLineNotes") as TextBox;
            TextBox txtActNotes = item.FindControl("txtActNotes") as TextBox;
            TextBox txtMonthlyCost = item.FindControl("txtMonthlyCost") as TextBox;

            TextBox txtMEIDIMEI = item.FindControl("txtMEIDIMEI") as TextBox;
            TextBox txtSimID = item.FindControl("txtSimID") as TextBox;

            DropDownList ddlPortIn = item.FindControl("ddlPortIn") as DropDownList;
            DropDownList ddlContract = item.FindControl("ddlContract") as DropDownList;
            DropDownList ddlMake = item.FindControl("ddlMake") as DropDownList;
            DropDownList ddlModel = item.FindControl("ddlModel") as DropDownList;
            DropDownList ddldivRatePlan = item.FindControl("ddldivRatePlan") as DropDownList;
            DropDownList ddlFeatures = item.FindControl("ddlFeatures") as DropDownList;


            #endregion


            RepeaterItem destination = rptLine.Items[rptLine.Items.Count - 1];
            if (destination != null)
            {
                #region Destination Order Line
                HiddenField DesthdnOrderDetailId = destination.FindControl("hdnOrderDetailId") as HiddenField;
                TextBox DesttxtMobileNo = destination.FindControl("txtMobileNo") as TextBox;
                TextBox DesttxtAreaCode = destination.FindControl("txtAreaCode") as TextBox;
                TextBox DesttxtUserName = destination.FindControl("txtUserName") as TextBox;
                TextBox DesttxtLineNotes = destination.FindControl("txtLineNotes") as TextBox;
                TextBox DesttxtActNotes = destination.FindControl("txtActNotes") as TextBox;
                TextBox DesttxtMonthlyCost = destination.FindControl("txtMonthlyCost") as TextBox;

                TextBox DesttxtMEIDIMEI = destination.FindControl("txtMEIDIMEI") as TextBox;
                TextBox DesttxtSimID = destination.FindControl("txtSimID") as TextBox;

                DropDownList DestddlPortIn = destination.FindControl("ddlPortIn") as DropDownList;
                DropDownList DestddlContract = destination.FindControl("ddlContract") as DropDownList;
                DropDownList DestddlMake = destination.FindControl("ddlMake") as DropDownList;
                DropDownList DestddlModel = destination.FindControl("ddlModel") as DropDownList;
                DropDownList DestddldivRatePlan = destination.FindControl("ddldivRatePlan") as DropDownList;
                DropDownList DestddlFeatures = destination.FindControl("ddlFeatures") as DropDownList;


                #endregion

                DesttxtAreaCode.Text = txtAreaCode.Text;
                DesttxtUserName.Text = txtUserName.Text;
                DesttxtLineNotes.Text = txtLineNotes.Text;
                DesttxtActNotes.Text = txtActNotes.Text;
                DesttxtMonthlyCost.Text = txtMonthlyCost.Text;

                DesttxtMEIDIMEI.Text = txtMEIDIMEI.Text;
                DesttxtSimID.Text = txtSimID.Text;

                DestddlPortIn.SelectedValue = ddlPortIn.SelectedValue;
                DestddlContract.SelectedValue = ddlContract.SelectedValue;
                DestddlMake.SelectedValue = ddlMake.SelectedValue;

                BindModel(Convert.ToInt32(DestddlMake.SelectedValue), DestddlModel);

                DestddlModel.SelectedValue = ddlModel.SelectedValue;
                DestddldivRatePlan.SelectedValue = ddldivRatePlan.SelectedValue;
                DestddlFeatures.SelectedValue = ddlFeatures.SelectedValue;

                HtmlGenericControl divAreaCode = destination.FindControl("divAreaCode") as HtmlGenericControl;
                HtmlGenericControl divMobile = destination.FindControl("divMobile") as HtmlGenericControl;

                if (DestddlPortIn.SelectedValue == "1")
                {
                    DesttxtMobileNo.Text = txtMobileNo.Text;
                    divAreaCode.Style.Add("display", "none");
                    divMobile.Style.Add("display", "block");
                }
                else
                {
                    divAreaCode.Style.Add("display", "block");
                    divMobile.Style.Add("display", "none");
                }
            }
        }
        hdnItemsCount.Value = rptLine.Items.Count.ToString();
    }

    protected void btnCopytoNewLine_Click(object sender, EventArgs e)
    {
        RepeaterItem item = rptLine.Items[0];
        if (item != null)
        {
            #region Source Order Line
            HiddenField hdnOrderDetailId = item.FindControl("hdnOrderDetailId") as HiddenField;
            TextBox txtMobileNo = item.FindControl("txtMobileNo") as TextBox;
            TextBox txtAreaCode = item.FindControl("txtAreaCode") as TextBox;
            TextBox txtUserName = item.FindControl("txtUserName") as TextBox;
            TextBox txtLineNotes = item.FindControl("txtLineNotes") as TextBox;
            TextBox txtActNotes = item.FindControl("txtActNotes") as TextBox;
            TextBox txtMonthlyCost = item.FindControl("txtMonthlyCost") as TextBox;

            TextBox txtMEIDIMEI = item.FindControl("txtMEIDIMEI") as TextBox;
            TextBox txtSimID = item.FindControl("txtSimID") as TextBox;

            DropDownList ddlPortIn = item.FindControl("ddlPortIn") as DropDownList;
            DropDownList ddlContract = item.FindControl("ddlContract") as DropDownList;
            DropDownList ddlMake = item.FindControl("ddlMake") as DropDownList;
            DropDownList ddlModel = item.FindControl("ddlModel") as DropDownList;
            DropDownList ddldivRatePlan = item.FindControl("ddldivRatePlan") as DropDownList;
            DropDownList ddlFeatures = item.FindControl("ddlFeatures") as DropDownList;


            #endregion

            AddNewLine();
            txtNoOfCopy.Text = "1";
            RepeaterItem destination = rptLine.Items[0];
            if (destination != null)
            {
                #region Destination Order Line
                HiddenField DesthdnOrderDetailId = destination.FindControl("hdnOrderDetailId") as HiddenField;
                TextBox DesttxtMobileNo = destination.FindControl("txtMobileNo") as TextBox;
                TextBox DesttxtAreaCode = destination.FindControl("txtAreaCode") as TextBox;
                TextBox DesttxtUserName = destination.FindControl("txtUserName") as TextBox;
                TextBox DesttxtLineNotes = destination.FindControl("txtLineNotes") as TextBox;
                TextBox DesttxtActNotes = destination.FindControl("txtActNotes") as TextBox;
                TextBox DesttxtMonthlyCost = destination.FindControl("txtMonthlyCost") as TextBox;

                TextBox DesttxtMEIDIMEI = destination.FindControl("txtMEIDIMEI") as TextBox;
                TextBox DesttxtSimID = destination.FindControl("txtSimID") as TextBox;

                DropDownList DestddlPortIn = destination.FindControl("ddlPortIn") as DropDownList;
                DropDownList DestddlContract = destination.FindControl("ddlContract") as DropDownList;
                DropDownList DestddlMake = destination.FindControl("ddlMake") as DropDownList;
                DropDownList DestddlModel = destination.FindControl("ddlModel") as DropDownList;
                DropDownList DestddldivRatePlan = destination.FindControl("ddldivRatePlan") as DropDownList;
                DropDownList DestddlFeatures = destination.FindControl("ddlFeatures") as DropDownList;


                #endregion

                //DesttxtAreaCode.Text = txtAreaCode.Text;
                //DesttxtUserName.Text = txtUserName.Text;
                //DesttxtLineNotes.Text = txtLineNotes.Text;
                // DesttxtActNotes.Text = txtActNotes.Text;
                //DesttxtMonthlyCost.Text = txtMonthlyCost.Text;

                //DesttxtMEIDIMEI.Text = txtMEIDIMEI.Text;
                //DesttxtSimID.Text = txtSimID.Text;

                //DestddlPortIn.SelectedValue = ddlPortIn.SelectedValue;
                //DestddlContract.SelectedValue = ddlContract.SelectedValue;
                //DestddlMake.SelectedValue = ddlMake.SelectedValue;

                //BindModel(Convert.ToInt32(DestddlMake.SelectedValue), DestddlModel);

                //DestddlModel.SelectedValue = ddlModel.SelectedValue;
                //DestddldivRatePlan.SelectedValue = ddldivRatePlan.SelectedValue;
                //DestddlFeatures.SelectedValue = ddlFeatures.SelectedValue;

                HtmlGenericControl divAreaCode = destination.FindControl("divAreaCode") as HtmlGenericControl;
                HtmlGenericControl divMobile = destination.FindControl("divMobile") as HtmlGenericControl;

                if (DestddlPortIn.SelectedValue == "1")
                {
                    DesttxtMobileNo.Text = txtMobileNo.Text;
                    divAreaCode.Style.Add("display", "none");
                    divMobile.Style.Add("display", "block");
                }
                else
                {
                    divAreaCode.Style.Add("display", "block");
                    divMobile.Style.Add("display", "none");
                }
            }
        }
        hdnItemsCount.Value = rptLine.Items.Count.ToString();


    }

    private void BindDropDownShippingMethod(string ClientId)
    {
        ViewState["DTShippingMethod"] = CommonMethods.BindDropDownShippingMethod(ddlShippingMethod, ClientId, objManageMasters);
    }

    protected void chkNoShippingRequired_CheckedChanged(object sender, EventArgs e)
    {
        ShippingRequired();
    }

    private void ShippingRequired()
    {
        if (chkNoShippingRequired.Checked)
        {
            ddlShippingAddress.Enabled = false;
            rfvShippingAddress.Enabled = false;
            pnlShippingOption.Visible = false;
        }
        else
        { ddlShippingAddress.Enabled = true; rfvShippingAddress.Enabled = true; pnlShippingOption.Visible = true; }
    }

    private void SaveNewOrder(DataTable dt)
    {
        foreach (DataRow dr in dt.Rows)
        {
            objOrders = new AdminOrders();
            objOrders.OrderId = hdnOrderId.Value == "" ? 0 : Convert.ToInt32(hdnOrderId.Value);
            objOrders.OrderDetailId = 0;
            objOrders.SPType = objOrders.OrderDetailId == 0 ? AppConstants.INSERT : AppConstants.UPDATE;
            objOrders.MobileNo = Conversion.ParseString(dr["MobileNo"]);
            string areacode = Conversion.ParseString(dr["NewAreaCode"]);

            if (objOrders.MobileNo != "" || areacode != "")
            {
                objOrders.MobileNo = objOrders.MobileNo == "" ? areacode : objOrders.MobileNo;
                objOrders.PortIn = Conversion.ParseString(dr["PortIn"]);
                objOrders.ContractId = 0;
                objOrders.UserName = Conversion.ParseString(dr["UserName"]);
                objOrders.MakeId = Conversion.ParseInt(dr["MakeId"]);
                objOrders.ModelId = Conversion.ParseInt(dr["ModelId"]);
                objOrders.MEID_IMEI = Conversion.ParseString(dr["IMEI_SN"]);
                objOrders.SIMID = Conversion.ParseString(dr["Sim_ESN"]);
                string lineNotesInstruction = "";
                if (objOrders.PortIn != null && objOrders.PortIn.ToLower() == "true")
                {
                    lineNotesInstruction += "Existing Mobile: " + Conversion.ParseString(dr["MobileNo"]) + Environment.NewLine;
                    lineNotesInstruction += "Existing Account #: " + Conversion.ParseString(dr["CustomerAccountNumber"]) + Environment.NewLine;
                    lineNotesInstruction += "Existing Customer Pin/Password: " + Conversion.ParseString(dr["CustomerNewPin"]) + Environment.NewLine;
                }
                objOrders.LineNotesInstruction = lineNotesInstruction;
                objOrders.ActOrderInternalNotes = "";
                objOrders.ResponseToOrderDesk = "";
                objOrders.CarrierId = ddlCarrier.SelectedValue == "" ? 0 : Convert.ToInt32(ddlCarrier.SelectedValue);
                objOrders.RatePlanId = 0;
                objOrders.FeatureId = 0;
                objOrders.MonthlyCost = 0;
                objOrders.DeviceType = "";
                objOrders.DeviceID = "";
                objOrders.ServiceDetails = "";
                objOrders.OrderStatus = AppConstants.SAVED;
                objOrders.ClientID = Convert.ToInt32(AppHelper.GetValueFromQueryString(ClientId, this.Context));
                objOrders.IsActive = 1;
                objOrders.IsDeleted = 0;

                objOrders.CreatedDate = DateTime.Now;
                objOrders.CreatedBy = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);

                string retMsg = "";
                int orderdetailId = objOrders.SaveOrderDetail(out retMsg);

            }
        }
    }

    private void DeleteLine(object sender)
    {
        try
        {
            int selectedIndex = 0;
            Int32.TryParse(((Button)sender).CommandArgument, out selectedIndex);
            if (selectedIndex > -1)
            {
                RepeaterItem item = rptLine.Items[selectedIndex];
                if (item != null)
                {
                    item.Visible = false;
                    HiddenField hdnOrderDetailId = item.FindControl("hdnOrderDetailId") as HiddenField;
                    if (hdnOrderDetailId != null)
                    {
                        int orderDetailId = 0;
                        int.TryParse(hdnOrderDetailId.Value, out orderDetailId);
                        if (orderDetailId > 0)
                        {
                            AdminOrders objOrders = new AdminOrders();
                            objOrders.SPType = AppConstants.DELETE;
                            objOrders.OrderDetailId = orderDetailId;
                            objOrders.SaveOrderDetailData();

                            LoadOrderRelated(); //BindData();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void BindData()
    {
        DataSet dsData = LoadOrderOnLoad();
        if (dsData.Tables.Count > 1)
        {
            DataTable dtOrderDetails = dsData.Tables.Count > 1 ? dsData.Tables[1] : null;
            if (dtOrderDetails != null)
            {
                LoadOrderDetails(dtOrderDetails);
            }
        }
    }

    public string SaveDeviceLease(string buttonCall)
    {
        string retMsg = "";
        retMsg = SaveOrderDetail(buttonCall);
        LoadOrderRelated();
        return retMsg;
    }
    private string SaveOrderDeviceLease(string buttonCall, int orderDetailId, int index)
    {
        string retMsg = "";
        if (rptLine.Items.Count > 0)
        {
            Initialize(adminorders);

            //foreach (RepeaterItem item in rptLine.Items)
            RepeaterItem item = rptLine.Items[index];
            if (item != null)
            {
                objManageMasters = new ManageMasters();
                {
                    #region Save Order Line
                    HiddenField hdnDeviceLeaseId = item.FindControl("hdnDeviceLeaseId") as HiddenField;
                    HiddenField hdnOrderDetailId = item.FindControl("hdnOrderDetailId") as HiddenField;
                    TextBox txtMobileNo = item.FindControl("txtMobileNo") as TextBox;
                    TextBox txtAreaCode = item.FindControl("txtAreaCode") as TextBox;
                    TextBox txtUserName = item.FindControl("txtUserName") as TextBox;
                    TextBox txtLineNotes = item.FindControl("txtLineNotes") as TextBox;
                    TextBox txtActNotes = item.FindControl("txtActNotes") as TextBox;
                    TextBox txtMonthlyCost = item.FindControl("txtMonthlyCost") as TextBox;

                    TextBox txtMEIDIMEI = item.FindControl("txtMEIDIMEI") as TextBox;
                    TextBox txtSimID = item.FindControl("txtSimID") as TextBox;

                    DropDownList ddlPortIn = item.FindControl("ddlPortIn") as DropDownList;
                    DropDownList ddlContract = item.FindControl("ddlContract") as DropDownList;
                    DropDownList ddlMake = item.FindControl("ddlMake") as DropDownList;
                    DropDownList ddlModel = item.FindControl("ddlModel") as DropDownList;
                    DropDownList ddldivRatePlan = item.FindControl("ddldivRatePlan") as DropDownList;
                    DropDownList ddlFeatures = item.FindControl("ddlFeatures") as DropDownList;

                    Label lblItemOrderStatus = item.FindControl("lblOrderStatus") as Label;
                    Label lblErrorMessage = item.FindControl("lblErrorMessage") as Label;

                    objManageMasters.SPType = (hdnDeviceLeaseId.Value == "" || hdnDeviceLeaseId.Value == "0") ? AppConstants.INSERT : AppConstants.UPDATE;
                    objManageMasters.Id = (hdnDeviceLeaseId.Value == "" || hdnDeviceLeaseId.Value == "0") ? 0 : Conversion.ParseInt(hdnDeviceLeaseId.Value);
                    objManageMasters.IsDeleted = 0;
                    objManageMasters.StatusId = 1;
                    objManageMasters.ClientId = Conversion.ParseInt(hdnClientID.Value);
                    objManageMasters.LeaseDate = DateTime.Today.ToShortDateString();
                    objManageMasters.CarrierId = Conversion.ParseInt(ddlCarrier.SelectedValue);
                    objManageMasters.LeaseTerm = (this.Parent.FindControl("ddldivLeaseTerm") as DropDownList).SelectedValue;
                    objManageMasters.UserName = txtUserName.Text;
                    objManageMasters.MakeId = Conversion.ParseInt(ddlMake.SelectedValue);
                    objManageMasters.ModelId = Conversion.ParseInt(ddlModel.SelectedValue);

                    objManageMasters.SalesRepId = Conversion.ParseInt(Session[AppConstants.SessionSaleRepID]);
                    objManageMasters.MobileNo = AppHelper.FormatMaskedMobileTextBox(txtMobileNo);
                    objManageMasters.IMEI = txtMEIDIMEI.Text;
                    objManageMasters.SimID = txtSimID.Text;
                    objManageMasters.ReturnDate = string.Empty;
                    objManageMasters.ActNotes = txtActNotes.Text.Trim();

                    objManageMasters.CreatedOn = DateTime.Now;
                    objManageMasters.CreatedBy = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);
                    objManageMasters.OrderDetailId = orderDetailId;

                    retMsg = ValidateDeviceLease(item.ItemIndex + 1, txtMobileNo, txtUserName, txtMEIDIMEI, txtSimID);

                    if (retMsg == "")
                    {
                        string result = objManageMasters.SaveDeviceLease();
                        lblErrorMessage.Text = retMsg;
                        if (result != "")
                        {
                        }
                    }
                    else if (retMsg != "")
                    {
                        AppHelper.SetMessage(retMsg, lblMsg);
                        AppHelper.SetMessage(retMsg, lblMsgDown);
                    }

                    #endregion
                }
            }
        }

        return retMsg;
    }

    private string SaveOrderDetailFull(string buttonCall)
    {
        string retMsg = "";
        string OrderXML = "<Root><Order>";
        if (rptLine.Items.Count > 0)
        {
            DataTable dtRptLine = null;
            try
            {
                dtRptLine = ViewState["rptLine"] as DataTable;
            }
            catch (Exception ex)
            {
            }


            Initialize(adminorders);
            int lastItem = 0;
            foreach (RepeaterItem item in rptLine.Items)
            {
                #region Save Order Line
                #region controls
                HiddenField hdnOrderDetailId = item.FindControl("hdnOrderDetailId") as HiddenField;
                TextBox txtMobileNo = item.FindControl("txtMobileNo") as TextBox;
                TextBox txtAreaCode = item.FindControl("txtAreaCode") as TextBox;
                TextBox txtUserName = item.FindControl("txtUserName") as TextBox;
                TextBox txtLineNotes = item.FindControl("txtLineNotes") as TextBox;
                TextBox txtActNotes = item.FindControl("txtActNotes") as TextBox;
                TextBox txtMonthlyCost = item.FindControl("txtMonthlyCost") as TextBox;

                TextBox txtMEIDIMEI = item.FindControl("txtMEIDIMEI") as TextBox;
                TextBox txtSimID = item.FindControl("txtSimID") as TextBox;

                DropDownList ddlPortIn = item.FindControl("ddlPortIn") as DropDownList;
                DropDownList ddlContract = item.FindControl("ddlContract") as DropDownList;
                DropDownList ddlMake = item.FindControl("ddlMake") as DropDownList;
                DropDownList ddlModel = item.FindControl("ddlModel") as DropDownList;
                DropDownList ddldivRatePlan = item.FindControl("ddldivRatePlan") as DropDownList;
                DropDownList ddlFeatures = item.FindControl("ddlFeatures") as DropDownList;

                Label lblItemOrderStatus = item.FindControl("lblOrderStatus") as Label;
                Label lblErrorMessage = item.FindControl("lblErrorMessage") as Label;
                #endregion



                bool toSave = false;
                objOrders.OrderId = hdnOrderId.Value == "" ? 0 : Convert.ToInt32(hdnOrderId.Value);
                objOrders.OrderDetailId = hdnOrderDetailId.Value == "" ? 0 : Convert.ToInt32(hdnOrderDetailId.Value);
                objOrders.SPType = objOrders.OrderDetailId == 0 ? AppConstants.INSERT : AppConstants.UPDATE;

                if (buttonCall != AppConstants.CREATEMANAGELINES && buttonCall != AppConstants.SAVEANDCLOSE && buttonCall != AppConstants.SAVEDEVICELEASE)
                    objOrders.MobileNo = AppHelper.FormatMaskedMobileTextBox(ddlPortIn.SelectedValue == "1" ? txtMobileNo : txtAreaCode, this.Request);
                else
                    objOrders.MobileNo = AppHelper.FormatMaskedMobileTextBox(txtMobileNo) != "" ? AppHelper.FormatMaskedMobileTextBox(txtMobileNo) : AppHelper.FormatMaskedMobileTextBox(txtMobileNo, this.Request);


                objOrders.OrderStatus = AppHelper.GetOrderStatus(buttonCall, objOrders.OrderDetailId, lblItemOrderStatus.Text);
                objOrders.ClientID = Convert.ToInt32(AppHelper.GetValueFromQueryString(ClientId, this.Context));
                objOrders.IsActive = 1;
                objOrders.IsDeleted = 0;
                objOrders.CreatedDate = DateTime.Now;
                objOrders.CreatedBy = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);

                objOrders.PortIn = ddlPortIn.SelectedValue;

                if (objOrders.MobileNo.Length > 3)
                    retMsg = CheckOrderForMobileNo(objOrders.MobileNo, buttonCall);

                bool isChanged = CompareItem(dtRptLine, hdnOrderDetailId, txtUserName, txtLineNotes, txtActNotes, txtMonthlyCost, txtMEIDIMEI, txtSimID, ddlPortIn, ddlContract, ddlMake, ddlModel);
                if (isChanged)
                {
                    string OrderDetail = "<OD>";
                    OrderDetail += @"<OrdId>" + hdnOrderId.Value + "</OrdId>";
                    OrderDetail += @"<ID>" + hdnOrderDetailId.Value + "</ID>";
                    OrderDetail += @"<Mob>" + AppHelper.FormatTelePhoneNumber(objOrders.MobileNo) + "</Mob>";
                    OrderDetail += @"<Area>" + AppHelper.FormatTelePhoneNumber(txtAreaCode.Text) + "</Area>";
                    OrderDetail += @"<CarrierId>" + ddlCarrier.SelectedValue + "</CarrierId>";
                    OrderDetail += @"<UName>" + txtUserName.Text + "</UName>";
                    OrderDetail += @"<Line>" + txtLineNotes.Text + "</Line>";
                    OrderDetail += @"<Act>" + txtActNotes.Text + "</Act>";
                    OrderDetail += @"<Meid>" + txtMEIDIMEI.Text + "</Meid>";
                    OrderDetail += @"<Sim>" + txtSimID.Text + "</Sim>";
                    OrderDetail += @"<Port>" + ddlPortIn.SelectedValue + "</Port>";
                    OrderDetail += @"<Cont>" + ddlContract.SelectedValue + "</Cont>";
                    OrderDetail += @"<Make>" + ddlMake.SelectedValue + "</Make>";
                    OrderDetail += @"<Model>" + ddlModel.SelectedValue + "</Model>";
                    OrderDetail += @"<RP>" + ddldivRatePlan.SelectedValue + "</RP>";
                    OrderDetail += @"<Feature>" + ddlFeatures.SelectedValue + "</Feature>";
                    OrderDetail += @"<Cost>" + txtMonthlyCost.Text + "</Cost>";
                    OrderDetail += @"<OrdStatus>" + objOrders.OrderStatus + "</OrdStatus>";
                    OrderDetail += "</OD>";
                    OrderXML += OrderDetail;
                }

                #endregion
            }
            if (rptLine.Items.Count > 0)
                OrderXML += "</Order></Root>";

        }
        else
            OrderXML = "";

        objOrders.OrderId = (hdnOrderId.Value == "" || hdnOrderId.Value == "0") ? GetQueryId(OrderId) : Convert.ToInt32(hdnOrderId.Value);
        objOrders.XML = OrderXML;

        int orderdetailId = objOrders.SaveOrderDetailXML(out retMsg);

        //if (buttonCall == AppConstants.SAVEDEVICELEASE)
        //    retMsg = SaveOrderDeviceLease(buttonCall, orderdetailId, item.ItemIndex);
        if (string.IsNullOrEmpty(retMsg))
        {
            AppHelper.SetMessage("Successfully saved details.", lblMsg);
            AppHelper.SetMessage("Successfully saved details.", lblMsgDown);
        }
        if (buttonCall == AppConstants.NEWLINE)
        {
            LoadOrderRelated();
        }
        else if (buttonCall != AppConstants.SUBMITORDER && buttonCall != AppConstants.SUBMITACTIVATION
            && buttonCall != AppConstants.CREATEMANAGELINES && string.IsNullOrEmpty(retMsg)
        && buttonCall != AppConstants.SAVEANDCLOSE && buttonCall != AppConstants.TASKRESPONSE && buttonCall != AppConstants.SAVEDEVICELEASE)
        {
            Response.Redirect("ManageOrdersNewAct.aspx?ClientId=" + GetQueryId(ClientId) + "&OrderId=" + objOrders.OrderId
                + "&sot=" + hdnOrderTypeSub.Value + "&id=" + GetQueryId(TaskId)
                + (buttonCall == AppConstants.SAVEORDER ? "&so=1" : "")
                + (buttonCall != AppConstants.NEWLINE ? GetDeleteLineIndex() : ""));
        }

        return OrderXML;
    }

    private bool CompareItem(DataTable dtRptLine, HiddenField hdnOrderDetailId, TextBox txtUserName, TextBox txtLineNotes, TextBox txtActNotes, TextBox txtMonthlyCost, TextBox txtMEIDIMEI, TextBox txtSimID, DropDownList ddlPortIn, DropDownList ddlContract, DropDownList ddlMake, DropDownList ddlModel)
    {
        bool isChanged = true;
        if (dtRptLine != null)
        {
            DataRow drItem = dtRptLine.AsEnumerable()
                .Where(d => d["OrderDetailId"].GetStringFromObject() == hdnOrderDetailId.Value)
                .FirstOrDefault();
            if (drItem != null)
            {
                if (
                    drItem["OrderDetailId"].GetStringFromObject().Equals(hdnOrderDetailId.Value)
                    && drItem["PortIn"].GetStringFromObject().Equals(ddlPortIn.SelectedValue)
                    && drItem["ContractId"].GetStringFromObject().Equals(ddlContract.SelectedValue)
                    && drItem["MobileNo"].GetStringFromObject().Equals(objOrders.MobileNo)
                    && drItem["MakeId"].GetStringFromObject().Equals(ddlMake.SelectedValue)
                    && drItem["ModelId"].GetStringFromObject().Equals(ddlModel.SelectedValue)
                    && drItem["UserName"].GetStringFromObject().Equals(txtUserName.Text.Trim())
                    && drItem["MonthlyCost"].GetStringFromObject().Equals(txtMonthlyCost.Text.Trim())
                    && drItem["MEID_IMEI"].GetStringFromObject().Equals(txtMEIDIMEI.Text.Trim())
                    && drItem["SIMID"].GetStringFromObject().Equals(txtSimID.Text.Trim())
                    && drItem["ActOrderInternalNotes"].GetStringFromObject().Equals(txtActNotes.Text.Trim())
                    && drItem["LineNotesInstruction"].GetStringFromObject().Equals(txtLineNotes.Text.Trim())
                    )
                {
                    isChanged = false;
                }
            }
        }

        return isChanged;
    }

    protected void btnCopytoNewLineXML_Click(object sender, EventArgs e)
    {
        lblMsgDown.Text = SaveOrderDetailFull(string.Empty);
    }
    protected void ddlShippingAddress_SelectedIndexChanged(object sender, EventArgs e)
    {
        //ValidateShippingDetails();
    }

    public string ValidateShippingDetails()
    {
        try
        {
            string address = ddlShippingAddress.SelectedItem.Text;
            DataTable dtShippingOption = (DataTable)ViewState["DTShippingOption"];
            DataTable dtShippingMethod = (DataTable)ViewState["DTShippingMethod"];
            if (dtShippingOption != null)
            {
                if (address.ToLower().StartsWith("ws fulfillment center"))
                {
                    try
                    {
                        var opt = dtShippingOption.AsEnumerable().Where(d => d["SHIPPINGVALUE"].GetStringFromObject().ToLower().StartsWith("in person"));

                        if (opt != null && opt.Count() > 0)
                        {
                            ddlShippingOption.Items.Clear();
                            ddlShippingOption.DataSource = opt.CopyToDataTable();
                            ddlShippingOption.DataTextField = "SHIPPINGVALUE";
                            ddlShippingOption.DataValueField = "MasterDataPreferencesId";
                            ddlShippingOption.DataBind();

                            ddlShippingOption.Items.Insert(0, new ListItem(AppConstants.DDLSelect, AppConstants.DDLZero));
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        var method = dtShippingMethod.AsEnumerable().Where(d => d["ShippingName"].GetStringFromObject().ToLower().StartsWith("in person"));

                        if (method != null && method.Count() > 0)
                        {
                            ddlShippingMethod.Items.Clear();
                            ddlShippingMethod.DataSource = method.CopyToDataTable();
                            ddlShippingMethod.DataTextField = "ShippingName";
                            ddlShippingMethod.DataValueField = "ShippingMethodId";
                            ddlShippingMethod.DataBind();
                            ddlShippingMethod.Items.Insert(0, new ListItem(AppConstants.DDLSelect, AppConstants.DDLZero));
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    if (lblOrderStatus.Text == AppConstants.SUBMITORDER
                        &&
                        (ddlShippingAddress.SelectedItem.Text.ToLower().IndexOf("ws fulfillment") == -1 && ddlShippingOption.SelectedItem.Text.ToLower().IndexOf("in person") > -1)
                        ||
                        (ddlShippingAddress.SelectedItem.Text.ToLower().IndexOf("ws fulfillment") > -1 && ddlShippingOption.SelectedItem.Text.ToLower().IndexOf("in person") == -1)
                        )
                    {
                        lblMsg.Text = "Please select Address: WS FULFILLMENT with IN PERSON PICKUP";
                        ddlShippingOption.SelectedIndex = 0;
                    }
                }
            }
        }
        catch (Exception ex)
        {

        }
        return lblMsg.Text;
    }

    #region Shipping Label Creation

    protected void btnCreateShippingLabel_Click(object sender, EventArgs e)
    {
        try
        {
            if (ddlShippingOptionLabel.SelectedValue == "")
            {
                SetMessage("Please select Shipping Option");
                grdShippingLabel.Focus();
            }
            else
                GenerateLabelAsync("ORDERSHIPPING").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
        }
    }
    public decimal GetGridTotalDeviceWeight()
    {
        decimal weight = 0;

        foreach (GridViewRow rows in grdOrderInfo.Rows)
        {

            Dictionary<string, string> objLineItemTokens = new Dictionary<string, string>();
            DropDownList ddlMake = rows.FindControl("ddlMake") as DropDownList;
            DropDownList ddlModel = rows.FindControl("ddlModel") as DropDownList;
            Label hidnDeviceWeight = rows.FindControl("hidnDeviceWeight") as Label;
            decimal currentWeight = 0;
            if (hidnDeviceWeight != null)
                decimal.TryParse(hidnDeviceWeight.Text, out currentWeight);
            weight = weight + currentWeight;

        }
        hdnDeviceGridWeight.Value = weight.ToString();
        return weight;
    }
    private async Task<HttpResponseMessage> GenerateLabelAsync(string labelType)
    {
        try
        {
            if (ddlShippingAddress.SelectedValue != "0")
            {
                string StreetAddress = "", City = "", State = "", ZipCode = "", LocationName = "", ShippingAttn = "", ShippingCarrierServiceCode = "";

                GetShippingAddress(ref StreetAddress, ref City, ref State, ref ZipCode, ref LocationName, ref ShippingAttn);

                //ShippingCarrierServiceCode = CommonMethods.GetShippingServiceCode((ViewState["DTShippingMethodAcc"] as DataTable), ShippingCarrierServiceCode, ddlShippingOptionLabel.SelectedItem.Text);

                ShippingCarrierServiceCode = ddlShippingOptionLabel.SelectedValue;

                string cont = "";
                string apiResult = String.Empty;
                try
                {
                    var shipfromName = "";
                    var shipfromCompanyName = txtCompanyName.Text;
                    var workPhone = AppHelper.FormatTelePhoneNumber(hdnWorkPhone.Value);
                    var dheight = txtBoxHight.Text;
                    var dlenght = txtBoxLength.Text;
                    var dwirth = txtBoxWidth.Text;
                    int Wirth = int.Parse(dwirth);
                    int height = int.Parse(dheight);
                    int Lenght = int.Parse(dlenght);
                    if (!string.IsNullOrEmpty(ShippingCarrierServiceCode) && ShippingCarrierServiceCode != "NA" && ShippingCarrierServiceCode != "N/A")
                    {
                        ShipEngineHelper shipEngineHelper = new ShipEngineHelper();
                        ShipEngineAPI shipEngineResult = new ShipEngineAPI();
                        StreetAddress = StreetAddress.Trim().Replace("Attn:", "").Replace("Attn :", "").Trim();

                        decimal weightUnitValue = 0;
                        if (txtDeviceWeight.Text != "")
                        {
                            weightUnitValue = Convert.ToDecimal(txtDeviceWeight.Text);
                            /// weightUnitValue = txtDeviceWeight.Text;
                            txtDeviceWeight.Text = weightUnitValue.ToString();


                        }
                        else
                        {
                            weightUnitValue = orderAccessoryOrder.GetAccessoryWeightInform<decimal>(ShippingCarrierServiceCode, "weight");
                            weightUnitValue += GetDeviceWeight<decimal>(ShippingCarrierServiceCode, "weight");

                            txtDeviceWeight.Text = weightUnitValue.ToString();
                        }
                        shipEngineResult.ShippingMethodServiceCode = ShippingCarrierServiceCode;
                        shipEngineResult.Packages = new Packages();
                        shipEngineResult.Packages.WeightUnit = orderAccessoryOrder.GetAccessoryWeightInfo<string>(ShippingCarrierServiceCode, "unit");
                        shipEngineResult.Packages.WeightValue = weightUnitValue;
                        shipEngineResult.Packages.DimensionsHeight = Wirth;
                        shipEngineResult.Packages.DimensionsLength = Lenght;
                        shipEngineResult.Packages.DimensionsWidth = height;
                        shipEngineResult.Packages.LabelMessageReference1 = string.Empty;
                        shipEngineResult.Packages.LabelMessageReference2 = string.Empty;
                        shipEngineResult.Packages.LabelMessageReference3 = string.Empty;


                        shipEngineResult.DeliveryConfirmationRequest = "none";
                        if (chkSignatureRequired.Checked)
                        {
                            shipEngineResult.DeliveryConfirmationRequest = "signature";
                        }

                        {
                            shipEngineResult.ToAddress = new ShipEngineAddress();
                            shipEngineResult.ToAddress.Address = StreetAddress;
                            shipEngineResult.ToAddress.City = City;
                            shipEngineResult.ToAddress.State = State;
                            shipEngineResult.ToAddress.CompanyName = shipfromCompanyName;
                            shipEngineResult.ToAddress.IsResidential = "no";
                            shipEngineResult.ToAddress.Name = (string.IsNullOrEmpty(ShippingAttn) ? shipfromName : ShippingAttn);
                            shipEngineResult.ToAddress.Phone = workPhone;
                            shipEngineResult.ToAddress.Zip = ZipCode;

                            shipEngineResult.FromAddress = new ShipEngineAddress();
                            shipEngineResult.FromAddress.Address = "21828 Lassen St Unit O";
                            shipEngineResult.FromAddress.City = "Chatsworth";
                            shipEngineResult.FromAddress.State = "CA";
                            shipEngineResult.FromAddress.CompanyName = string.Empty;
                            shipEngineResult.FromAddress.IsResidential = "no";
                            shipEngineResult.FromAddress.Name = "WS Fulfillment";
                            shipEngineResult.FromAddress.Phone = "18777877678";
                            shipEngineResult.FromAddress.Zip = "91311";
                        }

                        cont = shipEngineHelper.FormAPIInput(shipEngineResult);

                        var strContent = new StringContent(cont, Encoding.UTF32, "application/json");

                        shipEngineResult = shipEngineHelper.CallShipEngineAPI(cont, ShipEngineHelper.CreateLabel);
                        if (shipEngineResult != null)
                        {
                            if (!string.IsNullOrEmpty(shipEngineResult.ErrorString))
                            {
                                ScriptManager.RegisterStartupScript(this, this.GetType(), "msg", "javascript:alert('" + shipEngineResult.ErrorString + "');", true);
                            }
                            apiResult = shipEngineResult.OutputString;
                        }
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "msg", "javascript:alert('Shipping Service value missing or NA!');", true);
                    }
                }

                catch (Exception ex)
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "msg", "javascript:alert('Error in API Call: " + ex.Message + "');", true);
                }

                if (apiResult != null && !string.IsNullOrEmpty(apiResult))
                {
                    SetAndUploadLabel(apiResult, 0, cont, labelType);
                }
            }
            else
            {
                SetMessage("Please select Shipping address");
                ddlShippingAddress.Focus();
            }
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "msg", "javascript:alert('Error in GenerateLabelAsync: " + ex.Message + "');", true);
        }
        return null;
    }

    private void GetShippingAddress(ref string StreetAddress, ref string City, ref string State, ref string ZipCode, ref string LocationName, ref string ShippingAttn)
    {
        try
        {
            DataTable dtShip = ViewState["DTShippingAddress"] as DataTable;
            DataRow dr = dtShip.AsEnumerable().Where(d => d["ShippingId"].GetStringFromObject().Equals(ddlShippingAddress.SelectedValue)).FirstOrDefault();
            if (dr != null)
            {
                StreetAddress = dr["StreetAddress"].GetStringFromObject();
                City = dr["City"].GetStringFromObject();
                State = dr["msstate"].GetStringFromObject();
                ZipCode = dr["ZipCode"].GetStringFromObject();
                LocationName = dr["LocationName"].GetStringFromObject();
                ShippingAttn = dr["ShippingAttn"].GetStringFromObject();
            }
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "msg", "javascript:alert('Error in ShippingAddressList: " + ex.Message + "');", true);
        }
    }

    private int SaveAPIResult(string cont, string apiResult, string trackingNumber, string labelType, string filename = "", string label_id = "")
    {
        if (objManageMasters == null)
            objManageMasters = new ManageMasters();
        //txtShippingTracking.Text = trackingNumber;
        //hdnLabelShippingTrackingNumber.Value = trackingNumber;
        objManageMasters.SPType = labelType;
        objManageMasters.RepairId = Convert.ToInt32(Request.QueryString["OrderId"]);
        objManageMasters.ShipingTrackingNumber = trackingNumber;
        objManageMasters.ShippingLabelId = label_id;
        objManageMasters.LabelResult = "{ \"Request\":" + cont + ",\"Response\":" + apiResult + "}";
        objManageMasters.CreatedBy = Conversion.ParseInt(Session[AppConstants.SessionAdminUserId]);
        if (!string.IsNullOrEmpty(filename))
        {
            objManageMasters.UniqueFileName = filename;
            objManageMasters.FileName = filename;
            objManageMasters.DocumentPath = Common.GetBaseURL + accessoryUploads + filename;
        }

        return objManageMasters.ClientRepair_Update_Specific();
    }

    private void SetAndUploadLabel(string apiResult, int repairID, string cont, string labelType)
    {
        try
        {
            repairID = Conversion.ParseInt(hdnOrderId.Value);
            string tracking_number = "", label_id = "";
            JObject jObject = JObject.Parse(apiResult);
            if (jObject != null)
            {
                var errors = jObject["errors"];
                if (errors != null)
                {
                    SaveAPIResult(cont, apiResult, string.Empty, labelType);

                    var errorItem = errors.Children();
                    var error_code = errorItem["error_code"].FirstOrDefault();
                    var message = errorItem["message"].FirstOrDefault();
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "msg", "javascript:alert('Error in API Call, Error Code =" + error_code
                        + ", Message = " + message + "');", true);
                }
                else
                {
                    int deviceLeaseBatchId = 0;
                    string filename = "", postedFileName = "", moduleName = "";
                    try
                    {
                        var status = jObject["status"];
                        var label_download = Conversion.ParseString(jObject["label_download"]["pdf"]);
                        tracking_number = Conversion.ParseString(jObject["tracking_number"]);
                        label_id = Conversion.ParseString(jObject["label_id"]);

                        txtShippingTrackingNumber.Text = tracking_number;

                        if (ddlShippingMethod.SelectedIndex == 0)
                        {
                            if (ddlShippingOptionLabel.SelectedValue.IndexOf("ups") > -1)
                                ddlShippingMethod.SelectedValue = ddlShippingMethod.Items.FindByText("UPS").Value;
                            if (ddlShippingOptionLabel.SelectedValue.IndexOf("usps") > -1)
                                ddlShippingMethod.SelectedValue = ddlShippingMethod.Items.FindByText("USPS").Value;
                        }

                        //repairID = Convert.ToInt32(Request.QueryString["RepairId"]);
                        filename = repairID + "_" + tracking_number + "_" + Path.GetFileName(label_download);
                        postedFileName = filename;

                        deviceLeaseBatchId = SaveAPIResult(cont, apiResult, tracking_number, labelType, filename, label_id);

                        moduleName = OTNewActivationOrder;
                        attachedLabelFile = AppHelper.CreateDirectory(HttpContext.Current.Server.MapPath(accessoryUploads)) + filename;
                        using (WebClient web1 = new WebClient())
                        {
                            web1.DownloadFile(label_download, attachedLabelFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "msg", "javascript:alert('Error in DownloadFile: " + ex.Message + "');", true);
                    }
                    SiteAttachments objAttachments = new SiteAttachments();
                    FormAttachmentModel(repairID, objAttachments, filename, postedFileName, moduleName, OTNewActivationOrder);
                    objAttachments.SPType = AppConstants.INSERT;
                    int res = 0;

                    {
                        res = BindDataAttachment(grdShippingLabel, objAttachments); if (grdShippingLabel.Rows.Count > 0)
                            grdShippingLabel.Rows[0].Style.Add("background-color", "yellowgreen");
                    }
                    //Send label to customer
                    try
                    {
                        //attachedLabelFile = string.Empty;
                        //tbl1.Visible = false;
                        //tbl2.Visible = true;
                        //ScriptManager.RegisterStartupScript(this, this.GetType(), "", "javascript:makeMessage('confirm');", true);
                        /*
                        string sitePath = Common.GetBaseURL + "Uploads/DeviceReplacement/ShipLabel/" + filename;
                        hypShippingPdf.PostBackUrl = sitePath;
                        hypShippingPdf_Click(null, null);

                        string navigateURL = " <script>window.open('" + sitePath + "','_blank') </script>";
                        ClientScript.RegisterClientScriptBlock(this.GetType(), "JSScriptBlock", navigateURL);
                        */

                    }
                    catch (Exception ex)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "msg", "javascript:alert('Error in EmailMethod: " + ex.Message + "');", true);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "msg", "javascript:alert('Error in SetAndUploadLabel: " + ex.Message + "');", true);
        }
    }

    private void FormAttachmentModel(int repairID, SiteAttachments objAttachments, string filename, string postedFileName, string moduleName, string subModuleName)
    {
        objAttachments.UniqueFileName = filename;
        objAttachments.FileName = filename;
        objAttachments.DocumentPath = Common.GetBaseURL + accessoryUploads + filename;

        objAttachments.UserId = Conversion.ParseInt(HttpContext.Current.Session[AppConstants.SessionAdminUserId]);
        objAttachments.PrimaryId = repairID;
        objAttachments.IsDeleted = 0;
        objAttachments.CreatedDate = DateTime.Now;
        objAttachments.ModuleName = moduleName;
        objAttachments.SubModuleName = subModuleName;

        //return objAttachments;
    }

    private int BindDataAttachment(GridView grd, SiteAttachments ObjAttach)
    {
        int res;
        DataTable dtData = ObjAttach.SaveAdminAttachment(out res);

        BindGrid(grd, dtData);

        return res;
    }

    private void BindGrid(GridView grd, DataTable dtData)
    {
        if (grd != null && dtData != null)
        {
            DataTable dt = dtData;

            if (grd.ID == "grdShippingLabel")
            {
                var dtTable = dtData.AsEnumerable().Where(d => d["SubModuleName"].ToString() == OTNewActivationOrder);
                dt = CoptDataTable(dt, dtTable);
                hidTotalRecordsShipping.Value = dt.Rows.Count.ToString();
            }

            if (grd.ID == "grdAttachments")
            {
                var dtTable = dtData.AsEnumerable().Where(d => d["SubModuleName"].ToString() != AppConstants.SHIPPINGLABEL_ORDERACCESSORY);
                dt = CoptDataTable(dt, dtTable);
                hdnItemsCount.Value = dt.Rows.Count.ToString();
            }

            grd.DataSource = dt;
            grd.DataBind();
            decimal weightUnitAccsValue = orderAccessoryOrder.GetAccessoryWeightInform<decimal>("ddlShippingOptionLabel.SelectedValue", "weight");
            decimal weightUnitValue = GetDeviceWeight<decimal>("ddlShippingOptionLabel.SelectedValue", "weight");
            // textGetWeight();
            //GetGridTotalDeviceWeight();


            //GetDeviceWeight("usps_first_class_mail", "unit");
        }

    }

    private static DataTable CoptDataTable(DataTable dt, EnumerableRowCollection<DataRow> dtTable)
    {
        if (dtTable != null)
        {
            try
            {
                dt = dtTable.CopyToDataTable();
            }
            catch (Exception ex)
            {
                dt = new DataTable();
            }
        }

        return dt;
    }
    private T GetDeviceWeight<T>(string ShippingCarrierServiceCode, string unit)
    {

        decimal weight = 0;
        string resunit = "";
        if (unit == "weight")

            foreach (GridViewRow rows in grdOrderInfo.Rows)
            {

                Dictionary<string, string> objLineItemTokens = new Dictionary<string, string>();
                DropDownList ddlMake = rows.FindControl("ddlMake") as DropDownList;
                DropDownList ddlModel = rows.FindControl("ddlModel") as DropDownList;
                Label hidnDeviceWeight = rows.FindControl("hidnDeviceWeight") as Label;


                decimal currentWeight = 0;


                if (hidnDeviceWeight != null)
                    decimal.TryParse(hidnDeviceWeight.Text, out currentWeight);
                weight = weight + currentWeight;

            }
        hdnLineWeight.Value = weight.ToString();

        resunit = "pound";
        if (ShippingCarrierServiceCode == "usps_first_class_mail")
        {
            resunit = "ounce";
            weight = weight * 16;
        }
        //txtDeviceWeight.Text = weight.ToString();

        //weight = Convert.ToDecimal(txtDeviceWeight.Text);
        if (unit == "unit")
            return (T)Convert.ChangeType(resunit, typeof(T));
        if (unit == "weight")
            return (T)Convert.ChangeType(weight, typeof(T));

        return (T)Convert.ChangeType(weight, typeof(T));
    }

    #region Grid Shipping Label
    protected void grdShippingLabel_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {

    }

    protected void grdShippingLabel_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {

    }

    protected void grdShippingLabel_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }

    protected void grdShippingLabel_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.Pager)
        {
            Sindhu.PagingControl Paging1 = (Sindhu.PagingControl)e.Row.FindControl("PagingPhone");
            Label lblPagingSummary = (Label)e.Row.FindControl("lblPagingSummaryPhone");
            Paging1.PageSize = grdShippingLabel.PageSize;
            try
            {
                Paging1.TotalRecord = Convert.ToInt32(hidTotalRecordsShipping.Value);
            }
            catch
            {
                Paging1.TotalRecord = 1;
            }
            Paging1.CurrentPage = grdShippingLabel.PageIndex + 1;
            Paging1.DataLoad();

            lblPagingSummary.Text = "Total records : " + hidTotalRecordsShipping.Value + ", Page " + Paging1.CurrentPage.ToString() + " of " + Paging1.TotalPages.ToString() + "";
            int TotalPages = Paging1.TotalPages;
            Paging1.Visible = true;
            if (TotalPages <= 1)
            {
                Paging1.Visible = false;
            }
        }
    }

    protected void grdShippingLabel_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "DeleteData")
        {
            int adminTaskAttachmentId = 0;
            GridView gvPhone = (GridView)sender;
            DataKey key = gvPhone.SelectedDataKey;
            int.TryParse(e.CommandArgument.ToString().Split('|')[0], out adminTaskAttachmentId);

            Initialize(siteattach);
            objAttach.SPType = AppConstants.DELETE;
            objAttach.PrimaryId = GetQueryId(OrderId);
            objAttach.AttachmentId = adminTaskAttachmentId;
            objAttach.TaskCreatedBy = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);
            objAttach.CreatedDate = DateTime.Now;
            objAttach.DeletedDate = DateTime.Now;
            LoadAttachments(objAttach.SaveAdminAttachment(out adminTaskAttachmentId));

            if (e.CommandArgument.ToString().Split('|').Length > 1)
                VoidLabel(e.CommandArgument.ToString().Split('|')[1]);

            LoadAttachments(grdShippingLabel, "");
        }
    }

    public ShipEngineLabel VoidLabel(string labelid)
    {
        ShipEngineHelper shipEngineHelper = new ShipEngineHelper();
        ShipEngineLabel shipEngineAPI = shipEngineHelper.PutShipEngineAPI(new ShipEngineLabel()
        {
            ShippingTrackingId = labelid,
            PrimaryId = (hdnOrderId.Value),
            LabelDate = DateTime.Now.ToString(),
            LabelType = "VOID",
            LabelStatus = AppConstants.COMPLETED.ToLower(),
            InputString = labelid,
            CreatedBy = (HttpContext.Current.Session[AppConstants.SessionAdminUserId]).GetStringFromObject(),
            ClientId = Conversion.ParseInt(hdnClientID.Value)
        }); ;
        return shipEngineAPI;
    }

    public void PagingPhone_Click(object sender, CommandEventArgs e)
    {
        PagingControl pg = (PagingControl)sender;
        string CurrentPage = e.CommandArgument.ToString();

        if (pg.ClientID.Contains("grdShippingLabel"))
        {
            grdShippingLabel.PageIndex = Convert.ToInt32(CurrentPage) - 1;
            //LoadAttachments(grdShippingLabel, "");
        }

    }
    #endregion

    #endregion

    public string FormShippingAddress()
    {
        string StreetAddress = "", City = "", State = "", ZipCode = "", LocationName = "", ShippingAttn = "", address = "";

        GetShippingAddress(ref StreetAddress, ref City, ref State, ref ZipCode, ref LocationName, ref ShippingAttn);

        if (!string.IsNullOrEmpty(ShippingAttn))
            address += "Attn: " + ShippingAttn + "<br/>";
        if (!string.IsNullOrEmpty(LocationName))
            address += LocationName + "<br/>";
        if (!string.IsNullOrEmpty(StreetAddress))
            address += StreetAddress + "<br/>";
        if (!string.IsNullOrEmpty(City))
            address += City;
        if (!string.IsNullOrEmpty(State))
            address += ", " + State;
        if (!string.IsNullOrEmpty(ZipCode))
            address += "<br/>" + ZipCode;
        return address;
    }

    public string GetDeviceDataHTML()
    {
        string htmlFile = System.Web.HttpContext.Current.Server.MapPath(@"\Admin\Template\");
        var fullHTML = File.ReadAllText(htmlFile + @"\OrderPackageSlipNewActivationDevices.html");
        string itemHTML = "", html = ""; int row = 0;
        List<string> listPackageSlipContent = new List<string>();
        foreach (GridViewRow rows in grdOrderInfo.Rows)
        {
            string packageSlipContent = "";
            //row = row + 1;
            //DropDownList ddlPortIn = item.FindControl("ddlPortIn") as DropDownList;
            //TextBox txtMobileNo = item.FindControl("txtMobileNo") as TextBox;
            //TextBox txtAreaCode = item.FindControl("txtAreaCode") as TextBox;
            //DropDownList ddlContract = item.FindControl("ddlContract") as DropDownList;
            //TextBox txtUserName = item.FindControl("txtUserName") as TextBox;
            //DropDownList ddlMake = (DropDownList)item.FindControl("ddlMake") as DropDownList;
            //DropDownList ddlModel = (DropDownList)item.FindControl("ddlModel") as DropDownList;
            DropDownList ddlMake = rows.FindControl("ddlMake") as DropDownList;
            DropDownList ddlModel = rows.FindControl("ddlModel") as DropDownList;
            //TextBox txtActNotes = item.FindControl("txtActNotes") as TextBox;
            //TextBox txtLineNotes = item.FindControl("txtLineNotes") as TextBox;
            //TextBox txtMEIDIMEI = item.FindControl("txtMEIDIMEI") as TextBox;
            //TextBox txtSimID = item.FindControl("txtSimID") as TextBox;
            //DropDownList ddldivRatePlan = item.FindControl("ddldivRatePlan") as DropDownList;
            //DropDownList ddlFeatures = item.FindControl("ddlFeatures") as DropDownList;
            Label make = rows.FindControl("lblMake") as Label;
            Label model = rows.FindControl("lblModel") as Label;
            //TextBox txtGridAccessory = item.FindControl("txtGridAccessory") as TextBox;
            //TextBox txtGridAccessoryCount = item.FindControl("txtGridAccessoryCount") as TextBox;

            packageSlipContent = make.Text + " " + model.Text;

            listPackageSlipContent.Add(packageSlipContent);
            //itemHTML = fullHTML;
            //itemHTML = itemHTML.Replace("%PORTIN%", ddlPortIn.SelectedItem.Text);
            //itemHTML = itemHTML.Replace("%MOBILENO%", txtMobileNo.Text);
            //itemHTML = itemHTML.Replace("%CONTRACT%", ddlContract.SelectedItem.Text);
            //itemHTML = itemHTML.Replace("%RATEPLAN%", ddldivRatePlan.SelectedItem.Text);
            //itemHTML = itemHTML.Replace("%MAKE%", ddlMake.SelectedItem.Text);
            //itemHTML = itemHTML.Replace("%MODEL%", ddlModel.SelectedItem.Text);
            //itemHTML = itemHTML.Replace("%FEATURE%", ddlFeatures.SelectedItem.Text);
            //itemHTML = itemHTML.Replace("%COST%", ddlMake.SelectedItem.Text);
            //itemHTML = itemHTML.Replace("%IMEI%", txtMEIDIMEI.Text);
            //itemHTML = itemHTML.Replace("%SIMID%", txtSimID.Text);

            //itemHTML = itemHTML.Replace("%SNO%", row.ToString());
            //itemHTML = itemHTML.Replace("%MAKEMODEL%", ddlMake.SelectedItem.Text + " " + ddlModel.SelectedItem.Text);

            //html += itemHTML;
        }

        var list = listPackageSlipContent.GroupBy(d => d)
            .OrderBy(d => d.Key)
            .ToList();
        foreach (var item in list)
        {
            row = row + 1;

            itemHTML = fullHTML;

            itemHTML = itemHTML.Replace("%SNO%", row.ToString());
            itemHTML = itemHTML.Replace("%MAKEMODEL%", item.Key);
            itemHTML = itemHTML.Replace("%QTY%", item.Count().ToString());

            html += itemHTML;
        }
        return html;
    }

    #region Child Usercontrol Methods
    public string GetAccessoryDataHTML()
    {
        return orderAccessoryOrder.GetAccessoryDataHTML();
    }

    public string GetInputValue(string inputName)
    {
        string value = "";
        switch (inputName)
        {
            case "lblOrderNo":
                value = lblOrderNo.Text;
                break;
            case "hdnWorkPhone":
                value = (this.orderAccessoryOrder.FindControl("hdnWorkPhone") as HiddenField).Value;
                break;
            case "ddlShippingOptionLabel":
                value = ddlShippingOptionLabel.SelectedItem.Text;
                break;
            case "ddlShippingOptionLabelValue":
                value = ddlShippingOptionLabel.SelectedValue;
                break;

            case "txtShippingTrackingNumber":
                value = txtShippingTrackingNumber.Text;
                break;
            case "grdShippingLabel":
                value = grdShippingLabel.Rows.Count.ToString();
                break;

            default:
                break;
        }
        return value;
    }
    #endregion


    #region OrderInfo

    protected void grdOrderInfo_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        try
        {
            int orderDetailId = 0;
            GridView gvPhone = (GridView)sender;
            int.TryParse(e.CommandArgument.ToString(), out orderDetailId);

            if (e.CommandName == "Edit")
            {
                if (ViewState["rptLine"] != null)
                {
                    using (DataTable dt = (DataTable)ViewState["rptLine"])
                    {
                        if (dt.Rows.Count > 0)
                        {
                            var dtNew = dt.AsEnumerable().Where(d => d["OrderDetailId"].ToString() == orderDetailId.ToString()).CopyToDataTable();
                            if (dt.Rows.Count > 0)
                            {
                                DataRow dr = dtNew.Rows[0];

                                RepeaterItem item = rptLine.Items[0];
                                HiddenField hdnOrderDetailId = item.FindControl("hdnOrderDetailId") as HiddenField;
                                hdnOrderDetailId.Value = dr["OrderDetailId"].ToString();

                                TextBox txtMobileNo = item.FindControl("txtMobileNo") as TextBox;
                                txtMobileNo.Text = dr["MobileNo"].ToString();
                                TextBox txtAreaCode = item.FindControl("txtAreaCode") as TextBox;
                                txtAreaCode.Text = dr["MobileNo"].ToString();
                                TextBox txtUserName = item.FindControl("txtUserName") as TextBox;
                                txtUserName.Text = dr["UserName"].ToString();
                                TextBox txtLineNotes = item.FindControl("txtLineNotes") as TextBox;
                                txtLineNotes.Text = dr["LineNotesInstruction"].ToString();
                                TextBox txtActNotes = item.FindControl("txtActNotes") as TextBox;
                                txtActNotes.Text = dr["ActOrderInternalNotes"].ToString();
                                TextBox txtMonthlyCost = item.FindControl("txtMonthlyCost") as TextBox;
                                txtMonthlyCost.Text = dr["MonthlyCost"].ToString();
                                TextBox txtMEIDIMEI = item.FindControl("txtMEIDIMEI") as TextBox;
                                txtMEIDIMEI.Text = dr["MEID_IMEI"].ToString();
                                TextBox txtSimID = item.FindControl("txtSimID") as TextBox;
                                txtSimID.Text = dr["SIMID"].ToString();
                                DropDownList ddlPortIn = item.FindControl("ddlPortIn") as DropDownList;
                                ddlPortIn.SelectedValue = dr["PortIn"].ToString();
                                DropDownList ddlContract = item.FindControl("ddlContract") as DropDownList;
                                ddlContract.SelectedValue = dr["ContractId"].ToString();
                                DropDownList ddlMake = item.FindControl("ddlMake") as DropDownList;
                                ddlMake.SelectedValue = dr["MakeId"].ToString();
                                ddlMake_SelectedIndexChanged(ddlMake, null);
                                DropDownList ddlModel = item.FindControl("ddlModel") as DropDownList;
                                ddlModel.SelectedValue = dr["ModelId"].ToString();
                                DropDownList ddldivRatePlan = item.FindControl("ddldivRatePlan") as DropDownList;
                                ddldivRatePlan.SelectedValue = dr["RatePlanId"].ToString();
                                DropDownList ddlFeatures = item.FindControl("ddlFeatures") as DropDownList;
                                ddlFeatures.SelectedValue = dr["FeatureId"].ToString();

                            }

                            btnAddNewLine.Visible = true;
                            btnAddNewLine.Text = "Update";

                        }


                    }
                }

            }
            if (e.CommandName == "DeleteData")
            {
                if (orderDetailId > 0)
                {
                    AdminOrders objOrders = new AdminOrders();
                    objOrders.SPType = AppConstants.DELETE;
                    objOrders.OrderDetailId = orderDetailId;
                    objOrders.SaveOrderDetailData();

                    LoadOrderRelated(); //BindData();

                }

            }
        }
        catch (Exception ex)
        {

        }

    }

    protected void grdOrderInfo_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }

    protected void grdOrderInfo_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            (e.Row.FindControl("lblRowNumber") as Label).Text = (e.Row.RowIndex + 1).ToString();
        }
        if (e.Row.RowType == DataControlRowType.Pager)
        {
            Sindhu.PagingControl Paging1 = (Sindhu.PagingControl)e.Row.FindControl("PagingGrdOrderInfo");
            Label lblPagingSummary = (Label)e.Row.FindControl("lblPagingSummaryOrderInfo");
            Paging1.PageSize = grdOrderInfo.PageSize;
            try
            {
                //Paging1.TotalRecord = Convert.ToInt32(hidTotalRecordsPhone.Value);
            }
            catch
            {
                Paging1.TotalRecord = 1;
            }
            Paging1.CurrentPage = grdOrderInfo.PageIndex + 1;
            Paging1.DataLoad();

            //lblPagingSummary.Text = "Total records : " + hidTotalRecordsPhone.Value + ", Page " + Paging1.CurrentPage.ToString() + " of " + Paging1.TotalPages.ToString() + "";
            int TotalPages = Paging1.TotalPages;
            Paging1.Visible = true;
            if (TotalPages <= 1)
            {
                Paging1.Visible = false;
            }
        }
    }

    protected void grdOrderInfo_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        grdOrderInfo.PageIndex = e.NewPageIndex;
    }

    protected void grdOrderInfo_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {

    }

    #endregion

    protected void grdShippingLabel_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {

    }


    protected void btnSaveMultiItems_Click(object sender, EventArgs e)
    {
        SaveOrderDetail(AppConstants.SAVEORDER);
        hdnIsMultiEditBtn.Value = "0";
    }

    protected void btnFirstPopUpCan_Click(object sender, EventArgs e)
    {
        hdnIsMultiEditBtn.Value = "0";
        btnUpdateMultiDetails.Visible = true;
    }

    protected void grdOrderInfo_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    protected void txtBoxDimension_TextChanged(object sender, EventArgs e)
    {

    }
}

internal class ExcelHelper
{
    public void DownloadExcel(DataTable dt, string filePath, Dictionary<int, string> formatting, Dictionary<int, string> columnName)
    {
        try
        {
            if (dt != null && dt.Rows.Count > 0)
            {
                string fileName = Path.GetFileName(filePath);
                if (dt.Columns.Contains("OldColumn"))
                    dt.Columns["OldColumn"].ColumnName = "NewColumn";

                ReplaceColumnName(dt, "OldColumn", "NewColumn");
                try
                {


                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        IXLWorksheet worksheet = workbook.AddWorksheet(dt, CleanSheetName(fileName));
                        worksheet.Tables.FirstOrDefault().DataType = XLDataType.Text;
                        worksheet.Tables.FirstOrDefault().ShowAutoFilter = false;

                        worksheet.Tables.FirstOrDefault().Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Tables.FirstOrDefault().Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        worksheet.Tables.FirstOrDefault().Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Tables.FirstOrDefault().Style.Border.LeftBorder = XLBorderStyleValues.Thin;

                        bool FirstRow = true;
                        //Range for reading the cells based on the last cell used.  
                        string readRange = "1:1";
                        foreach (IXLRow row in worksheet.Rows())
                        {

                            //If Reading the First Row (used) then add them as column name  
                            if (FirstRow)
                            {
                                //row.Style.Fill.SetBackgroundColor(XLColor.NonPhotoBlue);

                                //worksheet.Tables.FirstOrDefault().ShowAutoFilter = false;                    
                                worksheet.Tables.FirstOrDefault().Style.Font.FontColor = XLColor.FromTheme(XLThemeColor.Text1);
                                worksheet.Tables.FirstOrDefault().Style.Fill.SetBackgroundColor(XLColor.White);

                                //Checking the Last cell used for column generation in datatable  
                                readRange = string.Format("{0}:{1}", 1, row.LastCellUsed().Address.ColumnNumber);


                                foreach (IXLCell cell in row.Cells(readRange))
                                {
                                    cell.DataType = XLDataType.Text;
                                    cell.Style.Font.Bold = true;
                                    //cell.Style.Font.FontColor = XLColor.FromTheme(XLThemeColor.Text1);

                                    int colName = cell.Address.ColumnNumber;
                                    if (columnName != null)
                                    {
                                        var dataType = columnName.ContainsKey(colName) ? columnName[colName] : "";
                                        if (dataType != null)
                                        {
                                            var col = dataType.Split(new char[] { '|' });
                                            if (col.Length > 0)
                                            {
                                                if (col[0] == "Column")
                                                {
                                                    cell.Value = col[1];
                                                }
                                            }
                                        }
                                    }
                                }
                                FirstRow = false;
                            }
                            else
                            {
                                int cellIndex = 0;
                                foreach (IXLCell cell in row.Cells(readRange))
                                {
                                    cell.DataType = XLDataType.Text;
                                    cell.Style.Fill.BackgroundColor = XLColor.White;

                                    cell.SetValue(cell.Value).SetDataType(XLDataType.Text);

                                    int colName = cell.Address.ColumnNumber;
                                    if (formatting != null)
                                    {
                                        var dataType = formatting.ContainsKey(colName) ? formatting[colName] : "";
                                        if (dataType != null)
                                        {
                                            var col = dataType.Split(new char[] { '|' });
                                            if (col.Length > 0)
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(cell.Value)))
                                                {
                                                    if (col[0] == "Currency")
                                                    {
                                                        cell.Style.NumberFormat.Format = (col.Length > 1 ? col[1] : DefaultFormats.CurrencyRoundedNoSpace);
                                                        cell.DataType = XLDataType.Number;
                                                    }
                                                    else if (col[0] == "Date")
                                                    {
                                                        cell.Style.NumberFormat.Format = (col.Length > 1 ? col[1] : DefaultFormats.DateMMddyyyyHHmmAMPM);
                                                        cell.DataType = XLDataType.DateTime;
                                                    }
                                                    else if (col[0] == "Boolean")
                                                    {
                                                        cell.Value = Convert.ToString(cell.Value) == "1" ? "TRUE" : "FALSE";
                                                    }
                                                    else if (col[0] == "NumberField" || col[0] == "NumberFieldNo")
                                                    {
                                                        int v = 0;
                                                        int.TryParse(cell.Value.ToString(), out v);
                                                        cell.Value = v.ToString();
                                                        cell.Style.NumberFormat.Format = "#,##0";
                                                        cell.DataType = XLDataType.Number;
                                                    }
                                                    else if (col[0] == "DecimalOne" || col[0] == "DecimalOneNo")
                                                    {
                                                        decimal v = 0;
                                                        decimal.TryParse(cell.Value.ToString(), out v);
                                                        cell.Value = v.ToString();
                                                        cell.Style.NumberFormat.Format = "#,##0.0"; cell.DataType = XLDataType.Number;
                                                    }
                                                    else if (col[0] == "DecimalTwo" || col[0] == "DecimalTwoNo")
                                                    {
                                                        decimal v = 0;
                                                        decimal.TryParse(cell.Value.ToString(), out v);
                                                        cell.Value = v.ToString();
                                                        cell.Style.NumberFormat.Format = "#,##0.00"; cell.DataType = XLDataType.Number;
                                                    }
                                                    else if (col[0] == "PercentageOne" || col[0] == "PercentageOneNo")
                                                    {
                                                        decimal v = 0;
                                                        decimal.TryParse(cell.Value.ToString(), out v);
                                                        cell.Value = v.ToString();
                                                        cell.Style.NumberFormat.Format = "#,##0.0%"; cell.DataType = XLDataType.Number;
                                                    }
                                                    else if (col[0] == "PercentageTwo" || col[0] == "PercentageTwoNo")
                                                    {
                                                        decimal v = 0;
                                                        decimal.TryParse(cell.Value.ToString(), out v);
                                                        cell.Value = v.ToString();
                                                        cell.Style.NumberFormat.Format = "#,##0.00%"; cell.DataType = XLDataType.Number;
                                                    }
                                                    else if (col[0] == "TickMark")
                                                    {
                                                        cell.Value = char.ConvertFromUtf32(0x2713);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    cellIndex++;
                                }
                            }
                            row.AdjustToContents();
                        }
                        //List<string[]> HeaderRows = new List<string[]>();
                        //string[] Columns = new string[] { "1:3|Column Name 1", "4:6|Column Name 2" };//Row 1
                        //HeaderRows.Add(Columns);
                        //Columns = new string[] { "1:3|Column Name 3", "4:6|Column Name 4" };//Row 1
                        //HeaderRows.Add(Columns);

                        //// Add Header Rows
                        //if (HeaderRows != null && HeaderRows.Count > 0)
                        //{
                        //    worksheet.Row(1).InsertRowsAbove(HeaderRows.Count + 1);
                        //    int rowIndex = 1;
                        //    foreach (string[] strItems in HeaderRows)
                        //    {
                        //        for (int item = 0; item < strItems.Length; item++)
                        //        {
                        //            var index = strItems[item].Split(new char[] { '|' });
                        //            var columnIndex = index[0].Split(new char[] { ':' });

                        //            int startIndex = 0, endIndex = 0;
                        //            int.TryParse(columnIndex[0], out startIndex);
                        //            int.TryParse(columnIndex[1], out endIndex);

                        //            string columnNameFromIndex = "";
                        //            if (index.Length > 1)
                        //                columnNameFromIndex = index[1];
                        //            var cellMerge = worksheet.Range(worksheet.Cell(rowIndex + 1, startIndex), worksheet.Cell(rowIndex + 1, endIndex));
                        //            cellMerge.Merge();
                        //            cellMerge.Value = columnNameFromIndex;
                        //            cellMerge.Style.Font.Bold = true;
                        //            cellMerge.Style.Fill.SetBackgroundColor(XLColor.DarkBlue);
                        //        }
                        //        rowIndex++;
                        //    }
                        //}

                        // Add Report information
                        //worksheet.Row(1).InsertRowsAbove(5);
                        //worksheet.Row(1).Cell(2).Value = "Report Name";
                        //worksheet.Row(2).Cell(2).Value = "Filter From: ";
                        //worksheet.Row(3).Cell(2).Value = "Filter to: ";

                        // Download
                        //Response.Clear();
                        //Response.Buffer = true;
                        //Response.Charset = "";
                        //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        //Response.AddHeader("content-disposition", "attachment;filename=" + fileName + ".xlsx");

                        SendEmail objEmail = new SendEmail();
                        Dictionary<string, string> objEmailTokens = new Dictionary<string, string>();
                        List<Dictionary<string, string>> listEmailTokens = new List<Dictionary<string, string>>();
                        string[] att = null;
                        using (MemoryStream MyMemoryStream = new MemoryStream())
                        {
                            workbook.SaveAs(filePath);
                            //workbook.SaveAs(MyMemoryStream);
                            //MyMemoryStream.WriteTo(Response.OutputStream);
                            //Response.Flush();
                            //Response.End();


                            //System.Net.Mail.Attachment attFile = new System.Net.Mail.Attachment(MyMemoryStream, "file.xls", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                            //System.Net.Mail.Attachment attFile = new System.Net.Mail.Attachment(MyMemoryStream, MediaTypeNames.Application.Octet);

                            //objEmail.AttachmentListObject = new List<System.Net.Mail.Attachment>();
                            //objEmail.AttachmentListObject.Add(attFile);
                            //try
                            //{
                            //    objEmail.SendUserEmail("nautnajarlura@gmail.com", objEmailTokens, listEmailTokens, "NewActivationOrder", "NewActivationOrderLineItem", att, "SUPPORT", "");
                            //}
                            //catch (Exception ex)
                            //{
                            //    //here 1
                            //    throw ex;
                            //}
                        }
                    }
                }
                catch (Exception ex)
                {

                    //here 3
                    throw ex;
                }
            }
        }
        catch (Exception ex)
        {
            //here 2
            throw ex;
        }
    }


    private string CleanSheetName(String str1)
    {
        Regex reg = new Regex("[*'\",_&#^@]");
        str1 = reg.Replace(str1, string.Empty);

        Regex reg1 = new Regex("[ ]");
        str1 = reg.Replace(str1, "-");
        return str1;
    }

    private DataTable RemoveColumns(DataTable dt, string[] columnsToRemove)
    {
        if (dt != null)
        {
            foreach (string colName in columnsToRemove)
            {
                if (dt.Columns.Contains(colName))
                {
                    dt.Columns.Remove(colName);
                    dt.AcceptChanges();
                }

            }
            dt.AcceptChanges();
        }
        return dt;
    }

    private DataTable SetColumnOrder(DataTable dt, string[] columnInOrder)
    {
        int columnIndex = 0;
        foreach (var columnName in columnInOrder)
        {
            dt.Columns[columnName].SetOrdinal(columnIndex);
            columnIndex++;
        }

        DataTable dtCopy = new DataTable();
        foreach (DataColumn item in dt.Columns)
        {
            if (!dtCopy.Columns.Contains(item.ColumnName))
            {
                dtCopy.Columns.Add(item.ColumnName, System.Type.GetType("System.String"));
            }
        }

        dtCopy = dt.Copy();
        return dtCopy;
    }

    private DataTable ReplaceColumnName(DataTable dt, string oldColumn, string newColumn)
    {
        if (dt.Columns.Contains(oldColumn))
            dt.Columns[oldColumn].ColumnName = newColumn;

        return dt;
    }
}



public static class ExcelMultipleColumns
{
    public static List<string[]> Rows { get; set; }
    public static string[] Columns { get; set; }
    public static List<string> ListColumns { get; set; }
}
public static class DefaultFormats
{
    public static string Currency = "_($#,##0.00_);_(($#,##0.00);_($0.00_);_(@_)";
    public static string CurrencyRoundedNoSpace = "_($#,##0_);_(($(#,##0);_($0_);_(@_)";
    public static string DateMMddyyyyHHmmAMPM = "MM/dd/yyyy HH:mm AM/PM";
    public static string DateMMddyyyyHHmm = "MM/dd/yyyy HH:mm";
    public static string DateMMddyyyy = "MM/dd/yyyy";
    public static string DateddMMyyyyHHmmAMPM = "dd/MM/yyyy HH:mm AM/PM";
    public static string DateddMMyyyyHHmm = "dd/MM/yyyy HH:mm";
    public static string DateddMMyyyy = "dd/MM/yyyy";
    public static string PercentageRounded = "0%";
    public static string Percentage = "0.00%";
}
public static class Constants
{
    public static string Title = "Title";
    public static string Font = "Font";
    public static string IsBold = "Bold";

}