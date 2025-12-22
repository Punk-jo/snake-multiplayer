using System;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    private float _speed;
    [SerializeField] private float _rotateSpeed  = 90f;
    private Vector3 _targetDirection = Vector3.zero;

    public void Init(float speed)
    {
        _speed = speed;
    }
    void Update()
    {
        Move();
        Rotate();
    }

    private void Rotate()
    {
       Quaternion targetRotation = Quaternion.LookRotation(_targetDirection);
       transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * _rotateSpeed);
    }

    private void Move()
    {
        transform.position += transform.forward * _speed * Time.deltaTime; 
    }

    public void SetTargetDirection(Vector3 pointToLook)
    {
        _targetDirection = pointToLook - transform.position;
    }
    public void GetMoveInfo(out Vector3 position)
    {
        position = transform.position;
    }
}
