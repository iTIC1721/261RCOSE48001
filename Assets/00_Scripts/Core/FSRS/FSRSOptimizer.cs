using System;
using System.Collections.Generic;
using UnityEngine;

public class FSRSOptimizer
{
    public const double lr = 0.0003;
    public const double cutoffThreshold = 0.01;

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¸Á°¢ °î¼± : R(S, t) = (1 + t / (9S))^(-1)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    double R(double S, double t)
    {
        return Math.Pow(1.0 + t / (9.0 * S), -1.0);
    }

    // Loss gradient : ¡ÓL/¡ÓR  (Binary cross-entropy ±â¹Ý)
    double dL_dR(double r, double y)
    {
        return (r - y) / (r * (1.0 - r) + 1e-12);
    }

    // ¡ÓR/¡ÓS
    double dR_dS(double S, double t)
    {
        double baseTerm = 1.0 + t / (9.0 * S);
        return (t / (9.0 * S * S)) * Math.Pow(baseTerm, -2.0);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ´ÙÀ½ Stability °è»ê
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    double NextStability(double s, double d, double r, int rating, double[] w)
    {
        if (rating == 1)
        {
            double sf = w[11] * Math.Pow(d, -w[12]) *
                        (Math.Pow(s + 1.0, w[13]) - 1.0) *
                        Math.Exp(w[14] * (1.0 - r));
            return Math.Min(sf, s); // post-lapse stability ¡Â s
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

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¡ÓD / ¡Ów  (difficulty ¾÷µ¥ÀÌÆ® ¼ö½Ä¿¡¼­)
    //
    // D' = D + delta(rating) * (10 - D) / 9   (linear damping)
    // D''= 0.1*w4 + 0.9*D'                    (mean reversion)
    //
    // ¡ÓD''/¡Ów4 = 0.1
    // ¡ÓD''/¡Ów[delta_idx] = 0.9 * (10 - D) / 9  (ÇØ´ç ratingÀÇ delta index)
    //
    // w[6]=Again delta, w[7]=Hard, w[8]=-Good, w[9]=-Easy
    //
    // ÁÖÀÇ: w[8]Àº Stability ¼ö½Ä¿¡¼­µµ »ç¿ëµÇÁö¸¸, ¿©±â¼­´Â
    //       difficulty °æ·Î¿¡¼­ÀÇ ¡ÓD/¡Ów[8] ¸¸ °è»êÇÕ´Ï´Ù.
    //       Stability ¼ö½ÄÀÇ ¡ÓS/¡Ów[8] Àº dS_dw ¿¡¼­ º°µµ °è»êÇÕ´Ï´Ù.
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    double dD_dw(int i, double d, int rating, double[] w)
    {
        double scale = 0.9 * (10.0 - d) / 9.0;

        // mean reversion Ç×: ¡ÓD''/¡Ów4 = 0.1
        if (i == 4) return 0.1;

        // linear damping Ç×: ¡ÓD''/¡Ów[delta] = scale
        if (rating == 1 && i == 6) return scale;
        if (rating == 2 && i == 7) return scale;

        // Good(3)Àº -w[8] ÀÌÁö¸¸, w[8]Àº stability ¼ö½Ä°ú °øÀ¯
        // ¡æ difficulty °æ·Î¿¡¼­´Â º°µµ ÆÄ¶ó¹ÌÅÍÃ³·³ ´Ù·ç±â À§ÇØ ºÐ¸®
        // ÇöÀç ÄÚµå¿¡¼­´Â w[8]/w[9]¸¦ difficulty delta·Îµµ ¾²´Â ±¸Á¶ÀÌ¹Ç·Î
        // difficulty °æ·ÎÀÇ ±â¿©ºÐ¸¸ ¹ÝÈ¯ÇÕ´Ï´Ù.
        if (rating == 3 && i == 8) return -scale;   // ¡ç difficulty ±â¿©ºÐ
        if (rating == 4 && i == 9) return -scale;   // ¡ç difficulty ±â¿©ºÐ

        return 0.0;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¡ÓS_next / ¡ÓD  (stability ¼ö½Ä ¾ÈÀÇ D ÀÇÁ¸¼º)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    double dS_dD(double s, double d, double r, int rating, double[] w)
    {
        if (rating == 1)
        {
            double baseVal =
                Math.Pow(d, -w[12]) *
                (Math.Pow(s + 1.0, w[13]) - 1.0) *
                Math.Exp(w[14] * (1.0 - r));
            return -w[11] * w[12] * baseVal / d;
        }
        else
        {
            return -s * Math.Exp(w[8]) *
                   Math.Pow(s, -w[9]) *
                   (Math.Exp((1.0 - r) * w[10]) - 1.0);
        }
    }


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¡ÓS_next / ¡Ów  (stability ¼ö½Ä Á÷Á¢ ÀÇÁ¸¼º)
    //
    // w[8]ÀÌ Forget/Recall ¾çÂÊ¿¡ °ü¿©ÇÏÁö ¾Êµµ·Ï
    // rating==1 (Forget) ¿¡¼­´Â w[8] ±â¿© = 0
    // rating!=1 (Recall) ¿¡¼­´Â w[8] ±â¿© °è»ê
    // ¡æ µÎ °æ·Î°¡ ¸íÈ®È÷ ºÐ¸®µË´Ï´Ù.
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    double dS_dw(int i, double s, double d, double r, int rating, double[] w)
    {
        if (rating == 1) // Forget °æ·Î
        {
            double baseVal =
                Math.Pow(d, -w[12]) *
                (Math.Pow(s + 1.0, w[13]) - 1.0) *
                Math.Exp(w[14] * (1.0 - r));

            if (i == 11) return baseVal;
            if (i == 12) return -w[11] * baseVal * Math.Log(d + 1e-12);
            if (i == 13) return w[11] * baseVal * Math.Log(s + 1.0 + 1e-12);
            if (i == 14) return w[11] * baseVal * (1.0 - r);
        }
        else // Recall °æ·Î
        {
            double hardPenalty = (rating == 2) ? w[15] : 1.0;
            double easyBonus = (rating == 4) ? w[16] : 1.0;
            double expTerm = Math.Exp(w[8]);

            double growth =
                expTerm *
                (11.0 - d) *
                Math.Pow(s, -w[9]) *
                (Math.Exp((1.0 - r) * w[10]) - 1.0);

            // w[8]: stability ¼ö½ÄÀÇ Á÷Á¢ ±â¿© (exp(w8) ½ºÄÉÀÏ)
            // w[8]ÀÌ difficulty delta(-w[8])·Îµµ ¾²ÀÌÁö¸¸,
            // stability ¼ö½ÄÀÇ ¡ÓS/¡Ów[8] Àº ¿©±â¼­¸¸ °è»êÇÕ´Ï´Ù.
            if (i == 8) return s * growth * hardPenalty * easyBonus;
            if (i == 9) return -s * growth * Math.Log(s + 1e-12) * hardPenalty * easyBonus;
            if (i == 10) return s * growth * (1.0 - r) * hardPenalty * easyBonus;
            if (i == 15 && rating == 2) return s * growth;
            if (i == 16 && rating == 4) return s * growth;
        }

        return 0.0;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¡ÓS_next / ¡Ów  (ÀüÃ¼ = Á÷Á¢ °æ·Î + difficulty °æÀ¯ °æ·Î)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    double dNextStability_dw(int i, double s, double d, double r, int rating, double[] w)
    {
        double direct = dS_dw(i, s, d, r, rating, w);
        double via_diff = dS_dD(s, d, r, rating, w) * dD_dw(i, d, rating, w);
        return direct + via_diff;
    }

    // --------------------

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÇÐ½À µ¥ÀÌÅÍ ½ÃÄö½º ±¸¼º
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    List<List<FSRSData>> BuildSequences(Deck deck)
    {
        var sequences = new List<List<FSRSData>>();

        foreach (var card in deck.cards)
        {
            var seq = new List<FSRSData>();
            var logs = card.logs;

            for (int i = 0; i < logs.Count - 1; i++)
            {
                // Ã¹ ¹øÂ° ·Î±×(ÃÊ±âÈ­)´Â optimizer ÇÐ½À¿¡¼­ Á¦¿Ü
                // (D0, S0 ÃÊ±âÈ­ ½ÃÁ¡ÀÇ µ¥ÀÌÅÍ´Â ÀÇ¹ÌÀÖ´Â t_next°¡ ¾øÀ½)
                if (logs[i].elapsedDays < 0.001f) continue;

                seq.Add(new FSRSData(logs[i], logs[i + 1]));
            }

            if (seq.Count > 0)
                sequences.Add(seq);
        }

        return sequences;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÇÐ½À ÁøÀÔÁ¡
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void Train(Deck deck, int epochs = 5)
    {
        var sequences = BuildSequences(deck);
        double[] w = ConvertToDouble(deck.w);

        for (int e = 0; e < epochs; e++)
        {
            foreach (var seq in sequences)
                TrainSequence(w, seq);
        }

        deck.w = ConvertToFloat(w);
        Log.LogMessage("FSRS Optimizer Done");
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ½ÃÄö½º ´ÜÀ§ °æ»ç ÇÏ°­¹ý
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void TrainSequence(double[] w, List<FSRSData> seq)
    {
        double[] grad = new double[w.Length];

        foreach (var data in seq)
        {
            double d = data.d;
            double s = data.s;
            double t = data.t;

            // ³Ê¹« ÂªÀº °£°Ý µ¥ÀÌÅÍ Á¦¿Ü
            if (data.t_next < cutoffThreshold * s) continue;

            double r_prev = R(s, t);
            double s_next = NextStability(s, d, r_prev, data.rating, w);
            double r = R(s_next, data.t_next);
            double y = data.y;

            double dl_dr = dL_dR(r, y);
            double dr_ds = dR_dS(s_next, data.t_next);
            double dL_dS = dl_dr * dr_ds;

            for (int i = 0; i < w.Length; i++)
            {
                double dS = dNextStability_dw(i, s, d, r_prev, data.rating, w);
                grad[i] += dL_dS * dS;
            }
        }

        for (int i = 0; i < w.Length; i++)
            w[i] -= lr * grad[i];
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
