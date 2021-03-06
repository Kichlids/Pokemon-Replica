using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField]
    private PokemonBase _base;
    [SerializeField]
    private int level;

    public PokemonBase Base {
        get {
            return _base;
        }
    }

    public int Level {
        get {
            return level;
        }
    }

    public int HP { get; set; }

    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }

    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();
    public bool HpChanged { get; set; }
    public event Action OnStatusChanged;

    public void  Init() {

        // Generate moves
        Moves = new List<Move>();
        foreach (LearnableMove move in Base.LearnableMoves) {
            if (move.Level <= Level) {
                Moves.Add(new Move(move.Base));
            }
            if (Moves.Count >= 4) {
                break;
            }
        }

        CalculateStats();
        HP = MaxHP;

        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }

    private void CalculateStats() {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHP = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10 + Level;
    }

    private void ResetStatBoost() {
        StatBoosts = new Dictionary<Stat, int>() {
            {Stat.Attack, 0 },
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0}
        };
    }

    private int GetStat(Stat stat) {
        int statVal = Stats[stat];

        // Apply stat boost
        int boostLevel = StatBoosts[stat];
        float[] boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boostLevel >= 0) {
            statVal = Mathf.FloorToInt(statVal * boostValues[boostLevel]);
        }
        else {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boostLevel]);
        }

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts) {
        foreach (StatBoost statBoost in statBoosts) {
            Stat stat = statBoost.stat;
            int boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0) {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            }
            else {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }

            Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    public int Attack {
        get { return GetStat(Stat.Attack); }
    }
    public int SpAttack {
        get { return GetStat(Stat.SpAttack); }
    }
    public int Defense {
        get { return GetStat(Stat.Defense); }
    }
    public int SpDefense {
        get { return GetStat(Stat.SpDefense); }
    }
    public int Speed {
        get { return GetStat(Stat.Speed); }
    }
    public int MaxHP { get; private set; }

    public DamageDetails TakeDamage(Move move, Pokemon attacker) {

        float critical = 1f;
        if (UnityEngine.Random.value * 100f <= 6.25f)
            critical = 2f;

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        DamageDetails damageDetails = new DamageDetails() {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = UnityEngine.Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * (attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);

        return damageDetails;
    }

    public void UpdateHP(int damage) {
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        HpChanged = true;
    }

    public void SetStatus(ConditionID conditionId) {
        if (Status != null)
            return;

        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
        return;
    }

    public void CureStatus() {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionId) {
        if (VolatileStatus != null)
            return;

        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus() {
        VolatileStatus = null;
    }

    public Move GetRandomMove() {

        List<Move> movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = UnityEngine.Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public bool OnBeforeMove() {
        bool canPerformMove = true;

        if (Status?.OnBeforeMove != null) {
            if (!Status.OnBeforeMove(this)) {
                canPerformMove = false;
            }
        }
        if (VolatileStatus?.OnBeforeMove != null) {
            if (!VolatileStatus.OnBeforeMove(this)) {
                canPerformMove = false;
            }
        }

        return canPerformMove;
    }

    public void OnAfterTurn() {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver() {
        VolatileStatus = null;
        ResetStatBoost();
    }
}

public class DamageDetails {
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}
