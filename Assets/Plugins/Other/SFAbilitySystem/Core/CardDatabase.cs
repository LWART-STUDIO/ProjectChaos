using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace SFAbilitySystem.Core
{
    /// <summary>
    /// Central repository for all card data in the ability system.
    /// Handles card storage, lookup, and procedural generation of available cards.
    /// Uses bit-packing for efficient ability identification in network/save scenarios.
    /// </summary>
    public class CardDatabase : ScriptableObject
    {
        [SerializeField] private CardData[] _cards;

        /// <summary>
        /// Creates a new CardDatabase asset in Resources folder.
        /// Menu item accessible via: SFAbilitySystem > Create Card Database
        /// </summary>
        [MenuItem("Tools/SFAbilitySystem/Create Card Database")]
        public static void CreateCardDatabase()
        {
            var db = ScriptableObject.CreateInstance<CardDatabase>();
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            AssetDatabase.CreateAsset(db, "Assets/Resources/CardDatabase.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = db;
        }

        /// <summary>
        /// Generates a compact 16-bit hash identifying a specific ability instance.
        /// Hash structure: [5 bits ability tier][11 bits card index].
        /// Returns 0 if ability not found in database.
        /// </summary>
        public short GetHashByAbility(AbilityBase ability)
        {
            var card = _cards.FirstOrDefault(s => s.abilities.Contains(ability));
            return card != null
                ? PackValues(Array.IndexOf(card.abilities, ability), Array.IndexOf(_cards, card))
                : (short)0;
        }

        /// <summary>
        /// Retrieves an ability using its packed 16-bit hash.
        /// Reverse operation of GetHashByAbility().
        /// Throws IndexOutOfRangeException for invalid hashes.
        /// </summary>
        public AbilityBase GetAbilityByHash(short hash)
        {
            UnpackValues(hash, out var abilityId, out var cardId);
            return _cards[cardId].abilities[abilityId];
        }

        /// <summary>
        /// Packs two integer values into a 16-bit short.
        /// Bit allocation: 5 bits (ability tier) + 11 bits (card index).
        /// Throws ArgumentOutOfRangeException if values exceed bit capacity.
        /// For Save/RPC
        /// </summary>
        public short PackValues(int abilityId, int cardId)
        {
            if (abilityId > 0x1F || cardId > 0x7FF)
                throw new ArgumentOutOfRangeException("Values exceed bit capacity");
            return (short)((abilityId << 11) | (cardId & 0x7FF));
        }

        /// <summary>
        /// Unpacks a 16-bit hash into its component parts.
        /// Outputs: ability tier (0-31), card index (0-2047).
        /// For Save/RPC
        /// </summary>
        public void UnpackValues(short hash, out int abilityId, out int cardId)
        {
            abilityId = (hash >> 11) & 0x1F;
            cardId = hash & 0x7FF;
        }

        /// <summary>
        /// Removes duplicate card references from the database.
        /// Should be called after manually editing the _cards array.
        /// </summary>
        public void CleanDuplicates() => _cards = _cards.Distinct().ToArray();

        /// <summary>
        /// Returns random cards meeting availability requirements:
        /// 1. Unlock conditions satisfied (if any)
        /// 2. Not at maximum ability tier
        /// Preserves existing card levels from availableCards parameter.
        /// </summary>
        public CardEntry[] GetRandomCards(CardEntry[] availableCards, int count = 5)
        {
            var validCards = _cards.Where(c => CardAvailable(c, availableCards)).ToList();
            var result = new List<CardEntry>();

            while (result.Count < count && validCards.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, validCards.Count);
                var card = validCards[index];
                var existing = availableCards.FirstOrDefault(c => c.card == card);
                result.Add(new CardEntry(card, existing?.level ?? -1));
                validCards.RemoveAt(index);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Returns all cards meeting availability requirements.
        /// Includes both unlocked cards and cards with remaining upgrade tiers.
        /// </summary>
        public CardEntry[] GetAllAvailableCards(CardEntry[] availableCards) =>
            _cards.Where(c => CardAvailable(c, availableCards))
                .Select(c => new CardEntry(c, availableCards.FirstOrDefault(a => a.card == c)?.level ?? -1))
                .ToArray();

        /// <summary>
        /// Determines card availability based on:
        /// 1. Unlock requirements (null or present in availableCards)
        /// 2. Upgrade potential (not at max tier)
        /// </summary>
        private bool CardAvailable(CardData card, CardEntry[] availableCards) =>
            (card.cardToUnlock == null || availableCards.Any(a => a.card == card.cardToUnlock)) &&
            !availableCards.Any(a => a.card == card && a.level >= card.abilityTiersCount - 1);

        /// <summary>
        /// Gets the array index of a specific card.
        /// Returns -1 if card not found in database.
        /// </summary>
        public int IndexOf(CardData card) => Array.IndexOf(_cards, card);

        /// <summary>
        /// Retrieves card by its database index.
        /// Use IndexOf() to get valid indices.
        /// </summary>
        public CardData CardAt(int index) => _cards[index];
    }
}