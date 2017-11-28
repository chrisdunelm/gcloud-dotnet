﻿// Copyright 2017 Google Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Google.Cloud.EntityFrameworkCore.Spanner.IntegrationTests
{
    public class DataAnnotationSpannerTest : DataAnnotationTestBase<SpannerTestStore, DataAnnotationSpannerFixture>
    {
        public DataAnnotationSpannerTest(DataAnnotationSpannerFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public override void StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            // Spanner does not support length
        }

        public override void TimestampAttribute_throws_if_value_in_database_changed()
        {
            // Spanner does not support length
        }

        public override void MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            // Spanner does not support length            
        }
    }
}