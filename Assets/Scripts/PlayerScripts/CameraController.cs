using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform _camera;
    [SerializeField] Transform _player;
    public static bool canMove = false;

    private void LateUpdate()
    {
        if (!canMove) return;
        Vector3 camPos = new Vector3(_player.position.x, _player.position.y, _camera.position.z);
        _camera.position = camPos;
    }
}
