using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TeamPassione;
using UnityEngine;

public class PlayedHand : MonoBehaviour
{
    [SerializeField] private PlayingCardHolder PlayingCardHolder;
    [SerializeField] private Card selectedCard;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private List<Card> playedCards;
    [SerializeField] public List<CardAttributes> botDeck;
    [SerializeField] private CardVisual cardVisual;

    private List<CardAttributes> playedCardsBot;

    private void Start()
    {
        Shuffle(botDeck);


        foreach (Card card in playedCards)
        {
            card.SelectEvent.AddListener(Select);
        }
    }

    void Shuffle(List<CardAttributes> deck)
    {

        for (int i = deck.Count - 1; i > 0; i--)
        {

            int rnd = UnityEngine.Random.Range(0, i);


            CardAttributes temp = deck[i];

            deck[i] = deck[rnd];
            deck[rnd] = temp;
        }


    }

    public void MoveCardsToPlayedHand()
    {

        playedCards = new List<Card>();

        foreach (Card card in PlayingCardHolder.selectedCards)
        {
            playedCards.Add(card);
            MoveCardsToPlayedArea(card);
        }

        PlayingCardHolder.selectedCards.Clear();
        BotPlay();


        // StartCoroutine(PlaySequence(0));
        StartCoroutine(DestroyCards());
    }

    private void MoveCardsToPlayedArea(Card card)
    {
        card.selected = false;
        card.isPlayed = true;
        card.transform.parent.SetParent(this.transform, false);

        card.transform.localPosition = Vector3.zero;
        // ReadCard(card);

    }

    private void BotPlay()
    {
        playedCardsBot = new List<CardAttributes>();
        for (int i = 0; i < playedCards.Count; i++)
        {
            int botDeckLength = botDeck.Count;
            int randomCard = UnityEngine.Random.Range(0, botDeckLength - 1);
            if (!playedCardsBot.Contains(botDeck[randomCard]))
            {
                playedCardsBot.Add(botDeck[randomCard]);
                botDeck.RemoveAt(randomCard);

            }
        }
    }

    public IEnumerator PlaySequence(int index)
    {
        yield return new WaitForSeconds(0.8f);
        if (index < playedCards.Count)
        {

            BattleCards(playedCardsBot[index], playedCards[index].cardType);
            Debug.Log("played card at" + " " + index);

            index++;
            yield return StartCoroutine(PlaySequence(index));

        }



    }




    private void BattleCards(CardAttributes botCard, CardAttributes playerCard)
    {
        if (botCard.DMG > playerCard.DMG)
        {
            Debug.Log("Bot Wins" + " " + botCard.DMG + " " + playerCard.DMG);
        }
        else if (botCard.DMG < playerCard.DMG)
        {
            Debug.Log("Player Wins" + " " + botCard.DMG + " " + playerCard.DMG);
        }
        else
        {
            Debug.Log("Draw" + " " + botCard.DMG + " " + playerCard.DMG);
        }


    }

    IEnumerator DestroyCards()
    {
        yield return PlaySequence(0);

        int i = 0;
        // int index = 0;
        Debug.Log(playedCards.Count);
        while (i < playedCards.Count)
        {

            Destroy(playedCards[i].transform.parent.gameObject);

            Debug.Log("ran the destroy" + i);
            Debug.Log(playedCards.Count);
            i++;
            yield return new WaitForSeconds(0.5f);
        }
        // for (int i = 0; i < playedCards.Count; i++)
        // {
        // }
        Debug.Log("done playing");
        playedCards.Clear();
    }

    private void Select(Card card, bool state)
    {
        card.SelectEvent.Invoke(card, card.selected);
    }




}