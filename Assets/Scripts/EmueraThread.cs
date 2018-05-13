using System;
using System.Threading;
using UnityEngine;

public class EmueraThread
{
    public static EmueraThread instance { get { return instance_; } }
    static EmueraThread instance_ = new EmueraThread();

    EmueraThread()
    { }

    public void Start(bool debug, bool use_coroutine)
    {
        debugmode = debug;
        running = true;
        if(use_coroutine)
        {
            coroutine = GenericUtils.StartCoroutine(WorkCo());
            return;
        }
        ThreadPool.QueueUserWorkItem(new WaitCallback(p =>
        {
            Work();
        }));
    }

    public void End()
    {
        if(coroutine != null)
        {
            GenericUtils.StopCoroutine(coroutine);
            coroutine = null;
        }
        running = false;
    }

    public bool Running()
    {
        var console = MinorShift.Emuera.GlobalStatic.Console;
        if(console != null && console.IsInProcess)
            return true;
        return false;
    }

    public void Input(string c, bool from_button, bool skip = false)
    {
        var console = MinorShift.Emuera.GlobalStatic.Console;
        if(console == null)
            return;
        if(!from_button && console.IsWaitingInputSomething)
            return;
        input = c;
        skipflag = skip;
    }
    public bool IsSkipFlag { get { return skipflag; } }

    void Work()
    {
        //初始化
        MinorShift.Emuera.Program.debugMode = debugmode;
        MinorShift.Emuera.Program.Main(new string[0] { });

        uEmuera.Utils.ResourceClear();
        GC.Collect();

        input = null;
        var console = MinorShift.Emuera.GlobalStatic.Console;
        var random = new System.Random();
        while(running)
        {
            skipflag = false;

            while(input == null)
            {
                Thread.Sleep(1);
                if(!running)
                    return;
                uEmuera.Forms.Timer.Update();
            }

            if(console.IsWaitingInput)
            {
                if(console.IsWaitingEnterKey)
                    input = "";
                console.PressEnterKey(skipflag, input, false);
            }
            Thread.Sleep(10);
            input = null;
        }
    }

    System.Collections.IEnumerator WorkCo()
    {
        //初始化
        MinorShift.Emuera.Program.debugMode = debugmode;
        MinorShift.Emuera.Program.Main(new string[0] { });

        uEmuera.Utils.ResourceClear();
        GC.Collect();
        yield return null;

        input = null;
        var console = MinorShift.Emuera.GlobalStatic.Console;
        while(running)
        {
            skipflag = false;

            while(input == null)
            {
                yield return null;
                if(!running)
                    yield break;
                uEmuera.Forms.Timer.Update();
            }

            if(console.IsWaitingInput)
            {
                if(console.IsWaitingEnterKey)
                    input = "";
                console.PressEnterKey(skipflag, input, false);
            }
            yield return new WaitForSeconds(0.01f);
            input = null;
        }
    }

    UnityEngine.Coroutine coroutine = null;
    bool debugmode;
    bool running;
    string input;
    bool skipflag;
}
