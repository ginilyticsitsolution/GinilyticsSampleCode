using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ZOHO_API.Models;
using ZOHO_API.Service;

namespace ZOHO_API.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly IAccessTokenService _accessTokenService;
        private readonly IAuthorizeService _authorizeService;
        private readonly ZohoAPIContext _dbContext;
        public PurchaseController(IAccessTokenService accessTokenService, IAuthorizeService authorizeService, ZohoAPIContext dbContext)
        {
            _accessTokenService = accessTokenService;
            _authorizeService = authorizeService;
            _dbContext = dbContext;
        }
        public IActionResult PurchaseList()
        {
            var accessToken = _accessTokenService.GetAccessToken();


            ViewBag.AccessToken = accessToken;
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> FetchPurchaseOrder()
        {
            try
            {
                // Retrieve access token using the service
                var accessToken = _accessTokenService.GetAccessToken();

                using (var client = new HttpClient())
                {
                    // Set Zoho Books API proxy endpoint
                    var organizationid = _authorizeService.GetOrganizationid();
                    var proxyUrl = $"https://www.zohoapis.in/books/v3/purchaseorders?organization_id={organizationid}";

                    // Set authorization header
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                    // Make a request to the proxy endpoint
                    var response = await client.GetAsync(proxyUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();

                        // Deserialize JSON response into ZohoApiResponse object
                        var zohoApiResponse = JsonConvert.DeserializeObject<ZohoApiResponse>(responseData);

                        // Check if ZohoApiResponse is not null and contains projects
                        if (zohoApiResponse != null && zohoApiResponse.purchaseorders != null && zohoApiResponse.purchaseorders.Any())
                        {
                            // Access the ExpenseData from the response
                            var PurchaseOrderData = zohoApiResponse.purchaseorders;

                            foreach (var PurchaseData in PurchaseOrderData)
                            {
                                // Check if ExpenseData is not null
                                if (PurchaseData != null)
                                {
                                    // Check if the ExpenseData already exists in the database based on ProjectId
                                    var existingpurchaseorder = _dbContext.purchaseOrders.FirstOrDefault(p => p.purchaseorder_id == PurchaseData.purchaseorder_id);

                                    if (existingpurchaseorder != null)
                                    {
                                        // Update existing record
                                        existingpurchaseorder.purchaseorder_id= PurchaseData.purchaseorder_id;
                                        existingpurchaseorder.vendor_name = PurchaseData.vendor_name;
                                        existingpurchaseorder.purchaseorder_number = PurchaseData.purchaseorder_number;
                                        existingpurchaseorder.status = PurchaseData.status;
                                        existingpurchaseorder.total = PurchaseData.total;
                                      


                                    }
                                    else
                                    {
                                        // Add new recrd
                                        _dbContext.purchaseOrders.Add(PurchaseData);
                                    }
                                }
                            }

                            await _dbContext.SaveChangesAsync();
                        }

                        return Content(responseData, "application/json");

                    }
                    return StatusCode((int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                return StatusCode(500);
            }

        }

        public ActionResult GetPurchaseDetail(string purchaseorderid)
        {
            var Purchaseid = purchaseorderid;
            ViewBag.PurchaseId = Purchaseid;
            return View();
        }


        [HttpGet]
        public async Task<ActionResult> FetchPurchaseOrderDetail(string purchaseorderid)
        {
            try
            {
                // Retrieve access token using the service
                var accessToken = _accessTokenService.GetAccessToken();

                using (var client = new HttpClient())
                {
                    // Set Zoho Books API proxy endpoint
                    var organizationid = _authorizeService.GetOrganizationid();
                    var proxyUrl = $"https://www.zohoapis.in/books/v3/purchaseorders/{purchaseorderid}?organization_id={organizationid}";

                    // Set authorization header
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                    // Make a request to the proxy endpoint
                    var response = await client.GetAsync(proxyUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();

                        // Deserialize JSON response into expenseDetail object
                        var zohoApiResponse = JsonConvert.DeserializeObject<ZohoApiResponse>(responseData);
                        var PurchaseorderDetail = zohoApiResponse?.purchaseorder;

                        // Check if expenseDetail is not null
                        if (PurchaseorderDetail != null)
                        {
                            // Check if the billDetail already exists in the database based on bill_id
                            var existingpurchaseDetail = _dbContext.purchaseoderDetails
                                .Include(ed => ed.line_items)
                                .FirstOrDefault(p => p.purchaseorder_id == PurchaseorderDetail.purchaseorder_id);

                            // Add line items directly to billDetail
                            PurchaseorderDetail.line_items = PurchaseorderDetail.line_items ?? new List<Puchaseorderlineitems>();

                            // Process line items
                            foreach (var lineItem in PurchaseorderDetail.line_items)
                            {
                                // Update existing line item properties if needed
                                var existingLineItem = existingpurchaseDetail?.line_items
                                    .FirstOrDefault(li => li.line_item_id == lineItem.line_item_id);

                                if (existingLineItem == null)
                                {
                                    // Add new line item to the database
                                    existingpurchaseDetail?.line_items.Add(lineItem);
                                }
                                else
                                {
                                    // Update existing line item properties
                                    existingLineItem.line_item_id = lineItem.line_item_id;
                                    existingLineItem.purchaseorder_id = PurchaseorderDetail.purchaseorder_id;
                                    existingLineItem.name = lineItem.name;
                                    existingLineItem.account_name = lineItem.account_name;
                                    existingLineItem.bcy_rate = lineItem.bcy_rate;
                                    existingLineItem.rate = lineItem.rate;
                                    existingLineItem.quantity = lineItem.quantity;
                                    existingLineItem.discount = lineItem.discount;
                                    existingLineItem.tax_name = lineItem.tax_name;
                                    existingLineItem.tax_type = lineItem.tax_type;
                                    existingLineItem.tax_percentage = lineItem.tax_percentage;
                                    existingLineItem.item_total = lineItem.item_total;
                                    existingLineItem.item_type = lineItem.item_type;
                                    existingLineItem.unit = lineItem.unit;
                                    existingLineItem.project_id = lineItem.project_id;
                                    
                                }
                            }

                            if (existingpurchaseDetail == null)
                            {
                                // Add new record
                                _dbContext.purchaseoderDetails.Add(PurchaseorderDetail);
                            }
                            else
                            {
                                // Update existing billDetail properties if needed
                                existingpurchaseDetail.purchaseorder_id = PurchaseorderDetail.purchaseorder_id;
                                existingpurchaseDetail.vendor_name = PurchaseorderDetail.vendor_name;
                                existingpurchaseDetail.purchaseorder_number = PurchaseorderDetail.purchaseorder_number;
                                existingpurchaseDetail.status = PurchaseorderDetail.status;
                                existingpurchaseDetail.adjustment = PurchaseorderDetail.adjustment;
                                existingpurchaseDetail.discount_amount = PurchaseorderDetail.discount_amount;
                                existingpurchaseDetail.discount = PurchaseorderDetail.discount;
                                existingpurchaseDetail.discount_applied_on_amount = PurchaseorderDetail.discount_applied_on_amount;
                                existingpurchaseDetail.discount_type = PurchaseorderDetail.discount_type;
                                existingpurchaseDetail.sub_total = PurchaseorderDetail.sub_total;
                                existingpurchaseDetail.sub_total_inclusive_of_tax = PurchaseorderDetail.sub_total_inclusive_of_tax;
                                existingpurchaseDetail.tax_total = PurchaseorderDetail.tax_total;
                                existingpurchaseDetail.total = PurchaseorderDetail.total;
                            

                            }
                        }

                        // Save changes outside of the conditional blocks
                        await _dbContext.SaveChangesAsync();

                        return Content(responseData, "application/json");
                    }

                    return StatusCode((int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                return StatusCode(500);
            }

        }
    }
}
