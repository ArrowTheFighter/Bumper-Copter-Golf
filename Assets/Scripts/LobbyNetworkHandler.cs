using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class LobbyNetworkHandler : MonoBehaviour
{
    public UnityTransport unityTransport;
    public TMP_InputField AddressInputField;
    public TMP_InputField PortInputField;
    public Button HostLobbyButton;
    public Button JoinLobbyButton;
    public Button StartGameButton;

    public GameObject StartUI;
    public GameObject LobbyUI;

    public TextMeshProUGUI connectedPlayers;
    int connectedPlayersCount;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HostLobbyButton.onClick.AddListener(() =>
        {
            unityTransport.SetConnectionData(AddressInputField.text, ushort.Parse(PortInputField.text));
            NetworkManager.Singleton.StartHost();
            StartUI.SetActive(false);
            LobbyUI.SetActive(true);
        });
        JoinLobbyButton.onClick.AddListener(() =>
        {
            unityTransport.SetConnectionData(AddressInputField.text, ushort.Parse(PortInputField.text));
            NetworkManager.Singleton.StartClient();
            StartUI.SetActive(false);
            LobbyUI.SetActive(true);
        });
        StartGameButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        });
        NetworkManager.Singleton.OnClientConnectedCallback += (context) =>
        {
            connectedPlayersCount++;
            connectedPlayers.text = "Connected players: " + NetworkManager.Singleton.ConnectedClients.Count;
        };
    }

    // Update is called once per frame
    void Update()
    {

    }
}
