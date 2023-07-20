<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NewActivationOrder.ascx.cs" Inherits="Admin_UserControls_NewActivationOrder" %>

<%@ Register Src="~/Controls/PagingControl.ascx" TagName="PagingControl" TagPrefix="uc1" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<%@ Register Src="~/Admin/UserControls/AccessoryOrder.ascx" TagName="AccessoryOrder" TagPrefix="uc2" %>
<%--<%@ Register Src="~/Admin/UserControls/ShippingServiceMethod.ascx" TagName="ShippingServiceMethod" TagPrefix="uc2" %>--%>

<%--<ajaxToolkit:ToolkitScriptManager ID="scriptmanager" runat="server">
    </ajaxToolkit:ToolkitScriptManager>--%>

<%--WS FULFILLMENT CENTER WIRELESS SUPPORT21828 Lassen St Suite O Chatsworth CA 91311 --%>

<input name="hidHTitleSort" id="Hidden1" type="hidden" runat="server" />

<input name="txtHTitleSortType" id="hidHTitleSortType" type="hidden" runat="server" />
<style type="text/css">
    #TB_window {
        top: 15% !important;
        margin-top: -2% !important;
    }

    .AlternateItem {
        margin-top: 10px;
        height: 20px;
        border: solid 2px antiquewhite;
        padding: 10px 0px;
        background-color: antiquewhite;
    }

    #ContentPlaceHolder1_ucNewActivationOrder_orderAccessoryOrder_pnlOrderGrid {
        margin: -20px;
    }
</style>

<asp:UpdatePanel ID="updOrderItem" runat="server" ChildrenAsTriggers="true" UpdateMode="Always">
    <ContentTemplate>
        <asp:HiddenField ID="hdnLineWeight" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdnDmBoxDbHeight" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdnDmBoxDbLenght" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdnDmBoxDbWirth" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdnDbShippingLabelWeight" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdnDeviceGridWeight" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdnClientID" runat="server" />
        <asp:HiddenField ID="hdnTaskLock" runat="server" />
        <asp:HiddenField ID="hdnOrderId" runat="server" />
        <asp:HiddenField ID="hdnDeletePermission" runat="server" />
        <asp:HiddenField ID="hdnAddEditPermission" runat="server" />
        <asp:HiddenField ID="hdnViewPermission" runat="server" />
        <asp:HiddenField ID="hdnAssignPermission" runat="server" />
        <asp:HiddenField ID="hdnTaskDetailsUserHistory" runat="server" />
        <asp:HiddenField ID="hdnDeleteLine" runat="server" />
        <asp:HiddenField ID="hdnOrderTypeSub" runat="server" />
        <asp:HiddenField ID="hdnSalesEmail" runat="server" />
        <asp:HiddenField ID="hdnOrderStatusSub" runat="server" />
        <asp:HiddenField ID="hdnAssociatedUserId" runat="server" Value="0" />

        <div class="right_main">
            <div class="main_input">
                <div class="input_row">
                    <div class="input_row_left_InnerFieldSet">
                        <div class="row">
                            <asp:Label ID="lblMsg" runat="server" CssClass="col-10 Span_Label error-msg">
                            </asp:Label>
                        </div>
                        <div class="row">
                            <div class="col-2 fl ">
                                <span>Order no :</span>
                            </div>
                            <div class="col-2 fl ">
                                <asp:Label ID="lblOrderNo" runat="server" CssClass="col-4 "></asp:Label>
                            </div>
                            <div class="col-2 fl">
                                <span>Order status:</span>
                            </div>
                            <div class="col-2 fl">
                                <asp:Label ID="lblOrderStatus" runat="server" CssClass="col-4 ">
                                </asp:Label>
                            </div>
                            <div class="col-3 fl">
                                <asp:CheckBox ID="chkMarkUrgent" runat="server" Text="Mark as Urgent" />
                            </div>
                        </div>
                        <div class="row" style="margin-top: 30px">

                            <div class="col-4 fl">
                                <asp:Panel ID="pnlInstallMDM" runat="server" CssClass="col-4 fl" Visible="true">
                                    Install MDM
                                <asp:RadioButtonList ID="rdoInstallMDM" runat="server" RepeatDirection="Horizontal">
                                    <asp:ListItem Text="Yes" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="No" Value="0"></asp:ListItem>
                                </asp:RadioButtonList>
                                    <asp:RequiredFieldValidator ID="rfInstallMDM" runat="server" ControlToValidate="rdoInstallMDM" CssClass="error-msg"
                                        Display="Dynamic" ErrorMessage="Please select option" ValidationGroup="NewLine1" InitialValue=""></asp:RequiredFieldValidator>
                                </asp:Panel>
                            </div>
                            <div class="col-4 fl">
                                <asp:Panel ID="Panel1" runat="server" CssClass="col-10 fl" Visible="true">
                                    Client Status
                                <asp:RadioButtonList ID="rdoclientStatus" runat="server" RepeatDirection="Horizontal">
                                    <asp:ListItem Text="New (Never Ordered)" Value="1" onClick="showActivationOrder(1);"></asp:ListItem>
                                    <asp:ListItem style="padding-left: 10px" Text="Existing" Value="0" onClick="showActivationOrder(0);"></asp:ListItem>
                                </asp:RadioButtonList>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="rdoclientStatus" CssClass="error-msg"
                                        Display="Dynamic" ErrorMessage="Please select Client Status" ValidationGroup="NewLine" InitialValue=""></asp:RequiredFieldValidator>
                                </asp:Panel>
                            </div>
                            <div class="col-4 fl" id="OrderActivationGain" runat="server" clientidmode="Static">
                                <asp:Panel ID="Panel2" runat="server" CssClass="row fl" Visible="true">
                                    <div cssclass="col-2">Activation Gain</div>

                                    <div cssclass="col-5 fl" style="display: inline">
                                        <asp:Label ID="NewNetGain" runat="server" Text="New (Net Gain)"></asp:Label>
                                        <asp:TextBox ID="txnetActivationGain" runat="server" Text="" Style="width: 15%;" TextMode="SingleLine" min="0" MaxLength="4"></asp:TextBox>

                                    </div>
                                    <div cssclass="col-5 fl" style="display: inline">
                                        <asp:Label ID="RefreshGain" runat="server" Text="Refresh(Not Net Gain)"></asp:Label>
                                        <asp:TextBox ID="txtRefreshGain" runat="server" Text="" Style="width: 15%;" TextMode="SingleLine" min="0" MaxLength="4"></asp:TextBox>
                                    </div>
                                </asp:Panel>
                            </div>

                            <div class="fr" style="border: solid 0px red;" id="divButtonAreaClose" runat="server">
                                <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                                    <ProgressTemplate>
                                        <img class="progress_mid" src="Images/progressbar.gif" alt="Loading" />
                                    </ProgressTemplate>
                                </asp:UpdateProgress>
                                <div class="col-4 fl" style="border: solid 0px red;" id="divButtonArea" runat="server">
                                </div>
                                <div class="col-1" style="border: solid 0px red;" id="div1" runat="server">
                                </div>
                                <asp:Button ID="btnSaveOrder" runat="server" Text="Save Order" CssClass="fl button_Update" ValidationGroup="NewLine" Visible="true"
                                    OnClick="btnSaveOrder_Click" />
                                <asp:Button ID="btnSubmitOrder" runat="server" Text="Submit Order" CssClass="button_Update-save" ValidationGroup="NewLine1"
                                    OnClick="btnSubmitOrder_Click" OnClientClick="validateControls()" Visible="false" />


                                <asp:Button ID="btnClose" runat="server" Text="Close" CssClass="button_Close"
                                    OnClick="btnClose_Click" />


                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <fieldset>
                            <legend class="bold">Account Information</legend>
                            <div class="col-2 fl">
                                <span class="Span_Label">Company Name:</span>
                            </div>
                            <div class="col-10 fl">
                                <asp:TextBox ID="txtCompanyName" runat="server" onkeyup="SearchText(this.id);" onblur="CheckAndHighlight('client');" col-name="MBC.CompanyName" CssClass="col-10 width552">
                                </asp:TextBox>
                                <asp:HiddenField ID="hdnWorkPhone" runat="server" />
                            </div>
                            <div class="div_clear"></div>
                            <div class="col-2 fl">
                                <span class="Span_Label">Carrier:</span>
                            </div>
                            <div class="col-5 fl">
                                <asp:DropDownList ID="ddlCarrier" runat="server" CssClass="col-10">
                                </asp:DropDownList>
                            </div>
                            <div class="col-1 fl">
                                <span class="Span_Label">Account #: </span>
                            </div>
                            <div class="col-3 fl">
                                <asp:TextBox ID="txtAccountNo" runat="server" CssClass="col-10">
                                </asp:TextBox>
                            </div>
                            <div class="div_clear"></div>
                            <div class="col-2 fl">
                                <span class="Span_Label">Passcode:</span>
                            </div>
                            <div class="col-5 fl">
                                <asp:TextBox ID="txtPassCode" runat="server" CssClass="col-10">
                                </asp:TextBox>
                            </div>
                            <div class="col-1 fl">
                                <span class="Span_Label">Tax Id:</span>
                            </div>
                            <div class="col-3 fl">
                                <asp:TextBox ID="txtTaxID" runat="server" CssClass="col-10">
                                </asp:TextBox>
                            </div>
                            <div class="div_clear"></div>
                            <div class="col-2 fl">
                                <span class="Span_Label">Spoc:</span>
                            </div>
                            <div class="col-5 fl">
                                <asp:TextBox ID="txtSpoc" runat="server" CssClass="col-10">
                                </asp:TextBox>
                            </div>
                            <div class="col-1 fl">
                                <span class="Span_Label">Spoc email: </span>
                            </div>
                            <div class="col-3 fl">
                                <asp:TextBox ID="txtSpocEmail" runat="server" CssClass="col-10">
                                </asp:TextBox>
                            </div>
                            <div class="div_clear"></div>
                            <div class="col-2 fl">
                                <span class="Span_Label">Associated Name:</span>
                            </div>
                            <div class="col-5 fl">
                                <asp:TextBox ID="txtAssociatedName" runat="server" CssClass="col-10">
                                </asp:TextBox>
                            </div>
                            <div class="col-1 fl">
                                <span class="Span_Label">Associated Email: </span>
                            </div>
                            <div class="col-3 fl">
                                <asp:TextBox ID="txtAssociatedEmail" runat="server" CssClass="col-10">
                                </asp:TextBox>
                            </div>
                            <div class="row">
                                <div class="col-2 fl">
                                    <span class="Span_Label">Order By: </span>
                                </div>
                                <div class="col-5 fl">
                                    <asp:TextBox ID="txtOredrBy" CssClass="col-10" MaxLength="100" runat="server"></asp:TextBox>
                                </div>
                            </div>
                            <div class="div_clear"></div>

                            <asp:Panel ID="pnlShippingOption" runat="server">
                                <legend><b>Shipping Service Method</b></legend>
                                <asp:Panel ID="pnlCOD" runat="server" Visible="false">
                                    <div class="col-2 fl">
                                        <span class="Span_Label">No Shipping Required: </span>
                                    </div>
                                    <div class="col-10 fl">
                                        <asp:CheckBox ID="chkNoShippingRequired" runat="server" CssClass="col-10" OnCheckedChanged="chkNoShippingRequired_CheckedChanged" AutoPostBack="true"></asp:CheckBox>
                                    </div>
                                    <div class="div_clear"></div>
                                </asp:Panel>
                                <asp:Panel ID="pnlShippingAddress" runat="server">
                                    <div class="col-2 fl">
                                        <span class="Span_Label">Shipping address: </span>
                                    </div>
                                    <div class="col-10 fl">
                                        <asp:DropDownList ID="ddlShippingAddress" runat="server" CssClass="col-10"
                                            OnSelectedIndexChanged="ddlShippingAddress_SelectedIndexChanged" AutoPostBack="false"
                                            onchange="return validateShippingOption()">
                                        </asp:DropDownList><br />
                                        <asp:RequiredFieldValidator ID="rfvShippingAddress" runat="server" ControlToValidate="ddlShippingAddress" CssClass="error-msg"
                                            Display="Dynamic" ErrorMessage="Please select shipping address" ValidationGroup="OrderInfo" InitialValue="0"></asp:RequiredFieldValidator>
                                    </div>
                                    <div class="div_clear"></div>
                                </asp:Panel>
                                <asp:Panel ID="pnlShippingOptionDDL" runat="server">
                                    <div class="col-2 fl">
                                        <span class="Span_Label">Shipping option: </span>
                                    </div>
                                    <div class="col-10 fl">
                                        <asp:DropDownList ID="ddlShippingOption" runat="server" CssClass="col-10" onchange="validateShippingOption()">
                                        </asp:DropDownList><br />
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="ddlShippingOption" CssClass="error-msg"
                                            Display="Dynamic" ErrorMessage="Please select shipping option" ValidationGroup="OrderInfo" InitialValue="0"></asp:RequiredFieldValidator>
                                    </div>
                                    <div class="div_clear"></div>
                                </asp:Panel>
                                <asp:Panel ID="pnlShippingSales" runat="server" Visible="false">
                                    <div class="col-2 fl">
                                        <span class="Span_Label">Shipping Tracking Number:</span>
                                    </div>
                                    <div class="col-5 fl">
                                        <asp:TextBox ID="txtShippingTrackingNumber" runat="server" CssClass="col-10">
                                        </asp:TextBox>
                                        <!-- onblur="askSendNotfClient(this)" -->
                                        <br />
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtShippingTrackingNumber" CssClass="error-msg" Enabled="false"
                                            Display="Dynamic" ErrorMessage="Please enter shipping tracking number" ValidationGroup="NewLine" InitialValue=""></asp:RequiredFieldValidator>
                                    </div>
                                    <div class="col-1 fl">
                                        <span class="Span_Label">Shipping Method: </span>
                                    </div>
                                    <div class="col-3 fl">
                                        <asp:DropDownList ID="ddlShippingMethod" runat="server" CssClass="col-10">
                                        </asp:DropDownList><br />
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="ddlShippingMethod" CssClass="error-msg" Enabled="false"
                                            Display="Dynamic" ErrorMessage="Please select shipping method" ValidationGroup="NewLine" InitialValue="0"></asp:RequiredFieldValidator>
                                    </div>

                                    <div class="div_clear"></div>
                                </asp:Panel>
                                <div class="div_clear"></div>
                            </asp:Panel>


                            <div class="row" id="pnlShippingInfo" runat="server" visible="false">
                                <%--<uc2:ShippingServiceMethod ID="ShippingServiceMethod" runat="server" ></uc2:ShippingServiceMethod>--%>
                                <fieldset id="fldShippingLabel" runat="server" visible="true">

                                    <div class=" fl padding-btn padding-top5 padding-bottom5 col-12">
                                        <div class="col-3 fl">
                                            <asp:Button ID="btnCreateShippingLabel" runat="server" Text="Create Shipping Label" OnClientClick="return checkShippingLabel()"
                                                CssClass="button_Update" OnClick="btnCreateShippingLabel_Click" />
                                        </div>
                                        <div class="col-3 fr">
                                            <asp:Label ID="LblDeviceWeight" runat="server" Text="Device Weight"></asp:Label>
                                            <asp:TextBox ID="txtDeviceWeight" CssClass="col-2" runat="server" ClientIDMode="Static"></asp:TextBox>
                                        </div>
                                        <div class="col-3 fl">
                                            Shipping Option: 
                                    <asp:DropDownList ID="ddlShippingOptionLabel" CssClass="txt_Box150" runat="server" AutoPostBack="false">

                                        <asp:ListItem Text="USPS Priority Mail" Value="usps_priority_mail"></asp:ListItem>
                                        <asp:ListItem Text="USPS First Class Mail" Value="usps_first_class_mail"></asp:ListItem>
                                        <asp:ListItem Text="UPS® Ground" Value="ups_ground"></asp:ListItem>
                                        <asp:ListItem Text="USPS Ground Advantage™" Value="usps_ground_advantage"></asp:ListItem>
                                        <asp:ListItem Text="UPS 3 Day Select®" Value="ups_3_day_select"></asp:ListItem>
                                        <asp:ListItem Text="UPS 2nd Day Air®" Value="ups_2nd_day_air"></asp:ListItem>
                                        <asp:ListItem Text="UPS Next Day Air®" Value="ups_next_day_air"></asp:ListItem>
                                        <asp:ListItem Text="IN PERSON PICKUP" Value="IN PERSON PICKUP"></asp:ListItem>
                                        <asp:ListItem Text="No label required" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                        </div>
                                        <div class="col-1 fl">
                                            <asp:Label ID="LblBoxHight" runat="server" Text="Height"></asp:Label>
                                            <asp:TextBox ID="txtBoxHight" TextMode="Number" min="0" runat="server" Text="5" class="col-4" OnTextChanged="txtBoxDimension_TextChanged"></asp:TextBox>
                                        </div>
                                        <div class="col-1 fl">
                                            <asp:Label ID="LblBoxLenght" runat="server" Text="Length"></asp:Label>
                                            <asp:TextBox ID="txtBoxLength" TextMode="Number" min="0" runat="server" Text="5" class="col-4" OnTextChanged="txtBoxDimension_TextChanged"></asp:TextBox>
                                        </div>
                                        <div class="col-1 fl">
                                            <asp:Label ID="LblBoxWidth" runat="server" Text="Width"></asp:Label>
                                            <asp:TextBox ID="txtBoxWidth" TextMode="Number" min="0" runat="server" Text="5" class="col-4" OnTextChanged="txtBoxDimension_TextChanged"></asp:TextBox>
                                        </div>

                                        <div class="row  col-2 fr">
                                            <div class="col-2 fl "></div>
                                            <div class="col-12 fr">
                                                <asp:CheckBox ID="chkSignatureRequired" runat="server" Text="Signature Required" />
                                            </div>
                                        </div>
                                        <asp:UpdateProgress ID="UpdateProgress3" runat="server">
                                            <ProgressTemplate>
                                                <img class="progress_mid" src="Images/progressbar.gif" alt="Loading" />
                                            </ProgressTemplate>
                                        </asp:UpdateProgress>
                                    </div>

                                    <div class="div_clear"></div>
                                    <div style="display: block">
                                        <asp:HiddenField ID="hidTotalRecordsShipping" runat="server" />
                                        <asp:GridView ID="grdShippingLabel" runat="server" PageSize="2" AutoGenerateColumns="false" ShowHeaderWhenEmpty="true"
                                            Width="100%" AllowPaging="true" AllowSorting="true" OnRowCommand="grdShippingLabel_RowCommand"
                                            OnRowDataBound="grdShippingLabel_RowDataBound" OnRowEditing="grdShippingLabel_RowEditing"
                                            DataKeyNames="AttachmentId,PrimaryId" OnRowDeleting="grdShippingLabel_RowDeleting" CssClass="grid_column" PagerSettings-Visible="true"
                                            PagerSettings-Position="Bottom" OnPageIndexChanging="grdShippingLabel_PageIndexChanging"
                                            OnRowUpdating="grdShippingLabel_RowUpdating">
                                            <EmptyDataTemplate>
                                                No records found!
                                            </EmptyDataTemplate>
                                            <EmptyDataRowStyle HorizontalAlign="Center" />
                                            <Columns>
                                                <asp:TemplateField HeaderText="S.No." Visible="true" ItemStyle-Width="5%" HeaderStyle-HorizontalAlign="Center"
                                                    ItemStyle-HorizontalAlign="Right">
                                                    <ItemTemplate>
                                                        Attachment #<%# Container.DataItemIndex+1 %><asp:HiddenField ID="hdngProposalId" runat="server" Value='<%#Eval("AttachmentId")%>' />
                                                        <asp:HiddenField ID="hdngSalePipelineId" runat="server" Value='<%#Eval("AttachmentId")%>' />
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
                                                        <asp:HiddenField ID="hdnDocumentPath" runat="server" Value='<%#Eval("DocumentPath")%>' />
                                                        <a href='<%#Eval("DocumentPath")%>' target="_blank"><%#Eval("FileName")%></a>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left" HeaderText="Created Date">
                                                    <ItemTemplate>
                                                        <asp:Literal ID="lnkCreatedDate" runat="server" Text='<%# Eval("CreatedDate").ToString() %>'></asp:Literal>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="14%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                                    <HeaderTemplate>
                                                        Action
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <a href='<%#Eval("DocumentPath")%>' target="_blank">Open and Print</a>
                                                        <asp:LinkButton ID="lnkDeleteAttachment" runat="server" CommandName="DeleteData" Visible="true"
                                                            CommandArgument='<%# Eval("AttachmentId").ToString() +"|"+ Eval("ShippingTrackingId").ToString()%>'
                                                            OnClientClick="return confirm('Are you sure to delete?');"> / Delete </asp:LinkButton>
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
                                                            <uc1:PagingControl ID="PagingPhone" OnPaging_Click="PagingPhone_Click" FirstString="<< First"
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
                                    </div>
                                </fieldset>
                            </div>
                    </div>
                    <div class="row">
                        <fieldset>
                            <legend class="bold">Order Emails</legend>
                            <asp:TextBox ID="txtOrderEmails" runat="server" CssClass="col-12" Rows="3" Columns="50" TextMode="MultiLine" onblur="return checkForExludedEmails(this.id)"></asp:TextBox>
                            Separate emails by (,) comma.
                                <asp:HiddenField ID="hdnIsValidOrderEmail" runat="server" />
                        </fieldset>
                    </div>
                    <div class="row">
                        <fieldset>
                            <legend class="bold">Order Instructions</legend>
                            <asp:TextBox ID="txtOrderInstructions" runat="server" CssClass="col-12" Rows="3" Columns="50" TextMode="MultiLine"></asp:TextBox>
                        </fieldset>
                    </div>

                    <div class="row">
                        <fieldset>
                            <legend class="bold">Order Information</legend>
                            <asp:HiddenField ID="hdnItemsCount" runat="server" Value="0" />
                            <asp:Repeater ID="rptLine" runat="server" OnItemDataBound="rptLine_ItemDataBound" OnItemCreated="rptLine_ItemCreated">
                                <ItemTemplate>
                                    <div>
                                        <asp:HiddenField ID="hdnDeviceLeaseId" runat="server" Value='<%# Eval("DeviceLeaseId")%>' />
                                        <asp:HiddenField ID="hdnOrderDetailId" runat="server" Value='<%# Eval("OrderDetailId")%>' />
                                        <asp:HiddenField ID="hdnDeviceWeight" runat="server" Value='<%# Eval("DeviceWeight")%>' />
                                        <div class="col-12 fl AlternateItem" style="display: none">
                                            <span class="col-10 Span_Label bold" style="margin-top: -11px;">Line                                                         
                                                        <asp:Label ID="lblLineNumber" runat="server" Text='<%# (Container.ItemIndex + 1) %>'></asp:Label>
                                                -
                                                        <%--<asp:HiddenField ID="hdnOrderStatusSub" runat="server" Value='<%# Eval("OrderStatusSub") %>'></asp:HiddenField>--%>
                                                <asp:Label ID="lblOrderStatus" runat="server" Text='<%# Eval("OrderStatus") %>'></asp:Label>

                                            </span>
                                            <div class="fr">
                                                <asp:Button ID="btnDeleteLine" runat="server" Text="Delete Line" CommandName="DeleteLine"
                                                    CommandArgument='<%# (Container.ItemIndex) %>' CssClass="button_Update" OnClick="btnDeleteLine_Click" />
                                            </div>
                                        </div>
                                        <div class="div_clear"></div>
                                        <asp:Label ID="lblErrorMessage" runat="server" CssClass="error-msg"></asp:Label>
                                        <div class="div_clear"></div>
                                        <div class="col-1 fl">
                                            <span class="Span_Label">Port In:</span>
                                            <asp:DropDownList ID="ddlPortIn" runat="server" onchange="SetPortIn(this.id)">
                                                <asp:ListItem Text="No" Value="0"></asp:ListItem>
                                                <asp:ListItem Text="Yes" Value="1"></asp:ListItem>
                                            </asp:DropDownList>
                                        </div>
                                        <div id="divAreaCode" runat="server">
                                            <div class="col-2 fl">
                                                <span class="Span_Label" id="lblAreaCode" runat="server">Area Code:</span>
                                                <asp:TextBox ID="txtAreaCode" runat="server" CssClass="col-10" Text='<%# Eval("MobileNo")%>'>
                                                </asp:TextBox>
                                                <ajaxToolkit:MaskedEditExtender ID="mskdDivPhone" runat="server" Enabled="True" Mask="999"
                                                    MaskType="None" TargetControlID="txtAreaCode" ClearMaskOnLostFocus="false" />
                                            </div>
                                            <div class="col-2 fl">
                                            </div>
                                        </div>
                                        <div id="divMobile" runat="server">
                                            <div class="col-2 fl">
                                                <span class="Span_Label" id="lblMobileNo" runat="server">Mobile No:</span>
                                                <asp:TextBox ID="txtMobileNo" runat="server" CssClass="col-10" Text='<%# Eval("MobileNo")%>' MaxLength="10">
                                                </asp:TextBox>
                                                <ajaxToolkit:MaskedEditExtender ID="mskdDivMobile" runat="server" Enabled="True" Mask="(999)999-9999"
                                                    MaskType="None" TargetControlID="txtMobileNo" ClearMaskOnLostFocus="false" />
                                                <br />
                                                <ajaxToolkit:MaskedEditValidator ID="MaskedEditValidator1" runat="server" ControlExtender="mskdDivMobile"
                                                    ControlToValidate="txtMobileNo" Display="Dynamic" EmptyValueMessage="Please enter mobile no" CssClass="error-msg"
                                                    InvalidValueMessage="Please Enter Mobile" ErrorMessage="Please enter valid mobile no" IsValidEmpty="False"
                                                    ValidationGroup="NewLine" ValidationExpression="((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}"></ajaxToolkit:MaskedEditValidator>

                                            </div>
                                        </div>
                                        <div class="col-3 fl">
                                            <span class="Span_Label">User name: </span>
                                            <asp:TextBox ID="txtUserName" runat="server" CssClass="col-10" Text='<%# Eval("UserName")%>' ValidationGroup="OrderInfo">
                                            </asp:TextBox>
                                            <asp:RequiredFieldValidator ID="rftxtUserName" runat="server" ControlToValidate="txtUserName" CssClass="error-msg"
                                                Display="Dynamic" ErrorMessage="Please enter username" ValidationGroup="OrderInfo" InitialValue=""></asp:RequiredFieldValidator>
                                        </div>
                                        <div class="col-3 fl">
                                            <span class="Span_Label">Contract:</span>
                                            <asp:DropDownList ID="ddlContract" runat="server" CssClass="col-10" ValidationGroup="OrderInfo">
                                            </asp:DropDownList>
                                            <asp:RequiredFieldValidator ID="rfddlContract" runat="server" ControlToValidate="ddlContract" CssClass="error-msg"
                                                Display="Dynamic" ErrorMessage="Please select contract" ValidationGroup="OrderInfo" InitialValue="0"></asp:RequiredFieldValidator>
                                        </div>
                                        <div class="col-3 fl">
                                            <span class="Span_Label">Rate plan:</span>
                                            <asp:DropDownList ID="ddldivRatePlan" runat="server" CssClass="col-10" ValidationGroup="OrderInfo" AutoPostBack="true"
                                                OnSelectedIndexChanged="ddldivRatePlan_SelectedIndexChanged" CommandArgument='<%# (Container.ItemIndex) %>'>
                                            </asp:DropDownList>
                                            <asp:RequiredFieldValidator ID="rfddldivRatePlan" runat="server" ControlToValidate="ddldivRatePlan" CssClass="error-msg"
                                                Display="Dynamic" ErrorMessage="Please select rate plan" ValidationGroup="OrderInfo" InitialValue="0"></asp:RequiredFieldValidator>
                                        </div>
                                        <div class="div_clear"></div>
                                        <div class="col-2 fl">
                                            <span class="Span_Label">Make:</span>
                                            <asp:DropDownList ID="ddlMake" runat="server" CssClass="col-10" AutoPostBack="true" ValidationGroup="OrderInfo"
                                                OnSelectedIndexChanged="ddlMake_SelectedIndexChanged">
                                            </asp:DropDownList>
                                            <asp:RequiredFieldValidator ID="rfddlMake" runat="server" ControlToValidate="ddlMake" CssClass="error-msg"
                                                Display="Dynamic" ErrorMessage="Please select make" ValidationGroup="OrderInfo" InitialValue="0"></asp:RequiredFieldValidator>
                                        </div>
                                        <div class="col-3 fl">
                                            <span class="Span_Label">Model: </span>

                                            <asp:DropDownList ID="ddlModel" runat="server" CssClass="col-10" ValidationGroup="OrderInfo" AutoPostBack="true"
                                                OnSelectedIndexChanged="ddlModel_SelectedIndexChanged">
                                            </asp:DropDownList>
                                            <asp:RequiredFieldValidator ID="rfddlModel" runat="server" ControlToValidate="ddlModel" CssClass="error-msg"
                                                Display="Dynamic" ErrorMessage="Please select model" ValidationGroup="OrderInfo" InitialValue="0"></asp:RequiredFieldValidator>
                                        </div>
                                        <div class="col-3 fl">
                                            <span class="Span_Label">Feature Add on's: </span>

                                            <asp:DropDownList ID="ddlFeatures" runat="server" CssClass="col-10">
                                            </asp:DropDownList>
                                        </div>
                                        <div class="col-3 fl">
                                            <span class="Span_Label">Monthly Cost: </span>

                                            <asp:TextBox ID="txtMonthlyCost" runat="server" CssClass="col-10" ReadOnly="true" Text='<%# Eval("MonthlyCost")%>'>
                                            </asp:TextBox>
                                        </div>
                                        <div class="div_clear"></div>
                                        <div id="divManageLines" runat="server">
                                            <div class="col-1 fl">
                                                <span class="Span_Label">MEID/IMEI:</span>
                                            </div>
                                            <div class="col-4 fl">
                                                <asp:TextBox ID="txtMEIDIMEI" runat="server" CssClass="col-10" TextMode="SingleLine"
                                                    Text='<%# Eval("MEID_IMEI")%>' ValidationGroup="OrderInfo" MaxLength="35">
                                                </asp:TextBox><br />
                                                <ajaxToolkit:FilteredTextBoxExtender ID="FilteredTextBoxExtender1" runat="server"
                                                    FilterType="Numbers" TargetControlID="txtMEIDIMEI"></ajaxToolkit:FilteredTextBoxExtender>
                                                <asp:RequiredFieldValidator ID="rftxtMEIDIMEI" runat="server" ControlToValidate="txtMEIDIMEI" CssClass="error-msg"
                                                    Display="Dynamic" ErrorMessage="Please enter MEID/IMEI" ValidationGroup="OrderInfo" InitialValue=""></asp:RequiredFieldValidator>
                                            </div>
                                            <div class="col-2 fl">
                                                <span class="Span_Label">Sim ID:</span>
                                            </div>
                                            <div class="col-4 fl">
                                                <asp:TextBox ID="txtSimID" runat="server" CssClass="col-10" TextMode="SingleLine"
                                                    Text='<%# Eval("SIMID")%>' ValidationGroup="OrderInfo" MaxLength="35">
                                                </asp:TextBox><br />
                                                <ajaxToolkit:FilteredTextBoxExtender ID="FilteredTextBoxExtender6" runat="server"
                                                    FilterType="Numbers" TargetControlID="txtSimID"></ajaxToolkit:FilteredTextBoxExtender>
                                                <asp:RequiredFieldValidator ID="rftxtSimID" runat="server" ControlToValidate="txtSimID" CssClass="error-msg"
                                                    Display="Dynamic" ErrorMessage="Please enter Sim Id" ValidationGroup="OrderInfo" InitialValue=""></asp:RequiredFieldValidator>
                                            </div>
                                            `                                           
                                        </div>
                                        <div class="div_clear"></div>
                                        <div class="div_clear"></div>
                                        <div class="col-1 fl">
                                            <span class="Span_Label">Act notes:</span>
                                        </div>
                                        <div class="col-4 fl">
                                            <asp:TextBox ID="txtActNotes" runat="server" CssClass="col-10" Rows="3" Columns="50" TextMode="MultiLine"
                                                Text='<%# Eval("ActOrderInternalNotes")%>' ValidationGroup="OrderInfo">
                                            </asp:TextBox>
                                            <asp:RequiredFieldValidator ID="rftxtActNotes" runat="server" ControlToValidate="txtActNotes" CssClass="error-msg"
                                                Display="Dynamic" ErrorMessage="*" ValidationGroup="OrderInfo" InitialValue="" Enabled="false"></asp:RequiredFieldValidator>
                                        </div>
                                        <div class="col-2 fl">
                                            <span class="Span_Label">Line notes/Instruction:</span>
                                        </div>
                                        <div class="col-4 fl">
                                            <asp:TextBox ID="txtLineNotes" runat="server" CssClass="col-10" Rows="3" Columns="50" TextMode="MultiLine"
                                                Text='<%# Eval("LineNotesInstruction")%>' ValidationGroup="OrderInfo">
                                            </asp:TextBox><br />
                                            <asp:RequiredFieldValidator ID="rftxtLineNotes" runat="server" ControlToValidate="txtLineNotes" CssClass="error-msg"
                                                Display="Dynamic" ErrorMessage="*" ValidationGroup="OrderInfo" InitialValue="" Enabled="false"></asp:RequiredFieldValidator>
                                        </div>

                                        <div class="div_clear"></div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>

                            <div class="col-12 fl">
                                <asp:Button ID="btnAddNewLine" runat="server" Text="Add" CssClass="button_Update"
                                    OnClick="btnAddNewLine_Click" ValidationGroup="OrderInfo" OnClientClick="validateControls()" />

                                <asp:Button ID="btnCopytoNewLine" runat="server" Text="Copy To New Line" CssClass="button_Update"
                                    OnClick="btnCopytoNewLine_Click" ValidationGroup="OrderInfo" OnClientClick="validateControls('copy')" />
                                <asp:TextBox ID="txtNoOfCopy" runat="server" CssClass="col-1" Style="float: left; width: 2.33% !important; margin-top: 3px;"
                                    TextMode="SingleLine"
                                    Text="1" MaxLength="2"> </asp:TextBox>
                                <ajaxToolkit:FilteredTextBoxExtender ID="FltTxtNoOfCopy" runat="server"
                                    FilterType="Numbers" TargetControlID="txtNoOfCopy"></ajaxToolkit:FilteredTextBoxExtender>

                                <asp:Button ID="btnUpdateMultiDetails" runat="server" Text="Update Selected Records" CssClass="button_Update" Style="display: none; margin-left: 20px;"
                                    OnClick="btnUpdateMultiDetails_Click" />
                            </div>
                            <div class="div_clear"></div>

                            <div class="col-12 fl">
                                <asp:GridView ID="grdOrderInfo" runat="server" PageSize="100" AutoGenerateColumns="false" ShowHeaderWhenEmpty="true"
                                    Width="100%" AllowPaging="true" AllowSorting="true" OnRowCommand="grdOrderInfo_RowCommand"
                                    OnRowDataBound="grdOrderInfo_RowDataBound" OnRowEditing="grdOrderInfo_RowEditing"
                                    DataKeyNames="OrderDetailId" CssClass="grid_column" PagerSettings-Visible="true"
                                    PagerSettings-Position="Bottom" OnPageIndexChanging="grdOrderInfo_PageIndexChanging"
                                    OnRowUpdating="grdOrderInfo_RowUpdating" EmptyDataText="No attachments found." OnSelectedIndexChanged="grdOrderInfo_SelectedIndexChanged">
                                    <Columns>
                                        <asp:TemplateField HeaderText="" Visible="true" ItemStyle-Width="5%" HeaderStyle-HorizontalAlign="Center"
                                            ItemStyle-HorizontalAlign="Right">
                                            <HeaderTemplate>
                                                <asp:CheckBox ID="checkAll" runat="server" onclick="checkAll(this);" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkOrderDetail" onclick="chkOrderDetail_clicked()" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Line" Visible="true" ItemStyle-Width="5%" HeaderStyle-HorizontalAlign="Center"
                                            ItemStyle-HorizontalAlign="Right">
                                            <ItemTemplate>
                                                <asp:Label ID="lblRowNumber" runat="server"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Port In" Visible="true" ItemStyle-Width="5%" HeaderStyle-HorizontalAlign="Center"
                                            ItemStyle-HorizontalAlign="Right">
                                            <ItemTemplate>
                                               <asp:Label ID="lblPortIn" runat="server" Text='<%# Eval("Port") %>'></asp:Label>
                                                <asp:HiddenField ID="hdnDeviceLeaseId" runat="server" Value='<%# Eval("DeviceLeaseId")%>' />
                                                <asp:HiddenField ID="hdnOrderDetailId" runat="server" Value='<%# Eval("OrderDetailId")%>' />
                                                <asp:HiddenField ID="hdnDeviceWeight" runat="server" Value='<%# Eval("DeviceWeight")%>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="DeviceWeight" Visible="False" ItemStyle-Width="1%" HeaderStyle-HorizontalAlign="Center"
                                            ItemStyle-HorizontalAlign="Right" ShowHeader="False">
                                            <ItemTemplate>
                                                <asp:Label ID="hidnDeviceWeight" Visible="False" runat="server" Text='<%# Eval("DeviceWeight")%>'></asp:Label>

                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                            <HeaderTemplate>
                                                <a href="javascript:PhoneSort.ChangePhoneSort('MobileNo')"><font color="black">Area Code</font></a>
                                                <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                    id="img_mobileno_asc" style="border: 0; display: none" />
                                                <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                    id="img_mobileno_desc" style="border: 0; display: none" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="lblMobileNo" runat="server" Text='<%# Eval("MobileNo") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                            <HeaderTemplate>
                                                <a href="javascript:PhoneSort.ChangePhoneSort('UserName')"><font color="black">User Name</font></a>
                                                <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                    id="img_username_asc" style="border: 0; display: none" />
                                                <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                    id="img_username_desc" style="border: 0; display: none" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="lblUserName" runat="server" Text='<%# Eval("UserName") %>'></asp:Label>
                                            </ItemTemplate>

                                        </asp:TemplateField>

                                        <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                            <HeaderTemplate>
                                                <a href="javascript:PhoneSort.ChangePhoneSort('Contract')"><font color="black">Contract</font></a>
                                                <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                    id="img_contractid_asc" style="border: 0; display: none" />
                                                <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                    id="img_contractid_desc" style="border: 0; display: none" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="lblContractId" runat="server" Text='<%# Eval("Contract") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                            <HeaderTemplate>
                                                <a href="javascript:PhoneSort.ChangePhoneSort('RatePlans')"><font color="black">Rate plan</font></a>
                                                <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                    id="img_contractid_asc" style="border: 0; display: none" />
                                                <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                    id="img_contractid_desc" style="border: 0; display: none" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="lblRatePlan" runat="server" Text='<%# Eval("RatePlans") %>'></asp:Label>
                                                <asp:HiddenField ID="hdnRatePlanId" runat="server" Value='<%# Eval("RatePlanId")%>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                            <HeaderTemplate>
                                                <a href="javascript:PhoneSort.ChangePhoneSort('Make')"><font color="black">Make</font></a>
                                                <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                    id="img_contractid_asc" style="border: 0; display: none" />
                                                <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                    id="img_contractid_desc" style="border: 0; display: none" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="lblMake" runat="server" Text='<%# Eval("Make") %>'></asp:Label>
                                                <asp:HiddenField ID="hdnMakeId" runat="server" Value='<%# Eval("MakeId")%>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                            <HeaderTemplate>
                                                <a href="javascript:PhoneSort.ChangePhoneSort('Model')"><font color="black">Model</font></a>
                                                <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                    id="img_contractid_asc" style="border: 0; display: none" />
                                                <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                    id="img_contractid_desc" style="border: 0; display: none" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="lblModel" runat="server" Text='<%# Eval("Model") %>'></asp:Label>
                                                <asp:HiddenField ID="hdnModelId" runat="server" Value='<%# Eval("ModelId")%>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                            <HeaderTemplate>
                                                <a href="javascript:PhoneSort.ChangePhoneSort('Features')"><font color="black">Feature Add on's</font></a>
                                                <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                    id="img_contractid_asc" style="border: 0; display: none" />
                                                <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                    id="img_contractid_desc" style="border: 0; display: none" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="lblFeature" runat="server" Text='<%# Eval("Features") %>'></asp:Label>
                                                <asp:HiddenField ID="hdnFeatureId" runat="server" Value='<%# Eval("FeatureId")%>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                            <HeaderTemplate>
                                                <a href="javascript:PhoneSort.ChangePhoneSort('MonthlyCost')"><font color="black">Monthly Cost</font></a>
                                                <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                    id="img_contractid_asc" style="border: 0; display: none" />
                                                <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                    id="img_contractid_desc" style="border: 0; display: none" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="lblMonthlyCost" runat="server" Text='<%# Eval("MonthlyCost") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left" Visible="false">
                                            <HeaderTemplate>
                                                <a href="javascript:PhoneSort.ChangePhoneSort('MEID_IMEI')"><font color="black">MEID/IMEI</font></a>
                                                <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                    id="img_contractid_asc" style="border: 0; display: none" />
                                                <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                    id="img_contractid_desc" style="border: 0; display: none" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="lblMEID_IMEI" runat="server" Text='<%# Eval("MEID_IMEI") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left" Visible="false">
                                            <HeaderTemplate>
                                                <a href="javascript:PhoneSort.ChangePhoneSort('SIMID')"><font color="black">Sim ID</font></a>
                                                <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                    id="img_contractid_asc" style="border: 0; display: none" />
                                                <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                    id="img_contractid_desc" style="border: 0; display: none" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="lblSIMID" runat="server" Text='<%# Eval("SIMID") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                            <HeaderTemplate>
                                                <a href="javascript:PhoneSort.ChangePhoneSort('ActOrderInternalNotes')"><font color="black">Act notes</font></a>
                                                <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                    id="img_contractid_asc" style="border: 0; display: none" />
                                                <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                    id="img_contractid_desc" style="border: 0; display: none" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="lblActOrderInternalNotes" runat="server" Text='<%# Eval("ActOrderInternalNotes") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField ItemStyle-Width="15%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Left">
                                            <HeaderTemplate>
                                                <a href="javascript:PhoneSort.ChangePhoneSort('LineNotesInstruction')"><font color="black">Line notes/Instruction</font></a>
                                                <img alt="Ascending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_up.gif'
                                                    id="img_contractid_asc" style="border: 0; display: none" />
                                                <img alt="Desending" src='<%=Classes.Common.GetBaseURL%>admin/Images/sort_dn.gif'
                                                    id="img_contractid_desc" style="border: 0; display: none" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="lblLineNotesInstruction" runat="server" Text='<%# Eval("LineNotesInstruction") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField ItemStyle-Width="14%" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                            <HeaderTemplate>
                                                Action
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:LinkButton ID="lnkEditOrderDetail" runat="server" CommandName="Edit" Visible="true"
                                                    CommandArgument='<%# Eval("OrderDetailId").ToString() %>'> Edit </asp:LinkButton>
                                                <asp:LinkButton ID="lnkDeleteOrderDetail" runat="server" CommandName="DeleteData" Visible="true"
                                                    CommandArgument='<%# Eval("OrderDetailId").ToString() %>'
                                                    OnClientClick="return confirm('Are you sure to delete?');"> | Delete </asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                    <PagerTemplate>
                                        <table cellpadding="0" cellspacing="0" width="100%" border="0">
                                            <tr>
                                                <td>
                                                    <asp:Label ID="lblPagingSummaryOrderInfo" CssClass="pagingLabelText" runat="server"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <uc1:PagingControl ID="PagingGrdOrderInfo" OnPaging_Click="Paging_Click" FirstString="<< First"
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
                            </div>

                            <div style="display: none;">
                                <asp:Button ID="btnMultiItemUpdatePopup" runat="server" />
                            </div>
                            <div>
                                <ajaxToolkit:ModalPopupExtender ID="mdlMultiItemUpdateForm" runat="server" BehaviorID="mdlMultiItemUpdateForm"
                                    TargetControlID="btnMultiItemUpdatePopup" PopupControlID="pnlEdit" BackgroundCssClass="modalBackground"
                                    CancelControlID="btnFirstPopUpCan">
                                </ajaxToolkit:ModalPopupExtender>
                            </div>
                            <div id="divEdit">
                                <center>
                                    <asp:Panel runat="server" ID="pnlEdit">
                                        <div class="main_input-popup-1" style="float: none; margin: 0 auto; width: 1050px; height: 600px;">
                                            <div class="popup-heading">
                                                Multi Line Edit
                                            </div>
                                            <div style="width: 1030px; margin-top: 40px; padding: 0 10px; height: 550px; overflow: auto; background-color: #D4FAFA; color: #333333; font-size: 12px; font-family: Arial;">

                                                <div class="clr">
                                                </div>
                                                <asp:Button ID="btnSaveMultiItems" runat="server" CssClass="button_Update-save" Text="Save" ValidationGroup="OrderInfo1" CausesValidation="true"
                                                    OnClick="btnSaveMultiItems_Click" />
                                                <asp:Button ID="btnFirstPopUpCan" runat="server" CssClass="button_Close" Text="X Close" OnClick="btnFirstPopUpCan_Click" />
                                                <asp:HiddenField ID="hdnIsMultiEditBtn" runat="server" Value="0" />
                                                <div class="clr">
                                                </div>
                                                <asp:Repeater ID="rptLineItems" runat="server" OnItemDataBound="rptLine_ItemDataBound" OnItemCreated="rptLine_ItemCreated">
                                                    <ItemTemplate>
                                                        <div>
                                                            <asp:HiddenField ID="hdnDeviceLeaseId" runat="server" Value='<%# Eval("DeviceLeaseId")%>' />
                                                            <asp:HiddenField ID="hdnOrderDetailId" runat="server" Value='<%# Eval("OrderDetailId")%>' />
                                                            <asp:HiddenField ID="hdnDeviceWeight" runat="server" Value='<%# Eval("DeviceWeight")%>' />
                                                            <div class="col-12 fl AlternateItem" style="height: 30px">
                                                                <span class="col-10 Span_Label bold" style="margin-top: -5px;">Line                                                         
                                                        <asp:Label ID="lblLineNumber" runat="server" Text='<%# (Container.ItemIndex + 1) %>'></asp:Label>
                                                                    -
                                                        <%--<asp:HiddenField ID="hdnOrderStatusSub" runat="server" Value='<%# Eval("OrderStatusSub") %>'></asp:HiddenField>--%>
                                                                    <asp:Label ID="lblOrderStatus" runat="server" Text='<%# Eval("OrderStatus") %>'></asp:Label>

                                                                </span>
                                                                <div class="fr" style="display: none">
                                                                    <asp:Button ID="btnDeleteLine" runat="server" Text="Delete Line" CommandName="DeleteLine"
                                                                        CommandArgument='<%# (Container.ItemIndex) %>' CssClass="button_Update" OnClick="btnDeleteLine_Click" />
                                                                </div>
                                                            </div>
                                                            <div class="div_clear"></div>
                                                            <asp:Label ID="lblErrorMessage" runat="server" CssClass="error-msg"></asp:Label>
                                                            <div class="div_clear"></div>
                                                            <div class="col-1 fl">
                                                                <span class="Span_Label">Port In:</span>
                                                                <asp:DropDownList ID="ddlPortIn" runat="server" onchange="SetPortIn(this.id)">
                                                                    <asp:ListItem Text="No" Value="0"></asp:ListItem>
                                                                    <asp:ListItem Text="Yes" Value="1"></asp:ListItem>
                                                                </asp:DropDownList>
                                                            </div>
                                                            <div id="divAreaCode" runat="server">
                                                                <div class="col-2 fl">
                                                                    <span class="Span_Label" id="lblAreaCode" runat="server">Area Code:</span>
                                                                    <asp:TextBox ID="txtAreaCode" runat="server" CssClass="col-10" Text='<%# Eval("MobileNo")%>'>
                                                                    </asp:TextBox>
                                                                    <ajaxToolkit:MaskedEditExtender ID="mskdDivPhone" runat="server" Enabled="True" Mask="999"
                                                                        MaskType="None" TargetControlID="txtAreaCode" ClearMaskOnLostFocus="false" />
                                                                </div>
                                                                <div class="col-2 fl">
                                                                </div>
                                                            </div>
                                                            <div id="divMobile" runat="server">
                                                                <div class="col-2 fl">
                                                                    <span class="Span_Label" id="lblMobileNo" runat="server">Mobile No:</span>
                                                                    <asp:TextBox ID="txtMobileNo" runat="server" CssClass="col-10" Text='<%# Eval("MobileNo")%>' MaxLength="10">
                                                                    </asp:TextBox>
                                                                    <ajaxToolkit:MaskedEditExtender ID="mskdDivMobile" runat="server" Enabled="True" Mask="(999)999-9999"
                                                                        MaskType="None" TargetControlID="txtMobileNo" ClearMaskOnLostFocus="false" />
                                                                    <br />
                                                                    <ajaxToolkit:MaskedEditValidator ID="MaskedEditValidator1" runat="server" ControlExtender="mskdDivMobile"
                                                                        ControlToValidate="txtMobileNo" Display="Dynamic" EmptyValueMessage="Please enter mobile no" CssClass="error-msg"
                                                                        InvalidValueMessage="Please Enter Mobile" ErrorMessage="Please enter valid mobile no" IsValidEmpty="False"
                                                                        ValidationGroup="NewLine" ValidationExpression="((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}"></ajaxToolkit:MaskedEditValidator>

                                                                </div>
                                                            </div>
                                                            <div class="col-3 fl">
                                                                <span class="Span_Label">User name: </span>
                                                                <asp:TextBox ID="txtUserName" runat="server" CssClass="col-10" Text='<%# Eval("UserName")%>' ValidationGroup="OrderInfo1">
                                                                </asp:TextBox>
                                                                <asp:RequiredFieldValidator ID="rftxtUserName" runat="server" ControlToValidate="txtUserName" CssClass="error-msg"
                                                                    Display="Dynamic" ErrorMessage="Please enter username" ValidationGroup="OrderInfo1" InitialValue=""></asp:RequiredFieldValidator>
                                                            </div>
                                                            <div class="col-3 fl">
                                                                <span class="Span_Label">Contract:</span>
                                                                <asp:DropDownList ID="ddlContract" runat="server" CssClass="col-10" ValidationGroup="OrderInfo1">
                                                                </asp:DropDownList>
                                                                <asp:RequiredFieldValidator ID="rfddlContract" runat="server" ControlToValidate="ddlContract" CssClass="error-msg"
                                                                    Display="Dynamic" ErrorMessage="Please select contract" ValidationGroup="OrderInfo1" InitialValue="0"></asp:RequiredFieldValidator>
                                                            </div>
                                                            <div class="col-3 fl">
                                                                <span class="Span_Label">Rate plan:</span>
                                                                <asp:DropDownList ID="ddldivRatePlan" runat="server" CssClass="col-10" ValidationGroup="OrderInfo1" AutoPostBack="false">
                                                                </asp:DropDownList>
                                                                <asp:RequiredFieldValidator ID="rfddldivRatePlan" runat="server" ControlToValidate="ddldivRatePlan" CssClass="error-msg"
                                                                    Display="Dynamic" ErrorMessage="Please select rate plan" ValidationGroup="OrderInfo1" InitialValue="0"></asp:RequiredFieldValidator>
                                                            </div>
                                                            <div class="div_clear"></div>
                                                            <div class="col-2 fl">
                                                                <span class="Span_Label">Make:</span>
                                                                <asp:DropDownList ID="ddlMake" runat="server" CssClass="col-10" AutoPostBack="false" ValidationGroup="OrderInfo1">
                                                                </asp:DropDownList>
                                                                <asp:RequiredFieldValidator ID="rfddlMake" runat="server" ControlToValidate="ddlMake" CssClass="error-msg"
                                                                    Display="Dynamic" ErrorMessage="Please select make" ValidationGroup="OrderInfo1" InitialValue="0"></asp:RequiredFieldValidator>
                                                            </div>
                                                            <div class="col-3 fl">
                                                                <span class="Span_Label">Model: </span>

                                                                <asp:DropDownList ID="ddlModel" runat="server" CssClass="col-10" ValidationGroup="OrderInfo1" AutoPostBack="false">
                                                                </asp:DropDownList>
                                                                <asp:RequiredFieldValidator ID="rfddlModel" runat="server" ControlToValidate="ddlModel" CssClass="error-msg"
                                                                    Display="Dynamic" ErrorMessage="Please select model" ValidationGroup="OrderInfo1" InitialValue="0"></asp:RequiredFieldValidator>
                                                            </div>
                                                            <div class="col-3 fl">
                                                                <span class="Span_Label">Feature Add on's: </span>

                                                                <asp:DropDownList ID="ddlFeatures" runat="server" CssClass="col-10">
                                                                </asp:DropDownList>
                                                            </div>
                                                            <div class="col-3 fl">
                                                                <span class="Span_Label">Monthly Cost: </span>

                                                                <asp:TextBox ID="txtMonthlyCost" runat="server" CssClass="col-10" ReadOnly="true" Text='<%# Eval("MonthlyCost")%>'>
                                                                </asp:TextBox>
                                                            </div>
                                                            <div class="div_clear"></div>
                                                            <div id="divManageLines" runat="server">
                                                                <div class="col-1 fl">
                                                                    <span class="Span_Label">MEID/IMEI:</span>
                                                                </div>
                                                                <div class="col-4 fl">
                                                                    <asp:TextBox ID="txtMEIDIMEI" runat="server" CssClass="col-10" TextMode="SingleLine"
                                                                        Text='<%# Eval("MEID_IMEI")%>' ValidationGroup="OrderInfo1" MaxLength="35">
                                                                    </asp:TextBox><br />
                                                                    <ajaxToolkit:FilteredTextBoxExtender ID="FilteredTextBoxExtender1" runat="server"
                                                                        FilterType="Numbers" TargetControlID="txtMEIDIMEI"></ajaxToolkit:FilteredTextBoxExtender>
                                                                    <asp:RequiredFieldValidator ID="rftxtMEIDIMEI" runat="server" ControlToValidate="txtMEIDIMEI" CssClass="error-msg"
                                                                        Display="Dynamic" ErrorMessage="Please enter MEID/IMEI" ValidationGroup="OrderInfo1" InitialValue=""></asp:RequiredFieldValidator>
                                                                </div>
                                                                <div class="col-2 fl">
                                                                    <span class="Span_Label">Sim ID:</span>
                                                                </div>
                                                                <div class="col-4 fl">
                                                                    <asp:TextBox ID="txtSimID" runat="server" CssClass="col-10" TextMode="SingleLine"
                                                                        Text='<%# Eval("SIMID")%>' ValidationGroup="OrderInfo1" MaxLength="35">
                                                                    </asp:TextBox><br />
                                                                    <ajaxToolkit:FilteredTextBoxExtender ID="FilteredTextBoxExtender6" runat="server"
                                                                        FilterType="Numbers" TargetControlID="txtSimID"></ajaxToolkit:FilteredTextBoxExtender>
                                                                    <asp:RequiredFieldValidator ID="rftxtSimID" runat="server" ControlToValidate="txtSimID" CssClass="error-msg"
                                                                        Display="Dynamic" ErrorMessage="Please enter Sim Id" ValidationGroup="OrderInfo1" InitialValue=""></asp:RequiredFieldValidator>
                                                                </div>
                                                                `                                           
                                                            </div>
                                                            <div class="div_clear"></div>
                                                            <div class="div_clear"></div>
                                                            <div class="col-1 fl">
                                                                <span class="Span_Label">Act notes:</span>
                                                            </div>
                                                            <div class="col-4 fl">
                                                                <asp:TextBox ID="txtActNotes" runat="server" CssClass="col-10" Rows="3" Columns="50" TextMode="MultiLine"
                                                                    Text='<%# Eval("ActOrderInternalNotes")%>' ValidationGroup="OrderInfo1">
                                                                </asp:TextBox>
                                                                <asp:RequiredFieldValidator ID="rftxtActNotes" runat="server" ControlToValidate="txtActNotes" CssClass="error-msg"
                                                                    Display="Dynamic" ErrorMessage="*" ValidationGroup="OrderInfo1" InitialValue="" Enabled="false"></asp:RequiredFieldValidator>
                                                            </div>
                                                            <div class="col-2 fl">
                                                                <span class="Span_Label">Line notes/Instruction:</span>
                                                            </div>
                                                            <div class="col-4 fl">
                                                                <asp:TextBox ID="txtLineNotes" runat="server" CssClass="col-10" Rows="3" Columns="50" TextMode="MultiLine"
                                                                    Text='<%# Eval("LineNotesInstruction")%>' ValidationGroup="OrderInfo1">
                                                                </asp:TextBox><br />
                                                                <asp:RequiredFieldValidator ID="rftxtLineNotes" runat="server" ControlToValidate="txtLineNotes" CssClass="error-msg"
                                                                    Display="Dynamic" ErrorMessage="*" ValidationGroup="OrderInfo1" InitialValue="" Enabled="false"></asp:RequiredFieldValidator>
                                                            </div>

                                                            <div class="div_clear"></div>
                                                        </div>
                                                    </ItemTemplate>
                                                </asp:Repeater>


                                                <div class="clr">
                                                </div>

                                            </div>
                                        </div>
                                    </asp:Panel>
                                </center>
                            </div>
                    </div>
                    <div class="row">
                        <div class="col-12 fl">
                            <asp:GridView ID="grdAttachments" runat="server" PageSize="5" AutoGenerateColumns="false" ShowHeaderWhenEmpty="true"
                                Width="100%" AllowPaging="true" AllowSorting="true" OnRowCommand="grdAttachments_RowCommand"
                                OnRowDataBound="grdAttachments_RowDataBound" OnRowEditing="grdAttachments_RowEditing"
                                DataKeyNames="PrimaryId,AttachmentId" CssClass="grid_column" PagerSettings-Visible="true"
                                PagerSettings-Position="Bottom" OnPageIndexChanging="grdAttachments_PageIndexChanging"
                                OnRowUpdating="grdAttachments_OnRowUpdating" EmptyDataText="No attachments found.">
                                <Columns>
                                    <asp:TemplateField HeaderText="Attach with Response" Visible="true" ItemStyle-Width="5%" HeaderStyle-HorizontalAlign="Center"
                                        ItemStyle-HorizontalAlign="Right">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkAttachment" runat="server" />
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
                                            <asp:Literal ID="ltrUniqueFileName" runat="server" Text='<%#Eval("UniqueFileName")%>' Visible="false"></asp:Literal>
                                            <asp:Literal ID="ltrLocalPath" runat="server" Text='<%#Eval("DocumentPath")%>' Visible="false"></asp:Literal>
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
                                            |<asp:LinkButton ID="lnkDeleteAttachment" runat="server" CommandName="DeleteData" CommandArgument='<%# Eval("AttachmentId").ToString() %>'
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
                        </div>
                        <div class="row" id="divAttachment" runat="server">
                            <div class="col-12 fl">
                                <fieldset>
                                    <legend class="error-msg bold">Attachments for Orders Only</legend>
                                    <asp:Button ID="btnAttachFile" runat="server" Text="Attach File" CssClass="fl button_Update" ValidationGroup="C11"
                                        OnClick="btnAttachFile_Click" />
                                    <asp:FileUpload ID="fUAttachment" runat="server" Width="200px" />
                                </fieldset>
                            </div>
                        </div>
                        <div class="row">
                            <div class="fl">
                                <asp:UpdateProgress ID="UpdateProgress41" runat="server">
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
                    </div>
                    <div class="row">
                        <uc2:AccessoryOrder ID="orderAccessoryOrder" runat="server"></uc2:AccessoryOrder>
                    </div>
                </div>

            </div>
        </div>
        </div>
        </div>
    </ContentTemplate>
    <Triggers>
        <asp:PostBackTrigger ControlID="btnAttachFile" />
        <asp:PostBackTrigger ControlID="btnSaveOrder" />
        <asp:PostBackTrigger ControlID="grdOrderInfo" />
    </Triggers>
</asp:UpdatePanel>

<script type="text/javascript">
    function OpenThickBox(urltb, title) {
        //debugger;
        tb_show(title, urltb, 'null');
    }
</script>

<script type="text/javascript" language="javascript">
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
    function EndRequestHandler(sender, args) {
        if (args.get_error() != undefined) {
            args.set_errorHandled(true);
        }
    }

    function SetPortIn(ctrlID) {

        var index = ctrlID.substring(ctrlID.lastIndexOf("_") + 1);
        var masterControlId = '#ContentPlaceHolder1_ucNewActivationOrder_';

        var divAreaCode = masterControlId + 'rptLine_divAreaCode_' + index;
        var divMobile = masterControlId + 'rptLine_divMobile_' + index;

        var txtAreaCode = masterControlId + 'rptLine_txtAreaCode_' + index;
        var txtMobileNo = masterControlId + 'rptLine_txtMobileNo_' + index;

        var lblAreaCode = masterControlId + 'rptLine_lblAreaCode_' + index;
        var lblMobileNo = masterControlId + 'rptLine_lblMobileNo_' + index;

        var txtLineNotes = masterControlId + 'rptLine_txtLineNotes_' + index;

        var MaskedEditValidator1 = masterControlId + 'rptLine_MaskedEditValidator1_' + index;
        //var mskdEditPhone = '#ContentPlaceHolder1_rptLine_mskdEditPhone_' + index;

        debugger;

        $(txtLineNotes).val($(txtLineNotes).val().replace("\nPort in information is required to be entered here.", ""));
        if ($('#' + ctrlID).val() == '1') {
            $(txtLineNotes).val($(txtLineNotes).val() + ' \nPort in information is required to be entered here.');
            $(divAreaCode).css('display', 'none');
            $(divMobile).css('display', 'block');
            enable(MaskedEditValidator1);
            //disable(mskdEditPhone);
        }
        else {
            $(divAreaCode).css('display', 'block');
            $(divMobile).css('display', 'none');
            disable(MaskedEditValidator1);
            //enable(mskdEditPhone);
        }
    }

    function disable(id) {
        var behavior = $find(id); // "MaskedEditExtenderEx" - BehaviorID
        // to prevent the base dispose() method call - it removes the behavior from components list
        //------------------------------------------------------------------------
        var savedDispose = AjaxControlToolkit.MaskedEditBehavior.callBaseMethod;
        AjaxControlToolkit.MaskedEditBehavior.callBaseMethod = function (instance, name) {
        };
        //------------------------------------------------------------------------
        behavior.dispose();
        // restore the base dispose() method
        AjaxControlToolkit.MaskedEditBehavior.callBaseMethod = savedDispose;
    }
    function enable(id) { // enable it again
        var behavior = $find(id); // "MaskedEditExtenderEx" - BehaviorID
        behavior.initialize();
    }
    function validateControls(caller) {

        var masterControlId = '#ContentPlaceHolder1_ucNewActivationOrder_';
        var count = $(masterControlId + 'hdnItemsCount').val();
        if (count == undefined) count = 0;
        if (caller == 'copy') {
            for (i = 0; i < count; i++) {
                var txtMEIDIMEI = masterControlId + 'rptLine_txtMEIDIMEI_' + i;
                var txtSimID = masterControlId + 'rptLine_txtSimID_' + i;
                if ($(txtMEIDIMEI).val() == '')
                    $(txtMEIDIMEI).val('0');
                if ($(txtSimID).val() == '')
                    $(txtSimID).val('0');
            }
            return true;
        }
        else {


            for (i = 0; i < count; i++) {
                var txtMobileNo = $(masterControlId + '_rptLine_txtMobileNo_' + i).val();
                var ddlPortIn = $(masterControlId + '_rptLine_ddlPortIn_' + i).val();
                if (ddlPortIn == '1' && txtMobileNo == "(___)___-____") {
                    alert('Please enter mobile no');
                    return false;
                }
            }
        }
        return true;

    }

    function validateShippingOption() {
        var ddlShippingAddress = $('#<%=ddlShippingAddress.ClientID%>'), ddlShippingOption = $('#<%=ddlShippingOption.ClientID%>');
        debugger;
        if (ddlShippingAddress != null && ddlShippingOption != null) {
            var ddlShippingAddressText = $('#' + $(ddlShippingAddress).attr('id') + ' option:selected').text()
            var ddlShippingOptionText = $('#' + $(ddlShippingOption).attr('id') + ' option:selected').text()
            /**/
            if
                ($('#ContentPlaceHolder1_orderNewActivation_lblOrderStatus').text() == 'SUBMITORDER'
                &&
                (
                    (ddlShippingAddressText.toLowerCase().indexOf('ws fulfillment') == -1 && ddlShippingOptionText.toLowerCase().indexOf('in person') > -1) ||
                    (ddlShippingAddressText.toLowerCase().indexOf('ws fulfillment') > -1 && ddlShippingOptionText.toLowerCase().indexOf('in person') == -1)
                )
            ) {
                alert('Please select Address: WS FULFILLMENT with IN PERSON PICKUP');
                $(ddlShippingOption).val('-1');
            }

        }
    }

    function chkOrderDetail_clicked() {
        debugger;
        var grdId = '<%= grdOrderInfo.ClientID %>';
        var btn = '<%= btnUpdateMultiDetails.ClientID %>';
        var checkedCount = $('#' + grdId).find('input[type="checkbox"]:checked');
        if (checkedCount.length > 1) {
            $("#" + btn).css("display", "block");
        }
        else {
            $("#" + btn).css("display", "none");
        }
    }
    function checkAll(objRef) {

        var GridView = objRef.parentNode.parentNode.parentNode;

        var inputList = GridView.getElementsByTagName("input");

        for (var i = 0; i < inputList.length; i++) {

            if (inputList[i].type == "checkbox" && objRef != inputList[i]) {

                if (objRef.checked) {
                    inputList[i].checked = true;
                }
                else {
                    inputList[i].checked = false;
                }
            }
        }
        var grdId = '<%= grdOrderInfo.ClientID %>';
        var btn = '<%= btnUpdateMultiDetails.ClientID %>';
        var checkedCount = $('#' + grdId).find('input[type="checkbox"]:checked');
        if (checkedCount.length > 1) {
            $("#" + btn).css("display", "block");
        }
        else {
            $("#" + btn).css("display", "none");
        }

    }
    function showActivationOrder(value) {
        debugger;
        if (value == 0) {
            $("#OrderActivationGain").show();
        }
        else {
            $("#OrderActivationGain").hide();
            $("#OrderActivationGain").hide();
        }

    }

    function showTotalActivationGain() {
        debugger;

        $("#TotalActivationGain").show();
        $("#txnetActivationGain").show();



    }





</script>

