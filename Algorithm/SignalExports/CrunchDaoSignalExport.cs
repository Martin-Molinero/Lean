using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using QuantConnect.Interfaces;
using QuantConnect.Algorithm.Framework.Portfolio;

/*
*************************************************
TODO:
-Comments & Documentation

-Verify the upload works

-Confirm what uses SecurityType.Index (SPY and SPX both dont) and if it should be considered Equity

-How to upload CSV without saving as a file

*************************************************
*/



//Documentation: https://colab.research.google.com/drive/1YW1xtHrIZ8ZHW69JvNANWowmxPcnkNu0?authuser=0&pli=1#scrollTo=n_5hg9-zfPNN
//API Reference: https://api.tournament.crunchdao.com/swagger-ui/index.html#/alpha/create_1

namespace QuantConnect.Algorithm.Framework.SignalExports
{

    public class CrunchDaoSignalExport : ISignalExport
    {

        private readonly string _apiKey;
        private readonly string _modelName;
        private readonly string _submissionName;
        private readonly string _comment;

        private const bool _requiresQuantityAsPercentage = true;

        private const string _destination = "https://api.tournament.crunchdao.com/v3/alpha-submissions";


        public bool RequiresQuantityAsPercentage()
        {
            return _requiresQuantityAsPercentage;
        }

        public CrunchDaoSignalExport(string apiKey, string modelName, string submissionName = null, string comment = null)
        {
            _apiKey = apiKey;
            _modelName = modelName;
            _submissionName = submissionName;
            _comment = comment;
        }


        public void Send(List<PortfolioTarget> holdings)
        {
            VerifySignals(holdings);
            CreateCSV(holdings);
            SendPositions();
            string blah = Console.ReadLine();
        }


        /// <summary>
        /// Verifies that all signals are US Equities and throws an exception if otherwise
        /// </summary>
        private void VerifySignals(List<PortfolioTarget> holdings)
        {
            foreach (var signal in holdings)
            {
                if (!(signal.Symbol.SecurityType == SecurityType.Equity || signal.Symbol.SecurityType == SecurityType.Index))
                {
                    throw new NotImplementedException(signal.Symbol.SecurityType + " security type is not implemented: CrunchDao only accepts signals for US Equities");
                }
            }
        }

        private string filePath = "./signals.csv";
        public void CreateCSV(List<PortfolioTarget> holdings)
        {
            //output format:
            //ticker,weight
            //SPY,0.2
            //TSLA,0.3
            //GOOG,0.5
            Console.WriteLine("Creating CSV *****************");
            List<string> blah = new List<string>();

            var csv = new StringBuilder();

            // var firstLine = "ticker,weight";
            // csv.AppendLine(firstLine);

            foreach (var target in holdings)
            {
                string first = target.Symbol.Value;
                string second = target.Quantity.ToString();
                var newLine = $"{first},{second}";
                csv.AppendLine(newLine);
            }
            File.WriteAllText(filePath, csv.ToString());
            Console.WriteLine("CSV Created *****************");

        }

        /*

                StatusCode: 400, ReasonPhrase: '', Version: 1.1, Content: System.Net.Http.HttpConnectionResponseContent, Headers:
                {
                Server: nginx/1.14.0 (Ubuntu)
                Date: Fri, 24 Feb 2023 20:58:48 GMT
                Transfer-Encoding: chunked
                Connection: keep-alive
                Access-Control-Allow-Origin: *
                Access-Control-Allow-Methods: POST, GET, OPTIONS, PUT, DELETE
                Access-Control-Max-Age: 3600
                Access-Control-Allow-Headers: Content-Type, x-requested-with, authorization
                Vary: Origin
                Vary: Access-Control-Request-Method
                Vary: Access-Control-Request-Headers
                X-Content-Type-Options: nosniff
                X-XSS-Protection: 1; mode=block
                Cache-Control: no-cache, no-store, max-age=0, must-revalidate
                Pragma: no-cache
                Content-Type: application/json
                Expires: 0
                }

        */

        /// <summary>
        /// Creates the JSON message to send to Collective2
        /// </summary>
        private string CreateMessage()
        {
            var payload = new
            {
                apiKey = _apiKey,
                model = _modelName,
                label = _submissionName,
                comment = _comment
            };

            string jsonMessage = JsonConvert.SerializeObject(payload);

            Console.WriteLine("CrunchDao JSON Message *****************");
            Console.WriteLine(jsonMessage);

            return jsonMessage;

        }


        async private void SendPositions()
        {

            await using var stream = File.OpenRead("./signals.csv");
            var payload = new
            {
                apikey = _apiKey,
                model = _modelName,
                label = _submissionName,
                comment = _comment
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "file");
            using var content = new MultipartFormDataContent
            {
                {new StreamContent(stream)},
                {new StringContent(CreateMessage(), Encoding.UTF8, "application/json")}
            };
            request.Content = content;

            Console.WriteLine("Ready to send *****************");
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(_destination,content);
                Console.WriteLine(response.ToString());
            }
            Console.WriteLine("Sent *****************");

        }
    }
}