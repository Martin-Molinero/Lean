using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Securities;
using QuantConnect.Algorithm.Framework.Portfolio;


namespace QuantConnect.Interfaces
{


    public interface ISignalExport
    {

        /// <summary>
        /// Determines whether the SignalExport requires individual holdings to be submitted as a percentage of the total portfolio value
        /// </summary>
        bool RequiresQuantityAsPercentage();

        void Send(List<PortfolioTarget> holdings);
    }

}
