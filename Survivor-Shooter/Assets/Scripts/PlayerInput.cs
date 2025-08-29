using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class PlayerInput : MonoBehaviour
{
    public string verticalAxis = "Vertical";
    public string horizontalAxis = "Horizontal";
    public string fire = "Fire1";

    public float move { get; private set; }
    public float rotate { get; private set; }
    public bool fire1 { get; private set; }

    private void Update()
    {
        move = Input.GetAxis(verticalAxis);
        rotate = Input.GetAxis(horizontalAxis);
        fire1 = Input.GetButton(fire);
    }
}
