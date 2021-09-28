using System;
using UnityEngine;
using UnityEngine.UI;

public class Keyboard : MonoBehaviour
{
    enum Layout
    {
        LowerCase,
        UpperCase,
        Symbol1,
        Symbol2
    }

    InputField originalInputField;

    public GameObject[] layouts;
    public InputField3 inputField;

    public void Add(string input)
    {
        inputField.Add(input);
    }

    public void Backspace()
    {
        inputField.ExecuteBackspace();
    }

    public void MoveLeft()
    {
        inputField.ExecuteMoveLeft();
    }

    public void MoveRight()
    {
        inputField.ExecuteMoveRight();
    }

    public void ChangeLayout(int type)
    {
        Array values = Enum.GetValues(typeof(Layout));
        foreach (int value in values)
        {
            layouts[value].SetActive(value == type);
        }
    }

    public void SetText(InputField setInputField)
    {
        originalInputField = setInputField;
        inputField.text = originalInputField.text;
    }

    public void UpdateText()
    {
        originalInputField.text = inputField.text;
    }
}
