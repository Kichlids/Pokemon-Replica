using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]
public class PokemonBase : ScriptableObject {
    [SerializeField]
    private string name;
    [SerializeField]
    private int number;
    [SerializeField]
    private string description;
    [SerializeField]
    private Sprite frontSprite1;
    [SerializeField]
    private Sprite frontSprite2;
    [SerializeField]
    private Sprite backSprite1;
    [SerializeField]
    private Sprite backSprite2;
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
    public Sprite FrontSprite1 {
        get { return frontSprite1;  }
        set { }
    }
    public Sprite FrontSprite2 {
        get { return frontSprite2; }
        set { }
    }
    public Sprite BackSprite1 {
        get { return backSprite1; }
        set { }
    }
    public Sprite BackSprite2 {
        get { return backSprite2; }
        set { }
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

public enum Stat {
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,

    // These 2 aren't actual stats; they're used to boost move accuracy  
    Accuracy,
    Evasion
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

public class TypeChart {
    static float[][] chart = {
        //                  NOR FIR WAT ELE GRA ICE FIG POI GRO FLY PSY BUG ROC GHO DRA DAR STE FAI
        /*NOR*/new float[] {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 0f, 1f, 1f, 0.5f, 1f},
        /*FIR*/new float[] {1f, 0.5f, 0.5f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 0.5f, 1f, 2f, 1f},
        /*WAT*/new float[] {1f, 2f, 0.5f, 2f, 0.5f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f, 1f, 1f},
        /*ELE*/new float[] {1f, 1f, 2f, 0.5f, 0.5f, 2f, 1f, 1f, 0f, 2f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f, 1f},
        /*GRS*/new float[] {1f, 0.5f, 2f, 2f, 0.5f, 1f, 1f, 0.5f, 2f, 0.5f, 1f, 0.5f, 2f, 1f, 0.5f, 1f, 0.5f, 1f},
        /*ICE*/new float[] {1f, 0.5f, 0.5f, 1f, 2f, 0.5f, 1f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f},
        /*FIG*/new float[] {2f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f, 0.5f, 0.5f, 0.5f, 2f, 0f, 1f, 2f, 2f, 0.5f, 1f},
        /*POI*/new float[] {1f, 1f, 1f, 1f, 2f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 0f, 2f},
        /*GRO*/new float[] {1f, 2f, 1f, 2f, 0.5f, 1f, 1f, 2f, 1f, 0f, 1f, 0.5f, 2f, 1f, 1f, 1f, 2f, 1f},
        /*FLY*/new float[] {1f, 1f, 1f, 0.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 1f, 1f, 0.5f, 1f},
        /*PSY*/new float[] {1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 1f, 1f, 0.5f, 1f, 1f, 1f, 1f, 0f, 0.5f, 1f},
        /*BUG*/new float[] {1f, 0.5f, 1f, 1f, 2f, 1f, 0.5f, 0.5f, 1f, 0.5f, 2f, 1f, 1f, 0.5f, 1f, 2f, 0.5f, 0.5f},
        /*ROC*/new float[] {1f, 2f, 1f, 1f, 1f, 2f, 0.5f, 1f, 0.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 0.5f, 1f},
        /*GHO*/new float[] {0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f, 0.5f, 1f, 1f},
        /*DRA*/new float[] {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 0f},
        /*DAR*/new float[] {1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f, 0.5f, 1f, 0.5f},
        /*STE*/new float[] {1f, 0.5f, 0.5f, 0.5f, 1f, 2f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 0.5f, 2f},
        /*POI*/new float[] {1f, 0.5f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 0.5f, 1f}
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType) {
        if (attackType == PokemonType.None || defenseType == PokemonType.None) {
            return 1;
        }

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}
