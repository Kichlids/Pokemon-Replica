using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    
    [SerializeField]
    private bool isPlayerUnit;
    [SerializeField]
    private BattleHud hud;

    public bool IsPlayerUnit {
        get { return isPlayerUnit; }
    }

    public BattleHud Hud {
        get { return hud; }
    }

    private Image image;
    private Vector3 originalPos;
    private Color originalColor;

    private void Awake() {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }

    public Pokemon Pokemon { get; set; }

    public void Setup(Pokemon pokemon) {
        Pokemon = pokemon;

        if (isPlayerUnit) {
           image.sprite = Pokemon.Base.BackSprite1;
        }
        else {
            image.sprite = Pokemon.Base.FrontSprite1;
        }

        hud.SetData(pokemon);

        image.color = originalColor;
        StartCoroutine(PlayEnterAnimation());
    }

    public IEnumerator PlayEnterAnimation() {
        if (isPlayerUnit) {
            image.transform.localPosition = new Vector3(-500f, originalPos.y);
        }
        else {
            image.transform.localPosition = new Vector3(500f, originalPos.y);
        }

        image.transform.DOLocalMoveX(originalPos.x, 1f);

        yield return new WaitForSeconds(1f);
        yield return PlayEncounterAnimation();
    }

    public IEnumerator PlayEncounterAnimation() {
        if (isPlayerUnit) {
            image.sprite = Pokemon.Base.BackSprite1;
            yield return new WaitForSeconds(0.5f);
            image.sprite = Pokemon.Base.BackSprite2;
            yield return new WaitForSeconds(0.5f);
            image.sprite = Pokemon.Base.BackSprite1;
        }
        else {
            image.sprite = Pokemon.Base.FrontSprite1;
            yield return new WaitForSeconds(0.5f);
            image.sprite = Pokemon.Base.FrontSprite2;
            yield return new WaitForSeconds(0.5f);
            image.sprite = Pokemon.Base.FrontSprite1;
        }
    }

    public void PlayAttackAnimation() {
        Sequence sequence = DOTween.Sequence();

        if (isPlayerUnit) {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        }
        else {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));
        }

        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
    }

    public void PlayHitAnimation() {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }

    public void PlayFaintAnimation() {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }

    public void PlaySwitchAnimation() {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 500f, 1f));
    }
}
