using AutoMapper.Configuration;
using CryptoTime.Models.ViewModels;
using CryptoTime.Service.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CryptoTime.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class CoinController : BaseController
    {
        private readonly ICoinService _coinService;
        private readonly string hashKey;

        public CoinController(ICoinService service, IOptions<AppSettings> appSettings)
        {
            _coinService = service;
            hashKey = appSettings.Value.JobSecurityHash;
        }

        [HttpGet]
        public IActionResult GetAllCoins(int userId)
        {
            return Ok(_coinService.GetAllCoins(userId));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AddCoin()
        {
            string authHeader = HttpContext.Request.Headers["Authorization"];
            if (hashKey == authHeader)
            {
                return Ok(_coinService.AddCoin());
            }
            {
                return Unauthorized();
            }
        }

        [HttpGet]
        public IActionResult GetPrice(string coinId, string currencies)
        {
            return Ok(_coinService.GetPrice(coinId, currencies));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ProcessPreferredCoinPrice()
        {
            string authHeader = HttpContext.Request.Headers["Authorization"];
            if(hashKey == authHeader)
            {
                return Ok(_coinService.ProcessPreferredCoinPrice());
            }
            else
            {
                return Unauthorized();
            }
          
        }
    }
}
