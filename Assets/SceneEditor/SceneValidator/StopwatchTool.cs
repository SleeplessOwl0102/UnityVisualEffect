
using UnityEngine;
using watch = System.Diagnostics.Stopwatch;

public static class StopwatchTool
{
    private static watch m_watch;

    static StopwatchTool()
    {
        m_watch = new watch();
    }

    private static string m_messageCache;
    private static long m_preElapsedMilliseconds;

    public static void Start(string message)
    {
        m_watch.Stop();
        if (m_messageCache != null)
        {
            var sec = (m_watch.ElapsedMilliseconds - m_preElapsedMilliseconds) / 1000f;
            Debug.LogWarning($"{m_messageCache} cost : {sec} seconds");
        }
        else
        {
            m_watch.Reset();
        }
        m_preElapsedMilliseconds = m_watch.ElapsedMilliseconds;
        m_messageCache = message;
        m_watch.Start();
    }

    public static void End()
    {

        m_watch.Stop();
        if (m_messageCache != null)
        {
            var sec = (m_watch.ElapsedMilliseconds - m_preElapsedMilliseconds) / 1000f;
            Debug.LogWarning($"{m_messageCache} cost : {sec} seconds");
            Debug.LogWarning($"Total cost : {m_watch.ElapsedMilliseconds/1000f} seconds");
        }

        m_preElapsedMilliseconds = 0;
        m_messageCache = null;
        m_watch.Reset();
    }

}