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
            _browser.ExecuteScriptAsyncWhenPageLoaded("document.forms[\"login\"].elements[\"pw\"].value=\"a72b84zx\";");
            _browser.ExecuteScriptAsyncWhenPageLoaded("document.forms[\"login\"].submit();");
        }
    }
}
