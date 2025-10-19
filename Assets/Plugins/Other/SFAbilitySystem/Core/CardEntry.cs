using System;

namespace SFAbilitySystem.Core
{
    /// <summary>
    /// Represents an instance of a card with its current progression level.
    /// Used for tracking player's card collection and upgrade states.
    /// </summary>
    [Serializable]
    public class CardEntry
    {
        /// <summary>
        /// Reference to the card's static data (abilities, metadata, etc.)
        /// </summary>
        public CardData card;

        /// <summary>
        /// Current upgrade tier of the card (0-based index).
        /// Value of -1 indicates the card is unlocked but not yet upgraded.
        /// </summary>
        public int level;

        /// <summary>
        /// Creates a new card entry with specified data and level
        /// </summary>
        /// <param name="cardData">The card definition from CardDatabase</param>
        /// <param name="level">
        /// Current upgrade tier:
        /// -1 = Unlocked but not upgraded
        /// 0+ = Upgrade tier index
        /// </param>
        public CardEntry(CardData cardData, int level)
        {
            this.card = cardData;
            this.level = level;
        }
    }
}