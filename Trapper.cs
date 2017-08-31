using System;
using System.Collections.Generic;
using System.Globalization;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Trapper", "redBDGR", "1.0.8", ResourceId = 2417)]
    [Description("Adds a few new features to traps")]
    class Trapper : RustPlugin
    {
        private const string permissionNameADMIN = "trapper.admin";
        private const string permissionName = "trapper.auto";
        private const string permissionNameOWNER = "trapper.owner";
        private const string permissionNameFRIENDS = "trapper.friends";

        private bool Changed;
        [PluginReference] private Plugin Friends; private bool friendsEnabled;
        [PluginReference] private Plugin RustIO; private bool rustIOEnabled;
        [PluginReference] private Plugin ClansReborn; private bool clansRebornEnabled;
        private bool hurtFriends;
        private bool hurtOwner;
        private float resetTime = 5f;

        private void Init()
        {
            LoadVariables();
            permission.RegisterPermission(permissionName, this);
            permission.RegisterPermission(permissionNameOWNER, this);
            permission.RegisterPermission(permissionNameFRIENDS, this);
            permission.RegisterPermission(permissionNameADMIN, this);
        }

        protected override void LoadDefaultConfig()
        {
            Config.Clear();
            LoadVariables();
        }

        private void LoadVariables()
        {
            resetTime = Convert.ToSingle(GetConfig("Settings", "Reset Time", 5f));
            hurtOwner = Convert.ToBoolean(GetConfig("Settings", "Trigger for Owner", true));
            hurtFriends = Convert.ToBoolean(GetConfig("Settings", "Trigger for Friends", true));

            if (!Changed) return;
            SaveConfig();
            Changed = false;
        }

        private void OnServerInitialized()
        {
            if (hurtFriends)
            {
                rustIOEnabled = RustIO != null && RustIO.Call<bool>("IsInstalled");
                friendsEnabled = Friends != null;
                clansRebornEnabled = ClansReborn != null;
            }
        }

        private object OnTrapTrigger(BaseTrap trap, GameObject obj)
        {
            if (!(trap is BearTrap) && !(trap is Landmine)) return null;
            BasePlayer target = obj.GetComponent<BasePlayer>();
            if (target != null)
            {
                if (permission.UserHasPermission(target.UserIDString, permissionNameADMIN))
                    return false;
            }
            var player = FindPlayer(trap.OwnerID.ToString());;
            if (!hurtOwner || !hurtFriends)
            {
                if (target != null && player != null)
                {
                    if (!hurtOwner)
                        if (target == player)
                            if (permission.UserHasPermission(target.UserIDString, permissionNameOWNER))
                                return false;
                    if (!hurtFriends)
                    {
                        if (rustIOEnabled)
                            if ((bool)RustIO?.Call("HasFriend", target.UserIDString, player.UserIDString))
                                if (permission.UserHasPermission(player.UserIDString, permissionNameFRIENDS))
                                    return false;
                        if (friendsEnabled)
                            if ((bool)Friends?.CallHook("AreFriends", target.userID, player.userID))
                                if (permission.UserHasPermission(player.UserIDString, permissionNameFRIENDS))
                                    return false;
                    }
                }
            }

            if (!(trap is BearTrap)) return null;
            if (player == null) return null;
            if (permission.UserHasPermission(player.UserIDString, permissionName))
                timer.Once(resetTime, () =>
                {
                    if (trap != null)
                        ((BearTrap) trap).Arm();
                });
            return null;
        }

        private object GetConfig(string menu, string datavalue, object defaultValue)
        {
            var data = Config[menu] as Dictionary<string, object>;
            if (data == null)
            {
                data = new Dictionary<string, object>();
                Config[menu] = data;
                Changed = true;
            }
            object value;
            if (data.TryGetValue(datavalue, out value)) return value;
            value = defaultValue;
            data[datavalue] = value;
            Changed = true;
            return value;
        }

        private static BasePlayer FindPlayer(string nameOrId)
        {
            foreach (var activePlayer in BasePlayer.activePlayerList)
            {
                if (activePlayer.UserIDString == nameOrId)
                    return activePlayer;
                if (activePlayer.displayName.Contains(nameOrId, CompareOptions.OrdinalIgnoreCase))
                    return activePlayer;
            }
            foreach (var sleepingPlayer in BasePlayer.sleepingPlayerList)
            {
                if (sleepingPlayer.UserIDString == nameOrId)
                    return sleepingPlayer;
                if (sleepingPlayer.displayName.Contains(nameOrId, CompareOptions.OrdinalIgnoreCase))
                    return sleepingPlayer;
            }
            return null;
        }
    }
}