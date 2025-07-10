using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using CodeMonkey.Utils;

public class UI_InputWindow : MonoBehaviour
{
    private static UI_InputWindow instance;

    private Button_UI okBtn;
    private Button_UI cancelBtn;
    private TextMeshProUGUI titleText;
    private TMP_InputField inputField;

    private Action onCancel;
    private Action<string> onOk;

    private InputAction submitAction;
    private InputAction cancelAction;

    private void Awake()
    {
        instance = this;

        okBtn = transform.Find("okBtn").GetComponent<Button_UI>();
        cancelBtn = transform.Find("cancelBtn").GetComponent<Button_UI>();
        titleText = transform.Find("titleText").GetComponent<TextMeshProUGUI>();
        inputField = transform.Find("inputField").GetComponent<TMP_InputField>();

        // Setup input actions for enter and escape
        submitAction = new InputAction(binding: "<Keyboard>/enter");
        submitAction.AddBinding("<Keyboard>/numpadEnter");

        cancelAction = new InputAction(binding: "<Keyboard>/escape");

        submitAction.performed += ctx => SubmitInput();
        cancelAction.performed += ctx => CancelInput();

        Hide();
    }

    private void OnEnable()
    {
        submitAction.Enable();
        cancelAction.Enable();
    }

    private void OnDisable()
    {
        submitAction.Disable();
        cancelAction.Disable();
    }

    private void SubmitInput()
    {
        if (!gameObject.activeSelf) return;

        Hide();
        onOk?.Invoke(inputField.text);
    }

    private void CancelInput()
    {
        if (!gameObject.activeSelf) return;

        Hide();
        onCancel?.Invoke();
    }

    private void Show(string titleString, string inputString, string validCharacters, int characterLimit, Action onCancel, Action<string> onOk)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        titleText.text = titleString;

        inputField.characterLimit = characterLimit;
        inputField.onValidateInput = (string text, int charIndex, char addedChar) =>
        {
            return ValidateChar(validCharacters, addedChar);
        };

        inputField.text = inputString;
        inputField.Select();
        inputField.ActivateInputField();

        this.onCancel = onCancel;
        this.onOk = onOk;

        okBtn.ClickFunc = SubmitInput;
        cancelBtn.ClickFunc = CancelInput;
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private char ValidateChar(string validCharacters, char addedChar)
    {
        return validCharacters.IndexOf(addedChar) != -1 ? addedChar : '\0';
    }

    public static void Show_Static(string titleString, string inputString, string validCharacters, int characterLimit, Action onCancel, Action<string> onOk)
    {
        instance.Show(titleString, inputString, validCharacters, characterLimit, onCancel, onOk);
    }

    public static void Show_Static(string titleString, int defaultInt, Action onCancel, Action<int> onOk)
    {
        instance.Show(titleString, defaultInt.ToString(), "0123456789-", 20, onCancel,
            (string inputText) =>
            {
                if (int.TryParse(inputText, out int _i))
                    onOk(_i);
                else
                    onOk(defaultInt);
            });
    }
}
