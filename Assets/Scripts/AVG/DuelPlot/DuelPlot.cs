using System;
using System.Collections;
using System.Collections.Generic;
using MDPro3;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using UnityEditor.Build.Reporting;
using UnityEngine;
using YgomGame.Duel;
using static MDPro3.Room;

public class DuelPlot
{
    public int step =0;
    public List<DuelPlotStep> plotSteps= new List<DuelPlotStep>();

    virtual public List<puzzle_pointer> preload(){
        return new List<puzzle_pointer>();
    }

    virtual public void update(){
        
    }

    public static explicit operator DuelPlot(Type v)
    {
        throw new NotImplementedException();
    }
}
