using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="Move", menuName="Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField]
    private string name;
    [SerializeField]
    private string description;
    [SerializeField]
    private PokemonType type;
    [SerializeField]
    private int power;
    [SerializeField]
    private int accuracy;
    [SerializeField]
    private int pp;
    [SerializeField]
    private MoveCategory category;
    [SerializeField]
    private MoveEffects effects;
    [SerializeField]
    private MoveTarget target;

    public string Name {
        get { return name; }
    }
    public string Description {
        get { return description; }
    }
    public PokemonType Type {
        get { return type; }
    }
    public int Power {
        get { return power; }
    }
    public int Accuracy {
        get { return accuracy; }
    }
    public int PP {
        get { return pp; }
    }
    public MoveCategory Category {
        get { return category; }
    }

    public MoveEffects Effects {
        get { return effects; }
    }

    public MoveTarget Target {
        get { return target; }
    }
}

[System.Serializable]
public class MoveEffects {
    [SerializeField]
    private List<StatBoost> boosts;
    [SerializeField]
    private ConditionID status;
    [SerializeField]
    private ConditionID volatileStatus;

    public List<StatBoost> Boosts {
        get { return boosts; }
    }

    public ConditionID Status {
        get { return status; }
    }

    public ConditionID VolatileStatus {
        get { return volatileStatus; }
    }

}

[System.Serializable]
public class StatBoost {
    public Stat stat;
    public int boost;
}

public enum MoveCategory {
    Physical,
    Special,
    Status
}

public enum MoveTarget {
    Foe,
    Self
}
