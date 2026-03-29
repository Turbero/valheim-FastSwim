using BepInEx;
using BepInEx.Configuration;
using ServerSync;
using System;
using System.IO;

namespace FastSwim
{
    internal class ConfigurationFile
    {
        private static ConfigEntry<bool> _serverConfigLocked = null;

        public static ConfigEntry<bool> debug;
        
        public static ConfigEntry<bool> baseSwimAttributesOverride;
        public static ConfigEntry<float> baseSwimDepth;
        public static ConfigEntry<float> baseSwimSpeed;
        public static ConfigEntry<float> baseSwimTurnSpeed;
        public static ConfigEntry<float> baseSwimAcceleration;
                
        public static ConfigEntry<bool> fastSwimAttributesActivate;
        public static ConfigEntry<float> fastSwimDepth;
        public static ConfigEntry<float> fastSwimSpeed;
        public static ConfigEntry<float> fastSwimTurnSpeed;
        public static ConfigEntry<float> fastSwimAcceleration;

        private static ConfigFile configFile;
        private static readonly string ConfigFileName = FastSwim.GUID + ".cfg";
        private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private static readonly ConfigSync ConfigSync = new ConfigSync(FastSwim.GUID)
        {
            DisplayName = FastSwim.NAME,
            CurrentVersion = FastSwim.VERSION,
            MinimumRequiredVersion = FastSwim.VERSION
        };

        internal static void LoadConfig(BaseUnityPlugin plugin)
        {
            {
                configFile = plugin.Config;

                _serverConfigLocked = config("1 - General", "Lock Configuration", true, "If on, the configuration is locked and can be changed by server admins only.");
                _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
                debug = config("1 - General", "DebugMode", false, "Enabling/Disabling the debugging in the console (default = false)", false);
                
                baseSwimAttributesOverride = config("2 - Base swim", "Override Base Swim", false, "If on, it will override the player swim base attributes with the 2.1 section values (default = false).");
                baseSwimDepth = config("2.1 - Base swim attributes", "Base Swim Depth", 1.5f, "Flat depth value to apply to the player when swimming normally (default = 1.5f).");
                baseSwimSpeed = config("2.1 - Base swim attributes", "Base Swim Speed", 2f, "Flat speed value to apply to the player when swimming normally (default = 2f).");
                baseSwimTurnSpeed = config("2.1 - Base swim attributes", "Base Swim Turn Speed", 100f, "Flat turn speed value to apply to the player when swimming normally (default = 2f).");
                baseSwimAcceleration = config("2.1 - Base swim attributes", "Base Swim Acceleration", 0.05f, "Flat acceleration value to apply to the player when swimming normally (default = 0.05f).");
                
                fastSwimAttributesActivate = config("3 - Fast swim", "Activate Fast Swim", true, "If on, it will allow the user to swim at fast speed with the 3.1 section values (default = true)");
                fastSwimDepth = config("3.1 - Fast swim attributes", "Fast Swim Depth", 1.5f, "Flat depth value to apply to the player when swimming fast (default = 1.5f).");
                fastSwimSpeed = config("3.1 - Fast swim attributes", "Fast Swim Speed", 4f, "Flat speed value to apply to the player when swimming fast (default = 4f).");
                fastSwimTurnSpeed = config("3.1 - Fast swim attributes", "Fast Swim Turn Speed", 200f, "Flat turn speed value to apply to the player when swimming fast (default = 200f).");
                fastSwimAcceleration = config("3.1 - Fast swim attributes", "Fast Swim Acceleration", 0.10f, "Flat acceleration value to apply to the player when swimming fast (default = 0.10f).");

                SetupWatcher();
            }
        }

        private static void SetupWatcher()
        {
            FileSystemWatcher watcher = new FileSystemWatcher(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private static void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Logger.Log("Attempting to reload configuration...");
                configFile.Reload();
                SettingsChanged(null, null);
                Logger.Log("Configuration reload complete.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"There was an issue loading {ConfigFileName}: " + ex);
            }
        }

        private static void SettingsChanged(object sender, EventArgs e)
        {
            if (Player.m_localPlayer != null)
            {
                SwimSpeedPatch.Prefix(Player.m_localPlayer);
            }
        }

        private static ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new ConfigDescription(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = configFile.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }
    }
}
