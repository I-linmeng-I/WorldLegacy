using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using YgomSystem.ElementSystem;
using MDPro3.YGOSharp;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using MDPro3.UI;

namespace MDPro3
{
    public class TimeLineManager : Manager
    {
        public static ElementObjectManager currentManager;
        public static ElementObjectManager currentSyncManager;
        public static GameObject dummyCard;
        public static bool skippable;
        public static bool skipping;
        public static bool inSummonMaterial;
        int summoned;
        public override void Initialize()
        {
            base.Initialize();
        }

        int summonCard;
        int reason;
        List<GameCard> materials;

        IEnumerator CacheCutin()
        {
            if (!MonsterCutin.HasCutin(summonCard))
                yield break;

            string path;
            IEnumerator ie;
            if (Directory.Exists(Program.root + "Monstercutin/" + summonCard))
                ie = ABLoader.LoadFromFolderAsync("Monstercutin/" + summonCard, "Spine" + summonCard, true, false);
            else
                ie = ABLoader.LoadFromFileAsync("Monstercutin2/" + summonCard, true, false);
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
        }

        public IEnumerator SummonMaterial()
        {
            inSummonMaterial = true;
            skippable = false;
            skipping = false;
            summoned = 0;

            summonCard = Program.I().ocgcore.summonCard.GetData().Id;
            materials = Program.I().ocgcore.materialCards;
            reason = materials[0].GetData().Reason;
            if ((reason & (uint)CardReason.MATERIAL) == 0)
                reason = (int)materials[0].p.reason;
            //Debug.LogFormat("{0}: {1:X}" , materials[0].GetData().Name, reason);
            var ie = CacheCutin();
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            skippable = true;
            SyncSummonTimeline();

            GameObject ms;
            if (materials.Count > 8)
                ms = ABLoader.LoadFromFolder("timeline/summon/summonfusion/summonfusionshowunitcard08",
                "summonfusionshowunitcard08", true);
            else
                ms = ABLoader.LoadFromFolder("timeline/summon/summonfusion/summonfusionshowunitcard0" + materials.Count,
                "summonfusionshowunitcard0" + materials.Count, true);

            Program.I().ocgcore.allGameObjects.Add(ms);

            var manager = ms.transform.GetChild(0).GetComponent<ElementObjectManager>();
            currentManager = manager;

            bool tunerFound = false;
            for (int i = 0; i < (materials.Count > 8 ? 8 : materials.Count); i++)
            {
                var dummyCard = manager.GetElement<ElementObjectManager>("DummyCard0" + (i + 1).ToString());
                var renderer = dummyCard.GetElement<Renderer>("DummyCardModel_front");
                var code = materials[i].GetData().Id;
                StartCoroutine(RefreshCardFace(renderer, code));

                var cardBack01 = dummyCard.transform.parent.GetChild(0).GetComponent<ParticleSystem>().main;
                var cardBack02 = dummyCard.transform.parent.GetChild(0).GetChild(0).GetComponent<ParticleSystem>().main;
                if ((reason & (uint)CardReason.Synchro) > 0)
                {
                    if ((materials[i].GetData().Type & (uint)CardType.Tuner) > 0)
                    {
                        cardBack01.startColor = Color.green;
                        cardBack02.startColor = Color.green;
                        tunerFound = true;
                    }
                    else
                    {
                        cardBack01.startColor = Color.cyan;
                        cardBack02.startColor = Color.cyan;
                    }
                }
                else if ((reason & (uint)CardReason.Xyz) > 0)
                {
                    cardBack01.startColor = Color.yellow;
                    cardBack02.startColor = Color.yellow;
                }
                else if ((reason & (uint)CardReason.Link) > 0)
                {
                    cardBack01.startColor = Color.red;
                    cardBack02.startColor = Color.red;
                }
                else if ((reason & (uint)CardReason.Ritual) > 0)
                {
                    cardBack01.startColor = new Color(0f, 0.5f, 1f, 1f);
                    cardBack02.startColor = new Color(0f, 0.5f, 1f, 1f);
                }
            }

            if ((reason & (uint)CardReason.Synchro) > 0 && !tunerFound)
            {
                var dummyCard = manager.GetElement<ElementObjectManager>("DummyCard01");
                var cardBack01 = dummyCard.transform.parent.GetChild(0).GetComponent<ParticleSystem>().main;
                var cardBack02 = dummyCard.transform.parent.GetChild(0).GetChild(0).GetComponent<ParticleSystem>().main;
                cardBack01.startColor = Color.green;
                cardBack02.startColor = Color.green;
            }
            manager.GetComponent<PlayableDirector>().Play();
            yield return new WaitForSeconds(1.33f);
            inSummonMaterial = false;
            Destroy(ms);
            ShiftToSummonTimeline();
        }

        void SyncSummonTimeline()
        {
            var type = CardsManager.Get(summonCard).Type;
            if ((reason & (uint)CardReason.Ritual) > 0 && (type & (uint)CardType.Ritual) > 0)
                StartCoroutine(SummonRitual());
            else if ((reason & (uint)CardReason.Synchro) > 0 && (type & (uint)CardType.Synchro) > 0)
                StartCoroutine(SummonSynchro());
            else if ((reason & (uint)CardReason.Xyz) > 0 && (type & (uint)CardType.Xyz) > 0)
                StartCoroutine(SummonXyz());
            else if ((reason & (uint)CardReason.Link) > 0 && (type & (uint)CardType.Link) > 0)
                StartCoroutine(SummonLink());
            else
            {
                CameraManager.BlackIn(0f, 0.3f);
                currentSyncManager = null;
            }
        }
        void ShiftToSummonTimeline()
        {
            if (skipping)
                return;
            if (summoned == 0)
                StartCoroutine(SummonFusion());
        }

        IEnumerator SummonFusion()
        {
            skippable = true;

            GameObject summon;
            if (materials.Count > 5)
                summon = ABLoader.LoadFromFolder("timeline/summon/summonfusion/fusionnum",
                "FusionNum", true);
            else
                summon = ABLoader.LoadFromFolder("timeline/summon/summonfusion/summonfusion0" + materials.Count + "_01",
                "SummonFusion0" + materials.Count, true);
            Program.I().ocgcore.allGameObjects.Add(summon);
            DeleteWhenStop(summon.transform.GetChild(0).gameObject);
            var manager = summon.transform.GetChild(0).GetComponent<ElementObjectManager>();
            currentManager = manager;
            dummyCard = manager.GetElement("PostFusionPosDummy");

            var cardModel = manager.GetElement<Renderer>("CardModel");
            if (cardModel != null)
                StartCoroutine(RefreshCardFrame(cardModel, summonCard));
            var postFusion = manager.GetElement<Renderer>("PostFusion");
            if (postFusion != null)
                StartCoroutine(RefreshCardFrame(postFusion, summonCard));

            switch (materials.Count)
            {
                case 1:
                    var card01 = manager.GetElement<Renderer>("FusionCard01");
                    StartCoroutine(RefreshCardFrame(card01, 1));
                    break;
                case 2:
                case 3:
                case 4:
                    card01 = manager.GetElement<Renderer>("FusionCard01");
                    var card02 = manager.GetElement<Renderer>("FusionCard02");
                    StartCoroutine(RefreshCardFrame(card01, materials.Count));
                    StartCoroutine(RefreshCardFrame(card02, materials.Count));
                    break;
                case 5:
                    for (int i = 1; i < 6; i++)
                    {
                        var card = manager.GetElement<Renderer>("FusionCard0" + i);
                        StartCoroutine(RefreshCardFrame(card, 1, i - 1));
                    }
                    var cardAll = manager.GetElement<Renderer>("FusionCardAll");
                    StartCoroutine(RefreshCardFrame(cardAll, 5));
                    break;
                default:
                    for (int i = 1; i < 7; i++)
                    {
                        var card = manager.GetElement<Renderer>("FusionCard0" + i);
                        StartCoroutine(RefreshCardFrame(card, 1, i - 1));
                    }
                    break;
            }
            yield return null;
        }
        IEnumerator SummonRitual()
        {
            summoned = 1;
            GameObject summon;
            if (materials.Count > 0)
                summon = ABLoader.LoadFromFolder("timeline/summon/summonritual/summonritual01", "SummonRitual01", true);
            else
                summon = ABLoader.LoadFromFolder("timeline/summon/summonritual/summonritual02", "SummonRitual02", true);

            ElementObjectManager manager = null;
            for (int i = 0; i < summon.transform.childCount; i++)
            {
                if (summon.transform.GetChild(i).GetComponent<PlayableDirector>() == null)
                    Destroy(summon.transform.GetChild(i).gameObject);
                else
                {
                    manager = summon.transform.GetChild(i).GetComponent<ElementObjectManager>();
                    DeleteWhenStop(summon.transform.GetChild(i).gameObject);
                }
            }

            Program.I().ocgcore.allGameObjects.Add(summon);
            currentSyncManager = manager;
            manager.transform.GetChild(0).gameObject.AddComponent<AutoScaleOnce>();

            var subManager = manager.GetElement<ElementObjectManager>("SummonRitualpostRitual");
            dummyCard = subManager.GetElement("DummyCardRitual");

            var cardModel = subManager.GetElement<ElementObjectManager>("DummyCardRitual");
            var cardFace = cardModel.GetElement<Renderer>("DummyCardModel_front");
            StartCoroutine(RefreshCardFace(cardFace, summonCard));

            var postRitual = subManager.GetElement<Renderer>("DummyCardRitualAdd");
            StartCoroutine(RefreshCardFace(postRitual, summonCard, true));

            switch (materials.Count)
            {
                case 0:
                    break;
                case 1:
                    manager.GetElement("RitualTrailIn02").SetActive(false);
                    manager.GetElement("RitualTrailIn03").SetActive(false);
                    break;
                case 2:
                    manager.GetElement("RitualTrailIn01").SetActive(false);
                    manager.GetElement("RitualTrailIn03").SetActive(false);
                    break;
                case 3:
                    manager.GetElement("RitualTrailIn01").SetActive(false);
                    manager.GetElement("RitualTrailIn02").SetActive(false);
                    break;
                case 4:
                    manager.GetElement("RitualTrailIn02").SetActive(false);
                    break;
                case 5:
                    manager.GetElement("RitualTrailIn01").SetActive(false);
                    break;
            }

            yield return null;
        }
        IEnumerator SummonSynchro()
        {
            summoned = 2;
            GameObject summon;
            if (materials.Count > 0)
                summon = ABLoader.LoadFromFile("timeline/summon/summonsynchro/summonsynchro01", true);
            else
                summon = ABLoader.LoadFromFile("timeline/summon/summonsynchro/summonsynchro02", true);

            Program.I().ocgcore.allGameObjects.Add(summon);
            DeleteWhenStop(summon);
            var manager = summon.GetComponent<ElementObjectManager>();
            currentSyncManager = manager;
            manager.transform.GetChild(0).GetChild(0).gameObject.AddComponent<AutoScaleOnce>();

            var subManager = manager.GetElement<ElementObjectManager>("SummonSynchroPostSynchro");
            dummyCard = subManager.GetElement("DummyCardSynchro");

            var cardModel = subManager.GetElement<ElementObjectManager>("DummyCardSynchro");
            var cardFace = cardModel.GetElement<Renderer>("DummyCardModel_front");
            StartCoroutine(RefreshCardFace(cardFace, summonCard));
            var postSynchro = subManager.GetElement<Renderer>("DummyCardLinkAdd");
            StartCoroutine(RefreshCardFace(postSynchro, summonCard, true));

            int tunerLevel = GetTunerLevel();
            int level = CardsManager.Get(summonCard).Level;
            int nonTunerLevel = level - tunerLevel;

            for (int i = 1; i < 12; i++)
                if (i != nonTunerLevel)
                {
                    manager.GetElement("NumberNonTuner" + (i > 9 ? i.ToString() : "0" + i.ToString())).SetActive(false);
                    manager.GetElement("SynchroStarLevel" + (i > 9 ? i.ToString() : "0" + i.ToString())).SetActive(false);
                }
            for (int i = 1; i < 12; i++)
                if (i != tunerLevel)
                    manager.GetElement("NumberTuner" + (i > 9 ? i.ToString() : "0" + i.ToString())).SetActive(false);

            if (tunerLevel >= 9)
            {
                manager.GetElement("SynchroCircle01").SetActive(false);
                manager.GetElement("SynchroCircle02").SetActive(false);
            }
            else if (tunerLevel >= 6)
            {
                manager.GetElement("SynchroCircle01").SetActive(false);
                manager.GetElement("SynchroCircle03").SetActive(false);
            }
            else
            {
                manager.GetElement("SynchroCircle02").SetActive(false);
                manager.GetElement("SynchroCircle03").SetActive(false);
            }

            yield return null;
        }
        IEnumerator SummonXyz()
        {
            summoned = 3;

            GameObject summon;
            if (materials.Count == 0)
                summon = ABLoader.LoadFromFile("timeline/summon/summonxyz/summonxyz00_01", true);
            else if (materials.Count == 1)
                summon = ABLoader.LoadFromFile("timeline/summon/summonxyz/summonxyz01_01", true);
            else if (materials.Count == 2)
                summon = ABLoader.LoadFromFile("timeline/summon/summonxyz/summonxyz02_01", true);
            else
                summon = ABLoader.LoadFromFile("timeline/summon/summonxyz/summonxyz03_01", true);

            Program.I().ocgcore.allGameObjects.Add(summon);
            DeleteWhenStop(summon);
            summon.transform.GetChild(0).gameObject.AddComponent<AutoScaleOnce>();

            var manager = summon.GetComponent<ElementObjectManager>();
            currentSyncManager = manager;
            var subManager = manager.GetElement<ElementObjectManager>("DummyCardXYZ");
            dummyCard = subManager.gameObject;

            var ie = Program.I().texture_.LoadDummyCard(subManager, summonCard);
            StartCoroutine(ie);

            Destroy(manager.GetElement("SummonXYZShowUnitCard0" + (materials.Count > 2 ? "3" : materials.Count.ToString())));

            foreach (var child in summon.transform.GetComponentsInChildren<MeshRenderer>(true))
                if (child.name.StartsWith("XYZInMesh"))
                    child.material.GetTexture("_Texture2D").wrapMode = TextureWrapMode.Clamp;

            yield return null;
        }
        IEnumerator SummonLink()
        {
            summoned = 4;
            GameObject summon;
            int linkCount = CardDescription.GetCardLinkCount(CardsManager.Get(summonCard));
            if (linkCount == 1)
                summon = ABLoader.LoadFromFile("timeline/summon/summonlink/summonlink01_01", true);
            else if (linkCount == 2)
                summon = ABLoader.LoadFromFile("timeline/summon/summonlink/summonlink02_01", true);
            else
                summon = ABLoader.LoadFromFile("timeline/summon/summonlink/summonlink03_01", true);

            Program.I().ocgcore.allGameObjects.Add(summon);
            DeleteWhenStop(summon);
            var manager = summon.GetComponent<ElementObjectManager>();
            currentSyncManager = manager;
            manager.GetElement("BlackNormal").AddComponent<AutoScaleOnce>();

            var subManager = manager.GetElement<ElementObjectManager>("SummonLinkpostLink");
            dummyCard = subManager.GetElement("DummyCardLink");

            var cardModel = subManager.GetElement<ElementObjectManager>("DummyCardLink");
            var ie = Program.I().texture_.LoadDummyCard(cardModel, summonCard);
            StartCoroutine(ie);

            var postLink = subManager.GetElement<Renderer>("DummyCardLinkAdd");
            StartCoroutine(RefreshCardFace(postLink, summonCard, true));

            Destroy(manager.GetElement("SummonLinkShowUnitCardSet"));

            var linkMarker = CardsManager.Get(summonCard).LinkMarker;
            var trail1 = manager.GetElement<ElementObjectManager>("LinkTrailIn01");
            linkMarker = DestroyLinkTrail(trail1, linkMarker, linkCount > 5 ? 2 : 1);
            if (linkCount > 1)
            {
                var trail2 = manager.GetElement<ElementObjectManager>("LinkTrailIn02");
                linkMarker = DestroyLinkTrail(trail2, linkMarker, linkCount > 4 ? 2 : 1);
            }
            if (linkCount > 2)
            {
                var trail3 = manager.GetElement<ElementObjectManager>("LinkTrailIn03");
                DestroyLinkTrail(trail3, linkMarker, linkCount > 3 ? 2 : 1);
            }

            foreach (var child in summon.transform.GetComponentsInChildren<MeshRenderer>(true))
                if (child.name.StartsWith("SummonLinkTrail"))
                    child.material.GetTexture("_Texture2D").wrapMode = TextureWrapMode.Clamp;

            //var director = summon.GetComponent<PlayableDirector>();
            //foreach (PlayableBinding pb in director.playableAsset.outputs)
            //{
            //    var track = pb.sourceObject as TrackAsset;
            //    if (track != null)
            //    {
            //        foreach (TimelineClip clip in track.GetClips())
            //            if (clip.asset is SoundPlayableAsset asset)
            //                if (asset.startLabel == "SE_SMN_CMN_CARD_01")
            //                {
            //                    asset.startLabel = "";
            //                    break;
            //                }
            //    }
            //}

            yield return null;
        }


        int DestroyLinkTrail(ElementObjectManager manager, int linkMarker, int need)
        {
            int foundMarker = 0;
            int foundMarkerCount = 0;
            var parent = manager.transform.parent.GetComponent<ElementObjectManager>();
            if ((linkMarker & (int)CardLinkMarker.Top) > 0)
            {
                foundMarkerCount++;
                foundMarker += (int)CardLinkMarker.Top;
            }
            else
            {
                Destroy(manager.GetElement("LinkTrailG02"));
                Destroy(parent.GetElement("Marker" + manager.name.Substring(manager.name.Length - 2, 2) + "_02"));
            }
            if (foundMarkerCount < need && (linkMarker & (int)CardLinkMarker.TopLeft) > 0)
            {
                foundMarkerCount++;
                foundMarker += (int)CardLinkMarker.TopLeft;
            }
            else
            {
                Destroy(manager.GetElement("LinkTrailG01"));
                Destroy(parent.GetElement("Marker" + manager.name.Substring(manager.name.Length - 2, 2) + "_01"));
            }
            if (foundMarkerCount < need && (linkMarker & (int)CardLinkMarker.Left) > 0)
            {
                foundMarkerCount++;
                foundMarker += (int)CardLinkMarker.Left;
            }
            else
            {
                Destroy(manager.GetElement("LinkTrailG04"));
                Destroy(parent.GetElement("Marker" + manager.name.Substring(manager.name.Length - 2, 2) + "_04"));
            }
            if (foundMarkerCount < need && (linkMarker & (int)CardLinkMarker.BottomLeft) > 0)
            {
                foundMarkerCount++;
                foundMarker += (int)CardLinkMarker.BottomLeft;
            }
            else
            {
                Destroy(manager.GetElement("LinkTrailG06"));
                Destroy(parent.GetElement("Marker" + manager.name.Substring(manager.name.Length - 2, 2) + "_06"));
            }
            if (foundMarkerCount < need && (linkMarker & (int)CardLinkMarker.Bottom) > 0)
            {
                foundMarkerCount++;
                foundMarker += (int)CardLinkMarker.Bottom;
            }
            else
            {
                Destroy(manager.GetElement("LinkTrailG07"));
                Destroy(parent.GetElement("Marker" + manager.name.Substring(manager.name.Length - 2, 2) + "_07"));
            }
            if (foundMarkerCount < need && (linkMarker & (int)CardLinkMarker.BottomRight) > 0)
            {
                foundMarkerCount++;
                foundMarker += (int)CardLinkMarker.BottomRight;
            }
            else
            {
                Destroy(manager.GetElement("LinkTrailG08"));
                Destroy(parent.GetElement("Marker" + manager.name.Substring(manager.name.Length - 2, 2) + "_08"));
            }
            if (foundMarkerCount < need && (linkMarker & (int)CardLinkMarker.Right) > 0)
            {
                foundMarkerCount++;
                foundMarker += (int)CardLinkMarker.Right;
            }
            else
            {
                Destroy(manager.GetElement("LinkTrailG05"));
                Destroy(parent.GetElement("Marker" + manager.name.Substring(manager.name.Length - 2, 2) + "_05"));
            }
            if (foundMarkerCount < need && (linkMarker & (int)CardLinkMarker.TopRight) > 0)
            {
                foundMarkerCount++;
                foundMarker += (int)CardLinkMarker.TopRight;
            }
            else
            {
                Destroy(manager.GetElement("LinkTrailG03"));
                Destroy(parent.GetElement("Marker" + manager.name.Substring(manager.name.Length - 2, 2) + "_03"));
            }

            return linkMarker - foundMarker;
        }


        int GetTunerLevel()
        {
            int tunerLevel = 0;

            bool levelForSelect1 = false;
            foreach (var material in materials)
                tunerLevel += material.levelForSelect_1;
            if (tunerLevel == CardsManager.Get(summonCard).Level)
                levelForSelect1 = true;

            tunerLevel = 0;
            foreach (var material in materials)
            {
                if ((material.GetData().Type & (uint)CardType.Tuner) > 0)
                {
                    if (levelForSelect1)
                        tunerLevel += material.levelForSelect_1;
                    else
                        tunerLevel += material.levelForSelect_2;
                }
            }
            if (tunerLevel == 0)
            {
                foreach (var material in materials)
                {
                    var data = material.GetCachedData();
                    if ((data.Type & (uint)CardType.Tuner) > 0)
                        tunerLevel += data.Level;
                }
                if (tunerLevel == 0)
                    tunerLevel = materials[0].GetCachedData().Level;
            }
            return tunerLevel;
        }

        public IEnumerator RefreshCardFace(Renderer face, int code, bool post = false)
        {
            var ie = Program.I().texture_.LoadCardAsync(code);
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            if (!post)
            {
                var mat = TextureManager.GetCardMaterial(code);
                face.material = mat;
            }
            face.material.mainTexture = ie.Current;
        }

        public IEnumerator RefreshCardFrame(Renderer face, int count, int order = 0)
        {
            if (count > 100)
            {
                var ie = Program.I().texture_.LoadCardAsync(count);
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                face.material.SetTexture("_CardFrameA", ie.Current);
            }
            else
            {
                var materials = Program.I().ocgcore.materialCards;
                for (int i = 0; i < count; i++)
                {
                    var ie = Program.I().texture_.LoadCardAsync(materials[i + order].GetData().Id);
                    StartCoroutine(ie);
                    while (ie.MoveNext())
                        yield return null;
                    face.material.SetTexture("_CardFrame" + (char)('A' + i), ie.Current);
                }
            }
        }

        public void Skip()
        {
            if (inSummonMaterial)
            {
                Destroy(currentManager.transform.parent.gameObject);
                inSummonMaterial = false;
                Program.I().timeline_.ShiftToSummonTimeline();
                skipping = true;
            }
            PlayableDirector director;
            if (currentSyncManager == null)
            {
                if (currentManager == null)
                {
                    Debug.Log("TimelineManager: Did not find Manager!!!");
                    return;
                }
                director = currentManager.GetComponent<PlayableDirector>();
            }
            else
                director = currentSyncManager.GetComponent<PlayableDirector>();

            skippable = false;
            AudioManager.ResetSESource();
            foreach (PlayableBinding pb in director.playableAsset.outputs)
            {
                var track = pb.sourceObject as TrackAsset;
                if (track != null)
                {
                    foreach (TimelineClip clip in track.GetClips())
                        if (clip.displayName == "StrongSummon")
                            director.time = clip.start;
                }
            }
        }

        void DeleteWhenStop(GameObject director)
        {
            if (director == null)
                return;
            var mono = director.AddComponent<DoWhenPlayableDirectorStop>();
            mono.action = () =>
            {
                Destroy(director);
            };
        }
    }
}
