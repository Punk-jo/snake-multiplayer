using UnityEngine;

public class SetSkin : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] _meshRenders;
    public void Set(Material material)
    {
        for(int i = 0; i<_meshRenders.Length; i++)
        {
            _meshRenders[i].material = material;
        }
    }
}
