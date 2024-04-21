using System.Collections;
using System.Collections.Generic;
using MDPro3;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using Naninovel.Commands;
using UnityEngine;
using static MDPro3.Room;

public class TestPlot : DuelPlot
{
    public override List<puzzle_pointer> preload(){
        List<puzzle_pointer> puzzles = new List<puzzle_pointer>
        {
            new puzzle_pointer(80611581, 0, 0, (sbyte)CardLocation.Grave, 0, (sbyte)CardPosition.FaceUp),
            new puzzle_pointer(80611581, 0, 0, (sbyte)CardLocation.MonsterZone, 0, (sbyte)CardPosition.FaceUpAttack ,1)
        };
        return puzzles;
    }
}
