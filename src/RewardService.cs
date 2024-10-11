using Archipelago.MultiClient.Net.Models;
using Astrea;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AstreaArchipelago.src
{
    public class RewardService
    {
        internal static ManualLogSource Logger;

        // the basic list of rewards 
        List<Reward> basicBattleReward;
        List<Reward> basicHardBattleReward;
        List<Reward> basicBossBattleReward;

        // 
        List<Reward> originalBattleReward;
        List<Reward> originalHardBattleReward;
        List<Reward> originalBossBattleReward;

        List<Reward>[] pendingRewards;
        static Dictionary<string, string> rewardNameMap = null;
        static Dictionary<string, Reward> rewardMap = null;

        public RewardService(ManualLogSource logger)
        {
            Logger = logger;
        }

        public void BuildRewardMap()
        {
            if (rewardNameMap != null)
            {
                return;
            }

            rewardNameMap = new Dictionary<string, string>
            {
                { "Epic Dice Choice", "Gain an Epic Die" },
                { "Standard Dice Choice", "GetLocalizedString failed. -> ID: Chest" }, // hate this
                { "Regular Blessing", "Gain a Star Blessing" },
                { "BlackHole Blessing", "GetLocalizedString failed. -> ID: Black Hole Blessing" }, // TODO make this better
                { "Duplicate Dice", "Duplicate a non-Epic Die from your dice pool 1 times." },
                { "Forge Draw", "Forge: Draw 2" },
            };
        }

        public Reward ArchipelagoItemToReward(ItemInfo item)
        {
            string targetReward;
            if (!rewardNameMap.TryGetValue(item.ItemName, out targetReward))
            {
                Logger.LogInfo($"item not found in map, id {item.ItemId}, name {item.ItemName}");
                return null;
            }

            if (rewardMap == null)
            {
                FindAllRewards();
            }
            if (rewardMap == null)
            {
                return null;
            }

            Reward r = rewardMap[targetReward];
            
            return r;
        }

        public void FindAllRewards()
        {
            Reward[] r = Resources.FindObjectsOfTypeAll<Reward>();

            Logger.LogInfo($"reward length {r.Length}");

            for (int i = 0; i < r.Length; i++)
            {
                rewardMap[r[i].GetRewardName()] = r[i];
                Logger.LogInfo($"reward list, checking {r[i].RewardName}, getting {r[i].GetRewardName()}");
            }
        }

        // lots of testing to get node rewards stored right
        public void storeRewards(MapHandler m)
        {
            if (m == null)
            {
                return;
            }

            Logger.LogInfo($"storeRewards 1: node idx {m.CurrentNodeIndex}, area idx {m.CurrentAreaLevelIndex}");

            if (m.CurrentNodeIndex < 0 || m.CurrentAreaLevelIndex < 0)
            {
                return;
            }

            NodeData node = m.CurrentNodeData;

            if (node == null)
            {
                return;
            }
            Logger.LogInfo($"storeRewards 2");

            NodeEnum e = node.NodeEnum;

            if (m.currentReward == null)
            {
                return;
            }
            if (m.currentReward.rewards == null)
            {
                return;
            }
            Logger.LogInfo($"storeRewards 3");


            if (m.currentReward.rewards.Length == 0)
            {
                return;
            }

            for (int i = 0; i < m.currentReward.rewards.Length; i++)
            {
                Reward r = m.currentReward.rewards[i];
                if (r != null)
                {
                    Logger.LogInfo($"Reward info for node, name: {r.GetRewardName()}");
                }
            }

            Logger.LogInfo($"storing rewards {e}");

            if (e != NodeEnum.BATTLENORMAL && e != NodeEnum.BATTLEHARD && e != NodeEnum.BATTLEBOSS)
            {
                Logger.LogInfo($"not a battle node, skip");
                return;
            }

            if (e == NodeEnum.BATTLEHARD)
            {
                if (this.originalHardBattleReward == null)
                {
                    Logger.LogInfo($"Sotring for hard battle");
                    this.originalHardBattleReward = new List<Reward>();
                    this.originalHardBattleReward.AddRange(m.currentReward.rewards.ToList());
                }
                return;
            }

            if (e == NodeEnum.BATTLENORMAL)
            {
                if (originalBattleReward == null)
                {
                    Logger.LogInfo($"Sotring for normal battle");
                    originalBattleReward = new List<Reward>();
                    originalBattleReward.AddRange(m.currentReward.rewards.ToList());
                    Reward r1 = m.currentReward.rewards[0];
                    Reward r2 = m.currentReward.rewards[1];
                    basicBattleReward = new List<Reward> { r1, r2 };
                    Logger.LogInfo($"Changing battle rewards");
                    m.currentReward.rewards = basicBattleReward.ToArray();
                }
            }
        }
    }
}
