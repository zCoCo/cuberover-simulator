using UnityEngine;

// Modified from: https://github.com/off99555/Unity3D-Python-Communication.git (commit d396c2f)

public class ZMQClient : MonoBehaviour
{
    private ZMQSocket _helloRequester;

    private void Start()
    {
        _helloRequester = new ZMQSocket();
        _helloRequester.Start();
    }

    private void OnDestroy()
    {
        _helloRequester.Stop();
    }
}