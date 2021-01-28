//   Copyright 2017-2019 Davinci Codes
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Also use the software for non-commercial purposes.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE

using System;

namespace Pandora.Client.ClientLib
{
    /// <summary>
    /// Active = The currency is tradable and can be used for sending and recieving coins.
    /// Disabled =  The currency is down and may not return to active status.
    /// Maintenance = The currency is needs to be fixed on the server side before tx can be sent.
    /// </summary>
    public enum CurrencyStatus { Active, Disabled, Maintenance, Updated }

    public class CurrencyStatusItem
    {
        public CurrencyStatusItem()
        {
        }

        public CurrencyStatusItem(long aStatusId, long aCurrencyId, DateTime aStatusTime, CurrencyStatus aStatus, string aExtendedInfo, long aBlockHeight)
        {
            StatusId = aStatusId;
            CurrencyId = aCurrencyId;
            StatusTime = aStatusTime;
            Status = aStatus;
            ExtendedInfo = aExtendedInfo;
            BlockHeight = aBlockHeight;
        }

        public long StatusId { get; private set; }

        public long CurrencyId { get; private set; }

        public DateTime StatusTime { get; private set; }

        public CurrencyStatus Status { get; private set; }

        public string ExtendedInfo { get; private set; }

        public long BlockHeight { get; private set; }
    }
}