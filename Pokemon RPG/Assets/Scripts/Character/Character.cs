using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float moveSpeed;

    public bool IsMoving { get; private set; }

    private CharacterAnimator animator;

    public CharacterAnimator Animator {
        get => animator;
    }

    private void Awake() {
        animator = GetComponent<CharacterAnimator>();
    }

    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver=null) {

        // Set animation
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        Vector3 targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if (!IsWalkable(targetPos))
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

    private bool IsWalkable(Vector2 targetPos) {
        if (Physics2D.OverlapCircle(targetPos, 0.05f, GameLayers.Instance.SolidLayer | GameLayers.Instance.InteractableLayer) != null) {
            return false;
        }
        return true;
    }
}
