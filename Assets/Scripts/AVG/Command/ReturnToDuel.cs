using MDPro3;
using Naninovel;
using Naninovel.Commands;
using Naninovel.UI;
using System;
using System.Threading;
using UnityEngine;
using static Naninovel.Command;

[CommandAlias("RetuenDuel")]
public class ReturnToDuel : Command
{

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        Program.I().ExitCurrentServant();
        Program.I().StopTimeForShow = false;
        return UniTask.CompletedTask;
    }
}