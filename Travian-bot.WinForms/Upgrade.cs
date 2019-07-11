using CefSharp.WinForms;

namespace CefSharp.MinimalExample.WinForms
{
    public class Upgrade
    {
        private readonly ChromiumWebBrowser _browser;
        private readonly string _server;

        public Upgrade(ChromiumWebBrowser browser, string server)
        {
            _browser = browser;
            _server = server;
        }

        public void UpgradeResource()
        {
            if (_browser.Address.Contains("https://" + _server + ".ttwars.com/dorf1.php"))
            {
                _browser.ExecuteScriptAsyncWhenPageLoaded("var element = document.getElementById(\"rx\").children;" +
                                                         "\r\nelement[3].click();");
            }
            else if (_browser.Address.Contains("https://" + _server + ".ttwars.com/build.php?id="))
            {
                _browser.ExecuteScriptAsyncWhenPageLoaded(
                    @"function getElementsByText(text) {
                    function rec(ele, arr)
                    {
                        if (ele.childNodes.length > 0) 
                            for (var i = 0; i < ele.childNodes.length; i++) 
                                rec(ele.childNodes[i], arr);
                        else if (ele.nodeType == Node.TEXT_NODE && 
                            ele.nodeValue.indexOf(text) != -1) 
                            arr.push(ele.parentNode);
                        return arr;
                    }
                    return rec(document.body, []);
                }"
                    + "\r\nvar item = getElementsByText('Upgrade to level ');"
                    + "\r\nitem[0].click();");
            }
        }
    }
}
