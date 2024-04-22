using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MDPro3;
using Naninovel;
using Naninovel.Commands;
using UnityEngine;

public class StoryPlot : Servant
{
    public GameObject AVG;
    public override async void Initialize()
    {
        depth = 1;
        haveLine = true;
        returnServant = Program.I().menu;
        base.Initialize();
        await Load();
        setup();
    }
    public override void Show(int preDepth)
    {
        base.Show(preDepth);
        AVG.gameObject.SetActive(true);
        // var switchCommand = new SwitchToNovelMode { ScriptName = "TestDuelDialogue" };
        // switchCommand.ExecuteAsync().Forget();
        var naniCamera = Engine.GetService<ICameraManager>().Camera;
        naniCamera.enabled = true;
        var player = Engine.GetService<IScriptPlayer>();
        player.PreloadAndPlayAsync("TestDuelDialogue").Forget();
    }
    public async Task Load()
    {
        await RuntimeInitializer.InitializeAsync();
    }

    public void setup(){
        // 5. Switch cameras.
        var naniCamera = Engine.GetService<ICameraManager>().Camera;
        naniCamera.enabled = false;
    }
}
