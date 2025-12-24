
using TMPro;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public float Speed{get{return _speed;}}
    [SerializeField] private Tail _tailPrefab;
    [SerializeField] private int _playerLayer = 7;
    [field:SerializeField] public Transform _head{get;private set;}
    [SerializeField] private float _speed = 2;
    private Tail _tail;
    [SerializeField]private TextMeshProUGUI _loginName;

    public void Init(int detailCount, Material skinMaterial,string login, bool isPlayer = false)
    {
        if (isPlayer)
        {
            gameObject.layer = _playerLayer;
            var childrens = GetComponentsInChildren<Transform>();
            for(int i =0; i< childrens.Length; i++)
            {
                childrens[i].gameObject.layer = _playerLayer;
            }
        }
        GetComponent<SetSkin>()?.Set(skinMaterial);
        _loginName.text = login;

        _tail = Instantiate(_tailPrefab, transform.position, Quaternion.identity);
        _tail.Init(_head, _speed, detailCount-2, skinMaterial, _playerLayer, isPlayer);
    }
    void Update()
    {
        Move();
    }

    public void SetDetailCount(int detailCount)
    {
        _tail.SetDetailCount(detailCount);
    }
    public void SetRotation(Vector3 pointToLook)
    {

        _head.LookAt(pointToLook);
    }

    private Vector3 _targetDirection = Vector3.zero;
    private void Move()
    {
        transform.position += _head.forward * Time.deltaTime * _speed;
    }

    public void Destroy(string clientID)
    {
        var detailPosition = _tail.GetDetailPositions();
        detailPosition.id = clientID;
        string json = JsonUtility.ToJson(detailPosition);
        MultiplayerManager.Instance.SendMessage("gameOver", json);
        _tail.Destroy();
        Destroy(gameObject);
    }
}
