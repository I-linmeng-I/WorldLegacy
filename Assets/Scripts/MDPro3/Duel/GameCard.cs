using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using YgomGame.Duel;
using YgomSystem.ElementSystem;
using MDPro3.UI;
using MDPro3.YGOSharp;
using MDPro3.YGOSharp.OCGWrapper.Enums;


namespace MDPro3
{
    public class GPS
    {
        public uint controller;
        public uint location;
        public uint sequence;
        public int position;
        public uint reason;
    }
    public class Effect
    {
        public string desc;
        public int flag;
        public int ptr;
    }


    public class GameCard : MonoBehaviour
    {
        private Card data = new Card();
        private Card cachedData = new Card();
        public GPS p;
        bool m_disabled;
        public bool disabled
        {
            get
            {
                return m_disabled;
            }
            set
            {
                m_disabled = value;
                SetDisabled();
            }
        }
        public bool negated;
        public bool SemiNomiSummoned = false;
        public int md5 = -233;
        public int selectPtr = 0;
        public int levelForSelect_1 = 0;
        public int levelForSelect_2 = 0;
        public int counterCanCount = 0;
        public int counterSelected = 0;
        public List<GameCard> targets = new List<GameCard>();
        public List<GameCard> effectTargets = new List<GameCard>();
        public List<GameCard> overlays = new List<GameCard>();
        public GameCard overlayParent;
        public GameCard equipedCard;
        public List<Effect> effects = new List<Effect>();


        public int overFatherCount;

        public GameObject model;
        public ElementObjectManager manager;
        public void Dispose()
        {
            Destroy(model);
            Destroy(this);
        }

        bool clicked;
        bool hover;
        bool hoving;

        bool needRefresh;

        private void LateUpdate()
        {
            if (model != null)
            {
                hover = Program.hoverObject == manager.GetElement("CardModel");
                if (!hover) hoving = false;

                if (hover && Program.InputGetMouse0Up && !Program.I().ocgcore.handCardDraged)
                    OnClick();
                else if (!hover && Program.InputGetMouse0Up)
                {
                    if (Program.hoverObject == null)
                        NotClickThis();
                    else if (Program.hoverObject.name != "PlaceSelector")
                        NotClickThis();
                }
                if (Math.Abs(Program.I().ocgcore.handOffset - Program.I().ocgcore.lastHandOffset) > 10)
                    NotClickThis();

                if ((p.location & (uint)CardLocation.Hand) > 0)
                {
                    if (hover && hoving == false && clicked == false)
                    {
                        hoving = true;
                        handDefault = false;
                        AnimationHandHover();
                    }
                    if (hover && Program.InputGetMouse0Up && !Program.I().ocgcore.handCardDraged)
                    {
                        clicked = true;
                        handDefault = false;
                        AnimationHandAppeal();
                    }
                    if (!hover && Program.InputGetMouse0Down)
                    {
                        clicked = false;
                    }
                    if (!hover && !clicked && !handDefault)
                    {

                        AnimationHandDefault(0.1f);
                    }

                    if (Math.Abs(Program.I().ocgcore.handOffset - Program.I().ocgcore.lastHandOffset) > 10)
                        SetHandDefault();
                }
            }
        }

        public Material GetMaterial()
        {
            if (model == null)
                return null;
            return manager.GetElement<Transform>("CardModel").GetChild(1).GetComponent<Renderer>().material;
        }

        public void OnClick()
        {
            if (model == null)
                return;
            if ((p.location & (uint)CardLocation.Hand) == 0)
                AudioManager.PlaySE("SE_DUEL_SELECT");
            Program.I().ocgcore.description.Show(this, GetMaterial());
            if ((data.Type & (uint)CardType.Xyz) > 0 && (p.location & (uint)CardLocation.MonsterZone) > 0)
                Program.I().ocgcore.list.Show(Program.I().ocgcore.GCS_GetOverlays(this), CardLocation.Overlay, (int)p.controller);
            else
                Program.I().ocgcore.list.Hide();

            if (equipedCard != null)
                Program.I().ocgcore.ShowEquipLine(model.transform.position, equipedCard.model.transform.position);
            if (targets != null)
                Program.I().ocgcore.ShowTargetLines(model.transform.position, targets);

            if (buttons.Count == 0 || Program.I().ocgcore.currentPopup != null)
                return;
            foreach (var button in buttonObjs)
                button.Show();
            if (hightYellow)
                manager.GetElement("EffectHighlightYellowSelect").SetActive(true);
            else
                manager.GetElement("EffectHighlightBlueSelect").SetActive(true);
        }

        public void NotClickThis()
        {
            if (model == null || buttons.Count == 0)
                return;
            foreach (var button in buttonObjs)
                button.Hide();
            if (hightYellow)
                manager.GetElement("EffectHighlightYellowSelect").SetActive(false);
            else
                manager.GetElement("EffectHighlightBlueSelect").SetActive(false);
        }

        private GameObject CreateModel(bool real = true)
        {
            model = Instantiate(Program.I().ocgcore.container.cardModel);
            manager = model.GetComponent<ElementObjectManager>();

            var cardMono = manager.GetElement<GameCardMono>("CardModel");
            cardMono.cookieCard = this;

            var cardParmUp = ABLoader.LoadFromFile("effects/eff_prm/fxp_cardparm_up_001", true);
            var cardParmDown = ABLoader.LoadFromFile("effects/eff_prm/fxp_cardparm_down_001", true);
            var cardParmChange = ABLoader.LoadFromFile("effects/eff_prm/fxp_cardparm_change_001", true);
            var cardBuffActive = ABLoader.LoadFromFile("effects/buff/fxp_bff_active_001", true);
            var cardNegate = ABLoader.LoadFromFile("effects/buff/fxp_bff_disable_001", true);

            var cardBlueHighlight = ABLoader.LoadFromFile("effects/hitghlight/fxp_hl_set_001", true);
            var cardBlueHighlightSelect = ABLoader.LoadFromFile("effects/hitghlight/fxp_hl_set_sct_001", true);
            var cardYellowHighlight = ABLoader.LoadFromFile("effects/hitghlight/fxp_hl_spsom_001", true);
            var cardYellowHighlightSelect = ABLoader.LoadFromFile("effects/hitghlight/fxp_hl_spsom_sct_001", true);


            cardParmUp.transform.SetParent(manager.GetElement<Transform>("Turn").GetChild(1), false);
            cardParmDown.transform.SetParent(manager.GetElement<Transform>("Turn").GetChild(1), false);
            cardParmChange.transform.SetParent(manager.GetElement<Transform>("Turn").GetChild(1), false);
            cardBuffActive.transform.SetParent(manager.GetElement<Transform>("Turn").GetChild(1), false);
            cardBuffActive.transform.localPosition = new Vector3(0, 0.1f, 0);
            cardNegate.transform.SetParent(manager.GetElement<Transform>("Turn").GetChild(1), false);
            cardNegate.transform.localPosition = new Vector3(0, 0.1f, 0);

            var highlight = new GameObject("Highlight");
            cardBlueHighlight.transform.SetParent(highlight.transform, false);
            cardBlueHighlightSelect.transform.SetParent(highlight.transform, false);
            cardYellowHighlight.transform.SetParent(highlight.transform, false);
            cardYellowHighlightSelect.transform.SetParent(highlight.transform, false);
            highlight.transform.SetParent(manager.GetElement<Transform>("Turn").GetChild(1), false);
            //Tools.ChangeSortingLayer(highlight, "DuelEffect_Low");
            Tools.ChangeMaterialRenderQueue(highlight, 3001);

            var e1 = cardParmUp.AddComponent<ElementObject>();
            e1.label = "EffectBuff";
            var e2 = cardParmDown.AddComponent<ElementObject>();
            e2.label = "EffectDebuff";
            var e3 = cardParmChange.AddComponent<ElementObject>();
            e3.label = "EffectChange";
            var e4 = cardBuffActive.AddComponent<ElementObject>();
            e4.label = "EffectBuffActive";
            var e5 = cardNegate.AddComponent<ElementObject>();
            e5.label = "EffectNegate";

            var e7 = cardBlueHighlight.AddComponent<ElementObject>();
            e7.label = "EffectHighlightBlue";
            var e8 = cardBlueHighlightSelect.AddComponent<ElementObject>();
            e8.label = "EffectHighlightBlueSelect";
            var e9 = cardYellowHighlight.AddComponent<ElementObject>();
            e9.label = "EffectHighlightYellow";
            var e10 = cardYellowHighlightSelect.AddComponent<ElementObject>();
            e10.label = "EffectHighlightYellowSelect";



            var list = manager.serializedElements.ToList();
            list.Add(e1);
            list.Add(e2);
            list.Add(e3);
            list.Add(e4);
            list.Add(e5);

            list.Add(e7);
            list.Add(e8);
            list.Add(e9);
            list.Add(e10);

            manager.serializedElements = list.ToArray();

            cardParmUp.SetActive(false);
            cardParmDown.SetActive(false);
            cardParmChange.SetActive(false);
            cardBuffActive.SetActive(false);
            cardNegate.SetActive(false);

            cardBlueHighlight.SetActive(false);
            cardBlueHighlightSelect.SetActive(false);
            cardYellowHighlight.SetActive(false);
            cardYellowHighlightSelect.SetActive(false);

            var back = manager.GetElement<Transform>("CardModel").GetChild(0).GetComponent<Renderer>();

            switch (Program.I().ocgcore.condition)
            {
                case OcgCore.Condition.Duel:
                    back.material = p.controller == 0 ? Appearance.duelProtector0 : Appearance.duelProtector1;
                    break;
                case OcgCore.Condition.Watch:
                    back.material = p.controller == 0 ? Appearance.watchProtector0 : Appearance.watchProtector1;
                    break;
                case OcgCore.Condition.Replay:
                    back.material = p.controller == 0 ? Appearance.replayProtector0 : Appearance.replayProtector1;
                    break;
            }
            back.material.renderQueue = 3000;
            StartCoroutine(SetFace());

            if (p.controller == 0)
                model.transform.SetParent(Program.I().ocgcore.field0Manager.transform, true);
            else
                model.transform.SetParent(Program.I().ocgcore.field1Manager.transform, true);

            if (real)
                return model;
            else
            {
                var go = model;
                model = null;
                return go;
            }
        }
        IEnumerator SetFace()
        {
            Renderer cardFace = manager.GetElement<Transform>("CardModel").
                    GetChild(1).GetComponent<Renderer>();
            cardFace.material = TextureManager.GetCardMaterial(data.Id);
            cardFace.material.renderQueue = 2999;
            IEnumerator enumerator;
            enumerator = Program.I().texture_.LoadCardAsync(data.Id, true);
            StartCoroutine(enumerator);
            while (enumerator.MoveNext())
                yield return null;
            if (model == null)
                yield break;
            cardFace.material.mainTexture = enumerator.Current as Texture;
            SetDisabled();

            if (p.controller == 0 && Program.I().ocgcore.deck != null)
            {
                var ie = ABLoader.LoadProtectorMaterial(Program.I().ocgcore.deck.Protector[0].ToString());
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                var back = manager.GetElement<Transform>("CardModel").GetChild(0).GetComponent<Renderer>();
                back.material = ie.Current;
            }
        }


        public Card GetData()
        {
            return data;
        }
        public Card CacheData()
        {
            cachedData = data.Clone();
            return cachedData;
        }
        public Card GetCachedData()
        {
            return cachedData;
        }

        public void SetData(Card d)
        {
            data = d;
            if (model != null)
            {
                if (data.Id > 0)
                    StartCoroutine(SetFace());
                RefreshLabel();
            }
            if(d.Id > 0)
                if ((p.location & (uint)CardLocation.Extra) > 0)
                    if ((p.position & (uint)CardPosition.FaceUp) > 0)
                        if (p.sequence == Program.I().ocgcore.GetLocationCardCount(CardLocation.Extra, p.controller) - 1)
                            StartCoroutine(Program.I().ocgcore.UpdateDeckTop(p.controller, this));
        }

        public void SetCode(int code)
        {
            if (code > 0)
            {
                if (data.Id != code)
                {
                    SetData(CardsManager.Get(code));
                    data.Id = code;
                    if (p.controller == 1)
                        if (Program.I().ocgcore.condition == OcgCore.Condition.Duel)
                            if (!Program.I().ocgcore.sideReference.Main.Contains(code))
                                Program.I().ocgcore.sideReference.Main.Add(code);
                }
            }
        }

        public void RefreshData()
        {
            CardsManager.Get(data.Id).CloneTo(data);
            SetData(data);
            ClearAllTails();
        }
        public void EraseData()
        {
            SetData(CardsManager.Get(0));
            disabled = false;
            ClearAllTails();
        }

        public void AddTarget(GameCard card)
        {
            if (!targets.Contains(card))
                targets.Add(card);
        }

        public void RemoveTarget(GameCard card)
        {
            targets.Remove(card);
        }

        public void AddEffectTarget(GameCard card)
        {
            if (!effectTargets.Contains(card))
                effectTargets.Add(card);
        }
        public void RemoveEffectTarget(GameCard card)
        {
            effectTargets.Remove(card);
        }

        public static Vector3 GetCardPosition(GPS p, GameCard c = null, GameCard overlayParent = null)
        {
            var returnValue = Vector3.zero;

            if ((p.location & (uint)CardLocation.Search) > 0)
            {
                return new Vector3(0, -50, 0);
            }
            else if ((p.location & (uint)CardLocation.Unknown) > 0)
            {
                return new Vector3(0, 10, 0);
            }
            else if ((p.location & (uint)CardLocation.Hand) > 0)
            {
                int handsCount;
                if(c == null)
                    return Vector3.zero;

                if (c.p.controller == 0)
                    handsCount = Program.I().ocgcore.GetMyHandCount();
                else
                    handsCount = Program.I().ocgcore.GetOpHandCount();

                float x = p.sequence * 4 - (handsCount - 1) * 2;

                var z0 = -28 + (30 - Program.I().camera_.cameraMain.fieldOfView) * 0.7f;
                var z1 = 23 - (30 - Program.I().camera_.cameraMain.fieldOfView) * 0.7f;

                if (p.controller == 0)
                    return new Vector3(x + Program.I().ocgcore.handOffset * UIManager.ScreenLengthWithoutScalerX(0.038f), 15, z0);
                else
                    return new Vector3(-x, 5, z1);
            }
            else if ((p.location & (uint)CardLocation.Deck) > 0)
            {
                if (p.controller == 0)
                    returnValue = new Vector3(26.86f, 1.5f, -23.93f);
                else
                    returnValue = new Vector3(-26.86f, 1.5f, 23.93f);
                returnValue.y += p.sequence * 0.1f;
            }
            else if ((p.location & (uint)CardLocation.Extra) > 0)
            {
                if (p.controller == 0)
                    returnValue = new Vector3(-26.86f, 1.5f, -23.93f);
                else
                    returnValue = new Vector3(26.86f, 1.5f, 23.93f);
                returnValue.y += p.sequence * 0.1f;
            }
            else if ((p.location & (uint)CardLocation.Grave) > 0)
            {
                if (p.controller == 0)
                    returnValue = new Vector3(25.74f, 5f, -14.26f);
                else
                    returnValue = new Vector3(-25.74f, 5f, 14.26f);
            }

            else if ((p.location & (uint)CardLocation.Removed) > 0)
            {
                if (p.controller == 0)
                    returnValue = new Vector3(25.74f + 1.842971f, 5f, -14.26f + 6.236011f);
                else
                    returnValue = new Vector3(-25.74f - 1.842971f, 5f, 14.26f - 6.236011f);
            }

            else if ((p.location & (uint)CardLocation.MonsterZone) > 0)
            {
                var realIndex = p.sequence;
                if (p.controller == 0)
                {
                    realIndex = p.sequence;
                    returnValue.y = 0.2f;
                    returnValue.z = -9.48f;
                }
                else
                {
                    if (realIndex <= 4)
                        realIndex = 4 - p.sequence;
                    else if (realIndex == 5)
                        realIndex = 6;
                    else if (realIndex == 6) realIndex = 5;
                    returnValue.y = 0.2f;
                    returnValue.z = 9.51f;
                }

                switch (realIndex)
                {
                    case 0:
                        returnValue.x = -17.2f;
                        break;
                    case 1:
                        returnValue.x = -8.6f;
                        break;
                    case 2:
                        returnValue.x = 0f;
                        break;
                    case 3:
                        returnValue.x = 8.6f;
                        break;
                    case 4:
                        returnValue.x = 17.2f;
                        break;
                    case 5:
                        returnValue.x = -8.6f;
                        returnValue.z = 0;
                        break;
                    case 6:
                        returnValue.x = 8.6f;
                        returnValue.z = 0;
                        break;
                }
            }

            else if ((p.location & (uint)CardLocation.SpellZone) > 0)
            {
                if (p.sequence < 5 || (p.sequence == 6 || p.sequence == 7) && Program.I().ocgcore.MasterRule >= 4)
                {
                    var realIndex = p.sequence;
                    if (p.controller == 0)
                    {
                        realIndex = p.sequence;
                        returnValue.y = 0.2f;
                        returnValue.z = -18f;
                    }
                    else
                    {
                        if (realIndex <= 4)
                            realIndex = 4 - p.sequence;
                        else if (realIndex == 7)
                            realIndex = 6;
                        else if (realIndex == 6) realIndex = 7;
                        returnValue.y = 0.2f;
                        returnValue.z = 18f;
                    }

                    switch (realIndex)
                    {
                        case 0:
                            returnValue.x = -17.2f;
                            break;
                        case 1:
                            returnValue.x = -8.6f;
                            break;
                        case 2:
                            returnValue.x = 0f;
                            break;
                        case 3:
                            returnValue.x = 8.6f;
                            break;
                        case 4:
                            returnValue.x = 17.2f;
                            break;
                        case 6:
                            returnValue.x = -8.6f;
                            break;
                        case 7:
                            returnValue.x = 8.6f;
                            break;
                    }
                }

                if (p.sequence == 5)
                {
                    if (p.controller == 0)
                        returnValue = new Vector3(-25f, 0.1f, -10f);
                    else
                        returnValue = new Vector3(25f, 0.1f, 10f);
                }

                if (Program.I().ocgcore.MasterRule <= 3)
                {
                    if (p.sequence == 6)
                    {
                        if (p.controller == 0)
                            returnValue = new Vector3(-30f, 10, -15f);
                        else
                            returnValue = new Vector3(30f, 10, 10f);
                    }

                    if (p.sequence == 7)
                    {
                        if (p.controller == 0)
                            returnValue = new Vector3(30f, 10, -15f);
                        else
                            returnValue = new Vector3(-30f, 10, 10f);
                    }
                }
            }

            if ((p.location & (uint)CardLocation.Overlay) > 0)
            {
                if (overlayParent != null)
                {
                    var pposition = overlayParent.overFatherCount - 1 - p.position;
                    returnValue.y -= (pposition + 2) * 0.02f;
                    returnValue.x += (pposition + 1) * 0.2f;
                }
                else
                {
                    returnValue.y -= (p.position + 2) * 0.02f;
                    returnValue.x += (p.position + 1) * 0.2f;
                }
            }
            return returnValue;
        }
        public static Vector3 GetCardRotation(GPS p, int code = 0)
        {
            var condition = CardRuleCondition.MeUpAtk;
            if ((p.location & (uint)CardLocation.Deck) > 0)
            {
                if ((p.position & (uint)CardPosition.FaceUp) > 0)
                    condition = CardRuleCondition.MeUpDeck;
                else
                    condition = CardRuleCondition.MeDownDeck;
            }
            else if ((p.location & (uint)CardLocation.Extra) > 0)
            {
                if ((p.position & (uint)CardPosition.FaceUp) > 0)
                    condition = CardRuleCondition.MeUpExDeck;
                else
                    condition = CardRuleCondition.MeDownExDeck;
            }
            else if ((p.location & (uint)CardLocation.Grave) > 0)
            {
                if ((p.position & (uint)CardPosition.FaceUp) > 0)
                    condition = CardRuleCondition.MeUpGrave;
                else
                    condition = CardRuleCondition.MeDownGrave;
            }
            else if ((p.location & (uint)CardLocation.Removed) > 0)
            {
                if ((p.position & (uint)CardPosition.FaceUp) > 0)
                    condition = CardRuleCondition.MeUpRemoved;
                else
                    condition = CardRuleCondition.MeDownRemoved;
            }
            else if ((p.location & (uint)CardLocation.MonsterZone) > 0)
            {
                if ((p.position & (uint)CardPosition.FaceUp) > 0)
                {
                    if ((p.position & (uint)CardPosition.Attack) > 0)
                        condition = CardRuleCondition.MeUpAtk;
                    else
                        condition = CardRuleCondition.MeUpDef;
                }
                else
                {
                    if ((p.position & (uint)CardPosition.Attack) > 0)
                        condition = CardRuleCondition.MeDownAtk;
                    else
                        condition = CardRuleCondition.MeDownDef;

                }
            }
            else if ((p.location & (uint)CardLocation.SpellZone) > 0)
            {
                if ((p.position & (uint)CardPosition.FaceUp) > 0)
                    condition = CardRuleCondition.MeUpAtk;
                else
                    condition = CardRuleCondition.MeDownAtk;
            }
            else if ((p.location & (uint)CardLocation.Hand) > 0)
            {
                if (code != 0)
                    condition = CardRuleCondition.MeUpHand;
                else
                    condition = CardRuleCondition.MeDownHand;
            }

            if ((p.location & (uint)CardLocation.Overlay) > 0)
                condition = CardRuleCondition.MeUpAtk;

            if (p.controller != 0)
            {
                switch (condition)
                {
                    case CardRuleCondition.MeUpAtk:
                        condition = CardRuleCondition.OpUpAtk;
                        break;
                    case CardRuleCondition.MeDownAtk:
                        condition = CardRuleCondition.OpDownAtk;
                        break;
                    case CardRuleCondition.MeUpDef:
                        condition = CardRuleCondition.OpUpDef;
                        break;
                    case CardRuleCondition.MeDownDef:
                        condition = CardRuleCondition.OpDownDef;
                        break;
                    case CardRuleCondition.MeUpDeck:
                        condition = CardRuleCondition.OpUpDeck;
                        break;
                    case CardRuleCondition.MeDownDeck:
                        condition = CardRuleCondition.OpDownDeck;
                        break;
                    case CardRuleCondition.MeUpExDeck:
                        condition = CardRuleCondition.OpUpExDeck;
                        break;
                    case CardRuleCondition.MeDownExDeck:
                        condition = CardRuleCondition.OpDownExDeck;
                        break;
                    case CardRuleCondition.MeUpGrave:
                        condition = CardRuleCondition.OpUpGrave;
                        break;
                    case CardRuleCondition.MeDownGrave:
                        condition = CardRuleCondition.OpDownGrave;
                        break;
                    case CardRuleCondition.MeUpRemoved:
                        condition = CardRuleCondition.OpUpRemoved;
                        break;
                    case CardRuleCondition.MeDownRemoved:
                        condition = CardRuleCondition.OpDownRemoved;
                        break;
                    case CardRuleCondition.MeUpHand:
                        condition = CardRuleCondition.OpUpHand;
                        break;
                    case CardRuleCondition.MeDownHand:
                        condition = CardRuleCondition.OpDownHand;
                        break;
                }
            }

            switch (condition)
            {
                case CardRuleCondition.MeUpAtk:
                    return new Vector3(0, 0, 0);
                case CardRuleCondition.MeUpDef:
                    return new Vector3(0, 270, 0);
                case CardRuleCondition.MeDownAtk:
                    return new Vector3(0, 0, 180);
                case CardRuleCondition.MeDownDef:
                    return new Vector3(0, 270, 180);
                case CardRuleCondition.MeUpDeck:
                    return new Vector3(0, -19.5f, 0);
                case CardRuleCondition.MeDownDeck:
                    return new Vector3(0, -19.5f, 180);
                case CardRuleCondition.MeUpExDeck:
                    return new Vector3(0, 19.5f, 0);
                case CardRuleCondition.MeDownExDeck:
                    return new Vector3(0, 19.5f, 180);
                case CardRuleCondition.MeUpGrave:
                    return new Vector3(0, 0, 0);
                case CardRuleCondition.MeDownGrave:
                    return new Vector3(0, 270, 0);
                case CardRuleCondition.MeUpRemoved:
                    return new Vector3(0, 90, 0);
                case CardRuleCondition.MeDownRemoved:
                    return new Vector3(0, 90, 180);
                case CardRuleCondition.MeUpHand:
                    return new Vector3(-20, 0, 0);
                case CardRuleCondition.MeDownHand:
                    return new Vector3(-20, 0, 180);

                case CardRuleCondition.OpUpAtk:
                    return new Vector3(0, 180, 0);
                case CardRuleCondition.OpUpDef:
                    return new Vector3(0, 90, 0);
                case CardRuleCondition.OpDownAtk:
                    return new Vector3(0, 180, 180);
                case CardRuleCondition.OpDownDef:
                    return new Vector3(0, 90, 180);
                case CardRuleCondition.OpUpDeck:
                    return new Vector3(0, 160.5f, 0);
                case CardRuleCondition.OpDownDeck:
                    return new Vector3(0, 160.5f, 180);
                case CardRuleCondition.OpUpExDeck:
                    return new Vector3(0, 199.5f, 0);
                case CardRuleCondition.OpDownExDeck:
                    return new Vector3(0, 199.5f, 180);
                case CardRuleCondition.OpUpGrave:
                    return new Vector3(0, 180, 0);
                case CardRuleCondition.OpDownGrave:
                    return new Vector3(0, 180, 180);
                case CardRuleCondition.OpUpRemoved:
                    return new Vector3(0, 270, 0);
                case CardRuleCondition.OpDownRemoved:
                    return new Vector3(0, 270, 180);
                case CardRuleCondition.OpUpHand:
                    return new Vector3(20, 180, 0);
                case CardRuleCondition.OpDownHand:
                    return new Vector3(20, 180, 180);
                default:
                    return Vector3.zero;
            }
        }
        public static Vector3 GetEffectRotaion(GPS p)
        {
            if ((p.controller == 0))
            {
                if ((p.position & (uint)CardPosition.Attack) > 0)
                    return new Vector3(0, 0, 0);
                else
                    return new Vector3(0, 270, 0);
            }
            else
            {
                if ((p.position & (uint)CardPosition.Attack) > 0)
                    return new Vector3(0, 180, 0);
                else
                    return new Vector3(0, 90, 0);
            }
        }
        public static Vector3 GetCardScale(GPS p)
        {
            if ((p.location & (uint)CardLocation.SpellZone) > 0)
                return Vector3.one * 0.8f;
            else
                return Vector3.one;
        }
        bool ThisLocationShouldHaveModel(GPS p)
        {
            if ((p.location & (uint)CardLocation.Hand) > 0)
                return true;
            else if ((p.location & (uint)CardLocation.Overlay) > 0)
                return false;
            else if ((p.location & (uint)CardLocation.Onfield) > 0)
                return true;
            else
                return false;
        }

        public static bool NeedStrongSummon(Card data)
        {
            if ((data.Type & (uint)CardType.Link) > 0)
            {
                if (CardDescription.GetCardLinkCount(data) > 2)
                    return true;
                else return false;
            }
            else if ((data.Type & (uint)CardType.Xyz) > 0)
            {
                if (data.Level > 3)
                    return true;
                else return false;
            }
            else if ((data.Type & (uint)CardType.Synchro) > 0)
            {
                if (data.Level > 5)
                    return true;
                else return false;
            }
            else if ((data.Type & (uint)CardType.Fusion) > 0)
            {
                if (data.Level > 5)
                    return true;
                else return false;
            }
            else if ((data.Type & (uint)CardType.Ritual) > 0)
            {
                if (data.Level > 5)
                    return true;
                else return false;
            }
            else
            {
                if (data.Level > 6)
                    return true;
                else return false;
            }
        }

        GPS cacheP;
        bool inAnimation;
        static uint lastMovedLocation;
        public float Move(GPS gps, bool rush = false)
        {
            Program.I().ocgcore.lastMoveCard = this;

            //Move Analyse
            if (p.location != gps.location || (gps.position & (uint)CardPosition.FaceDown) > 0)
            {
                targets.Clear();
                equipedCard = null;
                foreach (var card in Program.I().ocgcore.cards)
                    card.RemoveTarget(this);
                disabled = false;
                RefreshData();
            }

            overlays = Program.I().ocgcore.GCS_GetOverlays(this);

            cacheP = p;
            p = gps;

            if (!SemiNomiSummoned
                && (CardsManager.Get(data.Id).Type & 0x68020C0) > 0
                && (CardsManager.Get(data.Id).Type & (uint)CardType.Monster) > 0
                && (p.location & ((uint)CardLocation.Grave + (uint)CardLocation.Removed)) > 0
                )
                AddStringTail(InterString.Get("未正规登场"));
            else
                RemoveStringTail(InterString.Get("未正规登场"), true);

            //Debug.LogFormat("{0}: reason: {1:X} location: {2:X}", data.Name, p.reason, p.location);

            for (int i = 0; i < overlays.Count; i++)
            {
                overlays[i].overlayParent = this;
                overlays[i].p.controller = gps.controller;
                overlays[i].p.location = gps.location | (uint)CardLocation.Overlay;
                overlays[i].p.sequence = gps.sequence;
                overlays[i].p.position = i;
            }
            Program.I().ocgcore.ArrangeCards();

            if (Program.I().ocgcore.currentMessage == GameMessage.Move
                && cacheP.location != p.location
                && (p.reason & (uint)CardReason.MATERIAL) > 0
                && (cacheP.location & (uint)CardLocation.Overlay) == 0)
                Program.I().ocgcore.materialCards.Add(this);

            if (!ThisLocationShouldHaveModel(p) && cacheP.location == p.location)
                return 0;
            if ((cacheP.location & (uint)CardLocation.Overlay) > 0
                && ((p.reason & (uint)CardReason.RULE) > 0 || (p.location & (uint)CardLocation.Extra) > 0))
                return 0;

            float moveTime = 0.3f;

            if (rush)
            {
                if (ThisLocationShouldHaveModel(p) && model == null)
                {
                    CreateModel();
                    ModelAt(p);
                }
                return 0;
            }
            else
            {
                OcgCore.messagePass = false;

                if (model == null)
                {
                    CreateModel();
                    ModelAt(cacheP);
                }

                inAnimation = true;
                Program.I().ocgcore.needRefreshHand0 = true;
                Program.I().ocgcore.needRefreshHand1 = true;
                Program.I().ocgcore.RefreshHandCardPosition();
                Program.I().ocgcore.RefreshBgState();

                string se = "";
                var sequence = DOTween.Sequence();
                float timePassed = 0;

                var position = GetCardPosition(p, this);
                var rotation = GetCardRotation(p, data.Id);

                //From 墓地 or 除外
                if ((cacheP.location & ((uint)CardLocation.Grave + (uint)CardLocation.Removed)) > 0)
                    timePassed += SequenceFromGrave(sequence, cacheP);
                //从卡组到手卡
                if ((cacheP.location & (uint)CardLocation.Deck) > 0
                    && (p.location & (uint)CardLocation.Hand) > 0)
                    se = "SE_CARD_MOVE_0" + UnityEngine.Random.Range(1, 5);
                //破坏
                if ((p.reason & (uint)CardReason.DESTROY) > 0
                    && model != null
                    && (cacheP.location & ((uint)CardLocation.Onfield + (uint)CardLocation.Hand)) > 0
                    )
                {
                    se = "SE_CARDBREAK_01";
                    if ((data.Type & (uint)CardType.Token) == 0)
                    {
                        var fx = ABLoader.LoadFromFile("effects/break/fxp_cardbrk_bff_001", true);
                        fx.transform.position = model.transform.position;
                        Destroy(fx, 3f);
                    }
                }

                //超量素材
                if ((cacheP.location & (uint)CardLocation.MonsterZone) > 0
                    && (p.location & (uint)CardLocation.Overlay) > 0
                    && (p.location & (uint)CardLocation.Extra) > 0
                    )
                {
                    AudioManager.PlaySE(se);
                    AudioManager.PlaySE("SE_SUMMON_EYZ_MATERIAL");
                    var fx = ABLoader.LoadFromFolder("Timeline/Summon/SummonFusion/fusiontrailfieldcard01", "FieldCard", true);
                    fx.transform.localPosition = model.transform.position;
                    fx.transform.localEulerAngles = GetEffectRotaion(cacheP);
                    var manager = fx.transform.GetChild(0).GetComponent<ElementObjectManager>();
                    manager = manager.GetElement<ElementObjectManager>("DummyCard01");
                    var dummyFace = manager.GetElement<MeshRenderer>("DummyCardModel_front");
                    dummyFace.material = this.manager.GetElement<Transform>("CardModel").GetChild(1).GetComponent<Renderer>().material;
                    if ((cacheP.position & (uint)CardPosition.Defence) > 0)
                        fx.transform.eulerAngles = new Vector3(0, 90, 0);
                    foreach (var p in fx.GetComponentsInChildren<ParticleSystem>(true))
                    {
                        var main = p.main;
                        main.startColor = Color.yellow;
                    }

                    Destroy(model);
                    Destroy(fx, 2);
                    OcgCore.messagePass = true;
                    return 0.2f;
                }
                //召唤素材
                if (ThisLocationShouldHaveModel(cacheP)
                    && (p.reason & (uint)CardReason.MATERIAL) > 0
                    && (p.location & (uint)CardLocation.Onfield) == 0
                    && ((p.reason & ((uint)CardReason.Ritual) + (uint)CardReason.Fusion + (uint)CardReason.Synchro + (uint)CardReason.Link) > 0))
                {
                    AudioManager.PlaySE(se);
                    AudioManager.PlaySE("SE_SUMMON_EYZ_MATERIAL");
                    var fx = ABLoader.LoadFromFolder("Timeline/Summon/SummonFusion/fusiontrailfieldcard01", "FieldCard", true);
                    fx.transform.localPosition = model.transform.position;
                    fx.transform.localEulerAngles = GetEffectRotaion(cacheP);
                    var manager = fx.transform.GetChild(0).GetComponent<ElementObjectManager>();
                    manager = manager.GetElement<ElementObjectManager>("DummyCard01");
                    var dummyFace = manager.GetElement<MeshRenderer>("DummyCardModel_front");
                    dummyFace.material = this.manager.GetElement<Transform>("CardModel").GetChild(1).GetComponent<Renderer>().material;

                    foreach (var particle in fx.GetComponentsInChildren<ParticleSystem>(true))
                    {
                        var main = particle.main;
                        if ((p.reason & (uint)CardReason.Ritual) > 0)
                            main.startColor = new Color(0f, 0.5f, 1f, 1f);
                        else if ((p.reason & (uint)CardReason.Synchro) > 0)
                            main.startColor = Color.green;
                        else if ((p.reason & (uint)CardReason.Link) > 0)
                            main.startColor = Color.red;
                    }

                    if((p.location & (uint)CardLocation.Extra) > 0
                        && (p.location & (uint)CardLocation.Overlay) == 0)
                        Program.I().ocgcore.SetDeckTop(this);
                    Destroy(model);
                    Destroy(fx, 2);
                    OcgCore.messagePass = true;
                    return 0.2f;
                }

                //Token In (from unknow)
                if (cacheP.location == 0)
                {
                    AudioManager.PlaySE("SE_CARD_TOKEN_SUMMON");
                    var fx = ABLoader.LoadFromFolder("Timeline/Summon/SummonToken", "SummonToken", true);
                    fx.transform.position = GetCardPosition(p);
                    fx.transform.localEulerAngles = GetEffectRotaion(p);
                    ModelAt(p);
                    model.SetActive(false);
                    var tokenManager = fx.transform.GetChild(0).GetComponent<ElementObjectManager>();
                    var ie = Program.I().texture_.LoadDummyCard(tokenManager.GetElement<ElementObjectManager>("DummyCard01"), data.Id);
                    StartCoroutine(ie);
                    DOTween.To(v => { }, 0, 0, 1.25f).OnComplete(() =>
                    {
                        OcgCore.messagePass = true;
                        model.SetActive(true);
                    });
                    Destroy(fx, 2f);
                    return 0.5f;
                }
                //Token Out (to unknow)
                if (p.location == 0)
                {
                    AudioManager.PlaySE("SE_CARD_TOKEN_BREAK");
                    var fx = ABLoader.LoadFromFolder("Effects/buff/fxp_bff_tokese", "fxp_bff_tokese", true);
                    fx.transform.position = model.transform.position;
                    fx.transform.localEulerAngles = GetEffectRotaion(p);
                    Destroy(model);
                    Destroy(fx, 3f);
                    OcgCore.messagePass = true;
                    return 0.2f;
                }
                //解放
                if ((p.reason & (uint)CardReason.RELEASE) > 0 && model != null)
                {
                    se = "SE_SUMMON_ADVANCE";
                    var fx = ABLoader.LoadFromFile("effects/sacrifice/fxp_sacrifice_rls_001", true);
                    fx.transform.position = model.transform.position;
                    Destroy(fx, 5f);
                }

                //特殊召唤
                if ((p.reason & (uint)CardReason.SPSUMMON) > 0
                    && (p.location & (uint)CardLocation.MonsterZone) > 0
                    && (cacheP.location & (uint)CardLocation.MonsterZone) == 0
                    && (p.location & (uint)CardLocation.Overlay) == 0)
                {
                    bool summonEffect = true;
                    if (Program.I().ocgcore.condition == OcgCore.Condition.Duel
                        && Config.Get("DuelSummon", "1") == "0")
                        summonEffect = false;
                    if (Program.I().ocgcore.condition == OcgCore.Condition.Watch
                        && Config.Get("WatchSummon", "1") == "0")
                        summonEffect = false;
                    if (Program.I().ocgcore.condition == OcgCore.Condition.Replay
                        && Config.Get("ReplaySummon", "1") == "0")
                        summonEffect = false;

                    if (summonEffect
                        && Program.I().ocgcore.materialCards.Count > 0
                        && (OcgCore.TypeMatchReason(data.Type, (int)Program.I().ocgcore.materialCards[0].p.reason)
                        || OcgCore.TypeMatchReason(data.Type, Program.I().ocgcore.materialCards[0].GetData().Reason)))
                    {
                        Program.I().ocgcore.description.Hide();
                        Program.I().ocgcore.list.Hide();
                        Program.I().ocgcore.summonCard = this;
                        StartCoroutine(Program.I().timeline_.SummonMaterial());
                        goto SummonPass;
                    }
                    else
                    {
                        bool cutin = MonsterCutin.HasCutin(data.Id);
                        if (cutin)
                            MonsterCutin.Play(data.Id, (int)p.controller);
                        if (NeedStrongSummon(data))
                            SequenceStrongSummon(sequence, position, rotation, cutin ? 1.6f : 0, timePassed);
                        else
                            SequenceNormalSummon(sequence, position, rotation, cutin ? 1.6f : 0, timePassed);
                        goto SummonPass;
                    }
                }
                //通常召唤
                else if ((p.position & (uint)CardPosition.FaceUp) > 0
                    && (p.location & (uint)CardLocation.MonsterZone) > 0
                    && (cacheP.location & (uint)CardLocation.Hand) > 0
                    && (p.location & (uint)CardLocation.Overlay) == 0)
                {
                    bool cutin = MonsterCutin.HasCutin(data.Id);
                    if (cutin)
                        MonsterCutin.Play(data.Id, (int)p.controller);
                    if (NeedStrongSummon(data))
                        SequenceStrongSummon(sequence, position, rotation, cutin ? 1.6f : 0);
                    else
                        SequenceNormalSummon(sequence, position, rotation, cutin ? 1.6f : 0);
                    goto SummonPass;
                }

                switch (Program.I().ocgcore.currentMessage)
                {
                    case GameMessage.Draw:
                        moveTime = 0.15f;
                        break;
                    case GameMessage.Move:
                        if ((p.location & (uint)CardLocation.Onfield) > 0)
                            moveTime = 0.3f;
                        else
                            moveTime = 0.2f;
                        break;
                    case GameMessage.FlipSummoning:
                    case GameMessage.PosChange:
                        moveTime = 0.1f;
                        break;
                    case GameMessage.ShuffleSetCard:
                    case GameMessage.Swap:
                        moveTime = 0.2f;
                        break;
                }
                var cardPlane = manager.GetElement<Transform>("CardPlane");
                var pivot = manager.GetElement<Transform>("Pivot");
                var offset = manager.GetElement<Transform>("Offset");
                var turn = manager.GetElement<Transform>("Turn");

                //主体移动
                sequence.Append(model.transform.DOLocalMove(position, moveTime).OnStart(() =>
                {
                    if ((cacheP.location & (uint)CardLocation.Extra) > 0
                        && (p.location & (uint)CardLocation.Extra) == 0
                        && cacheP.sequence == Program.I().ocgcore.GetLocationCardCount(CardLocation.Extra, cacheP.controller) - 1)
                        StartCoroutine(Program.I().ocgcore.UpdateDeckTop(cacheP.controller));
                }));
                sequence.Join(pivot.DOScale(GetCardScale(p), moveTime * 0.95f));
                //Turn
                if ((p.location & (uint)CardLocation.Removed) > 0
                    || (p.location & (uint)CardLocation.Deck) > 0
                    || (p.location & (uint)CardLocation.Extra) > 0)
                    sequence.Join(turn.DOLocalRotate(new Vector3(0, 0, rotation.z), moveTime * 0.5f));
                else
                    sequence.Join(turn.DOLocalRotate(new Vector3(0, (rotation.y == 0) || (rotation.y == 180) ? 0 : 270, rotation.z), moveTime * 0.5f));
                //CardPlane
                if ((p.location & (uint)CardLocation.Deck) > 0
                    || (p.location & (uint)CardLocation.Extra) > 0
                    || (p.location & (uint)CardLocation.Removed) > 0)
                    sequence.Join(cardPlane.DOLocalRotate(new Vector3(rotation.x, rotation.y, 0), moveTime * 0.5f));
                else
                    sequence.Join(cardPlane.DOLocalRotate(new Vector3(rotation.x, (rotation.y == 0 || rotation.y == 270) ? 0 : 180, 0), moveTime * 0.5f));

                //Pivot && Offset
                if ((p.location & (uint)CardLocation.Hand) > 0)
                {
                    sequence.Join(pivot.DOLocalMove(new Vector3(0, 0, HandOffsetPositionByX(position.x)), moveTime / 4));
                    sequence.Join(offset.DOLocalRotate(new Vector3(0, HandOffsetRotationByX(position.x), handAngle), moveTime / 4));
                    sequence.Join(turn.DOLocalRotate(new Vector3(0, 0, data.Id > 0 ? 0 : 180), moveTime / 4));
                    handDefault = true;
                }
                else
                {
                    sequence.Join(offset.DOLocalMove(Vector3.zero, moveTime / 4));
                    sequence.Join(offset.DOLocalRotate(Vector3.zero, 0.21f));
                    sequence.Join(pivot.DOLocalMove(Vector3.zero, moveTime / 4));
                    sequence.Join(pivot.DOLocalRotate(Vector3.zero, moveTime / 4));
                }

                if ((p.location & ((uint)CardLocation.Grave + (uint)CardLocation.Removed)) > 0)
                {
                    OcgCore.messagePass = true;
                    SequenceToGrave(sequence, p);
                    moveTime = 0.1f;
                }

                //Overlay Out
                if ((cacheP.location & (uint)CardLocation.Overlay) > 0
                    && ((p.reason & (uint)CardReason.EFFECT) > 0 || (p.location & (uint)CardLocation.Overlay) == 0)
                    && (p.reason & (uint)CardReason.RULE) == 0)
                {
                    se = "SE_CARD_XYZ_OUT";
                    var fx = ABLoader.LoadFromFile("effects/buff/fxp_bff_overlay/fxp_bff_overlay_out_001", true);
                    fx.transform.position = GetCardPosition(cacheP);
                    Destroy(fx, 3f);
                }

                //Overlay In
                if ((cacheP.location & (uint)CardLocation.Overlay) == 0
                    && (p.location & (uint)CardLocation.Extra) == 0
                    && (p.location & (uint)CardLocation.Overlay) > 0
                    && (p.reason & (uint)CardReason.Xyz) > 0)
                {
                    se = "";
                    DOTween.To(v => { }, 0, 0, moveTime + timePassed).OnComplete(() =>
                    {
                        AudioManager.PlaySE("SE_CARD_XYZ_IN");
                        var fx = ABLoader.LoadFromFile("effects/buff/fxp_bff_overlay/fxp_bff_overlay_in_001", true);
                        fx.transform.position = GetCardPosition(p);
                        Destroy(fx, 3f);
                    });
                }

                sequence.OnComplete(() =>
                {
                    if (!ThisLocationShouldHaveModel(p) && model != null)
                        Destroy(model);
                    inAnimation = false;
                    if ((p.location & ((uint)CardLocation.Grave + (uint)CardLocation.Removed)) == 0)
                        OcgCore.messagePass = true;
                    if((p.location & (uint)CardLocation.Extra) > 0 
                        && (cacheP.location & (uint)CardLocation.Extra) == 0)
                        Program.I().ocgcore.SetDeckTop(this);
                });

            SummonPass:
                AudioManager.PlaySE(se);

                if (model != null)
                {
                    if (p.controller == 0)
                        model.transform.SetParent(Program.I().ocgcore.field0Manager.transform, true);
                    else
                        model.transform.SetParent(Program.I().ocgcore.field1Manager.transform, true);
                }
                return moveTime + timePassed + 0.05f;
            }
        }

        public void StrongSummonLand(Vector3 fromPosition, Vector3 fromRotation)
        {
            if (model == null)
                return;

            ResetModelPositon();
            model.transform.localPosition = fromPosition;
            model.transform.eulerAngles = fromRotation;
            var position = GetCardPosition(p);
            var rotaion = GetCardRotation(p);
            var sequence = DOTween.Sequence();
            float interval;
            if (MonsterCutin.HasCutin(data.Id))
            {
                if ((data.Type & (uint)CardType.Fusion) > 0)
                    interval = 1f;
                else
                    interval = 1.3f;
            }
            else
            {
                if ((data.Type & (uint)CardType.Fusion) > 0)
                    interval = 0.6f;
                else
                    interval = 0.8f;
            }
            SequenceStrongSummon(sequence, position, rotaion, interval);
        }

        void SequenceStrongSummon(Sequence sequence, Vector3 position, Vector3 angle, float interval, float timeBefore = 0)
        {
            sequence.AppendInterval(interval);
            sequence.Append(manager.transform.DOMove(position, 0.2f).OnStart(() =>
            {
                if ((cacheP.location & (uint)CardLocation.Extra) > 0
                && (p.location & (uint)CardLocation.Extra) == 0
                && cacheP.sequence == Program.I().ocgcore.GetLocationCardCount(CardLocation.Extra, cacheP.controller))
                    StartCoroutine(Program.I().ocgcore.UpdateDeckTop(cacheP.controller));
            }));
            sequence.Join(manager.transform.DOLocalRotate(Vector3.zero, 0.1f));
            sequence.Join(manager.GetElement<Transform>("CardPlane").DOLocalRotate(new Vector3(0, (angle.y == 0) || (angle.y == 270) ? 0 : 180, 0), 0.2f));
            sequence.Join(manager.GetElement<Transform>("Turn").DOLocalRotate(new Vector3(0, (angle.y == 0) || (angle.y == 180) ? 0 : 270, angle.z), 0.2f));
            sequence.Join(manager.GetElement<Transform>("Pivot").DOLocalMove(Vector3.zero, 0.1f));
            sequence.Join(manager.GetElement<Transform>("Pivot").DOLocalRotate(Vector3.zero, 0.1f));
            sequence.Join(manager.GetElement<Transform>("Pivot").DOScale(GetCardScale(p), 0.2f));
            sequence.Join(manager.GetElement<Transform>("Offset").DOLocalMove(Vector3.zero, 0.1f));
            sequence.Join(manager.GetElement<Transform>("Offset").DOLocalRotate(Vector3.zero, 0.1f));

            sequence.Insert(timeBefore + interval + 0.1f, manager.GetElement<Transform>("Pivot").DOLocalRotate(new Vector3(-30, 0, 0), 0.4f).SetEase(Ease.OutQuart));
            sequence.Insert(timeBefore + interval + 0.1f, manager.GetElement<Transform>("Pivot").DOLocalMoveY(22, 0.4f).SetEase(Ease.OutQuart));

            sequence.Append(manager.GetElement<Transform>("Pivot").DOLocalRotate(Vector3.zero, 0.1f).SetEase(Ease.InExpo));
            sequence.Join(manager.GetElement<Transform>("Pivot").DOLocalMoveY(0, 0.1f).SetEase(Ease.InExpo));

            sequence.OnComplete(() =>
            {
                inAnimation = false;
                OcgCore.messagePass = true;
            });
        }
        void SequenceNormalSummon(Sequence sequence, Vector3 position, Vector3 angle, float interval, float timeBefore = 0)
        {
            sequence.AppendInterval(interval);
            sequence.Append(model.transform.DOMove(position, 0.2f));
            sequence.Join(manager.transform.DOLocalRotate(Vector3.zero, 0.1f));
            sequence.Join(manager.GetElement<Transform>("CardPlane").DOLocalRotate(new Vector3(0, (angle.y == 0) || (angle.y == 270) ? 0 : 180, 0), 0.2f));
            sequence.Join(manager.GetElement<Transform>("Turn").DOLocalRotate(new Vector3(0, (angle.y == 0) || (angle.y == 180) ? 0 : 270, angle.z), 0.2f));
            sequence.Join(manager.GetElement<Transform>("Pivot").DOLocalMove(Vector3.zero, 0.1f));
            sequence.Join(manager.GetElement<Transform>("Pivot").DOLocalRotate(Vector3.zero, 0.1f));
            sequence.Join(manager.GetElement<Transform>("Pivot").DOScale(GetCardScale(p), 0.2f));
            sequence.Join(manager.GetElement<Transform>("Offset").DOLocalMove(Vector3.zero, 0.1f));
            sequence.Join(manager.GetElement<Transform>("Offset").DOLocalRotate(Vector3.zero, 0.1f));

            sequence.Insert(timeBefore + interval + 0.1f, manager.GetElement<Transform>("Pivot").DOLocalRotate(new Vector3(-10, 0, 0), 0.2f).SetEase(Ease.OutQuart));
            sequence.Insert(timeBefore + interval + 0.1f, manager.GetElement<Transform>("Pivot").DOLocalMoveY(10, 0.2f).SetEase(Ease.OutQuart));

            sequence.Append(manager.GetElement<Transform>("Pivot").DOLocalRotate(Vector3.zero, 0.1f).SetEase(Ease.InExpo));
            sequence.Join(manager.GetElement<Transform>("Pivot").DOLocalMoveY(0, 0.1f).SetEase(Ease.InExpo));

            sequence.OnComplete(() =>
            {
                inAnimation = false;
                OcgCore.messagePass = true;
            });
        }

        float SequenceFromGrave(Sequence sequence, GPS p)
        {
            var timeUp = 0.3f;
            var timeStay = 0.2f;
            var pivot = manager.GetElement<Transform>("Pivot");
            pivot.localPosition = new Vector3(0, -5, 0);
            pivot.localScale = Vector3.one * 0.2f;
            pivot.localEulerAngles = new Vector3(270, 0, 0);

            sequence.Append(pivot.DOScale(Vector3.one, timeUp).SetEase(Ease.InCubic));
            sequence.Join(pivot.DOLocalRotate(Vector3.zero, timeUp).SetEase(Ease.InCubic));
            sequence.Join(pivot.DOLocalMove(Vector3.zero, timeUp).SetEase(Ease.InCubic));
            sequence.AppendInterval(timeStay);
            Program.I().ocgcore.GraveBgEffect(p, false);
            return timeUp + timeStay;
        }
        float SequenceToGrave(Sequence sequence, GPS p)
        {
            var timeDown = 0.3f;
            var timeStay = 0.2f;
            var pivot = manager.GetElement<Transform>("Pivot");

            sequence.AppendInterval(timeStay).OnStart(() =>
            {
                Program.I().ocgcore.GraveBgEffect(p, true);
            });
            sequence.Append(pivot.DOScale(Vector3.one * 0.2f, timeDown).SetEase(Ease.OutCubic).OnStart(() =>
            {
                //Program.I().ocgcore.GraveBgEffect(p, true);
            }));
            sequence.Join(pivot.DOLocalRotate(new Vector3(270, 0, 0), timeDown).SetEase(Ease.OutCubic));
            sequence.Join(pivot.DOLocalMove(new Vector3(0, -5, 0), timeDown).SetEase(Ease.OutCubic));
            return timeDown + timeStay;
        }

        void ResetModelPositon()
        {
            if (model == null)
                return;
            model.transform.localEulerAngles = Vector3.zero;
            manager.GetElement<Transform>("CardPlane").localEulerAngles = Vector3.zero;
            manager.GetElement<Transform>("CardPlane").localPosition = Vector3.zero;
            manager.GetElement<Transform>("Pivot").localScale = GetCardScale(p);
            manager.GetElement<Transform>("Pivot").localEulerAngles = Vector3.zero;
            manager.GetElement<Transform>("Pivot").localPosition = Vector3.zero;
            manager.GetElement<Transform>("Offset").localEulerAngles = Vector3.zero;
            manager.GetElement<Transform>("Offset").localPosition = Vector3.zero;
            manager.GetElement<Transform>("Turn").localEulerAngles = Vector3.zero;
            manager.GetElement<Transform>("Turn").localPosition = Vector3.zero;
        }

        #region Animation

        public static float handAngle = -10f;

        void ModelAt(GPS gps, GameObject model = null)
        {
            ElementObjectManager manager;
            if (model == null)
            {
                model = this.model;
                manager = model.GetComponent<ElementObjectManager>();
            }
            else
                manager = this.manager;
            model.transform.localPosition = GetCardPosition(gps, this);
            Vector3 rotation = GetCardRotation(gps, data.Id);

            var cardPlane = manager.GetElement<Transform>("CardPlane");
            if ((gps.location & (uint)CardLocation.Deck) > 0
                || (gps.location & (uint)CardLocation.Extra) > 0
                || (gps.location & (uint)CardLocation.Removed) > 0)
                cardPlane.localEulerAngles = new Vector3(rotation.x, rotation.y, 0);
            else
                cardPlane.localEulerAngles = new Vector3(rotation.x, (rotation.y == 0 || rotation.y == 270) ? 0 : 180, 0);

            manager.GetElement<Transform>("Pivot").localScale = GetCardScale(p);

            if ((rotation.y == 90 || rotation.y == 270) && (gps.location & (uint)CardLocation.Removed) == 0)
                manager.GetElement<Transform>("Turn").localEulerAngles = new Vector3(0, 270, rotation.z);
            else
                manager.GetElement<Transform>("Turn").localEulerAngles = new Vector3(0, 0, rotation.z);
        }

        public void AnimationShuffle(float shuffleTime)
        {
            inAnimation = true;
            if ((p.location & (uint)CardLocation.Hand) == 0)
                return;
            if (model != null)
            {
                manager.GetElement<Transform>("Pivot").DOLocalMoveZ(0, shuffleTime);
                manager.GetElement<Transform>("Offset").DOLocalRotate(Vector3.zero, shuffleTime);
                manager.GetElement<Transform>("Turn").DOLocalRotate(new Vector3(0, 0, 180), shuffleTime);

                var x = model.transform.position.x;
                model.transform.DOLocalMoveX(0, shuffleTime).OnComplete(() =>
                {
                    if (Program.I().ocgcore.cards.Contains(this))
                        AnimationHandDefault(shuffleTime, true);
                    else
                        Dispose();
                });
            }
            else// model == null for TagSwap
            {
                DOTween.To(v => { }, 0, 0, shuffleTime).OnComplete(() =>
                {
                    CreateModel();
                    ModelAt(p);
                    manager.GetElement<Transform>("Pivot").localPosition = Vector3.zero;
                    manager.GetElement<Transform>("Offset").localEulerAngles = Vector3.zero;
                    manager.GetElement<Transform>("Turn").localEulerAngles = new Vector3(0, 0, 180);
                    var position = model.transform.localPosition;
                    model.transform.localPosition = new Vector3(0, position.y, position.z);
                    AnimationHandDefault(shuffleTime, true);
                });
            }
            DOTween.To(v => { }, 0, 0, shuffleTime - 0.1f).OnComplete(() =>
            {
                OcgCore.messagePass = true;
            });
        }

        bool handDefault;
        public float HandOffsetRotationByX(float x)
        {
            var abs = x > 0 ? x : -x;
            return x * (abs * -0.006f + 1.2f) * ((p.controller == 0) ? 1 : -1);
        }
        public float HandOffsetPositionByX(float x)
        {
            var abs = x > 0 ? x : -x;
            return -abs * (abs * 0.0055f + 0.08f);
        }

        public void AnimationHandDefault(float time, bool ignore = false)
        {
            if (model == null || (p.location & (uint)CardLocation.Hand) == 0 || (inAnimation && !ignore))
                return;
            model.transform.SetParent(null, true);
            handDefault = true;
            appealed = false;
            var targetPosition = GetCardPosition(p, this);
            var x = targetPosition.x;
            model.transform.DOLocalMove(targetPosition, time);

            Transform pivot = manager.GetElement<Transform>("Pivot");
            Transform offset = manager.GetElement<Transform>("Offset");
            Transform turn = manager.GetElement<Transform>("Turn");
            pivot.DOLocalMove(new Vector3(0, 0, HandOffsetPositionByX(x)), time).OnComplete(() =>
            {
                if (ignore)
                    inAnimation = false;
            });
            offset.DOLocalMove(Vector3.zero, time);
            offset.DOLocalRotate(new Vector3(0, HandOffsetRotationByX(x), handAngle), time);
            if (data.Id == 0)
                turn.DOLocalRotate(new Vector3(0, 0, 180), time);
            else
                turn.DOLocalRotate(Vector3.zero, time);
        }
        public void SetHandToDefault()
        {
            if (model == null || (p.location & (uint)CardLocation.Hand) == 0 || inAnimation)
                return;

            clicked = false;
            handDefault = false;
        }
        public void SetHandDefault()
        {
            if (model == null || (p.location & (uint)CardLocation.Hand) == 0)
                return;
            appealed = false;
            model.transform.localPosition = GetCardPosition(p, this);
            float x = model.transform.localPosition.x;
            manager.GetElement<Transform>("Pivot").localPosition = new Vector3(0, 0, HandOffsetPositionByX(x));
            manager.GetElement<Transform>("Offset").localPosition = Vector3.zero;
            manager.GetElement<Transform>("Offset").localEulerAngles = new Vector3(0, HandOffsetRotationByX(x), handAngle);
            manager.GetElement<Transform>("Turn").localEulerAngles = new Vector3(0, 0, (data.Id == 0) ? 180 : 0);
        }
        void AnimationHandHover()
        {
            if (inAnimation)
                return;
            var offset = manager.GetElement<Transform>("Offset");
            offset.DOLocalMove(new Vector3(0, 2, 1), 0.1f);
        }

        bool appealed = false;
        public void AnimationHandAppeal()
        {
            if (appealed || inAnimation)
                return;
            appealed = true;
            manager.GetElement<Transform>("Pivot").DOLocalMove(new Vector3(0, 2, 3), 0.1f);
            manager.GetElement<Transform>("Offset").DOLocalRotate(Vector3.zero, 0.1f);
            manager.GetElement<Transform>("Offset").DOLocalMove(Vector3.zero, 0.1f);
            AudioManager.PlaySE("SE_CARD_MOVE_0" + UnityEngine.Random.Range(1, 5));
        }
        public void AnimationNegate()
        {
            AudioManager.PlaySE("SE_EFFECT_INVALID");
            CameraManager.BlackInOut(0f, 0.2f, 0.5f, 0.3f);
            ElementObjectManager manager;
            GameObject model;
            if (ThisLocationShouldHaveModel(p))
            {
                model = this.model;
                manager = this.manager;
            }
            else
            {
                model = CreateModel(false);
                ModelAt(p, model);
                manager = model.GetComponent<ElementObjectManager>();
            }
            Tools.ChangeLayer(model, "DuelOverlay3D");
            CameraManager.DuelOverlay3DPlus();
            manager.GetElement("EffectNegate").SetActive(false);
            manager.GetElement("EffectNegate").SetActive(true);
            var pivot = manager.GetElement<Transform>("Pivot");
            var offset = manager.GetElement<Transform>("Offset");
            var scale = pivot.localScale;
            var sequence = DOTween.Sequence();
            if ((p.location & (uint)CardLocation.Onfield) > 0
            || (p.location & (uint)CardLocation.Extra) > 0
            || (p.location & (uint)CardLocation.Deck) > 0)
            {
                HideLabel();
                sequence.Append(offset.DOLocalMoveY(5, 0.1f));
                sequence.AppendInterval(0.5f);
                sequence.Append(offset.DOLocalMoveY(0f, 0.2f));
                sequence.Insert(0, pivot.DOScale(1f, 0.1f));
                sequence.Insert(0.6f, pivot.DOScale(scale, 0.2f));
                sequence.OnComplete(() =>
                {
                    Tools.ChangeLayer(model, "Default");
                    CameraManager.DuelOverlay3DMinus();
                    RefreshLabel();
                    if (!ThisLocationShouldHaveModel(p))
                        Destroy(model);
                });
            }
            else if ((p.location & (uint)CardLocation.Hand) > 0)
            {
                inAnimation = true;
                if (p.controller != 0)
                    manager.GetElement<Transform>("Turn").DOLocalRotate(Vector3.zero, 0.1f);
                var originRotaion = pivot.localEulerAngles;

                sequence.Append(offset.DOLocalMoveY(1, 0.1f));
                sequence.Join(offset.DOLocalMoveZ(5, 0.1f));
                sequence.Join(offset.DOLocalRotate(Vector3.zero, 0.1f));
                sequence.Join(pivot.DOLocalRotate(Vector3.zero, 0.1f));
                sequence.Join(manager.GetElement<Transform>("Turn").DOLocalRotate(Vector3.zero, 0.1f));
                sequence.Append(offset.DOLocalMoveY(1.2f, 0.5f));
                sequence.Join(offset.DOLocalMoveZ(5.5f, 0.5f));
                sequence.Append(offset.DOLocalMoveY(0f, 0.2f));
                sequence.Join(offset.DOLocalMoveZ(0f, 0.2f));
                sequence.Join(pivot.DOLocalRotate(originRotaion, 0.15f));
                sequence.Insert(0, pivot.DOScale(1.2f, 0.2f));
                sequence.Insert(0.6f, pivot.DOScale(scale, 0.2f));
                sequence.OnComplete(() =>
                {
                    Tools.ChangeLayer(model, "Default");
                    CameraManager.DuelOverlay3DMinus();
                    inAnimation = false;
                });
            }
            else if ((p.location & (uint)CardLocation.Grave) > 0
                || (p.location & (uint)CardLocation.Removed) > 0)
            {
                offset.localPosition = new Vector3(0, -5, 0);
                sequence.Append(offset.DOLocalMoveY(0, 0.1f));
                sequence.AppendInterval(0.5f);
                sequence.Append(offset.DOLocalMoveY(-5f, 0.2f));
                sequence.Insert(0, offset.DOScale(1f, 0.1f));
                sequence.Insert(0.6f, offset.DOScale(Vector3.one * 0.2f, 0.2f));
                sequence.OnComplete(() =>
                {
                    Destroy(model);
                    CameraManager.DuelOverlay3DMinus();
                });
            }

        }
        public void AnimationActivate()
        {
            AudioManager.PlaySE("SE_CARDVIEW_01");
            CameraManager.BlackInOut(0f, 0.3f, 0.4f, 0.3f);
            ElementObjectManager manager;
            GameObject model;
            if (ThisLocationShouldHaveModel(p))
            {
                model = this.model;
                manager = this.manager;
            }
            else
            {
                model = CreateModel(false);
                ModelAt(p, model);
                manager = model.GetComponent<ElementObjectManager>();
            }
            Tools.ChangeLayer(model, "DuelOverlay3D");
            CameraManager.DuelOverlay3DPlus();
            manager.GetElement("EffectBuffActive").SetActive(false);
            manager.GetElement("EffectBuffActive").SetActive(true);
            var pivot = manager.GetElement<Transform>("Pivot");
            var offset = manager.GetElement<Transform>("Offset");
            var scale = pivot.localScale;
            var sequence = DOTween.Sequence();

            if ((p.location & (uint)CardLocation.Onfield) > 0
            || (p.location & (uint)CardLocation.Extra) > 0
            || (p.location & (uint)CardLocation.Deck) > 0)
            {
                HideLabel();
                sequence.Append(offset.DOLocalMoveY(5, 0.15f));
                sequence.Append(offset.DOLocalMoveY(5.5f, 0.5f));
                sequence.Append(offset.DOLocalMoveY(0f, 0.2f));
                sequence.Insert(0, pivot.DOScale(1.2f, 0.65f));
                sequence.Insert(0.65f, pivot.DOScale(scale, 0.2f));
                sequence.OnComplete(() =>
                {
                    Tools.ChangeLayer(model, "Default");
                    CameraManager.DuelOverlay3DMinus();
                    RefreshLabel();
                    if (!ThisLocationShouldHaveModel(p))
                        Destroy(model);
                });
            }
            else if ((p.location & (uint)CardLocation.Hand) > 0)
            {
                inAnimation = true;
                if (p.controller != 0)
                    manager.GetElement<Transform>("Turn").DOLocalRotate(Vector3.zero, 0.1f);
                var originRotaion = pivot.localEulerAngles;

                sequence.Append(offset.DOLocalMoveY(1, 0.15f));
                sequence.Join(offset.DOLocalMoveZ(5, 0.15f));
                sequence.Join(offset.DOLocalRotate(Vector3.zero, 0.15f));
                sequence.Join(pivot.DOLocalRotate(Vector3.zero, 0.15f));
                sequence.Join(manager.GetElement<Transform>("Turn").DOLocalRotate(Vector3.zero, 0.15f));
                sequence.Append(offset.DOLocalMoveY(1.2f, 0.5f));
                sequence.Join(offset.DOLocalMoveZ(5.5f, 0.5f));
                sequence.Append(offset.DOLocalMoveY(0f, 0.2f));
                sequence.Join(offset.DOLocalMoveZ(0f, 0.2f));
                sequence.Join(pivot.DOLocalRotate(originRotaion, 0.15f));
                sequence.Insert(0, pivot.DOScale(1.2f, 0.65f));
                sequence.Insert(0.65f, pivot.DOScale(scale, 0.2f));
                sequence.OnComplete(() =>
                {
                    Tools.ChangeLayer(model, "Default");
                    CameraManager.DuelOverlay3DMinus();
                    inAnimation = false;
                });
            }
            else if ((p.location & (uint)CardLocation.Grave) > 0
                || (p.location & (uint)CardLocation.Removed) > 0)
            {
                offset.localPosition = new Vector3(0, -5, 0);
                sequence.Append(offset.DOLocalMoveY(0, 0.15f));
                sequence.Append(offset.DOLocalMoveY(0.5f, 0.5f));
                sequence.Append(offset.DOLocalMoveY(-5f, 0.2f));
                sequence.Insert(0, offset.DOScale(1f, 0.1f));
                sequence.Insert(0.1f, offset.DOScale(1.2f, 0.55f));
                sequence.Insert(0.65f, offset.DOScale(Vector3.zero, 0.2f));
                sequence.OnComplete(() =>
                {
                    Destroy(model);
                    CameraManager.DuelOverlay3DMinus();
                });
            }
        }

        public void AnimationConfirm(int id)
        {
            if (!ThisLocationShouldHaveModel(p))
            {
                CreateModel();
                ModelAt(p);
            }
            inAnimation = true;
            var offset = manager.GetElement<Transform>("Offset");
            var offsetPosition = offset.localPosition;
            var turn = manager.GetElement<Transform>("Turn");
            var turnEulerAngles = turn.localEulerAngles;
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(id);
            sequence.Append(offset.DOLocalMove(new Vector3(0, 2, 3), 0.1f).OnStart(() => 
            {
                AudioManager.PlaySE("SE_CARDVIEW_02");
            }));
            sequence.Join(turn.DOLocalRotate(Vector3.zero, 0.1f).OnComplete(() =>
            {
                var highlight = ABLoader.LoadFromFile("Effects/other/fxp_card_decide_001", true);
                highlight.transform.position = offset.position;
                highlight.transform.rotation = offset.rotation;
                Destroy(highlight, 1f);
            }));
            sequence.AppendInterval(0.8f);
            sequence.OnComplete(() =>
            {
                inAnimation = false;

                if ((p.location & (uint)CardLocation.Hand) > 0)
                    SetHandToDefault();
                else
                {
                    offset.DOLocalMove(offsetPosition, 0.1f);
                    turn.DOLocalRotate(turnEulerAngles, 0.1f).OnComplete(() =>
                    {
                        if (!ThisLocationShouldHaveModel(p) && model != null)
                            Destroy(model);
                    });
                }
            });
        }
        public void AnimationPositon(float delay = 0)
        {
            if (model == null)
                return;

            var positionManager = manager.GetElement<ElementObjectManager>("FieldCardChangeIcon");
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(delay);
            if ((p.position & (uint)CardPosition.Attack) > 0)
            {
                AudioManager.PlaySE("SE_DISP_ATTACK", 0.6f);
                positionManager.GetElement<Transform>("Sword1").localEulerAngles = new Vector3(90, 0, 0);
                positionManager.GetElement<Transform>("Sword2").localEulerAngles = new Vector3(90, 0, 0);
                sequence.Append(positionManager.GetElement<Transform>("Sword1").DOLocalRotate(new Vector3(90, 0, 60), 0.2f).SetEase(Ease.OutCubic));
                sequence.Join(positionManager.GetElement<Transform>("Sword2").DOLocalRotate(new Vector3(90, 0, -60), 0.2f).SetEase(Ease.OutCubic));
                sequence.Join(positionManager.GetElement<SpriteRenderer>("Sword1").DOFade(1, 0.35f).SetEase(Ease.OutCubic));
                sequence.Join(positionManager.GetElement<SpriteRenderer>("Sword2").DOFade(1, 0.35f).SetEase(Ease.OutCubic));
                sequence.Insert(0.2f, positionManager.GetElement<Transform>("Sword1").DOLocalRotate(new Vector3(90, 0, 45), 0.15f).SetEase(Ease.OutCubic));
                sequence.Insert(0.2f, positionManager.GetElement<Transform>("Sword2").DOLocalRotate(new Vector3(90, 0, -45), 0.15f).SetEase(Ease.OutCubic));
                sequence.AppendInterval(0.2f);
                sequence.Append(positionManager.GetElement<SpriteRenderer>("Sword1").DOFade(0, 0.3f).SetEase(Ease.InQuad));
                sequence.Join(positionManager.GetElement<SpriteRenderer>("Sword2").DOFade(0, 0.3f).SetEase(Ease.InQuad));
            }
            else
            {
                AudioManager.PlaySE("SE_DISP_DEFENS", 0.6f);
                positionManager.GetElement<Transform>("Defense").localScale = new Vector3(3, 3, 3);
                sequence.Append(positionManager.GetElement<Transform>("Defense").DOScale(new Vector3(3.8f, 3.8f, 3.8f), 0.35f));
                sequence.Join(positionManager.GetElement<SpriteRenderer>("Defense").DOFade(1, 0.3f).SetEase(Ease.OutCubic));
                sequence.AppendInterval(0.2f);
                sequence.Append(positionManager.GetElement<SpriteRenderer>("Defense").DOFade(0, 0.3f).SetEase(Ease.InCubic));
            }
        }
        public void AnimationTarget()
        {
            AudioManager.PlaySE("SE_CEMETERY_CARD");

            GameObject model;
            if (ThisLocationShouldHaveModel(p))
                model = this.model;
            else
            {
                model = CreateModel(false);
                ModelAt(p, model);
                Destroy(model, 0.49f);
            }

            var fx = ABLoader.LoadFromFile("effects/other/fxp_card_decide_001", true);
            fx.transform.position = model.transform.position;
            if ((p.location & (uint)CardLocation.MonsterZone) > 0 && (p.position & (uint)CardPosition.Defence) > 0)
                fx.transform.localEulerAngles = new Vector3(0, 90, 0);
            if ((p.location & (uint)CardLocation.Removed) > 0)
                fx.transform.localEulerAngles = new Vector3(0, 90, 0);
            if ((p.location & (uint)CardLocation.SpellZone) > 0)
                fx.transform.localScale = Vector3.one * 0.8f;
            if ((p.location & ((uint)CardLocation.Deck + (uint)CardLocation.Extra)) > 0)
            {
                fx.transform.localEulerAngles = new Vector3(0, GetCardRotation(p).y, 0);
            }
            Destroy(fx, 1f);
        }
        public void AnimationLandShake(GameCard card, bool huge)
        {
            if (card == this)
                return;
            if ((p.location & (uint)CardLocation.Onfield) == 0)
                return;
            if (model == null)//Overlays
                return;
            if (huge)
            {
                model.transform.DOShakePosition(0.6f, Vector3.one * 0.5f, 10, 90, false, true, ShakeRandomnessMode.Harmonic);
                model.transform.DOShakeRotation(0.6f, 10f);
                Sequence sequence = DOTween.Sequence();
                sequence.Append(model.transform.DOLocalMoveY(6, 0.3f));
                sequence.Append(model.transform.DOLocalMoveY(0.2f, 0.3f));
            }
            else
            {
                model.transform.DOShakePosition(0.4f, Vector3.one * 0.2f, 10, 90, false, true, ShakeRandomnessMode.Harmonic);
                model.transform.DOShakeRotation(0.4f, 5f);
                Sequence sequence = DOTween.Sequence();
                sequence.Append(model.transform.DOLocalMoveY(3, 0.2f));
                sequence.Append(model.transform.DOLocalMoveY(0.2f, 0.2f));
            }
        }
        public void AnimationConfirmDeckTop(int id)
        {
            CreateModel();
            ModelAt(p);
            model.SetActive(false);
            var turn = manager.GetElement<Transform>("Turn");
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(1f * id);
            sequence.Append(turn.DOLocalMoveY(2, 0.1f).OnStart(() =>
            {
                Program.I().ocgcore.description.Show(this, null);
                model.SetActive(true);
                if (Program.I().ocgcore.GetLocationCardCount(CardLocation.Deck, p.controller) == 1)
                {
                    if (p.controller == 0)
                        Program.I().ocgcore.myDeck.gameObject.SetActive(false);
                    else
                        Program.I().ocgcore.opDeck.gameObject.SetActive(false);
                }
            }));
            sequence.Join(turn.DOLocalRotate(Vector3.zero, 0.1f));
            sequence.Append(turn.DOLocalMoveY(0.1f * (id + 1), 0.1f).OnComplete(() =>
            {
                AudioManager.PlaySE("SE_CARDVIEW_02");
                var effect = ABLoader.LoadFromFile("Effects/other/fxp_card_decide_deck_001", true);
                effect.transform.position = turn.position;
                effect.transform.rotation = turn.rotation;
                Destroy(effect, 1f);
            }));
            sequence.AppendInterval(0.6f);
            sequence.Append(turn.DOLocalMoveY(2, 0.1f));
            sequence.Join(turn.DOLocalRotate(new Vector3(0, 0, 180), 0.1f));
            sequence.Append(turn.DOLocalMoveY(0f, 0.1f));
            sequence.OnComplete(() =>
            {
                Destroy(model);
                if (p.controller == 0)
                    Program.I().ocgcore.myDeck.gameObject.SetActive(true);
                else
                    Program.I().ocgcore.opDeck.gameObject.SetActive(true);
            });
        }

        #endregion

        #region Button

        public struct DuelButtonInfo
        {
            public List<int> response;
            public string hint;
            public ButtonType type;
        }
        public List<DuelButtonInfo> buttons = new List<DuelButtonInfo>();
        List<DuelButton> buttonObjs = new List<DuelButton>();

        public void AddButton(int response, string hint, ButtonType type)
        {
            bool exist = false;
            foreach (var button in buttons)
                if (button.type == type)
                {
                    exist = true;
                    button.response.Add(response);
                }
            if (!exist)
                buttons.Add(new DuelButtonInfo() { response = new List<int>() { response }, hint = hint, type = type });
        }
        bool hightYellow = false;
        public void CreateButtons()
        {
            if (model == null || buttons.Count == 0)
                return;
            buttons.Sort((x, y) => x.type.CompareTo(y.type));
            for (int i = 0; i < buttons.Count; i++)
            {
                var obj = Instantiate(Program.I().ocgcore.container.duelButton);
                var mono = obj.GetComponent<DuelButton>();
                buttonObjs.Add(mono);
                mono.response = buttons[i].response;
                mono.hint = buttons[i].hint;
                mono.type = buttons[i].type;
                mono.id = i;
                mono.buttonsCount = buttons.Count;
                mono.cookieCard = this;
            }
            hightYellow = false;
            foreach (var button in buttons)
            {
                if (button.type == ButtonType.Activate
                    || button.type == ButtonType.PenSummon
                    || button.type == ButtonType.SetPendulum
                    || button.type == ButtonType.SpSummon
                    )
                { hightYellow = true; break; }
            }
            if (hightYellow)
                manager.GetElement("EffectHighlightYellow").SetActive(true);
            else
                manager.GetElement("EffectHighlightBlue").SetActive(true);
            var highlightParent = manager.GetElement("EffectHighlightBlue").transform.parent.gameObject;

            if ((p.location & (uint)CardLocation.Hand) > 0)
                Tools.ChangeSortingLayer(highlightParent, "Default");
            else
                Tools.ChangeSortingLayer(highlightParent, "DuelEffect_Low");
        }

        public void ClearButtons()
        {
            buttons.Clear();
            if (model == null)
                return;
            foreach (var button in buttonObjs)
                Destroy(button.gameObject);
            buttonObjs.Clear();
            manager.GetElement("EffectHighlightBlue").SetActive(false);
            manager.GetElement("EffectHighlightYellow").SetActive(false);
            manager.GetElement("EffectHighlightBlueSelect").SetActive(false);
            manager.GetElement("EffectHighlightYellowSelect").SetActive(false);
        }


        #endregion

        #region Label
        public bool labelShowing = false;
        static readonly string upColor = "<color=#00FFFF>";
        static readonly string upGrayColor = "<color=#009999>";
        static readonly string normalColor = "<color=#FFFFFF>";
        static readonly string normalGrayColor = "<color=#999999>";
        static readonly string downColor = "<color=#FF0000>";
        static readonly string downGrayColor = "<color=#990000>";
        static readonly string smallSize = "<size=20>";
        static readonly string normalSize = "<size=25>";

        int attack = 0;
        int defense = 0;
        float changeTime = 0.6f;
        int lastAttribute;
        int lastRace;
        public void RefreshLabel()
        {
            if ((p.location & (uint)CardLocation.Onfield) == 0 || (p.position & (uint)CardPosition.FaceUp) == 0)
            {
                HideLabel();
                return;
            }

            Card origin = CardsManager.Get(data.Id);

            if ((p.location & (uint)CardLocation.MonsterZone) > 0)
            {
                //LinkMarker
                if ((data.Type & (uint)CardType.Link) > 0)
                {
                    manager.GetElement("LinkMarker0").SetActive((data.LinkMarker & (uint)CardLinkMarker.TopLeft) > 0);
                    manager.GetElement("LinkMarker1").SetActive((data.LinkMarker & (uint)CardLinkMarker.Top) > 0);
                    manager.GetElement("LinkMarker2").SetActive((data.LinkMarker & (uint)CardLinkMarker.TopRight) > 0);
                    manager.GetElement("LinkMarker3").SetActive((data.LinkMarker & (uint)CardLinkMarker.Left) > 0);
                    manager.GetElement("LinkMarker4").SetActive((data.LinkMarker & (uint)CardLinkMarker.Right) > 0);
                    manager.GetElement("LinkMarker5").SetActive((data.LinkMarker & (uint)CardLinkMarker.BottomLeft) > 0);
                    manager.GetElement("LinkMarker6").SetActive((data.LinkMarker & (uint)CardLinkMarker.Bottom) > 0);
                    manager.GetElement("LinkMarker7").SetActive((data.LinkMarker & (uint)CardLinkMarker.BottomRight) > 0);
                }
                else
                    for (int i = 0; i < 8; i++)
                        manager.GetElement("LinkMarker" + i).SetActive(false);

                //Overlay Material
                if ((data.Type & (uint)CardType.Xyz) > 0)
                {
                    manager.GetElement("MonsterMaterialsRoot").SetActive(true);
                    int overlayCounts = Program.I().ocgcore.GCS_GetOverlays(this).Count;
                    manager.GetElement<TextMeshPro>("TextMonsterMaterials").text = overlayCounts.ToString();
                }
                else
                    manager.GetElement("MonsterMaterialsRoot").SetActive(false);
                //Attack/Defence
                manager.GetElement("CardAttackBody").SetActive(true);
                var text = manager.GetElement<TextMeshPro>("TextPowerPoint");
                if (!labelShowing)
                {
                    string atkDef = "";

                    if ((data.Type & (uint)CardType.Link) > 0)
                    {
                        if (data.Attack > data.rAttack)
                            atkDef = upColor + data.Attack.ToString() + "</color>";
                        else if (data.Attack < data.rAttack)
                            atkDef = downColor + data.Attack.ToString() + "</color>";
                        else
                            atkDef = normalColor + data.Attack.ToString() + "</color>";
                    }
                    else
                    {
                        int rAtk = data.rAttack;
                        int rDef = data.rDefense;
                        if (rAtk < 0) rAtk = 0;
                        if (rDef < 0) rDef = 0;
                        if (data.Attack > rAtk)
                        {
                            if ((p.position & (uint)CardPosition.Attack) > 0)
                                atkDef += upColor + normalSize;
                            else
                                atkDef += upGrayColor + smallSize;
                        }
                        else if (data.Attack < rAtk)
                        {
                            if ((p.position & (uint)CardPosition.Attack) > 0)
                                atkDef += downColor + normalSize;
                            else
                                atkDef += downGrayColor + smallSize;
                        }
                        else
                        {
                            if ((p.position & (uint)CardPosition.Attack) > 0)
                                atkDef += normalColor + normalSize;
                            else
                                atkDef += normalGrayColor + smallSize;
                        }
                        atkDef += data.Attack.ToString() + "</size></color>/";
                        if (data.Defense > rDef)
                        {
                            if ((p.position & (uint)CardPosition.Attack) > 0)
                                atkDef += upGrayColor + smallSize;
                            else
                                atkDef += upColor + normalSize;
                        }
                        else if (data.Defense < rDef)
                        {
                            if ((p.position & (uint)CardPosition.Attack) > 0)
                                atkDef += downGrayColor + smallSize;
                            else
                                atkDef += downColor + normalSize;
                        }
                        else
                        {
                            if ((p.position & (uint)CardPosition.Attack) > 0)
                                atkDef += normalGrayColor + smallSize;
                            else
                                atkDef += normalColor + normalSize;
                        }
                        atkDef += data.Defense.ToString() + "</color></size>";
                    }
                    text.text = atkDef;
                }
                else
                {
                    if (data.Attack > attack)
                    {
                        AudioManager.PlaySE("SE_BUFF_ATTACK");
                        var buff = manager.GetElement("EffectBuff");
                        buff.SetActive(false);
                        buff.SetActive(true);
                    }
                    else if (data.Attack < attack)
                    {
                        AudioManager.PlaySE("SE_DEBUFF_ATTACK");
                        var buff = manager.GetElement("EffectDebuff");
                        buff.SetActive(false);
                        buff.SetActive(true);
                    }

                    if ((data.Type & (uint)CardType.Link) > 0)
                    {
                        var s1 = "";
                        if (data.Attack > data.rAttack)
                            s1 = upColor;
                        else if (data.Attack < data.rAttack)
                            s1 = downColor;
                        else
                            s1 = normalColor;

                        if (attack != data.Attack)
                        {
                            var originAttack = attack;
                            DOTween.To(() => originAttack, x =>
                            {
                                text.text = s1 + x + "</color>";
                            }, data.Attack, changeTime);
                        }
                        else
                            text.text = s1 + data.Attack.ToString() + "</color>";
                    }
                    else
                    {
                        int rAtk = data.rAttack;
                        int rDef = data.rDefense;
                        if (rAtk < 0) rAtk = 0;
                        if (rDef < 0) rDef = 0;
                        string s1 = "";

                        if (data.Attack > rAtk)
                        {
                            if ((p.position & (uint)CardPosition.Attack) > 0)
                                s1 += upColor + normalSize;
                            else
                                s1 += upGrayColor + smallSize;
                        }
                        else if (data.Attack < rAtk)
                        {
                            if ((p.position & (uint)CardPosition.Attack) > 0)
                                s1 += downColor + normalSize;
                            else
                                s1 += downGrayColor + smallSize;
                        }
                        else
                        {
                            if ((p.position & (uint)CardPosition.Attack) > 0)
                                s1 += normalColor + normalSize;
                            else
                                s1 += normalGrayColor + smallSize;
                        }
                        string s2 = "</size></color>/";
                        string s3 = "";
                        if (data.Defense > rDef)
                        {
                            if ((p.position & (uint)CardPosition.Attack) > 0)
                                s3 += upGrayColor + smallSize;
                            else
                                s3 += upColor + normalSize;
                        }
                        else if (data.Defense < rDef)
                        {
                            if ((p.position & (uint)CardPosition.Attack) > 0)
                                s3 += downGrayColor + smallSize;
                            else
                                s3 += downColor + normalSize;
                        }
                        else
                        {
                            if ((p.position & (uint)CardPosition.Attack) > 0)
                                s3 += normalGrayColor + smallSize;
                            else
                                s3 += normalColor + normalSize;
                        }
                        string s4 = "</color></size>";
                        var originAttack = attack;
                        var originDefense = defense;
                        if (data.Attack != attack && data.Defense == defense)
                        {
                            DOTween.To(() => originAttack, x =>
                            {
                                text.text = s1 + x + s2 + s3 + data.Defense + s4;
                            }, data.Attack, changeTime);
                        }
                        else if (data.Attack == attack && data.Defense != defense)
                        {
                            DOTween.To(() => originDefense, x =>
                            {
                                text.text = s1 + data.Attack + s2 + s3 + x + s4;
                            }, data.Defense, changeTime);
                        }
                        else if (data.Attack != attack && data.Defense != defense)
                        {
                            DOTween.To(() => originAttack, x =>
                            {
                                text.text = s1 + x + s2;
                            }, data.Attack, changeTime);
                            DOTween.To(() => originDefense, x =>
                            {
                                text.text += s3 + x + s4;
                            }, data.Defense, changeTime);
                        }
                        else
                            text.text = s1 + data.Attack + s2 + s3 + data.Defense + s4;
                    }
                }
                attack = data.Attack;
                defense = data.Defense;
                manager.GetElement("CardPendulumBody").SetActive(false);
                //Link Count & Level Count
                if ((data.Type & (uint)CardType.Link) > 0)
                {
                    manager.GetElement("LinkCount").SetActive(true);
                    manager.GetElement<TextMeshPro>("TextLinkCount").text = CardDescription.GetCardLinkCount(data).ToString();
                    manager.GetElement("CardLevel").SetActive(false);
                }
                else
                {
                    manager.GetElement("LinkCount").SetActive(false);
                    manager.GetElement("CardLevel").SetActive(true);
                    if ((data.Type & (uint)CardType.Xyz) > 0)
                        manager.GetElement<SpriteRenderer>("IconLevel").sprite = TextureManager.container.typeRank;
                    else
                        manager.GetElement<SpriteRenderer>("IconLevel").sprite = TextureManager.container.typeLevel;
                    string lv = "";
                    if (data.Level > origin.Level)
                        lv += upColor;
                    else if (data.Level < origin.Level)
                        lv += downColor;
                    else
                        lv += normalColor;
                    lv += data.Level.ToString() + "</color>";
                    manager.GetElement<TextMeshPro>("TextLevel").text = lv;
                }
                //Tuner
                if ((data.Type & (uint)CardType.Tuner) > 0)
                {
                    manager.GetElement("TunerIconRoot").SetActive(true);
                    if ((origin.Type & (uint)CardType.Tuner) > 0)
                        manager.GetElement("TunerIconOutline").SetActive(false);
                    else
                        manager.GetElement("TunerIconOutline").SetActive(true);
                }
                else
                    manager.GetElement("TunerIconRoot").SetActive(false);
                //Attribute
                if ((data.Attribute & (uint)CardAttribute.Light) > 0)
                    manager.GetElement<SpriteRenderer>("IconAttribute").sprite = TextureManager.container.attributeLight;
                else if ((data.Attribute & (uint)CardAttribute.Dark) > 0)
                    manager.GetElement<SpriteRenderer>("IconAttribute").sprite = TextureManager.container.attributeDark;
                else if ((data.Attribute & (uint)CardAttribute.Water) > 0)
                    manager.GetElement<SpriteRenderer>("IconAttribute").sprite = TextureManager.container.attributeWater;
                else if ((data.Attribute & (uint)CardAttribute.Fire) > 0)
                    manager.GetElement<SpriteRenderer>("IconAttribute").sprite = TextureManager.container.attributeFire;
                else if ((data.Attribute & (uint)CardAttribute.Earth) > 0)
                    manager.GetElement<SpriteRenderer>("IconAttribute").sprite = TextureManager.container.attributeEarth;
                else if ((data.Attribute & (uint)CardAttribute.Wind) > 0)
                    manager.GetElement<SpriteRenderer>("IconAttribute").sprite = TextureManager.container.attributeWind;
                else if ((data.Attribute & (uint)CardAttribute.Divine) > 0)
                    manager.GetElement<SpriteRenderer>("IconAttribute").sprite = TextureManager.container.attributeDivine;
                if (data.Id > 0 && data.Attribute != origin.Attribute)
                {
                    manager.GetElement("IconAttributeChange").SetActive(true);
                    if (lastAttribute != data.Attribute)
                    {
                        lastAttribute = data.Attribute;
                        AudioManager.PlaySE("SE_BUFF_CHANGE");
                        manager.GetElement("EffectChange").SetActive(false);
                        manager.GetElement("EffectChange").SetActive(true);
                    }
                }
                else
                    manager.GetElement("IconAttributeChange").SetActive(false);
                manager.GetElement<SpriteRenderer>("MagicType").sprite = TextureManager.container.typeNone;
                manager.GetElement("MagicTypeChange").SetActive(false);
                //Race
                if ((data.Race & (uint)CardRace.Warrior) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceWarrior;
                else if ((data.Race & (uint)CardRace.SpellCaster) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceSpellCaster;
                else if ((data.Race & (uint)CardRace.Fairy) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceFairy;
                else if ((data.Race & (uint)CardRace.Fiend) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceFiend;
                else if ((data.Race & (uint)CardRace.Zombie) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceZombie;
                else if ((data.Race & (uint)CardRace.Machine) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceMachine;
                else if ((data.Race & (uint)CardRace.Aqua) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceAqua;
                else if ((data.Race & (uint)CardRace.Pyro) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.racePyro;
                else if ((data.Race & (uint)CardRace.Rock) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceRock;
                else if ((data.Race & (uint)CardRace.WindBeast) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceWindBeast;
                else if ((data.Race & (uint)CardRace.Plant) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.racePlant;
                else if ((data.Race & (uint)CardRace.Insect) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceInsect;
                else if ((data.Race & (uint)CardRace.Thunder) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceThunder;
                else if ((data.Race & (uint)CardRace.Dragon) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceDragon;
                else if ((data.Race & (uint)CardRace.Beast) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceBeast;
                else if ((data.Race & (uint)CardRace.BeastWarrior) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceBeastWarrior;
                else if ((data.Race & (uint)CardRace.Dinosaur) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceDinosaur;
                else if ((data.Race & (uint)CardRace.Fish) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceFish;
                else if ((data.Race & (uint)CardRace.SeaSerpent) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceSeaSerpent;
                else if ((data.Race & (uint)CardRace.Reptile) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceReptile;
                else if ((data.Race & (uint)CardRace.Psycho) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.racePsycho;
                else if ((data.Race & (uint)CardRace.DivineBeast) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceDivineBeast;
                else if ((data.Race & (uint)CardRace.CreatorGod) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceCreatorGod;
                else if ((data.Race & (uint)CardRace.Wyrm) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceWyrm;
                else if ((data.Race & (uint)CardRace.Cyberse) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceCyberse;
                else if ((data.Race & (uint)CardRace.Illustion) > 0)
                    manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.raceIllustion;
                if (data.Id > 0 && data.Race != origin.Race)
                {
                    manager.GetElement("IconTypeChange").SetActive(true);
                    if (lastRace != data.Race)
                    {
                        lastRace = data.Race;
                        AudioManager.PlaySE("SE_BUFF_CHANGE");
                        manager.GetElement("EffectChange").SetActive(false);
                        manager.GetElement("EffectChange").SetActive(true);
                    }
                }
                else
                    manager.GetElement("IconTypeChange").SetActive(false);


            }
            else
            {
                for (int i = 0; i < 8; i++)
                    manager.GetElement("LinkMarker" + i).SetActive(false);
                manager.GetElement("MonsterMaterialsRoot").SetActive(false);
                manager.GetElement("CardAttackBody").SetActive(false);
                int p1 = 0;
                int p2 = 4;
                if (Program.I().ocgcore.MasterRule <= 3)
                {
                    p1 = 6;
                    p2 = 7;
                }

                //Pendulum Scale
                if ((p.location & (uint)CardLocation.PendulumZone) > 0 ||
                    ((data.Type & (uint)CardType.Pendulum) > 0
                    && (p.location & (uint)CardLocation.SpellZone) > 0
                    && (data.Type & (uint)CardType.Equip) == 0
                    && (data.Type & (uint)CardType.Continuous) == 0
                    && (data.Type & (uint)CardType.Trap) == 0)
                    && (p.sequence == p1 || p.sequence == p2))
                {
                    manager.GetElement("CardPendulumBody").SetActive(true);
                    string pendulum = "";
                    if (p.sequence == p1)
                    {
                        manager.GetElement("PendulumLeft").SetActive(true);
                        manager.GetElement("PendulumRight").SetActive(false);
                        if (data.LScale > origin.LScale)
                            pendulum += upColor;
                        else if (data.LScale < origin.LScale)
                            pendulum += downColor;
                        else
                            pendulum += normalColor;
                        pendulum += data.LScale.ToString() + "</color>";
                        manager.GetElement<TextMeshPro>("TextPendulumLeft").text = pendulum;
                    }
                    else
                    {
                        manager.GetElement("PendulumLeft").SetActive(false);
                        manager.GetElement("PendulumRight").SetActive(true);
                        if (data.RScale > origin.RScale)
                            pendulum += upColor;
                        else if (data.RScale < origin.RScale)
                            pendulum += downColor;
                        else
                            pendulum += normalColor;
                        pendulum += data.RScale.ToString() + "</color>";
                        manager.GetElement<TextMeshPro>("TextPendulumRight").text = pendulum;
                    }
                }
                else
                    manager.GetElement("CardPendulumBody").SetActive(false);
                manager.GetElement("LinkCount").SetActive(false);
                manager.GetElement("CardLevel").SetActive(false);
                manager.GetElement("TunerIconRoot").SetActive(false);
                //Attribute
                if ((data.Type & (uint)CardType.Spell) > 0)
                    manager.GetElement<SpriteRenderer>("IconAttribute").sprite = TextureManager.container.attributeSpell;
                else
                    manager.GetElement<SpriteRenderer>("IconAttribute").sprite = TextureManager.container.attributeTrap;
                manager.GetElement("IconAttributeChange").SetActive(false);

                //Magic Trap Type
                if ((data.Type & (uint)CardType.Counter) > 0)
                    manager.GetElement<SpriteRenderer>("MagicType").sprite = TextureManager.container.typeCounter;
                else if ((data.Type & (uint)CardType.Field) > 0)
                    manager.GetElement<SpriteRenderer>("MagicType").sprite = TextureManager.container.typeField;
                else if ((data.Type & (uint)CardType.Equip) > 0)
                    manager.GetElement<SpriteRenderer>("MagicType").sprite = TextureManager.container.typeEquip;
                else if ((data.Type & (uint)CardType.Continuous) > 0)
                    manager.GetElement<SpriteRenderer>("MagicType").sprite = TextureManager.container.typeContinuous;
                else if ((data.Type & (uint)CardType.QuickPlay) > 0)
                    manager.GetElement<SpriteRenderer>("MagicType").sprite = TextureManager.container.typeQuickPlay;
                else if ((data.Type & (uint)CardType.Ritual) > 0 && (origin.Type & (uint)CardType.Monster) == 0)
                    manager.GetElement<SpriteRenderer>("MagicType").sprite = TextureManager.container.typeRitual;
                else
                    manager.GetElement<SpriteRenderer>("MagicType").sprite = TextureManager.container.typeNone;
                manager.GetElement("MagicTypeChange").SetActive(false);
                manager.GetElement<SpriteRenderer>("IconType").sprite = TextureManager.container.typeNone;
                manager.GetElement("IconTypeChange").SetActive(false);
            }

            if (cardCounters.Count > 0)
            {
                int counter = 0;
                int count = 0;
                foreach (var cc in cardCounters)
                {
                    counter = cc.Key;
                    count = cc.Value;
                    break;
                }
                manager.GetElement<SpriteRenderer>("IconCounter").sprite = TextureManager.GetCardCounterIcon(counter);
                manager.GetElement<TextMeshPro>("TextCounter").text = count.ToString();
            }
            else
            {
                manager.GetElement<SpriteRenderer>("IconCounter").sprite = TextureManager.container.typeNone;
                manager.GetElement<TextMeshPro>("TextCounter").text = string.Empty;
            }


            if (p.controller == 0)
            {
                manager.GetElement<Transform>("MonsterMaterialsRoot").localEulerAngles = new Vector3(0f, 0f, 0f);
                manager.GetElement<Transform>("CardAttackBody").localEulerAngles = new Vector3(0f, 0f, 0f);
                manager.GetElement<Transform>("CardPendulumBody").localEulerAngles = new Vector3(0f, 0f, 0f);
                manager.GetElement<Transform>("LinkCount").localEulerAngles = new Vector3(0f, 0f, 0f);
                manager.GetElement<Transform>("CardLevel").localEulerAngles = new Vector3(0f, 0f, 0f);
                manager.GetElement<Transform>("TunerIconRoot").localEulerAngles = new Vector3(0f, 0f, 0f);
                manager.GetElement<Transform>("CardAttribute").localEulerAngles = new Vector3(0f, 0f, 0f);
                manager.GetElement<Transform>("MagicTypeBase").localEulerAngles = new Vector3(0f, 0f, 0f);
                manager.GetElement<Transform>("CardType").localEulerAngles = new Vector3(0f, 0f, 0f);
                manager.GetElement<Transform>("CardCounter").localEulerAngles = new Vector3(0f, 0f, 0f);
                manager.GetElement<Transform>("StatusIcon").localEulerAngles = new Vector3(0f, 0f, 0f);
            }
            else
            {
                manager.GetElement<Transform>("MonsterMaterialsRoot").localEulerAngles = new Vector3(0f, 180f, 0f);
                manager.GetElement<Transform>("CardAttackBody").localEulerAngles = new Vector3(0f, 180f, 0f);
                manager.GetElement<Transform>("CardPendulumBody").localEulerAngles = new Vector3(0f, 180f, 0f);
                manager.GetElement<Transform>("LinkCount").localEulerAngles = new Vector3(0f, 180f, 0f);
                manager.GetElement<Transform>("CardLevel").localEulerAngles = new Vector3(0f, 180f, 0f);
                manager.GetElement<Transform>("TunerIconRoot").localEulerAngles = new Vector3(0f, 180f, 0f);
                manager.GetElement<Transform>("CardAttribute").localEulerAngles = new Vector3(0f, 180f, 0f);
                manager.GetElement<Transform>("MagicTypeBase").localEulerAngles = new Vector3(0f, 180f, 0f);
                manager.GetElement<Transform>("CardType").localEulerAngles = new Vector3(0f, 180f, 0f);
                manager.GetElement<Transform>("CardCounter").localEulerAngles = new Vector3(0f, 180f, 0f);
                manager.GetElement<Transform>("StatusIcon").localEulerAngles = new Vector3(0f, 180f, 0f);
            }

            ShowLabel();
        }

        public void ShowLabel()
        {
            labelShowing = true;
            Transform labelRoot = manager.GetElement<Transform>("StatusLabelRoot");
            labelRoot.gameObject.SetActive(true);
            labelRoot.DOScale(1, 0.2f).SetEase(Ease.InCubic);

            HideHiddenLabel();
        }
        public void HideLabel()
        {
            labelShowing = false;
            Transform labelRoot = manager.GetElement<Transform>("StatusLabelRoot");
            labelRoot.DOScale(0, 0.2f).SetEase(Ease.OutCubic).OnComplete(() => labelRoot.gameObject.SetActive(false));
        }

        public void ShowHiddenLabel()
        {
            if (model == null)
                return;
            manager.GetElement("CardAttribute").SetActive(true);
            manager.GetElement("MagicTypeBase").SetActive(true);
            manager.GetElement("CardType").SetActive(true);

            Transform linkMarker = manager.GetElement<Transform>("LinkMarkerRoot");
            linkMarker.DOScale(1.7f, 0.2f).SetEase(Ease.InOutCubic);
        }
        public void HideHiddenLabel()
        {
            if (model == null || !labelShowing)
                return;
            if (!manager.GetElement("IconAttributeChange").activeSelf)
                manager.GetElement("CardAttribute").SetActive(false);
            else
                manager.GetElement("CardAttribute").SetActive(true);
            if ((p.location & (uint)CardLocation.SpellZone) == 0
                || !manager.GetElement("MagicTypeChange").activeSelf)
                manager.GetElement("MagicTypeBase").SetActive(false);
            else
                manager.GetElement("MagicTypeBase").SetActive(true);
            if ((p.location & (uint)CardLocation.MonsterZone) == 0
                || !manager.GetElement("IconTypeChange").activeSelf)
                manager.GetElement("CardType").SetActive(false);
            else
                manager.GetElement("CardType").SetActive(true);

            Transform linkMarker = manager.GetElement<Transform>("LinkMarkerRoot");
            linkMarker.DOScale(0.95f, 0.2f).SetEase(Ease.InOutCubic);
        }

        void SetDisabled()
        {
            if (model == null)
                return;
            var cardFace = manager.GetElement<Transform>("CardModel").GetChild(1).GetComponent<Renderer>();
            if ((p.location & (uint)CardLocation.Onfield) == 0)
                m_disabled = false;
            if ((p.position & (uint)CardPosition.FaceDown) > 0)
                m_disabled = false;

            if (disabled)
                cardFace.material.SetFloat("_Monochrome", 1);
            else
                cardFace.material.SetFloat("_Monochrome", 0);
        }
        #endregion

        #region CardCounter
        Dictionary<int, int> cardCounters = new Dictionary<int, int>();
        public void AddCounter(int counter, int count)
        {
            AudioManager.PlaySE("SE_CARD_COUNTER");
            bool have = false;
            foreach (var cc in cardCounters)
                if (cc.Key == counter)
                    have = true;
            int fullCount = count;
            if (have)
            {
                fullCount += cardCounters[counter];
                cardCounters.Remove(counter);
            }
            cardCounters.Add(counter, fullCount);

            var counterName = StringHelper.Get("counter", counter);
            for (int i = 0; i < count; i++)
                AddStringTail(counterName);
            RefreshLabel();
        }
        public void RemoveCounter(int counter, int count)
        {
            AudioManager.PlaySE("SE_CARD_COUNTER");
            var fullCount = cardCounters[counter] - count;
            cardCounters.Remove(counter);
            if (fullCount > 0)
                cardCounters.Add(counter, fullCount);

            var counterName = StringHelper.Get("counter", counter);
            for (int i = 0; i < count; i++)
                RemoveStringTail(counterName);
            RefreshLabel();
        }
        public void ClearCounter()
        {
            cardCounters.Clear();
        }
        #endregion

        #region String Tail
        public MultiStringMaster tails = new MultiStringMaster();
        public void AddStringTail(string tail)
        {
            tails.Add(tail);
        }
        public void RemoveStringTail(string tail, bool all = false)
        {
            tails.Remove(tail, all);
        }
        public void ClearAllTails()
        {
            ClearCounter();
            tails.Clear();
        }
        #endregion

        #region Chain
        public class Chain
        {
            public int i;
            public DuelChainSpot chainSpot;
        }
        public List<Chain> chains = new List<Chain>();

        public void AddChain(int i)
        {
            var obj = ABLoader.LoadFromFile("Timeline/DuelChain/ChainSpot");
            Program.I().ocgcore.allGameObjects.Add(obj);
            chains.Add(new Chain() { i = i, chainSpot = obj.GetComponent<DuelChainSpot>() });
            bool turn = (p.location & (uint)CardLocation.MonsterZone) > 0 && (p.position & (uint)CardPosition.Defence) > 0;
            chains[chains.Count - 1].chainSpot.Play(i, p.location, model != null, turn, GetCardPosition(p, this), i == 1);
        }
        public void ResolveChain(int i)
        {
            foreach (var chain in chains)
            {
                if (chain.i == i)
                {
                    chain.chainSpot.OnChainResolveBegin();
                    break;
                }
            }
        }
        public void RemoveChain(int i)
        {
            foreach (var chain in chains)
            {
                if (chain.i == i)
                {
                    chain.chainSpot.OnChainResolveEnd();
                    Destroy(chain.chainSpot.gameObject, 1f);
                    chains.Remove(chain);
                    break;
                }
            }
        }
        public void RemoveAllChain()
        {
            foreach (var chain in chains)
                Destroy(chain.chainSpot.gameObject, 1f);
            chains.Clear();
        }
        #endregion

        #region enum
        public enum Condition
        {
            None,
            Chaining,
            Selected
        }
        private enum CardRuleCondition
        {
            MeUpAtk,
            MeUpDef,
            MeDownAtk,
            MeDownDef,
            OpUpAtk,
            OpUpDef,
            OpDownAtk,
            OpDownDef,
            MeUpDeck,
            MeDownDeck,
            OpUpDeck,
            OpDownDeck,
            MeUpExDeck,
            MeDownExDeck,
            OpUpExDeck,
            OpDownExDeck,
            MeUpGrave,
            MeDownGrave,
            OpUpGrave,
            OpDownGrave,
            MeUpRemoved,
            MeDownRemoved,
            OpUpRemoved,
            OpDownRemoved,
            MeUpHand,
            MeDownHand,
            OpUpHand,
            OpDownHand
        }
        #endregion
    }
}
