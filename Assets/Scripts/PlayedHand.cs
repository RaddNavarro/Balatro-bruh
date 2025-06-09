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
    [SerializeField] private Card selectedCard;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private List<Card> playedCards;
    [SerializeField] public List<CardAttributes> botDeck;
    [SerializeField] private TextMeshProUGUI pokerHandText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI botHandText;

    // Button cooldown variables
    [SerializeField] private Button playHandButton; // Assign this in the inspector
    [SerializeField] private Button discardButton; // NEW: Assign this in the inspector
    private bool isButtonOnCooldown = false;

    private List<CardAttributes> playedCardsBot;
    // private int playerPoints = 0;
    // private int botPoints = 0;
    private int playerTotalScore = 0;
    private int botTotalScore = 0;
    private int playerBaseDmgScore = 0;
    private int botBaseDmgScore = 0;

    public enum RANKS
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

        // Initialize UI text
        scoreText.text = "Round Points - Player: " + playerBaseDmgScore + " Bot: " + botBaseDmgScore;
        totalScoreText.text = "Total Points - Player: " + playerTotalScore + " Bot: " + botTotalScore;
        if (pokerHandText != null)
            pokerHandText.text = "Player: Select cards to play";
        if (botHandText != null)
            botHandText.text = "Bot: Waiting...";

        // NEW: Setup discard button
        if (discardButton != null)
        {
            discardButton.onClick.AddListener(DiscardSelectedCards);
        }
    }

    private void Update()
    {
        if (isButtonOnCooldown && playHandButton != null)
        {
            playHandButton.interactable = false;
        }
        else
        {
            playHandButton.interactable = true;
        }

        // NEW: Update discard button interactability
        if (discardButton != null)
        {
            // Only allow discarding when not in cooldown and there are selected cards
            discardButton.interactable = !isButtonOnCooldown && PlayingCardHolder.selectedCards.Count > 0;
        }
    }

    // NEW: Discard selected cards functionality
    public void DiscardSelectedCards()
    {
        if (isButtonOnCooldown || PlayingCardHolder.selectedCards.Count == 0)
        {
            Debug.Log("Cannot discard: either on cooldown or no cards selected");
            return;
        }

        Debug.Log("Discarding " + PlayingCardHolder.selectedCards.Count + " selected cards");

        // Start the discard coroutine to handle the process properly
        StartCoroutine(DiscardCardsCoroutine());
    }

    // NEW: Coroutine to properly handle card discarding
    private IEnumerator DiscardCardsCoroutine()
    {
        // Create a copy of the selected cards list to avoid modification during iteration
        List<Card> cardsToDiscard = new List<Card>(PlayingCardHolder.selectedCards);

        // Clear the selected cards list first
        PlayingCardHolder.selectedCards.Clear();

        // Destroy the discarded cards
        foreach (Card card in cardsToDiscard)
        {
            if (card != null && card.gameObject != null)
            {
                // If the card has a parent (like a slot), destroy the parent
                if (card.transform.parent != null)
                {
                    Destroy(card.transform.parent.gameObject);
                }
                else
                {
                    Destroy(card.gameObject);
                }
            }
        }

        Debug.Log("Cards discarded successfully");

        // Wait for the objects to be actually destroyed
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.2f);

        // Clear any remaining references and force a fresh draw
        if (PlayingCardHolder.selectedCards == null)
        {
            PlayingCardHolder.selectedCards = new List<Card>();
        }

        // Try to clear the hand first if there's a method for it
        // Then draw new hand
        Debug.Log("Attempting to draw new hand...");
        PlayingCardHolder.DrawHand();

        // Wait a bit more and try again if the first attempt didn't work
        yield return new WaitForSeconds(0.1f);
        if (PlayingCardHolder.selectedCards.Count == 0)
        {
            Debug.Log("First draw attempt may have failed, trying again...");
            PlayingCardHolder.DrawHand();
        }

        Debug.Log("New cards drawn to replace discarded cards");
    }



    public void BattleSequence()
    {
        isButtonOnCooldown = true;
        playedCards = new List<Card>();

        foreach (Card card in PlayingCardHolder.selectedCards)
        {
            card.selected = false;
            card.isPlayed = true;
            card.transform.parent.SetParent(this.transform, false);
            card.transform.localPosition = Vector3.zero;

        }
    }


}