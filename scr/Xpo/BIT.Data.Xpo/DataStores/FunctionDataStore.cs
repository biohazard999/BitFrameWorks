﻿using BIT.Data.DataTransfer;
using BIT.Data.Services;
using BIT.Data.Xpo.Models;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Helpers;
using System;
using System.Threading.Tasks;


namespace BIT.Data.Xpo.DataStores
{
    public abstract class FunctionDataStore : IDataStore, ICommandChannel
    {

        IFunction FunctionClient { get; set; }
        IObjectSerializationService objectSerializationHelper;
        public FunctionDataStore(IFunction functionClient, IObjectSerializationService objectSerializationHelper, AutoCreateOption autoCreateOption)
        {
            this.FunctionClient = functionClient;
            this.objectSerializationHelper = objectSerializationHelper;
        }

        //HACK you need to implement the GetConnectionString on the child classes because you might need different information for differnt functions clients
        //public static string GetConnectionString(string EndPoint, string param1, string Param2)
        //{

        //    return $"{DataStoreBase.XpoProviderTypeParameterName}={XpoProviderTypeString};EndPoint={EndPoint};Token={param1};DataStoreId={param1}";
        //}
  
        private AutoCreateOption autoCreateOption;
        public AutoCreateOption AutoCreateOption => autoCreateOption;

        protected virtual async Task<ModificationResult> ModifyData(params ModificationStatement[] dmlStatements)
        {

            IDataParameters Parameters = new DataParameters();
            Parameters.MemberName = nameof(ModifyData);
            Parameters.ParametersValue = this.objectSerializationHelper.ToByteArray<ModificationStatement[]>(dmlStatements);
            var DataResult = await FunctionClient.ExecuteFunction(Parameters);
            var ModificationResults = this.objectSerializationHelper.GetObjectsFromByteArray<ModificationResult>(DataResult.ResultValue);
            return ModificationResults;
        }

        protected virtual async Task<SelectedData> SelectData(params SelectStatement[] selects)
        {
            IDataParameters Parameters = new DataParameters();
            Parameters.MemberName = nameof(SelectData);
            Parameters.ParametersValue = this.objectSerializationHelper.ToByteArray<SelectStatement[]>(selects);
            var DataResult = await FunctionClient.ExecuteFunction(Parameters);
            var SelectedData = this.objectSerializationHelper.GetObjectsFromByteArray<SelectedData>(DataResult.ResultValue);
            return SelectedData;
        }

        protected virtual async Task<UpdateSchemaResult> UpdateSchema(bool dontCreateIfFirstTableNotExist, params DBTable[] tables)
        {
            IDataParameters Parameters = new DataParameters();
            UpdateSchemaParameters updateSchemaParameters = new UpdateSchemaParameters(dontCreateIfFirstTableNotExist, tables);
            Parameters.MemberName = nameof(UpdateSchema);
            Parameters.ParametersValue = this.objectSerializationHelper.ToByteArray<UpdateSchemaParameters>(updateSchemaParameters);
            IDataResult DataResult = await FunctionClient.ExecuteFunction(Parameters);
            var UpdateSchemaResult = this.objectSerializationHelper.GetObjectsFromByteArray<UpdateSchemaResult>(DataResult.ResultValue);
            return UpdateSchemaResult;
        }

        protected virtual async Task<object> Do(string command, object args)
        {
            IDataParameters Parameters = new DataParameters();
            CommandChannelDoParams commandChannelDoParams = new CommandChannelDoParams(command, args);
            Parameters.MemberName = nameof(UpdateSchema);
            Parameters.ParametersValue = this.objectSerializationHelper.ToByteArray<CommandChannelDoParams>(commandChannelDoParams);
            IDataResult DataResult = await FunctionClient.ExecuteFunction(Parameters);
            var UpdateSchemaResult = this.objectSerializationHelper.GetObjectsFromByteArray<object>(DataResult.ResultValue);
            return UpdateSchemaResult;
        }
        object ICommandChannel.Do(string command, object args)
        {

            return this.Do(command, args).GetAwaiter().GetResult();

        }

        UpdateSchemaResult IDataStore.UpdateSchema(bool doNotCreateIfFirstTableNotExist, params DBTable[] tables)
        {
            return this.UpdateSchema(doNotCreateIfFirstTableNotExist, tables).GetAwaiter().GetResult();
        }

        SelectedData IDataStore.SelectData(params SelectStatement[] selects)
        {
            return this.SelectData(selects).GetAwaiter().GetResult();
        }

        ModificationResult IDataStore.ModifyData(params ModificationStatement[] dmlStatements)
        {
            return this.ModifyData(dmlStatements).GetAwaiter().GetResult();
        }
    }
}