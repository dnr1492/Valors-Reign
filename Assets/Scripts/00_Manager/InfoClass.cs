[System.Serializable]
public class CharacterInfo
{
    public int tokenKey;
    public int currentHp;
    public int currentMp;

    public bool IsDead => currentHp <= 0;
}
