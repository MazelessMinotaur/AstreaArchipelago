﻿using AstreaArchipelago.src.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstreaArchipelago.src.Archipelago
{
    public class APConnectInfo : APLastConnectInfo
    {
        public long index;
        public HashSet<long> @checked = new();
        public bool death_link = false;
        public Dictionary<long, HashSet<long>> resources_granted = new();

        public void FillFromLastConnect(APLastConnectInfo lastConnectInfo)
        {
            host_name = lastConnectInfo.host_name;
            slot_name = lastConnectInfo.slot_name;
            password = lastConnectInfo.password;
        }
        public APLastConnectInfo GetAsLastConnect()
        {
            var lastConnectInfo = new APLastConnectInfo();
            lastConnectInfo.host_name = host_name;
            lastConnectInfo.slot_name = slot_name;
            lastConnectInfo.password = password;
            return lastConnectInfo;
        }
    }
}
