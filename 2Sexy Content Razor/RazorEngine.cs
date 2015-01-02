﻿using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Compilation;
using System.Web.WebPages;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;
using ToSic.SexyContent.DataSources;
using ToSic.SexyContent.Razor.Helpers;
using ToSic.SexyContent.Razor;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.SexyContent.Search;

namespace ToSic.SexyContent.Engines
{
	public class RazorEngine : EngineBase
	{
		//protected string RazorScriptFile { get; set; }
		protected SexyContentWebPage Webpage { get; set; }
		protected dynamic Content { get; set; }
		protected dynamic Presentation { get; set; }
		protected dynamic ListContent { get; set; }
		protected dynamic ListPresentation { get; set; }
		protected List<Element> List { get; set; }

		private IEnumerable<DynamicEntity> GetEntitiesFromStream(IDictionary<string, IDataStream> streams, string streamName, string[] dimensions)
		{
			return streams.ContainsKey(streamName)
				? streams[streamName].List.Select(e => new DynamicEntity(e.Value, dimensions, Sexy))
				: new DynamicEntity[0];
		}

		protected override void Init()
		{
			// ToDo: Get rid of ModuleDataSource, use In-Streams instead
			var dimensionIds = new[] { System.Threading.Thread.CurrentThread.CurrentCulture.Name };
			var inStreams = ((IDataTarget)DataSource).In;
			//var moduleDataSource = DataPipelineFactory.FindDataSource<ModuleDataSource>((IDataTarget)DataSource);

			//var elements = moduleDataSource.ContentElements.Where(p => p.Content != null).ToList();
			var elements = GetEntitiesFromStream(inStreams, "Default", dimensionIds);
			//var listElement = moduleDataSource.ListElement;
			var listElement = GetEntitiesFromStream(inStreams, "ListContent", dimensionIds).FirstOrDefault();
			List = elements.Select(e => new Element { Content = e, EntityId = e.EntityId }).ToList();
			var presentation = GetEntitiesFromStream(inStreams, "Presentation", dimensionIds).FirstOrDefault();
			var listPresentation = GetEntitiesFromStream(inStreams, "ListPresentation", dimensionIds).FirstOrDefault();

			if (elements.Any())
			{
				Content = elements.First();
				Presentation = presentation;// elements.First().Presentation;
			}
			if (listElement != null)
			{
				ListContent = listElement;
				ListPresentation = listPresentation; //listElement.Presentation;
			}

			try
			{
				InitWebpage();
			}
			// Catch web.config Error on DNNs upgraded to 7
			catch (System.Configuration.ConfigurationErrorsException Exc)
			{
				var e = new Exception("Configuration Error: Please follow this checklist to solve the problem: http://swisschecklist.com/en/i4k4hhqo/2Sexy-Content-Solve-configuration-error-after-upgrading-to-DotNetNuke-7", Exc);
				throw e;
			}
		}



		protected HttpContextBase HttpContext
		{
			get
			{
				if (System.Web.HttpContext.Current == null)
					return null;
				return new HttpContextWrapper(System.Web.HttpContext.Current);
			}
		}

		public Type RequestedModelType()
		{
			if (Webpage != null)
			{
				var webpageType = Webpage.GetType();
				if (webpageType.BaseType.IsGenericType)
				{
					return webpageType.BaseType.GetGenericArguments()[0];
				}
			}
			return null;
		}

		// <2sic removed: Render<T>(TextWriter writer, T model)>
		// </2sic>

		public void Render(TextWriter writer)
		{
			Webpage.ExecutePageHierarchy(new WebPageContext(HttpContext, Webpage, null), writer, Webpage);
		}

		/// <summary>
		/// Renders the template
		/// </summary>
		/// <returns></returns>
		protected override string RenderTemplate()
		{
			var writer = new StringWriter();
			Render(writer);
			return writer.ToString();
		}

		private object CreateWebPageInstance()
		{
			var compiledType = BuildManager.GetCompiledType(TemplatePath);
			object objectValue = null;
			if (compiledType != null)
			{
				objectValue = RuntimeHelpers.GetObjectValue(Activator.CreateInstance(compiledType));
			}
			return objectValue;
		}

		private void InitHelpers(SexyContentWebPage webPage)
		{
			webPage.Html = new HtmlHelper();
			webPage.Url = new UrlHelper(ModuleInfo);

			webPage.Content = Content;
			webPage.Presentation = Presentation;
			webPage.ListContent = ListContent;
			webPage.ListPresentation = ListPresentation;
			webPage.List = List;
			webPage.Sexy = Sexy;
			webPage.AppAndDataHelpers = new AppAndDataHelpers(Sexy, ModuleInfo, (ViewDataSource)DataSource, App);

			// 2014-11-18 2dm...
			// Init data, show drafts if in edit mode only...
			// webPage.App.InitData(this.Webpage.InstancePurpose != InstancePurposes.IndexingForSearch && SexyContent.HasEditPermission(webPage.Dnn.Module));

			//// ToDo: Remove this as soon as App.Data getter on App class is fixed #1 and #2
			//if (webPage.App.Data == null)
			//{
			//    // ModulePermissionController does not work when indexing, return false for search
			//    var initialSource = SexyContent.GetInitialDataSource(App.ZoneId, App.AppId, this.Webpage.InstancePurpose != InstancePurposes.IndexingForSearch && SexyContent.HasEditPermission(webPage.Dnn.Module));

			//    //2014-11-18 2dm - using extended App...
			//    //App.Data = ToSic.Eav.DataSource.GetDataSource<ToSic.Eav.DataSources.App>(initialSource.ZoneId, initialSource.AppId, initialSource, initialSource.ConfigurationProvider);
			//    App.Data = ToSic.Eav.DataSource.GetDataSource<ToSic.SexyContent.DataSources.App>(initialSource.ZoneId, initialSource.AppId, initialSource, initialSource.ConfigurationProvider);
			//    var defaultLanguage = "";
			//    var languagesActive = SexyContent.GetCulturesWithActiveState(Webpage.Dnn.Portal.PortalId, App.ZoneId).Any(c => c.Active);
			//    if (languagesActive)
			//        defaultLanguage = Webpage.Dnn.Portal.DefaultLanguage;
			//    ((ToSic.SexyContent.DataSources.App)App.Data).DefaultLanguage = defaultLanguage;
			//    ((ToSic.SexyContent.DataSources.App)App.Data).CurrentUserName = Webpage.Dnn.User.Username;
			//}
		}

		private void InitWebpage()
		{
			if (string.IsNullOrEmpty(TemplatePath)) return;

			var objectValue = RuntimeHelpers.GetObjectValue(CreateWebPageInstance());
			if ((objectValue == null))
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage found at '{0}' was not created.", new object[] { TemplatePath }));

			Webpage = objectValue as SexyContentWebPage;

			if ((Webpage == null))
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage at '{0}' must derive from SexyContentWebPage.", new object[] { TemplatePath }));

			Webpage.Context = HttpContext;
			Webpage.VirtualPath = TemplatePath;
			Webpage.InstancePurpose = InstancePurposes;
			InitHelpers(Webpage);
		}

		public override void CustomizeData()
		{
			if (Webpage != null)
				Webpage.CustomizeData();
		}

		public override void CustomizeSearch(Dictionary<string, List<ISearchInfo>> searchInfos, ModuleInfo moduleInfo, DateTime beginDate)
		{
			if (Webpage != null)
				Webpage.CustomizeSearch(searchInfos, moduleInfo, beginDate);
		}
	}
}