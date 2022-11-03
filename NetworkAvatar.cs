using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice.Unity;
using Oculus.Avatar2;
using System;


public class NetworkAvatar : OvrAvatarEntity
{
    [SerializeField] private GameObject NetworkPlayer;
    [SerializeField] int m_avatarToUseInZipFolder = 2; 
    PhotonView m_photonView; 
    List<byte[]> m_streamedDataList = new List<byte[]>(); 
    int m_maxBytesToLog = 15; 
    [SerializeField] ulong m_instantiationData;
    float m_cycleStartTime = 0; 
    float m_intervalToSendData = 0.08f;

    

    protected override void Awake()
    {
        ConfigureAvatarEntity();
        base.Awake();
    }

    private void Start()
    {
        m_cycleStartTime = Time.time;
        m_instantiationData = GetUserIdFromPhotonInstantiationData();
        _userId = m_instantiationData;
        StartCoroutine(TryToLoadUser());
    }

    void ConfigureAvatarEntity()
    {
        m_photonView = GetComponent<PhotonView>();
        if (m_photonView.IsMine)
        {
            SetIsLocal(true);
            
            _creationInfo.renderFilters.viewFlags = CAPI.ovrAvatar2EntityViewFlags.FirstPerson;
            _creationInfo.features = Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Preset_Default;
            
            SampleInputManager sampleInputManager = OvrAvatarManager.Instance.gameObject.GetComponent<SampleInputManager>();
            SetBodyTracking(sampleInputManager);

            OvrAvatarLipSyncContext lipSyncInput = FindObjectOfType<OvrAvatarLipSyncContext>();
            SetLipSync(lipSyncInput);

            //Change Avatar Name
            NetworkPlayer.name = "MyNetworkPlayer";
            gameObject.name = "MyAvatar";
            
        }
        else
        {
            SetIsLocal(false);
            _creationInfo.renderFilters.viewFlags = CAPI.ovrAvatar2EntityViewFlags.ThirdPerson;
            _creationInfo.features = Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Preset_Remote;

            //Change Avatar Name
            NetworkPlayer.name = "OtherNetworkPlayer";
            gameObject.name = "OtherAvatar";

            //Remove Camera
            GameObject OtherUserCamera = GetComponentInChildren<Camera>().gameObject;
            Destroy(OtherUserCamera);

            //Turn Off Locomotion
            SampleAvatarLocomotion OtherUserLocomotion = GetComponent<SampleAvatarLocomotion>(); 
            OtherUserLocomotion.enabled = false;
        }
    }

    IEnumerator TryToLoadUser()
    {
        var hasAvatarRequest = OvrAvatarManager.Instance.UserHasAvatarAsync(_userId);
        while (hasAvatarRequest.IsCompleted == false)
        {
            yield return null;
        }
        LoadUser();
    }

    private void LateUpdate()
    {
        float elapsedTime = Time.time - m_cycleStartTime;
        if (elapsedTime > m_intervalToSendData)
        {
            RecordAndSendStreamDataIfMine();
            m_cycleStartTime = Time.time;
        }

    }

    void RecordAndSendStreamDataIfMine()
    {
        if (m_photonView.IsMine)
        {
            byte[] bytes = RecordStreamData(activeStreamLod);
            m_photonView.RPC("RecieveStreamData", RpcTarget.Others, bytes);
        }
    }

    [PunRPC]
    public void RecieveStreamData(byte[] bytes)
    {
        m_streamedDataList.Add(bytes);
    }

    void LogFirstFewBytesOf(byte[] bytes)
    {
        for (int i = 0; i < m_maxBytesToLog; i++)
        {
            string bytesString = Convert.ToString(bytes[i], 2).PadLeft(8, '0');
        }
    }

    private void Update()
    {
        if (m_streamedDataList.Count > 0)
        {
            if (IsLocal == false)
            {
                byte[] firstBytesInList = m_streamedDataList[0];
                if (firstBytesInList != null)
                {
                    ApplyStreamData(firstBytesInList);
                }
                m_streamedDataList.RemoveAt(0);
            }
        }
    }

    ulong GetUserIdFromPhotonInstantiationData()
    {
        PhotonView photonView = GetComponent<PhotonView>();
        object[] instantiationData = photonView.InstantiationData;
        Int64 data_as_int = (Int64)instantiationData[0];
        return Convert.ToUInt64(data_as_int);
        //return 5492165074224303;
    }


}
