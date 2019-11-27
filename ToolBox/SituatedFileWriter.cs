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
using System.IO;
using QuantConnect.Util;

namespace QuantConnect.ToolBox
{
    /// <summary>
    /// Helper class that writes lines to a file path and provides the starting
    /// position in bytes of each line
    /// </summary>
    /// <remarks>This class is useful to perform <see cref="Stream.Seek"/> operations during read</remarks>
    public class SituatedFileWriter : IDisposable
    {
        private readonly StreamWriter _writer;

        /// <summary>
        /// Creates a new instances of this file writer for a given path
        /// </summary>
        public SituatedFileWriter(string path)
        {
            _writer = new StreamWriter(path) { AutoFlush = true };
        }

        /// <summary>
        /// Write the provided data in a new line and returns the starting position
        /// </summary>
        public long WriteLine(string data)
        {
            var currentPosition = _writer.BaseStream.Length;
            _writer.WriteLine(data);
            return currentPosition;
        }

        public void Dispose()
        {
            _writer.DisposeSafely();
        }
    }
}
