using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using FoundryLaw.Common.CustomExceptions;
using FoundryLaw.Model.ViewModels;
using FoundryLaw.Service.Data.Model.Contract;
using FoundryLaw.Service.Setup;
using System.Security.Claims;
using System.Runtime.InteropServices;

namespace FoundryLaw.Api.Startup.Setup.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        readonly IServiceResult _serviceResultErrorResponse;
        private readonly GlobalLogic _globalLogic;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IServiceResult serviceResultErrorResponse)
        {
            _globalLogic = new GlobalLogic();
            _logger = logger;
            _next = next;
            _serviceResultErrorResponse = serviceResultErrorResponse;
            _serviceResultErrorResponse = _globalLogic.BuildServiceResult(serviceResultErrorResponse, false);
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (UnauthorizedAccessException ex)
            {
                HandleException(httpContext, ex);
            }
            catch (CustomValidationException ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
            catch (Exception ex)
            {
                if (httpContext.Response.StatusCode == 403)
                    await HandleForbidExceptionAsync(httpContext, ex);
                else
                    await HandleExceptionAsync(httpContext, ex);
            }
        }

        #region exception handlers
        private void HandleException(HttpContext context, UnauthorizedAccessException exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
        private async Task HandleExceptionAsync(HttpContext context, CustomValidationException exception)
        {
            Guid guid = Guid.NewGuid();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

             //_serviceResultErrorResponse.Message = JsonConvert.SerializeObject(exception.validationMessages);
            _serviceResultErrorResponse.Message = "Some thing went wrong";
            _serviceResultErrorResponse.ResultData = new { errorId = guid };

            var loggedinUser = GetUser(context);
            //string errorMessage = $"Validations failed:{string.Join(Environment.NewLine, exception.validationMessages)}. ErrorId: {guid}.";
            string errorMessage = "Some thing went wrong";
            if (loggedinUser != null)
            {
                errorMessage += $" Logged in userId: {loggedinUser.id}";
            }

            _logger.LogWarning(errorMessage);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(_serviceResultErrorResponse));
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            Guid guid = Guid.NewGuid();
            context.Response.ContentType = "application/json";
            if (context.Response.StatusCode != 403)
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            _serviceResultErrorResponse.Message = exception.Message;
            _serviceResultErrorResponse.ResultData = new { errorId = guid };

            var loggedinUser = GetUser(context);
            string errorMessage = $"{exception.Message}. ErrorId: {guid}.";
            if (loggedinUser != null)
            {
                errorMessage += $" Logged in userId: {loggedinUser.id}";
            }
            _logger.LogError(exception, errorMessage);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(_serviceResultErrorResponse));
        }
        private async Task HandleForbidExceptionAsync(HttpContext context, Exception exception)
        {
            Guid guid = Guid.NewGuid();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            _serviceResultErrorResponse.Message = exception.Message;
            _serviceResultErrorResponse.ResultData = new { errorId = guid };
            _serviceResultErrorResponse.StatusCode = (int)HttpStatusCode.Forbidden;
            var loggedinUser = GetUser(context);
            string errorMessage = $"{exception.Message}. ErrorId: {guid}.";
            if (loggedinUser != null)
            {
                errorMessage += $" Logged in userId: {loggedinUser.id}";
            }
            _logger.LogError(exception, errorMessage);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(_serviceResultErrorResponse));
        }
        #endregion
        private AuthViewModel GetUser(HttpContext context)
        {
            var userData = context.User.FindFirstValue(ClaimTypes.UserData);
            if (userData == null)
                return null;

            var resp = (AuthViewModel)JsonConvert.DeserializeObject(userData, typeof(AuthViewModel));
            return resp;
        }
    }
}