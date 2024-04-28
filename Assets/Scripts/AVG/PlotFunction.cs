using System;
using System.Collections.Generic;
using MDPro3;
using PimDeWitte.UnityMainThreadDispatcher;

// 定义事件管理器类
public class DuelEventManager
{
    // 定义事件与效果的对应关系
    private Dictionary<string, List<Action<object[]>>> eventEffectMap = new Dictionary<string, List<Action<object[]>>>();

    // 添加事件与效果的对应关系
    public void AddEventEffect(string eventType, Action<object[]> effect)
    {
        if (!eventEffectMap.ContainsKey(eventType))
        {
            eventEffectMap[eventType] = new List<Action<object[]>>();
        }
        eventEffectMap[eventType].Add(effect);
    }

    // 触发事件的方法（不带参数）
    public void TriggerEvent(string eventType, params object[] args)
    {
        TriggerEvent<object>(eventType, args);
    }

    // 触发事件的方法（带参数）
    public T TriggerEvent<T>(string eventType, params object[] args)
    {
        T result = default(T);
        if (eventEffectMap.ContainsKey(eventType))
        {
            foreach (var effect in eventEffectMap[eventType])
            {
                effect(args); // 传递参数给效果处理函数
            }
        }
        return result;
    }
}

public static class DuelEffectFunction
{
    public static void StartDialog(string name){
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            
            Program.I().duelStoryPlot.StartScriptName = name;
            Program.I().ocgcore.description.Hide();
            Program.I().ShowSubServant(Program.I().duelStoryPlot);
        });
    }
}