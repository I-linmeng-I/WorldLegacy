using System;
using System.Collections.Generic;
using MDPro3;
using PimDeWitte.UnityMainThreadDispatcher;

// 定义事件参数类
public class MyEventArgs : EventArgs
{
    public string EventType { get; set; }
    public string Effect { get; set; }

    public MyEventArgs(string eventType, string effect)
    {
        EventType = eventType;
        Effect = effect;
    }
}

// 定义事件管理器类
public class DuelEventManager
{
    // 定义事件与效果的对应关系
    private Dictionary<string, List<Action>> eventEffectMap = new Dictionary<string, List<Action>>();

    // 添加事件与效果的对应关系
    public void AddEventEffect(string eventType, Action effect)
    {
        if (!eventEffectMap.ContainsKey(eventType))
        {
            eventEffectMap[eventType] = new List<Action>();
        }
        eventEffectMap[eventType].Add(effect);
    }

    // 触发事件的方法
    public void TriggerEvent(string eventType)
    {
        if (eventEffectMap.ContainsKey(eventType))
        {
            foreach (var effect in eventEffectMap[eventType])
            {
                effect(); // 调用效果处理函数
            }
        }
    }

    // 添加事件与效果的对应关系的方法
    public void AddEventAndEffect(string eventType, DuelEffectManager.EffectHandler effect, DuelEffectManager effectManager)
    {
        effectManager.AddEffect(effect.Method.Name, effect); // 添加效果处理函数
        // 使用 lambda 表达式将 EffectHandler 转换为 Action
        Action action = () => effect(new object[] { }); 
        AddEventEffect(eventType, action); // 添加事件与效果的对应关系
    }
}

// 定义效果管理器类
public class DuelEffectManager
{
    // 效果处理函数的委托
    public delegate void EffectHandler(params object[] args);

    // 效果处理函数的字典
    private Dictionary<string, EffectHandler> effects = new Dictionary<string, EffectHandler>();

    // 添加效果处理函数
    public void AddEffect(string effectName, EffectHandler handler)
    {
        effects[effectName] = handler;
    }

    // 触发指定效果处理函数
    public void TriggerEffect(string effectName, params object[] args)
    {
        if (effects.ContainsKey(effectName))
        {
            effects[effectName]?.Invoke(args);
        }
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