using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PostJoinLobbyPlayerSingle : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Button readyButton;
    [SerializeField] private Image readyCheckMark;

    private void Awake()
    {
        readyCheckMark.gameObject.SetActive(false);

        readyButton.onClick.AddListener(() =>
        {
            PostJoinLobbyReady.Instance.SetPlayerReady();
        });
    }

    public void SetPlayerNameText(string text)
    {
        playerNameText.text = text;
    }

    public void ShowReadyButton(bool show)
    {
        readyButton.gameObject.SetActive(show);
    }

    public GameObject GetReadyCheckmarkGameObject()
    {
        return readyCheckMark.gameObject;
    }
}
