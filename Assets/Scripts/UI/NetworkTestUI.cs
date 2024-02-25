using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkTestUI : MonoBehaviour
{
    [SerializeField] private Button createHostButton;
    [SerializeField] private Button createClientButton;

    private void Awake()
    {
        Show();

        createHostButton.onClick.AddListener(() =>
        {
            GameMultiplayer.Instance.StartHost();
            Hide();
        });
        createClientButton.onClick.AddListener(() =>
        {
            GameMultiplayer.Instance.StartClient();
            Hide();
        });
    }

    private void Show()
    {
        this.gameObject.SetActive(true);
    }

    private void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
