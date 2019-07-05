namespace CefSharp.MinimalExample.WinForms
{
    public class Clay : IResource
    {
        public int Index { get; }
        public int ResLevel { get; }
        public Clay(int index, int resLevel)
        {
            Index = index;
            ResLevel = resLevel;
        }
    }
}