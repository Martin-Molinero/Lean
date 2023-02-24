using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Securities;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Data.Custom.AlphaStreams;
using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Portfolio;

/*
*************************************************
TODO:
-Comments & Documentation

-validate the commented section of SetTargetPortfolioPercentages

*************************************************
*/

namespace QuantConnect.Algorithm.Framework.SignalExports
{

    public class SignalExportsManager
    {
        private List<ISignalExport> _signalExports = new List<ISignalExport>();

        private List<PortfolioTarget> _portfolioTargetPercents = new List<PortfolioTarget>();
        private List<PortfolioTarget> _portfolioTargetQuantity = new List<PortfolioTarget>();
        private QCAlgorithm _algorithm;


        public SignalExportsManager(QCAlgorithm algorithm, params ISignalExport[] signalExports)
        {
            _algorithm = algorithm;
            _signalExports.AddRange(signalExports);
        }


        public void Add(params ISignalExport[] signalExports)
        {
            _signalExports.AddRange(signalExports);
        }


        /// <param name="portfolio">The portfolio *self.Portfolio</param>
        public void SetTargetPortfolio(SecurityPortfolioManager portfolio)
        {

            foreach (SecurityHolding holding in portfolio.Values)
            {
                decimal percent = holding.HoldingsValue / portfolio.TotalPortfolioValue;

                _portfolioTargetPercents.Add(new PortfolioTarget(holding.Symbol, percent));
                _portfolioTargetQuantity.Add(new PortfolioTarget(holding.Symbol, holding.Quantity));
            }
        }

        /// <param name="targets">The portfolio targets, where quantity is a percent of total portfolio</param>
        public void SetTargetPortfolioPercentages( params PortfolioTarget[] targets)
        {
            _portfolioTargetPercents.AddRange(targets);

            // foreach(PortfolioTarget target in targets)
            //     {
            //         IPortfolioTarget targetQty = PortfolioTarget.Percent(_algorithm,target.Symbol,target.Quantity);

            //         _portfolioTargetQuantity.Add(new PortfolioTarget(targetQty.Symbol,targetQty.Quantity));
            //     }
        }

        public void ExportSignals()
        {
            foreach (ISignalExport exportTarget in _signalExports)
            {
                if (exportTarget.RequiresQuantityAsPercentage())
                {
                    exportTarget.Send(_portfolioTargetPercents);
                }
                else
                {
                    exportTarget.Send(_portfolioTargetQuantity);
                }
            }
        }



    }

}

