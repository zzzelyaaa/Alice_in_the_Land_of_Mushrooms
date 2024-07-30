using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform _camera;
    [SerializeField] Transform _player;

    private void LateUpdate()
    {
        Vector3 camPos = new Vector3(_player.position.x, _player.position.y, _camera.position.z);
        _camera.position = camPos;
    }
}
