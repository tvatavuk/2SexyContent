﻿using System.Collections.Generic;
using System.Web;
using DotNetNuke.Services.Tokens;
using ToSic.SexyContent.DataSources.Tokens;

namespace ToSic.SexyContent.Engines.TokenEngine
{
    public class TokenReplace : DotNetNuke.Services.Tokens.TokenReplace
    {
        public TokenReplace(DynamicEntity Content, DynamicEntity Presentation, DynamicEntity ListContent, DynamicEntity ListPresentation, Dictionary<string,string> List, App app) : base()
        {
            if (HttpContext.Current != null)
            {
                HttpRequest request = HttpContext.Current.Request;
                PropertySource.Add("querystring", new FilteredNameValueCollectionPropertyAccess(request.QueryString));
                PropertySource.Add("form", new FilteredNameValueCollectionPropertyAccess(request.Form));
                PropertySource.Add("server", new FilteredNameValueCollectionPropertyAccess(request.ServerVariables));

                // Add our expando, requires that we convert it to a string/string table
                if (Content != null)
                {
                    /*Dictionary<string, object> ContentDictionary = new Dictionary<string, object>(Content.Dictionary);*/
                    PropertySource.Add("content", new DynamicEntityPropertyAccess("content", Content));
                }
                if(Presentation != null)
                {
                    /*Dictionary<string, object> PresentationDictionary = new Dictionary<string, object>(Presentation.Dictionary);*/
                    PropertySource.Add("presentation", new DynamicEntityPropertyAccess("presentation", Presentation));
                }
                if (ListContent != null)
                {
                    /*Dictionary<string, object> ContentDictionary = new Dictionary<string, object>(ListContent.Dictionary);*/
                    PropertySource.Add("listcontent", new DynamicEntityPropertyAccess("listcontent", ListContent));
                }
                if (ListPresentation != null)
                {
                    /*Dictionary<string, object> PresentationDictionary = new Dictionary<string, object>(ListPresentation.Dictionary);*/
                    PropertySource.Add("listpresentation", new DynamicEntityPropertyAccess("listpresentation", ListPresentation));
                }
                
                PropertySource.Add("app", new AppPropertyAccess("app", app));
                if(app.Settings != null)
                    PropertySource.Add("appsettings", new DynamicEntityPropertyAccess("appsettings", app.Settings));
                if (app.Resources != null)
                    PropertySource.Add("appresources", new DynamicEntityPropertyAccess("appresources", app.Resources));
                PropertySource.Add("list", new DictionaryPropertyAccess(List));
            }
        }

        protected override string replacedTokenValue(string strObjectName, string strPropertyName, string strFormat)
        {
            return base.replacedTokenValue(strObjectName, strPropertyName, strFormat);
        }
    }
}