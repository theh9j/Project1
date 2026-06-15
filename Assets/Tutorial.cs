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
    public float arrowOffsetPerc = 3f;

    [SerializeField] private Transform tutorial;
    [SerializeField] private Transform guide;
    [SerializeField] private TMP_Text guideT;
    [SerializeField] private Transform arrow;

    private Dictionary<int, Action> tutorialMap;

    [SerializeField] private GameObject levelDisplay;
    [SerializeField] private GameObject coinDisplay;
    [SerializeField] private GameObject boosterDisplay;
    [SerializeField] private Transform shuffleDis;
    [SerializeField] private Transform undoDis;
    [SerializeField] private Transform addDis;

    [SerializeField] private InputHandler input;

    //Tutorial Items
    [SerializeField] private BottleGen bottleGen;
    public readonly LanguageTrans lang = new();


    //Common variables
    private Vector2 centre = new(Screen.width / 2, Screen.height / 2);
    private List<Bottle> bottles = new();
    private Image arrowImg;
    private float arrowOffset;

    void Awake() {
        arrowImg = arrow.GetComponent<Image>();

        arrowOffset = (Camera.main.orthographicSize * 2f) * arrowOffsetPerc;

        tutorialMap = new() {
            { 0, () => {StartCoroutine(Level0()); } }, //Introduction
            { 1, () => {StartCoroutine(Level1()); } }, //Coin introduction/tracking
            { 5, () => {StartCoroutine(Level5()); } }, //Boosters introduction

            { 15, () => { StartCoroutine(Level10()); } }, //Mystery colors
            { 30, () => { StartCoroutine(Level30()); } } //Cover introduction
        };
    }

    private IEnumerator WaitForInput(string text) {
        yield return StartCoroutine(TextType(text));
        yield return input.WaitForAction();
        yield return input.WaitForRelease();
    }

    public void CheckForTutorial(bool tutorial) {
        if (!tutorial) return;
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

    private void SeqKill() {
        Sequence seq = DOTween.Sequence();

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
                input.CancelMode();
            })
        );
    }

    private void ArrowNewPosition(Vector2 selectedPos, float atAngle, bool undoInput = false) {
        Sequence seq = DOTween.Sequence();

        seq.Join(
            arrow.DOMove(
                selectedPos,
                .5f
                )
                .SetEase(Ease.OutSine)
            );

        seq.Join(
            arrow.DORotate(
                new Vector3(0, 0, atAngle),
                .5f
                )
            );

        if ( undoInput ) 
            seq.OnComplete(() => {
                input.CancelMode();
            });
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

        yield return StartCoroutine(WaitForInput(lang.hello));

        arrowImg.DOFade(1f, .5f).From(0f);
        ArrowNewPosition(point1Pos, arrowRot1, true);

        yield return StartCoroutine(WaitForInput(lang.aa));
        yield return StartCoroutine(WaitForInput(lang.ab));

        input.ToggleTutorialMode();
        ArrowNewPosition(point2Pos, arrowRot2);

        yield return StartCoroutine(WaitForInput(lang.ac));
        yield return StartCoroutine(WaitForInput(lang.ad));

        SeqKill();
    }

    private IEnumerator Level1() {
        

        Vector2 finalPos = ArrowOffsetCalc(levelDisplay.transform.position, arrowOffset-50, false);
        float angle = ArrowRotation(finalPos, levelDisplay.transform.position);


        Tutorialize(true);

        guide.DOMove(
            new Vector2(centre.x, centre.y * .6f),
            .4f
            ).From(new Vector2(centre.x, centre.y * -1f))
            .SetEase(Ease.OutBack, 2f);

        yield return StartCoroutine(WaitForInput(lang.ba));

        Sequence seq = DOTween.Sequence();

        seq.Append(
            levelDisplay.transform.DOMove(
                levelDisplay.transform.position,
                .5f
                ).From(new Vector2(centre.x, Screen.height * 1.2f))
                .SetEase(Ease.OutBack, 2f)
                .OnStart(() => {
                    levelDisplay.SetActive(true);
                })
            );

        ArrowNewPosition(finalPos, angle);

        seq.Join(arrowImg.DOFade(1f, .5f));

        yield return StartCoroutine(WaitForInput(lang.bb));

        SeqKill();

    }

    private IEnumerator Level5() {
        coinDisplay.SetActive(true);
        Vector2 shuff = ArrowOffsetCalc(shuffleDis.position, arrowOffset, false);
        float shuffAngle = ArrowRotation(shuff, shuffleDis.position);

        Vector2 und = ArrowOffsetCalc(undoDis.position, arrowOffset, false);
        float undAngle = ArrowRotation(und, undoDis.position);

        Vector2 addB = ArrowOffsetCalc(addDis.position, arrowOffset, false);
        float addBAngle = ArrowRotation(addB, addDis.position);
        Tutorialize(true);

        guide.DOMove(
            new Vector2(centre.x, centre.y * .6f),
            .4f
            ).From(new Vector2(centre.x, centre.y * -1f))
            .SetEase(Ease.OutBack, 2f);

        yield return StartCoroutine(WaitForInput(lang.ca));
        yield return StartCoroutine(WaitForInput(lang.cb));

        guide.DOMove(
            new Vector2(centre.x, centre.y * .8f),
            .3f
            )
            .SetEase(Ease.OutBack, 2f);

        boosterDisplay.transform.DOMove(
            boosterDisplay.transform.position,
            .5f
            ).From(new Vector2(centre.x, centre.y * -1f))
            .SetEase(Ease.OutBack, 2f)
            .OnStart(() => {
                boosterDisplay.SetActive(true);
            });

        arrowImg.DOFade(1f, .5f).From(0f);
        ArrowNewPosition(shuff, shuffAngle);

        yield return StartCoroutine(WaitForInput(lang.cc));

        ArrowNewPosition(und, undAngle);

        yield return StartCoroutine(WaitForInput(lang.cd));

        ArrowNewPosition(addB, addBAngle);

        yield return StartCoroutine(WaitForInput(lang.ce));
        yield return StartCoroutine(WaitForInput(lang.cf));
        SeqKill();
    }

    private IEnumerator Level10() {

        guide.DOMove(
            new Vector2(centre.x, centre.y * .6f),
            .4f
            ).From(new Vector2(centre.x, centre.y * -1f))
            .SetEase(Ease.OutBack, 2f);

        yield return StartCoroutine(WaitForInput(lang.da));
        yield return StartCoroutine(WaitForInput(lang.db));
    }

    private IEnumerator Level30() {
        yield return input.WaitForAction();
    }

}
