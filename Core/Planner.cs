using System.Collections.Generic;

public class Planner{

    protected int limit = 1000;

    internal static T Clone<T>(T x) => CloneUtil.DeepClone(x);

    protected T Pop<T>(List<T> l){
        int i = l.Count-1;
        var n = l[i];
        l.RemoveAt(i);
        return n;
    }

}