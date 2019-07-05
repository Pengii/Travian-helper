namespace CefSharp.MinimalExample.WinForms
{
    public class Crop : IResource
    {
        public int Index { get; }
        public int ResLevel { get; }

        public Crop(int index, int resLevel)
        {
            Index = index;
            ResLevel = resLevel;
        }


    }
}