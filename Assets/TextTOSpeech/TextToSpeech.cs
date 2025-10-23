using System.Runtime.InteropServices;

public static class TextToSpeech
{
    public static void SpeechText(string text)
    {
        ttsrust_say(text);
    }

    public static void StopSpeech()
    {
        ttsrust_stop();
    }

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_WEBGL)
    const string _dll = "__Internal";
#else
    const string _dll = "ttsrust";
#endif

    [DllImport(_dll)]
    static extern void ttsrust_say(string text);

    [DllImport(_dll)]
    static extern void ttsrust_stop();
}