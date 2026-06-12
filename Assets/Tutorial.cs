using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Sequence = DG.Tweening.Sequence;
using Image = UnityEngine.UI.Image;

public class Tutorial : MonoBehaviour
{
    [Header("Settings")]
    public float arrowOffset = 3f;

    [SerializeField] private Transform tutorial;
    [SerializeField] private Transform guide;
    [SerializeField] private TMP_Text guideT;
    [SerializeField] private Transform arrow;

    private Dictionary<int, Action> tutorialMap;

    [SerializeField] private GameObject levelDisplay;
    [SerializeField] private GameObject coinDisplay;
    [SerializeField] private GameObject boosterDisplay;
    [SerializeField] private InputHandler input;

    //Tutorial Items
    [SerializeField] private BottleGen bottleGen;
    public readonly LanguageTrans lang = new();


    //Common variables
    private Vector2 centre = new(Screen.width / 2, Screen.height / 2);
    private List<Bottle> bottles = new();
    private Image arrowImg;

    void Awake() {
        arrowImg = arrow.GetComponent<Image>();

        tutorialMap = new() {
            { 0, () => {StartCoroutine(Level0()); } }, //Introduction
            { 1, () => {StartCoroutine(Level1()); } }, //Level tracking
            { 2, () => {StartCoroutine(Level2()); } }, //Coin introduction/tracking
            { 5, () => {StartCoroutine(Level5()); } }, //Boosters introduction

            { 15, () => { } }, //Mystery colors
            { 30, () => { } } //Cover introduction
        };
    }

    public void CheckForTutorial() {
        if (tutorialMap.TryGetValue(SaveManager.Instance.level, out Action method)) {
            input.ToggleTutorialMode();
            method?.Invoke();
        }
    }

    private void Tutorialize(bool set) {
        tutorial.gameObject.SetActive(set);
    }

    private Vector2 ArrowOffsetCalc(Vector2 targetScreenPos, float offset, bool dir) {
        Vector2 dirFromCenter;
        if (dir) {
            dirFromCenter = (targetScreenPos - centre).normalized;
        } else {
            dirFromCenter = (centre - targetScreenPos).normalized;
        }

        return targetScreenPos + dirFromCenter * offset;
    }
    private float ArrowRotation(Vector2 start, Vector2 end) {
        Vector2 direction = end - start;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private IEnumerator TextType(string text) {
        guideT.text = text;
        guideT.maxVisibleCharacters = 0;

        foreach (char c in text) {
            if (input.CheckForInput()) {
                guideT.maxVisibleCharacters = text.Length;
                yield break;
            }

            guideT.maxVisibleCharacters++;
            yield return new WaitForSeconds(.06f);
        }
    }

    private IEnumerator Level0() {

        levelDisplay.SetActive(false);
        coinDisplay.SetActive(false);
        boosterDisplay.SetActive(false);

        yield return new WaitForSeconds(1f);
        bottles = bottleGen.DictionaryToSingularBottleConverter();

        Tutorialize(true);
        Vector2 firstBottleCoord = Camera.main.WorldToScreenPoint(
            bottles[0].transform.position
            );

        Vector2 secondBottleCoord = Camera.main.WorldToScreenPoint(
            bottles[2].transform.position
            );

        Vector2 point1Pos = ArrowOffsetCalc(firstBottleCoord, arrowOffset, true);
        Vector2 point2Pos = ArrowOffsetCalc(secondBottleCoord, arrowOffset, true);

        float arrowRot1 = ArrowRotation(point1Pos, firstBottleCoord);
        float arrowRot2 = ArrowRotation(point2Pos, secondBottleCoord);

        guide.DOMove(
            new Vector2(centre.x, centre.y * .6f),
            .4f
            ).From(new Vector2(centre.x, centre.y * -1f))
            .SetEase(Ease.OutBack, 2f);

        yield return StartCoroutine(TextType(lang.hello));
        yield return null;
        yield return input.WaitForAction();

        Sequence seq;
        
        seq = DOTween.Sequence();

        seq.Append(
            arrowImg.DOFade(1f, .5f)
            .From(0f)
            );

        seq.Join(
            arrow.DOMove(
                point1Pos,
                .5f
                ).From(new Vector2(centre.x * .1f, centre.y))
                .SetEase(Ease.OutSine)
            );

        seq.Join(
            arrow.DORotate(
                new Vector3(0, 0, arrowRot1),
                .5f
                )
            ).OnComplete(() => {
                input.CancelMode();
            });

        yield return seq.WaitForCompletion();
        yield return StartCoroutine(TextType(lang.aa));
        yield return null;
        yield return input.WaitForAction();
        yield return StartCoroutine(TextType(lang.ab));
        yield return null;
        yield return input.WaitForAction();

        input.ToggleTutorialMode();

        seq = DOTween.Sequence();

        seq.Join(
            arrow.DOMove(
                point2Pos,
                .5f
                )
                .SetEase(Ease.OutSine)
            );

        seq.Join(
            arrow.DORotate(
                new Vector3(0, 0, arrowRot2),
                .5f
                )
            );

        yield return seq.WaitForCompletion();
        yield return StartCoroutine(TextType(lang.ac));
        yield return null;
        yield return input.WaitForAction();
        yield return StartCoroutine(TextType(lang.ad));
        yield return input.WaitForAction();

        input.CancelMode();

        seq = DOTween.Sequence();

        seq.Append(
            guide.DOMove(
                new Vector2(centre.x, Screen.height * -.1f),
                .4f
                )
            );

        seq.Join(
            arrowImg.DOFade(0f, .4f)
            .OnComplete(() => {
                Tutorialize(false);
            })
            );
    }

    private IEnumerator Level1() {
        levelDisplay.SetActive(true);

        Vector2 finalPos = ArrowOffsetCalc(levelDisplay.transform.position, arrowOffset-50, false);
        float angle = ArrowRotation(finalPos, levelDisplay.transform.position);


        Tutorialize(true);

        guide.DOMove(
            new Vector2(centre.x, centre.y * .6f),
            .4f
            )
            .SetEase(Ease.OutBack, 2f);

        yield return StartCoroutine(TextType(lang.ba));
        yield return null;
        yield return input.WaitForAction();

        Sequence seq = DOTween.Sequence();

        seq.Append(
            arrow.DOMove(
                finalPos,
                .5f
                ).SetEase(Ease.OutSine)
            );

        seq.Join(
            arrow.DORotate(
                new Vector3(0, 0, angle),
                .5f
                )
            );

        seq.Join(arrowImg.DOFade(1f, .5f));

        yield return seq.WaitForCompletion();
        yield return StartCoroutine(TextType(lang.bb));
        yield return input.WaitForAction();

        input.CancelMode();

        seq = DOTween.Sequence();

        seq.Append(
            guide.DOMove(
                new Vector2(centre.x, Screen.height * -.1f),
                .4f
                )
            );

        seq.Join(
            arrowImg.DOFade(0f, .4f)
            .OnComplete(() => {
                Tutorialize(false);
            })
        );

    }

    private IEnumerator Level2() {
        yield return input.WaitForAction();
    }

    private IEnumerator Level5() {
        coinDisplay.SetActive(true);
        boosterDisplay.SetActive(true);

        yield return input.WaitForAction();


    }

    private IEnumerator Level15() {
        yield return input.WaitForAction();
    }

    private IEnumerator Level30() {
        yield return input.WaitForAction();
    }

}
