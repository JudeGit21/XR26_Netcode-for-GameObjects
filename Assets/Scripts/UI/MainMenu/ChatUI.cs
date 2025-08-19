using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class ChatUi : MonoBehaviour
{
    public static ChatUi Instance {   private set; get; }


     public TMP_InputField chatInput;

     public GameObject chatMsgPrefab;

     public Transform content;

    public UnityEvent OnMessageSubmit;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        chatInput.onSubmit.AddListener(t =>
        {
            OnMessageSubmit.Invoke();

        });
    }




    public void CreateChatMessage(string name, string msg)
    {
        var chatMsgObject = Instantiate(chatMsgPrefab, content, false);
        chatMsgObject.GetComponent<TMP_Text>().text = $"[{name}]: {msg}";
    }
}