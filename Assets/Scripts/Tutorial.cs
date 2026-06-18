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
using UnityEngine.Events;

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
    [SerializeField] private GameManager mana;
    public readonly LanguageTrans lang = new();
    public bool firstEver = false;
    


    //Common variables
    private Vector2 centre = new(Screen.width / 2, Screen.height / 2);
    private List<Bottle> bottles = new();
    private Image arrowImg;
    private float arrowOffset;
    private Tween current = null;
    private int next;

    //Level indentification
    private Coroutine level0;
    private Coroutine level1;
    private Coroutine level5;
    private Coroutine level10;
    private Coroutine level20;


    void Awake() {
        arrowImg = arrow.GetComponent<Image>();

        arrowOffset = (Camera.main.orthographicSize * 2f) * arrowOffsetPerc;

        tutorialMap = new() {
            { 0, () => {
                if (level0 != null) { StopCoroutine(level0); level0 = null; }
                level0 = StartCoroutine(Level0()); 
            }}, //Introduction

            { 1, () => {
                if (level1 != null) { StopCoroutine(level1); level1 = null; } 
                level1 = StartCoroutine(Level1()); 
            }},//Coin introduction/tracking

            { 5, () => {
                if (level5 != null) { StopCoroutine(level5); level5 = null; }
                level5 = StartCoroutine(Level5()); 
            }}, //Boosters introduction

            { 10, () => { 
                if (level10 != null) { StopCoroutine(level10); level10 = null; }
                level10 = StartCoroutine(Level10()); 
            }}, //Mystery colors

            { 20, () => {
                if (level20 != null) { StopCoroutine(level20); level20 = null; }
                level20 = StartCoroutine(Level20()); 
            }} //Cover introduction
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
            Tutorialize(false);
            input.ToggleTutorialMode();
            method?.Invoke();
        }
    }

    private void Tutorialize(bool set) {
        if (!set) {
            current?.Kill();
            current = null;
            arrow.DOMove(
                guide.transform.position,
                0
                );
            arrowImg.DOFade(0, 0);
        }
        
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

    private Tween ArrowBounce() {
        Vector3 dir = arrow.right;
        Tween bounceTween;
        return bounceTween = arrow.DOMove(
            arrow.position + (Vector3)dir * 30f,
            .5f
            ).SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
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

    private Sequence ArrowNewPosition(Vector2 selectedPos, float atAngle, bool fromOG = false, bool undoInput = false) {
        Vector2 from;
        if (fromOG) from = guide.transform.position;
        else from = arrow.transform.position;
        Sequence seq = DOTween.Sequence();

        seq.Join(
            arrow.DOMove(
                selectedPos,
                .5f
                )
                .SetEase(Ease.OutSine)
                .From(from)
            );

        seq.Join(
            arrow.DORotate(
                new Vector3(0, 0, atAngle),
                .5f
                )
            );

        if ( undoInput ) {
            input.CancelMode();
        }
            

        return seq;
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

    private IEnumerator WaitForEvent() {
        yield return new WaitUntil(() => next > 0);
        next = 0;
        input.ToggleTutorialMode();
    }

    private IEnumerator Level0() {
        levelDisplay.SetActive(false);
        coinDisplay.SetActive(false);
        boosterDisplay.SetActive(false);
        current = null;
        firstEver = true;
        next = 0;
        mana.nextStep?.RemoveAllListeners();

        mana.nextStep.AddListener(() => {
            next++;
        });


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
        arrow.DORotate(new Vector3(0, 0, arrowRot1), 0);

        guide.DOMove(
            new Vector2(centre.x, centre.y * .6f),
            .4f
            ).From(new Vector2(centre.x, centre.y * -1f))
            .SetEase(Ease.OutBack, 2f);

        yield return StartCoroutine(WaitForInput(lang.hello));
        
        ArrowNewPosition(point1Pos, arrowRot1, true, true).OnComplete(() => {
            arrowImg.DOFade(1f, .3f).From(0f);
            current = ArrowBounce();
        });

        yield return StartCoroutine(WaitForInput(lang.aa));
        yield return WaitForEvent();
        yield return StartCoroutine(WaitForInput(lang.ab));

        ArrowNewPosition(point2Pos, arrowRot2, undoInput: true)
            .OnStart(() => { current?.Kill(); })
            .OnComplete(() => { current = ArrowBounce(); });

        yield return StartCoroutine(WaitForInput(lang.ac));
        yield return WaitForEvent();
        yield return StartCoroutine(WaitForInput(lang.ad));

        current.Kill();
        SeqKill();
    }

    private IEnumerator Level1() {
        levelDisplay.SetActive(false);
        coinDisplay.SetActive(false);
        boosterDisplay.SetActive(false);
        Vector2 finalPos = ArrowOffsetCalc(levelDisplay.transform.position, arrowOffset * .5f, false);
        float angle = ArrowRotation(finalPos, levelDisplay.transform.position);
        arrow.DORotate(new Vector3(0, 0, angle), 0);

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

        ArrowNewPosition(finalPos, angle, true).OnComplete(() => {
            arrowImg.DOFade(1f, .3f).From(0f);
            current = ArrowBounce();
        });

        yield return StartCoroutine(WaitForInput(lang.bb));

        current?.Kill();
        SeqKill();

    }

    private IEnumerator Level5() {
        boosterDisplay.SetActive(false);
        coinDisplay.SetActive(true);
        Vector2 shuff = ArrowOffsetCalc(shuffleDis.position, arrowOffset, false);
        float shuffAngle = ArrowRotation(shuff, shuffleDis.position);

        Vector2 und = ArrowOffsetCalc(undoDis.position, arrowOffset, false);
        float undAngle = ArrowRotation(und, undoDis.position);

        Vector2 addB = ArrowOffsetCalc(addDis.position, arrowOffset, false);
        float addBAngle = ArrowRotation(addB, addDis.position);
        arrow.DORotate(new Vector3(0, 0, shuffAngle), 0);
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

        ArrowNewPosition(shuff, shuffAngle, true).OnComplete(() => { arrowImg.DOFade(1f, .3f).From(0f); current = ArrowBounce(); });

        yield return StartCoroutine(WaitForInput(lang.cc));

        ArrowNewPosition(und, undAngle).OnStart(() => { current?.Kill(); }).OnComplete(() => { current = ArrowBounce(); });

        yield return StartCoroutine(WaitForInput(lang.cd));

        ArrowNewPosition(addB, addBAngle).OnStart(() => { current?.Kill(); }).OnComplete(() => { current = ArrowBounce(); });

        yield return StartCoroutine(WaitForInput(lang.ce));
        yield return StartCoroutine(WaitForInput(lang.cf));
        current?.Kill();
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

        SeqKill();
    }

    private IEnumerator Level20() {
        guide.DOMove(
            new Vector2(centre.x, centre.y * .6f),
            .4f
            ).From(new Vector2(centre.x, centre.y * -1f))
            .SetEase(Ease.OutBack, 2f);

        yield return StartCoroutine(WaitForInput(lang.ea));
        yield return StartCoroutine(WaitForInput(lang.eb));
        yield return StartCoroutine(WaitForInput(lang.ec));
        yield return StartCoroutine(WaitForInput(lang.ed));
        yield return StartCoroutine(WaitForInput(lang.ee));

        

        SeqKill();
    }

}
