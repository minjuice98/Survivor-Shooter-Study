using UnityEngine;

[CreateAssetMenu(fileName = "DollData", menuName = "Scriptable Objects/DollData")]
public class DollData : ScriptableObject
{
    public GameObject dollPrefab;
    public int maxHp;
    public int damage;
    public int speed;
}
