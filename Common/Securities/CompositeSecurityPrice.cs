using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Data;
using QuantConnect.Interfaces;

namespace QuantConnect.Securities
{
    public class CompositeSecurityPrice : ISecurityPrice
    {
        private List<ISecurityPrice> _securityPrices;

        public CompositeSecurityPrice(List<ISecurityPrice> securityPrices)
        {
            _securityPrices = securityPrices;
        }

        public decimal Price => _securityPrices[0].Price;
        public decimal Close => _securityPrices[0].Close;
        public decimal Volume => _securityPrices[0].Volume;
        public decimal BidPrice => _securityPrices[0].BidPrice;
        public decimal BidSize => _securityPrices[0].BidSize;
        public decimal AskPrice => _securityPrices[0].AskPrice;
        public decimal AskSize => _securityPrices[0].AskSize;
        public long OpenInterest => _securityPrices[0].OpenInterest;
        public Symbol Symbol => _securityPrices[0].Symbol;
        public void SetMarketPrice(BaseData data)
        {
            for (var i = 0; i < _securityPrices.Count; i++)
            {
                _securityPrices[i].SetMarketPrice(data);
            }
        }

        public void Update(IReadOnlyList<BaseData> data, Type dataType, bool? containsFillForwardData)
        {
            for (var i = 0; i < _securityPrices.Count; i++)
            {
                _securityPrices[i].Update(data, dataType, containsFillForwardData);
            }
        }

        public BaseData GetLastData()
        {
            return _securityPrices[0].GetLastData();
        }
    }
}
