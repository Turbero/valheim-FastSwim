using BepInEx;
using HarmonyLib;

namespace FastSwim
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class FastSwim : BaseUnityPlugin
    {
        public const string GUID = "Turbero.FastSwim";
        public const string NAME = "Fast Swim";
        public const string VERSION = "1.0.0";

        private readonly Harmony harmony = new Harmony(GUID);

        void Awake()
        {
            ConfigurationFile.LoadConfig(this);

            harmony.PatchAll();
        }

        void onDestroy()
        {
            harmony.UnpatchSelf();
        }
    }
}
