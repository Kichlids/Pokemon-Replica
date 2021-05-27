using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    public float moveSpeed;

    public bool IsMoving { get; private set; }

    private CharacterAnimator animator;

    public CharacterAnimator Animator {
        get => animator;
    }

    private void Awake() {
        animator = GetComponent<CharacterAnimator>();
    }

    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver = null) {

        // Set animation
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        Vector3 targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if (!IsPathClear(targetPos))
            yield break;

        IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon) {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;

        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate() {
        animator.IsMoving = IsMoving;
    }

    private bool IsPathClear(Vector3 targetPos) {
        Vector3 diff = targetPos - transform.position;
        Vector3 dir = diff.normalized;

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1,
            GameLayers.Instance.SolidLayer | GameLayers.Instance.InteractableLayer | GameLayers.Instance.PlayerLayer)) {
            // Path not clear
            return false;
        }
        return true;
    }

    private bool IsWalkable(Vector2 targetPos) {
        if (Physics2D.OverlapCircle(targetPos, 0.05f, GameLayers.Instance.SolidLayer | GameLayers.Instance.InteractableLayer) != null) {
            return false;
        }
        return true;
    }

    public void LookTowards(Vector3 targetPos) {
        float dx = (Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x));
        float dy = (Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y));

        if (dx == 0 || dy == 0) {
            animator.MoveX = Mathf.Clamp(dx, -1f, 1f);
            animator.MoveY = Mathf.Clamp(dy, -1f, 1f);
        }
        else {
            Debug.LogError("Error in Look Towards: You can't ask the character to look diagonally");
        }
    }
}
