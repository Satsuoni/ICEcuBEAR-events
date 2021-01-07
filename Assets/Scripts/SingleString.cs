using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSynthesis.Synthesis;
using AudioSynthesis.Bank;


[RequireComponent(typeof(AudioSource))]
public class SingleString : MonoBehaviour
{

    AudioSource src;
    Synthesizer synthesizer;
    [SerializeField] int sampleRate = 44100;
    [SerializeField] int bufferSize = 1024;
    int bufferHead;
    float[] currentBuffer;
    // Start is called before the first frame update
    void Start()
    {
        src = gameObject.GetComponent<AudioSource>();
        if(synthesizer==null)
        synthesizer = new Synthesizer(sampleRate, 1, bufferSize, 1);
    }
    bool setup=false;
    PatchBank bank;
    int programNum = 0;

    int minKey = 20;
    int maxKey = 85;
    int winding = 0;
    float ballstep=0.1f;
    float[] ballDecay;
    public void SetupWithBank(PatchBank bank,int stringId,int maxPatch,int minkey,int maxkey,int numBalls,int maxString=86)
    {
        if (synthesizer == null)
            synthesizer = new Synthesizer(sampleRate, 1, bufferSize, 1);
        if (bank == null) return;
        if (numBalls == 0) return;
        synthesizer.UnloadBank();
        synthesizer.LoadBank(bank);
        this.bank = bank;
        int maxWinding = maxString / maxPatch;
        winding = stringId / maxPatch;
        if (minkey == 0) minkey = 20;
        if (maxkey == 0) maxkey = 85;
        if (maxkey <= minkey+maxWinding) maxkey = minkey + 60+maxWinding;
        minKey = minkey;
        maxKey = maxkey;
        ballstep = (float)(maxKey - maxWinding - minkey) / (float)numBalls;
        ballDecay = new float[numBalls];
        programNum = stringId % maxPatch;
        synthesizer.ProcessMidiMessage(1, 0xC0, programNum, 0);
        setup = true;
    }
    public void HitBall(int ballnum,int strength)
    {
        if (!setup) return;
         float noteal = minKey + winding + ballstep * (ballnum-1);
        int note = Mathf.FloorToInt(noteal);
#if UNITY_EDITOR
        //  Debug.LogFormat("Hitting {0} on {1}", note,bank.GetPatch(0, programNum).Name);
#endif
        //  note = 75;
        
        //Debug.LogFormat("Hitting note: {0} {1} {2},   {3} {4} pn: {5} {6} {7}",note,ballnum,minKey,ballstep,winding, programNum,strength,setup);
        synthesizer.NoteOffAll(false);
        
        try
        {
            synthesizer.NoteOn(1, note, strength);
        }
        catch
        {

        }
        
    }
    public void StopPlaying()
    {
        synthesizer.NoteOffAll(true);
    }


 
    void OnAudioFilterRead(float[] data, int channel)
    {
        if (!setup) return;
        if (bank==null) return;
        int count = 0;
        while (count < data.Length)
        {
            if (currentBuffer == null || bufferHead >= currentBuffer.Length)
            {

                synthesizer.GetNext();
                currentBuffer = synthesizer.WorkingBuffer;
                bufferHead = 0;
            }
            var length = Mathf.Min(currentBuffer.Length - bufferHead, data.Length - count);
            System.Array.Copy(currentBuffer, bufferHead, data, count, length);
            bufferHead += length;
            count += length;
        }
    }
}
