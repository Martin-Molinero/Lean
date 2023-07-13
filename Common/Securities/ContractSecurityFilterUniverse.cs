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
 *
*/

using System;
using System.Linq;
using Python.Runtime;
using System.Collections;
using System.Collections.Generic;

namespace QuantConnect.Securities
{
    /// <summary>
    /// Base class for contract symbols filtering universes.
    /// Used by OptionFilterUniverse and FutureFilterUniverse
    /// </summary>
    public abstract class ContractSecurityFilterUniverse<T> : IEnumerable<Symbol>
        where T: ContractSecurityFilterUniverse<T>
    {
        private DateTime _cacheDate;

        /// <summary>
        /// Mark this filter to be applied only on market open, even if it's dynamic
        /// </summary>
        private Func<DateTime, bool> _shouldSelectSymbols;

        /// <summary>
        /// Defines listed contract types with Flags attribute
        /// </summary>
        [Flags]
        protected enum ContractExpirationType : int
        {
            /// <summary>
            /// Standard contracts
            /// </summary>
            Standard = 1,

            /// <summary>
            /// Non standard weekly contracts
            /// </summary>
            Weekly = 2
        }

        /// <summary>
        /// Expiration Types allowed through the filter
        /// Standards only by default
        /// </summary>
        protected ContractExpirationType Type = ContractExpirationType.Standard;

        /// <summary>
        /// The local exchange current time
        /// </summary>
        public DateTime LocalTime { get; private set; }

        /// <summary>
        /// All Symbols in this filter
        /// Marked internal for use by extensions
        /// </summary>
        internal IEnumerable<Symbol> AllSymbols;

        /// <summary>
        /// Mark this filter dynamic for regular reapplying
        /// Marked internal for use by extensions
        /// </summary>
        internal bool IsDynamicInternal;

        /// <summary>
        /// Constructs ContractSecurityFilterUniverse
        /// </summary>
        protected ContractSecurityFilterUniverse()
        {
            _shouldSelectSymbols = (_) => {
                if (_cacheDate == default)
                {
                    // initial selection
                    return true;
                }

                if (!IsDynamicInternal)
                {
                    // if we are not dynamic, let's select once per day
                    return _cacheDate != LocalTime.Date;
                }
                // we are a dynamic filter
                return true;
            };
        }

        /// <summary>
        /// Constructs ContractSecurityFilterUniverse
        /// </summary>
        protected ContractSecurityFilterUniverse(IEnumerable<Symbol> allSymbols, DateTime localTime) : this()
        {
            Refresh(allSymbols, localTime);
        }

        /// <summary>
        /// Function to determine if the given symbol is a standard contract
        /// </summary>
        /// <returns>True if standard type</returns>
        protected abstract bool IsStandard(Symbol symbol);

        /// <summary>
        /// Returns universe, filtered by contract type
        /// </summary>
        /// <returns>Universe with filter applied</returns>
        internal T ApplyTypesFilter()
        {
            // memoization map for ApplyTypesFilter()
            var memoizedMap = new Dictionary<DateTime, bool>();

            Func<Symbol, bool> memoizedIsStandardType = symbol =>
            {
                var dt = symbol.ID.Date;

                bool result;
                if (memoizedMap.TryGetValue(dt, out result))
                    return result;
                var res = IsStandard(symbol);
                memoizedMap[dt] = res;

                return res;
            };

            AllSymbols = AllSymbols.Where(x =>
            {
                switch (Type)
                {
                    case ContractExpirationType.Weekly:
                        return !memoizedIsStandardType(x);
                    case ContractExpirationType.Standard:
                        return memoizedIsStandardType(x);
                    case ContractExpirationType.Standard | ContractExpirationType.Weekly:
                        return true;
                    default:
                        return false;
                }
            }).ToList();

            return (T) this;
        }

        /// <summary>
        /// Returns true if selection should happen
        /// </summary>
        internal bool ShouldSelectSymbols()
        {
            var result = _shouldSelectSymbols(LocalTime);
            _cacheDate = LocalTime.Date;
            if(result)
            {
                // refresh this before selection
                IsDynamicInternal = false;
            }
            return result;
        }

        /// <summary>
        /// Refreshes this filter universe
        /// </summary>
        /// <param name="allSymbols">All the contract symbols for the Universe</param>
        /// <param name="localTime">The local exchange current time</param>
        public void Refresh(IEnumerable<Symbol> allSymbols, DateTime localTime)
        {
            AllSymbols = allSymbols;
            LocalTime = localTime;
            Type = ContractExpirationType.Standard;
        }

        /// <summary>
        /// Sets universe of standard contracts (if any) as selection
        /// Contracts by default are standards; only needed to switch back if changed
        /// </summary>
        /// <returns>Universe with filter applied</returns>
        public T StandardsOnly()
        {
            Type = ContractExpirationType.Standard;
            return (T)this;
        }

        /// <summary>
        /// Includes universe of non-standard weeklys contracts (if any) into selection
        /// </summary>
        /// <returns>Universe with filter applied</returns>
        public T IncludeWeeklys()
        {
            Type |= ContractExpirationType.Weekly;
            return (T)this;
        }

        /// <summary>
        /// Sets universe of weeklys contracts (if any) as selection
        /// </summary>
        /// <returns>Universe with filter applied</returns>
        public T WeeklysOnly()
        {
            Type = ContractExpirationType.Weekly;
            return (T)this;
        }

        /// <summary>
        /// Returns front month contract
        /// </summary>
        /// <returns>Universe with filter applied</returns>
        public virtual T FrontMonth()
        {
            var ordered = this.OrderBy(x => x.ID.Date).ToList();
            if (ordered.Count == 0) return (T) this;
            var frontMonth = ordered.TakeWhile(x => ordered[0].ID.Date == x.ID.Date);

            AllSymbols = frontMonth.ToList();
            return (T) this;
        }

        /// <summary>
        /// Returns a list of back month contracts
        /// </summary>
        /// <returns>Universe with filter applied</returns>
        public virtual T BackMonths()
        {
            var ordered = this.OrderBy(x => x.ID.Date).ToList();
            if (ordered.Count == 0) return (T) this;
            var backMonths = ordered.SkipWhile(x => ordered[0].ID.Date == x.ID.Date);

            AllSymbols = backMonths.ToList();
            return (T) this;
        }

        /// <summary>
        /// Returns first of back month contracts
        /// </summary>
        /// <returns>Universe with filter applied</returns>
        public T BackMonth()
        {
            return BackMonths().FrontMonth();
        }

        /// <summary>
        /// Applies filter selecting options contracts based on a range of expiration dates relative to the current day
        /// </summary>
        /// <param name="minExpiry">The minimum time until expiry to include, for example, TimeSpan.FromDays(10)
        /// would exclude contracts expiring in more than 10 days</param>
        /// <param name="maxExpiry">The maximum time until expiry to include, for example, TimeSpan.FromDays(10)
        /// would exclude contracts expiring in less than 10 days</param>
        /// <returns>Universe with filter applied</returns>
        public virtual T Expiration(TimeSpan minExpiry, TimeSpan maxExpiry)
        {
            if (LocalTime == default)
            {
                return (T) this;
            }

            if (maxExpiry > Time.MaxTimeSpan) maxExpiry = Time.MaxTimeSpan;

            var minExpiryToDate = LocalTime.Date + minExpiry;
            var maxExpiryToDate = LocalTime.Date + maxExpiry;

            AllSymbols = AllSymbols
                .Where(symbol => symbol.ID.Date.Date >= minExpiryToDate && symbol.ID.Date.Date <= maxExpiryToDate)
                .ToList();

            return (T) this;
        }

        /// <summary>
        /// Applies filter selecting contracts based on a range of expiration dates relative to the current day
        /// </summary>
        /// <param name="minExpiryDays">The minimum time, expressed in days, until expiry to include, for example, 10
        /// would exclude contracts expiring in more than 10 days</param>
        /// <param name="maxExpiryDays">The maximum time, expressed in days, until expiry to include, for example, 10
        /// would exclude contracts expiring in less than 10 days</param>
        /// <returns>Universe with filter applied</returns>
        public T Expiration(int minExpiryDays, int maxExpiryDays)
        {
            return Expiration(TimeSpan.FromDays(minExpiryDays), TimeSpan.FromDays(maxExpiryDays));
        }

        /// <summary>
        /// Explicitly sets the selected contract symbols for this universe.
        /// This overrides and and all other methods of selecting symbols assuming it is called last.
        /// </summary>
        /// <param name="contracts">The option contract symbol objects to select</param>
        /// <returns>Universe with filter applied</returns>
        public T Contracts(PyObject contracts)
        {
            AllSymbols = contracts.ConvertToSymbolEnumerable();
            return (T) this;
        }

        /// <summary>
        /// Explicitly sets the selected contract symbols for this universe.
        /// This overrides and and all other methods of selecting symbols assuming it is called last.
        /// </summary>
        /// <param name="contracts">The option contract symbol objects to select</param>
        /// <returns>Universe with filter applied</returns>
        public T Contracts(IEnumerable<Symbol> contracts)
        {
            AllSymbols = contracts.ToList();
            return (T) this;
        }

        /// <summary>
        /// Sets a function used to filter the set of available contract filters. The input to the 'contractSelector'
        /// function will be the already filtered list if any other filters have already been applied.
        /// </summary>
        /// <param name="contractSelector">The option contract symbol objects to select</param>
        /// <returns>Universe with filter applied</returns>
        public T Contracts(Func<IEnumerable<Symbol>, IEnumerable<Symbol>> contractSelector)
        {
            // force materialization using ToList
            AllSymbols = contractSelector(AllSymbols).ToList();
            return (T) this;
        }

        /// <summary>
        /// Instructs the engine to only filter contracts on the first time step of each market day.
        /// </summary>
        /// <returns>Universe with filter applied</returns>
        public T OnlyApplyFilterAtMarketOpen()
        {
            Schedule(localTime => _cacheDate != localTime.Date );
            return (T) this;
        }

        /// <summary>
        /// Instructs the engine to only trigger selection based on the given functions result
        /// </summary>
        /// <param name="shouldSelect">A function which returns true if selection should happen for the given current local exchange time</param>
        /// <returns>Universe with filter applied</returns>
        public T Schedule(Func<DateTime, bool> shouldSelect)
        {
            _shouldSelectSymbols = shouldSelect;
            return (T) this;
        }

        /// <summary>
        /// Instructs the engine to only trigger selection based on the given functions result
        /// </summary>
        /// <param name="shouldSelect">A function which returns true if selection should happen for the given current local exchange time</param>
        /// <returns>Universe with filter applied</returns>
        public T Schedule(PyObject shouldSelect)
        {
            return Schedule(shouldSelect.ConvertToDelegate<Func<DateTime, bool>>());
        }

        /// <summary>
        /// IEnumerable interface method implementation
        /// </summary>
        /// <returns>IEnumerator of Symbols in Universe</returns>
        public IEnumerator<Symbol> GetEnumerator()
        {
            return AllSymbols.GetEnumerator();
        }

        /// <summary>
        /// IEnumerable interface method implementation
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
