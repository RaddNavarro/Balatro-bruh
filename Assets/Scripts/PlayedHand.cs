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
            playedCards.Add(card);
            MoveCardsToPlayedArea(card);
        }

        PlayingCardHolder.selectedCards.Clear();

        BotPlay();
        StartCoroutine(DestroyCards());
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

    private void CheckPokerHand(CardAttributes card, RANKS handRank)
    {

        float multiplier = GetHandMultiplier(handRank);
        playerBaseDmgScore += card.DMG;
        UpdatePokerHandUI(handRank, multiplier);
    }

    private void UpdatePokerHandUI(RANKS handRank, float multiplier)
    {
        if (pokerHandText != null)
        {
            string handName = GetHandDisplayName(handRank);
            pokerHandText.text = "Player: " + handName + " (" + multiplier + "x)";

            // Add color coding based on hand strength
            Color handColor = GetHandColor(handRank);
            pokerHandText.color = handColor;
        }
    }

    private void CheckBotPokerHand(CardAttributes botCard, RANKS handRankBot)
    {

        float multiplier = GetHandMultiplier(handRankBot);

        botBaseDmgScore += botCard.DMG;
        UpdateBotHandUI(handRankBot, multiplier);
    }

    private void UpdateBotHandUI(RANKS handRank, float multiplier)
    {
        if (botHandText != null)
        {
            string handName = GetHandDisplayName(handRank);
            botHandText.text = "Bot: " + handName + " (" + multiplier + "x)";

            // Add color coding based on hand strength
            Color handColor = GetHandColor(handRank);
            botHandText.color = handColor;
        }
    }

    private string GetHandDisplayName(RANKS handRank)
    {
        switch (handRank)
        {
            case RANKS.HighCard: return "High Card";
            case RANKS.OnePair: return "One Pair";
            case RANKS.TwoPair: return "Two Pair";
            case RANKS.ThreeKind: return "Three of a Kind";
            case RANKS.Straight: return "Straight";
            case RANKS.Flush: return "Flush";
            case RANKS.FullHouse: return "Full House";
            case RANKS.FourKind: return "Four of a Kind";
            case RANKS.StraightFlush: return "Straight Flush";
            case RANKS.RoyalFlush: return "Royal Flush";
            default: return "Unknown Hand";
        }
    }

    private Color GetHandColor(RANKS handRank)
    {
        switch (handRank)
        {
            case RANKS.HighCard: return Color.white;
            case RANKS.OnePair: return Color.gray;
            case RANKS.TwoPair: return Color.cyan;
            case RANKS.ThreeKind: return Color.green;
            case RANKS.Straight: return Color.blue;
            case RANKS.Flush: return Color.magenta;
            case RANKS.FullHouse: return Color.yellow;
            case RANKS.FourKind: return new Color(1f, 0.5f, 0f); // Orange
            case RANKS.StraightFlush: return Color.red;
            case RANKS.RoyalFlush: return new Color(1f, 0.84f, 0f); // Gold
            default: return Color.white;
        }
    }

    private void UpdateScore()
    {

        if (scoreText != null)
        {
            scoreText.text = "Round Points - Player: " + playerBaseDmgScore + " Bot: " + botBaseDmgScore;

        }
        else
        {
            Debug.LogWarning("Score Text is null! Make sure it's assigned in the inspector.");
        }
    }

    private void UpdateTotalScore(float multiplierPlayer, float multiplierBot)
    {
        int playerScore = (int)(Math.Round((float)(playerBaseDmgScore * multiplierPlayer)));
        int botScore = (int)(Math.Round((float)(botBaseDmgScore * multiplierBot)));

        playerTotalScore += playerScore;
        botTotalScore += botScore;

        if (totalScoreText != null)
        {
            totalScoreText.text = "Round Points - Player: " + playerTotalScore + " Bot: " + botTotalScore;

        }
        else
        {
            Debug.LogWarning("Score Text is null! Make sure it's assigned in the inspector.");
        }
    }

    private RANKS EvaluatePokerHand(List<CardAttributes> cards)
    {
        if (cards.Count < 2) return RANKS.HighCard;

        // Extract card values and suits from card names
        Dictionary<int, int> valueCounts = new Dictionary<int, int>();
        Dictionary<string, int> suitCounts = new Dictionary<string, int>();

        foreach (CardAttributes card in cards)
        {
            // Parse card name to get value and suit
            string cardName = card.name.ToUpper();
            int value = GetCardValue(cardName);
            string suit = GetCardSuit(cardName);

            Debug.Log("Card: " + cardName + " -> Value: " + value + ", Suit: " + suit);

            if (valueCounts.ContainsKey(value))
                valueCounts[value]++;
            else
                valueCounts[value] = 1;

            if (suitCounts.ContainsKey(suit))
                suitCounts[suit]++;
            else
                suitCounts[suit] = 1;
        }

        // Check for different poker hands
        List<int> counts = valueCounts.Values.ToList();
        counts.Sort((a, b) => b.CompareTo(a));

        // FIXED: Only allow flush-based hands with 5+ cards
        bool isFlush = cards.Count >= 5 && suitCounts.Values.Any(count => count >= 5);
        bool isStraight = cards.Count >= 5 && CheckStraight(valueCounts.Keys.ToList());

        Debug.Log("IsFlush: " + isFlush + ", IsStraight: " + isStraight);
        Debug.Log("Value counts: " + string.Join(", ", counts));

        // FIXED: Royal Flush and Straight Flush only work with 5+ cards
        // Royal Flush (A, K, Q, J, 10 all same suit)
        if (cards.Count >= 5 && isFlush && isStraight && valueCounts.Keys.Contains(1) && valueCounts.Keys.Contains(13))
            return RANKS.RoyalFlush;

        // Straight Flush
        if (cards.Count >= 5 && isFlush && isStraight)
            return RANKS.StraightFlush;

        // Four of a Kind
        if (counts.Count > 0 && counts[0] >= 4)
            return RANKS.FourKind;

        // Full House
        if (counts.Count >= 2 && counts[0] >= 3 && counts[1] >= 2)
            return RANKS.FullHouse;

        // FIXED: Flush only works with 5+ cards
        if (cards.Count >= 5 && isFlush)
            return RANKS.Flush;

        // FIXED: Straight only works with 5+ cards
        if (cards.Count >= 5 && isStraight)
            return RANKS.Straight;

        // Three of a Kind
        if (counts.Count > 0 && counts[0] >= 3)
            return RANKS.ThreeKind;

        // Two Pair
        if (counts.Count >= 2 && counts[0] >= 2 && counts[1] >= 2)
            return RANKS.TwoPair;

        // One Pair
        if (counts.Count > 0 && counts[0] >= 2)
            return RANKS.OnePair;

        return RANKS.HighCard;
    }

    private int GetCardValue(string cardName)
    {
        if (cardName.Contains("A")) return 14; // Ace high
        if (cardName.Contains("K")) return 13;
        if (cardName.Contains("Q")) return 12;
        if (cardName.Contains("J")) return 11;
        if (cardName.Contains("10")) return 10;

        // Try to parse number from card name
        for (int i = 2; i <= 10; i++)
        {
            if (cardName.Contains(i.ToString()))
                return i;
        }

        // If no standard card value found, use DMG value as fallback
        return UnityEngine.Random.Range(2, 15);
    }

    private string GetCardSuit(string cardName)
    {
        // Extract suit from card name
        if (cardName.Contains("H")) return "Hearts";
        if (cardName.Contains("D")) return "Diamonds";
        if (cardName.Contains("S")) return "Spades";
        if (cardName.Contains("C")) return "Clubs";

        // If no suit found in name, use hash as fallback
        return (cardName.GetHashCode() % 4).ToString();
    }

    private bool CheckStraight(List<int> values)
    {
        if (values.Count < 5) return false;

        List<int> uniqueValues = values.Distinct().ToList();
        uniqueValues.Sort();

        int consecutive = 1;

        for (int i = 1; i < uniqueValues.Count; i++)
        {
            if (uniqueValues[i] == uniqueValues[i - 1] + 1)
            {
                consecutive++;
                if (consecutive >= 5) return true;
            }
            else
            {
                consecutive = 1;
            }
        }

        // Check for A-2-3-4-5 straight (Ace low)
        if (uniqueValues.Contains(14) && uniqueValues.Contains(2) && uniqueValues.Contains(3) && uniqueValues.Contains(4) && uniqueValues.Contains(5))
        {
            return true;
        }

        return false;
    }

    private float GetHandMultiplier(RANKS handRank)
    {
        switch (handRank)
        {
            case RANKS.HighCard:
                return 1.0f;
            case RANKS.OnePair:
                return 1.2f;
            case RANKS.TwoPair:
                return 1.5f;
            case RANKS.ThreeKind:
                return 2.0f;
            case RANKS.Straight:
                return 2.5f;
            case RANKS.Flush:
                return 3.0f;
            case RANKS.FullHouse:
                return 4.0f;
            case RANKS.FourKind:
                return 6.0f;
            case RANKS.StraightFlush:
                return 10.0f;
            case RANKS.RoyalFlush:
                return 15.0f;
            default:
                return 1.0f;
        }
    }

    private void MoveCardsToPlayedArea(Card card)
    {
        card.selected = false;
        card.isPlayed = true;
        card.transform.parent.SetParent(this.transform, false);
        card.transform.localPosition = Vector3.zero;
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

    public IEnumerator PlaySequence(int index, RANKS handRankPlayer, RANKS handRankBot)
    {
        yield return new WaitForSeconds(0.8f);

        if (index < playedCards.Count)
        {
            // BattleCards(playedCardsBot[index], playedCards[index].cardType);
            // index++;
            Select(playedCards[index], playedCards[index].selected);
            CheckPokerHand(playedCards[index].cardType, handRankPlayer);
            CheckBotPokerHand(playedCardsBot[index], handRankBot);
            UpdateScore();
            index++;
            yield return StartCoroutine(PlaySequence(index, handRankPlayer, handRankBot));
        }
    }



    IEnumerator DestroyCards()
    {
        List<CardAttributes> playerHand = new List<CardAttributes>();
        for (int index = 0; index < playedCards.Count; index++)
        {
            playerHand.Add(playedCards[index].cardType);
        }
        RANKS handRankPlayer = EvaluatePokerHand(playerHand);
        RANKS handRankBot = EvaluatePokerHand(playedCardsBot);

        yield return PlaySequence(0, handRankPlayer, handRankBot);

        float multiplierPlayer = GetHandMultiplier(handRankPlayer);
        float multiplierBot = GetHandMultiplier(handRankBot);

        UpdateTotalScore(multiplierPlayer, multiplierBot);



        Debug.Log("Player Total Score: " + playerBaseDmgScore + " -> " + playerTotalScore + " (+" + (playerTotalScore - playerBaseDmgScore) + " bonus)");


        Debug.Log("Bot Total Score: " + botBaseDmgScore + " -> " + botTotalScore + " (+" + (botTotalScore - botBaseDmgScore) + " bonus)");

        playerBaseDmgScore = 0;
        botBaseDmgScore = 0;
        UpdateScore();

        int i = 0;
        while (i < playedCards.Count)
        {
            Destroy(playedCards[i].transform.parent.gameObject);
            i++;
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("done playing");


        Debug.Log("Round ended - Score reset");
        isButtonOnCooldown = false;

        if (pokerHandText != null)
        {
            pokerHandText.text = "Player: Select cards to play";
            pokerHandText.color = Color.white;
        }
        if (botHandText != null)
        {
            botHandText.text = "Bot: Waiting...";
            botHandText.color = Color.white;
        }

        playedCards.Clear();
        PlayingCardHolder.DrawHand();
    }

    private void Select(Card card, bool state)
    {
        card.SelectEvent.Invoke(card, card.selected);
    }
}