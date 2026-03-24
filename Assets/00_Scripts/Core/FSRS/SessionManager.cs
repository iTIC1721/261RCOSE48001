using System.Collections.Generic;
using UnityEngine;

public class SessionManager
{
    private Queue<Card> queue = new Queue<Card>();

    public SessionManager(List<Card> cards)
    {
        foreach (var c in cards)
            queue.Enqueue(c);
    }

    public bool HasNext()
    {
        return queue.Count > 0;
    }

    public Card GetNextCard()
    {
        if (queue.Count == 0)
            return null;

        return queue.Dequeue();
    }

    public void Requeue(Card card)
    {
        queue.Enqueue(card);
    }
}
