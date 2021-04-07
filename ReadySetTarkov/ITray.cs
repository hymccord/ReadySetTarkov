namespace ReadySetTarkov
{
    internal interface ITray
    {
        bool Visible { get; set; }
        void SetIcon(string resource);
        void SetStatus(string text);
    }
}