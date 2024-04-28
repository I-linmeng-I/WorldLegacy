using System.Collections;
using System.Collections.Generic;
using MDPro3;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEngine;
using WindBot.Game;
using YgomSystem.Network;
using static MDPro3.Room;


public class TestDuelPlotAI : GameAI
{
    public override PuzzleContent preload(){
        //设置残局
        PuzzleContent puzzleContent = new PuzzleContent();
        puzzleContent.player1LifePoint = 8000;
        puzzleContent.player2LifePoint = 50;
        puzzleContent.attackable = 1;
        puzzleContent.Puzzles = new List<puzzle_pointer>
        {
            new puzzle_pointer(32807846, 0, 0, (sbyte)CardLocation.SpellZone, 2, (sbyte)CardPosition.FaceDown),

            new puzzle_pointer(40044918, 0, 0,  (sbyte)CardLocation.Hand, 0, (sbyte)CardPosition.FaceDown),
            new puzzle_pointer(65367484, 0, 0, (sbyte)CardLocation.Hand, 1, (sbyte)CardPosition.FaceDown),

            new puzzle_pointer(79979666, 0, 0, (sbyte)CardLocation.Deck, 0, (sbyte)CardPosition.FaceUpAttack),
            new puzzle_pointer(79979666, 0, 0, (sbyte)CardLocation.Deck, 1, (sbyte)CardPosition.FaceUpAttack),

            new puzzle_pointer(28150174, 0, 0, (sbyte)CardLocation.Extra, 0, (sbyte)CardPosition.FaceDown),
            new puzzle_pointer(82944432, 0, 0, (sbyte)CardLocation.Extra, 1, (sbyte)CardPosition.FaceDown),
            new puzzle_pointer(60645181, 0, 0, (sbyte)CardLocation.Extra, 2, (sbyte)CardPosition.FaceDown),

            // new puzzle_pointer(61344030, 1, 1, (sbyte)CardLocation.MonsterZone, 2, (sbyte)CardPosition.FaceUpDefence),
            // new puzzle_pointer(92125819, 1, 1, (sbyte)CardLocation.MonsterZone, 2, (sbyte)CardPosition.FaceUpDefence),
            // new puzzle_pointer(92125819, 1, 1, (sbyte)CardLocation.MonsterZone, 2, (sbyte)CardPosition.FaceUpDefence),

            new puzzle_pointer(18964575, 1, 1, (sbyte)CardLocation.Deck, 0, (sbyte)CardPosition.FaceUpAttack ,false)
        };

        //注册演出效果
        eventManager.AddEventEffect("EVENT_SUMMON_SUCCESS",Effect1);

        return puzzleContent;
    }

    public void Effect1(params object[] args){
        if((int)args[0] == 1){
            DuelEffectFunction.StartDialog("test1");
        }
    }
}
