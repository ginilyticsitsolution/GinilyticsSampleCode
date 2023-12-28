using FinnlyS.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinnlyS.Shared.API.Schedule;
using FinnlyS.Server.Services;
using FinnlyS.Server.Data.API;
using FinnlyS.Server.Helpers;
using System.Security.Claims;
using FinnlyS.Server.Data.Facility;
using FinnlyS.Shared.Facility;
using System.Collections.Generic;
using FinnlyS.Shared.Site;
using FinnlyS.Server.Data.Site;
using System;

namespace FinnlyS.Server.Controllers.API
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class API_ScheduleController : ControllerBase
    {
        private readonly iJwtTokenService _jwt;
        private readonly iDbConnection _dbConn;
        private readonly iAPI_ScheduleData _API_ScheduleData;
        private readonly iFAC_Facility _facilityData;
        private readonly iFAC_EventType _eventTypeData;
        private readonly iSIT_Site _siteData;

        public API_ScheduleController(iJwtTokenService Jwt, iDbConnection DbConn, iAPI_ScheduleData API_ScheduleData, iFAC_Facility FacilityData, iSIT_Site SiteData, iFAC_EventType EventTypeData)
        {
            _jwt = Jwt;
            _dbConn = DbConn;
            _API_ScheduleData = API_ScheduleData;
            _facilityData = FacilityData;
            _eventTypeData = EventTypeData;
            _siteData = SiteData;
        }

        [HttpGet]
        [Route("events/{FacilityId}/{StartDate}/{EndDate}/{EventType?}")]
        public IActionResult GetEventList(long FacilityId, string StartDate, string EndDate, string EventType)
        {
            //Make sure user is a Reach user and for the correct site
            var userIdentity = new cUserIdentity((ClaimsIdentity)User.Identity);
            cActionResult<bool> rightsReachAction = userIdentity.IsFacilityValidForReachSite(_dbConn.ReadOnlyDbConnection, _facilityData, FacilityId);
            if (!rightsReachAction.Success)
            {
                return BadRequest("Get Failed");
            }
            cActionResult<bool> rightsDigitalAction = userIdentity.IsFacilityValidForDigitalSite(_dbConn.ReadOnlyDbConnection, _facilityData, FacilityId);
            if (!rightsDigitalAction.Success)
            {
                return BadRequest("Get Failed");
            }
            if (!rightsReachAction.Result && !rightsDigitalAction.Result && !userIdentity.IsFinnlyAdmin)
            {
                return Unauthorized("User does not have rights to the site.");
            }

            cActionResult<cAPI_ListEventsResponse> listEventAction = _API_ScheduleData.ListEvents(new cAPI_ListEventsRequest() { facilityId = FacilityId, startDate = StartDate, endDate = EndDate, eventType = EventType }, _dbConn.DbConnection, false);
            if (listEventAction.Success)
            {
                return Ok(listEventAction.Result);
            }
            else
            {
                return BadRequest("Get Failed");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("eventsanon/{FacilityId}/{StartDate}/{EndDate}/{EventType?}")]
        public IActionResult GetEventListAnon(long FacilityId, string StartDate, string EndDate, string EventType)
        {
            //Make sure user is a Reach user and for the correct site
            var userIdentity = new cUserIdentity((ClaimsIdentity)User.Identity);
            cActionResult<bool> rightsReachAction = userIdentity.IsFacilityValidForReachSite(_dbConn.ReadOnlyDbConnection, _facilityData, FacilityId);
            if (!rightsReachAction.Success)
            {
                return BadRequest("Get Failed");
            }
            cActionResult<bool> rightsDigitalAction = userIdentity.IsFacilityValidForDigitalSite(_dbConn.ReadOnlyDbConnection, _facilityData, FacilityId);
            if (!rightsDigitalAction.Success)
            {
                return BadRequest("Get Failed");
            }
            //if (!rightsReachAction.Result && !rightsDigitalAction.Result && !userIdentity.IsFinnlyAdmin)
            //{
            //    return Unauthorized("User does not have rights to the site.");
            //}

            cActionResult<cAPI_ListEventsResponse> listEventAction = _API_ScheduleData.ListEvents(new cAPI_ListEventsRequest() { facilityId = FacilityId, startDate = StartDate, endDate = EndDate, eventType = EventType }, _dbConn.DbConnection,false);
            if (listEventAction.Success)
            {
                return Ok(listEventAction.Result);
            }
            else
            {
                return BadRequest("Get Failed");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("ddeventsanon/{FacilityId}/{LeadTime}/{TrailTime}")]
        public IActionResult GetDdEventListAnon(long FacilityId, int LeadTime, int TrailTime)
        {
            //Make sure user is a Reach user and for the correct site
            var userIdentity = new cUserIdentity((ClaimsIdentity)User.Identity);
            cActionResult<bool> rightsReachAction = userIdentity.IsFacilityValidForReachSite(_dbConn.ReadOnlyDbConnection, _facilityData, FacilityId);
            if (!rightsReachAction.Success)
            {
                return BadRequest("Get Failed");
            }
            cActionResult<bool> rightsDigitalAction = userIdentity.IsFacilityValidForDigitalSite(_dbConn.ReadOnlyDbConnection, _facilityData, FacilityId);
            if (!rightsDigitalAction.Success)
            {
                return BadRequest("Get Failed");
            }
            //if (!rightsReachAction.Result && !rightsDigitalAction.Result && !userIdentity.IsFinnlyAdmin)
            //{
            //    return Unauthorized("User does not have rights to the site.");
            //}
            cActionResult<DateTime> currentTimeAction = _facilityData.GetFacilityTime(_dbConn.ReadOnlyDbConnection, FacilityId);
            if (!currentTimeAction.Success)
            {
                return BadRequest("Get Failed");
            }
            string startDate = currentTimeAction.Result.AddMinutes(-LeadTime).ToString("s");
            string endDate = currentTimeAction.Result.AddMinutes(TrailTime).ToString("s");

            cActionResult<cAPI_ListEventsResponse> listEventAction = _API_ScheduleData.ListEvents(new cAPI_ListEventsRequest() { facilityId = FacilityId, startDate = startDate, endDate = endDate, eventType = null }, _dbConn.DbConnection,true);
            if (listEventAction.Success)
            {
                return Ok(listEventAction.Result);
            }
            else
            {
                return BadRequest("Get Failed");
            }
        }

        [HttpGet]
        [Route("facilities/{SiteId}")]
        public IActionResult GetBySiteId(long SiteId)
        {
            //Make sure user is a Reach user and for the correct site
            var userIdentity = new cUserIdentity((ClaimsIdentity)User.Identity);
            cActionResult<bool> rightsAction = userIdentity.IsSiteValidForReachSite(SiteId);
            if (!rightsAction.Success)
            {
                return BadRequest("Get Failed");
            }
            if (!rightsAction.Result)
            {
                return Unauthorized("User does not have rights to the site.");
            }

            cActionResult<List<cFAC_FacilityModel>> facilityGetAction = _facilityData.GetFacilityListBySiteId(_dbConn.DbConnection, SiteId);
            if (facilityGetAction.Success)
            {
                return Ok(facilityGetAction.Result);
            }
            else
            {
                return BadRequest("Get Failed");
            }
        }

        [HttpGet]
        [Route("eventtypes/{SiteId}")]
        public IActionResult GetEventTypesBySiteId(long SiteId)
        {
            //Make sure user is a Reach user and for the correct site
            var userIdentity = new cUserIdentity((ClaimsIdentity)User.Identity);
            cActionResult<bool> rightsAction = userIdentity.IsSiteValidForReachSite(SiteId);
            if (!rightsAction.Success)
            {
                return BadRequest("Get Failed");
            }
            if (!rightsAction.Result)
            {
                return Unauthorized("User does not have rights to the site.");
            }

            cActionResult<List<cFAC_EventTypeModel>> eventTypeGetAction = _eventTypeData.GetEventTypeList(_dbConn.DbConnection, SiteId);

            if (eventTypeGetAction.Success)
            {
                return Ok(eventTypeGetAction.Result);
            }
            else
            {
                return BadRequest("Get Failed");
            }
        }

        [HttpGet]
        [Route("sites")]
        public IActionResult GetSites()
        {
            var userIdentity = new cUserIdentity((ClaimsIdentity)User.Identity);
            cActionResult<List<cSIT_SiteModel>> siteGetAction = _siteData.GetSiteList(_dbConn.DbConnection, userIdentity);
            if (siteGetAction.Success)
            {
                return Ok(siteGetAction.Result);
            }
            else
            {
                return BadRequest("Get Failed");
            }
        }
    }
}
