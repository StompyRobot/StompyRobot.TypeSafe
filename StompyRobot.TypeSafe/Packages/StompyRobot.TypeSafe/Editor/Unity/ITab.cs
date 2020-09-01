namespace TypeSafe.Editor.Unity
{
    internal interface ITab
    {
        bool IsEnabled { get; }
        string TabName { get; }
        bool CanExit { get; }
        void OnEnter();
        void OnExit();
        void OnGUI();
    }
}
