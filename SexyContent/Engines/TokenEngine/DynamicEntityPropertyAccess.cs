using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Tokens;
using ToSic.Eav;

namespace ToSic.SexyContent.Engines.TokenEngine
{
    public class DynamicEntityPropertyAccess : IPropertyAccess
    {
        private DynamicEntity _entity;

        public DynamicEntityPropertyAccess(DynamicEntity entity)
        {
            _entity = entity;
        }

        public CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        /// <summary>
        /// Get Property out of NameValueCollection
        /// </summary>
        /// <param name="strPropertyName"></param>
        /// <param name="strFormat"></param>
        /// <param name="formatProvider"></param>
        /// <param name="AccessingUser"></param>
        /// <param name="AccessLevel"></param>
        /// <param name="PropertyNotFound"></param>
        /// <returns></returns>
        public string GetProperty(string strPropertyName, string strFormat, CultureInfo formatProvider, UserInfo AccessingUser, Scope AccessLevel, ref bool PropertyNotFound)
        {
	        if (_entity == null)
				return string.Empty;
			var entityWithValue = _entity;
			// see if trying to access a sub-property...
	        if (strPropertyName.IndexOf('.') > 0)
	        {
		        var subPropertyName = strPropertyName.Substring(strPropertyName.IndexOf('.') + 1); // used if we're going for a property of a related entity...
		        strPropertyName = strPropertyName.Substring(0, strPropertyName.IndexOf('.'));
		        try
		        {
			        var maybeEntityList = _entity.Entity[strPropertyName];

			        if (maybeEntityList.GetType() == typeof (AttributeModel<EntityRelationshipModel>))
			        {
				        var entityIdList = ((ValueModel<EntityRelationshipModel>) (((AttributeModel<EntityRelationshipModel>) maybeEntityList).DefaultValue))
							.TypedContents.EntityIds;
						// var dEntityList = new DynamicEntity(_entity)[strPropertyName]
						
						// todo: get the entity with ID = entityIdList[0]
				        //[0] as DynamicEntity;
						// check if it is a list, select first, use that for the rest of the work...
						// then set strpropertyname to be the subPropertyName
						// entityWithValue = new DynamicEntity(childEntity);
				        strPropertyName = subPropertyName;
			        }
			        else
			        {
				        // this is the case if a subproperty was accessed, and the relationship doesn't even exist
				        return string.Empty;
			        }

		        }
		        catch
		        {
			        return string.Empty;
		        }
	        }

            // Return empty string if Entity is null
			if (entityWithValue == null)
                return string.Empty;

            string outputFormat = strFormat == string.Empty ? "g" : strFormat;

            bool propertyNotFound;
			object valueObject = entityWithValue.GetEntityValue(strPropertyName, out propertyNotFound);
	        

            if (!propertyNotFound && valueObject != null)
            {
                switch (valueObject.GetType().Name)
                {
                    case "String":
                        return PropertyAccess.FormatString((string)valueObject, strFormat);
                    case "Boolean":
                        return (PropertyAccess.Boolean2LocalizedYesNo((bool)valueObject, formatProvider));
                    case "DateTime":
                    case "Double":
                    case "Single":
                    case "Int32":
                    case "Int64":
                    case "Decimal":
                        return (((IFormattable)valueObject).ToString(outputFormat, formatProvider));

					// todo: 2dm - add ability to access related-objects properties like "Content:Author.Name"
					case "List`1":
		                throw new Exception("trying to get relationship to work...");
                    default:
                        return PropertyAccess.FormatString(valueObject.ToString(), strFormat);
                }

                ////string value = PropertyAccess.GetObjectProperty(valueObject, strPropertyName, outputFormat, formatProvider, ref PropertyNotFound);
                //string value = string.IsNullOrEmpty(outputFormat) ? valueObject.ToString() : string.Format(formatProvider, outputFormat, valueObject);
                //PortalSecurity Security = new PortalSecurity();
                //value = Security.InputFilter(value, PortalSecurity.FilterFlag.NoScripting);
                ////return Security.InputFilter(PropertyAccess.FormatString(value, strFormat), PortalSecurity.FilterFlag.NoScripting);
                //return Security.InputFilter(value, PortalSecurity.FilterFlag.NoScripting);
            }
            else
            {
                PropertyNotFound = true;
                return string.Empty;
            }
        }
    }
}