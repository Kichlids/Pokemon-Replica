using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState {
    Start,
    PlayerAction,
    PlayerMove,
    EnemyMove,
    Busy,
    PartyScreen
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
    [SerializeField]
    private PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    private BattleState state;
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
        if (state == BattleState.PlayerAction) {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove) {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen) {
            HandlePartySelection();
        }
    }

    public IEnumerator SetupBattle() {
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        playerHud.SetData(playerUnit.Pokemon);
        enemyUnit.Setup(wildPokemon);
        enemyHud.SetData(enemyUnit.Pokemon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return StartCoroutine(dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared."));

        PlayerAction();
    }

    private void PlayerAction() {
        state = BattleState.PlayerAction;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }

    private void OpenPartyScreen() {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
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
        move.PP--;

        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} used {move.Base.Name}");

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayHitAnimation();
        DamageDetails damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted) {
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} fainted");
            enemyUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        }
        else {
            StartCoroutine(EnemyMove());
        }
    }

    private IEnumerator EnemyMove() {
        state = BattleState.EnemyMove;

        Move move = enemyUnit.Pokemon.GetRandomMove();
        move.PP--;

        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} used {move.Base.Name}");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        playerUnit.PlayHitAnimation();
        DamageDetails damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted) {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} fainted");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            Pokemon nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null) {
                OpenPartyScreen();
            }
            else {
                OnBattleOver(false);
            }
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
                PlayerMove();
            }
            else if (currentAction == 1) {
                // Bag
            }
            else if (currentAction == 2) {
                // Pokemon
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
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X)) {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            PlayerAction();
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
            state = BattleState.Busy;

            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.X)) {
            partyScreen.gameObject.SetActive(false);

            PlayerAction();
        }
    }

    private IEnumerator SwitchPokemon(Pokemon newPokemon) {

        if (playerUnit.Pokemon.HP > 0) {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlaySwitchAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        playerHud.SetData(newPokemon);

        dialogBox.SetMoveNames(newPokemon.Moves);

        yield return StartCoroutine(dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!"));

        StartCoroutine(EnemyMove());
    }
}
