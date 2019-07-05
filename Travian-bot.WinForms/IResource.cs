using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefSharp.MinimalExample.WinForms
{
    public interface IResource
    {
        int Index { get; }
        int ResLevel { get; }
    }
}
