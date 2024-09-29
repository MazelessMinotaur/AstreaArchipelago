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
            if (session != null)
            {
                Logger.LogInfo("session already created?");
            }
            Connect();
            if (session != null)
            {
                Logger.LogInfo("session succesfully created ");
            }
        }

        private void Connect()
        {
            session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

            pendingRewards = new Queue<Reward>();

            Version v = new System.Version("0.5.0");
            LoginResult result;
            try
            {
                result = session.TryConnectAndLogin(
                    "Astrea",
                    "Astrea_test",
                    ItemsHandlingFlags.AllItems,
                    v
                    );
            } catch (Exception e)
            {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (!result.Successful)
            {
                LoginFailure failure = (LoginFailure)result;
                string errorMessage = $"Failed to Connect to";
                foreach (string error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }
                foreach (ConnectionRefusedError error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }

                return; // Did not connect, show the user the contents of `errorMessage`
            }


            session.Items.ItemReceived += (receivedItemsHelper) => {
                ItemInfo item = session.Items.DequeueItem();
                ReceiveItem(item);
                receivedItemsHelper.DequeueItem();
            };
            while (session.Items.Any())
            {
                ReceiveItem(session.Items.DequeueItem());
            }

            var loginSuccess = (LoginSuccessful)result;
        }

        static void SetUpRewardDict()
        {
            // TODO move to a const file
            rewardNameMap.Add("Epic Dice Choice", "Gain an Epic Die");
            rewardNameMap.Add("Standard Dice Choice", "2 Random Dice");
            rewardNameMap.Add("Regular Blessing", "Gain a Star Blessing");
            rewardNameMap.Add("BlackHole Blessing", "GetLocalizedString failed. -> ID: Black Hole Blessing"); // TODO there's a better way to do this
            rewardNameMap.Add("Duplicate Dice", "Duplicate a non-Epic Die from your dice pool 1 times.");
            rewardNameMap.Add("Forge Draw", "Forge: Draw 2");
        }

        static void ReceiveItem(ItemInfo item)
        {
            if (item == null)
            {
                return;
            }
            Logger.LogInfo(item.ItemName);

            if (item.ItemName == "77 Star Shards")
            {
                Logger.LogInfo("start shards received");

                if (playerData != null)
                {
                    // Yeah starshards are called gold.
                    playerData.ModifyGold(77);
                }
            }


            //Reward[] r = Resources.FindObjectsOfTypeAll<Astrea.Reward>();
            Reward reward = BuildRewardFromItem(item);
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
        static void RestoreRewards(EndOfBattleState __instance)
        {
            Logger.LogInfo($"end of battle postfix called");

            NodeEnum e = __instance.mapHandler.CurrentNodeData.NodeEnum;
            if (e != NodeEnum.BATTLENORMAL && e != NodeEnum.BATTLEHARD && e != NodeEnum.BATTLEBOSS)
            {
                Logger.LogInfo($"not a battle node, skip returning rewards");
                return;
            }

            if (storedRewards == null)
            {
                Logger.LogInfo($"no stored rewards?");
                return;
            }

            Logger.LogInfo($"attempting to return old rewards");

            __instance.mapHandler.currentReward.rewards = storedRewards;
            storedRewards = null;
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
                        session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Tainted Reef Fight 3 - Hard reward"));
                    }
                    break;
                case 4:
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Tainted Reef Boss Fight - First reward"));
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Tainted Reef Boss Fight - Second reward"));
                    session.Locations.CompleteLocationChecks(session.Locations.GetLocationIdFromName("Astrea", "Tainted Reef Boss Fight - Third reward"));
                    break;

            }
        }

    }
}


