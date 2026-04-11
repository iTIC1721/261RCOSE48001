using System.Collections.Generic;
using UnityEngine;

public class SessionManager
{
    private Queue<Card> queue = new Queue<Card>();

    public int newCount { get; private set; } = 0;
    public int reviewCount { get; private set; } = 0;
    public int studiedCount { get; private set; } = 0;

    public SessionManager(List<Card> cards)
    {
        Queue<Card> tmp = new Queue<Card>();

        foreach (var c in cards)
        {
            if (c.lastReview.Date == CustomTime.GetTimeNow())
            {
                if (c.state != CardState.Learning && c.state != CardState.Relearning)
                {
                    studiedCount++;
                }
                else
                {
                    tmp.Enqueue(c);
                }
            }
            else
            {
                if (c.state == CardState.New)
                {
                    newCount++;
                }
                else
                {
                    reviewCount++;
                }
                queue.Enqueue(c);
            }
        }

        // "다시" Card는 맨 뒤로 삽입되게
        foreach (var c in tmp)
        {
            if (c.state == CardState.New)
            {
                newCount++;
            }
            else
            {
                reviewCount++;
            }
            queue.Enqueue(c);
        }
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
