using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ArrayLayout))]
public class Custom2DGrid : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PrefixLabel(position, label);
        Rect newPosition = position;
        newPosition.y += 18f; // label's default height. Can be any number, went for it because it's showing better
        SerializedProperty data = property.FindPropertyRelative("rows");
        if (data.arraySize != 8)
        {
            data.arraySize = 8;
        }

        //getting data.rows[0][]
        for (int j = 0; j < 8; j++)
        {
            SerializedProperty row = data.GetArrayElementAtIndex(j).FindPropertyRelative("row");
            newPosition.height = 18f;
            if (row.arraySize != 8)
            {
                row.arraySize = 8;
            }
            newPosition.width = position.width / 8;
            
            for (int i = 0; i < 8; i++)
            {
                EditorGUI.PropertyField(newPosition, row.GetArrayElementAtIndex(i), GUIContent.none);
                newPosition.x += newPosition.width;
            }

            newPosition.x = position.x;
            newPosition.y += 18f;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 18f * 9;
    }
}
