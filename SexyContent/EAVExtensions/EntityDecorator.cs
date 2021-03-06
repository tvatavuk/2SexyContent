﻿using System;
using System.Collections.Generic;
using ToSic.Eav;

namespace ToSic.SexyContent.EAVExtensions
{
    public abstract class EntityDecorator : IEntity
    {
        private readonly IEntity _baseEntity;

        protected EntityDecorator(IEntity baseEntity)
        {
            _baseEntity = baseEntity;
        }

        #region IEntity Implementation

        public IEntity GetDraft()
        {
            return _baseEntity.GetDraft();
        }

        public IEntity GetPublished()
        {
            return _baseEntity.GetPublished();
        }

        public int EntityId {
            get { return _baseEntity.EntityId; }
        }
        public int RepositoryId {
            get { return _baseEntity.RepositoryId; }
        }
        public Guid EntityGuid {
            get { return _baseEntity.EntityGuid; }
        }
        public int AssignmentObjectTypeId {
            get { return _baseEntity.AssignmentObjectTypeId; }
        }
        public Dictionary<string, IAttribute> Attributes {
            get { return _baseEntity.Attributes; }
        }
        public IContentType Type {
            get { return _baseEntity.Type; }
        }
        public IAttribute Title {
            get { return _baseEntity.Title; }
        }

        public DateTime Modified {
            get { return _baseEntity.Modified; }
        }

        public IAttribute this[string attributeName]
        {
            get { return _baseEntity[attributeName]; }
        }

        public RelationshipManager Relationships {
            get { return _baseEntity.Relationships; }
        }
        public bool IsPublished {
            get { return _baseEntity.IsPublished; }
        }

        #endregion
    }
}