using Archipelago.MultiClient.Net.Packets;
using HarmonyLib;
using RewiredConsts;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AstreaArchipelago.src.Archipelago
{
    public class APUI : MonoBehaviour
    {
        public static string mouse_target_desc = "";
        private bool show_warps = false;
        private bool show_items = false;
        private float copied_fade = 0.0f;

        void OnGUI()
        {
            GUI.Box(new Rect(0, 0, Screen.width, 120), "");
            string ap_ver = "Archipelago v" + APState.AP_VERSION[0] + "." + APState.AP_VERSION[1] + "." + APState.AP_VERSION[2];
            if (APState.Session != null)
            {
                if (APState.Authenticated)
                {
                    GUI.Label(new Rect(16, 16, 300, 20), ap_ver + " Status: Connected");
                } else
                {
                    GUI.Label(new Rect(16, 16, 300, 20), ap_ver + " Status: Authentication failed");
                }
            } else
            {
                GUI.Label(new Rect(16, 16, 300, 20), ap_ver + " Status: Not Connected");
            }

            if ((APState.Session == null || !APState.Authenticated) && APState.state == APState.State.Menu)
            {
                GUI.Label(new Rect(16, 36, 150, 20), "Host: ");
                GUI.Label(new Rect(16, 56, 150, 20), "PlayerName: ");
                GUI.Label(new Rect(16, 76, 150, 20), "Password: ");

                bool submit = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;

                APState.ServerConnectInfo.host_name = GUI.TextField(new Rect(150 + 16 + 8, 36, 150, 20),
                    APState.ServerConnectInfo.host_name);
                APState.ServerConnectInfo.slot_name = GUI.TextField(new Rect(150 + 16 + 8, 56, 150, 20),
                    APState.ServerConnectInfo.slot_name);
                APState.ServerConnectInfo.password = GUI.TextField(new Rect(150 + 16 + 8, 76, 150, 20),
                    APState.ServerConnectInfo.password);

                if (submit && Event.current.type == EventType.KeyDown)
                {
                    // The text fields have not consumed the event, which means they were not focused.
                    submit = false;
                }

                if ((GUI.Button(new Rect(16, 96, 100, 20), "Connect") || submit) && APState.ServerConnectInfo.Valid)
                {
                    APState.Connect();
                }
            } else if (APState.state == APState.State.InGame && APState.Session != null)// && Player.main != null)
            {

                if (PlayerNearStart())
                {
                    //GUI.Label(new Rect(16, 76, 1000, 22),
                    //    "Goal: " + APState.Goal);
                    //if (APState.SwimRule.Length == 0)
                    //{
                    //    GUI.Label(new Rect(16, 96, 1000, 22),
                    //        "No Swim Rule sent by Server. Assuming items_hard." +
                    //        " Current Logical Depth: " + (TrackerThread.LogicSwimDepth +
                    //                                      TrackerThread.LogicVehicleDepth));
                    //} else
                    //{
                    //    GUI.Label(new Rect(16, 96, 1000, 22),
                    //        "Swim Rule: " + APState.SwimRule +
                    //        " Current Logical Depth: " + (TrackerThread.LogicSwimDepth +
                    //                                      TrackerThread.LogicVehicleDepth) +
                    //        " = " + TrackerThread.LogicSwimDepth + " (Swim) + " + TrackerThread.LogicVehicleDepth +
                    //        " (" + TrackerThread.LogicVehicle + ")");
                    //}
                }
                if (!APState.TrackerProcessing.IsAlive)
                {
                    GUI.Label(new Rect(16, 116, 1000, 22),
                        "Error: Tracker Thread died. Tracker will not update.");
                }
            }
        }

        public bool PlayerNearStart()
        {
            return false;
        }

        private void Start()
        {
            RegisterCmds();
        }

        public void RegisterCmds()
        {
            //DevConsole.RegisterConsoleCommand(this, "say", false, false);
            //DevConsole.RegisterConsoleCommand(this, "silent", false, false);
            //DevConsole.RegisterConsoleCommand(this, "tracker", false, false);
            //DevConsole.RegisterConsoleCommand(this, "deathlink", false, false);
            //DevConsole.RegisterConsoleCommand(this, "resync", false, false);
            //DevConsole.RegisterConsoleCommand(this, "apdebug", false, false);
        }

        //[HarmonyPatch(typeof(ConsoleInput))]
        //[HarmonyPatch("Validate")]
        //internal class ConsoleHook
        //{
        //    [HarmonyPrefix]
        //    private static bool AllowExclamationPoint(string text, int pos, char ch, ref char __result)
        //    {
        //        if (ch == '!')
        //        {
        //            __result = ch;
        //            return false;
        //        }

        //        return true;
        //    }
        //}

        //private void OnConsoleCommand_say(NotificationCenter.Notification n)
        //{
        //    string text = "";

        //    for (var i = 0; i < n.data.Count; i++)
        //    {
        //        text += (string)n.data[i];
        //        if (i < n.data.Count - 1) text += " ";
        //    }

        //    if (APState.Session != null && APState.Authenticated)
        //    {
        //        var packet = new SayPacket();
        //        packet.Text = text;
        //        APState.Session.Socket.SendPacket(packet);
        //    } else
        //    {
        //        Logging.Log("Can only 'say' while connected to Archipelago.");
        //    }
        //}
        //private void OnConsoleCommand_silent(NotificationCenter.Notification n)
        //{
        //    APState.Silent = !APState.Silent;

        //    if (APState.Silent)
        //    {
        //        Logging.Log("Muted Archipelago chat.");
        //        APState.message_queue.Clear();
        //    } else
        //    {
        //        Logging.Log("Enabled Archipelago chat.");
        //    }
        //}
        //private void OnConsoleCommand_tracker(NotificationCenter.Notification n)
        //{
        //    switch (APState.TrackedMode)
        //    {
        //        case TrackerMode.Disabled:
        //            APState.TrackedMode = TrackerMode.Closest;
        //            Logging.Log("Tracking Locations by proximity.");
        //            break;
        //        case TrackerMode.Closest:
        //            APState.TrackedMode = TrackerMode.Logical;
        //            Logging.Log("Tracking Locations by proximity and filtering by logic");
        //            break;
        //        case TrackerMode.Logical:
        //            APState.TrackedMode = TrackerMode.Disabled;
        //            Logging.Log("Location tracking disabled.");
        //            break;
        //    }
        //}
        //private void OnConsoleCommand_deathlink(NotificationCenter.Notification n)
        //{
        //    APState.ServerConnectInfo.death_link = !APState.ServerConnectInfo.death_link;
        //    APState.set_deathlink();

        //    if (APState.ServerConnectInfo.death_link)
        //    {
        //        Logging.Log("Enabled DeathLink.");
        //    } else
        //    {
        //        Logging.Log("Disabled DeathLink.");
        //    }
        //}

        //private void OnConsoleCommand_resync(NotificationCenter.Notification n)
        //{
        //    if (APState.state == APState.State.InGame)
        //    {
        //        Logging.Log("Beginning Item resync.");
        //        APState.Resync();
        //        Logging.Log("Item resync completed.");
        //    } else
        //    {
        //        Logging.Log("Cannot resync in menu.");
        //    }
        //}

        //private void OnConsoleCommand_apdebug(NotificationCenter.Notification n)
        //{
        //    //var loc = APState.TrackedLocation;
        //    //var loc_data = APState.LOCATIONS[loc];
        //    //DevConsole.SendConsoleCommand("warp "+(int)loc_data.Position.x+" "+(int)loc_data.Position.y+" "+(int)loc_data.Position.z);

        //    //Debug.LogError("Analysis:");
        //    //string json = JsonConvert.SerializeObject(Player.main.pdaData.analysisTech);
        //    //Debug.LogError(json);
        //}
    }

}
