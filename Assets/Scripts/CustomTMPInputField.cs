using UnityEngine;
using TMPro;

public class CustomTMPInputField : TMP_InputField
{
    private TMP_InputField inputField;
    private TouchScreenKeyboard keyboard;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onSelect.AddListener(OnInputFieldSelected);
    }

    private void OnInputFieldSelected(string _)
    {
        // Close TMP’s internally opened keyboard (optional)
        if (keyboard != null && keyboard.active)
            keyboard.active = false;

        // Reopen with your custom settings
        keyboard = TouchScreenKeyboard.Open(
            inputField.text,
            TouchScreenKeyboardType.Default,
            autocorrection: true,
            multiline: false,
            secure: false,
            alert: false
        );

        TouchScreenKeyboard.hideInput = false;
        Debug.Log("Custom keyboard opened!");
    }

    void Update()
    {
        // Update TMP text while typing
        if (keyboard != null && keyboard.active)
        {
            inputField.text = keyboard.text;
        }

        // Detect when keyboard closes
        if (keyboard != null &&
            (keyboard.status == TouchScreenKeyboard.Status.Done ||
             keyboard.status == TouchScreenKeyboard.Status.Canceled))
        {
            keyboard = null;
            inputField.DeactivateInputField();
            Debug.Log("Keyboard closed");
        }
    }
}


