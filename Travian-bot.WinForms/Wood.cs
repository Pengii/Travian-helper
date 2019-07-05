namespace CefSharp.MinimalExample.WinForms
{
    public class Wood : IResource
    {
        public int Index { get; }
        public int ResLevel { get; }

        public Wood(int index, int resLevel)
        {
            this.Index = index;
            this.ResLevel = resLevel;
        }
    }
}