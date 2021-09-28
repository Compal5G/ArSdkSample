using UnityEngine;
using UnityEngine.UI;

public class InputField2 : InputField
{
    public int index;

    protected override void Start()
    {
        //keyboardType = (TouchScreenKeyboardType)(-1);
        //keyboardType = TouchScreenKeyboardType.Default;
        base.Start();
        index = 0;
        //Debug.LogFormat("m_Keyboard is null: {0}", m_Keyboard == null);
        //Debug.LogFormat("touchScreenKeyboard is null: {0}", touchScreenKeyboard == null);
        //touchScreenKeyboard.active = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Debug.LogFormat("Text Length: {0}", m_Text.Length);
        m_CaretVisible = true;
    }

    protected override void LateUpdate()
    {
        //Debug.LogFormat("Active 1: {0}", touchScreenKeyboard.active);
        base.LateUpdate();
        if (m_Keyboard != null)
        {
            m_Keyboard.active = false;
        }
        //Debug.LogFormat("Active 2: {0}", touchScreenKeyboard.active);
        //if (!isFocused)
        {
            //Debug.Log("Focus");
            //if (m_Keyboard != null)
            //{
            //    m_Keyboard.text = m_Text;
            //}            //if (m_Keyboard != null)
            //{
            //    m_Keyboard.text = m_Text;
            //}
            //ActivateInputField();
            //DeactivateInputField();
            //InputAdmin.Instance.HideKeyboard();
        }
    }

    protected override void Append(string input)
    {
        for (int i = 0, imax = input.Length; i < imax; ++i)
        {
            char c = input[i];

            if (c >= ' ')
            {
                Append(c);
            }
        }
        index = caretPosition;
        Debug.LogFormat("selectionAnchorPosition: {0}, selectionFocusPosition: {1}, caretPosition: {2}", selectionAnchorPosition, selectionFocusPosition, caretPosition);
    }

    protected override void Append(char input)
    {
        // If we have an input validator, validate the input first
        if (onValidateInput != null)
            input = onValidateInput(text, caretPosition, input);
        else if (characterValidation != CharacterValidation.None)
            input = Validate(text, caretPosition, input);

        // If the input is invalid, skip it
        if (input == 0)
            return;

        // Append the character and update the label
        Insert(input);
    }

    private void Insert(char c)
    {
        string replaceString = c.ToString();

        // Can't go past the character limit
        if (characterLimit > 0 && text.Length >= characterLimit)
            return;

        m_Text = text.Insert(m_CaretPosition, replaceString);
        caretSelectPositionInternal = caretPosition += replaceString.Length;
    }

    public void Add(string input)
    {
        //Select();
        Debug.LogFormat("IsFocused: {0}", isFocused);
        Debug.LogFormat("Add: {0}", input);
        //ActivateInputField();
        Append(input);
        UpdateLabel();
        //ForceLabelUpdate();
        Debug.LogFormat("IsFocused: {0}", isFocused);
    }

    public void ExecuteBackspace()
    {
        Debug.Log("ExecuteBackspace");
        if (index > 0)
        {
            index--;
            text = text.Remove(index, 1);
            caretPosition = index;
            UpdateLabel();
        }
    }

    public void ExecuteMoveLeft()
    {
        MoveCaret(-1);
    }

    public void ExecuteMoveRight()
    {
        MoveCaret(1);
    }

    public void MoveCaret(int step)
    {
        Debug.Log("MoveCaret");
        index += step;
        caretPosition = index;
        UpdateLabel();
    }
}
