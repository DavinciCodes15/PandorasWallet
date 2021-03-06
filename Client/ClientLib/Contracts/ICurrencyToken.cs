﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.ClientLib.Contracts
{
    public interface ICurrencyToken : ICurrencyIdentity
    {
        string ContractAddress { get; }
        ushort Precision { get; }
        long ParentCurrencyID { get; }
    }
}