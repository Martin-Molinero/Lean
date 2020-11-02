using System.Linq;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Logging;
using QuantConnect.Securities;

namespace QuantConnect.Lean.Engine.DataFeeds.Enumerators
{
    public class FutureMappingEventProvider
    {
        private CachingFutureChainProvider _futureChainProvider = new CachingFutureChainProvider(new BacktestingFutureChainProvider());
        private SubscriptionDataConfig _config;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public FutureMappingEventProvider(SubscriptionDataConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public void HandleNewTradableDate(object sender, NewTradableDateEventArgs eventArgs)
        {
            var chain = _futureChainProvider.GetFutureContractList(_config.Symbol, eventArgs.Date);
            // 'FutureFilterUniverse' could be provided by the user
            // volume roll style? history?
            var currentSymbol = new FutureFilterUniverse(chain, new Tick(eventArgs.Date, _config.Symbol, 0, 0))
                .Expiration(5, 100).FrontMonth().SingleOrDefault();

            if (currentSymbol == null || _config.Symbol.ID == currentSymbol.ID)
            {
                return;
            }

            if (!_config.Symbol.IsCanonical())
            {
                Log.Trace($"Updating continuous future mapping from {_config.Symbol.ID} top {currentSymbol.ID}");
            }

            _config.MappedSymbol = currentSymbol.ID.ToString();
        }
    }
}
