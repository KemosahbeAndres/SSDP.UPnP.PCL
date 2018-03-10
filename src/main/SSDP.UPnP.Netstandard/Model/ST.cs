﻿using System;
using System.Collections.Generic;
using System.Text;
using ISSDP.UPnP.PCL.Enum;
using ISSDP.UPnP.PCL.Interfaces.Model;

namespace SSDP.UPnP.PCL.Model
{
    internal class ST : IST
    {
        public STtype STtype { get; internal set; }
        public string DeviceUUID { get; internal set; }
        public string Type { get; internal set; }
        public string Version { get; internal set; }
        public string DomainName { get; internal set; }
        public bool HasDomain { get; internal set; }
    }
}
