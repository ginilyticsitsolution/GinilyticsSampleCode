using AutoMapper;
using CryptoTime.Common.Utilities;
using CryptoTime.DataAccess.Repositories.Contracts;
using CryptoTime.Models.DataModels.User;
using CryptoTime.Models.ViewModels;
using CryptoTime.Service.Services.Contracts;
using CryptoTime.Models.ViewModels.CoingeckoModels;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;

namespace CryptoTime.Service.Services
{
    public class CoinService : ICoinService
    {
        public ICoinRepository _coinRepo;
        private ServiceResult _serviceResult;
        private readonly IEncryptionManager _encryptionManager;
        private readonly IMapper _mapper;
        public CoinService(ICoinRepository repo, IEncryptionManager encryptionManager, IMapper mapper)
        {
            _coinRepo = repo;
            _serviceResult = new ServiceResult { Status = true, Message = Global.Success, StatusCode = Convert.ToInt32(HttpStatusCode.OK) };
            _encryptionManager = encryptionManager;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all Coins list
        /// </summary>
        /// <returns></returns>
        public ServiceResult GetAllCoins(int userId)
        {
            try
            {
                _serviceResult.ResultData = _coinRepo.GetCoins(userId);
                return _serviceResult;
            }
            catch (Exception ex)
            {
                _serviceResult.Message = ex.Message;
                _serviceResult.ResultData = Global.Failed;
                _serviceResult.Status = false;
                _serviceResult.StatusCode = Convert.ToInt32(HttpStatusCode.ExpectationFailed);
                return _serviceResult;
            }
        }

        public ServiceResult AddCoin()
        {
            try
            {
                var client = new RestClient(UrlHelper.coinList);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var coin = JsonConvert.DeserializeObject<List<CoinList>>(response.Content);

                    for (int i = 0; i < coin.Count; i++)
                    {
                        var id = _coinRepo.FindBy(x => x.coinId == coin[i].id && x.active == true).FirstOrDefault();

                        if (id == null)
                        {
                            var coinDetails = GetCoinById(coin[i].id);
                            if (coinDetails != null)
                            {
                                Coin obj = new Coin();
                                obj.coinId = coin[i].id;
                                obj.coinName = coin[i].name;
                                obj.symbol = coinDetails.symbol;
                                obj.urlThumbImg = coinDetails.image.thumb;
                                obj.urlSmallImg = coinDetails.image.small;
                                obj.urlLargeImg = coinDetails.image.large;
                                obj.dateAdded = DateTime.Now.ToUniversalTime();
                                obj.active = true;
                                _coinRepo.Add(obj);
                                _coinRepo.Commit();
                            }

                        }
                    }
                }
                _serviceResult.Message = Global.Success;
                _serviceResult.Status = true;
                _serviceResult.StatusCode = Convert.ToInt32(HttpStatusCode.OK);
                return _serviceResult;
            }
            catch (Exception ex)
            {
                _serviceResult.Message = ex.Message;
                _serviceResult.ResultData = Global.Failed;
                _serviceResult.Status = false;
                _serviceResult.StatusCode = Convert.ToInt32(HttpStatusCode.ExpectationFailed);
                return _serviceResult;
            }
        }

        public CoinList GetCoinById(string coinId)
        {
            string body = "/" + coinId;
            var client = new RestClient(UrlHelper.coinDetailsById + body);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<CoinList>(response.Content);
            }
            else
                return null;
        }

        public ServiceResult GetPrice(string coinId, string currencies)
        {
            try
            {
                string queryString = "?ids=" + coinId + "&vs_currencies=" + currencies;
                var client = new RestClient(UrlHelper.price + queryString);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _serviceResult.ResultData = response.Content;
                    _serviceResult.Message = Global.Success;
                    _serviceResult.Status = true;
                    _serviceResult.StatusCode = Convert.ToInt32(HttpStatusCode.OK);
                }
                return _serviceResult;
            }
            catch (Exception ex)
            {
                _serviceResult.Message = ex.Message;
                _serviceResult.ResultData = Global.Failed;
                _serviceResult.Status = false;
                _serviceResult.StatusCode = Convert.ToInt32(HttpStatusCode.ExpectationFailed);
                return _serviceResult;
            }
        }

        public string ProcessPreferredCoinPrice()
        {
            try
            {
                var responsePrice = new CoinPrice();
                string ids = GetPreferredCoin();
                var currencies = "usd";                     //this needs to be dynamic
                string queryString = "?ids=" + ids + "&vs_currencies=" + currencies;
                var client = new RestClient(UrlHelper.price + queryString);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    responsePrice.coinPrice = response.Content;
                    UpdateLatestPrice(responsePrice);
                }
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public String GetPreferredCoin()
        {
            try
            {
                var coins = _coinRepo.GetPreferredCoins();
                var coinIds = coins.Select(x => x.coinId).Distinct().ToList();
                String commaSeparatedIds = String.Join(",", coinIds);
                return commaSeparatedIds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UpdateLatestPrice(CoinPrice coinPrice)
        {
            _coinRepo.UpdateLatestPrice(coinPrice);
            
        }

        public ServiceResult GetPreferredCoinPrice()
        {
            try
            {
                _serviceResult.ResultData = _coinRepo.GetPreferredCoinPrice();
                return _serviceResult;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
