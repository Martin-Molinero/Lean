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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Python.Runtime;
using QuantConnect.Configuration;
using QuantConnect.Logging;
using QuantConnect.Python;

namespace QuantConnect.AlgorithmFactory
{
    /// <summary>
    /// Helper class used to start a new debugging session
    /// </summary>
    public static class DebuggerHelper
    {
        /// <summary>
        /// The different implemented debugging methods
        /// </summary>
        public enum DebuggingMethod
        {
            /// <summary>
            /// Local debugging through cmdline.
            /// <see cref="Language.Python"/> will use built in 'pdb'
            /// </summary>
            LocalCmdline,

            /// <summary>
            /// Remote debugging.
            /// <see cref="Language.Python"/> will use 'web_pdb'
            /// </summary>
            Remote,

            /// <summary>
            /// Visual studio local debugging.
            /// <see cref="Language.Python"/> will use 'Python Tools for Visual Studio',
            /// attach manually selecting `Python` code type.
            /// </summary>
            VisualStudio
        }

        /// <summary>
        /// Will start a new debugging session
        /// </summary>
        public static void Initialize(Language language)
        {
            if (Config.GetBool("debugging"))
            {
                if (language == Language.Python)
                {
                    DebuggingMethod debuggingType;
                    Enum.TryParse(Config.Get("debugging-method", DebuggingMethod.VisualStudio.ToString()), out debuggingType);

                    Log.Trace("DebuggerHelper.Initialize(): initializing python...");
                    PythonInitializer.Initialize();
                    Log.Trace("DebuggerHelper.Initialize(): python initialization done");

                    using (Py.GIL())
                    {
                        Log.Trace("DebuggerHelper.Initialize(): starting...");
                        switch (debuggingType)
                        {
                            case DebuggingMethod.Remote:
                                var ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                                    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                                if (ipAddress == null)
                                {
                                    throw new InvalidOperationException("DebuggerHelper.Initialize(): no valid IP was found");
                                }

                                var environmentVariablePort = Environment.GetEnvironmentVariable("DEBUGGING_PORT");
                                var port = environmentVariablePort ?? Config.Get("debugging-port", "7676");
                                Log.Trace($"DebuggerHelper.Initialize(): waiting for remote web_pdb at {ipAddress}:{port}...");
                                PythonEngine.RunSimpleString($"import web_pdb; web_pdb.set_trace(host='{ipAddress}', port={port}).set_trace()");
                                break;

                            case DebuggingMethod.LocalCmdline:
                                PythonEngine.RunSimpleString("import pdb; pdb.set_trace()");
                                break;

                            case DebuggingMethod.VisualStudio:
                                Log.Trace("DebuggerHelper.Initialize(): waiting for debugger to attach...");
                                PythonEngine.RunSimpleString(@"import sys; import time;
while not sys.gettrace():
    time.sleep(0.25)");
                                break;
                        }
                        Log.Trace("DebuggerHelper.Initialize(): started");
                    }
                }
                else
                {
                    if(Debugger.IsAttached)
                    {
                        Log.Trace("DebuggerHelper.Initialize(): debugger is already attached, triggering initial break.");
                        Debugger.Break();
                    }
                    else
                    {
                        Log.Trace("DebuggerHelper.Initialize(): waiting for debugger to attach...");
                        while (!Debugger.IsAttached)
                        {
                            Thread.Sleep(250);
                        }
                        Log.Trace("DebuggerHelper.Initialize(): debugger attached");
                    }
                }
            }
        }
    }
}
