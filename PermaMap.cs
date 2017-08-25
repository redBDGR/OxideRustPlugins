using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("PermaMap", "redBDGR", "1.0.6", ResourceId = 2557)]
    [Description("Make sure that players always have access to a map")]
    internal class PermaMap : RustPlugin
    {
        private const string permissionName = "permamap.use";

        private void Init()
        {
            permission.RegisterPermission(permissionName, this);

            lang.RegisterMessages(new Dictionary<string, string>
            {
                //chat
                ["Unable to craft"] = "You already have a map hidden in your inventory! press your map button to use it"
            }, this);
        }

        private void OnPlayerRespawned(BasePlayer player)
        {
            AddMap(player);
        }

        private void OnPlayerInit(BasePlayer player)
        {
            timer.Once(5f, () =>
            {
                if (player.IsSleeping())
                {
                    OnPlayerInit(player);
                    return;
                }
                AddMap(player);
            });
        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (!(entity is BasePlayer)) return;
            var player = (BasePlayer) entity;
            RemoveMap(player);
        }

        private void JoinedEvent(BasePlayer player)
        {
            timer.Once(5f, () =>
            {
                if (player.IsSleeping())
                    JoinedEvent(player);
                else
                    RemoveMap(player);
            });
        }

        private void LeftEvent(BasePlayer player)
        {
            AddMap(player);
        }

        private object CanCraft(ItemCrafter itemCrafter, ItemBlueprint bp, int amount)
        {
            if (bp.name != "map.item") return null;
            var player = itemCrafter.containers[0].GetOwnerPlayer();
            if (player == null)
                return false;
            if (!permission.UserHasPermission(player.UserIDString, permissionName))
                return null;
            player.ChatMessage(msg("Unable to craft", player.UserIDString));
            return false;
        }

        private static void RemoveMap(BasePlayer player)
        {
            var item = player.inventory.containerBelt.GetSlot(6);
            if (item == null)
                return;
            item.RemoveFromContainer();
            item.Remove(0.1f);
        }

        private void AddMap(BasePlayer player)
        {
            if (!permission.UserHasPermission(player.UserIDString, permissionName))
                return;
            player.inventory.containerBelt.capacity = 7;
            if (player.inventory.containerBelt.GetSlot(6) != null)
                return;
            var item = ItemManager.CreateByItemID(107868, 1);
            item.MoveToContainer(player.inventory.containerBelt, 6);
        }

        private string msg(string key, string id = null)
        {
            return lang.GetMessage(key, this, id);
        }
    }
}