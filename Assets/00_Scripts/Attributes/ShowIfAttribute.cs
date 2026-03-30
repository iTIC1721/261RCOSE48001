using UnityEngine;

public class ShowIfAttribute : PropertyAttribute
{
    public string conditionField;
    public object compareValue;

    public ShowIfAttribute(string conditionField, object compareValue)
    {
        this.conditionField = conditionField;
        this.compareValue = compareValue;
    }
}
