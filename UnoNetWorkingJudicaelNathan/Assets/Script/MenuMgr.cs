using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuMgr : MonoBehaviour
{
    Button HostB = null;
    Button ClientB = null;
    InputField IPInput = null;
    InputField ChatInput = null;
    [SerializeField]
    GameMgr game;

    // Start is called before the first frame update
    void Start()
    {
        Transform panelConnec = transform.Find("ConnectionPanel");
        HostB = panelConnec.Find("Host").GetComponent<Button>();
        ClientB = panelConnec.Find("Client").GetComponent<Button>();
        IPInput = panelConnec.Find("IPInputField").GetComponent<InputField>();
        IPInput.gameObject.SetActive(false);
        Transform panel = transform.Find("ChatPanel");
        ChatInput = panel.Find("ChatInputField").GetComponent<InputField>();

        HostB.onClick.AddListener(() => {
            // nbMaxPlayer and port 50150
            NetWorkingCSharp.ServerTCP.CreateServer(20, 50150, true);
            panelConnec.gameObject.SetActive(false);
            game.CreateClient("127.0.0.1", 50150);
        });

        ClientB.onClick.AddListener(() => {
            IPInput.gameObject.SetActive(true);
            HostB.gameObject.SetActive(true);
            ClientB.gameObject.SetActive(true);
        });

        IPInput.onEndEdit.AddListener((Ip) => {
            if (game.CreateClient(Ip, 50150))
                IPInput.gameObject.SetActive(false);
        });

        ChatInput.onEndEdit.AddListener((Msg) => {
            game.SendMsg(Msg);
        });

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
