﻿//
// Copyright 2015 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using Npgsql;
using Steeltoe.CloudFoundry.Connector.Services;
using System;

namespace Steeltoe.CloudFoundry.Connector.PostgreSql
{
    public class PostgresProviderConnectorFactory
    {
        protected PostgresServiceInfo _info;
        protected PostgresProviderConnectorOptions _config;
        protected PostgresProviderConfigurer _configurer = new PostgresProviderConfigurer();

        internal PostgresProviderConnectorFactory()
        {

        }
        public PostgresProviderConnectorFactory(PostgresServiceInfo sinfo, PostgresProviderConnectorOptions config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _info = sinfo;
            _config = config;
        }
        internal protected virtual object Create(IServiceProvider provider)
        {
            var connectionString = CreateConnectionString();
            if (connectionString != null)
                return new NpgsqlConnection(connectionString);
            return null;
        }

        internal protected virtual string CreateConnectionString()
        {
            return _configurer.Configure(_info, _config);
        }
    }
}
