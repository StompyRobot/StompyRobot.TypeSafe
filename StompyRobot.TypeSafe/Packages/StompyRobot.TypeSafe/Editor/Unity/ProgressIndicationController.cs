using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity
{
    /// <summary>
    /// Handles displaying TypeSafe state to the user
    /// </summary>
    [InitializeOnLoad]
    internal static class ProgressIndicationController
    {
        private static States _previousState = States.Idle;

        static ProgressIndicationController()
        {
            if (!TypeSafeUtil.IsEnabled())
            {
                return;
            }

            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            var controller = TypeSafeController.Instance;

            switch (controller.State)
            {
                case States.Idle:
                {
                    if (_previousState != States.Idle)
                    {
                        SRProgressBar.Clear();
                    }

                    break;
                }

                case States.ScanQueued:
                {
                    SRProgressBar.Display("TypeSafe", "Scan Queued...", 0);
                    break;
                }

                case States.Scanning:
                {
                    var progress = (float) controller.ItemsCompleted/controller.TotalItems;

                    if (controller.TotalItems == 0)
                    {
                        progress = 0f;
                    }

                    SRProgressBar.Display("TypeSafe",
	                    string.Format("Scanning ({0}/{1})", controller.ItemsCompleted + 1, controller.TotalItems), progress);

                    break;
                }

                case States.Compiling:
                {
                    SRProgressBar.Display("TypeSafe", "Compiling...", 1f);
                    break;
                }

                case States.Waiting:
                {
                    var timeLeft = Settings.Instance.MinimumBuildTime - controller.BuildTimeElapsed;

                    SRProgressBar.Display("TypeSafe", string.Format("Waiting... ({0:#0.0}s)", timeLeft),
                        Mathf.Clamp01((float) controller.BuildTimeElapsed/Settings.Instance.MinimumBuildTime));

                    break;
                }
            }

            _previousState = controller.State;
        }
    }
}
