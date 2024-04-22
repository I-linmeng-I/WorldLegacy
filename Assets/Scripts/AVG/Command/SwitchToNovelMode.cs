using MDPro3;
using Naninovel;
using UnityEngine;

[CommandAlias("novel")]
public class SwitchToNovelMode : Command
{
    public StringParameter ScriptName;
    public StringParameter Label;

    public override async UniTask Execute (AsyncToken asyncToken = default)
    {
        // // 1. Disable character control.
        // var controller = Object.FindObjectOfType<CharacterController3D>();
        // controller.IsInputBlocked = true;

        // 2. Switch cameras.
        // var advCamera = GameObject.Find("AdventureModeCamera").GetComponent<Camera>();
        // advCamera.enabled = false;
        var naniCamera = Engine.GetService<ICameraManager>().Camera;
        naniCamera.enabled = true;

        // 3. Load and play specified script (if assigned).
        if (Assigned(ScriptName))
        {
            var scriptPlayer = Engine.GetService<IScriptPlayer>();
            await scriptPlayer.PreloadAndPlayAsync(ScriptName, label: Label);
        }

        // // 4. Enable Naninovel input.
        // Program.I().gameObject.GetComponent<InputManager>().ProcessInput = false;
        // var inputManager = Engine.GetService<IInputManager>();
        // inputManager.ProcessInput = true;
    }
}