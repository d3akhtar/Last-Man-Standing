using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyActionButtonsUI : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button joinWithCodeButton;

    [SerializeField] private TMP_InputField codeInputField;

    [SerializeField] private CreateLobbyUI createLobbyUI;

    private void Awake()
    {
        codeInputField.onSubmit.AddListener((string code) =>
        {
            GameLobby.Instance.JoinLobbyByCode(code);
        });

        codeInputField.gameObject.SetActive(false);

        createLobbyButton.onClick.AddListener(() =>
        {
            createLobbyUI.Show();
        });
        quickJoinButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.QuickJoin();
        });
        joinWithCodeButton.onClick.AddListener(() =>
        {
            codeInputField.gameObject.SetActive(!codeInputField.gameObject.active);
        });
    }
}
