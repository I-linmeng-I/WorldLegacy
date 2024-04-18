using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using MDPro3.YGOSharp;
using MDPro3.UI;
using System.Text.RegularExpressions;

namespace MDPro3
{
    public class MateView : Servant
    {
        public ScrollRect scrollRect;
        public InputField inputField;

        static List<int> crossDuelMates = new List<int>();
        static List<Card> cards = new List<Card>();
        List<string[]> tasks = new List<string[]>();

        SuperScrollView superScrollView;
        Camera targetCamera;

        static Mate mate;

        Vector2 clickInPosition;
        Vector3 mateAngel;
        Vector3 matePosition;
        float oSize;
        float clickInTime;
        bool clickInLeft;
        bool clickInRight;

        public override void Initialize()
        {
            depth = 1;
            haveLine = false;
            returnServant = Program.I().menu;
            base.Initialize();
            targetCamera = Program.I().camera_.cameraDuelOverlay2D;
            inputField.onEndEdit.AddListener(Print);
#if UNITY_ANDROID
            var files = Directory.GetFiles(Program.root + "CrossDuel", "*.bundle");
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file).Replace(".bundle", "");
                crossDuelMates.Add(int.Parse(fileName));
            }
#endif
            Load();
            LoadSeData();
        }
        public void Load()
        {
            cards.Clear();
            for (int i = 0; i < crossDuelMates.Count; i++)
            {
                var card = CardsManager.Get(crossDuelMates[i]);
                if (card.Id == 0)
                {
                    card.Id = crossDuelMates[i];
                    card.Name = GetRushDuelMateName(crossDuelMates[i]);
                }
                cards.Add(card);
            }
            cards.Sort(CardsManager.ComparisonOfCard());
            DOTween.To(v => { }, 0, 0, 0.1f).OnComplete(() =>
            {

            });
            Print();
        }
        public void Print(string search = "")
        {
            superScrollView?.Clear();
            tasks.Clear();
            foreach (var card in cards)
            {
                if (card.Name.Contains(search))
                {
                    string[] task = new string[] { card.Id.ToString(), card.Name };
                    tasks.Add(task);
                }
            }
            foreach (var mate in Program.items.mates)
            {
                if ((!string.IsNullOrEmpty(mate.name) && mate.name.Contains(search)))
                {
                    string[] task = new string[] { mate.id.ToString(), mate.name };
                    tasks.Add(task);
                }
            }
            var handle = Addressables.LoadAssetAsync<GameObject>("ButtonMateView");
            handle.Completed += (result) =>
            {
                superScrollView = new SuperScrollView
                    (
                    1,
                    360,
                    40,
                    0,
                    0,
                    result.Result,
                    ItemOnListRefresh,
                    scrollRect
                    );
                superScrollView.Print(tasks);
            };
        }

        public override void Show(int preDepth)
        {
            base.Show(preDepth);
            Program.I().camera_.light.gameObject.SetActive(true);
            CameraManager.DuelOverlay2DPlus();
            CameraReset();
            AudioManager.PlayBGM("BGM_OUT_TUTORIAL_2", 0.5f);
        }

        void CameraReset()
        {
            targetCamera.transform.localPosition =
                new Vector3(0, 20 * 0.95f, 200);
            targetCamera.transform.localEulerAngles =
                new Vector3(0, 180, 0);
            targetCamera.orthographicSize = 20;
        }
        public override void ApplyHideArrangement(int preDepth)
        {
            base.ApplyHideArrangement(preDepth);
            CameraManager.DuelOverlay2DMinus();
            Program.I().camera_.light.gameObject.SetActive(false);
            if (mate != null)
                Destroy(mate.gameObject);
            AudioManager.ResetSESource();
            AudioManager.PlaySE("SE_MENU_CANCEL");
            AudioManager.PlayBGM("BGM_MENU_01");
        }


        public override void PerFrameFunction()
        {
            base.PerFrameFunction();
            if (isShowed && mate != null)
            {
                if (Program.InputGetMouse0Down && Input.mousePosition.x > 450 * Screen.height / 1080)
                {
                    var widthOffset = Screen.width - 450 * Screen.height / 1080f;

                    if (Input.mousePosition.x > 450 * Screen.height / 1080 + widthOffset / 2)
                    {
                        clickInRight = true;
                        clickInTime = Time.time;
                        mateAngel = mate.transform.eulerAngles;
                        clickInPosition = Input.mousePosition;
                        oSize = targetCamera.orthographicSize;
                    }
                    else
                    {
                        clickInLeft = true;
                        clickInTime = Time.time;
                        clickInPosition = Input.mousePosition;
                        matePosition = mate.transform.position;
                    }
                }
                if (Program.InputGetMouse0 && clickInLeft)
                {
                    var x = matePosition.x + (clickInPosition.x - Input.mousePosition.x) * 0.01f;
                    var y = matePosition.y + (Input.mousePosition.y - clickInPosition.y) * 0.02f;
                    if (x > 10) x = 10;
                    if (x < -10) x = -10;
                    if (y > 0) y = 0;
                    if (y < -20) y = -20;
                    mate.transform.position = new Vector3(x, y, matePosition.z);
                }
                if (Program.InputGetMouse0 && clickInRight)
                {
                    mate.transform.eulerAngles = mateAngel +
                        new Vector3(0, (clickInPosition.x - Input.mousePosition.x) * 0.2f, 0);
                    var size = oSize + (clickInPosition.y - Input.mousePosition.y) * 0.01f;
                    if (size < 5) size = 5;
                    if (size > 20) size = 20;
                    targetCamera.orthographicSize = size;
                    targetCamera.transform.localPosition = new Vector3(0, size * 0.95f, 200);
                }
                if (Program.InputGetMouse0Up && (clickInLeft || clickInRight))
                {
                    clickInLeft = false;
                    clickInRight = false;
                    if ((Time.time - clickInTime) < 0.2f)
                    {
                        mate.Play(Mate.MateAction.Tap);
                        mate.ActiveCamera(Mate.MateAction.Tap, targetCamera.gameObject.layer);
                    }
                }
            }
        }

        public void ViewMate(int code)
        {
            if (mate != null)
            {
                if (mate.code == code)
                    return;
                else
                    Destroy(mate.gameObject);
            }
            StartCoroutine(LoadMateAsync(code));
        }

        IEnumerator LoadMateAsync(int code)
        {
            var ie = ABLoader.LoadMateAsync(code);
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            mate = ie.Current;
            Tools.ChangeLayer(mate.gameObject, targetCamera.gameObject.layer);
            yield return new WaitForSeconds(0.1f);
            AudioManager.ResetSESource();
            mate.gameObject.SetActive(true);
            mate.Play(Mate.MateAction.Entry);
            mate.ActiveCamera(Mate.MateAction.Entry, targetCamera.gameObject.layer);
            CameraReset();
        }

        void ItemOnListRefresh(string[] task, GameObject item)
        {
            var handler = item.GetComponent<SuperScrollViewItemForMate>();
            handler.code = int.Parse(task[0]);
            handler.mateName = task[1];
            handler.Refresh();
        }

        public static string GetRushDuelMateName(int code)
        {
            switch (code)
            {
                case 120105001:
                    return InterString.Get("���ǵ�ħ��ʦ");
                case 120105010:
                    return InterString.Get("�䵥ʹħ");
                case 120110001:
                    return InterString.Get("������ �ݳ�����");
                case 120110006:
                    return InterString.Get("˫����");
                case 120110010:
                    return InterString.Get("����С��");
                case 120115001:
                    return InterString.Get("���ǵ�ħŮ");
                case 120120018:
                    return InterString.Get("��������");
                case 120120025:
                    return InterString.Get("���ӷ��̻�����");
                case 120120024:
                    return InterString.Get("���Ӳ���Ͷ����");
                case 120120029:
                    return InterString.Get("ħ�� ����³��");
                case 120120003:
                    return InterString.Get("��֮�ػ���");
                case 120130016:
                    return InterString.Get("���ǵ���ʦ");
                case 120130026:
                    return InterString.Get("���� �ѵ�˹");
                case 120140023:
                    return InterString.Get("����ħ�塤����Ƥ");
                case 120145014:
                    return InterString.Get("��������Ů");
                case 120150002:
                    return InterString.Get("��ħ���� ��Ե���");
                case 120155019:
                    return InterString.Get("���� Ī������");
                default:
                    return string.Empty;
            }
        }


        #region CrossDuel Se Label Data
        public struct CrossDuelSeLabelData
        {
            public string name;
            public string label1;
            public float start1;
            public string label2;
            public float start2;
            public string label3;
            public float start3;
        }
        static List<CrossDuelSeLabelData> cdSeData = new List<CrossDuelSeLabelData>();

        void LoadSeData()
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>("EffectSeLabelData");
            handle.Completed += (result) =>
            {
                var content = handle.Result.text;
                var lines = Regex.Split(content, "\r\n");
                for(int i = 1; i < lines.Length; i++)
                {
                    var splits = lines[i].Split(',');
                    if (splits.Length == 7)
                    {
                        var seData = new CrossDuelSeLabelData();
                        seData.name = splits[0];
                        seData.label1 = splits[1];
                        if (string.IsNullOrEmpty(splits[2]))
                            seData.start1 = 0;
                        else
                            seData.start1 = float.Parse(splits[2]);
                        seData.label2 = splits[3];
                        if (string.IsNullOrEmpty(splits[4]))
                            seData.start2 = 0;
                        else
                            seData.start2 = float.Parse(splits[4]);
                        seData.label3 = splits[5];
                        if (string.IsNullOrEmpty(splits[6]))
                            seData.start3 = 0;
                        else
                            seData.start3 = float.Parse(splits[6]);
                        cdSeData.Add(seData);
                    }
                }
            };
        }

        public static void PlayCrossDuelSe(string name)
        {
            CrossDuelSeLabelData data = new CrossDuelSeLabelData();
            bool found = false;
            foreach(var seData in cdSeData)
                if (seData.name == name)
                {
                    data = seData;
                    found = true;
                }
            if (!found)
                return;
            if (!string.IsNullOrEmpty(data.label1))
            {
                DOTween.To(v => { }, 0, 0, data.start1).OnComplete(() =>
                {
                    AudioManager.PlaySE(data.label1.ToUpper());
                });
            }
            if (!string.IsNullOrEmpty(data.label2))
            {
                DOTween.To(v => { }, 0, 0, data.start2).OnComplete(() =>
                {
                    AudioManager.PlaySE(data.label2.ToUpper());
                });
            }
            if (!string.IsNullOrEmpty(data.label3))
            {
                DOTween.To(v => { }, 0, 0, data.start3).OnComplete(() =>
                {
                    AudioManager.PlaySE(data.label3.ToUpper());
                });
            }
        }

        #endregion
    }
}
