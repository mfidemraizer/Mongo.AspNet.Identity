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

    public abstract partial class MongoUserStore<TUserId, TUser> : IUserClaimStore<TUser>
    {
        public async Task AddClaimAsync(TUser user, Claim claim)
        {
            ExtenderUser<TUserId> extendedUser = await FindExtendedUserByIdAsync(((IIdentityUser<TUserId>)user).Id, f => f.Project(u => u.Claims));

            if (extendedUser.Claims.Add(claim))
            {
                IMongoCollection<ExtenderUser<TUserId>> userCollection = GetCollection<ExtenderUser<TUserId>>(UserCollectionName);

                await userCollection.UpdateOneAsync
                (
                    Builders<ExtenderUser<TUserId>>.Filter.Eq("Id", ((IIdentityUser<TUserId>)user).Id),
                    Builders<ExtenderUser<TUserId>>.Update.Set("Claims", extendedUser.Claims)
                );
            }
        }

        public async Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            HashSet<Claim> claims = (await FindExtendedUserByIdAsync(((IIdentityUser<TUserId>)user).Id, p => p.Project(extUser => extUser.Claims))).Claims;

            if (claims != null) return claims.ToList();
            else return new List<Claim>();
        }

        public async Task RemoveClaimAsync(TUser user, Claim claim)
        {
            ExtenderUser<TUserId> extendedUser = await FindExtendedUserByIdAsync(((IIdentityUser<TUserId>)user).Id, f => f.Project(u => u.Claims));

            if (extendedUser.Claims.Remove(claim))
            {
                IMongoCollection<ExtenderUser<TUserId>> userCollection = GetCollection<ExtenderUser<TUserId>>(UserCollectionName);

                await userCollection.UpdateOneAsync
                (
                    Builders<ExtenderUser<TUserId>>.Filter.Eq("Id", ((IIdentityUser<TUserId>)user).Id),
                    Builders<ExtenderUser<TUserId>>.Update.Set("Claims", extendedUser.Claims)
                );
            }
        }
    }
}