using System;

[Serializable]
public class ReviewResult
{
    public WordState word;
    public bool correct;
    public float responseTime;
    public bool isNew;
}
