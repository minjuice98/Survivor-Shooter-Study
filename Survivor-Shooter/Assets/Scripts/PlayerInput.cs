using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class PlayerInput : MonoBehaviour
{
    public string verticalAxis = "Vertical";
    public string horizontalAxis = "Horizontal";
    public string fire = "Fire1";

    public float moveZ { get; private set; }
    public float moveX { get; private set; }
    public bool fire1 { get; private set; }

    private void Update()
    {
        moveZ = Input.GetAxis(verticalAxis);
        moveX = Input.GetAxis(horizontalAxis);
        fire1 = Input.GetButton(fire);
    }
}
