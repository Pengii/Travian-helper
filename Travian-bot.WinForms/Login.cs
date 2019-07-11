using CefSharp.WinForms;

namespace CefSharp.MinimalExample.WinForms
{
    class Login
    {
        private readonly ChromiumWebBrowser _browser;

        public Login(ChromiumWebBrowser browser)
        {
            this._browser = browser;
        }
               
        public void LoginUser()
        {
            _browser.ExecuteScriptAsync("document.forms[\"login\"].elements[\"user\"].value=\"Saisama\";");
            _browser.ExecuteScriptAsync("document.forms[\"login\"].elements[\"pw\"].value=\"a72b84zx\";");
            _browser.ExecuteScriptAsync("document.forms[\"login\"].submit();");
        }
    }
}
