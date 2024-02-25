using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyUI : MonoBehaviour
{
    [SerializeField] private Button closeButton;

    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private Button createPublicButton;
    [SerializeField] private Button createPrivateButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
        createPublicButton.onClick.AddListener(() =>
        {
            // for testing purposes, will replace later
            GameLobby.Instance.CreateLobby(false, inputField.text);
        });
        createPrivateButton.onClick.AddListener(() =>
        {
            // for testing purposes, will replace later
            GameLobby.Instance.CreateLobby(true, inputField.text);
        });

        Hide();
    }
    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    private void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
