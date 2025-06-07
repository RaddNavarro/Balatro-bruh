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

    private List<CardAttributes> playedCardsBot;
    private int playerPoints = 0;
    private int botPoints = 0;
    private int scale = 3;

    private enum RANKS
    {
        HighCard,
        OnePair,
        TwoPair,
        ThreeKind,
        Straight,
        Flush,
        FullHouse,
        FourKind,
        StraightFlush,
        RoyalFlush,

    }

    private enum SUITS
    {
        Hearts,
        Diamonds,
        Spades,
        Clubs
    }



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
            Select(playedCards[index], playedCards[index].selected);
            index++;
            yield return StartCoroutine(PlaySequence(index));

        }



    }




    private void BattleCards(CardAttributes botCard, CardAttributes playerCard)
    {
        if (botCard.DMG > playerCard.DMG)
        {
            botPoints += 1;
            Debug.Log(botPoints + " : " + playerPoints);
        }
        else if (botCard.DMG < playerCard.DMG)
        {
            playerPoints += 1;
            Debug.Log(botPoints + " : " + playerPoints);

        }
        else
        {
            Debug.Log("Draw" + " " + botCard.DMG + " " + playerCard.DMG);
            Debug.Log("Draw" + " " + botPoints + " : " + playerPoints);
        }


    }

    IEnumerator DestroyCards()
    {
        yield return PlaySequence(0);

        int i = 0;
        while (i < playedCards.Count)
        {

            Destroy(playedCards[i].transform.parent.gameObject);

            Debug.Log(playedCards.Count);
            i++;
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("done playing");
        if (playerPoints > botPoints)
        {
            scale += 1;
            Debug.Log(scale);
            playerPoints = 0;
            botPoints = 0;
        }
        else
        {
            scale -= 1;
            Debug.Log(scale);
            playerPoints = 0;
            botPoints = 0;
        }
        playedCards.Clear();
        PlayingCardHolder.DrawHand();
    }

    private void Select(Card card, bool state)
    {
        card.SelectEvent.Invoke(card, card.selected);
    }







}