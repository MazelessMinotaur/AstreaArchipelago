using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstreaArchipelago.src.Archipelago
{
    public class APSaveData
    {
        private readonly ConfigEntry<string> lastServer;
        private readonly ConfigEntry<string> lastUsername;
        private readonly ConfigEntry<int> lastProcessedIndex;

        public string LastServer => lastServer.Value;
        public string LastUsername => lastUsername.Value;
        public int LastProcessedIndex => lastProcessedIndex.Value;

        public int LastReceivedIndex { get; private set; }


        public APSaveData(ConfigFile config)
        {
            lastServer = config.Bind(
                "Connection", "LastServer",
                "archipelago.gg:38281",
                "The last Archipelago server address used");

            lastUsername = config.Bind(
                "Connection", "LastUsername",
                "",
                "The last Archipelago slot name used");

            lastProcessedIndex = config.Bind(
                "Session", "LastProcessedIndex",
                0,
                "Index of the last processed item from the Archipelago server");
        }

        public void OnItemReceived(int index)
        {
            LastReceivedIndex = index;
        }

        public void OnItemProcessed()
        {
            lastProcessedIndex.Value = LastReceivedIndex;
        }

        public void OnConnect(string server, string username)
        {
            lastServer.Value = server;
            lastUsername.Value = username;
        }

        public void ResetSession()
        {
            lastProcessedIndex.Value = 0;
        }
    }
}