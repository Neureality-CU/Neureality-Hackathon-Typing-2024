using LSL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSLInletInterface : MonoBehaviour
{

    // Start is called before the first frame update

    public string streamName;
    public StreamInlet streamInlet;
    public float[] frameDataBuffer;
    public double frameTimestamp = 0;



    public float[,] chunkDataBuffer = new float[1000, 8];
    public double[] chunkTimestampsBuffer = new double[1000];
    public int chunkSampleNumber = 0;

    public float[,] trashDataBuffer = new float[1000, 8];
    public double[] trashTimestampsBuffer = new double[1000];



    public bool streamActivated = false;
    public bool streamAvailable = false;


    ContinuousResolver continuousResolver;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }



    protected void initFrameBuffer()
    {
        int channelCount = streamInlet.info().channel_count();
        frameDataBuffer = new float[channelCount];
        //timestampBuffer = new double[streamInlet.info().channel_count()];

        chunkDataBuffer = new float[1000, channelCount];
        chunkTimestampsBuffer = new double[1000];


        trashDataBuffer = new float[1000, channelCount];
        trashTimestampsBuffer = new double[1000];

    }




    protected void StartContinousResolver()
    {
        if (!streamName.Equals(""))
            continuousResolver = new ContinuousResolver("name", streamName);
        else
        {
            Debug.LogError("Object must specify a name for resolver to lookup a stream.");
            this.enabled = false;
            return;
        }
        StartCoroutine(ResolveLSLInlet());
    }



    //protected IEnumerator ResolveLSLInletArchive()
    //{

    //    var results = continuousResolver.results();
    //    while (results.Length == 0)
    //    {
    //        yield return new WaitForSeconds(.1f);
    //        results = continuousResolver.results();
    //    }

    //    streamInlet = new StreamInlet(results[0]);
    //    initFrameBuffer();
    //    clearBuffer();

    //    activated = true;
    //}


    protected IEnumerator ResolveLSLInlet()
    {

        var results = continuousResolver.results();
        while (true)
        {
            results = continuousResolver.results();
            if (results.Length > 0)
            {
                // if stream is available on the network
                streamAvailable = true;
                if (streamActivated == false)
                {
                    // setup the stream if not activated
                    streamInlet = new StreamInlet(results[0]);
                    initFrameBuffer();
                    clearBuffer();
                    streamActivated = true;
                    Debug.Log("Stream Activated");
                }
            }
            else
            {
                // if stream is not available on the network
                streamAvailable = false;
                if (streamActivated == true)
                {
                    // remove stream and set stream activated to false
                    streamInlet = null;
                    streamActivated = false;
                    Debug.Log("Stream DeActivated");

                }
            }
            yield return new WaitForSeconds(.1f);
        }

    }



    protected void startStream()
    {
        if (streamInlet != null)
        {
            streamInlet.open_stream();
        }
    }

    protected void stopStream()
    {
        if (streamInlet != null)
        {
            streamInlet.close_stream();
        }
    }

    protected void pullSample()
    {
        // please note that the data pull_sample function will not change the frameDataBuffer if there is no new data!
        frameTimestamp = streamInlet.pull_sample(frameDataBuffer, 0);
    }


    protected void pullChunk()
    {

        chunkSampleNumber = streamInlet.pull_chunk(chunkDataBuffer, chunkTimestampsBuffer);
    }


    public void clearBuffer()
    {
        while (streamInlet.samples_available() > 0)
        {
            streamInlet.pull_chunk(trashDataBuffer, trashTimestampsBuffer);
        }
        //Debug.Log("Buffer Cleared");
    }

}