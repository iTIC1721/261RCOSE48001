using System;

[Serializable]
public class WordState
{
    public string word;
    public string meaning;

    public float strength = 1.0f;
    public float difficulty = 0.3f;

    public int totalReviews = 0;
    public int correctCount = 0;

    public float avgResponseTime = 2.0f;

    public int lastReviewedDay = -1;
    public int nextReviewDay = 0;

    public bool isLearned = false;
}
