using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField]
    private GameObject dialogBox;
    [SerializeField]
    private Text dialogText;
    [SerializeField]
    private int lettersPerSecond;

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    private int currentLine = 0;
    private Dialog dialog;
    private Action onDialogFinished;

    private bool isTyping;

    public bool IsShowing { get; private set; }

    public static DialogManager Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public IEnumerator ShowDialog(Dialog dialog, Action onFinished=null) {

        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();

        IsShowing = true;
        this.dialog = dialog;
        onDialogFinished = onFinished;

        dialogBox.SetActive(true);
        StartCoroutine(TypeDialog(dialog.Lines[0]));
    }

    public IEnumerator TypeDialog(string line) {
        isTyping = true;
        dialogText.text = "";

        foreach (char letter in line.ToCharArray()) {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        isTyping = false;
    }

    public void HandleUpdate() {
        if (Input.GetKeyDown(KeyCode.Z) && !isTyping) {
            currentLine++;
            if (currentLine < dialog.Lines.Count) {
                StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
            }
            else {
                currentLine = 0;
                IsShowing = false;
                dialogBox.SetActive(false);
                onDialogFinished?.Invoke();
                OnCloseDialog?.Invoke();
            }
        }
    }
}
