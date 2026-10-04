using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ModDialogos
{
    [BepInPlugin("com.community.infinitelives.translation", "Infinite Lives Community Translation Mod", "1.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;

        // Dictionary with case-insensitive search
        public static Dictionary<string, string> TranslationDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // List of dialogue fragments for intelligent replacement of sentences containing NPC names
        public static List<KeyValuePair<string, string>> DialogueFragments = new List<KeyValuePair<string, string>>();

        private void Awake()
        {
            Log = Logger;

            // Load the external .txt translation file
            LoadTranslationFile();

            var harmony = new Harmony("com.community.infinitelives.translation");
            harmony.PatchAll();

            // Safe patch for TextMeshProUGUI via Reflection (to avoid missing assembly errors)
            try
            {
                var tmproType = AccessTools.TypeByName("TMPro.TextMeshProUGUI");
                if (tmproType != null)
                {
                    var original = AccessTools.PropertySetter(tmproType, "text");
                    string dummy = "";
                    var prefix = SymbolExtensions.GetMethodInfo(() => TMProPrefix(ref dummy));
                    harmony.Patch(original, new HarmonyMethod(prefix));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Warning when initializing TextMeshPro: " + ex.Message);
            }

            Logger.LogInfo($"[Translation Mod] Ready! {TranslationDictionary.Count} entries loaded from the translation file.");
        }

        private static string GetTranslationFilePath()
        {
            // The file must always be loaded from the same folder where the mod's DLL is located
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            return Path.Combine(assemblyDir, "traducao_infinitelives.txt");
        }

        public static void LoadTranslationFile()
        {
            string path = GetTranslationFilePath();
            if (!File.Exists(path))
            {
                Log.LogWarning($"[Translation Mod] Translation file not found! Please place 'traducao_infinitelives.txt' inside the mod's DLL folder: {path}");
                return;
            }

            Log.LogInfo($"[Translation Mod] Loading translations from: {path}");

            TranslationDictionary.Clear();
            var tempFragments = new List<KeyValuePair<string, string>>();

            try
            {
                string[] lines = File.ReadAllLines(path, Encoding.UTF8);
                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();
                    // Ignore empty lines or comments starting with # or //
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#") || trimmedLine.StartsWith("//") || !trimmedLine.Contains("="))
                        continue;

                    string[] parts = trimmedLine.Split(new char[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        string original = parts[0].Trim();
                        string translation = parts[1].Trim();

                        if (string.IsNullOrEmpty(translation)) continue;

                        // Save to the main dictionary. We use index assignment instead of .Add() 
                        // to prevent crashes if there are duplicate keys (e.g., 'Storyboard=' and 'STORYBOARD=')
                        if (!TranslationDictionary.ContainsKey(original))
                        {
                            TranslationDictionary[original] = translation;
                        }
                        else
                        {
                            // If the key already exists, overwrite it with the newest one (or choose to ignore)
                            TranslationDictionary[original] = translation;
                        }

                        // Only sentences with spaces or ending in punctuation count as dialogue fragments.
                        // (This prevents words like 'PLAY' from corrupting words like 'DISPLAY')
                        if (original.Length >= 4 && (original.Contains(" ") || original.EndsWith(",") || original.EndsWith(":") || original.EndsWith("!")))
                        {
                            tempFragments.Add(new KeyValuePair<string, string>(original, translation));
                        }
                    }
                }

                // Sort fragments from largest to smallest to substitute larger expressions first
                tempFragments.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));
                DialogueFragments = tempFragments;
            }
            catch (Exception ex)
            {
                Log.LogError($"[Translation Mod] Error reading translation file: {ex.Message}");
            }
        }

        // Intelligently preserves uppercase/lowercase (e.g., EXIT becomes SAIR, Exit becomes Sair)
        private static string PreserveCase(string original, string translation)
        {
            if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(translation))
                return translation;

            bool isAllCaps = true;
            bool hasLetters = false;

            foreach (char c in original)
            {
                if (char.IsLetter(c))
                {
                    hasLetters = true;
                    if (!char.IsUpper(c))
                    {
                        isAllCaps = false;
                        break;
                    }
                }
            }

            if (hasLetters && isAllCaps)
            {
                return translation.ToUpperInvariant();
            }

            return translation;
        }

        public static string TranslateText(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            string cleanValue = value.Trim();

            // 1. Exact match (Case-Insensitive) with uppercase preservation
            if (TranslationDictionary.TryGetValue(cleanValue, out string exactTranslation))
            {
                string adjustedTranslation = PreserveCase(cleanValue, exactTranslation);
                if (value.Length != cleanValue.Length)
                {
                    int start = value.IndexOf(cleanValue, StringComparison.Ordinal);
                    return value.Substring(0, start) + adjustedTranslation + value.Substring(start + cleanValue.Length);
                }
                return adjustedTranslation;
            }

            // 2. Safe substitution for composite dialogue lines (e.g., "What's your problem, " + NPCName)
            string result = value;
            foreach (var pair in DialogueFragments)
            {
                int index = result.IndexOf(pair.Key, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    string matchedPart = result.Substring(index, pair.Key.Length);
                    string replacement = PreserveCase(matchedPart, pair.Value);
                    result = result.Substring(0, index) + replacement + result.Substring(index + pair.Key.Length);
                }
            }

            return result;
        }

        public static void TMProPrefix(ref string value)
        {
            if (value != null)
                value = TranslateText(value);
        }
    }

    [HarmonyPatch(typeof(Text), "text", MethodType.Setter)]
    public class TextUI_Patch
    {
        static void Prefix(ref string value)
        {
            if (value != null)
                value = Plugin.TranslateText(value);
        }
    }
}
