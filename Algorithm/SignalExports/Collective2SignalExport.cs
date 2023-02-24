using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using QuantConnect.Interfaces;
using QuantConnect.Algorithm.Framework.Portfolio;

/*
*************************************************
TODO:
Verify the upload works. HTTP response shows message accepted, but it does not actually populate the strategy with positions when logged onto C2

_quantityAsPercentage = false (they accept # of SHARES, not % of portfolio)

-Confirm what uses SecurityType.Index (SPY and SPX both dont) and if it should be considered Equity
-Same for SecurityType.IndexOption considered as Option

Comments & Documentation
*************************************************
*/



//API Reference: https://collective2.com/api-docs/latest#setDesiredPositions

namespace QuantConnect.Algorithm.Framework.SignalExports
{
    public class Collective2SignalExport : ISignalExport
    {

        private readonly string _apiKey;

        private readonly int _systemId;

        private readonly string _platformId;

        private readonly string _destination;

        private const bool _requiresQuantityAsPercentage = true; //this needs to be set to FALSE when we go live


        public Collective2SignalExport(string ApiKey, int SystemId, string PlatformID = null)
        {
            _apiKey = ApiKey;
            _systemId = SystemId;
            _platformId = PlatformID;

            if (PlatformID == null)
            {
                _destination = "https://api.collective2.com/world/apiv3/setDesiredPositions";
            }
            else
            {
                _destination = "https://api.collective2.com/platform/apiv3/" + PlatformID + "/setDesiredPositions";
            }


        }

        public bool RequiresQuantityAsPercentage()
        {
            return _requiresQuantityAsPercentage;
        }


        public void Send(List<PortfolioTarget> holdings)
        {
            if (holdings.Count == 0) throw new ArgumentException("PortfolioTarget list is empty");    

            List<C2Position> positions = ConvertHoldings(holdings);
            string message = CreateMessage(positions);
            SendPositions(message);
        }

        /// <summary>
        /// Converts the list of positions into a format readable by Collective2
        /// </summary>
        private List<C2Position> ConvertHoldings(List<PortfolioTarget> holdings)
        {
            List<C2Position> positions = new List<C2Position>();

            foreach (PortfolioTarget target in holdings)
            {
                C2Position position = new C2Position { symbol = target.Symbol.Value, typeofsymbol = ConvertSecurityType(target.Symbol), quant = target.Quantity };
                positions.Add(position);
            }

            return positions;

        }
        
        /// <summary>
        /// Creates the JSON message to send to Collective2
        /// </summary>
        private string CreateMessage(List<C2Position> positions)
        {
            var payload = new
            {
                apikey = _apiKey,
                systemid = _systemId,
                positions = positions,
            };

            string jsonMessage = JsonConvert.SerializeObject(payload);

            Console.WriteLine("C2 JSON Message *****************");
            Console.WriteLine(jsonMessage);

            return jsonMessage;
        }

        /// <summary>
        /// The position format used by Collective2
        /// </summary>
        private class C2Position
        {
            public string symbol;
            public string typeofsymbol;
            public decimal quant; //represents number of shares (not % of portfolio)
        }


        /// <summary>
        /// Converts the Symbol.SecurityType to the TypeOfSymbol format used by Collective2
        /// </summary>
        private string ConvertSecurityType(Symbol symbol)
        {
            switch (symbol.SecurityType)
            {
                case SecurityType.Equity:
                    return "stock";

                case SecurityType.Option:
                    return "option";

                case SecurityType.Future:
                    return "future";

                case SecurityType.Forex:
                    return "forex";

                case SecurityType.Index:
                    return "stock";

                case SecurityType.IndexOption:
                    return "option";

                default:
                    throw new NotImplementedException(symbol.SecurityType + " security type has not been implemented by Collective2 yet.");
            }
        }


        async private void SendPositions(string jsonMessage)
        {

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    _destination,
                     new StringContent(jsonMessage, Encoding.UTF8, "application/json"));

                Console.WriteLine(response.ToString());


            }
        }

    }

}