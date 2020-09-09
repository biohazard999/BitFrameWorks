﻿using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BIT.Xpo
{
    public class XpoInitializer : IXpoInitializer, IAsyncXpoInitializer
    {





        private IDataLayer UpdateDal;
        private IDataLayer WorkindDal;

        readonly DataLayerType dataLayerType;


        XPDictionary dictionary;
        Type[] entityTypes;
        IDataStore dataStore;
        public DataLayerType DataLayerType => dataLayerType;

        public XpoInitializer(string connectionString, DataLayerType DataLayerType, params Type[] entityTypes)
           : this(XpoDefault.GetConnectionProvider(connectionString, AutoCreateOption.DatabaseAndSchema), DataLayerType, entityTypes)
        {

        }
        public XpoInitializer(IDataStore DataStore, DataLayerType DataLayerType, params Type[] entityTypes)
        {
            this.entityTypes = entityTypes;
            this.dataLayerType = DataLayerType;
            this.dataStore = DataStore;
            dictionary = this.PrepareDictionary(entityTypes);
          
            switch (DataLayerType)
            {
                case DataLayerType.Simple:
                    this.WorkindDal = new SimpleDataLayer(dictionary, DataStore);
                    break;
                case DataLayerType.ThreadSafe:
                    this.WorkindDal = new ThreadSafeDataLayer(dictionary, DataStore);
                    break;
            }
        }
        public XpoInitializer(string connectionString, params Type[] entityTypes)
              : this(XpoDefault.GetConnectionProvider(connectionString, AutoCreateOption.DatabaseAndSchema), DataLayerType.Simple, entityTypes)
        {

        }
        public XpoInitializer(IDataStore DataStore, params Type[] entityTypes)
             : this(DataStore, DataLayerType.Simple, entityTypes)
        {

        }

        public UpdateSchemaResult? InitSchema()
        {
            UpdateDal = new SimpleDataLayer(dictionary, dataStore);
            if (XpoDefault.DataLayer == null)
            {
                return UpdateDal.UpdateSchema(false, dictionary.CollectClassInfos(entityTypes));
            }
            return null;
        }
        public async Task<UpdateSchemaResult?> InitSchemaAsync(CancellationToken cancellationToken = default)
        {
            UpdateDal = new SimpleDataLayer(dictionary, dataStore);
            if (XpoDefault.DataLayer == null && UpdateDal is IDataLayerAsync dataLayerAsync)
            {
                return await dataLayerAsync.UpdateSchemaAsync(cancellationToken, false, dictionary.CollectClassInfos(entityTypes));
            }
            return null;
        }

        public UnitOfWork CreateUnitOfWork()
        {
            return new UnitOfWork(this.WorkindDal);
        }
        XPDictionary PrepareDictionary(Type[] entityTypes)
        {
            var dict = new ReflectionDictionary();
            dict.GetDataStoreSchema(entityTypes);
            return dict;
        }
    }
}
