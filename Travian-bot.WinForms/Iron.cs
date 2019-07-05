namespace CefSharp.MinimalExample.WinForms
{
    public class Iron : IResource
    {
        public int Index { get; }
        public int ResLevel { get; }
        public Iron(int index, int resLevel)
        {
            Index = index;
            ResLevel = resLevel;
        }
    }
}