using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TeamPassione
{
    [CreateAssetMenu(fileName = "New Card", menuName = "Card")]
    public class CardAttributes : ScriptableObject
    {
        public string cardName;
        public CardType cardType;
        public int DMG;

        public Sprite sprite;


        public enum CardType
        {
            Hearts,
            Diamonds,
            Spades,
            Clubs
        }
    }
}
