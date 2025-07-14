using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography.Xml;
using ZOHO_API.Models;
using ZOHO_API.Service;

namespace ZOHO_API.Controllers
{
    public class SalesOrderController : Controller
    {
        private readonly IAccessTokenService _accessTokenService;
        private readonly IAuthorizeService _authorizeService;
        private readonly ZohoAPIContext _dbContext;
        public SalesOrderController(IAccessTokenService accessTokenService, IAuthorizeService authorizeService , ZohoAPIContext dbContext)
        {
            _accessTokenService = accessTokenService;
            _authorizeService = authorizeService;
            _dbContext = dbContext;
        }
        public IActionResult SalesList()
        {
            var accessToken = _accessTokenService.GetAccessToken();


            ViewBag.AccessToken = accessToken;

            return View();
        }
        [HttpGet]
        public async Task<ActionResult> FetchSalesOrder()
        {
            try
            {
                // Retrieve access token using the service
                var accessToken = _accessTokenService.GetAccessToken();

                using (var client = new HttpClient())
                {
                    // Set Zoho Books API proxy endpoint
                    var organizationid = _authorizeService.GetOrganizationid();
                    var proxyUrl = $"https://www.zohoapis.in/books/v3/salesorders?organization_id={organizationid}";

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
                        if (zohoApiResponse != null && zohoApiResponse.salesorders != null && zohoApiResponse.salesorders.Any())
                        {
                            // Access the ExpenseData from the response
                            var SalesOrderData = zohoApiResponse.salesorders;

                            foreach (var SalesData in SalesOrderData)
                            {
                                // Check if ExpenseData is not null
                                if (SalesData != null)
                                {
                                    // Check if the ExpenseData already exists in the database based on ProjectId
                                    var existingSalesorder = _dbContext.salesOrders.FirstOrDefault(p => p.salesorder_id == SalesData.salesorder_id);

                                    if (existingSalesorder != null)
                                    {
                                        // Update existing record
                                        existingSalesorder.salesorder_id = SalesData.salesorder_id;
                                        existingSalesorder.customer_name = SalesData.customer_name;
                                        existingSalesorder.salesorder_number = SalesData.salesorder_number;
                                        existingSalesorder.salesperson_name = SalesData.salesperson_name;
                                        existingSalesorder.invoiced_status = SalesData.invoiced_status;
                                        existingSalesorder.total_invoiced_amount = SalesData.total_invoiced_amount;
                                        existingSalesorder.paid_status = SalesData.paid_status;
                                        existingSalesorder.bcy_total = SalesData.bcy_total;
                                        existingSalesorder.status = SalesData.status;
                                        existingSalesorder.total = SalesData.total;



                                    }
                                    else
                                    {
                                        // Add new recrd
                                        _dbContext.salesOrders.Add(SalesData);
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
        public ActionResult GetSalesDetail(string salesorderid)
        {
            var Salesid = salesorderid;
            ViewBag.SalesorderId = Salesid;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> FetchSalesOrderDetail(string salesorderid)
        {
            try
            {
                // Retrieve access token using the service
                var accessToken = _accessTokenService.GetAccessToken();

                using (var client = new HttpClient())
                {
                    // Set Zoho Books API proxy endpoint
                    var organizationid = _authorizeService.GetOrganizationid();
                    var proxyUrl = $"https://www.zohoapis.in/books/v3/salesorders/{salesorderid}?organization_id={organizationid}";

                    // Set authorization header
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                    // Make a request to the proxy endpoint
                    var response = await client.GetAsync(proxyUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();

                        // Deserialize JSON response into expenseDetail object
                        var zohoApiResponse = JsonConvert.DeserializeObject<ZohoApiResponse>(responseData);
                        var salesorderDetail = zohoApiResponse?.salesorder;

                        // Check if expenseDetail is not null
                        if (salesorderDetail != null)
                        {
                            // Check if the billDetail already exists in the database based on bill_id
                            var existingsaleDetail = _dbContext.salesOrdersDetail
                                .Include(ed => ed.line_items)
                                .FirstOrDefault(p => p.salesorder_id == salesorderDetail.salesorder_id);

                            // Add line items directly to billDetail
                            salesorderDetail.line_items = salesorderDetail.line_items ?? new List<Salesorderlineitem>();

                            // Process line items
                            foreach (var lineItem in salesorderDetail.line_items)
                            {
                                // Update existing line item properties if needed
                                var existingLineItem = existingsaleDetail?.line_items
                                    .FirstOrDefault(li => li.line_item_id == lineItem.line_item_id);

                                if (existingLineItem == null)
                                {
                                    // Add new line item to the database
                                    existingsaleDetail?.line_items.Add(lineItem);
                                }
                                else
                                {
                                    // Update existing line item properties
                                    existingLineItem.line_item_id = lineItem.line_item_id;
                                    existingLineItem.salesorder_id = salesorderDetail.salesorder_id;
                                    existingLineItem.name = lineItem.name;
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

                            if (existingsaleDetail == null)
                            {
                                // Add new record
                                _dbContext.salesOrdersDetail.Add(salesorderDetail);
                            }
                            else
                            {
                                // Update existing billDetail properties if needed
                                existingsaleDetail.salesorder_id = salesorderDetail.salesorder_id;
                                existingsaleDetail.customer_name = salesorderDetail.customer_name;
                                existingsaleDetail.salesorder_number = salesorderDetail.salesorder_number ;
                                existingsaleDetail.status = salesorderDetail.status;
                                existingsaleDetail.adjustment = salesorderDetail.adjustment;
                                existingsaleDetail.discount_percent = salesorderDetail.discount_percent;
                                existingsaleDetail.discount = salesorderDetail.discount;
                                existingsaleDetail.discount_applied_on_amount = salesorderDetail.discount_applied_on_amount;
                                existingsaleDetail.discount_type = salesorderDetail.discount_type;
                                existingsaleDetail.sub_total = salesorderDetail.sub_total;
                                existingsaleDetail.sub_total_inclusive_of_tax = salesorderDetail.sub_total_inclusive_of_tax;
                                existingsaleDetail.tax_total = salesorderDetail.tax_total;
                                existingsaleDetail.total = salesorderDetail.total;


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
