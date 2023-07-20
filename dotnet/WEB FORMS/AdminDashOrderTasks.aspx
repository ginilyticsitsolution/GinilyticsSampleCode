<%@ Page Title="" Language="C#" MasterPageFile="~/Admin/Master/AdminMaster.master" ValidateRequest="false" Async="true"
    AutoEventWireup="true" CodeFile="AdminDashOrderTasks.aspx.cs" Inherits="AdminDashOrderTasks" MaintainScrollPositionOnPostback="true" %>

<%@ Register Src="~/Admin/UserControls/AccessoryOrder.ascx" TagName="AccessoryOrder" TagPrefix="uc2" %>
<%@ Register Src="~/Admin/UserControls/NewActivationOrder.ascx" TagName="NewActivationOrder" TagPrefix="uc2" %>
<%@ Register Src="~/Admin/UserControls/UpgradeOrder.ascx" TagName="UpgradeOrder" TagPrefix="uc2" %>
<%@ Register Src="~/Admin/UserControls/GPSOrder.ascx" TagName="GPSOrder" TagPrefix="uc2" %>
<%@ Register Src="~/Controls/PagingControl.ascx" TagName="PagingControl" TagPrefix="uc1" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <!-- Latest compiled and minified CSS -->
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" integrity="sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u" crossorigin="anonymous">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <script src="../JS/common.js" type="text/javascript"></script>
    <link href="Styles/StyleSheet.css" rel="stylesheet" type="text/css" />
    <link href="Styles/style.css" rel="stylesheet" type="text/css" />
    <script src="../JS/jquery-latest.min.js" type="text/javascript"></script>
    <script src="<%=Classes.Common.GetBaseURL%>JS/jquery-ui.min.js" type="text/javascript"></script>
    <link href='<%=Classes.Common.GetBaseURL%>admin/Styles/jquery-ui.css' rel="Stylesheet" type="text/css" />

    <link href="../ThickBox/thickbox.css" rel="stylesheet" type="text/css" />

    <script src="../ThickBox/thickbox.js" type="text/javascript"></script>

    <%--    <ajaxToolkit:ToolkitScriptManager ID="toolkit" runat="server"></ajaxToolkit:ToolkitScriptManager>--%>
    <asp:ScriptManager ID="sc" runat="server" AsyncPostBackTimeout="7200"></asp:ScriptManager>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js" integrity="sha384-Tc5IQib027qvyjSMfHjOMaLkfuWVxZxUPnCJA7l2mCWNIpG9mGCD8wGNIcPD7Txa" crossorigin="anonymous"></script>
    <input name="hidHTitleSort" id="hidHTitleSort" type="hidden" runat="server" />
    <input name="txtHTitleSortType" id="hidHTitleSortType" type="hidden" runat="server" />
    <style type="text/css">
        #TB_window {
            top: 15% !important;
            margin-top: -2% !important;
        }

        #divOrder.right_main {
            width: 90%;
            margin: 10px;
        }

        .right_main {
            /*margin-left: -37px !important;*/
        }

        #ContentPlaceHolder1_divOrder .main_input {
            margin: 0px !important;
            width: 100% !important;
        }

        .scrollbar {
            margin-left: 10px;
            float: left;
            height: 300px;
            max-width: 100%;
            /*background: #F5F5F5;*/
            overflow-y: scroll;
            margin-bottom: 25px;
        }

        .force-overflow {
            min-height: 450px;
        }

        .heading_section {
            padding: 0px 4%;
        }

        .AlternateItem input[type="submit"] {
            margin-top: -12px;
        }
             /*style for hide extra element of other cantroler Accessory*/ 
    #ContentPlaceHolder1_orderUpgradeOrder_orderAccessoryOrder_pnlOrderStatusInfo { display:none }
    #ContentPlaceHolder1_orderUpgradeOrder_orderAccessoryOrder_pnlOrderInfo { display:none }
    #ContentPlaceHolder1_orderUpgradeOrder_orderAccessoryOrder_fldShippingLabel { display:none }
    #ContentPlaceHolder1_orderUpgradeOrder_orderAccessoryOrder_pnlShippingInfo { display:none }
    #ContentPlaceHolder1_orderUpgradeOrder_orderAccessoryOrder_pnlOrderInstInfo { display:none }
    #ContentPlaceHolder1_orderUpgradeOrder_orderAccessoryOrder_grdAttachments { display:none }
    #ContentPlaceHolder1_orderUpgradeOrder_orderAccessoryOrder_divAttachment { display:none }
    #ContentPlaceHolder1_orderUpgradeOrder_grdAttachments { display:none }
    #grdShippingLabelDevice { display:block }

        /*
 *  STYLE 1
 */

        #TaskDetailContent::-webkit-scrollbar-track {
            -webkit-box-shadow: inset 0 0 6px rgba(0,0,0,0.3);
            border-radius: 10px;
            background-color: #F5F5F5;
        }

        #TaskDetailContent::-webkit-scrollbar {
            width: 12px;
            background-color: #F5F5F5;
        }

        #TaskDetailContent::-webkit-scrollbar-thumb {
            border-radius: 10px;
            -webkit-box-shadow: inset 0 0 6px rgba(0,0,0,.3);
            background-color: #555;
        }

        .header_upper {
            height: 50px !important;
            width: 100%;
        }

        .heading_section {
            width: 100%;
        }

        .rloader {
        }
    </style>
    <asp:UpdatePanel ID="updCarrier" runat="server">
        <ContentTemplate>
            <asp:HiddenField ID="hdnClientID" runat="server" />
            <asp:HiddenField ID="hdnTaskLock" runat="server" />
            <asp:HiddenField ID="hdnAdminTaskId" runat="server" />
            <asp:HiddenField ID="hdnDeletePermission" runat="server" />
            <asp:HiddenField ID="hdnAddEditPermission" runat="server" />
            <asp:HiddenField ID="hdnViewPermission" runat="server" />
            <asp:HiddenField ID="hdnAssignPermission" runat="server" />
            <asp:HiddenField ID="hdnTaskDetailsUserHistory" runat="server" />
            <asp:HiddenField ID="hdnTaskNotes" runat="server" />
            <asp:HiddenField ID="hdnOtherEmail" runat="server" />
            <asp:HiddenField ID="hdnOrderId" runat="server" />
            <asp:HiddenField ID="hdnTaskType" runat="server" />
            <asp:HiddenField ID="hdnOrderStatusSub" runat="server" />
            <asp:HiddenField ID="hdnOrderTypeSub" runat="server" />
            <asp:HiddenField ID="hdnCarrierId" runat="server" />
            <asp:HiddenField ID="hdnLastEmailFrom" runat="server" />
            <asp:HiddenField ID="hdnConversationId" runat="server" Value="" />

            <div class="right_main">
                <h1><%=Classes.AppConstants.TLOrderTasks %>
                </h1>
                <div class="white_column">
                </div>
                <div class="main_input">
                    <div class="input_row">
                        <div class="input_row_left_InnerFieldSet">
                            <div class="row">
                                <asp:Label ID="lblMsg" runat="server" CssClass="col-10 Span_Label error-msg">
                                </asp:Label>
                            </div>
                            <div class="row">
                                <div class="col-3 fl">
                                    <asp:Label ID="lblTimeNow" runat="server" CssClass="col-10 Span_Label">
                                    </asp:Label>
                                </div>
                                <div class="col-1 fl">
                                    <span class="Span_Label">Request ID:</span>
                                </div>
                                <div class="col-1 fl">
                                    <asp:Label ID="lblRequestID" runat="server" CssClass="col-10 Span_Label">
                                    </asp:Label>
                                </div>
                                <asp:Panel ID="pnlButtons" runat="server" CssClass="col-7 fr">
                                    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                                        <ProgressTemplate>
                                            <img class="progress_mid" src="Images/progressbar.gif" alt="Loading" />
                                        </ProgressTemplate>
                                    </asp:UpdateProgress>

                                    <asp:Button ID="btnClose" runat="server" Text="Close" CssClass="button_Close" Style="float: right"
                                        OnClick="btnClose_Click" />
                                    <asp:Button ID="btnCloseAndAddToManageLines" runat="server" Text="Close & Add To Manage Lines" CssClass="fl button_Update" Style="float: right"
                                        OnClick="btnCloseAndAddToManageLines_Click" OnClientClick="return validateManageLines()" />
                                    <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="button_Update" ValidationGroup="C11" Visible="false" Style="float: right"
                                        OnClick="btnSave_Click" />
                                    <asp:Button ID="btnSaveActivation" runat="server" Text="Submit For Activation" CssClass="button_Update" ValidationGroup="NewLine" Visible="false" Style="float: right"
                                        OnClick="btnSaveActivation_Click" />
                                    <asp:Button ID="btnSaveClose" runat="server" Text="Save-Close" CssClass="button_Update-save" ValidationGroup="C11" Style="float: right"
                                        OnClick="btnSaveClose_Click" />
                                    <asp:Button ID="btnSaveOnly" runat="server" Text="Save" CssClass="button_Update" ValidationGroup="C11" Visible="true" Style="float: right"
                                        OnClick="btnSaveOnly_Click" />
                                    <asp:Button ID="btnEmail" runat="server" Text="Send Email" CssClass="button_Close" Visible="false" Style="float: right"
                                        OnClick="btnEmail_Click" />

                                    <%--</div>--%>
                                </asp:Panel>

                            </div>
                            <div class="row">
                                <div class="fl">
                                    <asp:Button ID="btnDeleteOrder" runat="server" Text="Delete Order" CssClass="button_Close"
                                        OnClientClick="return confirm('Are you sure to DELETE the order?');" OnClick="btnDeleteOrder_Click" />
                                </div>
                                <asp:Panel ID="pnlManageLines" runat="server" CssClass="fr">
                                    Activation/ Upgrade Date:
                                    <asp:TextBox ID="txtActivationDate" runat="server"></asp:TextBox>
                                    <ajaxToolkit:CalendarExtender ID="calActivation" runat="server" TargetControlID="txtActivationDate" />
                                </asp:Panel>
                            </div>
                            <div class="row">
                                <div class="col-2 fl">
                                    <span class="Span_Label">Company Name:</span>
                                </div>
                                <div class="col-3 fl">
                                    <asp:TextBox ID="txtCompanyName" runat="server" onkeyup="SearchText(this.id);" onblur="CheckAndHighlight('client');" col-name="MBC.CompanyName" CssClass="col-10 autosuggest search">
                                    </asp:TextBox>
                                </div>
                                <div class="col-2 fl">
                                    <span class="Span_Label">
                                        <asp:LinkButton ID="lnkOpenCompany" runat="server" Text="Open Company" OnClientClick="OpenCompany('open')"></asp:LinkButton></span>
                                </div>
                                <div class="col-1 fl">
                                    <span class="Span_Label padding-left17">Rep :</span>
                                </div>
                                <div class="col-3 fl padding-left6">
                                    <asp:DropDownList ID="ddlRep" runat="server" CssClass="col-10 search">
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ControlToValidate="ddlRep" InitialValue="0" ID="RequiredFieldValidator5"
                                        ErrorMessage="Please Select Rep" ForeColor="Red" runat="server" ValidationGroup="C11"
                                        Display="Dynamic">*
                                    </asp:RequiredFieldValidator>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-2 fl">
                                    <span class="Span_Label">Contact:</span>
                                </div>
                                <div class="col-5 fl">
                                    <asp:TextBox ID="txtContact" runat="server" CssClass="col-10 autosuggest search" onkeyup="SearchText(this.id);" onblur="CheckAndHighlight('contact');" col-name="ContactBoth">
                                    </asp:TextBox>
                                </div>
                                <div class="col-1 fl">
                                    <span class="Span_Label">Task Status: </span>
                                </div>
                                <div class="col-3 fl">
                                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="col-10 search" Enabled="false">
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ControlToValidate="ddlStatus" InitialValue="0" ID="RequiredFieldValidator6"
                                        ErrorMessage="Please Select Status" ForeColor="Red" runat="server" ValidationGroup="C11"
                                        Display="Dynamic">*
                                    </asp:RequiredFieldValidator>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-2 fl">
                                    <span class="Span_Label">Email Address:</span>
                                </div>
                                <div class="col-5 fl">
                                    <asp:TextBox ID="txtEmailAddress" runat="server" CssClass="col-10 search" onblur="return isValidEmailAddress(this.id)">
                                    </asp:TextBox>
                                </div>
                                <div class="col-1 fl">
                                    <span class="Span_Label">Task Type: </span>
                                </div>
                                <div class="col-3 fl">
                                    <asp:DropDownList ID="ddlOrderType" runat="server" CssClass="col-10 search" Enabled="false">
                                        <asp:ListItem Text="Client Request" Value="Client Request"></asp:ListItem>
                                        <asp:ListItem Text="Order" Value="Order"></asp:ListItem>
                                        <asp:ListItem Text="Repl. Request" Value="Repl. Request"></asp:ListItem>
                                        <asp:ListItem Text="Carrier Alert" Value="Carrier Alert"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ControlToValidate="ddlOrderType" InitialValue="0" ID="RequiredFieldValidator1"
                                        ErrorMessage="Please Select Order type" ForeColor="Red" runat="server" ValidationGroup="C11"
                                        Display="Dynamic">*
                                    </asp:RequiredFieldValidator>
                                </div>
                            </div>
                            <div class="row col-12 fr">
                                <div class="fl padding-right10 padding-left10">
                                    <asp:CheckBox ID="chkCopyContact" runat="server" Text=" Copy Other Contact" Checked="true"></asp:CheckBox>
                                </div>
                            </div>
                            <div class="div_clear"></div>
                            <asp:Panel ID="pnlInvoice" runat="server" Visible="false">

                                <div class="col-2 fl">
                                    <span class="Span_Label">Invoice:</span>
                                </div>
                                <div class="col-4 fl">
                                    <asp:TextBox ID="txtInvoice" runat="server" CssClass="col-10">
                                    </asp:TextBox>
                                </div>
                                <div class="col-2 fl">
                                    <asp:Button ID="btnSaveInvoice" runat="server" Text="Save Invoice"
                                        CssClass="button_Update" OnClick="btnSaveInvoice_Click" />
                                </div>
                                <div class="div_clear"></div>
                            </asp:Panel>
                            <div class="row col-12 fl">
                                <div class="fl">
                                    <asp:Button ID="btnSendMailToOrderDesk" runat="server" Text="Resend Order" CssClass="fl button_Update"
                                        OnClick="btnSendMailToOrderDesk_Click" />
                                </div>

                                <div class="fl">
                                    <asp:Button ID="btnSendOrderConfToClient" runat="server" Text="Send Order Conf To Client"
                                        CssClass="button_Update" OnClientClick="return validateShippingOption();" OnClick="btnSendOrderConfToClient_Click" /><br />
                                    <br />
                                    <asp:Label ID="lblSendOrderConfToClient" runat="server"></asp:Label>
                                    <asp:HiddenField ID="hdnSendOrderConfToClient" runat="server" />
                                </div>
                                <div class="fl">
                                    <asp:Button ID="btnPrint" runat="server" Text="Print Package Slip" CssClass="fl button_Update"
                                        OnClick="btnPrint_Click" />
                                </div>
                                <div class="clearfix"></div>
                                <div class="fl" id="divleaseterm" runat="server">

                                    <div class="fl width250">
                                        Lease Term:
                                        <asp:DropDownList ID="ddldivLeaseTerm" CssClass="ddl_160 wider_ddl" runat="server" Visible="true" ValidationGroup="NewLineLease">
                                            <asp:ListItem Text="--Select--" Value=""></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ControlToValidate="ddldivLeaseTerm" InitialValue="" ID="RequiredFieldValidator3"
                                            ErrorMessage="Please Select Lease Term" ForeColor="Red" runat="server" ValidationGroup="NewLineLease"
                                            Display="Dynamic">*
                                        </asp:RequiredFieldValidator>
                                    </div>
                                    <div class="fl">
                                        <asp:Button ID="btnConvertToDeviceLease" runat="server" Text="Convert Order to Device Lease"
                                            CssClass="button_Update" OnClick="btnConvertToDeviceLease_Click" ValidationGroup="NewLineLease" CausesValidation="true" /><br />
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-2 fl">
                                    <span class="Span_Label">Task Heading:</span>
                                </div>
                                <div class="col-10 fl">
                                    <asp:TextBox ID="txtTaskHeading" runat="server" CssClass="col-10 search">
                                    </asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" Display="Dynamic" ValidationGroup="C11"
                                        CssClass="error-msg" runat="server" ControlToValidate="txtTaskHeading" ErrorMessage="Please enter Task Heading.">*</asp:RequiredFieldValidator>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-2 fl">
                                    <span class="Span_Label">Task Details:</span>
                                </div>
                                <div class="col-2 fr">
                                    <span class="Span_Label">
                                        <asp:LinkButton ID="lnkbtn" runat="server" CssClass="" Width="110%" Text="View Email Threads" OnClick="lnkbtn_Click"></asp:LinkButton></span>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-12" id="style-1">
                                    <center>
                                        <div class="rloader" style="display: none; margin-right: 10px; width: 10%">
                                            <img class="loading-image" style="width: 50px; height: 50px; margin-top: -30px;" src="/Admin/Images/ajax-loader.gif" alt="loading..">
                                        </div>
                                    </center>
                                    <div class="scrollbar" id="TaskDetailContent" style="min-height: 250px; max-height: 300px; min-width: 100%; overflow-y: auto; padding: 5px; display: none"></div>
                                </div>
                            </div>
                            <div id="divOrder" runat="server" class="col-sm-12 fl">
                                <uc2:UpgradeOrder ID="orderUpgradeOrder" runat="server" Visible="false" />
                                <uc2:GPSOrder ID="orderGPS" runat="server" Visible="false" />
                                <uc2:NewActivationOrder ID="orderNewActivation" runat="server" Visible="false" />
                                <%--<uc2:AccessoryOrder ID="orderAccessoryOrder" runat="server" Visible="true" />--%>
                            </div>
                            <div class="row" id="divAdminTask" runat="server" style="margin-left: 20px">
                                <div class="row">
                                    <div class="col-sm-2 fl">
                                        <span class="Span_Label">Order Internal Notes:</span>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-sm-12 fl">
                                        <fieldset class="border-gray">
                                            <asp:TextBox ID="txtOrderInternalNotes" runat="server" CssClass="col-12 input-borderless" TextMode="MultiLine" Rows="4" onfocus="setTimeStamp('set')" onblur="setTimeStamp('del')">
                                            </asp:TextBox>
                                            <div id="div1" runat="server" class="input-borderless" style="padding: 0px 0px;"></div>
                                        </fieldset>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-sm-3 fl">
                                        <span class="Span_Label">Response to Order desk:</span>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-sm-12 fl">
                                        <fieldset class="border-gray">
                                            <asp:TextBox ID="txtResponseToOrderDesk" runat="server" CssClass="col-12 input-borderless" TextMode="MultiLine" Rows="10">
                                            </asp:TextBox>
                                            <div id="divSignature" runat="server" class="input-borderless" style="padding: 10px 10px;"></div>
                                        </fieldset>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-sm-12 fl">
                                    <fieldset>
                                        <legend class="error-msg bold">Attachments for response to Order Desk</legend>
                                        <asp:GridView ID="grdPhoneDetails" runat="server" PageSize="5" AutoGenerateColumns="false" ShowHeaderWhenEmpty="true"
                                            Width="100%" AllowPaging="true" AllowSorting="true" OnRowCommand="grdPhoneDetails_RowCommand"
                                            OnRowDataBound="grdPhoneDetails_RowDataBound" OnRowEditing="grdPhoneDetails_RowEditing"
                                            DataKeyNames="AdminTaskId,AdminTaskAttachId" CssClass="grid_column" PagerSettings-Visible="true"
                                            PagerSettings-Position="Bottom" OnPageIndexChanging="grdPhoneDetails_PageIndexChanging"
                                            OnRowUpdating="grdPhoneDetails_OnRowUpdating" EmptyDataText="No attachments found.">
                                            <Columns>
                                                <asp:TemplateField HeaderText="Attach with Response" Visible="true" ItemStyle-Width="5%" HeaderStyle-HorizontalAlign="Center"
                                                    ItemStyle-HorizontalAlign="Right">
                                                    <ItemTemplate>
                                                        <asp:CheckBox ID="chkAttachment" runat="server" Checked="true" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                                    <HeaderTemplate>
                                                        <a href="javascript:PhoneSort.ChangePhoneSort('FileName')"><font color="black">FileName</font></a>
                                                        <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                            id="img_filename_asc" style="border: 0; display: none" />
                                                        <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                            id="img_filename_desc" style="border: 0; display: none" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <a id="aDocumentPath" runat="server" href='<%#Eval("DocumentPath")%>' target="_blank"><%#Eval("FileName")%></a>
                                                        <asp:Literal ID="ltrLocalPath" runat="server" Text='<%#Eval("LocalPath")%>' Visible="false"></asp:Literal>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                                    <HeaderTemplate>
                                                        <a href="javascript:PhoneSort.ChangePhoneSort('createddate')"><font color="black">Uploaded Date</font></a>
                                                        <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                            id="img_createddate_asc" style="border: 0; display: none" />
                                                        <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                            id="img_createddate_desc" style="border: 0; display: none" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <%#Eval("createddate")%>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                                    <HeaderTemplate>
                                                        <a href="javascript:PhoneSort.ChangePhoneSort('Name')"><font color="black">Uploaded By</font></a>
                                                        <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                            id="img_Name_asc" style="border: 0; display: none" />
                                                        <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                            id="img_Name_desc" style="border: 0; display: none" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <%#Eval("Name")%>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="14%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                                    <HeaderTemplate>
                                                        Action
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <a href='<%#Eval("DocumentPath")%>' target="_blank">View</a>
                                                        |<asp:LinkButton ID="lnkDeleteAttachment" runat="server" CommandName="DeleteData" CommandArgument='<%# Eval("AdminTaskAttachId").ToString() %>'
                                                            OnClientClick="return confirm('Are you sure to delete?');">Delete </asp:LinkButton>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                            <PagerTemplate>
                                                <table cellpadding="0" cellspacing="0" width="100%" border="0">
                                                    <tr>
                                                        <td>
                                                            <asp:Label ID="lblPagingSummaryPhone" CssClass="pagingLabelText" runat="server"></asp:Label>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <uc1:PagingControl ID="PagingPhone" OnPaging_Click="Paging_Click" FirstString="<< First"
                                                                LastString=" Last >>" NextString="Next >" PrevString="< Prev" runat="server" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </PagerTemplate>
                                            <EmptyDataTemplate>
                                                No records found!
                                            </EmptyDataTemplate>
                                            <EmptyDataRowStyle HorizontalAlign="Center" />
                                        </asp:GridView>

                                    </fieldset>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-sm-12 fl">
                                    <fieldset>
                                        <legend>Attach a file</legend>
                                        <asp:Button ID="btnAttachFile" runat="server" Visible="false" Text="Attach File" CssClass="fl button_Update" ValidationGroup="C11"
                                            OnClick="btnAttachFile_Click" />

                                        <asp:Button ID="btnAsyncUpload" runat="server"
                                            Text="Attach File" OnClick="btnAttachFile_Click" />

                                        <asp:FileUpload ID="fUAttachment" runat="server" Width="200px" />
                                    </fieldset>
                                </div>
                            </div>
                            <div class="row">
                                <div class="fr">

                                    <asp:Button ID="btnOrderResponse" runat="server" Text="Send Response To Order Desk" CssClass="fl button_Update"
                                        OnClick="btnOrderResponse_Click" />
                                    <asp:Button ID="btnResponseCompleted" runat="server" Text="Send Completed Response" CssClass="fl button_Update"
                                        OnClick="btnResponseCompleted_Click" />
                                    <asp:Button ID="btnResponse" runat="server" Text="Send Reponse" CssClass="fl button_Update"
                                        OnClick="btnResponse_Click" />

                                    <div class="div_clear">
                                    </div>
                                </div>
                                <div class="fl">
                                    <asp:UpdateProgress ID="UpdateProgress4" runat="server">
                                        <ProgressTemplate>
                                            <img class="progress_mid" src="Images/progressbar.gif" alt="Loading" />
                                        </ProgressTemplate>
                                    </asp:UpdateProgress>
                                </div>

                            </div>
                            <div class="row">
                                <asp:Label ID="lblMsgDown" runat="server" CssClass="col-10 Span_Label error-msg">
                                </asp:Label>
                            </div>


                            <div class="row">
                                <div class="col-sm-12 fl">

                                    <fieldset>
                                        <legend class="error-msg bold">Activity Log</legend>

                                        <asp:GridView ID="grdActivityLog" runat="server" PageSize="5" AutoGenerateColumns="false" ShowHeaderWhenEmpty="true"
                                            Width="100%" AllowPaging="true" AllowSorting="true"
                                            DataKeyNames="AdminTaskId" CssClass="grid_column" PagerSettings-Visible="true"
                                            PagerSettings-Position="Bottom" OnPageIndexChanging="grdActivityLog_PageIndexChanging"
                                            EmptyDataText="No logs found.">
                                            <Columns>

                                                <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                                    <HeaderTemplate>
                                                        <a href="javascript:PhoneSort.ChangePhoneSort('modifieddate')"><font color="black">Activity Date</font></a>
                                                        <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                            id="img_modifieddate_asc" style="border: 0; display: none" />
                                                        <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                            id="img_modifieddate_desc" style="border: 0; display: none" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <%#Eval("ModifiedDate")%>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                                    <HeaderTemplate>
                                                        <a href="javascript:PhoneSort.ChangePhoneSort('activity')"><font color="black">Activity</font></a>
                                                        <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                            id="img_activity_asc" style="border: 0; display: none" />
                                                        <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                            id="img_activity_desc" style="border: 0; display: none" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <%#Eval("activity")%>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                            <PagerTemplate>
                                                <table cellpadding="0" cellspacing="0" width="100%" border="0">
                                                    <tr>
                                                        <td>
                                                            <asp:Label ID="lblPagingSummaryPhone" CssClass="pagingLabelText" runat="server"></asp:Label>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <uc1:PagingControl ID="PagingPhone" OnPaging_Click="Paging_Click" FirstString="<< First"
                                                                LastString=" Last >>" NextString="Next >" PrevString="< Prev" runat="server" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </PagerTemplate>
                                            <EmptyDataTemplate>
                                                No records found!
                                            </EmptyDataTemplate>
                                            <EmptyDataRowStyle HorizontalAlign="Center" />
                                        </asp:GridView>

                                    </fieldset>
                                </div>
                            </div>




                            <div style="display: none">
                                <asp:RadioButtonList CssClass="add-service-option_new" ID="RadioButtonList2" TextAlign="Right"
                                    RepeatDirection="Horizontal" runat="server">
                                    <asp:ListItem Value="1">Yes
                                    </asp:ListItem>
                                    <asp:ListItem Value="2" Selected="True">No
                                    </asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <ajaxToolkit:ModalPopupExtender ID="modalConfirm" runat="server" BehaviorID="modalPopupExtender5"
                                TargetControlID="RadioButtonList2" PopupControlID="pnlConfirmMail" OkControlID="lnkMakeMessage"
                                OnOkScript="lnkMakeMessage" BackgroundCssClass="modalBackground" CancelControlID="Button5">
                            </ajaxToolkit:ModalPopupExtender>

                            <asp:Panel ID="pnlConfirmMail" runat="server" Width="400px" CssClass="confirm-dialog">
                                <asp:HiddenField ID="hdnCaller" runat="server" />
                                <table class="popup-box3" id="tblNotification" runat="server" style="padding: 10px">
                                    <tr>
                                        <td class="popup-box3-heading" colspan="3">Please select<asp:HiddenField ID="hdnAction" runat="server" />
                                        </td>
                                    </tr>

                                    <tr id="trNotfCustEOD" runat="server">
                                        <td style="width: 10px"></td>
                                        <td class="" id="tdNotfCustEOD" runat="server">
                                            <div style="font-size: 14px; font-weight: bold; margin: 0px auto;">
                                                Send Notification To Customer:
                                            </div>
                                        </td>
                                        <td class="">
                                            <asp:RadioButtonList ID="rdoSendNotificationToCustomer" runat="server" RepeatDirection="Horizontal">
                                                <asp:ListItem Text="Yes" Value="1" Selected="True"></asp:ListItem>
                                                <asp:ListItem Text="No" Value="0"></asp:ListItem>
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                    <tr id="trNotfCustEODButton" runat="server">
                                        <td style="width: 10px"></td>
                                        <td class="" colspan="2">
                                            <asp:Button ID="btnSendnotf" runat="server" CssClass="button_Update" Width="100px" Text="Submit" OnClick="btnSendOrderConfToClient_Click" />

                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2">
                                            <asp:Label ID="lblMailSentMsg" runat="server"></asp:Label>
                                            <asp:HiddenField ID="hdnMsg" runat="server" />
                                            <%--<asp:Button ID="Button1" CssClass="button_Update" runat="server" Text="Close" />--%>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2">
                                            <asp:UpdateProgress ID="UpdateProgress2" runat="server">
                                                <ProgressTemplate>
                                                    <img class="progress_mid" src="Images/progressbar.gif" alt="Loading" />
                                                </ProgressTemplate>
                                            </asp:UpdateProgress>
                                            <asp:LinkButton ID="lnkMakeMessage" runat="server" />
                                            <asp:Button ID="Button5" Width="100px" CssClass="button_Update" runat="server" Text="Cancel" Style="display: none" />

                                        </td>
                                    </tr>
                                </table>
                            </asp:Panel>
                        </div>
                    </div>
                </div>
            </div>

        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnAsyncUpload" />

        </Triggers>
    </asp:UpdatePanel>

    <script>
        function CheckAndHighlight(inputtype) {
            OpenCompany(inputtype);
        }

        function SearchText(ctrl) {
            $(".autosuggest").autocomplete({
                source: function (request, response) {
                    //debugger;
                    var txt = $('.autosuggest').val();
                    var arr = new Array();
                    var a = $('#' + ctrl).val();

                    //var b = $("#ddlField1 option:selected").text();
                    var b = $('#' + ctrl).attr('col-name');// "MBC.CompanyName";
                    var ParentText = $('#<%=hdnClientID.ClientID%>').val() == '' ? $('#<%= txtCompanyName.ClientID %>').val() : $('#<%=hdnClientID.ClientID%>').val();
                    arr.push('a');
                    $.ajaxSettings.traditional = true;
                    arr.push('b');
                    $.ajax({
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        url: "AjaxMethods.aspx/GetAutoCompleteData",
                        data: "{'username':'" + a + "','column':'" + b + "','ParentColumn':'" + a + "','ParentText':'" + ParentText + "'}",
                        dataType: "json",
                        success: function (data) {
                            response(data.d);
                        },
                        error: function (result) {
                            alert("Error");
                        }
                    });
                }
            });


        }

        function OpenCompany(inputtype) {
            var companyName = $('#<%=txtCompanyName.ClientID%>').val();
            var contact = $('#<%=txtContact.ClientID%>').val();
            var openLink = (inputtype == 'open' ? 'yes' : inputtype);

            if (companyName == '' && (inputtype == 'client' || inputtype == 'open')) {
                //alert('Please select company name');
                //$('#<%=txtCompanyName.ClientID%>').focus();
            }
            else if (contact == '' && (inputtype == 'contact')) {
                //alert('Please select contact');
                //$('#<%=txtContact.ClientID%>').focus();
            }
            else {
                $.ajax({
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    url: "AjaxMethods.aspx/GetBusinessClientInfo",
                    data: "{'Column':'" + inputtype + "','Keyword1':'" + companyName + "','Keyword2':'" + contact + "'}",
                    dataType: "json",
                    success: function (data) {
                        //debugger;
                        if (data.d != null && data.d > 0) {
                            $('#<%=hdnClientID.ClientID%>').val(data.d);
                            if (openLink == 'yes')
                                window.open('/Admin/Business_Client_Manage.aspx?ClientId=' + data.d, "_blank");
                        }
                        else {
                            if (inputtype == 'client' || inputtype == 'open') {
                                inputtype = 'Company Name';
                                alert(inputtype + ' is invalid. Please select a valid ' + inputtype);
                            };
                            if (inputtype == 'contact') { inputtype = 'Contact'; };
                        }
                    },
                    error: function (result) {
                        //debugger;
                        alert("Error");
                    }
                });
            }
            return false;
        }
        window.onbeforeunload = function (event) {
            var adminTaskId = $('#<%=hdnAdminTaskId.ClientID%>').val();
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "AjaxMethods.aspx/UnlockItem",
                data: "{'adminTaskId':'" + adminTaskId + "','adminId':'" +<%=Session[Classes.AppConstants.SessionAdminUserId]%> + "'}",
                dataType: "json",
                success: function (data) {
                    //alert('unlocked');
                },
                error: function (result) {
                    //alert("Error");
                }
            });
        }

        $(document).ready(function () {

            var adminTaskId = $('#<%=hdnAdminTaskId.ClientID%>').val();
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "AjaxMethods.aspx/LockItem",
                data: "{'adminTaskId':'" + adminTaskId + "','adminId':'" +<%=Session[Classes.AppConstants.SessionAdminUserId]%> +"'}",
                dataType: "json",
                success: function (data) {
                    //alert('locked');
                },
                error: function (result) {
                    //alert("Error");
                }
            });

            //debugger
            //readEmailContent();

        });

        function readEmailContent() {
            var reademail_email = $("#ContentPlaceHolder1_txtEmailAddress").val();
            var reademail_subject = $("#ContentPlaceHolder1_txtTaskHeading").val();
            var reademail_conversationid = $("#ContentPlaceHolder1_hdnConversationId").val();
            try {
                $.ajax({
                    type: "POST",
                    url: "AjaxMethods.aspx/ReadEmails", //?email=" + reademail_email + "&subject=" + reademail_subject + "&conversationid=" + reademail_conversationid,
                    dataType: "json",
                    contentType: "application/json; charset=utf-8",
                    data: "{'email':'" + reademail_email + "','subject':'" + reademail_subject + "','conversationid':'" + reademail_conversationid + "'}",
                    //data: {'email':'" + reademail_email + "','subject':'" + reademail_subject + "','conversationid':'" + reademail_conversationid + "'},
                    success: function (data) {
                        var result = JSON.parse(data.d);
                        if (result.success) {
                            var data_result = result.aaData;
                            if (data_result != null) {
                                var emailcontent = "";
                                var tabindex = 1;
                                emailcontent += '<ul class="nav nav-tabs">';
                                $.each(data_result, function (datakey, datavalue) {
                                    var emailto = "";
                                    var emailbody = "";
                                    var emaildate = "";
                                    var emailfrom = "";
                                    var emailsubject = "";
                                    $.each(datavalue, function (key, value) {
                                        if (key != null) {
                                            if (key == "MailDateSent") {
                                                emaildate = value;
                                            }
                                            if (key == "Body") {
                                                emailbody = value;
                                            }
                                            if (key == "Subject") {
                                                emailsubject = value;
                                            }
                                            if (key == "Recipients") {
                                                var arrayemailto = value;
                                                var startingIndex = 0;
                                                $.each(arrayemailto, function (_key, _value) {
                                                    if (startingIndex == 0) {
                                                        emailto = _value;
                                                    }
                                                    else {
                                                        emailto = emailto + "," + _value;
                                                    }
                                                    startingIndex++;
                                                });
                                            }
                                            if (key == "From") {
                                                emailfrom = value;
                                            }
                                        }

                                    });
                                    if (tabindex == 1) {
                                        emailcontent += '<li class="active"><a data-toggle="tab" href="#email' + tabindex + '">Email ' + tabindex + '</a></li>';
                                    }
                                    else {
                                        emailcontent += '<li class=""><a data-toggle="tab" href="#email' + tabindex + '">Email ' + tabindex + '</a></li>';
                                    }
                                    tabindex++;
                                });
                                emailcontent += '</ul>';
                                emailcontent += '<div class="tab-content">';
                                //emailcontent = "";
                                tabindex = 1;
                                $.each(data_result, function (datakey, datavalue) {
                                    var emailto = "";
                                    var emailbody = "";
                                    var emaildate = "";
                                    var emailfrom = "";
                                    var emailsubject = "";
                                    $.each(datavalue, function (key, value) {
                                        if (key != null) {
                                            if (key == "MailDateSent") {
                                                emaildate = value;
                                            }
                                            if (key == "Body") {
                                                emailbody = value;
                                            }
                                            if (key == "Subject") {
                                                emailsubject = value;
                                            }
                                            if (key == "Recipients") {
                                                var arrayemailto = value;
                                                var startingIndex = 0;
                                                $.each(arrayemailto, function (_key, _value) {
                                                    if (startingIndex == 0) {
                                                        emailto = _value;
                                                    }
                                                    else {
                                                        emailto = emailto + "," + _value;
                                                    }
                                                    startingIndex++;
                                                });
                                            }
                                            if (key == "From") {
                                                emailfrom = value;
                                            }
                                        }

                                    });



                                    var _emailcontent = '<fieldset class="border-gray" style="padding-left:25px"><div class="row"><div class="col-12">';
                                    _emailcontent += '<div class="AlternateItem" style="background-color: antiquewhite;height:40px;"><span class="col-10 Span_Label bold">Email date: ' + emaildate + '</span></div>';
                                    _emailcontent += '</div></div>';
                                    _emailcontent += '<div class="row"><div class="col-12">';
                                    _emailcontent += '<div class="">To: ' + emailto + '</div>';
                                    _emailcontent += '</div></div>';
                                    _emailcontent += '<div class="row"><div class="col-12">';
                                    _emailcontent += '<div class="">From: ' + emailfrom + '</div>';
                                    _emailcontent += '</div></div>';
                                    _emailcontent += '<div class="row"><div class="col-12">';
                                    _emailcontent += '<div class="">Subject: ' + emailsubject + '</div>';
                                    _emailcontent += '</div></div>';
                                    _emailcontent += '<div class="row"><div class="col-12">';
                                    _emailcontent += '<div class="" style="overflow:hidden">' + emailbody + '</div>';
                                    _emailcontent += '</div></div></fieldset>';


                                    if (tabindex == 1) {
                                        emailcontent += '<div id="email' + tabindex + '" class="tab-pane fade in active">' + _emailcontent + '</div>';
                                    }
                                    else {
                                        emailcontent += '<div id="email' + tabindex + '" class="tab-pane fade">' + _emailcontent + '</div>';
                                    }

                                    tabindex++;
                                });
                                emailcontent += '</div>';
                                $(".rloader").hide();
                                $("#TaskDetailContent").html(emailcontent);
                            }
                        }
                        else {

                        }
                    },
                    error: function (result) {
                        //alert(result);
                        $(".rloader").hide();
                    },
                    beforeSend: function () {
                        $(".rloader").show();
                    },
                    complete: function () {
                        $(".rloader").hide();
                    }
                });
            }
            catch (e) {
                // alert(e);
            }
        }

        function validate(s, args) {
            if (document.getElementById("<%= txtCompanyName.ClientID %>").value == '') {
                args.IsValid = args.Value != '';
            }
            else {
                args.IsValid = true;
            }
        }
        function OpenThickBox(urltb, title) {
            //debugger;
            tb_show(title, urltb, 'null');
        }

        function Encode() {
            var value = (document.getElementById('<%=txtOrderInternalNotes.ClientID%>').value);
            value = value.replace('<', "&lt;");
            value = value.replace('>', "&gt;");
            document.getElementById('<%=txtOrderInternalNotes.ClientID%>').value = value;
        }
        //Encode();

        function refreshUserData() {
            var adminTaskId = $('#<%=hdnAdminTaskId.ClientID%>').val();
            if (adminTaskId > 0) {
                $.ajax({
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    url: BaseUrl + "Admin/AjaxMethods.aspx/GetLatestTaskInfo",
                    data: "{'repID':'" + 0 + "','AdminTaskId':'" + adminTaskId + "',sptype:'TASKOPENED'}",
                    dataType: "json",
                    success: function (response) {
                        //debugger;
                        if (response != null && response != undefined && response.d.length > 0) {
                            $('#<%=txtOrderInternalNotes.ClientID%>').val('');
                            $('#<%=txtOrderInternalNotes.ClientID%>').val(response.d[0].TaskNotes);
                        }
                    },
                    error: function (result) {

                    }
                });
            }
        }
        function isValidEmailAddress(ID) {
            //debugger;
            var emailAddress = $('#' + ID).val();
            if (emailAddress != '') {
                var pattern = /^([a-z\d!#$%&'*+\-\/=?^_`{|}~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]+(\.[a-z\d!#$%&'*+\-\/=?^_`{|}~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]+)*|"((([ \t]*\r\n)?[ \t]+)?([\x01-\x08\x0b\x0c\x0e-\x1f\x7f\x21\x23-\x5b\x5d-\x7e\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]|\\[\x01-\x09\x0b\x0c\x0d-\x7f\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))*(([ \t]*\r\n)?[ \t]+)?")@(([a-z\d\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]|[a-z\d\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF][a-z\d\-._~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]*[a-z\d\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])\.)+([a-z\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]|[a-z\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF][a-z\d\-._~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]*[a-z\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])\.?$/i;
                var isValid = pattern.test(emailAddress);
                if (!isValid) {
                    alert('Please enter valid email address');
                    $('#ContentPlaceHolder1_txtTaskHeading').focus();
                }
            }
        };
        function setTimeStamp(action) {
            var user = '<%=Session[Classes.AppConstants.SessionUserID]%>';
            var currentDate = new Date();
            var taskNotes = '#<%=txtOrderInternalNotes.ClientID%>';

            if (action == 'set' && $(taskNotes).val() == $('#<%=hdnTaskNotes.ClientID%>').val()) {
                //02/28/2017 11:06 PM admin :
                var content = '';
                content = (currentDate.toLocaleDateString() + ' ' + currentDate.toLocaleTimeString() + ' ' + user + ' : ').trim();
                $(taskNotes).val(($(taskNotes).val() + '\n' + content).trim());
            }

            if (action == 'del') {
            <%--if ($(taskNotes).val() == $('#<%=hdnTaskNotes.ClientID%>').val()) {
                $(taskNotes).val('');
                $(taskNotes).val($('#<%=hdnTaskNotes.ClientID%>').val());
            }
            else {
                debugger;--%>
                var splitContent = $(taskNotes).val().split('\n');
                var lastContent = splitContent[splitContent.length - 1].split(':');
                var lastCont = lastContent[lastContent.length - 1].replace(' ', '');
                if (lastCont == '') {
                    $(taskNotes).val('');
                    $(taskNotes).val($('#<%=hdnTaskNotes.ClientID%>').val());
                }
                //}
            }
        }
        function validateManageLines() {
            var trrackingNo = $('#ContentPlaceHolder1_orderNewActivation_txtShippingTrackingNumber').val();
            var method = $('#ContentPlaceHolder1_orderNewActivation_ddlShippingMethod').val();
            if (method == 'UPS' && (trrackingNo == '' || trrackingNo.length != 18)) {
                alert('Please enter a valid Shipping Tracking number.')
                return false;
            }
            if ($('#ContentPlaceHolder1_orderNewActivation_grdShippingLabel tr').length > 1
                && $('#ContentPlaceHolder1_orderNewActivation_ddlShippingOptionLabel').val() != '') {
                if ($($('#ContentPlaceHolder1_orderNewActivation_grdShippingLabel tr')[1]).find('td').length == 1
                    && $('#ContentPlaceHolder1_orderNewActivation_txtShippingTrackingNumber').val() == '') {
                    alert('Please generate Shipping Label/ enter Shipping Tracking number.')
                    return false;
                }


            }
            return true;
        }
        //window.setInterval(function () { refreshUserData(); }, 20000);

        $(document).ready(function () {
            // Stop user to press enter in textbox
            $("input:text").keypress(function (event) {
                if (event.keyCode == 13) {
                    event.preventDefault();
                    return false;
                }
            });
        });
    </script>
</asp:Content>
