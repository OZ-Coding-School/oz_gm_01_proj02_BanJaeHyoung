using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "GamblePark/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("기본 정보")]
    public string characterName;   // 이름
    public Sprite portrait;        // 초상화 이미지

    [Header("자산 정보")]
    public int initialMoney = 1000; // 초기 자금 (파산 후 복구될 금액)
}