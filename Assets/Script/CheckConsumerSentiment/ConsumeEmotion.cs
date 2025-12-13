using System;

public enum ConsumeEmotion
{
    Positive,
    Neutral,
    Negative
}

[Serializable]
public class DailyConsumeData
{
    public DateTime date;
    public int totalAmount;
    public ConsumeEmotion emotion;
}