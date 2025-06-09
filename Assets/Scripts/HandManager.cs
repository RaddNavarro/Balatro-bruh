using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TeamPassione;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    [SerializeField] private PlayingCardHolder PlayingCardHolder;
    [SerializeField] private List<Card> playedCards;
    [SerializeField] public List<CardAttributes> botDeck;
    [SerializeField] private TextMeshProUGUI pokerHandText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI currentRoundText;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI botHandText;
    [SerializeField] private TextMeshProUGUI discardCountText; // NEW: UI text to show remaining discards

    [SerializeField] private Button playHandButton;
    [SerializeField] private Button discardButton;

    public GameObject victoryScreen;
    [SerializeField] private Button nextLvlBtn;
    private bool isButtonOnCooldown = false;

    private List<CardAttributes> playedCardsBot;

    private int playerTotalScore = 0;
    private int botTotalScore = 0;
    private int playerBaseDmgScore = 0;
    private int botBaseDmgScore = 0;
    private int playerPoints = 0;
    private int botPoints = 0;
    private int round = 1;

    // NEW: Discard tracking variables
    private int maxDiscards = 4;
    private int discardsUsed = 0;

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
        victoryScreen.SetActive(false);
        Shuffle(botDeck);

        foreach (Card card in playedCards)
        {
            card.SelectEvent.AddListener(Select);
        }

        nextLvlBtn = victoryScreen.GetComponentInChildren<Button>();
        nextLvlBtn.gameObject.SetActive(false);

        round = 1;
        playerPoints = 0;
        botPoints = 0;
        discardsUsed = 0; // NEW: Initialize discard counter

        // Initialize UI text
        scoreText.text = "Round Points - Player: " + playerBaseDmgScore + " Bot: " + botBaseDmgScore;
        totalScoreText.text = "Total Points - Player: " + playerTotalScore + " Bot: " + botTotalScore;
        currentRoundText.text = "Round:  " + round;
        UpdateDiscardUI(); // NEW: Initialize discard UI
        
        if (pokerHandText != null)
            pokerHandText.text = "Player: Select cards to play";
        if (botHandText != null)
            botHandText.text = "Bot: Waiting...";
    }

    // NEW: Method to update discard UI
    private void UpdateDiscardUI()
    {
        if (discardCountText != null)
        {
            int remaining = maxDiscards - discardsUsed;
            discardCountText.text = "Discards Remaining: " + remaining;
            
            // Change color based on remaining discards
            if (remaining <= 0)
                discardCountText.color = Color.red;
            else if (remaining <= 1)
                discardCountText.color = Color.yellow;
            else
                discardCountText.color = Color.white;
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

        if (discardButton != null)
        {
            // MODIFIED: Only allow discarding when not in cooldown, there are selected cards, AND discards are available
            bool canDiscard = !isButtonOnCooldown && 
                             PlayingCardHolder.selectedCards.Count > 0 && 
                             discardsUsed < maxDiscards;
            discardButton.interactable = canDiscard;
        }
    }

    public void HandSequence()
    {
        isButtonOnCooldown = true;
        playedCards = new List<Card>();

        foreach (Card card in PlayingCardHolder.selectedCards)
        {
            playedCards.Add(card);

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

    // FIXED: These methods now only update UI during animation, don't recalculate scores
    private void CheckPokerHand(CardAttributes card, RANKS handRank)
    {
        float multiplier = GetHandMultiplier(handRank);
        // REMOVED: playerBaseDmgScore += card.DMG; - This was causing double counting
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
        // REMOVED: botBaseDmgScore += botCard.DMG; - This was causing double counting
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

    // NEW: Calculate score based on poker hand type - only count relevant cards
    private int CalculateHandScore(List<CardAttributes> cards, RANKS handRank)
    {
        if (cards.Count == 0) return 0;

        // For high-level hands that use all cards, count everything
        if (handRank == RANKS.RoyalFlush || handRank == RANKS.StraightFlush || 
            handRank == RANKS.Straight || handRank == RANKS.Flush)
        {
            return cards.Sum(card => card.DMG);
        }

        // Extract card values for analysis
        Dictionary<int, List<CardAttributes>> valueGroups = new Dictionary<int, List<CardAttributes>>();
        
        foreach (CardAttributes card in cards)
        {
            int value = GetCardValue(card.name.ToUpper());
            if (!valueGroups.ContainsKey(value))
                valueGroups[value] = new List<CardAttributes>();
            valueGroups[value].Add(card);
        }

        // Sort groups by count (descending) then by DMG sum (descending)
        var sortedGroups = valueGroups.Values
            .OrderByDescending(group => group.Count)
            .ThenByDescending(group => group.Sum(card => card.DMG))
            .ToList();

        switch (handRank)
        {
            case RANKS.HighCard:
                // Only count the highest value card
                return sortedGroups[0].OrderByDescending(card => card.DMG).First().DMG;

            case RANKS.OnePair:
                // Only count the pair (2 cards of same value)
                var pairGroup = sortedGroups.FirstOrDefault(group => group.Count >= 2);
                return pairGroup?.Take(2).Sum(card => card.DMG) ?? 0;

            case RANKS.TwoPair:
                // Only count both pairs (4 cards total)
                var pairs = sortedGroups.Where(group => group.Count >= 2).Take(2).ToList();
                int score = 0;
                foreach (var pair in pairs)
                {
                    score += pair.Take(2).Sum(card => card.DMG);
                }
                return score;

            case RANKS.ThreeKind:
                // Only count the three of a kind
                var threeGroup = sortedGroups.FirstOrDefault(group => group.Count >= 3);
                return threeGroup?.Take(3).Sum(card => card.DMG) ?? 0;

            case RANKS.FullHouse:
                // Count the three of a kind + the pair
                var fullHouseThree = sortedGroups.FirstOrDefault(group => group.Count >= 3);
                var fullHousePair = sortedGroups.Skip(1).FirstOrDefault(group => group.Count >= 2);
                int fullHouseScore = fullHouseThree?.Take(3).Sum(card => card.DMG) ?? 0;
                fullHouseScore += fullHousePair?.Take(2).Sum(card => card.DMG) ?? 0;
                return fullHouseScore;

            case RANKS.FourKind:
                // Only count the four of a kind
                var fourGroup = sortedGroups.FirstOrDefault(group => group.Count >= 4);
                return fourGroup?.Take(4).Sum(card => card.DMG) ?? 0;

            default:
                // Fallback: count all cards
                return cards.Sum(card => card.DMG);
        }
    }

    private void UpdateScore()
    {
        if (scoreText != null)
            scoreText.text = "Round Points - Player: " + playerBaseDmgScore + " Bot: " + botBaseDmgScore;
        currentRoundText.text = "Round: " + round;
    }
    private void UpdateTotalScore(float multiplierPlayer, float multiplierBot)
    {
        int playerScore = (int)(Math.Round((float)(playerBaseDmgScore * multiplierPlayer)));
        int botScore = (int)(Math.Round((float)(botBaseDmgScore * multiplierBot)));

        playerTotalScore += playerScore;
        botTotalScore += botScore;

        if (scoreText != null)
            scoreText.text = "Round Points - Player: " + playerTotalScore + " Bot: " + botTotalScore;
    }

    private void UpdatePoints()
    {
        if (playerTotalScore > botTotalScore)
            playerPoints += 1;
        else if (playerTotalScore < botTotalScore)
            botPoints += 1;

        if (totalScoreText != null)
            totalScoreText.text = "Round Points - Player: " + playerPoints + " Bot: " + botPoints;
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

    public IEnumerator PlaySequence(int index, RANKS handRankPlayer, RANKS handRankBot)
    {
        yield return new WaitForSeconds(0.8f);

        if (index < playedCards.Count)
        {
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

        // FIXED: Calculate scores once here based on poker hand validation
        playerBaseDmgScore = CalculateHandScore(playerHand, handRankPlayer);
        botBaseDmgScore = CalculateHandScore(playedCardsBot, handRankBot);

        // Update UI to show the validated scores
        UpdatePokerHandUI(handRankPlayer, GetHandMultiplier(handRankPlayer));
        UpdateBotHandUI(handRankBot, GetHandMultiplier(handRankBot));
        UpdateScore();

        yield return PlaySequence(0, handRankPlayer, handRankBot);

        float multiplierPlayer = GetHandMultiplier(handRankPlayer);
        float multiplierBot = GetHandMultiplier(handRankBot);

        UpdateTotalScore(multiplierPlayer, multiplierBot);

        int i = 0;
        while (i < playedCards.Count)
        {
            Destroy(playedCards[i].transform.parent.gameObject);
            i++;
            yield return new WaitForSeconds(0.5f);
        }
        playerBaseDmgScore = 0;
        botBaseDmgScore = 0;

        UpdateScore();
        UpdatePoints();
        playerTotalScore = 0;
        botTotalScore = 0;

        // Win
        if (playerPoints >= 3)
        {
            Win();
        }
        //Lose

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

        round++;
    }

    public void DiscardSelectedCards()
    {
        // MODIFIED: Check if discards are available - STRICT CHECK
        if (discardsUsed >= maxDiscards)
        {
            Debug.Log("Cannot discard: Maximum discards reached (" + discardsUsed + "/" + maxDiscards + ")");
            return;
        }

        if (isButtonOnCooldown)
        {
            Debug.Log("Cannot discard: Button on cooldown");
            return;
        }

        if (PlayingCardHolder.selectedCards.Count == 0)
        {
            Debug.Log("Cannot discard: No cards selected");
            return;
        }

        // Start the discard coroutine to handle the process properly
        StartCoroutine(DiscardCardsCoroutine());
    }

    // MODIFIED: Coroutine to properly handle card discarding with counter increment
    private IEnumerator DiscardCardsCoroutine()
    {
        // IMPORTANT: Increment discard counter FIRST to prevent multiple calls
        discardsUsed++;
        Debug.Log("Discard used: " + discardsUsed + "/" + maxDiscards);
        UpdateDiscardUI();

        // Temporarily disable the discard button to prevent spam clicking
        if (discardButton != null)
            discardButton.interactable = false;

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

        // Wait for the objects to be actually destroyed
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.2f);

        // Clear any remaining references and force a fresh draw
        if (PlayingCardHolder.selectedCards == null)
        {
            PlayingCardHolder.selectedCards = new List<Card>();
        }

        PlayingCardHolder.DrawHand();

        yield return new WaitForSeconds(0.1f);
        if (PlayingCardHolder.selectedCards.Count == 0)
        {
            PlayingCardHolder.DrawHand();
        }

        // Re-enable the discard button if discards are still available
        if (discardButton != null)
        {
            discardButton.interactable = discardsUsed < maxDiscards && PlayingCardHolder.selectedCards.Count > 0;
        }
    }




    private void Win()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int maxLevels = SceneManager.sceneCountInBuildSettings;


        victoryScreen.SetActive(true);

        if ((currentSceneIndex + 1) < maxLevels)
        {
            nextLvlBtn.gameObject.SetActive(true);
        }

    }





    private void Select(Card card, bool state)
    {
        card.SelectEvent.Invoke(card, card.selected);
    }


}