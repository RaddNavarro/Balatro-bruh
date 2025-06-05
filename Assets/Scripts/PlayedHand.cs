using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayedHand : MonoBehaviour
{
    [SerializeField] private PlayingCardHolder PlayingCardHolder;
    [SerializeField] private Cards selectedCard;
    [SerializeField] private GameObject slotPrefab;

    [SerializeField] private List<Cards> playedCards = new List<Cards>();
    public List<Cards> cards;
    bool isCrossing = false;
    public void MoveCardsToPlayedHand()
    {
        foreach (Cards card in PlayingCardHolder.selectedCards)
        {
            playedCards.Add(card);
            MoveCardsToPlayedArea(card);
        }
    }

    private void MoveCardsToPlayedArea(Cards card)
    {
        card.selected = false;
        card.isPlayed = true;
        card.transform.parent.SetParent(this.transform, false);

        card.transform.localPosition = Vector3.zero;


    }

}