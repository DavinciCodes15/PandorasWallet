﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.ClientLib.Contracts
{
    public interface ICurrencyIdentity
    {
        long Id { get; }

        string Name { get; }

        string Ticker { get; }
    }
}