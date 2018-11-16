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

#pragma once
#include "../QuantConnectC/PublicLocalFileSubscriptionStreamReader.h"

using namespace QuantConnect;

namespace CWrapper
{
	public ref class LocalFileSubscriptionStreamReaderCLR : public Interfaces::IStreamReader
	{
		PublicLocalFileSubscriptionStreamReader* reader;
	public:
		LocalFileSubscriptionStreamReaderCLR(System::String^ source);
		~LocalFileSubscriptionStreamReaderCLR()
		{
			delete reader;
		}

		virtual property bool EndOfStream
		{
			bool get() { return reader->EndOfStream(); }
		}

		/// <summary>Gets the transport medium of this stream reader</summary>
		virtual property SubscriptionTransportMedium TransportMedium
		{
			SubscriptionTransportMedium get() sealed { return SubscriptionTransportMedium::LocalFile; }
		}

		virtual System::String^ ReadLine();
	};
}
