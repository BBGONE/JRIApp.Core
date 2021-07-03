var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        if (typeof b !== "function" && b !== null)
            throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
define(["require", "exports", "jriapp_shared", "jriapp_db"], function (require, exports, RIAPP, dbMOD) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.DbContext = exports.DbSets = exports.FileSystemObjectDb = void 0;
    var FileSystemObjectEntity = (function (_super) {
        __extends(FileSystemObjectEntity, _super);
        function FileSystemObjectEntity(aspect) {
            return _super.call(this, aspect) || this;
        }
        FileSystemObjectEntity.prototype.toString = function () {
            return 'FileSystemObjectEntity';
        };
        Object.defineProperty(FileSystemObjectEntity.prototype, "Key", {
            get: function () { return this._aspect._getFieldVal('Key'); },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(FileSystemObjectEntity.prototype, "ParentKey", {
            get: function () { return this._aspect._getFieldVal('ParentKey'); },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(FileSystemObjectEntity.prototype, "Name", {
            get: function () { return this._aspect._getFieldVal('Name'); },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(FileSystemObjectEntity.prototype, "Level", {
            get: function () { return this._aspect._getFieldVal('Level'); },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(FileSystemObjectEntity.prototype, "HasSubDirs", {
            get: function () { return this._aspect._getFieldVal('HasSubDirs'); },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(FileSystemObjectEntity.prototype, "IsFolder", {
            get: function () { return this._aspect._getFieldVal('IsFolder'); },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(FileSystemObjectEntity.prototype, "fullPath", {
            get: function () { return this._aspect._getCalcFieldVal('fullPath'); },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(FileSystemObjectEntity.prototype, "ExtraProps", {
            get: function () { return this._aspect._getCalcFieldVal('ExtraProps'); },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(FileSystemObjectEntity.prototype, "Parent", {
            get: function () { return this._aspect._getNavFieldVal('Parent'); },
            set: function (v) { this._aspect._setNavFieldVal('Parent', v); },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(FileSystemObjectEntity.prototype, "Children", {
            get: function () { return this._aspect._getNavFieldVal('Children'); },
            enumerable: false,
            configurable: true
        });
        return FileSystemObjectEntity;
    }(RIAPP.CollectionItem));
    var FileSystemObjectDb = (function (_super) {
        __extends(FileSystemObjectDb, _super);
        function FileSystemObjectDb(dbContext) {
            var _this = this;
            var opts = {
                dbContext: dbContext,
                dbSetInfo: { "fieldInfos": [], "enablePaging": false, "pageSize": 100, "dbSetName": "FileSystemObject" },
                childAssoc: ([{ "name": "ChildToParent", "parentDbSetName": "FileSystemObject", "childDbSetName": "FileSystemObject", "childToParentName": "Parent", "parentToChildrenName": "Children", "onDeleteAction": 1, "fieldRels": [{ "parentField": "Key", "childField": "ParentKey" }] }]),
                parentAssoc: ([{ "name": "ChildToParent", "parentDbSetName": "FileSystemObject", "childDbSetName": "FileSystemObject", "childToParentName": "Parent", "parentToChildrenName": "Children", "onDeleteAction": 1, "fieldRels": [{ "parentField": "Key", "childField": "ParentKey" }] }])
            };
            opts.dbSetInfo.fieldInfos = ([{ "fieldName": "Key", "isPrimaryKey": 1, "dataType": 1, "isNullable": false, "isReadOnly": true, "isAutoGenerated": true, "isNeedOriginal": true, "maxLength": 255, "dateConversion": 0, "allowClientDefault": false, "range": "", "regex": "", "fieldType": 0, "dependentOn": "", "nested": null }, { "fieldName": "ParentKey", "isPrimaryKey": 0, "dataType": 1, "isNullable": true, "isReadOnly": true, "isAutoGenerated": false, "isNeedOriginal": true, "maxLength": 255, "dateConversion": 0, "allowClientDefault": false, "range": "", "regex": "", "fieldType": 0, "dependentOn": "", "nested": null }, { "fieldName": "Name", "isPrimaryKey": 0, "dataType": 1, "isNullable": false, "isReadOnly": true, "isAutoGenerated": false, "isNeedOriginal": true, "maxLength": 255, "dateConversion": 0, "allowClientDefault": false, "range": "", "regex": "", "fieldType": 0, "dependentOn": "", "nested": null }, { "fieldName": "Level", "isPrimaryKey": 0, "dataType": 3, "isNullable": false, "isReadOnly": true, "isAutoGenerated": false, "isNeedOriginal": true, "maxLength": -1, "dateConversion": 0, "allowClientDefault": false, "range": "", "regex": "", "fieldType": 0, "dependentOn": "", "nested": null }, { "fieldName": "HasSubDirs", "isPrimaryKey": 0, "dataType": 2, "isNullable": false, "isReadOnly": true, "isAutoGenerated": false, "isNeedOriginal": true, "maxLength": -1, "dateConversion": 0, "allowClientDefault": false, "range": "", "regex": "", "fieldType": 0, "dependentOn": "", "nested": null }, { "fieldName": "IsFolder", "isPrimaryKey": 0, "dataType": 2, "isNullable": false, "isReadOnly": true, "isAutoGenerated": false, "isNeedOriginal": true, "maxLength": -1, "dateConversion": 0, "allowClientDefault": false, "range": "", "regex": "", "fieldType": 0, "dependentOn": "", "nested": null }, { "fieldName": "fullPath", "isPrimaryKey": 0, "dataType": 1, "isNullable": true, "isReadOnly": false, "isAutoGenerated": false, "isNeedOriginal": true, "maxLength": -1, "dateConversion": 0, "allowClientDefault": false, "range": "", "regex": "", "fieldType": 2, "dependentOn": "", "nested": null }, { "fieldName": "ExtraProps", "isPrimaryKey": 0, "dataType": 0, "isNullable": true, "isReadOnly": false, "isAutoGenerated": false, "isNeedOriginal": true, "maxLength": -1, "dateConversion": 0, "allowClientDefault": false, "range": "", "regex": "", "fieldType": 2, "dependentOn": "", "nested": null }, { "fieldName": "Parent", "isPrimaryKey": 0, "dataType": 0, "isNullable": true, "isReadOnly": false, "isAutoGenerated": false, "isNeedOriginal": true, "maxLength": -1, "dateConversion": 0, "allowClientDefault": false, "range": "", "regex": "", "fieldType": 3, "dependentOn": "ParentKey", "nested": null }, { "fieldName": "Children", "isPrimaryKey": 0, "dataType": 0, "isNullable": true, "isReadOnly": false, "isAutoGenerated": false, "isNeedOriginal": true, "maxLength": -1, "dateConversion": 0, "allowClientDefault": false, "range": "", "regex": "", "fieldType": 3, "dependentOn": "", "nested": null }]);
            _this = _super.call(this, opts) || this;
            return _this;
        }
        FileSystemObjectDb.prototype.itemFactory = function (aspect) {
            return new FileSystemObjectEntity(aspect);
        };
        FileSystemObjectDb.prototype.findEntity = function (key) {
            return this.findByPK(RIAPP.Utils.arr.fromList(arguments));
        };
        FileSystemObjectDb.prototype.toString = function () {
            return 'FileSystemObjectDb';
        };
        FileSystemObjectDb.prototype.createReadAllQuery = function (args) {
            var query = this.createQuery('ReadAll');
            query.params = args;
            return query;
        };
        FileSystemObjectDb.prototype.createReadChildrenQuery = function (args) {
            var query = this.createQuery('ReadChildren');
            query.params = args;
            return query;
        };
        FileSystemObjectDb.prototype.createReadRootQuery = function (args) {
            var query = this.createQuery('ReadRoot');
            query.params = args;
            return query;
        };
        FileSystemObjectDb.prototype.definefullPathField = function (getFunc) { this._defineCalculatedField('fullPath', getFunc); };
        FileSystemObjectDb.prototype.defineExtraPropsField = function (getFunc) { this._defineCalculatedField('ExtraProps', getFunc); };
        return FileSystemObjectDb;
    }(dbMOD.DbSet));
    exports.FileSystemObjectDb = FileSystemObjectDb;
    var DbSets = (function (_super) {
        __extends(DbSets, _super);
        function DbSets(dbContext) {
            var _this = _super.call(this, dbContext) || this;
            _this._createDbSet("FileSystemObject", FileSystemObjectDb);
            return _this;
        }
        Object.defineProperty(DbSets.prototype, "FileSystemObject", {
            get: function () { return this.getDbSet("FileSystemObject"); },
            enumerable: false,
            configurable: true
        });
        return DbSets;
    }(dbMOD.DbSets));
    exports.DbSets = DbSets;
    var DbContext = (function (_super) {
        __extends(DbContext, _super);
        function DbContext() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        DbContext.prototype._createDbSets = function () {
            return new DbSets(this);
        };
        DbContext.prototype._createAssociations = function () {
            return [{ "name": "ChildToParent", "parentDbSetName": "FileSystemObject", "childDbSetName": "FileSystemObject", "childToParentName": "Parent", "parentToChildrenName": "Children", "onDeleteAction": 1, "fieldRels": [{ "parentField": "Key", "childField": "ParentKey" }] }];
        };
        DbContext.prototype._createMethods = function () {
            return [{ "methodName": "ReadAll", "parameters": [{ "name": "includeFiles", "dataType": 2, "isArray": false, "isNullable": false, "dateConversion": 0, "ordinal": 0 }, { "name": "infoType", "dataType": 1, "isArray": false, "isNullable": false, "dateConversion": 0, "ordinal": 1 }], "methodResult": true, "isQuery": true }, { "methodName": "ReadChildren", "parameters": [{ "name": "parentKey", "dataType": 1, "isArray": false, "isNullable": false, "dateConversion": 0, "ordinal": 0 }, { "name": "level", "dataType": 3, "isArray": false, "isNullable": false, "dateConversion": 0, "ordinal": 1 }, { "name": "path", "dataType": 1, "isArray": false, "isNullable": false, "dateConversion": 0, "ordinal": 2 }, { "name": "includeFiles", "dataType": 2, "isArray": false, "isNullable": false, "dateConversion": 0, "ordinal": 3 }, { "name": "infoType", "dataType": 1, "isArray": false, "isNullable": false, "dateConversion": 0, "ordinal": 4 }], "methodResult": true, "isQuery": true }, { "methodName": "ReadRoot", "parameters": [{ "name": "includeFiles", "dataType": 2, "isArray": false, "isNullable": false, "dateConversion": 0, "ordinal": 0 }, { "name": "infoType", "dataType": 1, "isArray": false, "isNullable": false, "dateConversion": 0, "ordinal": 1 }], "methodResult": true, "isQuery": true }];
        };
        return DbContext;
    }(dbMOD.DbContext));
    exports.DbContext = DbContext;
});
