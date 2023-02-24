using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Algorithm.Framework.SignalExports;

/*
*************************************************
TODO:
Create a portfolio & pass it through to SetTargetPortfolio 
- Issue: onData event never gets called, so SetHoldings doesn't work


*************************************************
*/

namespace QuantConnect.Algorithm.CSharp
{

    public class SignalExportTesting : QCAlgorithm
    {


        Dictionary<string, decimal> targetPortfolio = new Dictionary<string, decimal>() { 
            { "SPY", (decimal)(0.2) }, 
            { "TSLA", (decimal)(0.3) },         
            { "GOOG", (decimal)(0.5) } };

        List<PortfolioTarget> targetList = new List<PortfolioTarget>();
        QCAlgorithm _algorithm;


        public override void Initialize()
        {
            _algorithm = this;
            UniverseSettings.Resolution = Resolution.Minute;

            SetStartDate(2022, 10, 07);  //Set Start Date
            SetEndDate(2022, 10, 20);    //Set End Date
            SetCash(50000);             //Set Strategy Cash


            foreach (var target in targetPortfolio)
            {
                Debug("Adding Equity: " + target.Key);

                var item = AddEquity(target.Key);
                targetList.Add(new PortfolioTarget(item.Symbol, target.Value));
            }
        }


        public override void OnWarmupFinished()
        {
            Debug("EVENT: Warmup Finished **************************************************");
            SignalExportsManager manager = new SignalExportsManager(_algorithm);
            manager.Add(new CrunchDaoSignalExport("HKusSqaTBt3iokuCmimP8KqqppFlmRDgm7oxUoKOXkJlyvmXOD60uynQAVTt","qcTest"));
            manager.Add(new Collective2SignalExport("fnmzppYk0HO8YTrMRCPA2MBa3mLna6frsMjAJab1SyA5lpfbhY", 143679411));


            manager.SetTargetPortfolioPercentages(targetList.ToArray());
            manager.ExportSignals();

        }


        public override void OnData(Slice slice)
        {
            Debug("EVENT: OnData **************************************************");

        }



        public override void OnEndOfAlgorithm()
        {
            Debug("EVENT: End of Algorithm  **************************************************");

        }



    }
}
