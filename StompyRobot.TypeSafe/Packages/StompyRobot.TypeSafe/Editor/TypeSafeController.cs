using System;
using System.Diagnostics;
using TypeSafe.Editor.Unity;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor
{
    internal enum States
    {
        Idle,
        Scanning,
        Compiling,
        ScanQueued,
        Waiting
    }

    internal class TypeSafeController : ScriptableObject
    {
        private static TypeSafeController _instance;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private bool _abortWait;
        private CompileController _compileController;
        private ScanController _scanController;
        private ScanResult _scanResult;
        [SerializeField] private States _state = States.Idle;
        [SerializeField] private bool _userInitiated;

        public static TypeSafeController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreateInstance<TypeSafeController>();
                    _instance.hideFlags = HideFlags.HideAndDontSave;
                }

                return _instance;
            }
        }

        /// <summary>
        /// Number of items in the current operation completed
        /// </summary>
        public int ItemsCompleted { get; private set; }

        /// <summary>
        /// Total number of items to process in the current operation
        /// </summary>
        public int TotalItems { get; private set; }

        /// <summary>
        /// Amount of time that has passed during the current build.
        /// </summary>
        public double BuildTimeElapsed
        {
            get { return _stopwatch.Elapsed.TotalSeconds; }
        }

        public States State
        {
            get { return _state; }
        }

        /// <summary>
        /// Cancel any current operations and queue a new scan from scratch
        /// </summary>
        public void Refresh(bool userInitiated = false)
        {
            Cancel();
            Queue(userInitiated);
        }

        /// <summary>
        /// Queue a scan.
        /// </summary>
        public void Queue(bool userInitiated = false)
        {
            if (_state != States.Idle)
            {
                TSLog.LogWarning(LogCategory.Trace, "Cannot queue scan - state is not idle");
                return;
            }

            TSLog.Log(LogCategory.Trace, string.Format("Scan Queued (userInitiated={0})", userInitiated));

            _userInitiated = userInitiated;
            _state = States.ScanQueued;
        }

        /// <summary>
        /// Cancel any current operations
        /// </summary>
        public void Cancel()
        {
            switch (_state)
            {
                case States.ScanQueued:
                {
                    _state = States.Idle;
                    break;
                }

                case States.Scanning:
                {
                    AbortScan();
                    break;
                }

                case States.Compiling:
                {
                    AbortCompile();
                    break;
                }

                case States.Waiting:
                {
                    _compileController = null;
                    _state = States.Idle;

                    break;
                }
            }
        }

        public void AbortWait()
        {
            if (State == States.Waiting)
            {
                _abortWait = true;
                Step();
            }
        }

        private void OnEnable()
        {
            TSLog.Log(LogCategory.Trace,
	            string.Format("TypeSafeController.OnEnable (Version={0}, ", Strings.Version) +
	            string.Format("UnityVersion={0}, ", Application.unityVersion) +
	            string.Format("TypeSafePath={0}, ", PathUtility.GetTypeSafePath()) +
	            string.Format("TypeSafeEditorPath={0})", PathUtility.GetTypeSafeEditorPath()));

            _instance = this;

            TypeSafeUtil.EnsureCorrectUnityVersion();

            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            TSLog.Log(LogCategory.Trace, "TypeSafeController.OnDisable");

            TypeSafeUtil.ReportCodeRefreshStarted();

            if (State != States.Idle)
            {
                Cancel();
                Queue();
            }

            TSLog.CloseLog();
        }

        private void OnEditorUpdate()
        {
            TypeSafeUtil.ReportCodeRefreshCompleted();

            Settings settings = Settings.Instance;
            if (settings == null)
            {
	            return;
            }

            if (!WelcomeWindow.HasOpenedBefore() || !settings.HasShownWelcome)
            {
                TSLog.Log(LogCategory.Trace,
	                string.Format("Opening welcome window (HasOpenedBefore={0}, HasShownWelcome={1})", WelcomeWindow.HasOpenedBefore(), settings.HasShownWelcome));
                WelcomeWindow.Open();
            }
            else if (WelcomeWindow.GetPreviousOpenedVersion() != Strings.Version)
            {
                TSLog.Log(LogCategory.Trace,
	                string.Format("Opening changelog window (previousVersion={0}, currentVersion={1})", WelcomeWindow.GetPreviousOpenedVersion(), Strings.Version));
                WelcomeWindow.Open(true);

                TSLog.Log(LogCategory.Trace, "Clearing AssetTypeCache (new TypeSafe version detected)");
                AssetTypeCache.ClearCache();
            }

            Step();
        }

        private void Step()
        {
            switch (State)
            {
                case States.Idle:
                    break;

                case States.ScanQueued:

                    if (!TypeSafeUtil.ShouldBeOperating())
                    {
                        break;
                    }

                    BeginScan();

                    break;

                case States.Scanning:

                    if (_scanController == null)
                    {
                        TSLog.LogError(LogCategory.Trace, "ScanController = null, but State = Scanning");
                        _state = States.Idle;
                        break;
                    }

                    if (!TypeSafeUtil.ShouldBeOperating())
                    {
                        TSLog.Log(LogCategory.Trace, "Aborting scan due to script reload in progress.");

                        Cancel();
                        Queue();
                        break;
                    }

                    _scanController.Update();

                    ItemsCompleted = _scanController.ItemsCompleted;
                    TotalItems = _scanController.TotalItems;

                    if (_scanController.IsDone)
                    {
                        TSLog.Log(LogCategory.Trace, string.Format("Scan complete (took {0}s).", _stopwatch.Elapsed.TotalSeconds));

                        if (_scanController.WasSuccessful)
                        {
                            _scanResult = _scanController.Result;
                            _scanController = null;
                            BeginCompile();
                        }
                        else
                        {
                            TSLog.LogError(LogCategory.Info, "Error occured while scanning. Aborting process.");
                            _state = States.Idle;
                        }
                    }

                    break;

                case States.Compiling:
                case States.Waiting:

                    if (_compileController == null)
                    {
                        TSLog.LogError(LogCategory.Trace, "CompileController = null, but State = Compiling");
                        _state = States.Idle;
                        break;
                    }

                    if (!TypeSafeUtil.ShouldBeOperating())
                    {
                        TSLog.Log(LogCategory.Trace, "Aborting compile.");
                        Cancel();
                        Queue();
                        break;
                    }

                    if (_compileController.IsDone)
                    {
                        if (_state != States.Waiting)
                        {
                            // Perform a dry run of the deploy step to see if there were any changes since the last compile
                            int changeCount;
                            TypeSafeUtil.DeployBuildArtifacts(_compileController.Output, out changeCount, true);

                            // Delay for minimum build time if not user initiated and there were changes
                            if (Settings.Instance.EnableWaiting && changeCount > 0 && !_userInitiated &&
                                _stopwatch.Elapsed.TotalSeconds < Settings.Instance.MinimumBuildTime)
                            {
                                _state = States.Waiting;
                                break;
                            }
                        }
                        else
                        {
                            // Wait for wait stage to elapse
                            if (!_abortWait && _stopwatch.Elapsed.TotalSeconds < Settings.Instance.MinimumBuildTime)
                            {
                                break;
                            }
                        }

                        _abortWait = false;

                        TSLog.Log(LogCategory.Trace,
	                        string.Format("Compile Complete (WasSuccessful={0})", _compileController.WasSuccessful));

                        if (_compileController.WasSuccessful)
                        {
                            int updatedFileCount;
                            var deployResult = TypeSafeUtil.DeployBuildArtifacts(_compileController.Output,
                                out updatedFileCount);

                            TSLog.Log(LogCategory.Trace,
	                            string.Format("Deploy Complete (WasSuccessful={0}, updatedFileCount={1})", deployResult, updatedFileCount));

                            var shouldReport = _userInitiated || updatedFileCount > 0;

                            TSLog.EndBuffer(LogCategory.Compile, shouldReport);

                            if (!deployResult)
                            {
                                TSLog.LogError(LogCategory.Info, "Compile failed.");
                            }
                            else if (shouldReport)
                            {
                                if (updatedFileCount == 0)
                                {
                                    TSLog.Log(LogCategory.Info, "Compile complete, no changes.");
                                }
                                else
                                {
                                    TSLog.Log(LogCategory.Info,
	                                    string.Format("Compile completed. (Took {0}s)", _stopwatch.Elapsed.Seconds));
                                }
                            }
                        }

                        _compileController = null;
                        _state = States.Idle;
                    }

                    break;
            }
        }

        private void BeginScan()
        {
            if (_state != States.Idle && _state != States.ScanQueued)
            {
                throw new InvalidOperationException();
            }

            TSLog.BeginBuffer(LogCategory.Compile);
            TSLog.Log(LogCategory.Trace, "BeginScan");
            TypeSafeUtil.CheckForRemovedAssets();

            _stopwatch.Reset();
            _stopwatch.Start();

            _scanController = new ScanController();
            _scanController.Begin();

            _state = States.Scanning;
        }

        private void AbortScan()
        {
            if (_state != States.Scanning)
            {
                throw new InvalidOperationException();
            }

            TSLog.Log(LogCategory.Trace, "Aborting Scan");

            _scanController = null;

            _state = States.Idle;
            TSLog.EndBuffer(LogCategory.Compile, false);
        }

        private void BeginCompile()
        {
            if (_scanResult == null)
            {
                throw new InvalidOperationException();
            }

            TSLog.Log(LogCategory.Trace, "BeginCompile");

            _state = States.Compiling;

            _compileController = new CompileController();
            _compileController.ResourceDatabase = _scanResult.ResourceDatabase;
            _compileController.DataUnits = _scanResult.DataUnits;

            _compileController.Compile();
        }

        private void AbortCompile()
        {
            if (_state != States.Compiling)
            {
                throw new InvalidOperationException();
            }

            if (_compileController != null)
            {
                _compileController.Abort();
                _compileController = null;
            }

            _state = States.Idle;
            TSLog.EndBuffer(LogCategory.Compile, false);
        }
    }
}
