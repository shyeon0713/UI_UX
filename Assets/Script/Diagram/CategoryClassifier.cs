using System.Linq;
using UnityEngine;

public class CategoryClassifier : MonoBehaviour
{
    public CategoryRuleSet ruleSet; // 카테고리 분류조건 가져오기
    public CategoryType Classify(string storeName)
    {
        string lower = storeName.ToLower();

        foreach (var rule in ruleSet.rules)
        {
            foreach (var key in rule.keywords)
            {
                if (lower.Contains(key.ToLower()))
                    return rule.categoryType;
            }
        }

        return CategoryType.Unknown;
    }
}

