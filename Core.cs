using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
#if IL2CPP
using Il2Cpp;
#endif

[assembly: MelonInfo(typeof(ClickCounter.Core), "ClickCounter", "1.0.0", "Fibes", null)]
[assembly: MelonGame("PillowCastle", "Superliminal")]
[assembly: MelonGame("PillowCastle", "SuperliminalSteam")]
[assembly: MelonGame("PillowCastle", "SuperliminalGOG")]

namespace ClickCounter
{
    public class Core : MelonMod
    {
        public static int counter = 0;
        public static int restart_level_stored = 0;
        public static int reset_checkpoint_stored = 0;
        public static Font bebasneue_font = null;

        private MelonPreferences_Category ClickCounter;
        public static MelonPreferences_Entry<bool> segmented;
        private static MelonPreferences_Entry<TextAnchor> alignment;

        public override void OnInitializeMelon()
        {
            ClickCounter = MelonPreferences.CreateCategory("ClickCounter");
            segmented = ClickCounter.CreateEntry<bool>("segmented", true);
            alignment = ClickCounter.CreateEntry<TextAnchor>("alignment", TextAnchor.UpperLeft);
        }

        public override void OnLateInitializeMelon()
        {
            foreach (Font font in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (font.name == "BebasNeue Bold")
                {
                    bebasneue_font = font;
                }
            }
        }

        public override void OnUpdate()
        {
            if (GameManager.GM.player == null)
                return;

            if (Time.timeScale > 0 && Input.GetButtonDown("Grab"))
            {
                counter++;
            }
        }

        public static void DrawText()
        {
            GUI.skin.label.font = bebasneue_font;
            GUI.skin.label.alignment = Core.alignment.Value;
            GUI.Label(new Rect(20, 20, Screen.width - 40, Screen.height - 40), "<size=40>" + counter.ToString() + "</size>");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "StartScreen_Live")
            {
                counter = 0;
                restart_level_stored = 0;
                reset_checkpoint_stored = 0;
            }

            if (GameManager.GM.player == null)
                MelonEvents.OnGUI.Unsubscribe(DrawText);
            else
                MelonEvents.OnGUI.Subscribe(DrawText, 100);
        }
    }

#if LEGACY
    [HarmonyPatch(typeof(LevelInformation), "GetNextLevelScenePath")]
#else
    [HarmonyPatch(typeof(LevelInfo), "GetNextLevelScenePath")]
#endif
    public static class GetNextLevelScenePathPatch
    {
        private static void Prefix()
        {
            if (Core.segmented.Value)
            {
                Core.restart_level_stored = Core.counter;
                Core.reset_checkpoint_stored = Core.counter;
            }
        }
    }

    [HarmonyPatch(typeof(SaveAndCheckpointManager), "RestartLevel")]
    public static class RestartLevelPatch
    {
        private static void Prefix()
        {
            if (Core.segmented.Value)
            {
                Core.counter = Core.restart_level_stored;
                Core.reset_checkpoint_stored = Core.counter;
            }
            else if (SceneManager.GetActiveScene().name == "TestChamber_Live")
            {
                Core.counter = 0;
            }
        }
    }

    [HarmonyPatch(typeof(SaveAndCheckpointManager), "EnteredCheckpoint")]
    public static class EnteredCheckpointPatch
    {
        private static void Prefix()
        {
            if (Core.segmented.Value)
            {
                Core.reset_checkpoint_stored = Core.counter;
            }
        }
    }

    [HarmonyPatch(typeof(SaveAndCheckpointManager), "ResetToLastCheckpoint")]
    public static class ResetToLastCheckpointPatch
    {
        private static void Prefix()
        {
            if (Core.segmented.Value)
            {
                Core.counter = Core.reset_checkpoint_stored;
            }
        }
    }
}
