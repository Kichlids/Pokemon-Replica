using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState {
    Start,
    ActionSelection,
    MoveSelection,
    RunningTurn,
    Busy,
    PartyScreen,
    BattleOver
}

public enum BattleAction {
    Move, SwitchPokemon, UseItem, Run
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField]
    private BattleUnit playerUnit;
    [SerializeField]
    private BattleUnit enemyUnit;
    [SerializeField]
    private BattleDialogBox dialogBox;
    [SerializeField]
    private PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    private BattleState state;
    private BattleState? prevState;
    private int currentAction;
    private int currentMove;
    private int currentMember;

    private PokemonParty playerParty;
    private Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon) {

        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;

        currentAction = 0;
        currentMove = 0;
        currentMember = 0;

        StartCoroutine(SetupBattle());
    }

    public void HandleUpdate() {
        if (state == BattleState.ActionSelection) {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection) {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen) {
            HandlePartySelection();
        }
    }

    public IEnumerator SetupBattle() {
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return StartCoroutine(dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared."));

        ActionSelection();
    }

    private void BattleOver(bool won) {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    private void ActionSelection() {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }

    private void OpenPartyScreen() {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    private void MoveSelection() {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }
    
    IEnumerator RunTurns(BattleAction playerAction) {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move) {

            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority; 
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            // Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority) {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority) {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }

            BattleUnit firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            BattleUnit secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            Pokemon secondPokemon = secondUnit.Pokemon;

            // First turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            //  End battle if battle is over
            if (state == BattleState.BattleOver)
                yield break;

            if (secondPokemon.HP > 0) {
                // Second turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver)
                    yield break;
            }
        }
        else {
            if (playerAction == BattleAction.SwitchPokemon) {
                Pokemon selectedPokemon = playerParty.Pokemons[currentMember];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }

            // Enemy turn
            Move enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver)
                yield break;
        }

        if (state != BattleState.BattleOver) {
            ActionSelection();
        }
    }

    private IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move) {

        // Check if pokemon can move
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove) {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

        // Check if move hits
        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon)) {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);

            targetUnit.PlayHitAnimation();

            // Run move
            if (move.Base.Category == MoveCategory.Status) {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
            }
            else {
                DamageDetails damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            // Run secondary effects
            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0) {
                
                foreach (SecondaryEffects secondary in move.Base.Secondaries) {
                    int random = UnityEngine.Random.Range(1, 101);
                    if (random <= secondary.Chance) {
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                    }
                }
            }

            // Check if attacks faints the pokemon
            if (targetUnit.Pokemon.HP <= 0) {
                yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} fainted");
                targetUnit.PlayFaintAnimation();

                yield return new WaitForSeconds(2f);

                CheckForBattleOver(targetUnit);
            }
        }
        else {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attack missed");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget) {

        // Stat Boosting
        if (effects.Boosts != null) {
            if (moveTarget == MoveTarget.Self) {
                source.ApplyBoosts(effects.Boosts);
            }
            else {
                target.ApplyBoosts(effects.Boosts);
            }
        }

        // Status Condition
        if (effects.Status != ConditionID.none) {
            target.SetStatus(effects.Status);
        }
        // Volatile Status Condition
        if (effects.VolatileStatus != ConditionID.none) {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    private IEnumerator RunAfterTurn(BattleUnit sourceUnit) {

        if (state == BattleState.BattleOver)
            yield break;

        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        // Check if status faints the pokemon after turn
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();

        if (sourceUnit.Pokemon.HP <= 0) {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} fainted");
            sourceUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }
    }

    private bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target) {

        if (move.Base.AlwaysHits) {
            return true;
        }

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        float[] boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0) {
            moveAccuracy *= boostValues[accuracy];
        }
        else {
            moveAccuracy /= boostValues[-accuracy];
        }

        if (evasion > 0) {
            moveAccuracy /= boostValues[evasion];
        }
        else {
            moveAccuracy *= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    private IEnumerator ShowStatusChanges(Pokemon pokemon) {
        
        while (pokemon.StatusChanges.Count > 0) {
            string message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    private void CheckForBattleOver(BattleUnit faintedUnit) {
        if (faintedUnit.IsPlayerUnit) {
            Pokemon nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null) {
                OpenPartyScreen();
            }
            else {
                BattleOver(false);
            }
        }
        else {
            BattleOver(true);
        }
    }

    private IEnumerator ShowDamageDetails(DamageDetails damageDetails) {
        if (damageDetails.Critical > 1f) {
            yield return dialogBox.TypeDialog("A critical hit!");
        }
        if (damageDetails.TypeEffectiveness > 1) {
            yield return dialogBox.TypeDialog("It's super effective!");
        }
        else if (damageDetails.TypeEffectiveness < 1) {
            yield return dialogBox.TypeDialog("It's not very effective");
        }
    }

    private void HandleActionSelection() {
        
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            currentAction += 1;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            currentAction -= 1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z)) {
            if (currentAction == 0) {
                // Fight
                MoveSelection();
            }
            else if (currentAction == 1) {
                // Bag
            }
            else if (currentAction == 2) {
                // Pokemon
                prevState = state;
                OpenPartyScreen();
            }
            else if (currentAction == 3) {
                // Run
            }
        }
    }

    private void HandleMoveSelection() {

        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            currentMove += 1;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            currentMove -= 1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z)) {
            Move move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0)
                return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.X)) {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    private void HandlePartySelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            currentMember += 1;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            currentMember -= 1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z)) {
            Pokemon selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0) {
                partyScreen.SetMessageText("You can't send out a fainted Pokemon");
                return;
            }
            if (selectedMember == playerUnit.Pokemon) {
                partyScreen.SetMessageText("You can't switch with the same Pokemon");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            // Player chooses to switch pokemon
            if (prevState == BattleState.ActionSelection) {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            // Pokemon has fainted so player must switch to another pokemon
            else {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedMember));
            }
        }
        else if (Input.GetKeyDown(KeyCode.X)) {
            partyScreen.gameObject.SetActive(false); 
            ActionSelection();
        }
    }

    private IEnumerator SwitchPokemon(Pokemon newPokemon) {

        if (playerUnit.Pokemon.HP > 0) {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlaySwitchAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return StartCoroutine(dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!"));

        state = BattleState.RunningTurn;
    }
}
