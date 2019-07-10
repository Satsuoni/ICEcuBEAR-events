using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSynthesis.Synthesis;
using AudioSynthesis.Bank;

[RequireComponent(typeof(AudioSource))]
public class NoteTest : MonoBehaviour
{
    AudioSource src;
    [SerializeField] TextAssetStream bankSource;
    Synthesizer synthesizer;
    int sampleRate = 44100;
    int bufferSize = 1024;
    int channel = 1;
    PatchBank bank;


    public PatchBank Bank { get { return bank; } }

    // Start is called before the first frame update
    void Start()
    {
        src = gameObject.GetComponent<AudioSource>();
        synthesizer = new Synthesizer(sampleRate, channel, bufferSize, 1);
        LoadBank(new PatchBank(bankSource));
    }
    int bufferHead;
    float[] currentBuffer;
    // Update is called once per frame
    float dt = 0;
    void Update()

    {
        dt += Time.deltaTime;
        if(dt>1)
        {
            //synthesizer.ProcessMidiMessage()
            if(Random.Range(0.0f,1.0f)>0.5f)
              synthesizer.NoteOn(1, 60, 100);
            else
              synthesizer.NoteOn(1, 50, 100);


            changeProgram();
            dt = 0;
        }
    }
    public void LoadBank(PatchBank bank)
    {
        this.bank = bank;

        synthesizer.UnloadBank();
        synthesizer.LoadBank(bank);
        Debug.LogFormat("Loaded bank: {0}",bank.GetBank(0).Length);//[0].Name
       // for(int i=0;i< bank.GetBank(0).Length;i++)
        //{
         //   Debug.LogFormat("{0}: {1}",i, bank.GetPatch(0,i).Name);
       // }
    }

    void changeProgram()
    {
        synthesizer.ProcessMidiMessage(1, 0xC0, 2, 0);
    }
    void OnAudioFilterRead(float[] data, int channel)
    {
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
