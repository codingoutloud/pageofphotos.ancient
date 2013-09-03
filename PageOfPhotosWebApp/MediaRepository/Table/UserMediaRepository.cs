using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using MediaRepository.Extensions;
using PoP.Models;

namespace MediaRepository.Table
{
    public class UserMediaRepository
    {
        #region fields

        private readonly CloudTable _cloudTable;
        private readonly int _maxBatchEntityCount;

        #endregion

        #region table methods

        public void DeleteTable()
        {
            _cloudTable.DeleteIfExists();
        }

        public void CreateTable()
        {
            _cloudTable.CreateIfNotExists();
        }

        #endregion

        #region constructor

        // creates CloudTable object that maps to passed in storage account and table name 
        // If given table name does not exist, constructor will create the table.
        public UserMediaRepository(CloudStorageAccount storageAccount, String tableName)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            _cloudTable = tableClient.GetTableReference(tableName);
            _maxBatchEntityCount = 100;

            CreateTable();
        }

        #endregion

        #region entity methods

        public void Insert(UserMedia entity)
        {
            var insertOperation = TableOperation.Insert(entity);

            _cloudTable.Execute(insertOperation);
        }

        public async Task<TableResult> InsertAsync(UserMedia entity, CancellationToken ct = default(CancellationToken))
        {
            var insertOperation = TableOperation.Insert(entity);

            ICancellableAsyncResult ar = _cloudTable.BeginExecute(insertOperation, null, null);
            ct.Register(ar.Cancel);

            return await Task.Factory.FromAsync<TableResult>(ar, _cloudTable.EndExecute).ConfigureAwait(false);
        }

        public async Task<IList<TableResult>> InsertBatchAsync(IEnumerable<UserMedia> entities, CancellationToken ct = default(CancellationToken))
        {
            var tasks = new List<IList<TableResult>>();

            var entityBatches = entities.GroupAndSlice<UserMedia, string>(
                _maxBatchEntityCount,
                um => um.PartitionKey,
                KeyGroupPredicate
            );


            foreach (var entityBatch in entityBatches)
            {
                var tbo = new TableBatchOperation();

                foreach (var entity in entityBatch)
                {
                    tbo.Add(TableOperation.Insert(entity));
                }

                ICancellableAsyncResult ar = _cloudTable.BeginExecuteBatch(tbo, null, null);
                ct.Register(ar.Cancel);

                var batchTask = await Task.Factory.FromAsync<IList<TableResult>>(ar, _cloudTable.EndExecuteBatch).ConfigureAwait(false);

                tasks.Add(batchTask);
            }

            return tasks.SelectMany(t => t).ToList();
        }

        public UserMedia GetEntity(string partitionKey, string rowKey)
        {
            // Retrieval of entity by exact match, PartitionKey and RowKey, full primary key, the fastest search
            TableOperation operation = TableOperation.Retrieve<UserMedia>(partitionKey, rowKey);

            return (UserMedia)_cloudTable.Execute(operation).Result;
        }

        public Task<TableResult> GetEntityAsync(string partitionKey, string rowKey, CancellationToken ct = default(CancellationToken))
        {
            // Retrieval of entity by exact match, PartitionKey and RowKey, full primary key, the fastest search
            TableOperation operation = TableOperation.Retrieve<UserMedia>(partitionKey, rowKey);

            ICancellableAsyncResult ar = _cloudTable.BeginExecute(operation, null, null);
            ct.Register(ar.Cancel);

            return Task.Factory.FromAsync<TableResult>(ar, _cloudTable.EndExecute);
        }

        // synchronously get all entities in a given partition
        public IEnumerable<UserMedia> GetPartition(string partitionKey)
        {
            var tableQuery = new TableQuery<UserMedia>();

            var filter = CreatePartitionKeyFilter(partitionKey);

            tableQuery = tableQuery.Where(filter);

            var results = _cloudTable.ExecuteQuery(tableQuery)
                                              .ToList();
            return results;
        }

        // asynchronously get first 1000 entities in a given partition, pass in continuation token 
        // to retreive next 1000 entities and cancellation token to be able to signal to stop processing 
        // externally.  Cancellation token is optional.
        public async Task<TableQuerySegment<UserMedia>> GetPartitionAsync(string partitionKey, TableContinuationToken token = null, CancellationToken ct = default(CancellationToken))
        {
            var tableQuery = new TableQuery<UserMedia>();

            // Create and Set PartitionKey filter
            var filter = CreatePartitionKeyFilter(partitionKey);

            tableQuery = tableQuery.Where(filter);

            ICancellableAsyncResult ar = _cloudTable.BeginExecuteQuerySegmented(tableQuery, token, null, null);
            ct.Register(ar.Cancel);

            return await Task.Factory.FromAsync<TableQuerySegment<UserMedia>>(ar, _cloudTable.EndExecuteQuerySegmented<UserMedia>).ConfigureAwait(false);
        }

        #endregion

        #region filter methods

        // creates table query filter using built-in GenerateFilterCondition helper method which outputs 
        // string represention of the filter for retreivals by partition  
        // example: PartitionKey eq '15'
        private string CreatePartitionKeyFilter(string partitionKey)
        {
            return TableQuery.GenerateFilterCondition("PartitionKey",
                                                 QueryComparisons.Equal,
                                                 partitionKey);
        }

        private bool KeyGroupPredicate<T>(T entry1, T entry2) where T : TableEntity
        {
            if (entry1.PartitionKey == entry2.PartitionKey)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
