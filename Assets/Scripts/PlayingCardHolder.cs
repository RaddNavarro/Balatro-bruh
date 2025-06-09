using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using TeamPassione;
using UnityEditor;
using UnityEngine.SearchService;

public class PlayingCardHolder : MonoBehaviour
{
    [SerializeField] private Card selectedCard;
    [Header("Lists")]
    public List<CardAttributes> Deck = new List<CardAttributes>();
    [SerializeField] private HandManager handManager;

    [HideInInspector] public CardAttributes cardAttributes;

    [SerializeReference] private Card hoveredCard;

    [SerializeField] private GameObject slotPrefab;
    private RectTransform rect;

    [Header("Spawn Settings")]
    [SerializeField] private int cardsToSpawn = 8;
    private int maxCardsInHand = 8;
    [SerializeField] private List<Card> cards;
    public List<Card> selectedCards;

    bool isCrossing = false;
    [SerializeField] private bool tweenCardReturn = true;

    private void Start()
    {
        Shuffle(Deck);

        for (int i = 0; i < cardsToSpawn; i++)
        {
            int randomCardAttribute = Random.Range(0, Deck.Count);

            GameObject card = Instantiate(slotPrefab, transform);
            card.GetComponentInChildren<Card>().cardType = Deck[randomCardAttribute];
            Deck.RemoveAt(randomCardAttribute);
        }

        rect = GetComponent<RectTransform>();
        cards = GetComponentsInChildren<Card>().ToList();

        int cardCount = 0;

        foreach (Card card in cards)
        {
            card.PointerEnterEvent.AddListener(CardPointerEnter);
            card.PointerExitEvent.AddListener(CardPointerExit);
            card.BeginDragEvent.AddListener(BeginDrag);
            card.EndDragEvent.AddListener(EndDrag);
            card.name = cardCount.ToString();

            cardCount++;
        }

        StartCoroutine(Frame());

        IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].cardVisual != null)
                    cards[i].cardVisual.UpdateIndex(transform.childCount);
            }
        }

    }

    public void DrawHand()
    {
        int numberOfCardsToDraw = (maxCardsInHand - cards.Count);




        if (cards.Count < maxCardsInHand && cards.Count != maxCardsInHand)
        {
            Shuffle(Deck);
            for (int numberOfCards = 0; numberOfCards < numberOfCardsToDraw; numberOfCards++)
            {
                int randomCardAttribute = Random.Range(0, Deck.Count);

                GameObject card = Instantiate(slotPrefab, transform);
                card.GetComponentInChildren<Card>().cardType = Deck[randomCardAttribute];
                Deck.RemoveAt(randomCardAttribute);
            }
        }
        else
        {
            Debug.Log("Hand Full");
        }

        rect = GetComponent<RectTransform>();
        cards = GetComponentsInChildren<Card>().ToList();

        int cardCount = 0;

        foreach (Card card in cards)
        {
            card.PointerEnterEvent.AddListener(CardPointerEnter);
            card.PointerExitEvent.AddListener(CardPointerExit);
            card.BeginDragEvent.AddListener(BeginDrag);
            card.EndDragEvent.AddListener(EndDrag);
            card.name = cardCount.ToString();
            cardCount++;
        }

        StartCoroutine(Frame());

        IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].cardVisual != null)
                    cards[i].cardVisual.UpdateIndex(transform.childCount);
            }
        }
    }


    void Shuffle(List<CardAttributes> deck)
    {

        for (int i = deck.Count - 1; i > 0; i--)
        {

            int rnd = Random.Range(0, i);


            CardAttributes temp = deck[i];

            deck[i] = deck[rnd];
            deck[rnd] = temp;
        }


    }


    private void BeginDrag(Card card)
    {
        selectedCard = card;
    }

    private void EndDrag(Card card)
    {

        if (selectedCard == null)
            return;


        // animation here lmao xd not my code btw
        selectedCard.transform.DOLocalMove(selectedCard.selected ? new Vector3(0, selectedCard.selectionOffset, 0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

        // rsset the selected card to its parent container in 0, 0, 0
        //selectedCard.transform.localPosition = selectedCard.selected ? new Vector3(0, selectedCard.selectionOffset, 0) : Vector3.zero;

        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;

        selectedCard = null;
    }

    private void CardPointerEnter(Card card)
    {
        hoveredCard = card;
    }

    private void CardPointerExit(Card card)
    {
        hoveredCard = null;
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hoveredCard != null)
            {
                Destroy(hoveredCard.transform.parent.gameObject);
                cards.Remove(hoveredCard);

            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (Card card in cards)
            {

                card.Deselect();
            }
        }

        // if (handManager.hasWon)
        // {
        //     Debug.Log("We have won");
        //     foreach (Card card in cards)
        //     {
        //         card.PointerEnterEvent = null;
        //         card.PointerExitEvent = null;
        //         card.BeginDragEvent = null;
        //         card.EndDragEvent = null;
        //     }
        // }




        if (selectedCard == null)
            return;

        if (isCrossing)
            return;

        StartCoroutine(Frame());

        IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].cardVisual != null)
                    cards[i].cardVisual.UpdateIndex(transform.childCount);
            }
        }


        for (int index = 0; index < cards.Count; index++)
        {
            if (cards[index].isPlayed || selectedCard.isPlayed)
                return;

            if (selectedCard.transform.position.x > cards[index].transform.position.x)
            {
                if (selectedCard.ParentIndex() < cards[index].ParentIndex())
                {
                    Swap(index);

                    break;
                }
            }

            if (selectedCard.transform.position.x < cards[index].transform.position.x)
            {
                if (selectedCard.ParentIndex() > cards[index].ParentIndex())
                {
                    Swap(index);

                    break;
                }
            }
        }
    }


    void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = cards[index].transform.parent;

        cards[index].transform.SetParent(focusedParent);
        cards[index].transform.localPosition = cards[index].selected ? new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);

        isCrossing = false;

        if (cards[index].cardVisual == null)
            return;

        bool swapIsRight = cards[index].ParentIndex() > selectedCard.ParentIndex();
        cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        //Updated Visual Indexes
        foreach (Card card in cards)
        {
            card.cardVisual.UpdateIndex(transform.childCount);
        }

    }



    public void RemoveSelectedCardsPlayed()
    {

        for (int i = cards.Count - 1; i >= 0; i--)
        {
            Card card = cards[i];
            if (card.selected)
            {
                Debug.Log("Removed");
                cards.RemoveAt(i);

            }
        }


    }



}
