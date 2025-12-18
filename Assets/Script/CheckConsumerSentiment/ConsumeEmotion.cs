using System;

public enum ConsumeEmotion
{
    Normal,     // 기본값(기록 없음)
    Positive,   // 긍정
    Negative,   // 부정
    Impulse     // 충동
}

[Serializable]
public class DailyConsumeData
{
    public DateTime date;
    public int totalAmount;
    public ConsumeEmotion emotion;
}
