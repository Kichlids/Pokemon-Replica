using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]
public class PokemonBase : ScriptableObject {
    [SerializeField]
    private string name;
    [SerializeField]
    private string description;
    [SerializeField]
    private Sprite frontSprite;
    [SerializeField]
    private Sprite backSprite;
    [SerializeField]
    private PokemonType type1;
    [SerializeField]
    private PokemonType type2;

    // Base stats
    [SerializeField]
    private int maxHP;
    [SerializeField]
    private int attack;
    [SerializeField]
    private int defense;
    [SerializeField]
    private int spAttack;
    [SerializeField]
    private int spDefense;
    [SerializeField]
    private int speed;

    [SerializeField]
    private List<LearnableMove> learnableMoves;


    public string Name {
        get { return name; }
    }
    public string Description {
        get { return description; }
    }
    public Sprite FrontSprite {
        get { return frontSprite; }
    }
    public Sprite BackSprite {
        get { return backSprite; }
    }
    public PokemonType Type1 {
        get { return type1; }
    }
    public PokemonType Type2 {
        get { return type2; }
    }
    public int MaxHP {
        get { return maxHP; }
    }
    public int Attack {
        get { return attack; }
    }
    public int SpAttack {
        get { return spAttack; }
    }
    public int Defense {
        get { return defense; }
    }
    public int SpDefense {
        get { return spDefense; }
    }
    public int Speed {
        get { return speed; }
    }
    public List<LearnableMove> LearnableMoves {
        get { return learnableMoves; }
    }
}

[System.Serializable]
public class LearnableMove {
    [SerializeField]
    private MoveBase moveBase;
    [SerializeField]
    private int level;

    public MoveBase Base {
        get { return moveBase; }
    }
    public int Level {
        get { return level; }
    }
}

public enum PokemonType {
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon
}
