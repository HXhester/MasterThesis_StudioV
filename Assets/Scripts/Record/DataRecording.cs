using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using MicroLibrary;
using UnityEngine;

public class DataRecording : ThreadedJob
{

    public Vector3 Tobii_EyeDirection;
    public float InterPersonalDistance;
    

    public int FrameCount;
    public int ThreadSleepTime;
    public System.DateTime StartRecordingTime;

    public MicroTimer MicroTimer;

    private Thread _recordingThread;
    

    protected override void ThreadFunction()
    {
        StartRecordingTime = System.DateTime.Now;

        MicroTimer = new MicroTimer();
        MicroTimer.MicroTimerElapsed += OnTimedEvent;

        MicroTimer.Interval = ThreadSleepTime * 1000; // Call micro timer every 1000us

        // Can choose to ignore event if late by Xµs (by default will try to catch up)
        MicroTimer.IgnoreEventIfLateBy = 50; // 50µs (0.05ms)

        MicroTimer.Enabled = true; // Start timer

    }

    void OnTimedEvent(object sender, MicroTimerEventArgs timerEventArgs)
    {

        FrameCount = timerEventArgs.TimerCount;

    }
}
