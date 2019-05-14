﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;

namespace MetaLive
{
    enum UserDefinedScanType
    {
        FullScan,
        DataDependentScan,
        BoxCarScan

    }

    class UserDefinedScan
    {
        public UserDefinedScan(UserDefinedScanType userDefinedScanType)
        {
            UserDefinedScanType = userDefinedScanType;
        }

        public UserDefinedScanType UserDefinedScanType { get; }
        public List<Tuple<double, int>> Mass_Charges{get; set;}
        public List<double> dynamicBox { get; set; }

    }
}