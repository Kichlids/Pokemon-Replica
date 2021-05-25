using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState {
    FreeRoam,
    Battle
}

public class GameController : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController;
    [SerializeField]
    private BattleSystem battleSystem;
    [SerializeField]
    private Camera worldCamera;

    private GameState state;

    private void Awake() {
        ConditionsDB.Init();
    }

    private void Start() {
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
    }

    private void Update() {
        if (state == GameState.FreeRoam) {
            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle) {
            battleSystem.HandleUpdate();
        }
    }

    private void StartBattle() {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        PokemonParty playerParty = playerController.GetComponent<PokemonParty>();
        Pokemon wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        battleSystem.StartBattle(playerParty, wildPokemon);
    }

    private void EndBattle(bool won) {
        state = GameState.FreeRoam;

        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }
}
