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


namespace AstreaArchipelago
{

    // Class holding my random exerimenting of Astrea functions
    // Should be mostly logging
    class Experiments
    {
        internal static new ManualLogSource Logger;

        static void AddLogger(ManualLogSource l)
        {
            Logger = l;
        }

        [HarmonyPatch(typeof(DrawDicesState), nameof(DrawDicesState.DrawDices))]
        [HarmonyPrefix]
        static bool DrawDiceCount(int drawAmount)
        {
            Logger.LogInfo($"DrawDices? ${drawAmount}");

            return true;
        }

        [HarmonyPatch(typeof(Astrea.Node), nameof(Astrea.Node.BaseInitialize))]
        [HarmonyPrefix]
        static bool NodePrefix(int nodeIndex, int areaLevelIndex, NodeData nodeData, List<Astrea.Node> outgoing)
        {
            Logger.LogInfo($"NodeData, reward : {nodeData.Reward} event :{nodeData.Event}");
            Logger.LogInfo($"node index: {nodeIndex}, areaLevelIdx {areaLevelIndex}");

            return true;
        }


        [HarmonyPatch(typeof(EnemyTurn), nameof(EnemyTurn.OnStartTurn))]
        [HarmonyPrefix]
        static bool PrefixEnemyStart()
        {
            Logger.LogInfo($"Enemy Turn?");

            return true;
        }

        [HarmonyPatch(typeof(Astrea.AudioSettings), "ToggleBgm")]
        [HarmonyPrefix]
        static bool PrefixToggleBGM(bool toggle)
        {
            Logger.LogInfo($"toggle aduio ${toggle}");

            Reward[] r = Resources.FindObjectsOfTypeAll<Astrea.Reward>();
            for (int i = 0; i < r.Length; i++)
            {
                Logger.LogInfo($"reward {i}, {r[i].RewardName}, {r[i].GetType()},");
            }


            return true;
        }


        [HarmonyPatch(typeof(EndOfBattleState), "AllRewardsCollectedGoToMapOrToAstreasGate")]
        [HarmonyPostfix]
        static void EndOfBattlePostFix(EndOfBattleState __instance)
        {
            Logger.LogInfo($"end of battle postfix called");
        }

        [HarmonyPatch(typeof(BattleVictoryPanel), nameof(BattleVictoryPanel.Initialize))]
        [HarmonyPrefix]
        static bool BattleVictoryPanelPrefix(bool noReward, BattleVictoryPanel __instance)
        {
            Logger.LogInfo($"no reward ${noReward}");

            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();

            Logger.LogInfo(t.ToString());

            return true;
        }

        [HarmonyPatch(typeof(DiceRolling), "GetRolledDiceFaceIndex")]
        [HarmonyPrefix]
        static bool PrefixRollDie()
        {
            Logger.LogInfo($"GetRolledDiceFaceIndex?");

            return true;
        }



        [HarmonyPatch(typeof(MapHandler), "NodePressed")]
        [HarmonyPrefix]
        static bool NodePressedPrefix(int nodeIndex, int areaLevelIndex)
        {
            Logger.LogInfo($"map handler, node pressed: {areaLevelIndex}, {nodeIndex}, ");

            return true;
        }

        [HarmonyPatch(typeof(Node), "NodePressed")]
        [HarmonyPrefix]
        static bool NodeInfo(Node __instance)
        {
            Logger.LogInfo($"Node pressed: {__instance.areaLevelIndex}, {__instance.nodeIndex}, ");

            return true;
        }

    }
}
