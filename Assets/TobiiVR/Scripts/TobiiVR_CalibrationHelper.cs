// Copyright © 2015 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.VR
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using Tobii.StreamEngine;
    using Vector3 = UnityEngine.Vector3;

    public class TobiiVR_CalibrationHelper
    {
        #region Member variables

        public volatile bool Running;
        private readonly object _lock = new object();
        private Thread _thread;
        private IntPtr _context;
        private int _commandNumber;

        public enum CommandT
        {
            None,
            StartCalibration,
            ClearCalibration,
            CollectCalibrationDataForPoint,
            DiscardCalibrationDataForPoint,
            ComputeAndApplyCalibration,
            StopCalibration,
        }

        public class Command
        {
            public CommandT CommandType { get; private set; }

            public Vector3 Point { get; private set; }

            public tobii_error_t Result { get; set; }

            public long ExecutionMilliseconds { get; private set; }

            public int CommandNumber { get; private set; }

            private readonly Stopwatch _stopwatch;

            public Command(CommandT state, Vector3 point, int commandNumber)
            {
                CommandType = state;
                Point = point;
                CommandNumber = commandNumber;
                if (commandNumber >= 0)
                {
                    _stopwatch = new Stopwatch();
                }
            }

            public Command(CommandT state, int commandNumber) : this(state, Vector3.zero, commandNumber)
            {
            }

            public void TimeStart()
            {
                if (_stopwatch == null)
                {
                    return;
                }

                _stopwatch.Reset();
                _stopwatch.Start();
            }

            public void TimeStop()
            {
                if (_stopwatch == null)
                {
                    return;
                }

                _stopwatch.Stop();
                ExecutionMilliseconds = _stopwatch.ElapsedMilliseconds;
            }

            public override string ToString()
            {
                return string.Format("Command: {0}, Point: {1}, ApiResult: {2}", CommandType, Point, Result);
            }

            public string GetCommandResultAndTimeString()
            {
                return string.Format("Command: {0}, ApiResult: {1}, Duration: {2}ms", CommandType, Result, ExecutionMilliseconds);
            }
        }

        private readonly Queue<Command> _commands = new Queue<Command>();
        private readonly Queue<Command> _results = new Queue<Command>();

        #endregion Member variables

        #region Locked access

        private Command NextCommand
        {
            get
            {
                lock (_lock)
                {
                    return _commands.Count > 0 ? _commands.Dequeue() : new Command(CommandT.None, -1);
                }
            }

            set
            {
                lock (_lock)
                {
                    _commands.Enqueue(value);
                }
            }
        }

        public Command NextResult
        {
            get
            {
                lock (_lock)
                {
                    return _results.Count > 0 ? _results.Dequeue() : null;
                }
            }

            set
            {
                lock (_lock)
                {
                    _results.Enqueue(value);
                }
            }
        }

        private int CommandCount
        {
            get
            {
                lock (_lock)
                {
                    return _commands.Count;
                }
            }
        }

        public System.IntPtr Context
        {
            get
            {
                lock (_lock)
                {
                    return _context;
                }
            }

            private set
            {
                lock (_lock)
                {
                    _context = value;
                }
            }
        }

        #endregion Locked access

        #region Construction

        public TobiiVR_CalibrationHelper(System.IntPtr context)
        {
            Context = context;
            Running = true;
            Start();
        }

        #endregion Construction

        #region Commands

        public void StartCalibration()
        {
            NextCommand = new Command(CommandT.StartCalibration, _commandNumber++);
        }

        public void StopCalibration()
        {
            NextCommand = new Command(CommandT.StopCalibration, _commandNumber++);
        }

        public void ClearCalibration()
        {
            NextCommand = new Command(CommandT.ClearCalibration, _commandNumber++);
        }

        public void ComputeAndApplyCalibration()
        {
            NextCommand = new Command(CommandT.ComputeAndApplyCalibration, _commandNumber++);
        }

        public void CollectCalibrationDataForPoint(Vector3 point)
        {
            NextCommand = new Command(CommandT.CollectCalibrationDataForPoint, point, _commandNumber++);
        }

        public void DiscardCalibrationDataForPoint(Vector3 point)
        {
            NextCommand = new Command(CommandT.DiscardCalibrationDataForPoint, point, _commandNumber++);
        }

        #endregion Commands

        #region Thread handling

        /// <summary>
        /// Start. Call from constructor.
        /// </summary>
        private void Start()
        {
            if (_thread != null)
            {
                TobiiVR_Logging.LogError("Thread already started. Returning.");
                return;
            }

            _thread = new Thread(ThreadFunction)
            {
                IsBackground = true,
                Name = "Calibration Thread"
            };
            _thread.Start();
        }

        /// <summary>
        /// Join. Call from outher thread.
        /// </summary>
        /// <param name="millisecondsTimeout">Timeout for join</param>
        /// <param name="statePropagationRetries100Ms">Number of 100 ms iterations to wait for command queue to empty</param>
        /// <returns>True if successful join or no thread</returns>
        public bool KillJoin(int millisecondsTimeout, int statePropagationRetries100Ms)
        {
            if (_thread == null)
            {
                TobiiVR_Logging.LogError("No thread to join. Returning.");
                return true;
            }

            for (int i = 0; i < statePropagationRetries100Ms; i++)
            {
                if (CommandCount == 0)
                {
                    break;
                }

                Thread.Sleep(100);
            }

            Running = false;

            return _thread.Join(millisecondsTimeout);
        }

        public void ThreadFunction()
        {
            while (Running)
            {
                var command = NextCommand;
                command.TimeStart();

                switch (command.CommandType)
                {
                    case CommandT.None:
                        Thread.Sleep(100);
                        break;

                    case CommandT.StartCalibration:
                        command.TimeStart();
                        command.Result = Interop.tobii_calibration_start(Context);
                        command.TimeStop();
                        NextResult = command;
                        break;

                    case CommandT.ClearCalibration:
                        command.TimeStart();
                        command.Result = Interop.tobii_calibration_clear(Context);
                        command.TimeStop();
                        NextResult = command;
                        break;

                    case CommandT.CollectCalibrationDataForPoint:
                        var pointToAdd = command.Point;
                        command.TimeStart();
                        command.Result = Interop.tobii_calibration_collect_data_3d(Context, pointToAdd.x, pointToAdd.y, pointToAdd.z);
                        command.TimeStop();
                        NextResult = command;
                        break;

                    case CommandT.DiscardCalibrationDataForPoint:
                        var pointToRemove = command.Point;
                        command.TimeStart();
                        command.Result = Interop.tobii_calibration_discard_data_3d(Context, pointToRemove.x, pointToRemove.y, pointToRemove.z);
                        command.TimeStop();
                        NextResult = command;
                        break;

                    case CommandT.ComputeAndApplyCalibration:
                        command.TimeStart();
                        command.Result = Interop.tobii_calibration_compute_and_apply(Context);
                        command.TimeStop();
                        NextResult = command;
                        break;

                    case CommandT.StopCalibration:
                        command.TimeStart();
                        command.Result = Interop.tobii_calibration_stop(Context);
                        command.TimeStop();
                        NextResult = command;
                        Running = false;
                        break;

                    default:
                        break;
                }

                command.TimeStop();
            }
        }

        #endregion Thread handling
    }
}