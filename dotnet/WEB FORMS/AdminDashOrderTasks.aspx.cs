using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Classes;
using System.Data;
using System.IO;
using System.Configuration;
using System.Web.UI.HtmlControls;
using System.Text;
using Classes.Activity;
using System.Web.Services;
using PDFCommon = TheArtOfDev.HtmlRenderer.Demo.Common;
using PDFSharp = TheArtOfDev.HtmlRenderer.PdfSharp;

public partial class AdminDashOrderTasks : PageBaseAdmin
{
    ManageMasters objManageMasters;
    AdminTasks objTasks = new AdminTasks();
    AdminUserLogin adminUserLogin = new AdminUserLogin();
    const string AdminTaskId = "id";
    string adminDashTabNo = "1";

    string orderPackageSlipNewActivation = "OrderPackageSlipNewActivation";
    string PermAddMsg = "All orders need to be logged on the following client <br>CRM site: <b>#companyname#</b>";
    string INPERSONPICKUP = "IN PERSON PICKUP";
    protected void Page_Load(object sender, EventArgs e)
    {
        #region Check if the Session for Logged In user is Available
        if (AppHelper.CheckSessionEmpty())
        {
            Response.Redirect(Common.GetBaseURL + "admin/default.aspx", true);
        }
        #endregion
        ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
        scriptManager.RegisterPostBackControl(this.btnPrint);

        //orderNewActivation.TheFunc =SendOrderNotf();
        //lblMsg.Text = "";
        //lblMsgDown.Text = "";
        adminDashTabNo = Request.QueryString["b"].GetStringFromObject();
        if (!IsPostBack)
        {
            //txtResponseToOrderDesk.Text = FormDateTimeWithUserName();
            BindPermission(); LoadDefaultValues();
            if (Request.QueryString["s"] != null)
            {
                SetMessage("Line created successfully");
            }
            if (Request.QueryString["scm"] != null)
            {
                SetMessageBig("Order submitted successfully");
            }

            if (Request.QueryString["con"] != null)
            {

                lblMsg.Text = "";
                lblMsgDown.Text = "";
                string msg = "";
                if (Request.QueryString["con"].ToString() == "True")
                {
                    msg = "Email sent to client";
                }
                else if (Request.QueryString["con"].ToString() == "res")
                {
                    msg = "Email sent to Order desk";
                }
                else
                    msg = "Email NOT sent";

                SetMessage(msg);
            }
           
            GetActivityLog();

        }

    }

    private string FormDateTimeWithUserName()
    {
        return DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " " + Session[AppConstants.SessionUserID] + " :";
    }

    #region Button Click Events
    protected void btnSaveClose_Click(object sender, EventArgs e)
    {
        ValidateAndSave(AppConstants.SAVEANDCLOSE, false);
    }

    protected void btnClose_Click(object sender, EventArgs e)
    {
        RedirectPage();
    }

    protected void btnAttachFile_Click(object sender, EventArgs e)
    {
        try
        {
            UploadFile();
        }
        catch (Exception ex)
        {
            SetMessage(ex.Message);
        }
    }

    protected void btnResponseCompleted_Click(object sender, EventArgs e)
    {
        DataTable dt = new DataTable();
        string clientEmail = "";
        dt = GetClientContent(dt);

        clientEmail = GetEmailAddress(dt, clientEmail);
        //clientEmail = "nautnajarlura@gmail.com";
        if (clientEmail != "" && clientEmail.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Count() > 0)
        {
            string url = ResolveClientUrl("~/Admin/AdminDashResp.aspx?id=" + GetTaskIdFromQueryString() + "&cid=" + hdnClientID.Value + "&rid=" + ddlRep.SelectedValue + "&eid=" + clientEmail + "&keepThis=true&TB_iframe=true&height=450&width=850");
            ScriptManager.RegisterStartupScript(this, GetType(), "OpenPdopUp", "OpenThickBox('" + url + "','Response Templates ');", true);
        }
        else
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "", "javascript:alert('Email is not available. Unable to send email.')", true);
        }


    }

    private DataTable GetClientContent(DataTable dt)
    {
        if (objManageMasters == null)
            objManageMasters = new ManageMasters();

        objManageMasters.ClientId = hdnClientID.Value == "" ? 0 : Convert.ToInt32(hdnClientID.Value);
        objManageMasters.AdminTaskId = hdnAdminTaskId.Value == "" ? 0 : Convert.ToInt32(hdnAdminTaskId.Value);
        if (objManageMasters.ClientId > 0)
        {
            //dt = objManageMasters.BusinessClient_GetById();
            dt = objManageMasters.GetContactInfoByAssociatedUser();
        }
        return dt;
    }

    protected void btnResponse_Click(object sender, EventArgs e)
    {
        SendResponse(AppConstants.TASKRESPONSE, btnResponse.Text);
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        ValidateAndSave(AppConstants.SAVE, false);
    }

    protected void lnkbtn_Click(object sender, EventArgs e)
    {
        string url = ResolveClientUrl("~/Admin/AdminDashPrev.aspx?id=" + GetTaskIdFromQueryString() + "&keepThis=true&TB_iframe=true&height=450&width=850");
        ScriptManager.RegisterStartupScript(this, GetType(), "OpenPdopUp", "OpenThickBox('" + url + "','Previous Emails');", true);
    }

    #endregion

    #region Methods
    private void UploadFile()
    {
        Int64 AdminTaskId = Conversion.ParseInt(AppHelper.GetValueFromQueryString("id", this.Context));// ValidateAndSave(AppConstants.SAVEANDUPLOAD, false);
        if (AdminTaskId == 0)
        {
            SetMessage("Could not save the details.");
        }
        else if (fUAttachment.HasFile && fUAttachment.PostedFile.ContentLength > 0)
        {
            AdminTasks objTasks = new AdminTasks();

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

            objTasks.SPType = AppConstants.INSERT;
            objTasks.LocalPath = imagePath + "/" + filename;
            objTasks.DocumentPath = Common.GetBaseURL + "Uploads/AdminTasks/" + filename;
            objTasks.UniqueFileName = filename;
            objTasks.FileName = postedFileName;
            objTasks.TaskCreatedBy = Conversion.ParseInt(HttpContext.Current.Session[AppConstants.SessionAdminUserId]);
            objTasks.AdminTaskId = Conversion.ParseInt(AppHelper.GetValueFromQueryString("id", this.Context));
            objTasks.IsDeleted = 0;
            objTasks.CreatedDate = DateTime.Now;
            objTasks.DeletedDate = DateTime.Now;

            int AdminTaskAttachId = objTasks.SaveAdminTaskAttachment();
            if (AdminTaskAttachId > 0)
            {
                SetMessage("File saved!");
                LoadAttachments(AdminTaskId);
            }
            else
            {
                SetMessage("File not saved!");
            }
        }
        else
        {
            SetMessage("Please select a file.");
        }
    }


    private Int64 ValidateAndSave(string buttonName, bool redirect)
    {
        lblMsg.Text = "";
        HiddenField hdnIsValidOrderEmail = null;// this.orderNewActivation.FindControl("hdnIsValidOrderEmail") as HiddenField;

        if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeNewActivationOrder.ToLower())
        {
            hdnIsValidOrderEmail = this.orderNewActivation.FindControl("hdnIsValidOrderEmail") as HiddenField;
        }
        if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeUpgradeOrder.ToLower())
        {
            hdnIsValidOrderEmail = this.orderUpgradeOrder.FindControl("hdnIsValidOrderEmail") as HiddenField;
        }
        if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeGPSOrder.ToLower())
        {
            hdnIsValidOrderEmail = this.orderGPS.FindControl("hdnIsValidOrderEmail") as HiddenField;
        }

        if (txtCompanyName.Text.Trim() == "" && txtContact.Text.Trim() == "" && txtEmailAddress.Text.Trim() == "")
        {
            lblMsg.Text = "Please select Company Name or Contact Name or Email Address";
        }
        else if (hdnIsValidOrderEmail != null && hdnIsValidOrderEmail.Value != string.Empty)
        {
            lblMsg.Text = "Invalid Order emails.";
        }
        else if (hdnClientID.Value != "" && txtContact.Text != "")
        {
            ManageMasters objMaster = new ManageMasters();
            bool result = objMaster.BLBusinessClient_GetBusinessClientInfo(AppConstants.CONTACT, txtCompanyName.Text.Trim(), txtContact.Text.Trim());
            if (!result)
            {
                //lblMsg.Text = "Contact Name does not belong to selected company.";
                //return 1;
            }
        }
        if (txtEmailAddress.Text != "" && !AppHelper.IsValidEmail(txtEmailAddress.Text.Trim()))
        {
            lblMsg.Text = "Please enter valid email address";
            txtEmailAddress.Focus();
        }
        else if (ddlRep.SelectedIndex == 0)
        {
            lblMsg.Text = "Please select Rep";
            ddlRep.Focus();
        }
        else if (ddlStatus.SelectedIndex == 0)
        {
            lblMsg.Text = "Please select Status";
            ddlStatus.Focus();
        }
        else if (txtTaskHeading.Text.Trim() == "")
        {
            lblMsg.Text = "Please enter Task Heading";
            txtTaskHeading.Focus();
        }
        //else if (txtOrderInternalNotes.Text.Trim() == "")
        //{
        //    lblMsg.Text = "Please enter Order Internal notes";
        //    txtOrderInternalNotes.Focus();
        //}
        else if (txtResponseToOrderDesk.Text.Trim() == "" && buttonName == AppConstants.TASKORDRESPONSE)
        {
            lblMsg.Text = "Please enter Response to order desk";
            txtResponseToOrderDesk.Focus();
        }
        if (lblMsg.Text != "")
        {
            ShowAlert(lblMsg.Text);
            return 1;
        }

        string validationMessage = "";// orderNewActivation.SaveToManageLines(redirect, buttonName);

        if (buttonName != AppConstants.TASKRESPONSE && buttonName != AppConstants.TASKCOMPRESPONSE
            && buttonName != AppConstants.TASKORDRESPONSE && buttonName != AppConstants.SAVEONLY)
        {
            if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeNewActivationOrder.ToLower())
                validationMessage = orderNewActivation.SaveToManageLines(redirect, buttonName);
            if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeUpgradeOrder.ToLower())
                validationMessage = orderUpgradeOrder.SaveToManageLines(redirect, buttonName);
            if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeGPSOrder.ToLower())
                validationMessage = orderGPS.SaveToManageFeatures();

        }
        SaveAdminTask(buttonName, !redirect);

        if (buttonName == AppConstants.SAVEANDCLOSE && validationMessage.StartsWith("Saved!"))
        {
            RedirectPage();
        }
        if (validationMessage != "" && !validationMessage.StartsWith("Saved!"))
        {
            SetMessage(validationMessage);
        }

        return 1;
    }

    private void RedirectPage()
    {
        if (AppHelper.GetValueFromQueryString("pg", this.Context) != "")
            Response.Redirect("ManageOrdersHistory.aspx?ClientId=" + AppHelper.GetValueFromQueryString("ClientId", this.Context));
        else
            Response.Redirect("AdminDash.aspx?b=" + adminDashTabNo);
    }

    private void ShowAlert(string message)
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), "", "javascript:alert('" + message + "')", true);
    }

    private Int64 SaveAdminTask(string buttonName, bool redirect)
    {
        int retAdminTaskId = 0;
        objTasks = new AdminTasks();
        objManageMasters = new ManageMasters();
        try
        {
            int clientID;
            DataTable dtClient;
            GetClientInfo(out clientID, out dtClient);

            if (dtClient != null && dtClient.Rows.Count > 0)
            {
                int.TryParse(Convert.ToString(dtClient.Rows[0]["ClientID"]), out clientID);
                hdnClientID.Value = clientID.ToString();

                if (clientID > 0)
                {
                    retAdminTaskId = AdminTaskSaveProcess(buttonName, clientID, redirect);
                }
            }
            else
            {
                retAdminTaskId = AdminTaskSaveProcess(buttonName, clientID, redirect);
            }
        }
        catch (Exception ex)
        {
            SetMessage("Error: " + ex.Message.ToString());
        }

        return retAdminTaskId;
    }

    private int AdminTaskSaveProcess(string buttonName, int clientID, bool redirect)
    {
        int retAdminTaskId;
        int adminTaskId = AssignModelValue(clientID, buttonName);
        if (!redirect)
            objTasks.ActionType = AppConstants.SAVED;

        retAdminTaskId = objTasks.SaveAdminTask();
        objTasks.AdminTaskId = retAdminTaskId;
        objTasks.TaskAssignedRep = Convert.ToInt32(ddlRep.SelectedValue);
        objTasks.SaveAdminTaskHistory();

        if (redirect)
        {
            if (retAdminTaskId > 0 && buttonName == AppConstants.SAVE)
            {
                Response.Redirect("AdminDashNew.aspx?id=" + retAdminTaskId.ToString() + "&b=" + adminDashTabNo);
            }
            if (retAdminTaskId > 0 && buttonName == AppConstants.SAVEANDCLOSE)
            {
                Response.Redirect("AdminDash.aspx?res=1" + "&b=" + adminDashTabNo);
            }
        }
        if (retAdminTaskId > 0 && buttonName == AppConstants.SAVEANDUPLOAD)
        {
            LoadAttachments(adminTaskId);
        }

        return retAdminTaskId;
    }

    private int AssignModelValue(int clientID, string ActionType)
    {
        #region Assign Value for model
        int adminTaskId = GetTaskIdFromQueryString();

        objTasks.SPType = adminTaskId == 0 ? AppConstants.INSERT : AppConstants.UPDATE;
        objTasks.AdminTaskId = adminTaskId;
        objTasks.ActionType = ActionType;
        objTasks.RequestID = lblRequestID.Text;

        objTasks.TaskHeading = txtTaskHeading.Text;
        objTasks.Invoice = txtInvoice.Text;
        objTasks.ClientID = clientID;
        objTasks.TaskContact = txtContact.Text;
        objTasks.EmailAddress = txtEmailAddress.Text.Trim();
        objTasks.TaskNotes = txtOrderInternalNotes.Text;
        objTasks.TaskDetailsResponse = txtResponseToOrderDesk.Text;
        objTasks.TaskAssignedRep = Convert.ToInt32(ddlRep.SelectedValue);
        objTasks.TaskStatus = Convert.ToInt32(ddlStatus.SelectedValue);

        objTasks.ViewingAdminId = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);

        objTasks.TaskCreatedDateTime = DateTime.Now;
        objTasks.TaskCreatedBy = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);

        objTasks.TaskModifiedBy = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);

        objTasks.IsActive = 1;
        objTasks.IsDeleted = 0;
        #endregion
        return adminTaskId;
    }

    private void GetClientInfo(out int clientID, out DataTable dtClient)
    {
        clientID = 0;
        if (txtCompanyName.Text.Trim() != "")
            objManageMasters.SearchColumn = "CLIENT";
        if (txtContact.Text.Trim() != "")
            objManageMasters.SearchColumn = "CONTACT";
        if (txtCompanyName.Text.Trim() != "" && txtContact.Text.Trim() != "")
            objManageMasters.SearchColumn = "CLIENT_CONTACT";

        objManageMasters.SearchKeyWord = txtCompanyName.Text.Trim();
        objManageMasters.SearchKeyWord1 = txtContact.Text.Trim();
        if (txtContact.Text.Trim() == "" && txtCompanyName.Text.Trim() == "" && txtEmailAddress.Text != "")
        {
            objManageMasters.SearchColumn = "EMAIL";
            objManageMasters.SearchKeyWord1 = txtEmailAddress.Text.Trim();
        }
        dtClient = null;
        if (objManageMasters.SearchKeyWord != "" || objManageMasters.SearchKeyWord1 != "")
            dtClient = objManageMasters.BusinessClient_GetBusinessClientInfo();

        if (dtClient != null && txtCompanyName.Text.Trim() != "")
        {
            dtClient = dtClient.Select("CompanyName = '" + txtCompanyName.Text.Trim() + "'").CopyToDataTable();
        }
    }

    private void LoadTaskValues(string type)
    {
        if (objTasks == null)
            objTasks = new AdminTasks();

        objTasks.SPType = AppConstants.OPEN;
        objTasks.AdminTaskId = GetTaskIdFromQueryString();
        hdnAdminTaskId.Value = objTasks.AdminTaskId.ToString();
        objTasks.TaskHeading = "";
        objTasks.TaskStatus = 0;
        objTasks.TaskAssignedRep = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);
        DataSet ds = objTasks.GetAllTasks();

        if (objTasks.AdminTaskId > 0)
        {
            DataTable dt = ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                #region Assign Value for Fields
                lblTimeNow.Text = Convert.ToString(dt.Rows[0]["TaskCreatedDateTime"]);
                lblRequestID.Text = Convert.ToString(dt.Rows[0]["RequestID"]);
                txtTaskHeading.Text = Convert.ToString(dt.Rows[0]["TaskHeading"]);
                txtInvoice.Text = Convert.ToString(dt.Rows[0]["Invoice"]);
                txtCompanyName.Text = Convert.ToString(dt.Rows[0]["TaskCompany"]);
                txtContact.Text = Convert.ToString(dt.Rows[0]["TaskContact"]);
                txtOrderInternalNotes.Text = Convert.ToString(dt.Rows[0]["TaskNotes"]);
                txtEmailAddress.Text = Convert.ToString(dt.Rows[0]["EmailAddress"]);
                //txtOrderInternalNotes.Enabled = false;
                hdnTaskDetailsUserHistory.Value = Convert.ToString(dt.Rows[0]["HistoryTaskDetails"]);

                //this.txtTaskDetails.Text = Server.HtmlDecode(this.txtTaskDetails.Text);
                //txtResponseToOrderDesk.Text = Convert.ToString(dt.Rows[0]["TaskDetailsResponse"]);
                ddlRep.SelectedValue = Convert.ToString(dt.Rows[0]["ActRep"]);
                hdnCarrierId.Value = dt.Rows[0]["CarrierId"].GetStringFromObject();
                string deviceLeaseId = dt.Rows[0]["DeviceLeaseId"].GetStringFromObject();

                try
                {
                    hdnConversationId.Value = Convert.ToString(dt.Rows[0]["ConversationId"]);
                }
                catch (Exception ex) { }
                if (!string.IsNullOrEmpty(deviceLeaseId))
                {
                    divleaseterm.Visible = false;
                }

                // Section to load Sales Rep dropdown again based on permission
                if (Convert.ToString(Session[AppConstants.SessionAdminEmailAddress]) != AppConstants.AdminEmailAddress
                    && hdnAddEditPermission.Value != "1"
                    && (Convert.ToString(Session[AppConstants.SessionSaleRepID]) == ddlRep.SelectedValue || ddlRep.SelectedItem.Text == AppConstants.UNASSIGNED))
                {
                    objManageMasters = new ManageMasters();

                    objManageMasters.ShowFor = AppConstants.AddEdit;
                    objManageMasters.SalesRepId = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);

                    BindDropDownSalesRep();

                    ddlRep.SelectedValue = Convert.ToString(dt.Rows[0]["ActRep"]);
                }

                ddlStatus.SelectedValue = Convert.ToString(dt.Rows[0]["TaskStatusID"]);
                hdnTaskLock.Value = Convert.ToString(dt.Rows[0]["ViewingAdminId"]);

                txtResponseToOrderDesk.Text = "";// Convert.ToString(dt.Rows[0]["TaskNotes"]).Trim();
                hdnClientID.Value = Convert.ToString(dt.Rows[0]["ClientID"]).Trim();
                hdnTaskNotes.Value = Convert.ToString(dt.Rows[0]["TaskNotes"]).Trim();

                hdnLastEmailFrom.Value = Convert.ToString(dt.Rows[0]["LastEmailFrom"]).Trim();
                hdnOtherEmail.Value = Convert.ToString(dt.Rows[0]["OtherEmail"]).Trim();
                hdnOrderId.Value = Convert.ToString(dt.Rows[0]["OrderId"]).Trim();
                ddlOrderType.SelectedValue = Convert.ToString(dt.Rows[0]["TaskTypeName"]).Trim();

                string Notf_SentToClient = "";
                Notf_SentToClient = Convert.ToString(dt.Rows[0]["Notf_SentToClient"]);
                hdnSendOrderConfToClient.Value = Notf_SentToClient;
                if (!string.IsNullOrEmpty(Notf_SentToClient) && Notf_SentToClient != "0")
                    lblSendOrderConfToClient.Text = "Last notification sent on :" + Convert.ToString(dt.Rows[0]["Notf_SentToClientDate"]).Trim();

                int taskLockId = 0;
                int.TryParse(hdnTaskLock.Value, out taskLockId);
                if (taskLockId > 0 && taskLockId != Convert.ToInt32(Session[AppConstants.SessionAdminUserId]))
                {
                    //ViewMode();
                }
                if (ds.Tables.Count > 4 && ds.Tables[4] != null)
                {
                    string userNames = string.Join(",", ds.Tables[4].AsEnumerable().Select(d => d["UserID"]).ToList());
                    userNames = userNames.ToLower().Replace(Session[AppConstants.SessionUserID].ToString().ToLower(), "").Trim(new char[] { ',' });
                    if (userNames != "")
                    {
                        SetMessage(string.Format("User(s) viewing the task: {0}.", userNames.ToUpper()));
                        //ViewMode();
                    }
                }
                #endregion

                string sourceFrom = Convert.ToString(dt.Rows[0]["sourceFrom"]);
                hdnTaskType.Value = Convert.ToString(dt.Rows[0]["TaskType"]);
                hdnTaskType.Value = hdnTaskType.Value.Replace(" ", "");
                if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeNewActivationOrder.ToLower()
                    || hdnTaskType.Value.ToLower() == "clientrequest"
                    || (hdnTaskType.Value.ToLower() == "clientrequest" && sourceFrom == AppConstants.AMAZONDSP))
                {
                    orderNewActivation.Visible = true;
                }
                if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeUpgradeOrder.ToLower())
                {
                    orderUpgradeOrder.Visible = true;
                }
                if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeGPSOrder.ToLower())
                {
                    orderGPS.Visible = true;
                    btnCloseAndAddToManageLines.Text = "Close and Add to Manage Features";
                }
                string orderTypeSub = Convert.ToString(dt.Rows[0]["OrderTypeSub"]);
                string orderStatusSub = Convert.ToString(dt.Rows[0]["orderStatusSub"]);

                hdnOrderTypeSub.Value = CommonMethods.GetOrderSubType(false, orderTypeSub);
                bool isSalesUser = CommonMethods.CheckIfSalesUser();
                bool isAdmin = Session[AppConstants.SessionIsAdmin].GetStringFromObject() == "1" ? true : false;

                if (hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderWSFF)
                {
                    pnlManageLines.Visible = false;
                    btnCloseAndAddToManageLines.Visible = false;
                    btnSaveClose.Visible = false;
                    btnClose.Style.Add("float", "right");
                    btnSaveActivation.Visible = false;

                    if (!String.IsNullOrEmpty(orderTypeSub) && (orderStatusSub == AppConstants.SAVEANDCLOSE || orderStatusSub == AppConstants.SAVEONLY
                        || orderStatusSub == AppConstants.SUBMITTED || orderStatusSub == AppConstants.SUBMITORDER))
                    {
                        if (isSalesUser)
                        {
                            pnlManageLines.Visible = false;
                            btnCloseAndAddToManageLines.Visible = false;
                            btnSaveClose.Visible = false;
                            btnClose.Style.Add("float", "right");
                            btnSaveActivation.Visible = true;
                        }
                    }
                    if (!String.IsNullOrEmpty(orderTypeSub)
                        && orderStatusSub == AppConstants.SUBMITACTIVATION)
                    {
                        //if (Session[AppConstants.SessionIsAdmin].GetStringFromObject() == "1")
                        {
                            if (string.IsNullOrEmpty(deviceLeaseId))
                            {
                                divleaseterm.Visible = true;
                            }
                            pnlManageLines.Visible = true;
                            btnCloseAndAddToManageLines.Visible = true;
                            btnSaveClose.Visible = true;
                            btnClose.Style.Add("float", "right");
                            btnSaveActivation.Visible = false;
                        }
                        //else
                        //{
                        //btnCloseAndAddToManageLines.Visible = false;
                        //btnSaveClose.Visible = false;
                        //btnClose.Style.Add("float", "right");
                        //btnSaveActivation.Visible = false;
                        //                    }
                    }

                    // User does not have Manage Orders Admin permission.
                    if (orderStatusSub == AppConstants.SUBMITORDER && !isSalesUser)
                    {
                        pnlManageLines.Visible = false;
                        btnCloseAndAddToManageLines.Visible = false;
                        btnSaveClose.Visible = false;
                        btnClose.Visible = true;
                        btnClose.Style.Add("float", "right");
                        btnSaveActivation.Visible = false;
                    }
                }
                if (ddlStatus.SelectedItem.Text == AppConstants.CLOSED)
                {
                    LoadStatusClosed();
                }
            }
        }
        if (type == AppConstants.DEFAULT || type == AppConstants.EMPTY)
        {
            DataTable dtSign = ds.Tables.Count > 1 ? ds.Tables[1] : new DataTable();
            if (dtSign != null && dtSign.Rows.Count > 0)
            {
                divSignature.InnerHtml = Convert.ToString(dtSign.Rows[0]["EmailContents"]).Replace("%Name%", adminUserLogin.UserID.ToUpper());
            }
        }
    }

    private int GetTaskIdFromQueryString()
    {
        int AdminTaskId = 0;
        if (Request.QueryString["id"] != null)
        {
            Int32.TryParse(Convert.ToString(Request.QueryString["id"]).Replace("%27", ""), out AdminTaskId);
        }
        return AdminTaskId;
    }

    private void SendResponse(string actionType, string buttonCall)
    {
        // Save admin task details before sending email.
        if (ValidateAndSave(actionType, false) == 1)
        {
            DataTable dt = new DataTable();
            string clientEmail = "";
            if (objManageMasters == null)
                objManageMasters = new ManageMasters();

            objManageMasters.ClientId = hdnClientID.Value == "" ? 0 : Convert.ToInt32(hdnClientID.Value);
            objManageMasters.AdminTaskId = hdnAdminTaskId.Value == "" ? 0 : Convert.ToInt32(hdnAdminTaskId.Value);
            if (objManageMasters.ClientId > 0)
            {
                //dt = objManageMasters.BusinessClient_GetById();
                dt = objManageMasters.GetContactInfoByAssociatedUser();
            }

            if (dt.Rows.Count > 0 || objManageMasters.ClientId == 0)
            {
                if (actionType == AppConstants.TASKORDRESPONSE)
                {
                    clientEmail = GetOrdersAdmin("ORDERDESK");
                }
                else
                    clientEmail = GetEmailAddress(dt, clientEmail);
                if (!string.IsNullOrEmpty(clientEmail.Replace(";", "")))
                {
                    {
                        SendEmail objEmail = new SendEmail();

                        objEmail.AttachmentList = new List<string>();
                        for (int k = 0; k < grdPhoneDetails.Rows.Count; k++)
                        {
                            GridViewRow row = grdPhoneDetails.Rows[k];

                            CheckBox chkAttachment = row.FindControl("chkAttachment") as CheckBox;
                            if (chkAttachment.Checked)
                            {
                                HtmlAnchor documentPath = row.FindControl("aDocumentPath") as HtmlAnchor;
                                Literal ltrLocalPath = row.FindControl("ltrLocalPath") as Literal;
                                objEmail.AttachmentList.Add(ltrLocalPath.Text + "|" + documentPath.InnerText);
                            }
                        }
                        if (actionType != AppConstants.TASKCOMPRESPONSE)
                        {
                            Dictionary<string, string> objEmailTokens = new Dictionary<string, string>();
                            string contactName = "";

                            AdminTasks objTasks = new AdminTasks();

                            objTasks.SPType = AppConstants.OPEN;
                            objTasks.AdminTaskId = AppHelper.GetTaskIdFromQueryString("id", this.Context);
                            objTasks.TaskHeading = "";
                            objTasks.TaskStatus = 0;
                            objTasks.TaskAssignedRep = Convert.ToInt32(ddlRep.SelectedValue);
                            DataSet ds = objTasks.GetAllTasks();
                            DataTable dtTask = ds.Tables.Count > 0 ? ds.Tables[0] : null;

                            if (dtTask != null)
                            {
                                string lastEmailFrom = dtTask.Rows.Count > 0 ? dtTask.Rows[0]["LastEmailFrom"].GetStringFromObject() : "";
                                if (actionType == AppConstants.TASKORDRESPONSE)
                                {
                                    string[] eArr = clientEmail.Split(new char[] { ',', ';' });
                                    foreach (string s in eArr)
                                        contactName += s.Split('@')[0] + ", ";
                                }
                                else
                                    contactName = GetContactName(dt, contactName, lastEmailFrom);


                                contactName = contactName.Trim().TrimEnd(',');
                                if (txtTaskHeading.Text.ToLower().Contains(AppConstants.TLUpgradeActivationOrder.ToLower()) && buttonCall == btnSendMailToOrderDesk.Text)
                                {
                                    orderUpgradeOrder.EmailProcess(clientEmail, contactName);
                                }
                                else
                                {
                                    CreateEmailTokens(objEmailTokens, contactName, dtTask);

                                    try
                                    {
                                        objEmail.PrimaryId = hdnOrderId.Value;
                                        objEmail.ModuleName = AppConstants.ORDER;
                                        bool flag = objEmail.SendUserEmail(clientEmail, objEmailTokens, actionType, AppConstants.SUPPORT, true);

                                        if (flag)
                                        {
                                            SetMessage("Email to " + (actionType == AppConstants.TASKORDRESPONSE ? contactName : "Customer ") + " has been Sent!!");
                                            txtResponseToOrderDesk.Text = "";
                                        }
                                        else
                                        {
                                            SetMessage("Email to " + (actionType == AppConstants.TASKORDRESPONSE ? contactName : "Customer ") + " was Not Sent");
                                            //txtResponseToOrderDesk.Text = "";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        SetMessage("Exception = " + ex.Message);
                                        ScriptManager.RegisterStartupScript(this, this.GetType(), "", "javascript:alert('Exception'" + ex.Message + ")", true);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "", "javascript:alert('Email is not available. Unable to send email.')", true);
                }

            }
        }
        else
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "", "javascript:alert('Email not Sent.')", true);
        }

    }

    private void CreateEmailTokens(Dictionary<string, string> objEmailTokens, string contactName, DataTable dtTask)
    {
        objEmailTokens.Add("%Name%", contactName);
        objEmailTokens.Add("%RequestID%", lblRequestID.Text);
        objEmailTokens.Add("%TaskHeading%", txtTaskHeading.Text);
        objEmailTokens.Add("%Subject%", txtTaskHeading.Text);
        if (txtResponseToOrderDesk.Text != "")
            objEmailTokens.Add("%TaskResponse%", txtResponseToOrderDesk.Text.Replace("\n", "<br>").Trim());
        else
            objEmailTokens.Add("%TaskResponse%", string.Empty);

        string taskDetails = (dtTask != null && dtTask.Rows.Count > 0) ? Convert.ToString(dtTask.Rows[0]["HistoryTaskDetails"]).Replace("\n", "<br>") + "<br><br><br>" : "";
        string FirstEmailAddress = (dtTask != null && dtTask.Rows.Count > 0) ? Convert.ToString(dtTask.Rows[0]["FirstEmailAddress"]) : "";

        objEmailTokens.Add("%TaskDetail%", taskDetails.Trim());
        objEmailTokens.Add("%SaleRep%", divSignature.InnerHtml);
        objEmailTokens.Add("%TaskTime%", Convert.ToDateTime(lblTimeNow.Text).ToShortTimeString());
        objEmailTokens.Add("%TaskDate%", Convert.ToDateTime(lblTimeNow.Text).ToShortDateString());
        objEmailTokens.Add("%EmailDay%", System.DateTime.Now.ToString("dddd"));
        objEmailTokens.Add("%FromEmail%", FirstEmailAddress);
    }

    private string GetEmailAddress(DataTable dt, string clientEmail)
    {
        if (hdnLastEmailFrom.Value != "")
        {
            clientEmail = hdnLastEmailFrom.Value;
        }
        else
        {
            if (txtEmailAddress.Text.Trim() != "")
            {
                clientEmail = txtEmailAddress.Text.Trim();
            }
        }

        if (chkCopyContact.Checked)
        {
            if (txtEmailAddress.Text.Trim() != "")
            {
                clientEmail = txtEmailAddress.Text.Trim();
            }

            if (objManageMasters.ClientId != 0)
            {

                /*
                if (hdnLastEmailFrom.Value != Convert.ToString(dt.Rows[0]["Email1"]))
                    clientEmail += ";" + Convert.ToString(dt.Rows[0]["Email1"]);

                if (hdnLastEmailFrom.Value != Convert.ToString(dt.Rows[0]["Email2"]))
                    clientEmail += ";" + Convert.ToString(dt.Rows[0]["Email2"]);
                */
                clientEmail += AppHelper.GetCSVFromDT(dt, "EmailAddress");
            }

        }

        if (txtContact.Text != "" && AppHelper.IsValidEmail(txtContact.Text.Trim()))
        {
            clientEmail += ";" + txtContact.Text.Trim();
        }

        clientEmail += ";" + hdnOtherEmail.Value;

        List<string> listEmail = clientEmail.Split(new char[] { ';', ',' }).ToList();
        clientEmail = string.Join(";", listEmail.Distinct().ToArray());
        return clientEmail;
    }

    private string GetContactName(DataTable dt, string contactName, string clientEmail)
    {
        if (dt.Rows.Count > 0 && objManageMasters.ClientId != 0)
        {
            if (clientEmail == Convert.ToString(dt.Rows[0]["Email1"]))
            {
                contactName = Convert.ToString(dt.Rows[0]["Contact1"]);
            }
            else if (clientEmail == Convert.ToString(dt.Rows[0]["Email2"]))
            {
                contactName = Convert.ToString(dt.Rows[0]["Contact2"]);
            }

            /* Logic to get from Associated Users */
            var contactInfo = dt.AsEnumerable().Where(d => d["EmailAddress"].GetStringFromObject() == clientEmail).FirstOrDefault();
            if (contactInfo != null)
            {
                contactName = (contactInfo["FirstName"].GetStringFromObject().CamelCasing()).Trim();
            }
        }

        if (contactName == "" && txtContact.Text.Trim() != "")
        {
            contactName = txtContact.Text.Trim();
        }

        contactName = !contactName.StringIsNullOrEmpty() ? contactName.Split('@')[0] : "";
        return contactName;
    }

    #endregion

    #region Default Values
    private void LoadDefaultValues()
    {
        lblTimeNow.Text = DateTime.Now.ToString();

        objTasks = new AdminTasks();
        lblRequestID.Text = objTasks.GetNextTaskRequestID();
        BindDropDownSalesRep();
        BindDropDownStatus();
        CommonMethods.GetDeviceLeaseTerm(ddldivLeaseTerm, objManageMasters);
        if (Request.QueryString["id"] != null)
        {
            LoadTaskValues("");
            LoadAttachments(Conversion.ParseInt(Request.QueryString["id"]));
        }
        else
        {
            LoadTaskValues(AppConstants.DEFAULT);
        }
        if (Request.QueryString["tid"] != null)
        {
            if (objManageMasters == null)
                objManageMasters = new ManageMasters();

            objManageMasters.ResponseTemplateId = AppHelper.GetTaskIdFromQueryString("tid", this.Context);
            DataTable dt = objManageMasters.GetResponseTemplates();
            if (dt.Rows.Count > 0)
            {
                txtResponseToOrderDesk.Text = "";// Convert.ToString(dt.Rows[0]["ResponseContent"]);
            }
        }
        LoadAttachments(0);
        if (Request.QueryString["res"] != null && Convert.ToString(Request.QueryString["res"]) == "1")
        {
            SetMessage("Successfully saved.");
        }
    }

    private void BindDropDownSalesRep()
    {
        DataTable dt = new DataTable();
        if (objManageMasters == null)
            objManageMasters = new ManageMasters();

        dt = objManageMasters.Bussiness_GetSalesRepDropdown();
        ddlRep.DataSource = dt;
        ddlRep.DataTextField = "Name";
        ddlRep.DataValueField = "SaleRepId";
        ddlRep.DataBind();
        ddlRep.Items.Insert(0, new ListItem("-Select-", "0"));

        if (ddlRep.Items.Count > 0)
        {
            ddlRep.SelectedValue = Convert.ToString(Session[AppConstants.SessionSaleRepID]);
        }
    }

    private void BindDropDownStatus(string showFor = "")
    {
        DataTable dt = new DataTable();
        objManageMasters.ShowFor = showFor == "" ? AppConstants.ADMINTASK : showFor;
        dt = objManageMasters.Status_GetTaskStatus();
        ddlStatus.DataSource = dt;
        ddlStatus.DataTextField = "StatusName";
        ddlStatus.DataValueField = "TaskStatusId";
        ddlStatus.DataBind();
        ddlStatus.Items.Insert(0, new ListItem("-Select-", "0"));
    }

    #endregion

    #region grdAttachments Events

    /// <summary>
    /// For Grid Commands Edit and delete phone data
    /// </summary>
    protected void grdPhoneDetails_RowCommand(object sender, GridViewCommandEventArgs e)
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

            DeleteMethod(adminTaskAttachmentId);
        }
        LoadAttachments(0);
    }

    private void DeleteMethod(int adminTaskAttachmentId)
    {
        objTasks.SPType = AppConstants.DELETE;
        objTasks.AdminTaskAttachId = adminTaskAttachmentId;
        objTasks.TaskCreatedBy = Convert.ToInt32(Session[AppConstants.SessionAdminUserId]);
        objTasks.CreatedDate = DateTime.Now;
        objTasks.DeletedDate = DateTime.Now;
        objTasks.SaveAdminTaskAttachment();
    }

    /// <summary>
    /// Phone Details
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void grdPhoneDetails_RowDataBound(object sender, GridViewRowEventArgs e)
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
            Paging1.PageSize = grdPhoneDetails.PageSize;
            try
            {
                //Paging1.TotalRecord = Convert.ToInt32(hidTotalRecordsPhone.Value);
            }
            catch
            {
                Paging1.TotalRecord = 1;
            }
            Paging1.CurrentPage = grdPhoneDetails.PageIndex + 1;
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
    protected void grdPhoneDetails_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }
    protected void grdPhoneDetails_OnRowUpdating(object sender, GridViewUpdateEventArgs e)
    {

    }

    protected void grdPhoneDetails_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        grdPhoneDetails.PageIndex = e.NewPageIndex;
        //LoadProposalAttachments(ProposalSaveType());
    }

    private void LoadAttachments(long adminTaskId)
    {
        adminTaskId = adminTaskId == 0 ? GetTaskIdFromQueryString() : adminTaskId;
        objTasks = new AdminTasks();
        objTasks.AdminTaskId = adminTaskId;

        grdPhoneDetails.DataSource = objTasks.LoadAdminTaskAttachment();
        grdPhoneDetails.DataBind();

    }

    #endregion

    protected void Paging_Click(object sender, CommandEventArgs e)
    {

    }

    #region Permission And Information
    public void BindPermission()
    {
        System.Collections.Generic.Dictionary<string, string> dictPerm = new Dictionary<string, string>();
        if (HttpContext.Current.Session[AppConstants.SessionAdminEmailAddress] != null && HttpContext.Current.Session[AppConstants.SessionAdminEmailAddress].ToString() != "")
        {
            if (HttpContext.Current.Session[AppConstants.SessionAdminEmailAddress].ToString() != AppConstants.AdminEmailAddress)
            {
                int ParentID = AppHelper.GetPageId(AppConstants.TLTaskManager);
                dictPerm = AppHelper.BindPermission(ParentID);
                if (dictPerm != null && dictPerm.Count > 0)
                {
                    hdnAddEditPermission.Value = dictPerm[AppConstants.AddEditPermission];
                    hdnDeletePermission.Value = dictPerm[AppConstants.DeletePermission];
                    hdnViewPermission.Value = dictPerm[AppConstants.ViewPermission];
                    hdnAssignPermission.Value = dictPerm[AppConstants.AdditionalPermissions].Contains(AppConstants.AssignRep) ? "1" : "0";

                    ddlRep.Enabled = false;
                    if (hdnAssignPermission.Value == "1" || hdnAdminTaskId.Value == "" || hdnAdminTaskId.Value == "0")
                    {
                        ddlRep.Enabled = true;
                    }
                    if (hdnViewPermission.Value == "1" && hdnAddEditPermission.Value == "0")
                    {
                        ViewMode();
                    }
                    if (hdnAddEditPermission.Value == "0")
                    {
                        btnAttachFile.Visible = false;
                        btnSave.Visible = false;
                        btnSaveClose.Visible = false;
                        btnResponse.Visible = false;
                        btnResponseCompleted.Visible = false;
                    }
                }
                else
                {
                    Response.Redirect(ConfigurationManager.AppSettings["BaseUrl"].ToString() + "Admin/AdminHome.aspx?" + "&b=" + adminDashTabNo);
                }
            }
        }
    }

    private void ViewMode()
    {
        txtCompanyName.ReadOnly = true;
        ddlRep.Enabled = false;
        txtContact.ReadOnly = true;
        ddlStatus.Enabled = false;
        txtTaskHeading.ReadOnly = true;
        //txtOrderInternalNotes.ReadOnly = true;
        //txtResponseToOrderDesk.ReadOnly = true;

        btnAttachFile.Enabled = false;
        btnResponseCompleted.Enabled = false;
        btnResponse.Enabled = false;

        btnSave.Enabled = false;
        btnSaveClose.Enabled = false;

    }

    private void SetMessage(string message)
    {
        //Response.Write(message);
        lblMsg.Text += Common.DisplayMessage(message, Common.Enum_MessageType.ConfirmMessage, 5, Common.Enum_TableStyle.None);
        lblMsgDown.Text = Common.DisplayMessage(message, Common.Enum_MessageType.ConfirmMessage, 5, Common.Enum_TableStyle.None);
        ShowAlert(message);
    }

    private void SetMessageBig(string message)
    {
        lblMsg.Text = Common.DisplayMessageBig(message, Common.Enum_MessageType.ConfirmMessage, 5, Common.Enum_TableStyle.None);
        lblMsgDown.Text = Common.DisplayMessageBig(message, Common.Enum_MessageType.ConfirmMessage, 5, Common.Enum_TableStyle.None);
        ShowAlert(message);
    }

    #endregion

    protected void btnCloseAndAddToManageLines_Click(object sender, EventArgs e)
    {
        CloseAndAddToManageLines();
    }

    private void CloseAndAddToManageLines()
    {
        string result = "";
        lblMsg.Text = result;
        lblMsgDown.Text = result;
        try
        {

            if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeNewActivationOrder.ToLower())
                result = orderNewActivation.SaveToManageLinesV1(true, AppConstants.CREATEMANAGELINES, btnCloseAndAddToManageLines);
                //result = orderNewActivation.SaveToManageLines(true, AppConstants.CREATEMANAGELINES, btnCloseAndAddToManageLines);
            if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeUpgradeOrder.ToLower())
                result = orderUpgradeOrder.SaveToManageLines(true, AppConstants.CREATEMANAGELINES);
            if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeGPSOrder.ToLower())
                result = orderGPS.SaveToManageFeatures();

            if (result.ToLower().StartsWith("saved!")) //&& orderNewActivation.GetInputValue("ddlShippingOptionLabelValue") != ""
            {
                hdnMsg.Value = result;
                modalConfirm.Show();
            }
            else
            {
                ClosedAndRedirection(result);
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void ClosedAndRedirection(string result)
    {
        if (!string.IsNullOrEmpty(result))
        {
            SetMessage(result);
            if (result.ToLower().StartsWith("saved!"))
            {
                SaveTaskStatus();
                LoadStatusClosed();
                hdnMsg.Value = "";
                Response.Redirect(Common.GetBaseURL
                    + "/Admin/AdminDashOrderTasks.aspx?ClientId=" + AppHelper.GetValueFromQueryString("ClientId", this.Context)
                    + "&OrderId=" + AppHelper.GetValueFromQueryString("OrderId", this.Context)
                    + "&id=" + AppHelper.GetValueFromQueryString("id", this.Context)
                    + "&s=s"
                    + "&pg=" + AppHelper.GetValueFromQueryString("pg", this.Context)
                    + "&sot=" + AppHelper.GetValueFromQueryString("sot", this.Context)
                    + "&b=" + adminDashTabNo);
            }
        }
    }

    private void SaveTaskStatus()
    {
        AdminTasks objtasks = new AdminTasks();
        objtasks.AdminTaskId = Convert.ToInt32(AppHelper.GetValueFromQueryString(AdminTaskId, this.Context));
        objtasks.SPType = AppConstants.CLOSED;
        objtasks.SaveAdminTaskStatus();
    }

    private void LoadStatusClosed()
    {
        ddlStatus.Enabled = false;
        ddlStatus.SelectedItem.Text = AppConstants.CLOSED;
        pnlManageLines.Visible = false;
        btnCloseAndAddToManageLines.Visible = false;
        btnSaveClose.Visible = true;
        //btnEmail.Visible = true;
        btnClose.Style.Add("float", "right");
        pnlInvoice.Visible = true;
    }

    protected void btnSendOrderConfToClient_Click(object sender, EventArgs e)
    {   //EmailProcessForOrder(AppConstants.TitleORDERNEWACTIVATIONORDER, AppConstants.TitleORDERNEWACTIVATIONORDERLINEITEM, "");
        string msg = "";
        Button button = (Button)btnSendnotf;
        var res = orderNewActivation.ValidateShippingTracking(msg);
        if (string.IsNullOrEmpty(res))
            res = orderNewActivation.ValidateShippingDetails();
        if (string.IsNullOrEmpty(res))
        {
            SendOrderNotf(button.ID);

            if (button.ID == "btnSendnotf")
            {
                ClosedAndRedirection(hdnMsg.Value);
            }
        }
        else
            SetMessage(res);

    }

    protected void SendOrderNotf(string buttonId = "")
    {
        bool flag = EmailProcessForOrder(AppConstants.TitleORDERNEWACTIVATIONORDEREMAIL, AppConstants.TitleORDERNEWACTIVATIONORDERLINEITEM, btnSendOrderConfToClient.Text);
        // Update Send Order Conf to Client date
        if (flag)
        {
            TextBox txtShipTrackno = orderNewActivation.FindControl("txtShippingTrackingNumber") as TextBox;
            var txtShipTracknoValue = "";
            if (txtShipTrackno != null)
                txtShipTracknoValue = txtShipTrackno.Text.Trim();

            UpdateOrderSpecificInfo("Send Order Conf To Client", txtShipTracknoValue);
        }
        if (buttonId != "btnSendnotf")
        {
            var nvc = HttpUtility.ParseQueryString(Request.Url.Query);
            nvc.Remove("scm");
            nvc.Remove("con");
            nvc.Remove("notfcli");
            string url = Request.Url.AbsolutePath + "?" + nvc.ToString();
            Response.Redirect(url + "&con=" + flag);
        }
    }

    private bool EmailProcessForOrder(string emailTitle, string emailSubTitle, string buttonCall = "")
    {
        bool flag = false;
        SendEmail objEmail = new SendEmail();
        int clientId = Convert.ToInt32(AppHelper.GetValueFromQueryString("ClientId", this.Context));
        if (clientId > 0)
        {
            DataTable dt = new DataTable();
            string clientEmail = "", contactName = "";
            if (hdnOrderStatusSub.Value != AppConstants.SUBMITACTIVATION || buttonCall == "Send Order Conf To Client")
            {
                dt = GetClientContent(dt);
                if (dt != null)
                {
                    /*
                    if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["Email1"])))
                        clientEmail += ";" + Convert.ToString(dt.Rows[0]["Email1"]);

                    if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["Email2"])))
                        clientEmail += ";" + Convert.ToString(dt.Rows[0]["Email2"]);
                    */

                    clientEmail += AppHelper.GetCSVFromDT(dt, "EmailAddress");

                    /*
                    if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["Contact1"])))
                        contactName += Convert.ToString(dt.Rows[0]["Contact1"]);

                    if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["Contact2"])))
                        contactName += "/ " + Convert.ToString(dt.Rows[0]["Contact2"]);
                    */
                    contactName = AppHelper.GetCSVFromDT(dt, "FirstName", "/");
                    //contactName = "All";

                    contactName = contactName.Trim('/');
                }
            }
            else
            {
                clientEmail = GetOrdersAdmin("ORDERDESK");
            }


            if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeNewActivationOrder.ToLower())
            {
                flag = orderNewActivation.EmailProcess(clientEmail, contactName, emailTitle, emailSubTitle, buttonCall);
            }
            if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeUpgradeOrder.ToLower())
            {
                flag = orderUpgradeOrder.EmailProcess(clientEmail, contactName, buttonCall);
            }
            if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeGPSOrder.ToLower())
            {
                flag = orderGPS.EmailProcess(clientEmail, contactName);
            }

            if (flag)
            {
                lblMsg.Text = "Email sent to Client";
                SetMessage("Email sent to Client");
            }
            else
            {
                lblMsg.Text = "Email NOT sent to Client";
                SetMessage("Email NOT sent to Client");
            }
            //Response.Write("flag " + flag);
        }
        return flag;
    }

    protected void btnSaveActivation_Click(object sender, EventArgs e)
    {
        try
        {
            UpdateOrderSpecificInfo(AppConstants.UPDATE, AppConstants.SUBMITACTIVATION);
            hdnOrderStatusSub.Value = AppConstants.SUBMITACTIVATION;
            string validationMessage = orderNewActivation.SaveToManageLines(true, AppConstants.SUBMITACTIVATION);
            //if (string.IsNullOrEmpty(validationMessage.Trim()))
            {
                EmailProcessForOrder(AppConstants.TitleORDERNEWACTIVATIONORDERWSFF, AppConstants.TitleORDERNEWACTIVATIONORDERLINEITEM, AppConstants.SUBMITACTIVATION);

                Response.Redirect(Common.GetBaseURL
                        + "/Admin/AdminDashOrderTasks.aspx?ClientId=" + AppHelper.GetValueFromQueryString("ClientId", this.Context)
                        + "&OrderId=" + AppHelper.GetValueFromQueryString("OrderId", this.Context)
                        + "&id=" + AppHelper.GetValueFromQueryString("id", this.Context)
                        + "&pg=" + AppHelper.GetValueFromQueryString("pg", this.Context)
                        + "&sot=" + AppHelper.GetValueFromQueryString("sot", this.Context)
                        + "&scm=1"
                        + "&b=" + adminDashTabNo);
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void UpdateOrderSpecificInfo(string SPType, string statusSub)
    {
        AdminOrders objAdminOrder = new AdminOrders();
        objAdminOrder.SPType = SPType;
        objAdminOrder.OrderId = Conversion.ParseInt(hdnOrderId.Value);
        objAdminOrder.AdminTaskId = Conversion.ParseInt(hdnAdminTaskId.Value);
        objAdminOrder.OrderStatusSub = statusSub;
        objAdminOrder.UpdateOrder();
    }

    protected void btnEmail_Click(object sender, EventArgs e)
    {
        try
        {
            EmailProcessForOrder(AppConstants.TitleORDERNEWACTIVATIONORDEREMAIL, AppConstants.TitleORDERNEWACTIVATIONORDERLINEITEM);
        }
        catch (Exception ex)
        {

        }
    }

    protected void btnOrderResponse_Click(object sender, EventArgs e)
    {
        try
        {
            SendResponse(AppConstants.TASKORDRESPONSE, btnOrderResponse.Text);

            //EmailProcessForUpgrade();
        }
        catch (Exception ex)
        {

        }
    }

    private string GetOrdersAdmin(string emailType = "")
    {
        string email = "";

        DataTable dt = CommonMethods.GetMasterAdmin();
        if (dt != null && dt.Rows.Count > 0)
        {
            string oT = "";
            if (hdnTaskType.Value == AppConstants.TLNewActivationOrder || hdnTaskType.Value == AppConstants.OrderTypeNewActivationOrder)
            {
                oT = AppConstants.TLNewActivationOrder;
            }
            if (hdnTaskType.Value == AppConstants.TLUpgradeActivationOrder || hdnTaskType.Value == AppConstants.OrderTypeUpgradeOrder)
            {
                oT = AppConstants.TLUpgradeActivationOrder;
            }
            if (hdnTaskType.Value == AppConstants.TLGPSOrder || hdnTaskType.Value == AppConstants.OrderTypeGPSOrder || hdnTaskType.Value == AppConstants.OrderTypeGPS)
            {
                oT = AppConstants.OrderTypeGPS;
            }
            string query = "OrderType= '" + (oT) + "' And OrderSubType = '" + hdnOrderTypeSub.Value + "' And CarrierId = '" + hdnCarrierId.Value + "'";
            DataRow[] drList = dt.Select(query);
            if (drList != null && drList.Length > 0)
            {
                if (emailType == "ORDERDESK")
                    email = Conversion.ParseString(drList[0]["OrderDeskEmailAddress"]);
                else
                    email = Conversion.ParseString(drList[0]["EmailAddress"]);
            }
            //TLNewActivationOrder
        }
        return email;
    }

    protected void btnConvertToDeviceLease_Click(object sender, EventArgs e)
    {
        if (ddldivLeaseTerm.SelectedValue == "")
        {
            string retMsg = "Please select Device Lease Term.";
            SetMessage(retMsg);
        }
        else
        {
            ConvertToDeviceLease();
        }
    }

    private void ConvertToDeviceLease()
    {
        string retMsg = orderNewActivation.SaveDeviceLease(AppConstants.SAVEDEVICELEASE);
        if (string.IsNullOrEmpty(retMsg))
        {
            retMsg = "Order converted to device lease successfully.";
            divleaseterm.Visible = false;
        }

        SetMessageBig(retMsg);
        SetMessageBig(retMsg);
    }

    protected void btnSendMailToOrderDesk_Click(object sender, EventArgs e)
    {
        lblMsg.Text = "";
        lblMsgDown.Text = "";
        try
        {
            var sub = CommonMethods.GetOrderSubType(true);

            string emailTitle = "";
            if (sub == AppConstants.TLNewActivationOrderCFF)
            {
                emailTitle = AppConstants.TitleORDERNEWACTIVATIONORDERCFF;
            }
            if (sub == AppConstants.TLNewActivationOrderWSFF)
            {
                emailTitle = AppConstants.TitleORDERNEWACTIVATIONORDERWSFF;
            }
            if (sub == AppConstants.TLNewActivationOrderCOD)
            {
                emailTitle = AppConstants.TitleORDERNEWACTIVATIONORDERCOD;
            }
            //if (sub == AppConstants.TLUpgradeActivationOrderUG)
            if (txtTaskHeading.Text.ToLower().Contains(AppConstants.TLUpgradeActivationOrder.ToLower()))
            {
                emailTitle = AppConstants.OTNewActivationOrder;
            }
            string emailAddress = "";
            try
            {
                //if (txtTaskHeading.Text.ToLower().Contains(AppConstants.TLUpgradeActivationOrder.ToLower()))
                //{
                //    emailAddress = CommonMethods.GetMasterAdminEmailAddress(AppConstants.TLUpgradeActivationOrder, hdnCarrierId.Value, "");
                //}
                //else
                //    emailAddress = CommonMethods.GetMasterAdminEmailAddress(hdnOrderTypeSub.Value, hdnCarrierId.Value,
                //        ((sub == AppConstants.TLNewActivationOrderCFF || hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderCOD
                //        || hdnOrderTypeSub.Value == AppConstants.TLNewActivationOrderWSFF) ? "" : "EmailAddress"));
            }
            catch (Exception ex)
            {
            }

            bool flag = false;
            if (txtTaskHeading.Text.ToLower().Contains(AppConstants.TLUpgradeActivationOrder.ToLower()))
            {
                //flag = orderUpgradeOrder.EmailProcess(emailAddress, "Order Desk");
                flag = orderUpgradeOrder.EmailProcessForUpgrade();
            }
            else
                flag = orderNewActivation.EmailProcess(emailAddress, "Order Desk", emailTitle, AppConstants.TitleORDERNEWACTIVATIONORDERLINEITEM);


            if (flag)
            {
                SetMessage("Email to " + emailAddress + " has been Sent!!");
            }
            else
            {
                SetMessage("Email to " + emailAddress + " Not Sent!!");
            }

            var nvc = HttpUtility.ParseQueryString(Request.Url.Query);
            nvc.Remove("scm");
            nvc.Remove("con");
            string url = Request.Url.AbsolutePath + "?" + nvc.ToString();
            Response.Redirect(url + "&con=" + (flag ? "res" : "nores"));
        }
        catch (Exception ex)
        {

        }
    }

    protected void btnDeleteOrder_Click(object sender, EventArgs e)
    {
        Int64 AdminTaskId = Conversion.ParseInt(AppHelper.GetValueFromQueryString("id", this.Context));
        if (AdminTaskId > 0)
        {
            DeleteTask(AdminTaskId);
        }
    }

    /// <summary>
    /// To Delete Task Information
    /// </summary>      
    private void DeleteTask(Int64 TaskID)
    {
        objTasks.AdminTaskId = TaskID;
        objTasks.TaskCreatedBy = Conversion.ParseInt(Session[AppConstants.SessionAdminUserId]);
        int result = objTasks.DeleteAdminTask();
        if (result > 0)
        {
            Response.Redirect("~/Admin/AdminDash.aspx?s=1" + "&b=" + adminDashTabNo);
        }
        else
        { lblMsg.Text = Common.DisplayMessage("Deletion failed.", Common.Enum_MessageType.ErrorMessage, 5, Common.Enum_TableStyle.None); }
    }

    protected void btnSaveOnly_Click(object sender, EventArgs e)
    {
        try
        {
            if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeNewActivationOrder.ToLower())
                orderNewActivation.SaveOnly();
            if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeUpgradeOrder.ToLower())
                orderUpgradeOrder.SaveOnly();
            if (hdnTaskType.Value.ToLower() == AppConstants.OrderTypeGPSOrder.ToLower())
                orderGPS.SaveOnly();

            ValidateAndSave(AppConstants.SAVEONLY, false);
        }
        catch (Exception ex)
        {

        }
    }

    protected void grdActivityLog_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {

    }

    private void GetActivityLog()
    {
        try
        {
            long adminTaskId = GetTaskIdFromQueryString();
            ActivityModel activityModel = new ActivityModel()
            {
                PrimaryKey = adminTaskId,
                Module = "Task Manager"
            };
            DataSet ds = AppHelper.GetActivityAsync(activityModel).Result;
            if (ds != null && ds.Tables.Count > 0)
            {
                grdActivityLog.DataSource = ds.Tables[0];
                grdActivityLog.DataBind();
            }
        }
        catch (Exception ex)
        {
        }
    }

    #region Print Package Slip

    protected void btnPrint_Click(object sender, EventArgs e)
    {
        PrintPackagingSlip(sender, e);
    }

    private void PrintPackagingSlip(object sender, EventArgs e)
    {
        Button button  = (Button)sender;
       
        //SaveActionLogs("Print Packaging Slip printed by #REP#");

        SetPDFFilePath();

        btn_Click(sender, e);
    }

    protected void btn_Click(object sender, EventArgs e)
    {

    }


    private string SetPDFFilePath()
    {
        string path1 = ConvertHtmlToPDF();
        string link = SetFilePath("TEMP") + path1;

        string sitePath = Common.GetBaseURL + "files/" + path1;
        string navigateURL = " <script>window.open('" + sitePath + "','_blank') </script>";

        Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "JSScriptBlock", navigateURL);
        return sitePath;
    }

    private static string SetFilePath(string fileType)
    {
        //D:\Projects\RayRodgers\Dev\14092016\WebSite\ - Server.MapPath
        string fileUpload = System.Web.HttpContext.Current.Server.MapPath(@"\Uploads\");
        string htmlFile = System.Web.HttpContext.Current.Server.MapPath(@"\Admin\Template\");
        string tempFile = System.Web.HttpContext.Current.Server.MapPath(@"\files\");
        string logoPath = System.Web.HttpContext.Current.Server.MapPath(@"\fRONT\Image\");

        if (fileType == "OTP")
        {
            AppHelper.CreateDirectory(fileUpload);
            return fileUpload;
        }
        if (fileType == "HTML")
        {
            AppHelper.CreateDirectory(htmlFile);
            return htmlFile;
        }
        if (fileType == "TEMP")
        {
            AppHelper.CreateDirectory(tempFile);
            return tempFile;
        }
        if (fileType == "LOGO")
        {
            AppHelper.CreateDirectory(logoPath);
            return logoPath;
        }
        return "";
    }

    private string ConvertHtmlToPDF()
    {
        PDFSharp.PdfGenerateConfig config = new PDFSharp.PdfGenerateConfig();
        config.PageSize = PdfSharp.PageSize.A4;
        //config.SetMargins(1);
        config.MarginTop = 5;
        config.MarginRight = 1;
        config.MarginLeft = 1;
        config.MarginBottom = 20;

        var fullHTML = GetHTML();// File.ReadAllLines(SetFilePath("HTML") + @"\test1.html");
        var html = fullHTML;// string.Join("", fullHTML);
        var doc = PDFSharp.PdfGenerator.GeneratePdf(html, config, null, PDFCommon.DemoUtils.OnStylesheetLoad, HtmlRenderingHelper.OnImageLoadPdfSharp);
        var tmpFile = Convert.ToString(Request.QueryString["OrderId"]) + "_" + orderPackageSlipNewActivation;
        tmpFile = SetFilePath("TEMP") + Path.GetFileNameWithoutExtension(tmpFile) + ".pdf";
        doc.Save(tmpFile);
        return Path.GetFileNameWithoutExtension(tmpFile) + ".pdf";
    }

    private string GetHTML()
    {
        var fullHTML = File.ReadAllText(SetFilePath("HTML") + @"\" + orderPackageSlipNewActivation + ".html");

        var logoPath = string.Empty;
        if (Request.Url.ToString().Contains("localhost"))
            logoPath = Server.MapPath(@"\Admin\images\logoprint.png");
        else
            logoPath = @"D:\Plesk\Vhosts\wirelesssupport.com\admin.wirelesssupport.com\Admin\images\logoprint.png";

        string deviceSection = "<table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"1\" style=\"border:solid 1px black\" align=\"center\">";
        deviceSection += "<tr>";
        deviceSection += "<td align=\"left\" style=\"width:50px\">";
        deviceSection += "<span style=\"font: 14px Arial, Helvetica, sans-serif;font-weight:bold;\">S.No</span> ";
        deviceSection += "</td>";
        deviceSection += "<td align=\"left\" style=\"width:450px\">";
        deviceSection += "<span style=\"font: 14px Arial, Helvetica, sans-serif; font-weight: bold;\">Make/ Model</span> ";
        deviceSection += "</td>";
        deviceSection += "<td align=\"left\">";
        deviceSection += "<span style=\"font: 14px Arial, Helvetica, sans-serif; font-weight: bold;\">Quantity</span> ";
        deviceSection += "</td> ";
        deviceSection += "</tr> ";
        deviceSection += "#DEVICEITEMS#";
        deviceSection += "</table> ";

        string itemsSection = "<table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"1\" style=\"border:solid 1px black\" align=\"center\">";
        itemsSection += "<tr>";
        itemsSection += "<td align=\"left\" style=\"width:50px\">";
        itemsSection += "<span style=\"font: 14px Arial, Helvetica, sans-serif;font-weight:bold;\">S.No</span> ";
        itemsSection += "</td>";
        itemsSection += "<td align=\"left\" style=\"width:450px\">";
        itemsSection += "<span style=\"font: 14px Arial, Helvetica, sans-serif; font-weight: bold;\">Accessory Name</span> ";
        itemsSection += "</td>";
        itemsSection += "<td align=\"left\">";
        itemsSection += "<span style=\"font: 14px Arial, Helvetica, sans-serif; font-weight: bold;\">Quantity</span> ";
        itemsSection += "</td> ";
        itemsSection += "</tr> ";
        itemsSection += "#ITEMS#";
        itemsSection += "</table> ";


        fullHTML = fullHTML.Replace("#LOGO#", logoPath);



        fullHTML = fullHTML.Replace("#ORDERNO#", orderNewActivation.GetInputValue("lblOrderNo"));
        fullHTML = fullHTML.Replace("#DATE#", DateTime.Now.ToString("MM/dd/yyyy"));

        fullHTML = fullHTML.Replace("#COMPANYNAME#", txtCompanyName.Text);
        fullHTML = fullHTML.Replace("#WORKPHN#", orderNewActivation.GetInputValue("hdnWorkPhone"));

        string label = orderNewActivation.GetInputValue("grdShippingLabel");
        string ddlShippingOptionLabel = orderNewActivation.GetInputValue("ddlShippingOptionLabel");
        string txtShippingTrackingNumber = orderNewActivation.GetInputValue("txtShippingTrackingNumber");

        if (label != "" && label != "0" && ddlShippingOptionLabel != INPERSONPICKUP)
        {
            fullHTML = fullHTML.Replace("#SHIPPINGTRACKINGTEXT#", "Shipping Tracking #");
            fullHTML = fullHTML.Replace("#SHIPPINGTRACKING#", txtShippingTrackingNumber.Trim());
            fullHTML = fullHTML.Replace("#SHIPPINGMETHOD#", ddlShippingOptionLabel);
        }
        else if (ddlShippingOptionLabel == INPERSONPICKUP)
        {
            fullHTML = fullHTML.Replace("#SHIPPINGTRACKINGTEXT#", "Picked up by");
            fullHTML = fullHTML.Replace("#SHIPPINGTRACKING#", "");
            fullHTML = fullHTML.Replace("#SHIPPINGMETHOD#", INPERSONPICKUP);
        }
        else
        {
            fullHTML = fullHTML.Replace("#SHIPPINGTRACKINGTEXT#", "Shipping Tracking #");
            fullHTML = fullHTML.Replace("#SHIPPINGTRACKING#", string.Empty);
            fullHTML = fullHTML.Replace("#SHIPPINGMETHOD#", string.Empty);
        }

        fullHTML = fullHTML.Replace("#SHIPPINGADDRESS#", orderNewActivation.FormShippingAddress());
        fullHTML = fullHTML.Replace("#ORDERNO#", orderNewActivation.GetInputValue("lblOrderNo"));

        string accessoryString = orderNewActivation.GetAccessoryDataHTML();
        string deviceString = orderNewActivation.GetDeviceDataHTML();

        fullHTML = fullHTML.Replace("#DEVICESECTION#", deviceString != "" ? deviceSection : "");
        fullHTML = fullHTML.Replace("#ITEMSSECTION#", accessoryString != "" ? itemsSection : "");

        fullHTML = fullHTML.Replace("#ITEMS#", accessoryString);
        fullHTML = fullHTML.Replace("#DEVICEITEMS#", deviceString);

        File.WriteAllText(SetFilePath("HTML") + @"\" + orderPackageSlipNewActivation + "_print.html", fullHTML);

        return fullHTML;
    }
    #endregion

    private void SaveActionLogs(string logcontent)
    {
        try
        {
            logcontent = logcontent.Replace("#REP#", Session[AppConstants.SessionUserID].GetStringFromObject());

            objManageMasters = new ManageMasters();
            objManageMasters.SPType = AppConstants.ActionNEWACCESSORYORDER;
            objManageMasters.BodyContent = logcontent;
            objManageMasters.Id = Conversion.ParseInt(hdnOrderId.Value == "" ? "0" : hdnOrderId.Value);
            if (objManageMasters.Id > 0)
            {
                objManageMasters.SaveLogs();
                LoadActivity("", "");
            }

        }
        catch (Exception ex)
        {

        }
    }

    private void LoadActivity(string spType, string id)
    {
        try
        {
            objManageMasters = new ManageMasters();
            objManageMasters.SPType = AppConstants.ActionNEWACCESSORYORDER;
            objManageMasters.Id = hdnOrderId.Value == "" ? 0 : Conversion.ParseInt(hdnOrderId.Value);
            System.Data.DataTable dtActivity = objManageMasters.GetActivityLog().Tables[0];
            if (dtActivity != null && dtActivity.Columns.Count > 0)
            {
                if (dtActivity.Rows.Count > 0)
                {
                    //divActivityLog.Visible = true;
                    //BindGrid(grdActivityLog, dtActivity);
                }
            }
        }
        catch (Exception ex)
        {
        }

    }

    protected void btnSaveInvoice_Click(object sender, EventArgs e)
    {
        try
        {
            Button button = (Button)sender;
            long retAdminTaskId = SaveAdminTask(button.Text, false);

            if (retAdminTaskId > 0)
            {
                SetMessage("Invoice saved!!");
            }
        }
        catch (Exception ex)
        {

            SetMessage(ex.ToString());
        }
    }

}