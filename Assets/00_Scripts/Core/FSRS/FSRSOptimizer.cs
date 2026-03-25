using System.Collections.Generic;
using UnityEngine;

public class FSRSOptimizer
{
    public float lr = 0.0003f;

    float R(float S, float t)
    {
        return Mathf.Pow(1f + t / (9f * S), -1f);
    }

    float Loss(float r, float y)
    {
        return -(y * Mathf.Log(r + 1e-8f) + (1 - y) * Mathf.Log(1 - r + 1e-8f));
    }

    float dL_dR(float r, float y)
    {
        return (r - y) / (r * (1 - r) + 1e-8f);
    }

    float dR_dS(float S, float t)
    {
        float baseTerm = 1f + t / (9f * S);
        return (t / (9f * S * S)) * Mathf.Pow(baseTerm, -2f);
    }

    float NextStability(float s, float d, float r, int rating, float[] w)
    {
        // rating: 1=Again, 2=Hard, 3=Good, 4=Easy

        if (rating == 1)
        {
            // lapse case
            return w[11] * Mathf.Pow(d, -w[12]) *
                   (Mathf.Pow(s + 1f, w[13]) - 1f) *
                   Mathf.Exp(w[14] * (1f - r));
        }
        else
        {
            float hardPenalty = (rating == 2) ? w[15] : 1f;
            float easyBonus = (rating == 4) ? w[16] : 1f;

            float growth =
                Mathf.Exp(w[8]) *
                (11f - d) *
                Mathf.Pow(s, -w[9]) *
                (Mathf.Exp((1f - r) * w[10]) - 1f);

            return s * (1f + growth * hardPenalty * easyBonus);
        }
    }

    public void Train(Deck deck, int epochs = 5)
    {
        var sequences = BuildSequences(deck);

        for (int e = 0; e < epochs; e++)
        {
            foreach (var seq in sequences)
            {
                TrainSequence(deck.w, seq);
            }
        }

        Debug.Log("FSRS Full Optimizer Done");
    }

    void TrainSequence(float[] w, List<FSRSData> seq)
    {
        float[] grad = new float[w.Length];

        foreach (var data in seq)
        {
            float d = data.d;
            float s = data.s;
            float t = data.t;

            // 1. 이전 recall 확률
            float r_prev = R(s, t);

            // 2. stability update
            float s_next = NextStability(s, d, r_prev, data.rating, w);

            // 3. 다음 recall
            float r = R(s_next, data.t_next);

            float y = data.y;

            // -------------------
            // backward
            // -------------------
            float dl_dr = dL_dR(r, y);
            float dr_ds = dR_dS(s_next, data.t_next);

            float dL_dS = dl_dr * dr_ds;

            for (int i = 0; i < w.Length; i++)
            {
                float dS_dw = dNextStability_dw(i, s, d, r_prev, data.rating, w);

                grad[i] += dL_dS * dS_dw;
            }
        }

        // -------------------
        // update
        // -------------------
        for (int i = 0; i < w.Length; i++)
        {
            w[i] -= lr * grad[i];
        }
    }

    float dNextStability_dw(int i, float s, float d, float r, int rating, float[] w)
    {
        if (rating == 1)
        {
            float baseVal =
                Mathf.Pow(d, -w[12]) *
                (Mathf.Pow(s + 1f, w[13]) - 1f) *
                Mathf.Exp(w[14] * (1f - r));

            if (i == 11) return baseVal;
            if (i == 12) return -w[11] * baseVal * Mathf.Log(d);
            if (i == 13) return w[11] * baseVal * Mathf.Log(s + 1f);
            if (i == 14) return w[11] * baseVal * (1f - r);
        }
        else
        {
            float expTerm = Mathf.Exp(w[8]);
            float growth =
                expTerm *
                (11f - d) *
                Mathf.Pow(s, -w[9]) *
                (Mathf.Exp((1f - r) * w[10]) - 1f);

            if (i == 8) return s * growth;
            if (i == 9) return -s * growth * Mathf.Log(s);
            if (i == 10) return s * growth * (1f - r);
            if (i == 15 && rating == 2) return s * growth;
            if (i == 16 && rating == 4) return s * growth;
        }

        return 0f;
    }

    List<List<FSRSData>> BuildSequences(Deck deck)
    {
        List<List<FSRSData>> sequences = new List<List<FSRSData>>();

        foreach (var card in deck.cards)
        {
            List<FSRSData> seq = new List<FSRSData>();

            var logs = card.logs;

            for (int i = 0; i < logs.Count - 1; i++)
            {
                seq.Add(new FSRSData(logs[i], logs[i + 1]));
            }

            sequences.Add(seq);
        }

        return sequences;
    }
}
