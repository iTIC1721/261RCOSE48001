using System;
using System.Collections.Generic;
using UnityEngine;

public class FSRSOptimizer
{
    public const double lr = 0.0003;
    public const double cutoffThreshold = 0.01;

    double R(double S, double t)
    {
        return Math.Pow(1.0 + t / (9.0 * S), -1.0);
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

    // --------------------
    
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

    /// <summary>
    /// 경사하강법으로 w 파라미터  갱신
    /// </summary>
    /// <param name="w"></param>
    /// <param name="seq"></param>
    void TrainSequence(double[] w, List<FSRSData> seq)
    {
        double[] grad = new double[w.Length];

        foreach (var data in seq)
        {
            double d = data.d;  // 카드 difficulty
            double s = data.s;  // 카드 stability
            double t = data.t;  // 이전 리뷰까지 경과 시간

            // 시간 간격이 너무 짧은 데이터는 제거
            if (data.t_next < cutoffThreshold * s)
            {
                continue;
            }

            double r_prev = R(s, t);    // 이전 시점에서 기억을 떠올릴 확률

            double s_next = NextStability(s, d, r_prev, data.rating, w);    // 다음 복습 이후의 stability

            double r = R(s_next, data.t_next);  // 다음 시점에서 기억을 떠올릴 확률

            double y = data.y;  // 기억했는지 못했는지

            double dl_dr = dL_dR(r, y); // loss 미분: 예측값 r과 정답 y의 차이에 따른 loss gradient
            double dr_ds = dR_dS(s_next, data.t_next);  // r 미분: stability 변화가 recall probability에 미치는 영향

            double dL_dS = dl_dr * dr_ds;   // ∂L / ∂s

            for (int i = 0; i < w.Length; i++)
            {
                double dS_dw = dNextStability_dw(i, s, d, r_prev, data.rating, w);  // ∂s_next / ∂wi : 파라미터별 gradient
                grad[i] += dL_dS * dS_dw;
            }
        }

        for (int i = 0; i < w.Length; i++)
        {
            w[i] -= lr * grad[i];   // learning rate만큼 파라미터 업데이트
        }
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
