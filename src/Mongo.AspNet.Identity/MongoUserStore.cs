﻿/*
    Copyright 2015 Matías Fidemraizer (https://linkedin.com/in/mfidemraizer)
    
    "Mongo.AspNet.Identity" project (https://github.com/mfidemraizer/Mongo.AspNet.Identity)
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
 
    You may obtain a copy of the License at
    http://www.apache.org/licenses/LICENSE-2.0
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

namespace Mongo.AspNet.Identity
{
    using Microsoft.AspNet.Identity;
    using MongoDB.Bson.Serialization;
    using MongoDB.Driver;
    using System;
    using System.Configuration;
    using System.Diagnostics.Contracts;
    using System.Threading;

    public partial class MongoUserStore<TUser>
        where TUser : class, IUser, IIdentityUser
    {
        private readonly AutoResetEvent _configEvent = new AutoResetEvent(true);
        private readonly MongoClient _client;

        private const string RequiredSettingFormat = "'{0}' setting is missing or empty in current application configuration";

        public MongoUserStore(MongoClient client, Action<BsonClassMap<TUser>> genericUserMapper = null)
        {
            Contract.Assert(!string.IsNullOrEmpty(DatabaseName), string.Format(RequiredSettingFormat, "mongo:aspnetidentity:databaseName"));
            _client = client;

            _configEvent.WaitOne();

            try
            {
                if (!AlreadyConfigured)
                {
                    CreateClassMaps();

                    AlreadyConfigured = true;
                }
            }
            finally
            {
                _configEvent.Set();
            }
        }

        private MongoClient Client { get { return _client; } }
        public string DatabaseName { get { return ConfigurationManager.AppSettings["mongo:aspnetidentity:databaseName"]; } }
        private static bool AlreadyConfigured { get; set; }

        private IMongoCollection<TDocument> GetCollection<TDocument>(string name)
        {
            return Client.GetDatabase(DatabaseName).GetCollection<TDocument>(name);
        }

        protected virtual void CreateClassMaps()
        {
            BsonClassMap<IdentityUser> genericUserMap = BsonClassMap.RegisterClassMap<IdentityUser>
            (
                map =>
                {
                    map.MapMember(user => user.Id).SetElementName("id");
                    map.MapMember(user => user.UserName).SetElementName("userName");
                    map.MapMember(user => user.Email).SetElementName("email");
                    map.MapMember(user => user.PasswordHash).SetElementName("passwordHash");
                    map.MapMember(user => user.PhoneNumber).SetElementName("phoneNumber");
                    map.MapMember(user => user.SecurityStamp).SetElementName("securityStamp");

                    map.SetDiscriminator("type");
                }
            );

            BsonClassMap.RegisterClassMap<ExtendedUser>
            (
                map =>
                {
                    map.MapMember(user => user.Id).SetElementName("id");
                    map.MapMember(user => user.PhoneNumberConfirmed).SetElementName("phoneNumberConfirmed");
                    map.MapMember(user => user.TwoFactorAuthenticationEnabled).SetElementName("twoFactorAuthenticationEnabled");
                    map.MapMember(user => user.Roles).SetElementName("roles");
                    map.MapMember(user => user.Logins).SetElementName("logins");
                }
            );

            BsonClassMap.RegisterClassMap<UserLoginInfo>
            (
                map =>
                {
                    map.MapMember(user => user.LoginProvider).SetElementName("provider");
                    map.MapMember(user => user.ProviderKey).SetElementName("key");
                }
            );
        }
    }
}