using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField]
    private Text messageText;

    private PartyMemberUI[] memberSlots;
    private List<Pokemon> pokemons;

    public void Init() {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyData(List<Pokemon> pokemons) {

        this.pokemons = pokemons;

        for (int i = 0; i < memberSlots.Length; i++) {
            if (i < pokemons.Count) {
                memberSlots[i].SetData(pokemons[i]);
            }
            else {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        messageText.text = "Choose a Pokemon";
    }

    public void UpdateMemberSelection(int selectedMember) {
        for (int i = 0; i < pokemons.Count; i++) {
            memberSlots[i].SetSelected(i == selectedMember);
        }
    }

    public void SetMessageText(string message) {
        messageText.text = message;
    }
}
