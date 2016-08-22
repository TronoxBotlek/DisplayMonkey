/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.UI;
//using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using Microsoft.Exchange.WebServices.Data;
using System.Net;
using DisplayMonkey.Language;

namespace DisplayMonkey
{
    public partial class getPowerbiToken : IHttpHandler
    {
        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

            int frameId = DataAccess.IntOrZero(request.QueryString["frame"]);
            int panelId = DataAccess.IntOrZero(request.QueryString["panel"]);
            int displayId = DataAccess.IntOrZero(request.QueryString["display"]);
            string culture = DataAccess.StringOrBlank(request.QueryString["culture"]);

			string json = "";
				
			try
			{
                // set culture
                Powerbi powerbi = new Powerbi(frameId);
                Location location = new Location(displayId);

                if (string.IsNullOrWhiteSpace(culture))
                    culture = location.Culture;
                
                if (!string.IsNullOrWhiteSpace(culture))
                {
                    System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo(culture);
                    System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
                }

                JavaScriptSerializer jss = new JavaScriptSerializer();
                json = jss.Serialize(new
                {
                    accessToken = powerbi.AccessToken,
                });
			}

			catch (Exception ex)
			{
                JavaScriptSerializer s = new JavaScriptSerializer();
                json = s.Serialize(new
                {
                    Error = ex.Message,
                    //Stack = ex.StackTrace,
                    Data = new
                    {
                        FrameId = frameId,
                        PanelId = panelId,
                        DisplayId = displayId,
                        Culture = culture
                    },
                });
			}

            response.Clear();
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetSlidingExpiration(true);
            response.Cache.SetNoStore();
            response.ContentType = "application/json";
			response.Write(json);
            response.Flush();
        }
    }
}