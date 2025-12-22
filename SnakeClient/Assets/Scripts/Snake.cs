
using UnityEngine;

public class Snake : MonoBehaviour
{
    public float Speed{get{return _speed;}}
    [SerializeField] private Tail _tailPrefab;
    [SerializeField] private Transform _head;
    [SerializeField] private float _speed = 2;
    private Tail _tail;

    public void Init(int detailCount, Material skinMaterial)
    {
        GetComponent<SetSkin>()?.Set(skinMaterial);

        _tail = Instantiate(_tailPrefab, transform.position, Quaternion.identity);
        _tail.Init(_head, _speed, detailCount, skinMaterial);
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

    public void Destroy()
    {
        _tail.Destroy();
        Destroy(gameObject);
    }
}
