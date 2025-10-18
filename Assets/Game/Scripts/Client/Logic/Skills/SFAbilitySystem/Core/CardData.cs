using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
namespace SFAbilitySystem.Core
{
    [CreateAssetMenu(fileName = "Card", menuName = "SFAbilitySystem/Card", order = 1)]
    public class CardData : ScriptableObject
    {
        // Public accessors for ability data
        public AbilityBase[] abilities => _abilityContainer?.abilityTiers ?? Array.Empty<AbilityBase>();
        public int abilityTiersCount => abilities.Length;

        // Card metadata
        public string abilityName;
        public Sprite abilityIcon;
        [TextArea] public string abilityDescription;
        public CardData cardToUnlock;  // Unlock dependency

        [SerializeField]
        protected AbilityContainer _abilityContainer = new AbilityContainer();  // Actual ability storage

        // Returns ability of specific type and tier
        public T GetAbility<T>(int tier = 0) where T : AbilityBase
        {
            return (tier >= 0 && tier < abilities.Length) ? abilities[tier] as T : default;
        }

        // Finds index of specific ability
        public int IndexOf(AbilityBase ability) => Array.IndexOf(abilities, ability);

        // Generates dynamic description with variable substitution
        public string GetDescription(int level)
        {
            if (string.IsNullOrEmpty(abilityDescription)) return "No description available";
            if (level < 0 || level >= abilities.Length) return "Invalid level";
            if (abilities[level] == null) return "Ability not configured for this level";

            try
            {
                var result = abilityDescription;
                foreach (var variable in ExtractStringsInsideBraces(abilityDescription))
                {
                    var field = GetType().GetField(variable, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                             ?? abilities[level].GetType().GetField(variable, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    result = field != null
                        ? result.Replace("{" + variable + "}", field.GetValue(field.DeclaringType == GetType() ? this : abilities[level])?.ToString() ?? "null")
                        : result.Replace("{" + variable + "}", "N/A");
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[{name}] Description error for level {level}: {e.Message}", this);
                return "Error loading description";
            }
        }

        // Extracts {variable} placeholders from text
        public static string[] ExtractStringsInsideBraces(string input) =>
            string.IsNullOrEmpty(input)
                ? Array.Empty<string>()
                : new Regex(@"{([^}]+)}").Matches(input).Cast<Match>().Select(m => m.Groups[1].Value.Trim()).ToArray();

        private void OnValidate() => _abilityContainer ??= new AbilityContainer();  // Null check
    }
}