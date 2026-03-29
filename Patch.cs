using System.Collections.Generic;
using HarmonyLib;

namespace FastSwim
{
    public static class SwimmingValues
    {
        public static Dictionary<Player, bool> sprintStates = new();
        
        public static float vanillaSwimDepth;
        public static float vanillaSwimSpeed;
        public static float vanillaSwimTurnSpeed;
        public static float vanillaSwimAcceleration;

        public static void LogPlayerSwimValues(Player player, string prefix)
        {
            Logger.Log($"Player {prefix}| m_swimDepth: {player.m_swimDepth}");
            Logger.Log($"Player {prefix}| m_swimSpeed: {player.m_swimSpeed}");
            Logger.Log($"Player {prefix}| m_swimTurnSpeed: {player.m_swimTurnSpeed}");
            Logger.Log($"Player {prefix}| m_swimAcceleration: {player.m_swimAcceleration}");
        }
    }
    
    [HarmonyPatch(typeof (Player), "Awake")]
    public static class Player_Awake_Patch
    {
        private static bool initialized = false;
        public static void Postfix(Player __instance)
        {
            if (initialized) return;
            
            Logger.Log("Storing vanilla initial swimming values");
            SwimmingValues.LogPlayerSwimValues(__instance, "vanilla");
            SwimmingValues.vanillaSwimDepth = __instance.m_swimDepth;
            SwimmingValues.vanillaSwimSpeed = __instance.m_swimSpeed;
            SwimmingValues.vanillaSwimTurnSpeed = __instance.m_swimTurnSpeed;
            SwimmingValues.vanillaSwimAcceleration = __instance.m_swimAcceleration;

            initialized = true;
        }
    }

    [HarmonyPatch(typeof(Player), "Update")]
    public class SwimSpeedPatch
    {
        private static readonly Dictionary<Player, bool> sprintStates = new();

        public static void Prefix(Player __instance)
        {
            if (!__instance.IsSwimming()) return;

            bool isSprinting = ZInput.GetButton("Run") || ZInput.GetButton("JoyRun");
            
            if (!sprintStates.TryGetValue(__instance, out bool wasSprinting))
            {
                wasSprinting = false;
            }

            if (isSprinting && !wasSprinting && ConfigurationFile.fastSwimAttributesActivate.Value)
            {
                Logger.Log("Updating swimming stats");
                SwimmingValues.LogPlayerSwimValues(__instance, "before");
                __instance.m_swimDepth = ConfigurationFile.fastSwimDepth.Value;
                __instance.m_swimSpeed = ConfigurationFile.fastSwimSpeed.Value;
                __instance.m_swimTurnSpeed = ConfigurationFile.fastSwimTurnSpeed.Value;
                __instance.m_swimAcceleration = ConfigurationFile.fastSwimAcceleration.Value;
                SwimmingValues.LogPlayerSwimValues(__instance, "after");
            }
            else if (!isSprinting && wasSprinting)
            {
                Logger.Log("Restoring swimming stats");
                SwimmingValues.LogPlayerSwimValues(__instance, "before");
                if (ConfigurationFile.baseSwimAttributesOverride.Value)
                {
                    __instance.m_swimDepth = ConfigurationFile.baseSwimDepth.Value;
                    __instance.m_swimSpeed = ConfigurationFile.baseSwimSpeed.Value;
                    __instance.m_swimTurnSpeed = ConfigurationFile.baseSwimTurnSpeed.Value;
                    __instance.m_swimAcceleration = ConfigurationFile.baseSwimAcceleration.Value;
                }
                else
                {
                    __instance.m_swimDepth = SwimmingValues.vanillaSwimDepth;
                    __instance.m_swimSpeed = SwimmingValues.vanillaSwimSpeed;
                    __instance.m_swimTurnSpeed = SwimmingValues.vanillaSwimTurnSpeed;
                    __instance.m_swimAcceleration = SwimmingValues.vanillaSwimAcceleration;
                }
                SwimmingValues.LogPlayerSwimValues(__instance, "after");
            }

            sprintStates[__instance] = isSprinting;
        }
        
        public static void RemovePlayer(Player player)
        {
            sprintStates.Remove(player);
        }
    }
    
    [HarmonyPatch(typeof(Player), "OnDestroy")]
    public static class Player_Destroy_Patch
    {
        public static void Prefix(Player __instance)
        {
            Logger.Log("Cleaning sprint state for player");
            SwimSpeedPatch.RemovePlayer(__instance);
        }
    }
}