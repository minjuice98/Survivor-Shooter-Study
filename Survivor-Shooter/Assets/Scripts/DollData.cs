using UnityEngine;

[CreateAssetMenu(fileName = "DollData", menuName = "Scriptable Objects/DollData")]
public class DollData : ScriptableObject
{
    public GameObject dollPrefab;
    public int maxHp;
    public int damage;
    public int speed;

    // Animator Controller(공통 컨트롤러 또는 각 개체 전용 컨트롤러)
    public RuntimeAnimatorController controller;
}
