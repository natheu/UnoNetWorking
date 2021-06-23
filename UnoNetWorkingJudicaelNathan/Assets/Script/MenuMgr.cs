using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuMgr : MonoBehaviour
{
    Button HostB = null;
    Button ClientB = null;
    InputField IPInput = null;
    InputField ChatInput = null;

    InputField NameInput = null;
    Button Ready = null;
    Button Play = null;
    Button Disconnect = null;
    [SerializeField]
    GameMgr game;

    // Start is called before the first frame update
    void Start()
    {
        Transform panelConnec = transform.Find("ConnectionPanel");
        HostB = panelConnec.Find("Host").GetComponent<Button>();
        ClientB = panelConnec.Find("Client").GetComponent<Button>();
        IPInput = panelConnec.Find("IPInputField").GetComponent<InputField>();
        NameInput = transform.Find("LobbyPanel").Find("NameInput").GetComponent<InputField>();
        Ready = transform.Find("LobbyPanel").Find("Ready").GetComponent<Button>();
        Play = transform.Find("LobbyPanel").Find("Play").GetComponent<Button>();
        Disconnect = transform.Find("LobbyPanel").Find("Disconnect").GetComponent<Button>();

        NameInput.gameObject.SetActive(false);
        IPInput.gameObject.SetActive(false);
        Ready.gameObject.SetActive(false);
        Play.gameObject.SetActive(false);
        Disconnect.gameObject.SetActive(false);


        Transform panel = transform.Find("ChatPanel");
        ChatInput = panel.Find("ChatInputField").GetComponent<InputField>();

        HostB.onClick.AddListener(() => {
            // nbMaxPlayer and port 50150
            NetWorkingCSharp.ServerTCP.CreateServer(20, 50150, true);
            panelConnec.gameObject.SetActive(false);
            //game.CreateClient("127.0.0.1", 50150);
            NameInput.gameObject.SetActive(true);
            Play.gameObject.SetActive(true);
        });

        ClientB.onClick.AddListener(() => {
            IPInput.gameObject.SetActive(true);
            HostB.gameObject.SetActive(false);
            ClientB.gameObject.SetActive(false);
            Disconnect.gameObject.SetActive(true);
        });

        IPInput.onEndEdit.AddListener((Ip) => {
            if (game.CreateClient(Ip, 50150))
            {
                IPInput.gameObject.SetActive(false);
                NameInput.gameObject.SetActive(true);
                Ready.gameObject.SetActive(true);
            }
        });

        ChatInput.onEndEdit.AddListener((Msg) => {
            game.SendMsg(Msg);
        });
        NameInput.onEndEdit.AddListener((Msg) => {
            game.SendMsg(Msg, NetWorkingCSharp.EType.UPDATENAME);
            NameInput.gameObject.SetActive(false);
        });

        BindEventToPlayReady();

        Disconnect.onClick.AddListener(() =>
        {
            Disconnect.gameObject.SetActive(false);
            HostB.gameObject.SetActive(true);
            ClientB.gameObject.SetActive(true);
            game.DisconnectClient();
        });

    }

    void BindEventToPlayReady()
    {
        Play.onClick.AddListener(() => {
            game.StartAGame();
        });

        Ready.onClick.AddListener(() => {
            game.PlayerIsReady();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
