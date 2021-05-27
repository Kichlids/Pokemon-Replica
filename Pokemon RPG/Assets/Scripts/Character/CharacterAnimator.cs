using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField]
    private List<Sprite> walkDownSprites;
    [SerializeField]
    private List<Sprite> walkUpSprites;
    [SerializeField]
    private List<Sprite> walkRightSprites;
    [SerializeField]
    private List<Sprite> walkLeftSprites;


    // Parameters
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }

    // States
    private SpriteAnimator walkDownAnim;
    private SpriteAnimator walkUpAnim;
    private SpriteAnimator walkRightAnim;
    private SpriteAnimator walkLeftAnim;

    private SpriteAnimator currentAnim;
    private bool wasPreviouslyMoving;

    // References
    private SpriteRenderer spriteRenderer;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();

        walkDownAnim = new SpriteAnimator(spriteRenderer, walkDownSprites);
        walkUpAnim = new SpriteAnimator(spriteRenderer, walkUpSprites);
        walkRightAnim = new SpriteAnimator(spriteRenderer, walkRightSprites);
        walkLeftAnim = new SpriteAnimator(spriteRenderer, walkLeftSprites);

        currentAnim = walkDownAnim;
    }

    private void Update() {

        SpriteAnimator prevAnim = currentAnim;

        if (MoveX == 1) {
            currentAnim = walkRightAnim;
        }
        else if (MoveX == -1) {
            currentAnim = walkLeftAnim;
        }
        else if (MoveY == 1) {
            currentAnim = walkUpAnim;
        }
        else if (MoveY == -1) {
            currentAnim = walkDownAnim;
        }

        if (currentAnim != prevAnim || wasPreviouslyMoving != IsMoving) {
            currentAnim.Start();
        }

        if (IsMoving) {
            currentAnim.HandleUpdate();
        }
        else {
            spriteRenderer.sprite = currentAnim.Frames[0];
        }

        wasPreviouslyMoving = IsMoving;
    }
}
