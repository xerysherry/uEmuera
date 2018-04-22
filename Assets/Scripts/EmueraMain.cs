using UnityEngine;
using MinorShift.Emuera;
using MinorShift._Library;

public class EmueraMain : MonoBehaviour
{
    public void Run()
    {
        Sys.SetWorkFolder(workspace);
        Sys.SetSourceFolder(era_source);
        EmueraThread.instance.Start(debug, use_coroutine);
        working_ = true;
    }

    void Start()
    {}

    void Update()
    {
        if(!working_)
            return;

#if UNITY_EDITOR
        if(Input.anyKey)
            EmueraThread.instance.Input("", false);
        if(!string.IsNullOrEmpty(input))
        {
            EmueraThread.instance.Input(input, true);
            input = null;
        }
        if(MinorShift.Emuera.GlobalStatic.Console != null)
            console_lines = MinorShift.Emuera.GlobalStatic.Console.GetDisplayLinesCount();
#endif
        if(GlobalStatic.MainWindow != null)
            GlobalStatic.MainWindow.Update();
    }
    bool working_ = false;

    public string workspace;
    public string era_source;

    public bool debug = false;
    public bool use_coroutine = false;

#if UNITY_EDITOR
    public string input;
    public int console_lines;
#endif
}
