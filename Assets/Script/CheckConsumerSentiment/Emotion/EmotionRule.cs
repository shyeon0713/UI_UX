using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EmotionRule", menuName = "Scriptable Objects/EmotionRule")]
public class EmotionRule : ScriptableObject
{
    public ConsumeEmotion emotionType;
    public List<string> keywords;
}
