using DataTables.AspNetCore.Mvc.Binder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WirelessSupport.Model.ViewModel;
using WirelessSupport.Service.Services.Contracts;
using WirelessSupport.Web.Hubs;
using Microsoft.AspNetCore.Cors;

namespace WirelessSupport.Web.Controllers
{
    
    
    public class CommonController : Controller
    {
        private readonly ICommonService _commonService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<EmailNotificationHub> _EmailNotificationHubContext;
        public CommonController(ICommonService _commonService, 
            IHostingEnvironment _hostingEnvironment,
            IConfiguration _configuration,
            IHubContext<EmailNotificationHub> _EmailNotificationHubContext)
        {
            this._commonService = _commonService;
            this._hostingEnvironment = _hostingEnvironment;
            this._configuration = _configuration;
            this._EmailNotificationHubContext = _EmailNotificationHubContext;
        }

        [HttpGet]
        public async Task<JsonResult> GetSelectValues(int id, string type)
        {
            var data = await _commonService.GetSelectValues(type, id);
            return Json(data);
        }
        [HttpGet]
        public async Task<JsonResult> GetSelectValuesWithSearch(string type, string search, int id = 0)
        {
            var data = await _commonService.GetSelectValuesWithSearch(type, search, id);
            return Json(data);
        }
        [HttpGet]
        public async Task<IActionResult> VerifyName(int DeviceAccessoriesId, string Name)
        {
            var flag = await _commonService.VerifyNames(DeviceAccessoriesId, Name).ConfigureAwait(false);
            return Json(flag);
        }
        public async Task<IActionResult> VerifyEmail(int AccountId, string Email)
        {
            var flag = await _commonService.VerifyEmail(AccountId, Email).ConfigureAwait(false);
            return Json(flag);
        }
        public async Task<IActionResult> VerifyTemplateName(int ID, string TemplateName)
        {
            var flag = await _commonService.VerifyTemplateName(ID, TemplateName).ConfigureAwait(false);
            return Json(flag);
        }

        [HttpGet]
        public async Task<JsonResult> GetTemplates()
        {
            var data = await _commonService.GetTemplateList();
            return Json(data);
        }

        //[Route("upload_ckeditior")]
       
        [HttpPost]
        public async Task<IActionResult>  UploadFiles(IFormFile upload)
        {
            var fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + upload.FileName;
            var path = Path.Combine(Directory.GetCurrentDirectory(),
            _hostingEnvironment.WebRootPath, "Upload", fileName);
            var stream = new FileStream(path, FileMode.Create);
            await upload.CopyToAsync(stream);

            return new JsonResult(new { uploaded = true, fileName = upload.FileName, url = "/Upload/" + fileName });
        }
        [HttpGet]
        public async Task<IActionResult> Test()
        {
           

            return new JsonResult(new { uploaded = true});
        }
        //[Route("filebrowse")]
        [HttpGet]
        public IActionResult FileBrowse()
        {

            var dir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(),
            _hostingEnvironment.WebRootPath, "Upload"));
            ViewBag.fileInfos = dir.GetFiles();
            return View("FileBrowse");
        }

        public JsonResult GetEmailNotification()
        {

            using (var connection = new SqlConnection(_configuration.GetSection("ConnectionString").Value))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(@"SELECT iID FROM [dbo].[tbl_EmailDetails] WHERE [IsDeleted] <> 1", connection))
                {
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();
                    SqlDependency.Start(_configuration.GetSection("ConnectionString").Value);

                    // Make sure the command object does not already have
                    // a notification object associated with it.
                    command.Notification = null;

                    SqlDependency dependency = new SqlDependency(command);
                    //dependency.OnChange += new OnChangeEventHandler(dependency_OnChange);



                    SqlDataReader reader = command.ExecuteReader();

                    var listCus = reader.Cast<IDataRecord>()
                            .Select(x => new
                            {
                                iID = (int)x["iID"],
                                //Subject = (string)x["Subject"],

                            }).ToList();

                    return Json(new { listCus = listCus });

                }
            }
        }

        //private void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        //{
        //     new EmailNotificationHub(_EmailNotificationHubContext).SendMessage();
        //}

    }
}
