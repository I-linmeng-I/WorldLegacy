using MDPro3;
using Naninovel;
using Naninovel.Commands;
using Naninovel.UI;
using System;
using System.Threading;
using UnityEditor.VersionControl;
using UnityEngine;
using WindBot.Game;
using static Naninovel.Command;

[CommandAlias("puzzle")]
public class StartPuzzle : Command
{
    public StringParameter DuelPlotName;

    public StringParameter NextScript;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        // // 1. Disable character control.
        // var controller = Object.FindObjectOfType<CharacterController3D>();
        // controller.IsInputBlocked = true;

        // 2. Switch cameras.
        // var advCamera = GameObject.Find("AdventureModeCamera").GetComponent<Camera>();
        // advCamera.enabled = false;
        var hidePrinter = new HidePrinter();
        hidePrinter.Execute(asyncToken).Forget();

        Program.I().StoryPlot.ContinueInputUI.SetActive(false);

        var back = Engine.GetService<IBackgroundManager>();
        back.RemoveAllActors();

        Type type = Type.GetType(DuelPlotName);
        var plotInstance = Activator.CreateInstance(type);
        Program.I().StoryPlot.currentDuelPlot = (GameAI)plotInstance;
        Program.I().solo.StartAI(5);

        var naniCamera = Engine.GetService<ICameraManager>().Camera;
        naniCamera.enabled = false;

        Program.I().StoryPlot.StartScriptName = NextScript;
        // // 4. Enable Naninovel input.
        // Program.I().gameObject.GetComponent<InputManager>().ProcessInput = false;
        // var inputManager = Engine.GetService<IInputManager>();
        // inputManager.ProcessInput = true;
        //throw new System.NotImplementedException();
        return UniTask.CompletedTask;
    }
}