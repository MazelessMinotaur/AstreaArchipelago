using BepInEx;
using BepInEx.Logging;
using Astrea;
using Astrea.BattleActions;
using HarmonyLib;
using UnityEngine;
using Astrea.GameStates;
using Astrea.RunModifiers;
using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Enums;
using Archipelago;
using static System.Collections.Specialized.BitVector32;
using Archipelago.MultiClient.Net.Models;
using System.Linq;
using UnityEngine.Analytics;
using System.Collections.ObjectModel;
using AstreaArchipelago.src;
using System.Reflection;
using AstreaArchipelago.src.UI;
using AstreaArchipelago.src.Archipelago;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AstreaArchipelago
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        static PlayerData playerData;

        static private ArchipelagoSession session;

        static int count_check = 0;

        static int locations_checked = 0;

        static NodeData lastNodeSeen;

        static Reward[] storedRewards;

        static Queue<Reward> pendingRewards;

        static Dictionary<string, string> rewardNameMap = new Dictionary<string, string>();

        static RewardService rewardService;

        static GameObject apui = null;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            Logger.LogInfo($"Beginning patching, Good luck :)");
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogInfo($"Main patch done");
            if (true)
            {
                Logger.LogInfo($"patching experiments");
                Harmony.CreateAndPatchAll(typeof(Experiments));
                Experiments.Logger = Logger;
                Logger.LogInfo($"patching experiments finished");
            }
            SetUpRewardDict(); 
            Logger.LogInfo($"patching done!!!!a :)");
            //if (session != null)
            //{
            //    Logger.LogInfo("session already created?");
            //}
            Setup();
            //if (session != null)
            //{
            //    Logger.LogInfo("session succesfully created ");
            //}
            rewardService = new RewardService(Logger);
            rewardService.BuildRewardMap();
            CreateArchipelagoUI();
        }

        private void Setup()
        {
            if (APState.Session == null)
            {
                return;
            }

            if (session != null)
            {
                return;
            }
            session = APState.Session;
            pendingRewards = new Queue<Reward>();

            session.Items.ItemReceived += (receivedItemsHelper) => {
                Logger.LogInfo("running item received helper");
                ItemInfo item = session.Items.DequeueItem();
                ReceiveItem(item);
                receivedItemsHelper.DequeueItem();
                Logger.LogInfo("done receving?");

            };
            while (session.Items.Any())
            {
                Logger.LogInfo("dequeue item");

                ReceiveItem(session.Items.DequeueItem());
            }

        }

        static void SetUpRewardDict()
        {
            // TODO has moved to RewardService
            rewardNameMap.Add("Epic Dice Choice", "Gain an Epic Die");
            rewardNameMap.Add("Standard Dice Choice", "2 Random Dice");
            rewardNameMap.Add("Regular Blessing", "Gain a Star Blessing");
            rewardNameMap.Add("BlackHole Blessing", "GetLocalizedString failed. -> ID: Black Hole Blessing"); // TODO there's a better way to do this
            rewardNameMap.Add("Duplicate Dice", "Duplicate a non-Epic Die from your dice pool 1 times.");
            rewardNameMap.Add("Forge Draw", "Forge: Draw 2");
        }

        static void ReceiveItem(ItemInfo item)
        {
            Logger.LogInfo("item received");
            if (item == null)
            {
                Logger.LogInfo("item null");
                return;
            }
            Logger.LogInfo(item.ItemName);

            if (item.ItemName == "77 Star Shards")
            {
                Logger.LogInfo("start shards received");

                if (playerData != null)
                {
                    // Yeah starshards are called gold.
                    playerData.ModifyGold(77); // this didn't work?
                }
            }


            //Reward[] r = Resources.FindObjectsOfTypeAll<Astrea.Reward>();
            Reward reward = rewardService.ArchipelagoItemToReward(item);
            if (reward == null)
            {
                Logger.LogInfo($"failed to build reward from item");

                return;
            }

            Logger.LogInfo($"adding a new reward item to queue");

            pendingRewards.Enqueue(reward);

            Logger.LogInfo($"pending reward size ${pendingRewards.Count()}");
        }

        static Reward BuildRewardFromItem(ItemInfo item)
        {

            Reward[] r = Resources.FindObjectsOfTypeAll<Reward>();

            string targetReward;
            if (!rewardNameMap.TryGetValue(item.ItemName, out targetReward)) {
                Logger.LogInfo($"item not found in map, id {item.ItemId}, name {item.ItemName}");
                return null;
            }

            Logger.LogInfo($"reward length {r.Length}");

            for (int i = 0; i < r.Length; i++)
            {
                Logger.LogInfo($"reward check, checking {r[i].RewardName}, getting {r[i].GetRewardName()}, target {targetReward}");

                if (r[i].GetRewardName() == targetReward)
                {
                    return r[i];
                }
            }
            Logger.LogInfo($"reward for item not found, id {item.ItemId}, name {item.ItemName}, target {targetReward}");

            return null;
        }

        static void AddBonusRewards(EndOfBattleState state)
        {
            NodeData node = state.mapHandler.CurrentNodeData;
            Logger.LogInfo("adding bonus rewards");

            if (storedRewards == null)
            {
                Logger.LogInfo("storing rewards");
                storedRewards = state.mapHandler.currentReward.rewards;
            }

            var tempList = state.mapHandler.currentReward.rewards.ToList();
            while(pendingRewards.Any())
            {
                tempList.Add(pendingRewards.Dequeue());
            }

            state.mapHandler.currentReward.rewards = tempList.ToArray();

            return;
        }

        [HarmonyPatch(typeof(EndOfBattleState), "AllRewardsCollectedGoToMapOrToAstreasGate")]
        [HarmonyPostfix]
        public static void CreateArchipelagoUI()
        {
            if (APState.ArchipelagoUI != null)
            {
                Logger.LogInfo("skip");
                return;
            }
            // Create a game object that will be responsible to drawing the IMGUI in the Menu.
            var guiGameobject = new GameObject();
            guiGameobject.SetActive(false);
            APState.ArchipelagoUI = guiGameobject.AddComponent<APUI>();
            GameObject.DontDestroyOnLoad(guiGameobject);

            apui = guiGameobject;

            //var storage = PlatformUtils.main.GetServices().GetUserStorage() as UserStoragePC;
            //var rawPath = storage.GetType().GetField("savePath",
            //    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(storage);
            //var lastConnectInfo = APLastConnectInfo.LoadFromFile(rawPath + "/archipelago_last_connection.json");
            //if (lastConnectInfo != null)
            //{
            //    APState.ServerConnectInfo.FillFromLastConnect(lastConnectInfo);
            //}
        }



        [HarmonyPatch(typeof(MapHandler), "NodePressed")]
        [HarmonyPrefix]
        static bool NodePressedFromMapPrefix(int nodeIndex, int areaLevelIndex, MapHandler __instance)
        {
            Logger.LogInfo($"map handler, pre node pressed: {areaLevelIndex}, {nodeIndex}, ");
            //rewardService.storeRewards(__instance);

            return true;
        }


        [HarmonyPatch(typeof(MapHandler), "NodePressed")]
        [HarmonyPostfix]
        static void NodePressedFromMapPostFix(int nodeIndex, int areaLevelIndex, MapHandler __instance)
        {
            Logger.LogInfo($"map handler, post node pressed: {areaLevelIndex}, {nodeIndex}, ");
            //rewardService.storeRewards(__instance);

            return;
        }



        [HarmonyPatch(typeof(EndOfBattleState), "OnStartState")]
        [HarmonyPrefix]
        static bool EndOfBattlePrefix(EndOfBattleState __instance)
        {
            MapHandler m = __instance.mapHandler;
            NodeData node = __instance.mapHandler.CurrentNodeData;
            NodeEnum e = __instance.mapHandler.CurrentNodeData.NodeEnum;

            Logger.LogInfo($"end of battle {e}");

            Logger.LogInfo($"map info {m.GetCurrentAreaEnum()}");
            
            if (e != NodeEnum.BATTLENORMAL && e != NodeEnum.BATTLEHARD && e != NodeEnum.BATTLEBOSS)
            {
                Logger.LogInfo($"not a battle node, skip");
                return true;
            }

            AddBonusRewards(__instance);


            if (node.Equals(lastNodeSeen))
            {
                Logger.LogInfo("already handled this node");
                return true;
            }
            lastNodeSeen = node;
            
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();

            Logger.LogInfo(t.ToString());

            CompleteLocatoinCheck(__instance);

            return true;
        }


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                if (APState.ArchipelagoUI == null)
                {
                    return;
                }

                apui.SetActive(!apui.activeSelf);
                Setup();
            }
        }

        static void CompleteLocatoinCheck(EndOfBattleState state)
        {
            // TODO actually figure out what fight we're on
            count_check++;
            Logger.LogInfo($"check location {count_check}");

            ReadOnlyCollection<long> missing = session.Locations.AllMissingLocations;
            foreach (long location in missing)
            {
                Logger.LogInfo($"missing {location}");
                Logger.LogInfo(session.Locations.GetLocationNameFromId(location));
            }
            ReadOnlyCollection<long> all = session.Locations.AllLocations;
            foreach (long location in all)
            {
                Logger.LogInfo($"all {location}");
                Logger.LogInfo(session.Locations.GetLocationNameFromId(location));
            }


            switch (count_check)
            {
                // TODO this sucks do better
                case 1:
                    long id = (session.Locations.GetLocationIdFromName("Astrea", "Tainted Reef Fight 1 - First reward"));
                    Logger.LogInfo($"id: {id}");
                    session.Locations.CompleteLocationChecks(id);
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Tainted Reef Fight 1 - Second reward"));
                    Logger.LogInfo($"sent?");
                    break;
                case 2:
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Tainted Reef Fight 2 - First reward"));
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Tainted Reef Fight 2 - Second reward"));
                    break;
                case 3:
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Tainted Reef Fight 3 - First reward"));
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Tainted Reef Fight 3 - Second reward"));
                    if (state.mapHandler.CurrentNodeData.NodeEnum == NodeEnum.BATTLEHARD)
                    {
                        session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Tainted Reef Fight 3 - Hard reward")); // this didn't work?
                    }
                    break;
                case 4:
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Tainted Reef Boss Fight - First reward"));
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Tainted Reef Boss Fight - Second reward"));
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Tainted Reef Boss Fight - Third reward"));
                    break;
                case 5:
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Astropolis Ruins Fight 1 - First reward"));
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Astropolis Ruins Fight 1 - Second reward"));
                    break;
                case 6:
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Astropolis Ruins Fight 2 - First reward"));
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Astropolis Ruins Fight 2 - Second reward"));
                    if (state.mapHandler.CurrentNodeData.NodeEnum == NodeEnum.BATTLEHARD)
                    {
                        session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Astropolis Ruins Fight 2 - Hard reward")); // but this did?
                    }
                    break;
                case 7:
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Astropolis Ruins Boss Fight - First reward")); // Tehse didn't trigger?
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Astropolis Ruins Boss Fight - Second reward"));
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Astropolis Ruins Boss Fight - Third reward"));
                    break;
                case 8:
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Ground Zero Fight 1 - First reward"));  //didnt' send either?
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Ground Zero Fight 1 - Second reward"));
                    if (state.mapHandler.CurrentNodeData.NodeEnum == NodeEnum.BATTLEHARD)
                    {
                        session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Ground Zero Fight 1 - Hard reward"));
                    }
                    break;
                case 9:
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Ground Zero Fight 2 - First reward")); //also no?
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Ground Zero Fight 2 - Second reward"));
                    if (state.mapHandler.CurrentNodeData.NodeEnum == NodeEnum.BATTLEHARD)
                    {
                        session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Ground Zero Fight 2 - Hard reward"));
                    }
                    break;
                case 10:
                    break;
            }
        }

    }
}


