using Rust;

namespace Oxide.Plugins
{
    [Info("NoFallDamage", "redBDGR", "1.0.1")]
    [Description("Removes fall damage")]
    internal class NoFallDamage : RustPlugin
    {
        private void Init()
        {
            permission.RegisterPermission("nofalldamage.use", this);
        }

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (info.damageTypes.GetMajorityDamageType() != DamageType.Fall) return;
            if (permission.UserHasPermission(entity.ToPlayer()?.UserIDString, "nofalldamage.use"))
                info.damageTypes = new DamageTypeList();
        }
    }
}