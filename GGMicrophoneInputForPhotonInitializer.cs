using Oculus.Platform; 
using Photon.Pun; 
using Photon.Voice.PUN; 
using Photon.Voice.Unity; 
using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 

public class GGMicrophoneInputForPhotonInitializer : MonoBehaviour 
{

    [SerializeField] private Recorder recorder; 
    private void Start() 
    { 
        if (recorder != null) 
        { 
            recorder.SourceType = Recorder.InputSourceType.Factory; 
            recorder.InputFactory = () => new GGMicrophoneInputForPhoton(); 
        } 
        else 
        { 
            Debug.LogError("Could not set recorder's input source type because no recorder was found."); 
        } 
    } 
}
