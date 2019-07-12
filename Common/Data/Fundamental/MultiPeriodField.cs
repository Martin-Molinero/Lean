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
using Newtonsoft.Json;

namespace QuantConnect.Data.Fundamental
{
    public struct PeriodField
    {
        public PeriodField(byte period, float value)
        {
            Period = period;
            Value = value;
        }

        public byte Period { get; set; }
        public float Value { get; set; }
    }

    /// <summary>
    /// Abstract base class for multi-period fields
    /// </summary>
    public abstract class MultiPeriodField
    {
        /// <summary>
        /// The dictionary store containing all values for the multi-period field
        /// </summary>
        protected PeriodField[] Store;

        /// <summary>
        /// Gets the default period for the field
        /// </summary>
        protected virtual byte DefaultPeriod
        {
            get
            {
                if (Store == null)
                {
                    return 0;
                }
                var periodField = Store.FirstOrDefault();
                return periodField.Period;
            }
        }

        /// <summary>
        /// Gets the value of the field for the requested period
        /// </summary>
        /// <param name="period">The requested period</param>
        /// <returns>The value for the period</returns>
        public decimal GetPeriodValue(byte period)
        {
            if (Store == null)
            {
                return 0;
            }
            return (decimal)Store.FirstOrDefault(field => field.Period == period).Value;
        }

        /// <summary>
        /// Returns true if the field contains a value for the requested period
        /// </summary>
        /// <param name="period">The requested period</param>
        public bool HasPeriodValue(byte period) => Store != null && Store.Any(field => field.Period == period);

        /// <summary>
        /// Returns true if the field contains a value for the default period
        /// </summary>
        public bool HasValue => DefaultPeriod != 0;

        /// <summary>
        /// Gets the list of available period names for the field
        /// </summary>
        /// <returns>The list of periods</returns>
        public IEnumerable<byte> GetPeriodNames()
        {
            if (Store == null)
            {
                return Enumerable.Empty<byte>();
            }
            return Store.Select(field => field.Period);
        }

        /// <summary>
        /// Gets a dictionary of period names and values for the field
        /// </summary>
        /// <returns>The dictionary of period names and values</returns>
        public IReadOnlyList<PeriodField> GetPeriodValues()
        {
            if (Store == null)
            {
                return new List<PeriodField>();
            }
            return Store;
        }

        /// <summary>
        /// Returns the default value for the field
        /// </summary>
        [JsonIgnore]
        public float Value
        {
            get
            {
                if (Store == null || Store.Length == 0) return 0;

                return Store.First().Value;
            }
        }

        /// <summary>
        /// Returns the default value for the field
        /// </summary>
        /// <param name="field"></param>
        public static implicit operator decimal(MultiPeriodField field)
        {
            return (decimal)field.Value;
        }

        /// <summary>
        /// Sets the value of the field for the specified period
        /// </summary>
        /// <param name="period">The period</param>
        /// <param name="value">The value to be set</param>
        public void SetPeriodValue(byte period, decimal value)
        {
            var newValue = new PeriodField(period, (float)value);
            if (Store == null)
            {
                Store = new[] { newValue };
            }
            else
            {
                var existing = Array.Find(Store, field => field.Period == period);
                if (!existing.Equals(default(PeriodField)))
                {
                    // if it exists we update it
                    existing.Value = (float)value;
                }
                else
                {
                    // we add it
                    Store = new List<PeriodField>(Store) { newValue }.ToArray();
                }
            }
        }

        /// <summary>
        /// Returns true if the field has at least one value for one period
        /// </summary>
        public bool HasValues()
        {
            return Store != null && Store.Length > 0;
        }

        /// <summary>
        /// Applies updated values from <paramref name="update"/> to this instance
        /// </summary>
        /// <remarks>Used to apply data updates to the current instance. This WILL overwrite existing values.</remarks>
        /// <param name="update">The next data update for this instance</param>
        public void UpdateValues(MultiPeriodField update)
        {
            if (update == null)
                return;

            if (Store == null)
            {
                Store = new PeriodField[1];
            }
            foreach (var kvp in update.Store)
            {
                SetPeriodValue(kvp.Period, (decimal)kvp.Value);
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Join(";", Store.Select(x => x.Period + ":" + x.Value));
        }
    }
}
