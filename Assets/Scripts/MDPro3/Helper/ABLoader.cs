using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Willow;
using UnityEngine.Playables;
using Willow.InGameField;
using UnityEngine.Rendering;
using System;
using YgomGame;

namespace MDPro3
{
    public class ABLoader : MonoBehaviour
    {
        public static Dictionary<string, GameObject> cachedAB = new Dictionary<string, GameObject>();
        public static Dictionary<string, List<GameObject>> cachedABFolder = new Dictionary<string, List<GameObject>>();
        public static Dictionary<string, Material> cachedPMat = new Dictionary<string, Material>();
        public static IEnumerator CacheFromFileAsync(string path)
        {
            var abr = AssetBundle.LoadFromFileAsync(path);
            while (!abr.isDone)
                yield return null;
        }
        public static GameObject LoadFromFile(string path, bool cache = false)
        {
            GameObject returnValue;
            if (cachedAB.TryGetValue(path, out returnValue))
            {
                returnValue = Instantiate(returnValue);
                return returnValue;
            }

            AssetBundle ab;
            ab = AssetBundle.LoadFromFile(Program.root + path);
            var prefabs = ab.LoadAllAssets();
            foreach (UnityEngine.Object prefab in prefabs)
            {
                if (typeof(GameObject).IsInstanceOfType(prefab))
                {
                    if (cache)
                        cachedAB.Add(path, prefab as GameObject);
                    returnValue = Instantiate(prefab as GameObject);
                    ab.Unload(false);
                    return returnValue;
                }
            }
            return null;
        }
        public static IEnumerator<GameObject> LoadFromFileAsync(string path, bool cache = false, bool copy = true)
        {
            GameObject returnValue;
            if (cachedAB.TryGetValue(path, out returnValue))
            {
                if (copy)
                {
                    returnValue = Instantiate(returnValue);
                    yield return returnValue;
                }
                yield break;
            }

            var abr = AssetBundle.LoadFromFileAsync(Program.root + path);
            while (!abr.isDone)
                yield return null;
            AssetBundle ab = abr.assetBundle;
            var prefabs = ab.LoadAllAssets();

            foreach (UnityEngine.Object prefab in prefabs)
            {
                if (typeof(GameObject).IsInstanceOfType(prefab))
                {
                    if (cache)
                        cachedAB.Add(path, prefab as GameObject);
                    returnValue = prefab as GameObject;
                }
            }
            ab.Unload(false);
            if (copy)
                yield return Instantiate(returnValue);
        }
        public static GameObject LoadFromFolder(string path, string abName = "GameObject", bool cache = false)
        {
            GameObject returnValue = new GameObject(abName);
            if (cachedABFolder.TryGetValue(path, out var cachedPrefabs))
            {
                foreach (var prefab in cachedPrefabs)
                {
                    var go = Instantiate(prefab);
                    go.transform.SetParent(returnValue.transform, false);
                }
                return returnValue;
            }

            List<AssetBundle> bundles = new List<AssetBundle>();
            DirectoryInfo dir;
            dir = new DirectoryInfo(Program.root + path);
            FileInfo[] files = dir.GetFiles("*");
            for (int i = 0; i < files.Length; i++)
                bundles.Add(AssetBundle.LoadFromFile(files[i].FullName));
            List<GameObject> cached = new List<GameObject>();
            foreach (AssetBundle bundle in bundles)
            {
                var prefabs = bundle.LoadAllAssets();
                for (int j = 0; j < prefabs.Length; j++)
                    if (typeof(GameObject).IsInstanceOfType(prefabs[j]))
                        cached.Add(prefabs[j] as GameObject);
            }
            if (cache)
                if (!cachedABFolder.ContainsKey(path))
                    cachedABFolder.Add(path, cached);
            foreach (var prefab in cached)
            {
                var go = Instantiate(prefab);
                go.transform.SetParent(returnValue.transform, false);
            }

            foreach (AssetBundle bundle in bundles)
                bundle.Unload(false);

            return returnValue;
        }
        public static IEnumerator<GameObject> LoadFromFolderAsync(string path, string abName = "GameObject", bool cache = false, bool copy = true)
        {
            GameObject returnValue = new GameObject(abName);
            if (cachedABFolder.TryGetValue(path, out var cachedPrefabs))
            {
                if (copy)
                {
                    foreach (var prefab in cachedPrefabs)
                    {
                        var go = Instantiate(prefab);
                        go.transform.SetParent(returnValue.transform, false);
                    }
                    yield return returnValue;
                }
                else
                    Destroy(returnValue);
                yield break;
            }

            List<AssetBundle> bundles = new List<AssetBundle>();
            DirectoryInfo dir = new DirectoryInfo(Program.root + path);
            FileInfo[] files = dir.GetFiles("*");
            for (int i = 0; i < files.Length; i++)
            {
                var abr = AssetBundle.LoadFromFileAsync(files[i].FullName);
                while (!abr.isDone)
                    yield return null;
                bundles.Add(abr.assetBundle);
            }
            var cached = new List<GameObject>();
            foreach (AssetBundle bundle in bundles)
            {
                var prefabs = bundle.LoadAllAssets();
                for (int j = 0; j < prefabs.Length; j++)
                    if (typeof(GameObject).IsInstanceOfType(prefabs[j]))
                        cached.Add(prefabs[j] as GameObject);
            }

            if (cache)
                if (!cachedABFolder.ContainsKey(path))
                    cachedABFolder.Add(path, cached);
            foreach (AssetBundle bundle in bundles)
                bundle.Unload(false);
            if (copy)
            {
                foreach (var prefab in cached)
                {
                    var go = Instantiate(prefab);
                    go.transform.SetParent(returnValue.transform, false);
                }
                yield return returnValue;
            }
            else
            {
                Destroy(returnValue);
                yield return null;
            }
        }

        static bool loadingPMat;
        public static IEnumerator<Material> LoadProtectorMaterial(string code)
        {
            if (cachedPMat.TryGetValue(code, out var material))
            {
                if (material != null)
                {
                    yield return material;
                    yield break;
                }
                else
                    cachedPMat.Remove(code);
            }
            while (loadingPMat)
                yield return null;
            loadingPMat = true;
            if (File.Exists(Program.root + "Protector/" + code + Program.slash + code))
            {
                var abr = AssetBundle.LoadFromFileAsync(Program.root + "Protector/" + code + Program.slash + code);
                while (!abr.isDone)
                    yield return null;
                var abmr = AssetBundle.LoadFromFileAsync(Program.root + "Protector/" + code + "/PMat");
                while (!abmr.isDone)
                    yield return null;
                var ab = abr.assetBundle;
                var abm = abmr.assetBundle;
                material = abm.LoadAsset<Material>("PMat");
                material.renderQueue = 3000;
                ab.Unload(false);
                abm.Unload(false);
            }
            else
            {
                var ab1r = AssetBundle.LoadFromFileAsync(Program.root + "Protector/" + code + Program.slash + code + "_1");
                while (!ab1r.isDone)
                    yield return null;
                var ab1 = ab1r.assetBundle;

                var ab2r = AssetBundle.LoadFromFileAsync(Program.root + "Protector/" + code + Program.slash + code + "_2");
                while (!ab2r.isDone)
                    yield return null;
                var ab2 = ab2r.assetBundle;

                AssetBundle ab3 = null;
                if (File.Exists(Program.root + "Protector/" + code + Program.slash + code + "_3"))
                {
                    var ab3r = AssetBundle.LoadFromFileAsync(Program.root + "Protector/" + code + Program.slash + code + "_3");
                    while (!ab3r.isDone)
                        yield return null;
                    ab3 = ab3r.assetBundle;
                }

                var abmr = AssetBundle.LoadFromFileAsync(Program.root + "Protector/" + code + "/PMat");
                while (!abmr.isDone)
                    yield return null;
                var abm = abmr.assetBundle;
                material = abm.LoadAsset<Material>("PMat");
                material.renderQueue = 3000;
                material.enableInstancing = true;

                ab1.Unload(false);
                ab2.Unload(false);
                if (ab3 != null)
                    ab3.Unload(false);
                abm.Unload(false);
            }
            if (cachedPMat.ContainsKey(code))
                material = cachedPMat[code];
            else
                cachedPMat.Add(code, material);
            loadingPMat = false;
            yield return material;
        }
        public static IEnumerator<Material> LoadFrameMaterial(string code)
        {
            var abr = AssetBundle.LoadFromFileAsync(Program.root + "Frame/ProfileFrameMat" + code);
            while (!abr.isDone)
                yield return null;
            var ab = abr.assetBundle;
            var material = ab.LoadAsset<Material>("ProfileFrameMat" + code);
            ab.Unload(false);
            yield return material;
        }
        public static IEnumerator<Mate> LoadMateAsync(int code)
        {
            Items.Item item = new Items.Item();
            foreach (var mate in Program.items.mates)
            {
                if (mate.id == code)
                {
                    item = mate;
                    break;
                }
            }
            Mate.MateType type = Mate.MateType.MasterDuel;
            if (item.id == 0 && File.Exists(Program.root + "CrossDuel/" + code + ".bundle"))
                type = Mate.MateType.CrossDuel;
            Mate returnValue = null;
            if (type == Mate.MateType.CrossDuel)
            {
                var abr = AssetBundle.LoadFromFileAsync(Program.root + "CrossDuel/" + code + ".bundle");
                while (!abr.isDone)
                    yield return null;
                var ab = abr.assetBundle;
                var all = ab.LoadAllAssets();
                ab.Unload(false);
                foreach (var asset in all)
                {
                    if (asset is NamedAssetContainer container)
                    {
                        container.TryGet<GameObject>("prefab", out var prefab);
                        container.TryGet<NamedAssetContainer>("Timelines", out var timelines);
                        container.TryGet<ParameterContainer>("Settings", out var settings);
                        var mateGo = Instantiate(prefab);
                        mateGo.AddComponent<FieldParamEventController_AnimationEventReceiver>();
                        foreach (var s in timelines.AllNamedAssetNames())
                        {
                            timelines.TryGet<GameObject>(s, out var timeline);
                            var newT = Instantiate(timeline);
                            newT.transform.SetParent(mateGo.transform, false);
                            newT.SetActive(true);
                            for (int i = 0; i < newT.transform.childCount; i++)
                            {
                                if (newT.transform.GetChild(i).GetComponent<Volume>() != null)
                                    Destroy(newT.transform.GetChild(i).gameObject);
                                if (newT.transform.GetChild(i).name == "UIBattleDownAni")
                                    Destroy(newT.transform.GetChild(i).gameObject);
                            }
                            var controller = newT.GetComponent<CustomTimelineController>();
                            var bindTrackInfo = controller.checkReplacer.m_bindTrackInfo;
                            var director = newT.transform.GetChild(0).GetComponent<PlayableDirector>();

                            if (director == null)
                                continue;
                            Dictionary<string, PlayableBinding> bindingDict = new Dictionary<string, PlayableBinding>();
                            foreach (PlayableBinding pb in director.playableAsset.outputs)
                                foreach (var bind in bindTrackInfo)
                                    if (pb.streamName == bind.m_name
                                        && director.GetGenericBinding(pb.sourceObject) == null)
                                        director.SetGenericBinding(pb.sourceObject, mateGo.GetComponent<Animator>());
                        }
                        returnValue = mateGo.AddComponent<Mate>();
                    }
                }
            }
            else
            {
                var ie = LoadFromFileAsync(Program.items.CodeToPath(code.ToString(), Items.ItemType.Mate));
                while (ie.MoveNext())
                    yield return null;
                var mateGo = ie.Current;
                returnValue = mateGo.AddComponent<Mate>();
            }
            returnValue.type = type;
            returnValue.code = code;
            yield return returnValue;
        }


    }
}

