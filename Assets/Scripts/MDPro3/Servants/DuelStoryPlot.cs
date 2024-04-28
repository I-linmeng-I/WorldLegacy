using System.Collections;
using System.Collections.Generic;
using MDPro3;
using Naninovel;
using Naninovel.Commands;
using UnityEngine;

public class DuelStoryPlot : Servant
{
    public GameObject AVG;
    public RenderTexture rendertexture;
    public string StartScriptName;

    public override void Initialize()
    {
        depth = -1;
        haveLine = false;
        returnServant = Program.I().menu;
        base.Initialize();
    }

    public override void Show(int preDepth)
    {
        if (Program.I().currentServant == Program.I().ocgcore)
        {
            Program.I().currentSubServant = this;
        }
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

        Program.I().StoryPlot.ContinueInputUI.SetActive(true);
        base.Show(preDepth);
    }

    public override void OnReturn()
    {
        AsyncToken asyncToken = default;
        var hidePrinter = new HidePrinter();
        hidePrinter.Execute(asyncToken).Forget();

        Program.I().StoryPlot.ContinueInputUI.SetActive(false);

        var back = Engine.GetService<IBackgroundManager>();
        back.RemoveAllActors();

        var naniCamera = Engine.GetService<ICameraManager>().Camera;
        naniCamera.enabled = false;

        base.OnReturn();
    }
}
