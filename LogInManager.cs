using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Oculus.Platform;
using System;
using UnityEngine.UI;


public class LogInManager : MonoBehaviourPunCallbacks
{

    public GameObject _spawnPoint;
    [SerializeField] Text m_screenText;
    [SerializeField] ulong m_userId;

    //Singleton implementation
    private static LogInManager m_instance;
    public static LogInManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    private void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetUserIdFromLoggedInUser());
        StartCoroutine(ConnectToPhotonRoomOnceUserIdIsFound());
        StartCoroutine(InstantiateNetworkedAvatarOnceInRoom());
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator SetUserIdFromLoggedInUser()
    {
        if (OvrPlatformInit.status == OvrPlatformInitStatus.NotStarted)
        {
            OvrPlatformInit.InitializeOvrPlatform();
        }

        while (OvrPlatformInit.status != OvrPlatformInitStatus.Succeeded)
        {
            if (OvrPlatformInit.status == OvrPlatformInitStatus.Failed)
            {
                Debug.LogError("OVR Platform failed to initialise");
                m_screenText.text = "OVR Platform failed to initialise";
                yield break;
            }
            yield return null;
        }

        Users.GetLoggedInUser().OnComplete(message =>
        {
            if (message.IsError)
            {
                Debug.LogError("Getting Logged in user error " + message.GetError());
            }
            else
            {
                m_userId = message.Data.ID;
            }
        });
    }


    IEnumerator ConnectToPhotonRoomOnceUserIdIsFound()
    {
        while (m_userId == 0)
        {
            Debug.Log("Waiting for User id to be set before connecting to room");
            yield return null;
        }
        ConnectToPhotonRoom();
    }

    void ConnectToPhotonRoom()
    {
        PhotonNetwork.ConnectUsingSettings();
        m_screenText.text = "Connecting to Server";
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        m_screenText.text = "Connecting to Lobby";
    }


    public override void OnJoinedLobby()
    {
        m_screenText.text = "Creating Room";
        PhotonNetwork.JoinOrCreateRoom("room", null, null);
    }

    public override void OnJoinedRoom()
    {
        string roomName = PhotonNetwork.CurrentRoom.Name;
        m_screenText.text = "Joined room with name " + roomName;
    }

    IEnumerator InstantiateNetworkedAvatarOnceInRoom()
    {
        while (PhotonNetwork.InRoom == false)
        {
            Debug.Log("Waiting to be in room before intantiating avatar");
            yield return null;
        }
        InstantiateNetworkedAvatar();
    }

    void InstantiateNetworkedAvatar()
    {
        Int64 userId = Convert.ToInt64(m_userId);
        object[] objects = new object[1] { userId };
        GameObject myAvatar = PhotonNetwork.Instantiate("NetworkPlayer", _spawnPoint.transform.position, Quaternion.identity, 0, objects);
    }

    
}
