using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TextureContainer : ScriptableObject
{
    [Header("Common")]
    public Sprite black;
    public Sprite transparent;
    public Sprite unknownCard;
    public Sprite unknownArt;
    public Sprite cardBackDefault;
    [Header("Card Frame")]
    public Sprite cardFrameNormal;
    public Sprite cardFrameEffect;
    public Sprite cardFrameRitual;
    public Sprite cardFrameFusion;
    public Sprite cardFrameObelisk;
    public Sprite cardFrameOsiris;
    public Sprite cardFrameRa;
    public Sprite cardFrameSpell;
    public Sprite cardFrameTrap;
    public Sprite cardFrameToken;
    public Sprite cardFrameSynchro;
    public Sprite cardFrameDarkSynchro;
    public Sprite cardFrameXyz;
    public Sprite cardFramePendulumNormal;
    public Sprite cardFramePendulumEffect;
    public Sprite cardFramePendulumXyz;
    public Sprite cardFramePendulumSynchro;
    public Sprite cardFramePendulumFusion;
    public Sprite cardFrameLink;
    public Sprite cardFramePendulumRitual;
    public Sprite cardFrameNormalOF;
    public Sprite cardFrameEffectOF;
    public Sprite cardFrameRitualOF;
    public Sprite cardFrameFusionOF;
    public Sprite cardFrameObeliskOF;
    public Sprite cardFrameOsirisOF;
    public Sprite cardFrameRaOF;
    public Sprite cardFrameSpellOF;
    public Sprite cardFrameTrapOF;
    public Sprite cardFrameTokenOF;
    public Sprite cardFrameSynchroOF;
    public Sprite cardFrameDarkSynchroOF;
    public Sprite cardFrameXyzOF;
    public Sprite cardFramePendulumNormalOF;
    public Sprite cardFramePendulumEffectOF;
    public Sprite cardFramePendulumXyzOF;
    public Sprite cardFramePendulumSynchroOF;
    public Sprite cardFramePendulumFusionOF;
    public Sprite cardFrameLinkOF;
    public Sprite cardFramePendulumRitualOF;
    [Header("Card Frame Mask")]
    public Texture2D cardFrameMask;
    public Texture2D cardFrameMaskLink;
    public Texture2D cardFrameMaskPendulum;
    public Texture2D cardKiraMask;
    public Texture2D cardKiraMaskLink;
    public Texture2D cardKiraMaskPendulum;
    public Texture2D cardNormal;
    public Texture2D cardNormalLink;
    public Texture2D cardNormalPendulum;
    [Header("Card Attribute")]
    public Sprite attributeLight;
    public Sprite attributeDark;
    public Sprite attributeWater;
    public Sprite attributeFire;
    public Sprite attributeEarth;
    public Sprite attributeWind;
    public Sprite attributeDivine;
    public Sprite attributeSpell;
    public Sprite attributeTrap;
    [Header("CardType")]
    public Sprite typeNone;
    public Sprite typeCounter;
    public Sprite typeField;
    public Sprite typeEquip;
    public Sprite typeContinuous;
    public Sprite typeQuickPlay;
    public Sprite typeRitual;
    public Sprite typeLevel;
    public Sprite typeRank;
    public Sprite typePendulum;
    public Sprite typeLink;
    public Sprite typeLevelOff;
    public Sprite typeLinkOff;
    public Sprite typeLevelNone;
    public Sprite typeLevelRank;
    [Header("CardLimit")]
    public Sprite banned;
    public Sprite limit1;
    public Sprite limit2;
    [Header("CardRace")]
    public Sprite raceDragon;
    public Sprite raceZombie;
    public Sprite raceFiend;
    public Sprite racePyro;
    public Sprite raceSeaSerpent;
    public Sprite raceRock;
    public Sprite raceMachine;
    public Sprite raceFish;
    public Sprite raceDinosaur;
    public Sprite raceInsect;
    public Sprite raceBeast;
    public Sprite raceBeastWarrior;
    public Sprite racePlant;
    public Sprite raceAqua;
    public Sprite raceWarrior;
    public Sprite raceWindBeast;
    public Sprite raceFairy;
    public Sprite raceSpellCaster;
    public Sprite raceThunder;
    public Sprite raceReptile;
    public Sprite racePsycho;
    public Sprite raceWyrm;
    public Sprite raceCyberse;
    public Sprite raceDivineBeast;
    public Sprite raceIllustion;
    public Sprite raceCreatorGod;

    [Header("CardCounter")]
    public Sprite counterAlien;
    public Sprite counterAthlete;
    public Sprite counterBalloon;
    public Sprite counterBarrel;
    public Sprite counterBF;
    public Sprite counterBurn;
    public Sprite counterBushido;
    public Sprite counterChaos;
    public Sprite counterChronicle;
    public Sprite counterClock;
    public Sprite counterD;
    public Sprite counterDeath;
    public Sprite counterDefect;
    public Sprite counterDeformer;
    public Sprite counterDestiny;
    public Sprite counterDonguri;
    public Sprite counterDouble;
    public Sprite counterDragonic;
    public Sprite counterEarthBind;
    public Sprite counterEM;
    public Sprite counterFireStar;
    public Sprite counterFlower;
    public Sprite counterFog;
    public Sprite counterGardna;
    public Sprite counterGate;
    public Sprite counterGem;
    public Sprite counterGenex;
    public Sprite counterGG;
    public Sprite counterGirl;
    public Sprite counterGreed;
    public Sprite counterGuard;
    public Sprite counterGuard2;
    public Sprite counterHopeSlash;
    public Sprite counterHoukai;
    public Sprite counterHyper;
    public Sprite counterIce;
    public Sprite counterIllusion;
    public Sprite counterJunk;
    public Sprite counterKaiju;
    public Sprite counterKarakuri;
    public Sprite counterKattobing;
    public Sprite counterKyoumei;
    public Sprite counterMagic;
    public Sprite counterNormal;
    public Sprite counterOcean;
    public Sprite counterOrbital;
    public Sprite counterOtoshidama;
    public Sprite counterOunokagi;
    public Sprite counterPhantasm;
    public Sprite counterPiece;
    public Sprite counterPlant;
    public Sprite counterPolice;
    public Sprite counterPredator;
    public Sprite counterPsycho;
    public Sprite counterPumpkin;
    public Sprite counterRabbit;
    public Sprite counterScales;
    public Sprite counterShark;
    public Sprite counterShine;
    public Sprite counterSignal;
    public Sprite counterSound;
    public Sprite counterStone;
    public Sprite counterString;
    public Sprite counterSummon;
    public Sprite counterThunder;
    public Sprite counterVenemy;
    public Sprite counterVenom;
    public Sprite counterWedge;
    public Sprite counterWorm;
    public Sprite counterYosen;
    public Sprite counterZushin;

    [Header("Button Icon")]
    public Sprite[] battle;
    public Sprite[] select;
    public Sprite[] spSummon;
    public Sprite[] activate;
    public Sprite[] summon;
    public Sprite[] setSpell;
    public Sprite[] setMonster;
    public Sprite[] toAttack;
    public Sprite[] toDefense;
    public Sprite[] setPendulum;
    public Sprite[] penSummon;
    public Sprite[] cancel;
    public Sprite[] decide;
    public Sprite[] onTiming;
    public Sprite[] offTiming;
    public Sprite[] autoTiming;
    [Header("Location Icon")]
    public Sprite locationDeck;
    public Sprite locationExtra;
    public Sprite locationHand;
    public Sprite locationGrave;
    public Sprite locationRemoved;
    public Sprite locationField;
    public Sprite locationOverlay;
    public Sprite locationSearch;
    [Header("Card List Location Icon")]
    public Sprite listMyDeck;
    public Sprite listOpDeck;
    public Sprite listMyExtra;
    public Sprite listOpExtra;
    public Sprite listMyGrave;
    public Sprite listOpGrave;
    public Sprite listMyRemoved;
    public Sprite listOpRemoved;
    public Sprite listMyXyz;
    public Sprite listOpXyz;
    [Header("Card Affect")]
    public Sprite CardAffectDisable;
    public Sprite CardAffectEquip;
    public Sprite CardAffectField;
    public Sprite CardAffectPermanent;
    public Sprite CardAffectPower;
    public Sprite CardAffectTarget;
    [Header("Link Count")]
    public Sprite link1;
    public Sprite link2;
    public Sprite link3;
    public Sprite link4;
    public Sprite link5;
    public Sprite link6;
    public Sprite link1R;
    public Sprite link2R;
    public Sprite link3R;
    public Sprite link4R;
    public Sprite link5R;
    public Sprite link6R;
    public Sprite link7R;
    public Sprite link8R;
    [Header("Chain Circle Num")]
    public Sprite chainCircleNum0;
    public Sprite chainCircleNum1;
    public Sprite chainCircleNum2;
    public Sprite chainCircleNum3;
    public Sprite chainCircleNum4;
    public Sprite chainCircleNum5;
    public Sprite chainCircleNum6;
    public Sprite chainCircleNum7;
    public Sprite chainCircleNum8;
    public Sprite chainCircleNum9;
    [Header("Chain Num Set")]
    public Sprite chainNumSet0;
    public Sprite chainNumSet1;
    public Sprite chainNumSet2;
    public Sprite chainNumSet3;
    public Sprite chainNumSet4;
    public Sprite chainNumSet5;
    public Sprite chainNumSet6;
    public Sprite chainNumSet7;
    public Sprite chainNumSet8;
    public Sprite chainNumSet9;
    [Header("Window")]
    public Sprite toggleM;
    public Sprite toggleM_On;
    public Sprite toggleM_Over;

    [Header("Other")]
    public Texture2D fxt_Arrow;
    public Texture2D fxt_Arrow_002;
    public Texture2D fxt_Arrow_003;
    public Texture2D fxt_Arrow_004;
    public Texture2D fxt_msk_005;

}
