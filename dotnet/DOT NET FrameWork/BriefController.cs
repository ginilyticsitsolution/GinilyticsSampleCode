using BriefLynx.DataAccess.Model;
using BriefLynxNew.BriefLynx.Common.Files;
using BriefLynxNew.BriefLynx.Common.Interfaces.Services;
using BriefLynxNew.BriefLynx.Common.Interfaces.Services.Matters;
using BriefLynxNew.BriefLynx.Common.Interfaces.Services.Users;
using BriefLynxNew.BriefLynx.Common.ShareFile;
using BriefLynxNew.BriefLynx.DataAccess;
using BriefLynxNew.BriefLynx.DataAccess.Enumerations;
using BriefLynxNew.BriefLynx.DataAccess.Extensions;
using BriefLynxNew.BriefLynx.DataAccess.Model;
using BriefLynxNew.BriefLynx.Workspace.Domain;
using BriefLynxNew.BriefLynx.Workspace.Domain.Models;
using BriefLynxNew.Extensions;
using BriefLynxNew.Filter;
using BriefLynxNew.Models;
using BriefLynxNew.Query;
using BriefLynxNew.Services;
using Com.Wolfgang.Amadeus.Diagnostics2;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using static iTextSharp.text.pdf.parser.LocationTextExtractionStrategy;
using Path = System.IO.Path;

namespace BriefLynxNew.Controllers
{
    [Authorize]
    public class BriefController : Controller
    {
        #region Constants

        private const string LogCategory = "BriefController";
        private const string CustomFileUploadDirectory = "~/tmpCustomFileFolder";
        private const string MecroFileUploadDirectory = "~/tmpMecroFileFolder";
        private const string MacroDocumentUploadDirectory = "~/tmpMacroDocumentUpload";

        private static string PDFTOHtmlDirectory = ConfigurationManager.AppSettings["BriefLynx.HtmlDocument"];

        #endregion

        #region Static Fields

        private static readonly int[] ViewableShareStatusIds = new[] { (int)ShareStatusEnumeration.Shared, (int)ShareStatusEnumeration.Completed };

        #endregion

        #region Fields

        private readonly IWorkspaceExhibitListService _exhitbitListService;

        private readonly HttpServerUtilityBase _httpServerUtility;

        private readonly IMatterService _matterService;

        private readonly IUserSecurityService _securityService;

        private readonly IWorkspaceBriefService _service;

        private readonly IWorkspaceBriefPackagingService _packagingService;

        private readonly ITempPathProvider _tempPathProvider;

        private readonly IUserService _userService;

        private readonly IDocumentService _documentService;

        private readonly IWorkspaceBriefDocumentService _workspaceDocumentService;

        private readonly ILocalFileStorageService _fileStorageService;

        private IAutomatedLinkingService _automatedLinkingService;
        #endregion

        #region Constructors and Destructors

        public BriefController(
            IWorkspaceBriefService service,
            IWorkspaceBriefPackagingService packagingService,
            IWorkspaceExhibitListService exhibitListService,
            IUserSecurityService securityService,
            IMatterService matterService,
            ITempPathProvider tempPathProvider,
            IUserService userService,
            HttpServerUtilityBase httpServerUtility,
            IDocumentService documentService,
            ILocalFileStorageService fileStorageService,
            IWorkspaceBriefDocumentService workspaceDocumentService,
            IAutomatedLinkingService automatedLinkingService)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }
            if (packagingService == null)
            {
                throw new ArgumentNullException("packagingService");
            }

            if (exhibitListService == null)
            {
                throw new ArgumentNullException("exhibitListService");
            }

            if (securityService == null)
            {
                throw new ArgumentNullException("securityService");
            }

            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }

            if (tempPathProvider == null)
            {
                throw new ArgumentNullException("tempPathProvider");
            }

            if (httpServerUtility == null)
            {
                throw new ArgumentNullException("httpServerUtility");
            }

            if (documentService == null)
            {
                throw new ArgumentNullException("documentService");
            }

            if (fileStorageService == null)
            {
                throw new ArgumentNullException("fileStorageService");
            }

            if (workspaceDocumentService == null)
            {
                throw new ArgumentNullException("workspaceDocumentService");
            }

            _service = service;
            _packagingService = packagingService;
            _exhitbitListService = exhibitListService;
            _securityService = securityService;
            _matterService = matterService;
            _tempPathProvider = tempPathProvider;
            _httpServerUtility = httpServerUtility;
            _userService = userService;
            _documentService = documentService;
            _fileStorageService = fileStorageService;
            _workspaceDocumentService = workspaceDocumentService;
            _automatedLinkingService = automatedLinkingService;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Displays the Add New Brief View
        /// </summary>
        /// <param name="id">The identifier for the Matter to which the Brief will be added.</param>
        /// <returns></returns>
        [MatterAuthorization]
        public ActionResult Add(int id)
        {
            var matter = _matterService.GetMatter(id);

            if (matter == null)
            {
                return new HttpNotFoundResult("Matter not found.");
            }

            return View(new BriefViewModel(_securityService) { MatterId = id, BreadCrumb = new BreadCrumbViewModel(matter) });
        }

        public ActionResult AddExhibitDocuments(int exhbitListId)
        {
            var briefModel = new BriefViewModel(_securityService);

            return View("Index", briefModel);
        }

        [BriefAuthorization]
        [HttpPost]
        public ActionResult AttachmentPlaceholder(int id, string fileName, double fileSizeBytes)
        {
            fileName = Path.GetFileName(fileName);

            var fileSizeMb = fileSizeBytes / (1024 * 1024);

            var attachmentPlaceholderModel = new AttachmentPlaceholderModel(fileName, fileSizeMb);

            _service.AddAttachmentPlaceholders(id, new[] { attachmentPlaceholderModel });

            return Json(attachmentPlaceholderModel);
        }

        [HttpPost]
        [BriefAuthorization]
        public ActionResult AttachmentUpload(int id, IList<HttpPostedFileBase> files = null, List<ShareFileFile> shareFileFiles = null)
        {
            if ((files == null || !files.Any()) && (shareFileFiles == null || !shareFileFiles.Any()))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No files were provided to upload");
            }

            //var getfileextension = System.IO.Path.GetExtension(files.Select(f => f.FileName));

            //Atalasoft.PdfDoc f = new Atalasoft.PdfDoc();

            var brief = _service.GetBrief(id);

            if (brief == null)
            {
                return new HttpNotFoundResult("Brief does not exist");
            }

            var briefFiles = files != null
                                 ? files.Select(
                                     f =>
                                     {
                                         var briefLynxFile = new BriefLynxFile(
                                             f.InputStream,
                                             // episode # 52,245,315 of 'Why Does Internet Explorer suck so badly' http://stackoverflow.com/a/17600089
                                             Path.GetFileName(f.FileName));
                                         briefLynxFile.CacheToDisk(_tempPathProvider);
                                         return briefLynxFile;
                                     }).ToList()
                                 : Enumerable.Empty<BriefLynxFile>();

            string shareFileAuthToken;
            string shareFileUri;
            var shareFileFilesToAdd = shareFileFiles ?? new List<ShareFileFile>();

            if (!ShareFileLogOnViewModel.IsAuthenticated(Request, out shareFileAuthToken, out shareFileUri) && shareFileFilesToAdd.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "You are not authenticated to ShareFile, please login first.");
            }

            var taskId = 0;
            try
            {
                taskId = _service.AddBriefDocuments(id, briefFiles, shareFileFilesToAdd,
                    !string.IsNullOrEmpty(shareFileUri) ? new Uri(shareFileUri) : null, shareFileAuthToken);
            }
            catch (BriefLynxUserException ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, ex.Message);
            }

            return Json(new TaskModel { TaskID = taskId }, "text/plain");
        }

        [HttpPost]
        [BriefAuthorization]
        public ActionResult DeleteDocument(int id, int documentId)
        {
            _service.DeleteBriefAttachment(id, documentId);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [BriefAuthorization]
        public ActionResult Documents([DataSourceRequest] DataSourceRequest request, int id)
        {
            var brief = _service.GetBrief(id);

            if (brief == null)
            {
                return new HttpNotFoundResult("Brief not found");
            }

            var briefDocuments = ApplyBriefDocumentFilteringAndSorting(
                request, brief.BriefDocuments.Where(bd => bd.DocumentTypeID == (int)DocumentTypeEnumeration.Attachment).AsQueryable());

            var result = briefDocuments.ToDataSourceResult(request, document => new BriefDocumentViewModel(document));

            return Json(result);
        }

        [BriefAuthorization]
        public ActionResult AddBriefShare([DataSourceRequest] DataSourceRequest request, int id, BriefSharesViewModel model)
        {
            if (model != null && ModelState.IsValid)
            {
                var brief = _service.GetBrief(id);

                if (brief != null && model.ShareUserName != null)
                {
                    try
                    {
                        ValidateShareRange(id, model.StartPage, model.EndPage);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("EndPage", ex.Message);
                    }

                    if (ModelState.IsValid)
                    {
                        var user = _userService.GetUser(model.ShareUserName);

                        var newShare = new Share()
                        {
                            UserID = user != null ? user.UserID : 0,
                            CreatedDate = model.CreatedDate,
                            ShareStatusID = (int)ShareStatusEnumeration.Shared,
                            StartPage = model.StartPage,
                            EndPage = model.EndPage
                        };

                        brief.Shares.Add(newShare);
                        _service.CommitAllChanges();
                    }
                }
            }

            return Json(ModelState.ToDataSourceResult());
        }

        public ActionResult UpdateBriefShare([DataSourceRequest] DataSourceRequest request, int id, BriefSharesViewModel model)
        {
            if (model != null)
            {
                // TODO
            }

            return null;
        }

        [BriefAuthorization]
        public ActionResult DeleteBriefShare([DataSourceRequest] DataSourceRequest request, int id, BriefSharesViewModel model)
        {
            if (model != null)
            {
                var brief = _service.GetBrief(id);

                if (brief != null)
                {
                    var briefShares =
                        brief.Shares.Where(bs => bs.ShareID == model.ShareId);

                    if (briefShares.Any())
                    {
                        var share = briefShares.First();
                        share.ShareStatusID = (int)ShareStatusEnumeration.Canceled;
                        _service.CommitAllChanges();
                    }
                }
            }
            else
            {
                ModelState.AddModelError("Brief Share", "Unable to return share");
            }

            var result = new object[] { model }.ToDataSourceResult(request, ModelState);

            return Json(result);
        }

        [BriefAuthorization]
        public ActionResult GetShares([DataSourceRequest] DataSourceRequest request, int id)
        {
            var brief = _service.GetBrief(id);

            if (brief != null)
            {
                var briefShares =
                    brief.Shares.Where(bs => ViewableShareStatusIds.Contains(bs.ShareStatusID)).OrderBy(bs => bs.StartPage).ThenBy(bs => bs.EndPage);

                if (briefShares.Any())
                {
                    DataSourceResult result = briefShares.ToDataSourceResult(
                        request,
                        share =>
                        new BriefSharesViewModel
                        {
                            ShareId = share.ShareID,
                            CreatedDate = share.CreatedDate,
                            ShareUserName = (share.User != null ? share.User.UserName : string.Empty),
                            StartPage = share.StartPage,
                            EndPage = share.EndPage,
                            Status = share.ShareStatus.ShareStatusName
                        });

                    return Json(result);
                }
            }

            return null;
        }

        [BriefAuthorization]
        public ActionResult GetMyShares([DataSourceRequest] DataSourceRequest request, int id)
        {
            var brief = _service.GetBrief(id);
            var user = _securityService.CurrentUser;

            if (brief != null)
            {
                var briefShares =
                    brief.Shares.Where(bs => ViewableShareStatusIds.Contains(bs.ShareStatusID)).Where(bs => bs.UserID.Equals(user.UserID)).OrderBy(bs => bs.StartPage).ThenBy(bs => bs.EndPage);

                if (briefShares.Any())
                {
                    DataSourceResult result = briefShares.ToDataSourceResult(
                        request,
                        share =>
                        new BriefSharesViewModel
                        {
                            ShareId = share.ShareID,
                            CreatedDate = share.CreatedDate,
                            ShareUserName = (share.User != null ? share.User.UserName : string.Empty),
                            StartPage = share.StartPage,
                            EndPage = share.EndPage,
                            Status = share.ShareStatus.ShareStatusName
                        });

                    return Json(result);
                }
            }

            return null;
        }

        public ActionResult GetShareUsers()
        {
            var menuCookie = Request.Cookies[MenuCookieModel.CookieName] ?? new HttpCookie(MenuCookieModel.CookieName);

            var cookieValue = MenuCookieModel.GetCookieValue(menuCookie.Value, _httpServerUtility);

            if (cookieValue.BriefId.HasValue)
            {

                var brief = _service.GetBrief(cookieValue.BriefId.Value);

                if (brief != null)
                {
                    var creator = _userService.GetUser(brief.CreatorUserID);

                    var users =
                        brief.Creator.Firm.Users.Where(u => u.Enabled
                                                            // && u.UserID != creator.UserID
                                                            &&
                                                            new[]
                                                            {
                                                                RoleEnumeration.Administrator,
                                                                RoleEnumeration.FirmAdministrator,
                                                                RoleEnumeration.Creator
                                                            }.Cast<int>().Contains(u.UserRoles.First().RoleID));
                    var validUsers = users.Select(s => new { UserId = s.UserID, ShareUserName = s.UserName });

                    return Json(validUsers, JsonRequestBehavior.AllowGet);
                }
            }

            return null;
        }

        public ActionResult Index(int? id)
        {
            return GetBriefDetailResponse(id, (id.HasValue && id.Value > 0));
        }

        public ActionResult LinkReport(int id)
        {
            var brief = _service.GetBrief(id);

            if (brief == null)
            {
                return new HttpNotFoundResult();
            }

            var linkReportFile = _service.GenerateLinkReportFile(id);

            return File(linkReportFile, "text/plain", string.Format("{0} Link Report.txt", brief.Name));
        }

        public ActionResult List([DataSourceRequest] DataSourceRequest request)
        {
            var briefs = _service.ListAllBriefs();
            var result = briefs.ToDataSourceResult(request, brief => new BriefViewModel(_securityService, brief));
            return Json(result);
        }

        [BriefAuthorization]
        public ActionResult Package(int id)
        {
            var brief = _service.GetBrief(id);

            if (TempData.ContainsKey("ValidationMessage"))
            {
                ViewBag.Message = TempData["ValidationMessage"];
            }
            else if (brief.IsShared)
            {
                ViewBag.Message = "A Brief Package cannot be generated until all shares have been completed or canceled.";
            }

            var model = new BriefPackageViewModel(_securityService, brief)
            {
                PackagingTaskId = _packagingService.GetCurrentBriefPackagingTask(brief),
                AllowPackagingTaskCancelation =
                                    _securityService.CanCurrentUserEditThisBrief(brief.BriefID)
            };

            return View(model);
        }

        [HttpPost]
        [BriefAuthorization]
        public ActionResult Package(int id, BriefPackageSettingsViewModel model)
        {
            var brief = _service.GetBrief(id);

            if (brief == null)
            {
                return new HttpNotFoundResult("Brief does not exist");
            }

            if (ModelState.IsValid)
            {
                SaveBriefPackageSettings(model, brief);

                var packagingTaskId = _packagingService.GenerateBriefPackage(brief.BriefID);

                var responseModel = new BriefPackageViewModel(_securityService, brief)
                {
                    PackageSettings = model,
                    PackagingTaskId = packagingTaskId,
                    AllowPackagingTaskCancelation =
                                                _securityService.CanCurrentUserEditThisBrief(brief.BriefID)
                };

                responseModel.PackageSettings.PackageGenerationStatus = PackageStatusEnumeration.Generating;

                responseModel.mecroIds = new List<int>();
                //responseModel.mecros = new List<Mecro>();

                //try
                //{
                //    responseModel.mecros = _service.MecroList(id).ToList();
                //}
                //catch (Exception ex)
                //{ 
                //}

                return View(responseModel);
            }
            return View(model);
        }

        [BriefAuthorization(accessMode: BriefAccessMode.Read)]
        public ActionResult PackageDownload(int id)
        {
            var brief = _service.GetBrief(id);

            if (brief == null)
            {
                return new HttpNotFoundResult("Brief does not exist");
            }

            string friendlyFileName = brief.Name.FriendlyName();

            string briefPackageName = !String.IsNullOrEmpty(friendlyFileName)
                                          ? String.Format(CultureInfo.InvariantCulture, "{0}.zip", friendlyFileName)
                                          : String.Format(CultureInfo.InvariantCulture, "Brief{0}.zip", brief.BriefID);

            // If this is a draft, prefix draft to the file name.
            if (brief.IsDraft)
            {
                briefPackageName = "draft_" + briefPackageName;
            }

            var localFilePath = _packagingService.GetDownloadableBriefPackage(brief.BriefID);

            return File(localFilePath, "application/zip", briefPackageName);
        }

        [BriefAuthorization(accessMode: BriefAccessMode.Read)]
        public ActionResult IPadPackageDownload(int id)
        {
            var brief = _service.GetBrief(id);

            if (brief == null)
            {
                return new HttpNotFoundResult("Brief does not exist");
            }

            var localPath = _packagingService.GetDownloadableIPadBrief(id);

            return File(localPath, "application/pdf", string.Format("{0}.pdf", BriefPackageSettingsViewModel.GetPackageFileName(brief)));
        }

        [HttpPost]
        public ActionResult PackageSettings(int id, BriefPackageSettingsViewModel model)
        {
            var brief = _service.GetBrief(id);

            if (brief == null)
            {
                return new HttpNotFoundResult("Brief does not exist");
            }

            if (ModelState.IsValid)
            {
                SaveBriefPackageSettings(model, brief);
            }
            else
            {
                TempData["ValidationMessage"] = "You have submitted incomplete or invalid data.  Please check the form and try again.";
            }

            return RedirectToAction("Package", new { id });
        }

        [HttpPost]
        public ActionResult SaveBrief(BriefViewModel briefViewModel)//(string BriefName, int MatterId) BriefViewModel briefViewModel
        {
            try
            {
                var isnewbrief = false;
                string statusMessage = "Unable to save brief.  Please try again.";
                //BriefViewModel briefViewModel = new BriefViewModel();
                //briefViewModel.BriefName = BriefName;
                //briefViewModel.MatterId = MatterId;
                if (ModelState.IsValid == true)
                {
                    try
                    {
                        var brief = briefViewModel.BriefId.HasValue
                            ? _service.GetBrief(briefViewModel.BriefId.Value)
                            : _service.CreateBrief(briefViewModel.BriefName, _securityService.CurrentUser, briefViewModel.MatterId);

                        brief.Name = briefViewModel.BriefName;

                        if (briefViewModel.BriefId.HasValue)
                        {
                            brief.Enabled = briefViewModel.IsActive;
                        }

                        isnewbrief = !briefViewModel.BriefId.HasValue;

                        _service.CommitAllChanges();
                        briefViewModel.BriefId = brief.BriefID;
                        return GetBriefDetailResponse(briefViewModel.BriefId, isnewbrief);
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("Cannot insert duplicate key"))
                        {
                            statusMessage = "Name is already in use.  Please enter a different name.";
                        }
                    }
                }

                var matter = _matterService.GetMatter(briefViewModel.MatterId);
                var model = new BriefViewModel(_securityService, matter)
                {
                    StatusMessage = statusMessage
                };

                //return View((briefViewModel.BriefId.HasValue ? "Index" : "Add"), model);

                var viewname = (briefViewModel.BriefId.HasValue ? "~/Views/Brief/IndexNew.cshtml" : "~/Views/Brief/Add.cshtml");

                var Htmlstring = RenderPartialString.RenderPartialToString(ControllerContext,
                                  viewname,
                                  model,
                                  true);

                var Popuptitle = (briefViewModel.BriefId.HasValue ? "Brief Details" : "Add Brief");

                return Json(new
                {
                    success = briefViewModel.BriefId.HasValue,
                    htmlstring = Htmlstring,
                    title = Popuptitle,
                    //message = "Brief successfully saved.",
                    isnew = isnewbrief,
                    message = model.StatusMessage
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveSettings(BriefSettingsViewModel model)
        {
            if (model.BriefId.HasValue && _securityService.CanCurrentUserEditThisBrief(model.BriefId.Value))
            {
                var brief = _service.GetBrief(model.BriefId.Value);

                brief.Options.DefaultDocumentZoomLevelTypeID = (int)model.DefaultDocumentZoom;
                brief.Options.DefaultPageZoomTypeID = (int)model.DefaultPageRange;

                _service.CommitAllChanges();

                TempData["message"] = "Brief Settings Saved";

                if (model.BriefId.HasValue)
                {
                    brief = _service.GetBrief(model.BriefId.GetValueOrDefault());
                }

                model = new BriefSettingsViewModel(_securityService, brief);

                //return RedirectToAction("Settings", new { id = model.BriefId });
                var viewname = "~/Views/Brief/SettingsNew.cshtml";

                var Htmlstring = RenderPartialString.RenderPartialToString(ControllerContext,
                                  viewname,
                                  model,
                                  true);

                var Popuptitle = "Brief Settings";

                return Json(new { success = true, htmlstring = Htmlstring, title = Popuptitle }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = false
            });
        }

        [BriefAuthorization]
        public ActionResult Settings(int? id)
        {
            Brief brief = null;
            if (id.HasValue)
            {
                brief = _service.GetBrief(id.GetValueOrDefault());
            }

            var model = new BriefSettingsViewModel(_securityService, brief);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.StatusMessage = TempData["message"];
            }

            return View("Settings", model);
        }

        public ActionResult ShareIDSave(int id)
        {
            var brief = _service.GetBrief(id);

            if (brief == null)
            {
                return new HttpNotFoundResult("Brief does not exist");
            }

            try
            {

                _documentService.SaveShareId(id);

                var briefsaved = _service.GetBrief(id);

                return Json(new { success = true, docmentguid = briefsaved.MasterDocument.ShareID });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AllowAnonymous]
        public ActionResult Document(Guid? id)
        {

            if (!id.HasValue)
            {
                ViewBag.HtmlData = "<h2>Document Doesn't exists.</h2>";
            }
            else
            {
                var briefDocument = _documentService.GetDocumentByShareId<BriefDocument>(id.Value);

                string htmlfilepath = string.Empty;

                if (briefDocument == null)
                {
                    ViewBag.HtmlData = "<h2>Document Doesn't exists.</h2>";
                }
                else
                {
                    var pathwithbrief = Path.Combine(PDFTOHtmlDirectory, "brief-" + briefDocument.BriefID);
                    var htmldocdirec = Server.MapPath(pathwithbrief);

                    if (!Directory.Exists(htmldocdirec))
                    {
                        Directory.CreateDirectory(htmldocdirec);
                    }

                    htmlfilepath = Path.Combine(htmldocdirec, briefDocument.FileName).Replace(".pdf", ".html");

                    if (!System.IO.File.Exists(htmlfilepath))
                    {

                        var filePath = _fileStorageService.EnsureLocalFileExists(briefDocument.UriToFile);

                        #region generate Html file from PDF using Aspose
                        SautinSoft.PdfFocus f = new SautinSoft.PdfFocus();

                        f.HtmlOptions.IncludeImageInHtml = true;

                        f.HtmlOptions.InlineCSS = false;
                        f.HtmlOptions.UseNumericCharacterReference = true;
                        f.HtmlOptions.DetectTables = true;

                        f.WordOptions.KeepCharScaleAndSpacing = true;

                        f.WordOptions.RenderMode = SautinSoft.PdfFocus.CWordOptions.eRenderMode.Exact;

                        f.ExcelOptions.ConvertNonTabularDataToSpreadsheet = false;

                        f.OpenPdf(filePath);

                        if (f.PageCount > 0)
                        {
                            int res = f.ToHtml(htmlfilepath);

                        }

                        f.ClosePdf();

                        RemovedLicenseFromHtml(htmlfilepath);
                        #endregion
                    }

                    if (!string.IsNullOrEmpty(htmlfilepath))
                    {
                        RemovedLicenseFromHtml(htmlfilepath);

                        using (WebClient client = new WebClient())
                        {
                            var htmlData = client.DownloadData(htmlfilepath);
                            ViewBag.HtmlData = Encoding.UTF8.GetString(htmlData);
                        }
                    }
                }
            }

            return View();
        }

        [HttpPost]
        public ActionResult ApplyMacro(int id)
        {
            try
            {
                var briefDocument = _workspaceDocumentService.GetBriefDocument(id);

                var user = _userService.GetUser(Int32.Parse(User.Identity.Name));
                                                                                                                                        
                if (briefDocument == null)
                {
                    throw new BriefLynxServiceException(string.Format("A Brief Document with Id={0} does not exist", id));
                }

                var document = _documentService.GetDocument<BriefDocument>(briefDocument.DocumentID);

                if (document == null)
                {
                    throw new BriefLynxServiceException(string.Format("A Brief Document with Id={0} does not exist", id));
                }

                var filePath = _fileStorageService.EnsureLocalFileExists(document.UriToFile);

                #region save brif document file in temp folder

                var briftmppath = Path.Combine(MacroDocumentUploadDirectory, "brif-" + id);

                var documentpath = Path.Combine(Server.MapPath(briftmppath), "maindocument");
                var macrodocumentpath = Path.Combine(Server.MapPath(briftmppath), "macrodocument");

                if (Directory.Exists(documentpath))
                {
                    Directory.Delete(documentpath, true);
                }
                Directory.CreateDirectory(documentpath);

                if (Directory.Exists(macrodocumentpath))
                {
                    Directory.Delete(macrodocumentpath, true);
                }
                Directory.CreateDirectory(macrodocumentpath);

                // save brif main file
                var maindocumentfilepath = Path.Combine(documentpath, Path.GetFileName(document.UriToFile.AbsoluteUri));
                var documentfilepath = Path.Combine(macrodocumentpath, Path.GetFileName(document.UriToFile.AbsoluteUri));

                System.IO.File.Copy(filePath, maindocumentfilepath);
                //System.IO.File.Copy(filePath, documentfilepath);

                #endregion

                #region apply mecro

                using (FileStream fs = new FileStream(documentfilepath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(filePath))
                    {
                        using (iTextSharp.text.pdf.PdfStamper stamper = new iTextSharp.text.pdf.PdfStamper(reader, fs))
                        {
                            #region link annotation apply in PDF 
                            var macros = _service.MecroList(id).ToList();

                            //macros.ForEach(f =>
                            //{
                            //    f.Brief = null;
                            //});

                            var globalMecro = _service.GlobalMecroList().ToList();

                            macros = macros.Union(globalMecro).ToList();

                            foreach (var item in macros)
                            {
                                var getmecrorecord = item;

                                if (getmecrorecord != null)
                                {
                                    var getwordrect = GetPDFWordRects(maindocumentfilepath, getmecrorecord);

                                    if (getwordrect != null && getwordrect.Count > 0)
                                    {
                                        var stringUrl = string.Empty;

                                        if (getmecrorecord.ExhibitId.HasValue)
                                        {
                                            var exh = _service.ExhibitById(getmecrorecord.ExhibitId.GetValueOrDefault());

                                            if (exh != null)
                                            {
                                                var exdoc = exh.ExhibitDocument;

                                                if (exdoc == null)
                                                {
                                                    throw new BriefLynxServiceException(string.Format("A Exhibit Document with Id={0} does not exist", id));
                                                }

                                                var exfilePath = _fileStorageService.EnsureLocalFileExists(exdoc.UriToFile);

                                                var externamfilepath = Path.Combine(macrodocumentpath, "ExternalFiles");

                                                if (!Directory.Exists(externamfilepath))
                                                {
                                                    Directory.CreateDirectory(externamfilepath);
                                                }

                                                var custfilename = string.Format("{0}{1}", Guid.NewGuid(), Path.GetExtension(exdoc.FileName));

                                                var finalsavedfileloc = Path.Combine(externamfilepath, custfilename);

                                                if (System.IO.File.Exists(finalsavedfileloc))
                                                {
                                                    System.IO.File.Delete(finalsavedfileloc);
                                                }

                                                System.IO.File.Copy(exfilePath, finalsavedfileloc);


                                                Mecro mecroData = new Mecro()
                                                {
                                                    BriefID = id,
                                                    Name = getmecrorecord.Name,
                                                    FileName = custfilename,
                                                    Id = getmecrorecord.Id,
                                                    ExhibitId = getmecrorecord.ExhibitId,
                                                    UpdatedBy = user.UserName,
                                                    UpdatedDate = DateTime.UtcNow
                                                };

                                                _service.SaveMecro(mecroData);

                                                stringUrl = "http://" + HttpContext.Request.Url.Authority + "/BriefDocument/GetMecroDoc/" + id + "/" + getmecrorecord.Id;
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(getmecrorecord.FileName)
                                                && !string.IsNullOrEmpty(getmecrorecord.URL))
                                            {
                                                WebResponse response = null;

                                                WebRequest request = WebRequest.Create(getmecrorecord.URL);

                                                try
                                                {
                                                    response = request.GetResponse();
                                                    if (response != null)
                                                    {
                                                        var tmpURL = new Uri(getmecrorecord.URL);

                                                        var localfilename = Path.GetFileNameWithoutExtension(getmecrorecord.URL);
                                                        var localextension = Path.GetExtension(getmecrorecord.URL);

                                                        var custfilename = string.Format("{0}{1}", localfilename, localextension);

                                                        var externamfilepath = Path.Combine(macrodocumentpath, "ExternalFiles");

                                                        if (!Directory.Exists(externamfilepath))
                                                        {
                                                            Directory.CreateDirectory(externamfilepath);
                                                        }

                                                        Stream s = response.GetResponseStream();

                                                        var finalsavedfileloc = Path.Combine(externamfilepath, custfilename);

                                                        if (System.IO.File.Exists(finalsavedfileloc))
                                                        {
                                                            System.IO.File.Delete(finalsavedfileloc);
                                                        }

                                                        WebClient myWebClient = new WebClient();

                                                        myWebClient.DownloadFile(new Uri(getmecrorecord.URL).AbsoluteUri, finalsavedfileloc);

                                                        stringUrl = "http://" + HttpContext.Request.Url.Authority + "/BriefDocument/GetMecroDoc/" + id + "/" + getmecrorecord.Id;
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                            }
                                            else if (!string.IsNullOrEmpty(getmecrorecord.FileName))
                                            {
                                                var localfilename = Path.GetFileNameWithoutExtension(getmecrorecord.FileName);
                                                var localextension = Path.GetExtension(getmecrorecord.FileName);

                                                var custfilename = string.Format("{0}{1}", localfilename, localextension);

                                                var externamfilepath = Path.Combine(macrodocumentpath, "ExternalFiles");

                                                if (!Directory.Exists(externamfilepath))
                                                {
                                                    Directory.CreateDirectory(externamfilepath);
                                                }

                                                var finalsavedfileloc = Path.Combine(externamfilepath, custfilename);

                                                if (System.IO.File.Exists(finalsavedfileloc))
                                                {
                                                    System.IO.File.Delete(finalsavedfileloc);
                                                }

                                                var basesourceFilepath = Server.MapPath("~/tmpMecroFileFolder");

                                                var tsourceFilePath = string.Empty;

                                                if (getmecrorecord.IsGlobal)
                                                {
                                                    tsourceFilePath = Path.Combine(basesourceFilepath, string.Format("global_macro"));
                                                    var sourceFilePath = Path.Combine(tsourceFilePath, getmecrorecord.FileName);

                                                    System.IO.File.Copy(sourceFilePath, finalsavedfileloc);
                                                }
                                                else if (!getmecrorecord.IsGlobal && getmecrorecord.BriefID > 0)
                                                {
                                                    tsourceFilePath = Path.Combine(basesourceFilepath, string.Format("brief-{0}", getmecrorecord.BriefID));
                                                    var sourceFilePath = Path.Combine(tsourceFilePath, getmecrorecord.FileName);

                                                    System.IO.File.Copy(sourceFilePath, finalsavedfileloc);
                                                }


                                                //stringUrl = "ExternalFiles/" + custfilename;
                                                stringUrl = "http://" + HttpContext.Request.Url.Authority + "/BriefDocument/GetMecroDoc/" + id + "/" + getmecrorecord.Id;
                                            }

                                        }

                                        foreach (var matchword in getwordrect)
                                        {
                                            for (int page = 1; page <= reader.NumberOfPages; page++)
                                            {
                                                if (page == matchword.pageNumber)
                                                {
                                                    iTextSharp.text.pdf.PdfContentByte canvas = stamper.GetUnderContent(page);
                                                    iTextSharp.text.pdf.PdfAction action = new iTextSharp.text.pdf.PdfAction(stringUrl);

                                                    iTextSharp.text.Rectangle rect =
                                                        new iTextSharp.text.Rectangle(matchword.iPosX, matchword.iPosY, matchword.iPosX1, matchword.iPosY1);

                                                    rect.SoftCloneNonPositionParameters(rect);

                                                    iTextSharp.text.pdf.PdfAnnotation annchorlink = iTextSharp.text.pdf.PdfAnnotation.CreateLink(canvas.PdfWriter, rect, iTextSharp.text.pdf.PdfAnnotation.HIGHLIGHT_INVERT, action);

                                                    annchorlink.Color = iTextSharp.text.BaseColor.BLUE;

                                                    canvas.AddAnnotation(annchorlink, true);

                                                    canvas.SetColorStroke(iTextSharp.text.BaseColor.BLUE);
                                                    canvas.MoveTo(rect.Left, rect.Bottom);
                                                    canvas.LineTo(rect.Right, rect.Bottom);
                                                    //canvas.Stroke();
                                                    //canvas.RestoreState();

                                                    float[] quad = { rect.Left, rect.Bottom, rect.Right, rect.Bottom, rect.Left, rect.Top, rect.Right, rect.Top };

                                                    iTextSharp.text.pdf.PdfAnnotation underline = iTextSharp.text.pdf.PdfAnnotation.CreateMarkup(canvas.PdfWriter, rect, null, iTextSharp.text.pdf.PdfAnnotation.MARKUP_UNDERLINE, quad);

                                                    underline.Color = iTextSharp.text.BaseColor.BLUE;

                                                    canvas.AddAnnotation(underline, true);
                                                    //canvas.ClosePath();
                                                    canvas.Stroke();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                        }
                    }
                }

                #endregion

                return Json(new { success = true, message = "Successfully applied macros." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        public ActionResult GetApplyMacroCount(int id)
        {
            List<GetMecroCount> countlist = new List<GetMecroCount>();

            try
            {
                var briefDocument = _workspaceDocumentService.GetBriefDocument(id);

                var user = _userService.GetUser(Int32.Parse(User.Identity.Name));

                if (briefDocument == null)
                {
                    throw new BriefLynxServiceException(string.Format("A Brief Document with Id={0} does not exist", id));
                }

                var document = _documentService.GetDocument<BriefDocument>(briefDocument.DocumentID);

                if (document == null)
                {
                    throw new BriefLynxServiceException(string.Format("A Brief Document with Id={0} does not exist", id));
                }

                var filePath = _fileStorageService.EnsureLocalFileExists(document.UriToFile);

                #region save brif document file in temp folder

                var briftmppath = Path.Combine(MacroDocumentUploadDirectory, "brif-" + id);

                var documentpath = Path.Combine(Server.MapPath(briftmppath), "maindocument");
                var macrodocumentpath = Path.Combine(Server.MapPath(briftmppath), "macrodocument");

                if (Directory.Exists(documentpath))
                {
                    Directory.Delete(documentpath, true);
                }
                Directory.CreateDirectory(documentpath);

                if (Directory.Exists(macrodocumentpath))
                {
                    Directory.Delete(macrodocumentpath, true);
                }
                Directory.CreateDirectory(macrodocumentpath);

                // save brif main file
                var maindocumentfilepath = Path.Combine(documentpath, Path.GetFileName(document.UriToFile.AbsoluteUri));
                var documentfilepath = Path.Combine(macrodocumentpath, Path.GetFileName(document.UriToFile.AbsoluteUri));

                System.IO.File.Copy(filePath, maindocumentfilepath);
                //System.IO.File.Copy(filePath, documentfilepath);

                #endregion

                #region apply mecro

                using (FileStream fs = new FileStream(documentfilepath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(filePath))
                    {
                        using (iTextSharp.text.pdf.PdfStamper stamper = new iTextSharp.text.pdf.PdfStamper(reader, fs))
                        {
                            #region link annotation apply in PDF 
                            var macros = _service.MecroList(id).ToList();

                            var globalMecro = _service.GlobalMecroList().ToList();

                            macros = macros.Union(globalMecro).ToList();

                            foreach (var item in macros)
                            {
                                var getmecrorecord = item;

                                if (getmecrorecord != null) 
                                {
                                    var getwordrect = GetPDFWordRects(maindocumentfilepath, getmecrorecord);

                                    if (getwordrect != null && getwordrect.Count > 0)
                                    {
                                        var getcountwithtext = getwordrect
                                            .GroupBy(s => new { s.text, s.iPosX, s.iPosX1, s.iPosY, s.iPosY1})
                                            .Select(s => new GetMecroCount()
                                            {
                                                name = s.FirstOrDefault().text,
                                                count = s.Count()
                                            }).ToList();

                                        countlist.AddRange(getcountwithtext);
                                    }
                                }
                            }
                            #endregion

                        }
                    }
                }

                #endregion

                countlist = countlist.GroupBy(g => g.name).Select(s => new GetMecroCount()
                {
                    name = s.Key,
                    count = s.Sum(ss => ss.count)
                }).ToList();

                return Json(new { success = true, countlist });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private FileStream OpenShareableReadOnlyStream(string filePath)
        {
            return System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public static byte[] ReadFullyBytes(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }


        private List<SearchResultRect> GetPDFWordRects(string tempFilePath, Mecro getmecro)
        {
            List<SearchResultRect> resultRects = new List<SearchResultRect>();
            List<string> mecroList = new List<string>();

            if (getmecro != null)
            {

                List<MecroMatchedData> matchedstring = new List<MecroMatchedData>();

                using (var fileInputStream = System.IO.File.Open(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    using (UglyToad.PdfPig.PdfDocument pdfDoc = UglyToad.PdfPig.PdfDocument.Open(fileInputStream))
                    {
                        for (int i = 1; i <= pdfDoc.NumberOfPages; i++)
                        {
                            var page = pdfDoc.GetPage(i);

                            Regex regEx = new Regex(getmecro.Name, RegexOptions.Multiline | RegexOptions.Compiled);

                            var match = regEx.Matches(page.Text);

                            if (match.Count > 0)
                            {
                                foreach (var sentence in match)
                                {
                                    matchedstring.Add(new MecroMatchedData()
                                    {
                                        text = Convert.ToString(sentence),
                                        pageno = i
                                    });

                                }
                            }

                        }
                    }
                }


                using (var reader = new iTextSharp.text.pdf.PdfReader(tempFilePath))
                {
                    foreach (var item in matchedstring)
                    {

                        myLocationTextExtractionStrategy strategy1 = new myLocationTextExtractionStrategy();
                        string currentText = PdfTextExtractor.GetTextFromPage(reader, item.pageno, strategy1);

                        List<iTextSharp.text.Rectangle> MatchesFound = strategy1.GetTextLocations(item.text, StringComparison.Ordinal);

                        foreach (var word in MatchesFound)
                        {
                            var wordrec = new SearchResultRect()
                            {
                                iPosX = (float)(word.Left),
                                iPosY = (float)(word.Bottom - 2),
                                iPosX1 = (float)(word.Right),
                                iPosY1 = (float)(word.Top),
                                text = item.text,
                                pageNumber = item.pageno
                            };

                            resultRects.Add(wordrec);
                        }

                    }

                }

            }

            return resultRects;
        }


        public class MyLocationTextExtractionStrategy : LocationTextExtractionStrategy
        {
            //Hold each coordinate
            public List<RectAndText> myPoints = new List<RectAndText>();

            //The string that we're searching for
            public String TextToSearchFor { get; set; }

            //How to compare strings
            public System.Globalization.CompareOptions CompareOptions { get; set; }

            public MyLocationTextExtractionStrategy(String textToSearchFor, System.Globalization.CompareOptions compareOptions = System.Globalization.CompareOptions.None)
            {
                this.TextToSearchFor = textToSearchFor;
                this.CompareOptions = compareOptions;
            }

            //Automatically called for each chunk of text in the PDF
            public override void RenderText(TextRenderInfo renderInfo)
            {
                base.RenderText(renderInfo);

                //See if the current chunk contains the text
                var startPosition = System.Globalization.CultureInfo.CurrentCulture.CompareInfo.IndexOf(renderInfo.GetText(), this.TextToSearchFor, this.CompareOptions);

                //If not found bail
                if (startPosition < 0)
                {
                    return;
                }

                //Grab the individual characters
                var chars = renderInfo.GetCharacterRenderInfos().Skip(startPosition).Take(this.TextToSearchFor.Length).ToList();

                //Grab the first and last character
                var firstChar = chars.First();
                var lastChar = chars.Last();


                //Get the bounding box for the chunk of text
                var bottomLeft = firstChar.GetDescentLine().GetStartPoint();
                var topRight = lastChar.GetAscentLine().GetEndPoint();

                //Create a rectangle from it
                var rect = new iTextSharp.text.Rectangle(
                                                        bottomLeft[Vector.I1],
                                                        bottomLeft[Vector.I2],
                                                        topRight[Vector.I1],
                                                        topRight[Vector.I2]
                                                        );

                //Add this to our main collection
                this.myPoints.Add(new RectAndText(rect, this.TextToSearchFor));
            }
        }

        //Helper class that stores our rectangle and text
        public class RectAndText
        {
            public iTextSharp.text.Rectangle Rect;
            public String Text;
            public RectAndText(iTextSharp.text.Rectangle rect, String text)
            {
                this.Rect = rect;
                this.Text = text;
            }
        }

        #endregion

        #region Upload custom file
        [HttpPost]
        [BriefAuthorization]
        public ActionResult CustomeFileUpload(int id, HttpPostedFileBase choosecustfiles = null)
        {
            if (choosecustfiles == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No files were provided to upload");
            }

            var customUpload = Path.Combine(Server.MapPath(CustomFileUploadDirectory), string.Format("brief-{0}", id));

            if (!Directory.Exists(customUpload))
            {
                Directory.CreateDirectory(customUpload);
            }

            var tmpfilename = Path.GetFileNameWithoutExtension(choosecustfiles.FileName);
            var tmpfileexten = Path.GetExtension(choosecustfiles.FileName);

            var custfilename = string.Format("{0}_{1}{2}", tmpfilename, DateTime.Now.Ticks, tmpfileexten);

            var custfilepath = Path.Combine(customUpload, custfilename);

            choosecustfiles.SaveAs(custfilepath);

            return Json(custfilename, "text/plain");
        }

        #endregion

        #region Methods

        private static IQueryable<BriefDocument> ApplyBriefDocumentFilterDescriptor(
            IQueryable<BriefDocument> briefDocuments, FilterDescriptor descriptor)
        {
            if (descriptor.Member == "Format")
            {
                var formatTypeId = Convert.ToInt32(descriptor.ConvertedValue);
                switch (descriptor.Operator)
                {
                    case FilterOperator.IsEqualTo:
                        briefDocuments = briefDocuments.Where(bd => bd.DocumentFormatTypeID == formatTypeId);
                        break;
                    case FilterOperator.IsNotEqualTo:
                        briefDocuments = briefDocuments.Where(bd => bd.DocumentFormatTypeID != formatTypeId);
                        break;
                }
            }
            else if (descriptor.Member == "UploadStatus")
            {
                var uploadStatusId = Convert.ToInt32(descriptor.ConvertedValue);
                switch (descriptor.Operator)
                {
                    case FilterOperator.IsEqualTo:
                        briefDocuments = briefDocuments.Where(bd => bd.DocumentUploadStatusID == uploadStatusId);
                        break;
                    case FilterOperator.IsNotEqualTo:
                        briefDocuments = briefDocuments.Where(bd => bd.DocumentUploadStatusID != uploadStatusId);
                        break;
                }
            }
            else if (descriptor.Member == "IsLinkedTo")
            {
                var value = Convert.ToBoolean(descriptor.ConvertedValue);
                briefDocuments = value ? briefDocuments.Where(bd => bd.AttachmentLinks.Any()) : briefDocuments.Where(bd => !bd.AttachmentLinks.Any());
            }
            return briefDocuments;
        }

        private static IQueryable<BriefDocument> ApplyBriefDocumentFilteringAndSorting(
            DataSourceRequest request, IQueryable<BriefDocument> briefDocuments)
        {
            var sortsToRemove = new List<SortDescriptor>();
            var orderByClauses = new List<OrderByDescriptor<BriefDocument>>();
            var filtersToRemove = new List<IFilterDescriptor>();

            foreach (var sort in request.Sorts)
            {
                if (sort.Member == "IsLinkedTo")
                {
                    sortsToRemove.Add(sort);
                    orderByClauses.Add(new OrderByDescriptor<BriefDocument>(bd => bd.AttachmentLinks.Any(), sort.SortDirection));
                }
                if (sort.Member == "UploadStatus")
                {
                    sortsToRemove.Add(sort);
                    orderByClauses.Add(new OrderByDescriptor<BriefDocument>(bd => bd.DocumentUploadStatusID, sort.SortDirection));
                }
                if (sort.Member == "Format")
                {
                    sortsToRemove.Add(sort);
                    orderByClauses.Add(new OrderByDescriptor<BriefDocument>(bd => bd.DocumentFormatTypeID, sort.SortDirection));
                }
            }

            foreach (var filter in request.Filters)
            {
                var compositeFilterDescriptor = filter as CompositeFilterDescriptor;
                var filterDescriptor = filter as FilterDescriptor;
                if (compositeFilterDescriptor != null)
                {
                    briefDocuments = RecurseCompositeFilterDescriptor(briefDocuments, compositeFilterDescriptor);

                    filtersToRemove.Add(filter);
                }
                else if (filterDescriptor != null)
                {
                    briefDocuments = ApplyBriefDocumentFilterDescriptor(briefDocuments, filterDescriptor);

                    filtersToRemove.Add(filterDescriptor);
                }
            }

            foreach (var descriptor in sortsToRemove)
            {
                request.Sorts.Remove(descriptor);
            }

            foreach (var filter in filtersToRemove)
            {
                request.Filters.Remove(filter);
            }

            if (orderByClauses.Any())
            {
                IOrderedEnumerable<BriefDocument> orderedEnumerable = null;

                foreach (var clause in orderByClauses)
                {
                    if (orderedEnumerable == null)
                    {
                        orderedEnumerable = clause.SortDirection == ListSortDirection.Ascending
                                                ? briefDocuments.OrderBy(clause.OrderByClause)
                                                : briefDocuments.OrderByDescending(clause.OrderByClause);
                    }
                    else
                    {
                        orderedEnumerable = clause.SortDirection == ListSortDirection.Ascending
                                                ? orderedEnumerable.ThenBy(clause.OrderByClause)
                                                : orderedEnumerable.ThenByDescending(clause.OrderByClause);
                    }
                }

                briefDocuments = orderedEnumerable.AsQueryable();
            }
            return briefDocuments;
        }

        private static IQueryable<BriefDocument> RecurseCompositeFilterDescriptor(
            IQueryable<BriefDocument> briefDocuments, CompositeFilterDescriptor compositeFilterDescriptor)
        {
            //assume an AND logical operator
            foreach (var descriptor in compositeFilterDescriptor.FilterDescriptors)
            {
                var composite = descriptor as CompositeFilterDescriptor;
                var singleFilter = descriptor as FilterDescriptor;

                if (composite != null)
                {
                    briefDocuments = RecurseCompositeFilterDescriptor(briefDocuments, composite);
                }
                else if (singleFilter != null)
                {
                    briefDocuments = ApplyBriefDocumentFilterDescriptor(briefDocuments, singleFilter);
                }
            }
            return briefDocuments;
        }

        private ActionResult GetBriefDetailResponse(int? briefId, bool isnewbrief)
        {
            ViewBag.data = 1;

            var model = new BriefViewModel(_securityService);

            string shareFileAuthToken;
            string shareFileUrl;
            model.ShareFile.IsAuthenticated = ShareFileLogOnViewModel.IsAuthenticated(Request, out shareFileAuthToken, out shareFileUrl);

            var menuCookie = Request.Cookies[MenuCookieModel.CookieName] ?? new HttpCookie(MenuCookieModel.CookieName);

            try
            {
                if (briefId.HasValue)
                {
                    var brief = _service.GetBrief(briefId.GetValueOrDefault());
                    model.SetBrief(brief);
                    if (Request.IsAjaxRequest())
                    {

                        //var htmlstringbrief = RenderPartialString.RenderPartialToString(ControllerContext, "~/Views/Brief/_BriefDetail.cshtml", model, true);

                        return Json(new
                        {
                            success = true,
                            //htmlstring = htmlstringbrief,
                            isnew = isnewbrief,
                            isbriefdetails = true,
                            briefid = briefId.Value,
                            isredirectbrief = true
                        });//PartialView("_BriefDetail", model);
                    }

                    var cookieValue = MenuCookieModel.GetCookieValue(menuCookie.Value, _httpServerUtility);

                    cookieValue.BriefId = briefId.Value;

                    Response.Cookies.Add(MenuCookieModel.CreateCookie(cookieValue, _httpServerUtility));

                    //var htmlstringIndex = RenderPartialString.RenderPartialToString(ControllerContext, "~/Views/Brief/IndexNew.cshtml", model, true);

                    Redirect(Url.Action("Index", new { id = default(int?) }));

                }
                else
                {
                    var cookieValue = MenuCookieModel.GetCookieValue(menuCookie.Value, _httpServerUtility);

                    if (!cookieValue.BriefId.HasValue)
                    {
                        return Json(new { success = false });//RedirectToAction("Index", "Matter");
                    }

                    var brief = _service.GetBrief(cookieValue.BriefId.Value);

                    if (brief != null)
                    {
                        model.SetBrief(brief);
                    }
                    else
                    {
                        return Json(new { success = false });//RedirectToAction("Index", "Matter");
                    }
                }

                //var htmlstring = RenderPartialString.RenderPartialToString(ControllerContext, "~/Views/Brief/IndexNew.cshtml", model, true);

                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = true,
                        //htmlstring = htmlstring,
                        isnew = isnewbrief,
                        briefid = model.BriefId,
                        isredirectbrief = true
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View(model);
                }
            }
            catch (JsonException ex)
            {
                AcgTrace.WriteWarning(string.Format("Failed deserializing Menu Cookie Value: {0}", menuCookie.Value), LogCategory);
                return Json(new { success = false }); //RedirectToAction("Index", "Matter");
            }
        }

        private void SaveBriefPackageSettings(BriefPackageSettingsViewModel model, Brief brief)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            if (brief == null)
            {
                throw new ArgumentNullException("brief");
            }

            if (brief.MasterDocument != null)
            {
                brief.MasterDocument.PackageFileName = !string.IsNullOrEmpty(model.BriefPackageFileName) ? model.BriefPackageFileName : brief.Name;
            }

            brief.Options.AddPackageIndex = model.IncludeIndexPage;
            brief.Options.AddReturnButtons = model.IncludeReturnLinksInPackage;
            brief.Options.GenerateIpadBriefType = (int)model.IpadCompatibleGenerationType;
            brief.Options.GenerateMacLinks = model.LinkCompatibility.Equals(LinkFilePathSeparatorType.OSX);
            brief.Options.UnderlineLinks = model.UnderlineLinks;
            brief.Options.MecroIds = string.Empty;
            //if (model.mecroids != null)
            //{
            //    brief.Options.MecroIds = string.Join(",", model.mecroids);
            //}
            //else
            //{
            //    brief.Options.MecroIds = string.Empty;
            //}

            _service.CommitAllChanges();
        }

        private void ValidateShareRange(int briefId, int startPage, int endPage)
        {
            if (startPage <= 0 || endPage <= 0)
            {
                throw new InvalidOperationException("Start and end page must be great than zero.");
            }
            else if (startPage <= endPage)
            {
                var brief = _service.GetBrief(briefId);

                var overlaps = from s in brief.Shares
                               where s.ShareStatusEnumeration == ShareStatusEnumeration.Shared
                                    &&
                                    ((s.StartPage >= startPage && s.StartPage <= endPage)
                                    || (s.StartPage < startPage && (s.EndPage >= startPage && s.EndPage <= endPage))
                                    || (s.StartPage < startPage && s.EndPage > endPage))
                               select s;

                if (overlaps.Any())
                {
                    throw new InvalidOperationException("Page range cannot overlap existing active shares.");
                }
            }
            else
            {
                throw new InvalidOperationException("End Page must be greater than or equal to the Start Page.");
            }
        }


        private bool RemovedLicenseFromHtml(string htmlfilepath)
        {
            using (WebClient client = new WebClient())
            {
                var htmlData = client.DownloadData(htmlfilepath);
                string html = Encoding.UTF8.GetString(htmlData);

                #region update html file and removed license text from html
                html = html.Replace("Created by unlicensed version of PDF Focus .Net 7.8.1.29!", string.Empty);
                html = html.Replace("The unlicensed version inserts &quot;trial&quot; into random places.", string.Empty);
                html = html.Replace("<a href=\"https://www.sautinsoft.com/products/pdf-focus/order.php\"><span class=\"ssdspan cs15\" style=\"left:0pt\">This text will disappear after purchasing the license.</span></a>", string.Empty);
                html = html.Replace("<a href=\"https://www.sautinsoft.com/products/pdf-focus/order.php\">", string.Empty);
                html = html.Replace("This text will disappear after purchasing the license.", string.Empty);
                html = html.Replace("matrix(1,0,0,1,-191.7419921875,-781.95546875)", string.Empty);
                html = html.Replace(" M185,729 L605,729 L605,788 L185,788 Z", string.Empty);
                #endregion

                System.IO.File.WriteAllText(htmlfilepath, String.Empty);

                using (FileStream fs = new FileStream(htmlfilepath, FileMode.Open, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(html);

                        sw.Close();
                    }

                }
            }

            return true;
        }

        [BriefAuthorization]
        public ActionResult Mecro(int id)
        {
            ViewBag.data = 2;
            Brief brief = null;

            brief = _service.GetBrief(id);


            var model = new BriefViewModel(_securityService, brief);

            return View(model);
        }

        [HttpPost]
        public ActionResult GetMecro([DataSourceRequest] DataSourceRequest request, int id)
        {
            List<Mecro> MecroModels = new List<Mecro>();

            MecroModels = _service.MecroList(id).ToList();

            MecroModels.ForEach(f =>
            {
                f.Brief = null;
                f.Exhibit = null;
            });

            return Json(MecroModels.OrderByDescending(o => o.Id).ToDataSourceResult(request));
        }

        [HttpPost]
        public ActionResult GetAllMecro([DataSourceRequest] DataSourceRequest request, int id)
        {
            ViewBag.data = 2;
            List<Mecro> MecroModels = new List<Mecro>();

            MecroModels = _service.MecroList(id).ToList();

            MecroModels.ForEach(f =>
            {
                f.Brief = null;
            });

            var globalMecro = _service.GlobalMecroList().ToList();

            MecroModels = MecroModels.Union(globalMecro).ToList();

            return Json(MecroModels.OrderByDescending(o => o.Id).ToDataSourceResult(request));
        }

        [HttpPost]
        [BriefAuthorization]
        public ActionResult MecroFileUpload(int id, int mecroid, string mecroname, HttpPostedFileBase choosecustfiles = null)
        {
            if (choosecustfiles == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No files were provided to upload");
            }

            var customUpload = Path.Combine(Server.MapPath(MecroFileUploadDirectory), string.Format("brief-{0}", id));

            if (!Directory.Exists(customUpload))
            {
                Directory.CreateDirectory(customUpload);
            }

            var tmpfilename = Path.GetFileNameWithoutExtension(choosecustfiles.FileName);
            var tmpfileexten = Path.GetExtension(choosecustfiles.FileName);

            var custfilename = string.Format("{0}_{1}{2}", tmpfilename, DateTime.Now.Ticks, tmpfileexten);

            var custfilepath = Path.Combine(customUpload, custfilename);

            choosecustfiles.SaveAs(custfilepath);

            var user = _userService.GetUser(Int32.Parse(User.Identity.Name));

            Mecro mecroData = new Mecro()
            {
                BriefID = id,
                Name = mecroname,
                Id = mecroid,
                FileName = custfilename,
                CreatedBy = user.UserName,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = user.UserName,
                UpdatedDate = DateTime.UtcNow
            };

            _service.SaveMecro(mecroData);

            return Json(custfilename, "text/plain");
        }

        [HttpPost]
        public ActionResult DeleteMecro(int id, int mecroid)
        {
            try
            {
                _service.DeleteMecro(mecroid);

                return Json(new { issuccess = true, message = "Successfully deleted record." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { issuccess = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddMecroURL(int id, int mecroid, string name, string url)
        {
            try
            {

                var user = _userService.GetUser(Int32.Parse(User.Identity.Name));

                Mecro mecroData = new Mecro()
                {
                    BriefID = id,
                    Name = name,
                    Id = mecroid,
                    URL = url,
                    CreatedBy = user.UserName,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = user.UserName,
                    UpdatedDate = DateTime.UtcNow
                };

                _service.SaveMecro(mecroData);

                return Json(new { issuccess = true, message = "Successfully add/update Macro." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { issuccess = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddMecroExhibit(int id, int mecroid, string name, int exhibitid)
        {
            ViewBag.data = 1;
            try
            {

                var user = _userService.GetUser(Int32.Parse(User.Identity.Name));

                Mecro mecroData = new Mecro()
                {
                    BriefID = id,
                    Name = name,
                    Id = mecroid,
                    ExhibitId = exhibitid,
                    CreatedBy = user.UserName,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = user.UserName,
                    UpdatedDate = DateTime.UtcNow
                };

                _service.SaveMecro(mecroData);

                return Json(new { issuccess = true, message = "Successfully add/update Macro." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { issuccess = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Update Brief Update functionality
        [MatterAuthorization]
        public ActionResult GetAddPopup(int id)
        {
            var matter = _matterService.GetMatter(id);

            if (matter == null)
            {
                return new HttpNotFoundResult("Matter not found.");
            }
            try
            {
                var Htmlstring = RenderPartialString.RenderPartialToString(ControllerContext,
                                "~/Views/Brief/Add.cshtml",
                                new BriefViewModel(_securityService) { MatterId = id, BreadCrumb = new BreadCrumbViewModel(matter) },
                                true);

                return Json(new { success = true, htmlstring = Htmlstring }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = true, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Update Brief Detail functionality
        public ActionResult BriefDetailPopup(int? id)
        {
            ViewBag.data = 1;
            var model = new BriefViewModel(_securityService);

            string shareFileAuthToken;
            string shareFileUrl;
            model.ShareFile.IsAuthenticated = ShareFileLogOnViewModel.IsAuthenticated(Request, out shareFileAuthToken, out shareFileUrl);

            var menuCookie = Request.Cookies[MenuCookieModel.CookieName] ?? new HttpCookie(MenuCookieModel.CookieName);

            try
            {
                if (id.HasValue)
                {
                    var brief = _service.GetBrief(id.GetValueOrDefault());
                    model.SetBrief(brief);
                    if (Request.IsAjaxRequest())
                    {

                        var htmlstringbrief = RenderPartialString.RenderPartialToString(ControllerContext, "~/Views/Brief/_BriefDetail.cshtml", model, true);

                        return Json(new { success = true, htmlstring = htmlstringbrief }, JsonRequestBehavior.AllowGet);//PartialView("_BriefDetail", model);
                    }

                    var cookieValue = MenuCookieModel.GetCookieValue(menuCookie.Value, _httpServerUtility);

                    cookieValue.BriefId = id.Value;

                    Response.Cookies.Add(MenuCookieModel.CreateCookie(cookieValue, _httpServerUtility));

                    var htmlstringIndex = RenderPartialString.RenderPartialToString(ControllerContext, "~/Views/Brief/IndexNew.cshtml", model, true);

                    return Json(new { success = true, htmlstring = htmlstringIndex }, JsonRequestBehavior.AllowGet);
                    //Redirect(Url.Action("Index", new { id = default(int?) }));

                }
                else
                {
                    var cookieValue = MenuCookieModel.GetCookieValue(menuCookie.Value, _httpServerUtility);

                    if (!cookieValue.BriefId.HasValue)
                    {
                        return Json(new { success = false }, JsonRequestBehavior.AllowGet);//RedirectToAction("Index", "Matter");
                    }

                    var brief = _service.GetBrief(cookieValue.BriefId.Value);

                    if (brief != null)
                    {
                        model.SetBrief(brief);
                    }
                    else
                    {
                        return Json(new { success = false }, JsonRequestBehavior.AllowGet);//RedirectToAction("Index", "Matter");
                    }
                }

                var htmlstring = RenderPartialString.RenderPartialToString(ControllerContext, "~/Views/Brief/IndexNew.cshtml", model, true);

                return Json(new { success = true, htmlstring = htmlstring }, JsonRequestBehavior.AllowGet);
                //return View(model);
            }
            catch (JsonException ex)
            {
                AcgTrace.WriteWarning(string.Format("Failed deserializing Menu Cookie Value: {0}", menuCookie.Value), LogCategory);
                return Json(new { success = false }, JsonRequestBehavior.AllowGet); //RedirectToAction("Index", "Matter");
            }
        }
        #endregion

        #region Brief Settings functionality

        [BriefAuthorization]
        public ActionResult SettingsPopup(int? id)
        {
            Brief brief = null;
            if (id.HasValue)
            {
                brief = _service.GetBrief(id.GetValueOrDefault());
            }

            var model = new BriefSettingsViewModel(_securityService, brief);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.StatusMessage = TempData["message"];
            }

            var htmlstringbrief = RenderPartialString.RenderPartialToString(ControllerContext, "~/Views/Brief/SettingsNew.cshtml", model, true);

            return Json(new { success = true, htmlstring = htmlstringbrief }, JsonRequestBehavior.AllowGet);  //View("Settings", model);
        }
        #endregion

        #region Brief Package
        [BriefAuthorization]
        public ActionResult PackagePopup(int id)
        {
            var brief = _service.GetBrief(id);

            if (TempData.ContainsKey("ValidationMessage"))
            {
                ViewBag.Message = TempData["ValidationMessage"];
            }
            else if (brief.IsShared)
            {
                ViewBag.Message = "A Brief Package cannot be generated until all shares have been completed or canceled.";
            }

            var model = new BriefPackageViewModel(_securityService, brief)
            {
                PackagingTaskId = _packagingService.GetCurrentBriefPackagingTask(brief),
                AllowPackagingTaskCancelation =
                                    _securityService.CanCurrentUserEditThisBrief(brief.BriefID)
            };

            var htmlstringbrief = RenderPartialString.RenderPartialToString(ControllerContext, "~/Views/Brief/PackageNew.cshtml", model, true);

            return Json(new { success = true, htmlstring = htmlstringbrief }, JsonRequestBehavior.AllowGet); //return View(model);
        }


        [HttpPost]
        [BriefAuthorization]
        public ActionResult PackageNew(int id, BriefPackageSettingsViewModel model)
        {
            var brief = _service.GetBrief(id);

            if (brief == null)
            {
                return Json(new { success = false, message = "Brief does not exist" }); //new HttpNotFoundResult("Brief does not exist");
            }

            if (ModelState.IsValid)
            {
                SaveBriefPackageSettings(model, brief);

                var packagingTaskId = _packagingService.GenerateBriefPackage(brief.BriefID);

                var responseModel = new BriefPackageViewModel(_securityService, brief)
                {
                    PackageSettings = model,
                    PackagingTaskId = packagingTaskId,
                    AllowPackagingTaskCancelation =
                                                _securityService.CanCurrentUserEditThisBrief(brief.BriefID)
                };

                responseModel.PackageSettings.PackageGenerationStatus = PackageStatusEnumeration.Generating;

                responseModel.mecroIds = new List<int>();

                var htmlstringbrief = RenderPartialString.RenderPartialToString(ControllerContext, "~/Views/Brief/PackageNew.cshtml", responseModel, true);

                return Json(new { success = true, title = "Brief Package", htmlstring = htmlstringbrief }, JsonRequestBehavior.AllowGet);

            }

            var htmlstringpackagebrief = RenderPartialString.RenderPartialToString(ControllerContext, "~/Views/Brief/PackageNew.cshtml", model, true);

            return Json(new { success = true, title = "Brief Package", htmlstring = htmlstringpackagebrief }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult NewPackageSettings(int id, BriefPackageSettingsViewModel model)
        {
            var brief = _service.GetBrief(id);

            if (brief == null)
            {
                return Json(new { success = false, message = "Brief does not exist" });
            }

            if (ModelState.IsValid)
            {
                SaveBriefPackageSettings(model, brief);
            }
            else
            {
                return Json(new { success = false, message = "You have submitted incomplete or invalid data.  Please check the form and try again." });
            }

            return Json(new { success = true, message = "Package successfully saved" });
        }
        #endregion

        #region Mecro Details Popup
        [BriefAuthorization]
        public ActionResult MecroPopup(int id)
        {
            ViewBag.data = 1;

            Brief brief = null;

            brief = _service.GetBrief(id);


            var model = new BriefViewModel(_securityService, brief);

            var htmlstringmacrobrief = RenderPartialString.RenderPartialToString(ControllerContext, "~/Views/Brief/Mecro.cshtml", model);

            return Json(new { success = true, title = "Brief Macro", htmlstring = htmlstringmacrobrief }, JsonRequestBehavior.AllowGet);

            //return View(model);
        }

        public ActionResult GetExpressionMatchesForHtmlPreview(string expression, string targetText)
        {
            ViewBag.data = 2;
            try
            {
                var decodedTargetText = targetText;//.Base64ToUtf8();
                var htmlPreview = new StringBuilder(decodedTargetText);
                var matches = GetExpressionMatches(expression, decodedTargetText, false);//Base64ToUtf8()
                if (matches.Any())
                {
                    foreach (var match in matches)
                    {
                        htmlPreview.Replace(match, "<span class=\"ExpressionTestMatch\">{0}</span>".FormatCurrent(match));
                    }

                    return Json(new
                    {
                        status = 1,
                        matchedText = htmlPreview.ToString().Utf8ToBase64()
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    status = 1,
                    matchedText = "<span class=\"Error\">The expression did not generate any matches in the target text.</span>".Utf8ToBase64()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = 0,
                    msg = "Something went wrong! Unable to process."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private string[] GetExpressionMatches(string expression, string targetText, bool encodedStrings)
        {
            if (string.IsNullOrEmpty(expression) || string.IsNullOrEmpty(targetText))
            {
                return new string[0];
            }

            if (encodedStrings)
            {
                expression = expression.Base64ToUtf8();
                targetText = targetText.Base64ToUtf8();
            }

            try
            {
                return _automatedLinkingService.GetLinkMatches(targetText, expression).OrderBy(m => m.First).Select(
                        m => encodedStrings ? m.Second.Utf8ToBase64() : m.Second).ToArray();
            }
            catch (FormatException)
            {
                //expression format is invalid
                return new string[0];
            }
        }
        #endregion
    }
}