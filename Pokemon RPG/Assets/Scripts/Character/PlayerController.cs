using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public event Action OnEncountered;

    private Vector2 input;

    private Character character;

    private void Awake() {
        character = GetComponent<Character>();
    }

    public void HandleUpdate() {
        if (!character.IsMoving) {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
               
            // Disable diagonal movement
            if (input.x != 0)
                input.y = 0;

            if (input != Vector2.zero) {
                StartCoroutine(character.Move(input, CheckForEncounters));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z)) {
            Interact();
        }
    }

    private void CheckForEncounters() {
        if (Physics2D.OverlapCircle(transform.position, 0.1f, GameLayers.Instance.GrassLayer) != null) {
            if (UnityEngine.Random.Range(1, 101) < 10) {
                character.Animator.IsMoving = false;
                Debug.Log("Encountered a wild pokemon");

                OnEncountered();
            }
        }
    }

    private void Interact() {
        Vector3 facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        Vector3 interactPos = transform.position + facingDir;

        //Debug.DrawLine(transform.position, interactPos, Color.black, 0.5f);

        Collider2D collider = Physics2D.OverlapCircle(interactPos, 0.2f, GameLayers.Instance.InteractableLayer);
        if (collider != null) {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }
}
