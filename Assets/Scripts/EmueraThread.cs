using System;
using System.Threading;
using UnityEngine;

public class EmueraThread
{
    public static EmueraThread instance { get { return instance_; } }
    static EmueraThread instance_ = new EmueraThread();

    EmueraThread()
    { }

    public void Start(bool debug)
    {
        debugmode = debug;
        running = true;

        ////初始化
        //MinorShift.Emuera.Program.debugMode = debugmode;
        //MinorShift.Emuera.Program.Main(new string[0] { });

        ThreadPool.QueueUserWorkItem(new WaitCallback(p =>
        {
            Work();
        }));
    }

    public void End()
    {
        running = false;
    }

    public void Input(string c, bool from_button)
    {
        var console = MinorShift.Emuera.GlobalStatic.Console;
        if(console == null)
            return;
        if(!from_button && console.IsWaitingInputSomething)
            return;
        input = c;
    }

    void Work()
    {
        //初始化
        MinorShift.Emuera.Program.debugMode = debugmode;
        MinorShift.Emuera.Program.Main(new string[0] { });

        input = null;
        var console = MinorShift.Emuera.GlobalStatic.Console;
        while(running)
        {
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
                console.PressEnterKey(false, input, false);
            }
            Thread.Sleep(10);
            input = null;
        }
    }

    bool debugmode;
    bool running;
    string input;
}
