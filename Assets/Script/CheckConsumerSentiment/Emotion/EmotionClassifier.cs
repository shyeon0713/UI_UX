using System.Collections.Generic;
using UnityEngine;
public class EmotionClassifier : MonoBehaviour
{
    [Header("Emotion Rules")]
    public List<EmotionRule> emotionRules;

    public ConsumeEmotion Classify(string text)
    {
        // 기록이 없거나 비어있으면 기본값 Normal
        if (string.IsNullOrWhiteSpace(text))
            return ConsumeEmotion.Normal;

        //  우선순위: 충동 -> 부정 -> 긍정
        if (ContainsAny(text, ConsumeEmotion.Impulse)) return ConsumeEmotion.Impulse;
        if (ContainsAny(text, ConsumeEmotion.Negative)) return ConsumeEmotion.Negative;
        if (ContainsAny(text, ConsumeEmotion.Positive)) return ConsumeEmotion.Positive;

        //어떤 키워드도 없으면 Normal
        return ConsumeEmotion.Normal;
    }


    private bool ContainsAny(string text, ConsumeEmotion target)
    {
        foreach (var rule in emotionRules)
        {
            if (rule == null || rule.emotionType != target || rule.keywords == null)
                continue;

            foreach (var keyword in rule.keywords)
            {
                if (string.IsNullOrEmpty(keyword)) continue;

                // 단순 포함 매칭
                if (text.Contains(keyword))
                    return true;
            }
        }
        return false;
    }
}