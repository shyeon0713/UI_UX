using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CategoryRule", menuName = "Scriptable Objects/CategoryRule")]
public class CategoryRule : ScriptableObject
{
    // 카테고리 하나의 규칙을 담는 스크립트
    public CategoryType categoryType;

    [Tooltip("가맹점명에 포함되면 해당 카테고리로 분류되는 키워드 목록")]
    public List<string> keywords = new List<string>();
}
