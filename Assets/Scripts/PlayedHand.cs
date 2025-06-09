
using UnityEngine;

public class PlayedHand : MonoBehaviour
{
    [SerializeField] private PlayingCardHolder PlayingCardHolder;

    public void MoveCardsToPlayArea()
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