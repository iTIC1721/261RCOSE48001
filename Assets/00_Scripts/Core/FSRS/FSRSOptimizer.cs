using System.Collections.Generic;
using UnityEngine;

public class FSRSOptimizer
{
    public float lr = 0.0003f;

    float R(float S, float t)
    {
        return Mathf.Pow(1 + t / (9f * S), -1f);
    }

    float dL_dR(float R, float y)
    {
        return (R - y) / (R * (1 - R) + 1e-8f);
    }

    float dR_dS(float S, float t)
    {
        float baseTerm = 1 + t / (9f * S);
        return (t / (9f * S * S)) * Mathf.Pow(baseTerm, -2);
    }

    public void Train(Deck deck, int epochs = 5)
    {
        List<FSRSData> dataset = BuildDataset(deck);

        for (int e = 0; e < epochs; e++)
        {
            foreach (var data in dataset)
            {
                TrainStep(deck.w, data);
            }
        }

        Debug.Log("Optimizer Done");
    }

    void TrainStep(float[] w, FSRSData data)
    {
        float d = data.d;
        float s = data.s;
        float t = data.t;

        float r = R(s, t);
        float y = data.y;

        float dl_dr = dL_dR(r, y);
        float dr_ds = dR_dS(s, t);

        for (int i = 0; i < w.Length; i++)
        {
            float grad = dl_dr * dr_ds * Approx_dS_dw(i, d, s, r, data.rating, w);
            w[i] -= lr * grad;
        }
    }

    // 간단화된 전체 gradient (핵심 weight 반영)
    float Approx_dS_dw(int i, float d, float s, float r, int rating, float[] w)
    {
        if (rating == 1)
        {
            if (i == 11)
                return Mathf.Pow(s + 1, w[13]) - 1;
        }
        else
        {
            if (i == 4)
                return s;

            if (i == 5)
                return -s * Mathf.Log(s);

            if (i == 6)
                return s * (1 - r);
        }

        return 0f;
    }

    List<FSRSData> BuildDataset(Deck deck)
    {
        List<FSRSData> dataset = new List<FSRSData>();

        foreach (var card in deck.cards)
        {
            foreach (var log in card.logs)
            {
                dataset.Add(new FSRSData(log));
            }
        }

        return dataset;
    }
}
