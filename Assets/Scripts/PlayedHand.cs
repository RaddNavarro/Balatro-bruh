using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TeamPassione;
using TMPro;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

public class PlayedHand : MonoBehaviour
{
    [SerializeField] private PlayingCardHolder PlayingCardHolder;


    public void MoveCardsToPlayedArea()
    {
        foreach (Card card in PlayingCardHolder.selectedCards)
        {
            card.selected = false;
            card.isPlayed = true;
            card.transform.parent.SetParent(this.transform, false);
            card.transform.localPosition = Vector3.zero;

        }
    }


}