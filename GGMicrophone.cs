using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Voice;
using UnityEngine.SceneManagement;

public class GGMicrophone : MonoBehaviour 
{ 
    [SerializeField] AudioClip m_audioClip; 
    [Tooltip("Enable to see the Mic Input Volume")]
    [SerializeField] bool m_debugMicrophone = true; 
    [Range(0f, 1f)] [SerializeField] float m_micInputVolume = 0f; 
    [Range(0, 1000000)] [SerializeField] int m_microphonePosition; 
    [SerializeField] int m_audioClipId; 
    [SerializeField] float m_microphoneSensitivity = 50f; 
    [SerializeField] float m_threshold = 0.1f; int m_samplingRate = 48000; 


    int m_sampleWindow = 64; int m_audioClipLength = 1; 
    static private GGMicrophone m_instance; 
    static public GGMicrophone Instance { get { return m_instance; } } 


    private void Awake() 
    { 
        if (m_instance == null) 
        { 
            m_instance = this; 
        } else 
        { 
            Destroy(this.gameObject); } 
        } 
        
    [ContextMenu("Start Microphone")] 
    public void StartMicrophone() 
    { 
        string microphoneName = GetMicrophoneDeviceName(); 
        m_audioClip = Microphone.Start(microphoneName, true, m_audioClipLength, m_samplingRate); 
        string clipId = m_audioClip.GetInstanceID().ToString(); 
        m_audioClip.name = "GGMicAudioClip_" + clipId; 
    } 
    
    [ContextMenu("Stop Microphone")] 
    public void StopMicrophone() 
    { 
        string microphoneName = GetMicrophoneDeviceName(); 
        Microphone.End(microphoneName); 
    } 
    
    public bool IsRecording() 
    { 
        string microphoneName = GetMicrophoneDeviceName(); 
        return Microphone.IsRecording(microphoneName); 
    } 
    
    public int GetPosition() 
    { 
        string microphoneName = GetMicrophoneDeviceName(); 
        return Microphone.GetPosition(microphoneName); 
    } 
    
    public string GetMicrophoneDeviceName() 
    { 
        return Microphone.devices[0]; 
    } 
    
    public AudioClip GetMicrophoneAudioClip() 
    { 
        return m_audioClip; 
    } 
    
    private void Start() 
    { 
        StartMicrophone(); 
        SceneManager.sceneLoaded += OnSceneLoaded; 
    } 
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    { 
        StartMicrophone(); 
    } 
    
    private void Update() 
    { 
        if (m_debugMicrophone == true) 
        { 
            if ((m_audioClip != null)) 
            { 
                float micInputVolume = GetVolumeFromMicrophone() * m_microphoneSensitivity; 
                if (micInputVolume < m_threshold) 
                { 
                    micInputVolume = 0; 
                } 
                
                m_micInputVolume = micInputVolume; 
                m_audioClipId = m_audioClip.GetInstanceID(); 
            } 
            
            m_microphonePosition = GetPosition(); 
        } 
    } 
    
    public float GetVolumeFromMicrophone() 
    { 
        return GetVolumeFromAudioClip(Microphone.GetPosition(Microphone.devices[0]), m_audioClip);
    } 
    
    float GetVolumeFromAudioClip(int clipPosition, AudioClip audioClip) 
    { 
        if (clipPosition - m_sampleWindow < 0) { return 0; } float[] data = new float[m_sampleWindow]; 
        audioClip.GetData(data, clipPosition - m_sampleWindow); float totalVolume = 0; 
        
        for (int i = 0; i < data.Length; i++) 
        { 
            totalVolume += Mathf.Abs(data[i]); 
        } 
        
        float averageVolume = totalVolume / m_sampleWindow; 
        return averageVolume; 
    } 
    public void EnableMicrophoneDebugger(bool enabled) 
    { 
        m_debugMicrophone = enabled; 
    } 
    
    public void UpdateAudioClipLength(float clipLength) 
    { 
        m_audioClipLength = (int)clipLength; 
    } 
    
    public void SetSamplingRate(int samplingRate) 
    { 
        m_samplingRate = samplingRate; 
    } 
}
