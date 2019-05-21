﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RIAPP.DataService.Core;
using RIAPP.DataService.Core.Exceptions;
using RIAPP.DataService.Core.Metadata;
using RIAPP.DataService.Core.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using DataType = RIAPP.DataService.Core.Types.DataType;
using ValueConverter = RIAPP.DataService.Utils.ValueConverter;

namespace RIAPP.DataService.EFCore
{
    public abstract class EFDomainService<TDB> : BaseDomainService
       where TDB : DbContext
    {
        private TDB _db;
        private bool _ownsDb;

        public EFDomainService (IServiceContainer serviceContainer, TDB db = default(TDB))
            : base(serviceContainer)
        {
            _db = db;
        }

        public TDB DB
        {
            get
            {
                if (_db == null)
                {
                    _db = CreateDataContext();
                    if (_db != null)
                    {
                        _ownsDb = true;
                    }
                }
                return _db;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (_db != null && _ownsDb)
            {
                _db.Dispose();
                _db = null;
                _ownsDb = false;
            }

            base.Dispose(isDisposing);
        }

        #region Overridable Methods
        protected virtual TDB CreateDataContext()
        {
            return Activator.CreateInstance<TDB>();
        }

        protected override async Task ExecuteChangeSet()
        {
            try
            {
                using (var transScope = new TransactionScope(TransactionScopeOption.RequiresNew,
                    new TransactionOptions
                    {
                        IsolationLevel = IsolationLevel.ReadCommitted,
                        Timeout = TimeSpan.FromMinutes(1.0)
                    }, TransactionScopeAsyncFlowOption.Enabled))
                {
                    /*
                    var entities = from e in DB.ChangeTracker.Entries()
                                   where e.State == EntityState.Added
                                       || e.State == EntityState.Modified
                                   select e.Entity;
                    */
                    await DB.SaveChangesAsync();

                    transScope.Complete();
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException(ex.Message);
            }

            await AfterExecuteChangeSet();
        }

        protected override DesignTimeMetadata GetDesignTimeMetadata(bool isDraft)
        {
            var metadata = new DesignTimeMetadata();
            IModel dbModel = this.DB.Model;
            var allEntities = dbModel.GetEntityTypes().ToArray();
            var plainEntities = allEntities.Where(t => !t.IsOwned()).ToArray();

            Dictionary<string, IEntityType> ownedTypesMap = allEntities.Where(t => t.IsOwned()).ToDictionary(t => t.Name);

            foreach (IEntityType entityInfo in plainEntities)
            {
                IEnumerable<IProperty> edmProps = entityInfo.GetProperties().Where(p => !p.IsShadowProperty).ToArray();
                // IEnumerable<string> edmProps1 = entityInfo.GetNavigations().Select(n => n.ForeignKey.DeclaringEntityType.Name).ToArray();
                IEnumerable<INavigation> ownedTypes = entityInfo.GetNavigations().Where(n=>ownedTypesMap.ContainsKey(n.ForeignKey.DeclaringEntityType.Name)).ToArray();
                var dbSetInfo = new DbSetInfo
                {
                    dbSetName = entityInfo.ClrType.Name,
                    EntityType = entityInfo.ClrType
                };
                metadata.DbSets.Add(dbSetInfo);
                GenerateFieldInfos(metadata, entityInfo, dbSetInfo, edmProps, ownedTypes, ownedTypesMap);
                GenerateAssociations(metadata, entityInfo, dbSetInfo);
            }

            return metadata;
        }
        #endregion

        #region helper methods
        #region Complex Type Fields
        private void UpdateNestedFieldInfo(Field fieldInfo, PropertyInfo propInfo)
        {
            fieldInfo.fieldType = FieldType.None;

            var colAttr = propInfo.GetCustomAttributes<ColumnAttribute>().FirstOrDefault();
            if (colAttr != null && !string.IsNullOrEmpty(colAttr.TypeName))
            {
                if (colAttr.TypeName.ToLower() == "date")
                {
                    fieldInfo.dataType = DataType.Date;
                }
            }

            fieldInfo.isNullable = ServiceContainer.GetValueConverter().IsNullableType(propInfo.PropertyType) || (propInfo.PropertyType == typeof(string) && !propInfo.GetCustomAttributes<RequiredAttribute>().Any());
            fieldInfo.isReadOnly = fieldInfo.isAutoGenerated || propInfo.GetSetMethod() == null;

            var strLenAttr = propInfo.GetCustomAttributes<StringLengthAttribute>().FirstOrDefault();
            if (strLenAttr != null && strLenAttr.MaximumLength > 0)
            {
                fieldInfo.maxLength = strLenAttr.MaximumLength;
            }
            else
            {
                var maxLenAttr = propInfo.GetCustomAttributes<MaxLengthAttribute>().FirstOrDefault();
                if (maxLenAttr != null && maxLenAttr.Length > 0)
                {
                    fieldInfo.maxLength = maxLenAttr.Length;
                }
            }

            var notmappedAttr = propInfo.GetCustomAttributes<NotMappedAttribute>().FirstOrDefault();
            if (notmappedAttr != null)
            {
                fieldInfo.fieldType = FieldType.ServerCalculated;
            }
        }

        private void GenerateNestedFieldInfos(Field parentField, Type nestedType)
        {
            var nestedProps = nestedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToArray();

            foreach(var propInfo in nestedProps)
            {
                bool isContinue = false;
                var fieldInfo = new Field { fieldName = propInfo.Name };
                var isArray = false;
                try
                {
                    fieldInfo.dataType = ValueConverter.DataTypeFromTypeCore(propInfo.PropertyType, out isArray);
                }
                catch (UnsupportedTypeException)
                {
                    if (propInfo.PropertyType.IsClass)
                    {
                        // complex Type field
                        GenerateNestedFieldInfos(fieldInfo, propInfo.PropertyType);
                    }
                    else
                    {
                        isContinue = true;
                    }
                }

                if (isContinue)
                {
                    continue;
                }

                if (fieldInfo.fieldType != FieldType.Object)
                {
                    UpdateNestedFieldInfo(fieldInfo, propInfo);
                }

                parentField.fieldType = FieldType.Object;
                parentField.nested.Add(fieldInfo);
            }
        }
        #endregion

        private void GenerateAssociation(DesignTimeMetadata metadata, IEntityType entityInfo, DbSetInfo dbSetInfo, INavigation childToParentNav)
        {
            var inverseNavigation = childToParentNav.FindInverse();
            var assoc_name = string.Format("{0}_{1}", inverseNavigation.Name, childToParentNav.Name);
            // Console.WriteLine($"Generate association: {assoc_name}");

            var assoc = metadata.Associations.Where(a => a.name == assoc_name).FirstOrDefault();
            if (assoc == null)
            {
                IProperty[] principalProps = inverseNavigation.ForeignKey.PrincipalKey.Properties.ToArray();
                IProperty[] childProps = childToParentNav.ForeignKey.Properties.ToArray();

                assoc = new Association();
                assoc.name = assoc_name;
                var parentEntity = inverseNavigation.DeclaringEntityType;
                var childEntity = childToParentNav.DeclaringEntityType;

                assoc.parentDbSetName = parentEntity.ClrType.Name;
                assoc.childDbSetName = childEntity.ClrType.Name;
                assoc.childToParentName = childToParentNav.Name;

                if (inverseNavigation != null)
                {
                    assoc.parentToChildrenName = inverseNavigation.Name;
                }

                int i = 0;
                foreach (var pkProp in principalProps)
                {
                    var frel = new FieldRel();
                    frel.childField = childProps?[i]?.Name;
                    frel.parentField = pkProp.Name;
                    assoc.fieldRels.Add(frel);
                    ++i;
                }

                metadata.Associations.Add(assoc);
            }
        }

        private void UpdateFieldInfo(Field fieldInfo, IProperty edmProp)
        {
            fieldInfo.isAutoGenerated = isAutoGenerated(edmProp);
            fieldInfo.isNullable = edmProp.IsNullable;
            fieldInfo.isReadOnly = edmProp.AfterSaveBehavior == PropertySaveBehavior.Throw;
            fieldInfo.allowClientDefault = !fieldInfo.isAutoGenerated && fieldInfo.isReadOnly && edmProp.BeforeSaveBehavior == PropertySaveBehavior.Save;
            var maxLen = edmProp.GetMaxLength();
            if (maxLen.HasValue)
            {
                fieldInfo.maxLength = maxLen.Value;
            }

            if (edmProp.IsConcurrencyToken)
            {
                fieldInfo.fieldType = FieldType.RowTimeStamp;
            }
            else if (this.isNotMapped(edmProp))
            {
                fieldInfo.fieldType = FieldType.ServerCalculated;
            }
            else
            {
                fieldInfo.fieldType = FieldType.None;
            }
        }

        private void GenerateOwnedTypeFieldInfos(Field parentField, IEntityType ownedType, IEnumerable<INavigation> nestedOwnedTypes, Dictionary<string, IEntityType> ownedMap)
        {
            parentField.fieldType = FieldType.Object;

            foreach (INavigation ownedNavigation in nestedOwnedTypes)
            {
                var fieldInfo = new Field { fieldName = ownedNavigation.Name };
                string nm = ownedNavigation.ForeignKey.DeclaringEntityType.Name;
                IEntityType nestedOwnedType2 = ownedMap[nm];
                IEnumerable<INavigation> nestedOwnedTypes2 = nestedOwnedType2.GetNavigations().Where(n => ownedMap.ContainsKey(n.ForeignKey.DeclaringEntityType.Name)).ToArray();
                GenerateOwnedTypeFieldInfos(fieldInfo, nestedOwnedType2, nestedOwnedTypes2, ownedMap);
                parentField.nested.Add(fieldInfo);
            }

            var edmProps = ownedType.GetProperties().Where(p => !p.IsShadowProperty).ToArray();

            foreach (IProperty edmProp in edmProps)
            {
                bool isContinue = false;
                var fieldInfo = new Field { fieldName = edmProp.Name };
                var isArray = false;
                try
                {
                    fieldInfo.dataType = ValueConverter.DataTypeFromTypeCore(edmProp.ClrType, out isArray);
                }
                catch (UnsupportedTypeException)
                {
                    // Console.WriteLine($"{edmProp.Name} unsupported type {edmProp.ClrType.FullName}");

                    // complex type
                    if (edmProp.ClrType.IsClass)
                    {
                        GenerateNestedFieldInfos(fieldInfo, edmProp.ClrType);
                    }
                    else
                    {
                        isContinue = true;
                    }
                }

                if (isContinue)
                {
                    continue;
                }

                if (fieldInfo.fieldType != FieldType.Object)
                {
                    UpdateFieldInfo(fieldInfo, edmProp);
                }

                parentField.nested.Add(fieldInfo);
            }
        }

        private void GenerateFieldInfos(DesignTimeMetadata metadata, IEntityType entityInfo, DbSetInfo dbSetInfo, IEnumerable<IProperty> edmProps, IEnumerable<INavigation> ownedTypes, Dictionary<string, IEntityType> ownedMap)
        {
            short pkNum = 0;
            // Console.WriteLine($"Generate fields: {entityInfo.Name} FieldsCount: {edmProps.Count()}");

            foreach(INavigation ownedNavigation in ownedTypes)
            {
                var fieldInfo = new Field { fieldName = ownedNavigation.Name };
                string nm = ownedNavigation.ForeignKey.DeclaringEntityType.Name;
                IEntityType ownedType = ownedMap[nm];
                IEnumerable<INavigation> nestedOwnedTypes = ownedType.GetNavigations().Where(n => ownedMap.ContainsKey(n.ForeignKey.DeclaringEntityType.Name)).ToArray();
                GenerateOwnedTypeFieldInfos(fieldInfo, ownedType, nestedOwnedTypes, ownedMap);
                dbSetInfo.fieldInfos.Add(fieldInfo);
            }

            foreach (IProperty edmProp in edmProps)
            {
                bool isContinue = false;
                var fieldInfo = new Field { fieldName = edmProp.Name };
                var isArray = false;

                try
                {
                    fieldInfo.dataType = ValueConverter.DataTypeFromTypeCore(edmProp.ClrType, out isArray);
                }
                catch (UnsupportedTypeException)
                {
                    // Console.WriteLine($"{edmProp.Name} unsupported type {edmProp.ClrType.FullName}");
                    isContinue = true;
                }

                if (isContinue)
                {
                    continue;
                }

                if (fieldInfo.fieldType != FieldType.Object)
                {
                    UpdateFieldInfo(fieldInfo, edmProp);

                    if (edmProp.IsPrimaryKey())
                    {
                        ++pkNum;
                        fieldInfo.isPrimaryKey = pkNum;
                        fieldInfo.isReadOnly = true;
                    }
                }

                dbSetInfo.fieldInfos.Add(fieldInfo);
            }
        }

        private void GenerateAssociations(DesignTimeMetadata metadata, IEntityType entityInfo, DbSetInfo dbSetInfo)
        {
            IEnumerable<INavigation> childToParentNavs = entityInfo.GetNavigations().Where(n=> n.IsDependentToPrincipal());
            foreach (INavigation childToParentNav in childToParentNavs)
            {
                GenerateAssociation(metadata, entityInfo, dbSetInfo, childToParentNav);
            }
        }

        private bool isAutoGenerated(IProperty prop)
        {
            return prop.ValueGenerated == ValueGenerated.OnAdd;
        }

        private bool isNotMapped(IProperty prop)
        {
            return prop.BeforeSaveBehavior == PropertySaveBehavior.Ignore && prop.AfterSaveBehavior == PropertySaveBehavior.Ignore;
        }
        #endregion
    }
}
