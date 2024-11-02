using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using Archipelago.MultiClient.Net.Enums;
using LittleLeo.Events;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;

namespace AstreaArchipelago.src.Archipelago
{
    public static class APState
    {
        public struct Location
        {
            public long ID;
            public Vector3 Position;
        }

        public enum State
        {
            Menu,
            InGame
        }

        public static int[] AP_VERSION = new int[] { 0, 5, 0 };
        public static APConnectInfo ServerConnectInfo = new APConnectInfo();
        public static DeathLinkService DeathLinkService = null;
        public static bool DeathLinkKilling = false; // indicates player is currently getting DeathLinked
        public static Dictionary<string, int> archipelago_indexes = new();
        public static float unlock_dequeue_timeout = 0.0f;
        public static List<string> message_queue = new();
        public static float message_dequeue_timeout = 0.0f;
        public static State state = State.Menu;
        public static bool Authenticated;
        public static bool FreeSamples;
        public static bool Silent = false;
        public static Thread TrackerProcessing;
        public static long TrackedLocationsCount = 0;
        public static long TrackedFishCount = 0;
        public static string TrackedFish = "";
        public static long TrackedLocation = -1;
        public static string TrackedLocationName;
        public static float TrackedDistance;
        public static float TrackedAngle;

        public static ArchipelagoSession Session;
        public static APUI ArchipelagoUI = null;

        public static bool Connect()
        {
            if (Authenticated)
            {
                return true;
            }

            if (ServerConnectInfo.host_name is null || ServerConnectInfo.host_name.Length == 0)
            {
                return false;
            }

            // Start the archipelago session.
            Session = ArchipelagoSessionFactory.CreateSession(ServerConnectInfo.host_name);

            LoginResult loginResult = Session.TryConnectAndLogin(
                "Astrea",
                ServerConnectInfo.slot_name,
                ItemsHandlingFlags.AllItems,
                new Version(AP_VERSION[0], AP_VERSION[1], AP_VERSION[2]),
                null,
                "",
                ServerConnectInfo.password);

            if (loginResult is LoginSuccessful loginSuccess)
            {
                //var storage = PlatformUtils.main.GetServices().GetUserStorage() as UserStoragePC;
                //var rawPath = storage?.GetType().GetField("savePath",
                //        BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(storage);
                //if (rawPath != null)
                //{
                //    ServerConnectInfo.GetAsLastConnect().WriteToFile(rawPath + "/archipelago_last_connection.json");
                //} else
                //{
                //    Logging.LogError("Could not write most recent connect info to file.");
                //}

                Authenticated = true;
                state = State.InGame;
                //if (loginSuccess.SlotData.TryGetValue("swim_rule", out var swim_rule))
                //{
                //    SwimRule = (string)swim_rule;
                //}
                //if (loginSuccess.SlotData.TryGetValue("free_samples", out var free_samples))
                //{
                //    FreeSamples = Convert.ToInt32(free_samples) > 0;
                //}
                //Goal = (string)loginSuccess.SlotData["goal"];
                //GoalMapping.TryGetValue(Goal, out GoalEvent);
                //if (loginSuccess.SlotData["vanilla_tech"] is JArray temp)
                //{
                //    foreach (var tech in temp)
                //    {
                //        vanillaTech.Add((TechType)Enum.Parse(typeof(TechType), tech.ToString()));
                //    }
                //}


                Logging.Log("SlotData: " + JsonConvert.SerializeObject(loginSuccess.SlotData), ingame: false);
                //ServerConnectInfo.death_link = Convert.ToInt32(loginSuccess.SlotData["death_link"]) > 0;
                //set_deathlink();

            } else if (loginResult is LoginFailure loginFailure)
            {
                Authenticated = false;
                Logging.LogError("Connection Error: " + String.Join("\n", loginFailure.Errors));
                Session = null;
            }

            return loginResult.Successful;
        }
    }
}
