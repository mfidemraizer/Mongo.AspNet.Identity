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
    using MongoDB.Driver;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public partial class MongoUserStore<TUser> : IUserClaimStore<TUser>
    {

        public async Task AddClaimAsync(TUser user, Claim claim)
        {
            ExtendedUser extendedUser = await FindExtendedUserByIdAsync(((IUser)user).Id);

            if (extendedUser.Claims.Add(claim))
            {
                IMongoCollection<ExtendedUser> userCollection = GetCollection<ExtendedUser>(UserCollectionName);

                userCollection.UpdateOneAsync
                (
                    Builders<ExtendedUser>.Filter.Eq(extUser => extUser.Id, ((IUser)user).Id),
                    Builders<ExtendedUser>.Update.Set(extUser => extUser.Claims, extendedUser.Claims)
                );
            }
        }

        public async Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            return (await FindExtendedUserByIdAsync(((IUser)user).Id, p => p.Project(extUser => extUser.Claims))).Claims.ToList();
        }

        public async Task RemoveClaimAsync(TUser user, Claim claim)
        {
            ExtendedUser extendedUser = await FindExtendedUserByIdAsync(((IUser)user).Id);

            if (extendedUser.Claims.Remove(claim))
            {
                IMongoCollection<ExtendedUser> userCollection = GetCollection<ExtendedUser>(UserCollectionName);

                userCollection.UpdateOneAsync
                (
                    Builders<ExtendedUser>.Filter.Eq(extUser => extUser.Id, ((IUser)user).Id),
                    Builders<ExtendedUser>.Update.Set(extUser => extUser.Claims, extendedUser.Claims)
                );
            }
        }
    }
}