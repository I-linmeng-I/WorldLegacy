using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MDPro3;
using Naninovel;
using Naninovel.Commands;
using Naninovel.UI;
using UnityEngine;
using WindBot.Game;

public class StoryPlot : Servant
{
    public GameObject AVG;
    public GameObject ContinueInputUI;
    public RenderTexture rendertexture;
    public string StartScriptName;

    public GameAI currentDuelPlot;

    public override async void Initialize()
    {
        depth = 1;
        haveLine = false;
        returnServant = Program.I().menu;
        base.Initialize();
        await Load();
        setup();
    }
    public override void Show(int preDepth)
    {
        // var switchCommand = new SwitchToNovelMode { ScriptName = "TestDuelDialogue" };
        // switchCommand.ExecuteAsync().Forget();
        var naniCamera = Engine.GetService<ICameraManager>().Camera;
        naniCamera.enabled = true;
        var player = Engine.GetService<IScriptPlayer>();
        player.PreloadAndPlayAsync(StartScriptName).Forget();

        Camera camera = naniCamera.gameObject.GetComponent<Camera>();
        int width = camera.pixelWidth;
        int height = camera.pixelHeight;
        rendertexture = new RenderTexture(width,height,24);
        naniCamera.gameObject.GetComponent<Camera>().targetTexture = rendertexture;
        naniCamera.gameObject.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;

        AVG.GetComponent<RenderAVGDisplay>().setRenderTexture(rendertexture);

        ContinueInputUI.SetActive(true);
        base.Show(preDepth);
    }

    public async Task Load()
    {
        await RuntimeInitializer.InitializeAsync();
        // 5. Switch cameras.
        var naniCamera = Engine.GetService<ICameraManager>().Camera;
        naniCamera.enabled = false;
    }

    public void setup(){
        // 5. Switch cameras.
        var naniCamera = Engine.GetService<ICameraManager>().Camera;
        naniCamera.enabled = false;
        ContinueInputUI = GameObject.Find("ContinueInputUI").gameObject;
        ContinueInputUI.SetActive(false);
    }
}
