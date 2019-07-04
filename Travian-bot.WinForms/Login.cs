using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace CefSharp.MinimalExample.WinForms
{
    class Login
    {
        private readonly IWebBrowser _browser;

        public Login(IWebBrowser browser)
        {
            this._browser = browser;
        }
               
        public void LoginUser()
        {
            _browser.ExecuteScriptAsyncWhenPageLoaded("document.forms[\"login\"].elements[\"user\"].value=\"Saisama\";");
            Thread.Sleep(2000);
            _browser.ExecuteScriptAsyncWhenPageLoaded("document.forms[\"login\"].elements[\"pw\"].value=\"a72b84zx\";");
            Thread.Sleep(2000);
            _browser.ExecuteScriptAsyncWhenPageLoaded("document.forms[\"login\"].submit();");
        }
    }
}
