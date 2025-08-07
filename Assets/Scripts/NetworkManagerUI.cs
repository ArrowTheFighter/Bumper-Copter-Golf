using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] Button startHostButton;
    [SerializeField] Button startClientButton;


    void Awake()
    {
        startHostButton.onClick.AddListener(() =>
        {
            print("starting host");
            NetworkManager.Singleton.StartHost();
            HideButtons();
        });
        startClientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            HideButtons();
        });
    }

    void HideButtons()
    {
        startHostButton.gameObject.SetActive(false);
        startClientButton.gameObject.SetActive(false);
     }
}
