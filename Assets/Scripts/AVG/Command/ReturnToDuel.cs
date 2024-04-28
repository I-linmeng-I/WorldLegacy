using MDPro3;
using Naninovel;
using Naninovel.Commands;
using Naninovel.UI;
using System;
using System.Threading;
using UnityEditor.VersionControl;
using UnityEngine;
using static Naninovel.Command;

[CommandAlias("RetuenDuel")]
public class ReturnToDuel : Command
{

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        Program.I().ExitCurrentServant();
        return UniTask.CompletedTask;
    }
}