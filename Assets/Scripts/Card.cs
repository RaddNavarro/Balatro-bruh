using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamPassione
{
    [CreateAssetMenu(fileName ="New Card", menuName = "Card")]
    public class Card : ScriptableObject
    {
        public string cardName;
        public CardType cardType;
        public int DMG;


        public enum CardType
        {
            Hearts,
            Diamonds,
            Spades,
            Clubs
        }
    }
}
