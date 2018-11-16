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

#include "stdafx.h"
#include <fstream>
#include "LocalFileSubscriptionStreamReader.h"
#include "ziplib/Source/ZipLib/ZipFile.h"
#include "ziplib/Source/ZipLib/methods/Bzip2Method.h"

LocalFileSubscriptionStreamReader::LocalFileSubscriptionStreamReader(std::string source)
{
	try {
		std::string filename = source;
		std::string entryName = "";
		const auto hashIndex = source.find_last_of("#");
		if (hashIndex != std::string::npos)
		{
			entryName = source.substr(hashIndex + 1);
			filename = source.substr(0, hashIndex);
		}
		const auto extension = filename.substr(max(0, filename.length() - 4));
		if(extension == ".zip") {
			archive = ZipFile::Open(filename);
			if (archive && archive->GetEntriesCount() > 0) {
				if (entryName.empty()) {
					entry = archive->GetEntry(0);
				}
				else {
					entry = archive->GetEntry(entryName);
				}
				if (entry) {
					dataStream = entry->GetDecompressionStream();
				}
			}
		}
		else {
			dataStream = new std::ifstream(filename);
		}
	}
	// if the file does not exist it will throw an exception
	catch(...) {	}
}

LocalFileSubscriptionStreamReader::~LocalFileSubscriptionStreamReader()
{
}

bool LocalFileSubscriptionStreamReader::EndOfStream() const {
	return dataStream == nullptr || dataStream->peek() == EOF;
}

std::string LocalFileSubscriptionStreamReader::ReadLine() const {
	std::string line;
	std::getline(*dataStream, line);
	return line;
}