using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using static TeamPassione.Card;
using TeamPassione;

public class PlayingCardHolder : MonoBehaviour
{
    [SerializeField] private Cards selectedCard;
    [Header("Lists")]
    public List<Card> Deck = new List<Card>();


    [HideInInspector] public Card cardType;

    [SerializeReference] private Cards hoveredCard;

    [SerializeField] private GameObject slotPrefab;
    private RectTransform rect;

    [Header("Spawn Settings")]
    [SerializeField] private int cardsToSpawn = 8;
    private int maxCardsInHand = 8;
    public List<Cards> cards;
    public List<Cards> selectedCards;




    bool isCrossing = false;
    [SerializeField] private bool tweenCardReturn = true;

    


    private void Start()
    {
        Shuffle(Deck);

        for (int i = 0; i < cardsToSpawn; i++)
        {
            GameObject card = Instantiate(slotPrefab, transform);
            int randomCard = Random.Range(0, Deck.Count);

            card.GetComponentInChildren<Cards>().cardType = Deck[randomCard];
            Deck.RemoveAt(randomCard);
        }

        rect = GetComponent<RectTransform>();
        cards = GetComponentsInChildren<Cards>().ToList();

        int cardCount = 0;

        foreach (Cards card in cards)
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
        Shuffle(Deck);
        Debug.Log(cards.Count);
        int numberOfCardsToDraw = (maxCardsInHand - cards.Count);
        if (cards.Count < maxCardsInHand && cards.Count != maxCardsInHand) 
        {
            Debug.Log("inside the if");

            for (int numberOfCards = 0; numberOfCards < numberOfCardsToDraw; numberOfCards++)
            {
                GameObject card = Instantiate(slotPrefab, transform);
                int randomCard = Random.Range(0, Deck.Count);

                card.GetComponentInChildren<Cards>().cardType = Deck[randomCard];
                Deck.RemoveAt(randomCard);
            }
        }
        else
        {
            Debug.Log("Hand Full");
        }

        rect = GetComponent<RectTransform>();
        cards = GetComponentsInChildren<Cards>().ToList();

        int cardCount = 0;

        foreach (Cards card in cards)
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


    void Shuffle(List<Card> deck)
    {
        // Loops through array
        for (int i = deck.Count - 1; i > 0; i--)
        {
            // Randomize a number between 0 and i (so that the range decreases each time)
            int rnd = Random.Range(0, i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            Card temp = deck[i];

            // Swap the new and old values
            deck[i] = deck[rnd];
            deck[rnd] = temp;
        }


    }


    private void BeginDrag(Cards card)
    {
        selectedCard = card;
    }

    private void EndDrag(Cards card)
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

    private void CardPointerEnter(Cards card)
    {
        hoveredCard = card;
    }

    private void CardPointerExit(Cards card)
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
            foreach (Cards card in cards)
            {
                card.Deselect();
            }
        }

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
        foreach (Cards card in cards)
        {
            card.cardVisual.UpdateIndex(transform.childCount);
        }

    }

    public void PlayHand()
    {

        selectedCards.Clear();
        foreach (Cards card in cards)
        {
            if (card.selected)
            {
                
                selectedCards.Add(card);
                
            }
        }

        cards.RemoveAll(card => card.selected);
        
    }

   

}
