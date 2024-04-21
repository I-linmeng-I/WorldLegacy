using System;
using System.Collections;
using System.Collections.Generic;
using MDPro3;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using UnityEditor.Build.Reporting;
using UnityEngine;
using YgomGame.Duel;
using static MDPro3.Room;

public class DuelPlot : MonoBehaviour
{
    public int step =0;
    public List<DuelPlotStep> plotSteps= new List<DuelPlotStep>();

    virtual public PuzzleContent preload(){
        return new PuzzleContent();
    }

    virtual public void update(){
        
    }

    public static explicit operator DuelPlot(Type v)
    {
        throw new NotImplementedException();
    }
}
