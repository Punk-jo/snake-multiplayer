using UnityEngine;

public class PlayerSettings : MonoBehaviour
{
    public static PlayerSettings Instance{get; private set;}

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if(Instance == this) Instance = null;
    }

    public string Login{get; private set;}

    public void SetLogin(string login)
    {
        Login = login;
    }
}
