﻿using RIAPP.DataService.Core.Config;
using System;
using System.Collections.Generic;

namespace RIAPP.DataService.Core
{
    public class DataManagerContainer<TService> : IDataManagerContainer<TService>
        where TService : BaseDomainService
    {
        private readonly IServiceContainer<TService> _serviceContainer;
        private readonly IDataManagerRegister _dataManagerRegister;

        public DataManagerContainer(IServiceContainer<TService> serviceContainer,
            IDataManagerRegister dataManagerRegister)
        {
            _serviceContainer = serviceContainer ?? throw new ArgumentNullException(nameof(serviceContainer));
            _dataManagerRegister = dataManagerRegister ?? throw new ArgumentNullException(nameof(dataManagerRegister));
        }

        public object GetDataManager(Type modelType)
        {
            ServiceTypeDescriptor descriptor;
            if (_dataManagerRegister.TryGetDescriptor(modelType, out descriptor))
            {
                return _serviceContainer.GetService(descriptor.ServiceType);
            }

            return null;
        }

        public IDataManager<TModel> GetDataManager<TModel>()
            where TModel : class
        {
            var res = GetDataManager(typeof(TModel));
            return (IDataManager<TModel>)res;
        }

        public IEnumerable<ServiceTypeDescriptor> Descriptors
        {
            get
            {
                return _dataManagerRegister.Descriptors;
            }
        }
    }
}