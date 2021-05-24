using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState {
    Start,
    PlayerAction,
    PlayerMove,
    EnemyMove,
    Busy
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField]
    private BattleUnit playerUnit;
    [SerializeField]
    private BattleHud playerHud;
    [SerializeField]
    private BattleUnit enemyUnit;
    [SerializeField]
    private BattleHud enemyHud;

    [SerializeField]
    private BattleDialogBox dialogBox;

    private BattleState state;
    private int currentAction;
    private int currentMove;

    private void Start() {
        StartCoroutine(SetupBattle());
    }

    private void Update() {
        if (state == BattleState.PlayerAction) {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove) {
            HandleMoveSelection();
        }
    }

    public IEnumerator SetupBattle() {
        playerUnit.Setup();
        playerHud.SetData(playerUnit.Pokemon);
        enemyUnit.Setup();
        enemyHud.SetData(enemyUnit.Pokemon);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return StartCoroutine(dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared."));

        PlayerAction();
    }

    private void PlayerAction() {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Choose an action"));
        dialogBox.EnableActionSelector(true);
    }

    private void PlayerMove() {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    private IEnumerator PerformPlayerMove() {

        state = BattleState.Busy;

        Move move = playerUnit.Pokemon.Moves[currentMove];
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} used {move.Base.Name}");

        DamageDetails damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);

        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted) {
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} fainted");
        }
        else {
            StartCoroutine(EnemyMove());
        }
    }

    private IEnumerator EnemyMove() {
        state = BattleState.EnemyMove;

        Move move = enemyUnit.Pokemon.GetRandomMove();

        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} used {move.Base.Name}");

        DamageDetails damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted) {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} fainted");
        }
        else {
            PlayerAction();
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
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            if (currentAction < 1) {
                ++currentAction;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            if (currentAction > 0) {
                --currentAction;
            }
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z)) {
            if (currentAction == 0) {
                // Fight
                PlayerMove();
            }
            else if (currentAction == 1) {
                // Run
            }
        }
    }

    private void HandleMoveSelection() {

        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1) {
                currentMove += 1;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            if (currentMove > 0) {
                currentMove -= 1;
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2) {
                currentMove += 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            if (currentMove > 1) {
                currentMove -= 2;
            }
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z)) {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }
}
