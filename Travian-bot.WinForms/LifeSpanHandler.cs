using System;

namespace CefSharp.MinimalExample.WinForms
{
    public class LifespanHandler : ILifeSpanHandler
    {
        //event that receive url popup
        public event Action<string> PopupRequest;

        bool ILifeSpanHandler.OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            //get url popup
            PopupRequest?.Invoke("https://tr1.seafight.com/index.es?action=internalMap");

            //stop open popup window
            newBrowser = null;
            return true;
        }
        bool ILifeSpanHandler.DoClose(IWebBrowser browserControl, IBrowser browser)
        { return false; }

        void ILifeSpanHandler.OnBeforeClose(IWebBrowser browserControl, IBrowser browser) { }

        void ILifeSpanHandler.OnAfterCreated(IWebBrowser browserControl, IBrowser browser) { }
    }
}