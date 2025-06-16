using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : Singleton<SpriteManager>
{
    protected override void Awake()
    {
        base.Awake();

        LoadSprites();
    }

    public Dictionary<string, Sprite> dicCharacterOriginSprite = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> dicCharacterEnlargeSprite = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> dicSkillSprite = new Dictionary<string, Sprite>();

    public void LoadSprites()
    {
        var origin = Resources.LoadAll<Sprite>("Sprites/Character/Origin");
        foreach (var sprite in origin) {
            dicCharacterOriginSprite.Add(sprite.name, sprite);
        }

        var enlarge = Resources.LoadAll<Sprite>("Sprites/Character/Enlarge");
        foreach (var sprite in enlarge) {
            dicCharacterEnlargeSprite.Add(sprite.name, sprite);
        }

        var skill = Resources.LoadAll<Sprite>("Sprites/Skill");
        foreach (var sprite in skill) {
            dicSkillSprite.Add(sprite.name, sprite);
        }
    }
}
