using System;
using System.Collections.Generic;
using UnityEngine;

public class FSRSOptimizer
{
    public double lr = 0.0003;

    double R(double S, double t)
    {
        return Math.Pow(1.0 + t / (9.0 * S), -1.0);
    }

    double Loss(double r, double y)
    {
        return -(y * Math.Log(r + 1e-12) + (1 - y) * Math.Log(1 - r + 1e-12));
    }

    double dL_dR(double r, double y)
    {
        return (r - y) / (r * (1 - r) + 1e-12);
    }

    double dR_dS(double S, double t)
    {
        double baseTerm = 1.0 + t / (9.0 * S);
        return (t / (9.0 * S * S)) * Math.Pow(baseTerm, -2.0);
    }

    double NextStability(double s, double d, double r, int rating, double[] w)
    {
        if (rating == 1)
        {
            return w[11] * Math.Pow(d, -w[12]) *
                   (Math.Pow(s + 1.0, w[13]) - 1.0) *
                   Math.Exp(w[14] * (1.0 - r));
        }
        else
        {
            double hardPenalty = (rating == 2) ? w[15] : 1.0;
            double easyBonus = (rating == 4) ? w[16] : 1.0;

            double growth =
                Math.Exp(w[8]) *
                (11.0 - d) *
                Math.Pow(s, -w[9]) *
                (Math.Exp((1.0 - r) * w[10]) - 1.0);

            return s * (1.0 + growth * hardPenalty * easyBonus);
        }
    }

    public void Train(Deck deck, int epochs = 5)
    {
        var sequences = BuildSequences(deck);

        double[] w = ConvertToDouble(deck.w);

        for (int e = 0; e < epochs; e++)
        {
            foreach (var seq in sequences)
            {
                TrainSequence(w, seq);
            }
        }

        deck.w = ConvertToFloat(w);

        Debug.Log("FSRS Full Optimizer Done");
    }

    void TrainSequence(double[] w, List<FSRSData> seq)
    {
        double[] grad = new double[w.Length];

        foreach (var data in seq)
        {
            double d = data.d;
            double s = data.s;
            double t = data.t;

            double r_prev = R(s, t);

            double s_next = NextStability(s, d, r_prev, data.rating, w);

            double r = R(s_next, data.t_next);

            double y = data.y;

            double dl_dr = dL_dR(r, y);
            double dr_ds = dR_dS(s_next, data.t_next);

            double dL_dS = dl_dr * dr_ds;

            for (int i = 0; i < w.Length; i++)
            {
                double dS_dw = dNextStability_dw(i, s, d, r_prev, data.rating, w);
                grad[i] += dL_dS * dS_dw;
            }
        }

        for (int i = 0; i < w.Length; i++)
        {
            w[i] -= lr * grad[i];
        }
    }

    double dNextStability_dw(int i, double s, double d, double r, int rating, double[] w)
    {
        if (rating == 1)
        {
            double baseVal =
                Math.Pow(d, -w[12]) *
                (Math.Pow(s + 1.0, w[13]) - 1.0) *
                Math.Exp(w[14] * (1.0 - r));

            if (i == 11) return baseVal;
            if (i == 12) return -w[11] * baseVal * Math.Log(d);
            if (i == 13) return w[11] * baseVal * Math.Log(s + 1.0);
            if (i == 14) return w[11] * baseVal * (1.0 - r);
        }
        else
        {
            double expTerm = Math.Exp(w[8]);
            double growth =
                expTerm *
                (11.0 - d) *
                Math.Pow(s, -w[9]) *
                (Math.Exp((1.0 - r) * w[10]) - 1.0);

            if (i == 8) return s * growth;
            if (i == 9) return -s * growth * Math.Log(s);
            if (i == 10) return s * growth * (1.0 - r);
            if (i == 15 && rating == 2) return s * growth;
            if (i == 16 && rating == 4) return s * growth;
        }

        return 0.0;
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

    double[] ConvertToDouble(float[] arr)
    {
        double[] result = new double[arr.Length];
        for (int i = 0; i < arr.Length; i++)
            result[i] = arr[i];
        return result;
    }

    float[] ConvertToFloat(double[] arr)
    {
        float[] result = new float[arr.Length];
        for (int i = 0; i < arr.Length; i++)
            result[i] = (float)arr[i];
        return result;
    }
}
