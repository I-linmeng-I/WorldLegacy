using MDPro3;
using Naninovel;
using Naninovel.Commands;
using System;
using System.Threading;
using UnityEditor.VersionControl;
using UnityEngine;
using static Naninovel.Command;

[CommandAlias("hello")]
public class HelloWorld : Command
{
    public StringParameter Name;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        // // 1. Disable character control.
        // var controller = Object.FindObjectOfType<CharacterController3D>();
        // controller.IsInputBlocked = true;

        // 2. Switch cameras.
        // var advCamera = GameObject.Find("AdventureModeCamera").GetComponent<Camera>();
        // advCamera.enabled = false;
        var naniCamera = Engine.GetService<ICameraManager>().Camera;
        naniCamera.enabled = false;

        Type type = Type.GetType("TestPlot");
        var plotInstance = Activator.CreateInstance(type);
        Program.I().currentDuelPlot = (DuelPlot)plotInstance;
        Program.I().solo.StartAI(5);

        // // 4. Enable Naninovel input.
        // Program.I().gameObject.GetComponent<InputManager>().ProcessInput = false;
        // var inputManager = Engine.GetService<IInputManager>();
        // inputManager.ProcessInput = true;
        //throw new System.NotImplementedException();
        return UniTask.CompletedTask;
    }
}