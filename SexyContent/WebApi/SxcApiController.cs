﻿using DotNetNuke.Web.Api;
using System.Collections.Generic;
using ToSic.Eav;
using ToSic.Eav.DataSources;
using ToSic.SexyContent.DataSources;
using ToSic.SexyContent.Razor.Helpers;

namespace ToSic.SexyContent.WebApi
{

    [SupportedModules("2sxc-app")]
    public abstract class SxcApiController : DnnApiController, IAppAndDataHelpers
    {
        private SexyContent _sexyContent;
        private SexyContent _sexyContentUncached;

        private AppAndDataHelpers _appAndDataHelpers;
        private AppAndDataHelpers AppAndDataHelpers {
            get
            {
                if (_appAndDataHelpers == null)
                {
                    var moduleInfo = Request.FindModuleInfo();
                    var viewDataSource = Sexy.GetViewDataSource(Request.FindModuleId(), SexyContent.HasEditPermission(moduleInfo), Sexy.GetTemplateForModule(moduleInfo.ModuleID));
                    _appAndDataHelpers = new AppAndDataHelpers(Sexy, moduleInfo, (ViewDataSource)viewDataSource, Sexy.App);
                }
                return _appAndDataHelpers;
            }
        }

        // Sexy object should not be accessible for other assemblies - just internal use
        internal SexyContent Sexy
        {
            get
            {
                if (_sexyContent == null)
                    _sexyContent = ToSic.SexyContent.WebApi.HttpRequestMessageExtensions.GetSxcOfModuleContext(Request);
                return _sexyContent;
            }
        }
        internal SexyContent SexyUncached
        {
            get
            {
                if (_sexyContentUncached == null)
                    _sexyContentUncached = ToSic.SexyContent.WebApi.HttpRequestMessageExtensions.GetUncachedSxcOfModuleContext(Request);
                return _sexyContentUncached;
            }
        }


        #region AppAndDataHelpers implementation

        public DnnHelper Dnn
        {
            get { return AppAndDataHelpers.Dnn; }
        }
        public ToSic.SexyContent.App App
        {
            get { return AppAndDataHelpers.App; }
        }
        public ViewDataSource Data
        {
            get { return AppAndDataHelpers.Data; }
        }

        /// <summary>
        /// Transform a IEntity to a DynamicEntity as dynamic object
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public dynamic AsDynamic(IEntity entity)
        {
            return AppAndDataHelpers.AsDynamic(entity);
        }

        /// <summary>
        /// Makes sure a dynamicEntity could be wrapped in AsDynamic()
        /// </summary>
        /// <param name="dynamicEntity"></param>
        /// <returns></returns>
        public dynamic AsDynamic(dynamic dynamicEntity)
        {
            return AppAndDataHelpers.AsDynamic(dynamicEntity);
        }

        /// <summary>
        /// Returns the value of a KeyValuePair as DynamicEntity
        /// </summary>
        /// <param name="entityKeyValuePair"></param>
        /// <returns></returns>
        public dynamic AsDynamic(KeyValuePair<int, IEntity> entityKeyValuePair)
        {
            return AppAndDataHelpers.AsDynamic(entityKeyValuePair.Value);
        }

        /// <summary>
        /// In case AsDynamic is used with Data["name"]
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public IEnumerable<dynamic> AsDynamic(IDataStream stream)
        {
            return AppAndDataHelpers.AsDynamic(stream.List);
        }

        /// <summary>
        /// In case AsDynamic is used with Data["name"].List
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public IEnumerable<dynamic> AsDynamic(IDictionary<int, IEntity> list)
        {
            return AppAndDataHelpers.AsDynamic(list);
        }

        /// <summary>
        /// Transform a DynamicEntity dynamic object back to a IEntity instance
        /// </summary>
        /// <param name="dynamicEntity"></param>
        /// <returns></returns>
        public IEntity AsEntity(dynamic dynamicEntity)
        {
            return AppAndDataHelpers.AsEntity(dynamicEntity);
        }

        /// <summary>
        /// Returns a list of DynamicEntities
        /// </summary>
        /// <param name="entities">List of entities</param>
        /// <returns></returns>
        public IEnumerable<dynamic> AsDynamic(IEnumerable<IEntity> entities)
        {
            return AppAndDataHelpers.AsDynamic(entities);
        }

        public IDataSource CreateSource(string typeName = "", IDataSource inSource = null, IConfigurationProvider configurationProvider = null)
        {
            return AppAndDataHelpers.CreateSource(typeName, inSource, configurationProvider);
        }

        public T CreateSource<T>(IDataSource inSource = null, IConfigurationProvider configurationProvider = null)
        {
            return AppAndDataHelpers.CreateSource<T>(inSource, configurationProvider);
        }

        /// <summary>
        /// Create a source with initial stream to attach...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inStream"></param>
        /// <returns></returns>
        public T CreateSource<T>(IDataStream inStream)
        {
            return AppAndDataHelpers.CreateSource<T>(inStream);
        }

		public dynamic Content
		{
			get { return AppAndDataHelpers.Content; }
		}

		public dynamic Presentation
		{
			get { return AppAndDataHelpers.Presentation; }
		}

		public dynamic ListContent
		{
			get { return AppAndDataHelpers.ListContent; }
		}

		public dynamic ListPresentation
		{
			get { return AppAndDataHelpers.ListPresentation; }
		}

		public List<Element> List
		{
			get { return AppAndDataHelpers.List; }
		}

        #endregion

    }
}
