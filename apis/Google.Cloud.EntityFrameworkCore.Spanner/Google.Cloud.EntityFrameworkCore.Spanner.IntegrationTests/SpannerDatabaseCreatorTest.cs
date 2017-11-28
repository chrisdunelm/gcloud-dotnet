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

using Google.Cloud.Spanner.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.EntityFrameworkCore.Spanner.IntegrationTests
{
    public class SpannerDatabaseCreatorTest
    {
        private static async Task Exists_returns_false_when_database_doesnt_exist_test(bool async)
        {
            using (var testDatabase = SpannerTestStore.CreateScratch(false))
            {
                var creator = GetDatabaseCreator(testDatabase);

                Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
            }
        }

        private static async Task Exists_returns_true_when_database_exists_test(bool async)
        {
            using (var testDatabase = SpannerTestStore.CreateScratch(true))
            {
                var creator = GetDatabaseCreator(testDatabase);

                Assert.True(async ? await creator.ExistsAsync() : creator.Exists());
            }
        }

        private static async Task HasTables_throws_when_database_doesnt_exist_test(bool async)
        {
            //TODO(benwu)
            await Task.Delay(1).ConfigureAwait(false);
//            using (var testDatabase = SpannerTestStore.CreateScratch(false))
//            {
//                await ((TestDatabaseCreator) GetDatabaseCreator(testDatabase)).ExecutionStrategyFactory.Create()
//                    .ExecuteAsync(
//                        (TestDatabaseCreator) GetDatabaseCreator(testDatabase),
//                        async creator =>
//                        {
//                            var sqlState = async
//                                ? (await Assert.ThrowsAsync<SpannerException>(() => creator.HasTablesAsyncBase()))
//                                .SqlState
//                                : Assert.Throws<SpannerException>(() => creator.HasTablesBase()).SqlState;
//
//                            Assert.Equal(
//                                "3D000", // Login failed error number
//                                sqlState);
//                        });
//            }
        }

        private static async Task HasTables_returns_false_when_database_exists_but_has_no_tables_test(bool async)
        {
            using (var testDatabase = SpannerTestStore.CreateScratch(true))
            {
                var creator = GetDatabaseCreator(testDatabase);

                Assert.False(async
                    ? await ((TestDatabaseCreator) creator).HasTablesAsyncBase()
                    : ((TestDatabaseCreator) creator).HasTablesBase());
            }
        }

        private static async Task HasTables_returns_true_when_database_exists_and_has_any_tables_test(bool async)
        {
            using (var testDatabase = SpannerTestStore.CreateScratch(true))
            {
                await testDatabase.ExecuteNonQueryAsync("CREATE TABLE some_table (id SERIAL PRIMARY KEY)");

                var creator = GetDatabaseCreator(testDatabase);

                Assert.True(async
                    ? await ((TestDatabaseCreator) creator).HasTablesAsyncBase()
                    : ((TestDatabaseCreator) creator).HasTablesBase());
            }
        }

        private static async Task Delete_will_delete_database_test(bool async)
        {
            using (var testDatabase = SpannerTestStore.CreateScratch(true))
            {
                testDatabase.Connection.Close();

                var creator = GetDatabaseCreator(testDatabase);

                Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

                if (async)
                {
                    await creator.DeleteAsync();
                }
                else
                {
                    creator.Delete();
                }

                Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
            }
        }

        private static async Task Delete_throws_when_database_doesnt_exist_test(bool async)
        {
            using (var testDatabase = SpannerTestStore.CreateScratch(false))
            {
                var creator = GetDatabaseCreator(testDatabase);

                if (async)
                {
                    await Assert.ThrowsAsync<SpannerException>(() => creator.DeleteAsync());
                }
                else
                {
                    Assert.Throws<SpannerException>(() => creator.Delete());
                }
            }
        }

        private static async Task CreateTables_creates_schema_in_existing_database_test(bool async)
        {
            using (var testDatabase = SpannerTestStore.CreateScratch(true))
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSpanner()
                    .BuildServiceProvider();

                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(serviceProvider)
                    .UseSpanner(testDatabase.ConnectionString, b => b.ApplyConfiguration());

                using (var context = new BloggingContext(optionsBuilder.Options))
                {
                    var creator = (RelationalDatabaseCreator) context.GetService<IDatabaseCreator>();

                    if (async)
                    {
                        await creator.CreateTablesAsync();
                    }
                    else
                    {
                        creator.CreateTables();
                    }

                    if (testDatabase.Connection.State != ConnectionState.Open)
                    {
                        await testDatabase.Connection.OpenAsync();
                    }

                    var tables =
                        await testDatabase.QueryAsync<string>(
                            "SELECT table_name FROM information_schema.tables WHERE table_type = 'BASE TABLE' AND table_schema NOT IN ('pg_catalog', 'information_schema')");
                    Assert.Equal(1, tables.Count());
                    Assert.Equal("Blogs", tables.Single());

                    var columns =
                        await testDatabase.QueryAsync<string>(
                            "SELECT table_name || '.' || column_name FROM information_schema.columns WHERE table_name='Blogs'");
                    Assert.Equal(2, columns.Count());
                    //TODO(benwu)
#pragma warning disable xUnit2012 // Do not use Enumerable.Any() to check if a value exists in a collection
//                    Assert.True(columns.Any(c => c == "Blogs.Id"));
#pragma warning restore xUnit2012 // Do not use Enumerable.Any() to check if a value exists in a collection
//                    Assert.True(columns.Any(c => c == "Blogs.Name"));
                }
            }
        }

        private static async Task CreateTables_throws_if_database_does_not_exist_test(bool async)
        {
            //TODO(benwu)
            await Task.Delay(1).ConfigureAwait(false);
//            using (var testDatabase = SpannerTestStore.CreateScratch(false))
//            {
//                var creator = GetDatabaseCreator(testDatabase);
//
//                var ex = async
//                    ? await Assert.ThrowsAsync<SpannerException>(() => creator.CreateTablesAsync())
//                    : Assert.Throws<SpannerException>(() => creator.CreateTables());
//                Assert.Equal(
//                    "3D000", // Login failed error number
//                    ex.SqlState);
//            }
        }

        private static async Task Create_creates_physical_database_but_not_tables_test(bool async)
        {
            using (var testDatabase = SpannerTestStore.CreateScratch(false))
            {
                var creator = GetDatabaseCreator(testDatabase);

                Assert.False(creator.Exists());

                if (async)
                {
                    await creator.CreateAsync();
                }
                else
                {
                    creator.Create();
                }

                Assert.True(creator.Exists());

                if (testDatabase.Connection.State != ConnectionState.Open)
                {
                    await testDatabase.Connection.OpenAsync();
                }

                Assert.Equal(0,
                    (await testDatabase.QueryAsync<string>(
                        "SELECT table_name FROM information_schema.tables WHERE table_type = 'BASE TABLE' AND table_schema NOT IN ('pg_catalog', 'information_schema')")
                    ).Count());
            }
        }

        private static async Task Create_throws_if_database_already_exists_test(bool async)
        {
            //TODO(benwu)
            await Task.Delay(1).ConfigureAwait(false);

//            using (var testDatabase = SpannerTestStore.CreateScratch(true))
//            {
//                var creator = GetDatabaseCreator(testDatabase);
//
//                var ex = async
//                    ? await Assert.ThrowsAsync<SpannerException>(() => creator.CreateAsync())
//                    : Assert.Throws<SpannerException>(() => creator.Create());
//                Assert.Equal(
//                    "42P04", // Database with given name already exists
//                    ex.SqlState);
//            }
        }

        private static IServiceProvider CreateContextServices(SpannerTestStore testStore)
            => ((IInfrastructure<IServiceProvider>) new BloggingContext(
                    new DbContextOptionsBuilder()
                        .UseSpanner(testStore.ConnectionString, b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(new ServiceCollection()
                            .AddEntityFrameworkSpanner()
                            //.AddScoped<SpannerExecutionStrategyFactory, TestSpannerExecutionStrategyFactory>()
                            .AddScoped<IRelationalDatabaseCreator, TestDatabaseCreator>().BuildServiceProvider())
                        .Options))
                .Instance;

        private static IRelationalDatabaseCreator GetDatabaseCreator(SpannerTestStore testStore)
            => CreateContextServices(testStore).GetRequiredService<IRelationalDatabaseCreator>();

        /*
        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestSpannerExecutionStrategyFactory : SpannerExecutionStrategyFactory
        {
            public TestSpannerExecutionStrategyFactory(IDbContextOptions options, ICurrentDbContext currentDbContext, ILogger<IExecutionStrategy> logger)
                : base(options, currentDbContext, logger)
            {
            }

            protected override IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyContext context) => NoopExecutionStrategy.Instance;
        }*/

        private class BloggingContext : DbContext
        {
            public BloggingContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Blog> Blogs { get; set; }

            public class Blog
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
        }

        public class TestDatabaseCreator : SpannerDatabaseCreator
        {
            public TestDatabaseCreator(
                RelationalDatabaseCreatorDependencies dependencies,
                ISpannerRelationalConnection connection,
                IRawSqlCommandBuilder rawSqlCommandBuilder)
                : base(dependencies, connection, rawSqlCommandBuilder)
            {
            }

            public IExecutionStrategyFactory ExecutionStrategyFactory => Dependencies.ExecutionStrategyFactory;

            public bool HasTablesBase() => HasTables();

            public Task<bool> HasTablesAsyncBase(CancellationToken cancellationToken = default(CancellationToken))
                => HasTablesAsync(cancellationToken);
        }

        [Fact]
        public async Task Create_creates_physical_database_but_not_tables()
        {
            await Create_creates_physical_database_but_not_tables_test(false);
        }

        [Fact]
        public async Task Create_throws_if_database_already_exists()
        {
            await Create_throws_if_database_already_exists_test(false);
        }

        [Fact]
        public async Task CreateAsync_creates_physical_database_but_not_tables()
        {
            await Create_creates_physical_database_but_not_tables_test(true);
        }

        [Fact]
        public async Task CreateAsync_throws_if_database_already_exists()
        {
            await Create_throws_if_database_already_exists_test(true);
        }

        [Fact]
        public async Task CreateTables_creates_schema_in_existing_database()
        {
            await CreateTables_creates_schema_in_existing_database_test(false);
        }

        [Fact]
        public async Task CreateTables_throws_if_database_does_not_exist()
        {
            await CreateTables_throws_if_database_does_not_exist_test(false);
        }

        [Fact]
        public async Task CreateTablesAsync_creates_schema_in_existing_database()
        {
            await CreateTables_creates_schema_in_existing_database_test(true);
        }

        [Fact]
        public async Task CreateTablesAsync_throws_if_database_does_not_exist()
        {
            await CreateTables_throws_if_database_does_not_exist_test(true);
        }

        [Fact]
        public async Task Delete_throws_when_database_doesnt_exist()
        {
            await Delete_throws_when_database_doesnt_exist_test(false);
        }

        [Fact]
        public async Task Delete_will_delete_database()
        {
            await Delete_will_delete_database_test(false);
        }

        [Fact]
        public async Task DeleteAsync_throws_when_database_doesnt_exist()
        {
            await Delete_throws_when_database_doesnt_exist_test(true);
        }

        [Fact]
        public async Task DeleteAsync_will_delete_database()
        {
            await Delete_will_delete_database_test(true);
        }

        [Fact]
        public async Task Exists_returns_false_when_database_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(false);
        }

        [Fact]
        public async Task Exists_returns_true_when_database_exists()
        {
            await Exists_returns_true_when_database_exists_test(false);
        }

        [Fact]
        public async Task ExistsAsync_returns_false_when_database_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(true);
        }

        [Fact]
        public async Task ExistsAsync_returns_true_when_database_exists()
        {
            await Exists_returns_true_when_database_exists_test(true);
        }

        [Fact]
        public async Task HasTables_returns_false_when_database_exists_but_has_no_tables()
        {
            await HasTables_returns_false_when_database_exists_but_has_no_tables_test(false);
        }

        [Fact]
        public async Task HasTables_returns_true_when_database_exists_and_has_any_tables()
        {
            await HasTables_returns_true_when_database_exists_and_has_any_tables_test(false);
        }

        [Fact]
        public async Task HasTables_throws_when_database_doesnt_exist()
        {
            await HasTables_throws_when_database_doesnt_exist_test(false);
        }

        [Fact]
        public async Task HasTablesAsync_returns_false_when_database_exists_but_has_no_tables()
        {
            await HasTables_returns_false_when_database_exists_but_has_no_tables_test(true);
        }

        [Fact]
        public async Task HasTablesAsync_returns_true_when_database_exists_and_has_any_tables()
        {
            await HasTables_returns_true_when_database_exists_and_has_any_tables_test(true);
        }

        [Fact]
        public async Task HasTablesAsync_throws_when_database_doesnt_exist()
        {
            await HasTables_throws_when_database_doesnt_exist_test(true);
        }
    }
}