/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Python.Runtime;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;

namespace QuantConnect.Lean.Engine.DataFeeds.Enumerators
{
    /// <summary>
    ///
    /// </summary>
    public class PyCoarseFundamentalEnumerator
    {
        private const int _batchSize = 20;
        private readonly Dictionary<DateTime, PyObject> _timeCache = new Dictionary<DateTime, PyObject>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="baseDataEnumerable"></param>
        /// <returns></returns>
        public IEnumerable<BaseData> SetPyCoarseFundamental(IEnumerable<BaseData> baseDataEnumerable)
        {
            var coarseFundamentals = new CoarseFundamental[_batchSize];
            var count = 0;
            foreach (var baseData in baseDataEnumerable)
            {
                var coarseFundamental = baseData as CoarseFundamental;
                if (coarseFundamental != null)
                {
                    coarseFundamentals[count++] = coarseFundamental;

                    // due to performance cost of acquiring and releasing the GIL lock
                    // batch the PyCoarseFundamental initialization.
                    // Note: can not convert the entire coarse Fundamentals data together either
                    // because it will starve other threads waiting on the GIL lock
                    if (count % _batchSize == 0)
                    {
                        count = 0;
                        InitializePyCoarseFundamental(coarseFundamentals);
                        foreach (var data in coarseFundamentals)
                        {
                            yield return data;
                        }
                    }
                }
            }
            // check if there are any left
            if (count > 0)
            {
                var coarseLeft = coarseFundamentals.Take(count).ToArray();
                InitializePyCoarseFundamental(coarseLeft);
                foreach (var coarseFundamental in coarseLeft)
                {
                    yield return coarseFundamental;
                }
            }

            var state = PythonEngine.AcquireLock();
            foreach (var pyObject in _timeCache)
            {
                pyObject.Value.UnsafeDispose();
            }
            PythonEngine.ReleaseLock(state);
            _timeCache.Clear();
        }

        private void InitializePyCoarseFundamental(IEnumerable<CoarseFundamental> coarseFundamentals)
        {
            var state = PythonEngine.AcquireLock();
            foreach (var coarseFundamental in coarseFundamentals)
            {
                PyObject timePyObject;
                if (!_timeCache.TryGetValue(coarseFundamental.Time, out timePyObject))
                {
                    timePyObject = coarseFundamental.Time.ToPython();
                    _timeCache.Add(coarseFundamental.Time, timePyObject);
                }

                coarseFundamental.PyCoarseFundamental =
                    CoarseFundamental.PyCoarseFundamentalConstructor(
                        coarseFundamental.Price,
                        coarseFundamental.DollarVolume,
                        coarseFundamental.Volume,
                        coarseFundamental.HasFundamentalData,
                        coarseFundamental.AdjustedPrice,
                        timePyObject,
                        coarseFundamental.Symbol);
            }
            PythonEngine.ReleaseLock(state);
        }
    }
}
