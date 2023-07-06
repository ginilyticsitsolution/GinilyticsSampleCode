using CryptoTime.DataAccess.Repositories.Contracts;
using CryptoTime.Models.DataModels.User;
using CryptoTime.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CryptoTime.DataAccess.Repositories
{
    public class CoinRepository : GenericRepository<Coin>, ICoinRepository
    {
        private readonly CryptoTimeContext _context;
        public CoinRepository(CryptoTimeContext context) : base(context)
        {
            _context = context;
        }

        public List<CoinModel> GetCoins(int userId)
        {
            return (from coin in _context.Coins
                    from coinPref in _context.UserCoinPreferences.Where(x => x.userId == userId && coin.id == x.coinId && x.active == true)
                        .DefaultIfEmpty()
                    where (coin.active == true)
                    select new CoinModel
                    {
                        id = coin.id,
                        coinId = coin.coinId,
                        coinName = coin.coinName,
                        symbol = coin.symbol,
                        active = coin.active,
                        dateAdded = coin.dateAdded,
                        imgLarge = coin.urlLargeImg,
                        imgSmall = coin.urlSmallImg,
                        imgThumb = coin.urlThumbImg,
                        is_prefered_coin = (coinPref.dateAdded == null) ? false : true,
                    }).Take(100).ToList().OrderByDescending(x => x.is_prefered_coin).ToList();
        }

        public List<CoinPreferenceModel> GetPreferredCoins()
        {
            var result = from c in _context.Coins
                         join cp in _context.UserCoinPreferences on c.id equals cp.coinId
                         select new CoinPreferenceModel
                         {
                             id = c.id,
                             coinId = c.coinId,

                         };
            return result.ToList();
        }

        public void UpdateLatestPrice(CoinPrice coinPrice)
        {
            coinPrice.id = 1;
            _context.CoinPrices.Update(coinPrice);
            _context.SaveChanges();
        }
        public string GetPreferredCoinPrice()
        {
            return _context.CoinPrices.Select(x => x.coinPrice).FirstOrDefault();
        }

    }
}
