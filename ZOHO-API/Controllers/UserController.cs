using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ZOHO_API.Models;
using ZOHO_API.Service;

namespace ZOHO_API.Controllers
{
    public class UserController : Controller
    {
        private readonly IAccessTokenService _accessTokenService;
        private readonly IAuthorizeService _authorizeService;
        private readonly ZohoAPIContext _dbContext;

        public UserController(IAccessTokenService accessTokenService, IAuthorizeService authorizeService , ZohoAPIContext dbContext)
        {
            _accessTokenService = accessTokenService;
            _authorizeService = authorizeService;
            _dbContext = dbContext;
        }
        public ActionResult Users()
        {
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> FetchUsers()
        {
            try
            {
                // Retrieve access token using the service
                var accessToken = _accessTokenService.GetAccessToken();

                using (var client = new HttpClient())
                {
                    // Set Zoho Books API proxy endpoint
                    var organizationid = _authorizeService.GetOrganizationid();
                    var proxyUrl = $"https://www.zohoapis.in/books/v3/users?organization_id={organizationid}";

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
                        if (zohoApiResponse != null && zohoApiResponse.users != null && zohoApiResponse.users.Any())
                        {
                            // Access the ExpenseData from the response
                            var usersData = zohoApiResponse.users;

                            foreach (var userData in usersData)
                            {
                                // Check if ExpenseData is not null
                                if (userData != null)
                                {
                                    // Check if the ExpenseData already exists in the database based on ProjectId
                                    var existinguser = _dbContext.Users.FirstOrDefault(p => p.user_id == userData.user_id);

                                    if (existinguser != null)
                                    {
                                        // Update existing record
                                        existinguser.user_id = userData.user_id;
                                        existinguser.name = userData.name;
                                        existinguser.user_role = userData.user_role;
                                        existinguser.email = userData.email;
                                        existinguser.status = userData.status;
                                        existinguser.is_current_user = userData.is_current_user;
                                    


                                    }
                                    else
                                    {
                                        // Add new recrd
                                        _dbContext.Users.Add(userData);
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
    }
}
