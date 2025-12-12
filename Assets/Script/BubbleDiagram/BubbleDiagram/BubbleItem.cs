using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class BubbleItem
{
    public CategoryType category;
    public Image bubbleImage;
    public TMP_Text label;
    public Vector2 basePosition; // 고정 위치
}