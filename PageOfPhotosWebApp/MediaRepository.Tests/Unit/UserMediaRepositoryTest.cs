using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using MediaRepository.Table;
using PoP.Models;

namespace MediaRepository.Tests.Unit
{
    [TestClass]
    public class UserMediaRepositoryTest
    {
        #region properties

        private UserMediaRepository _repo { get; set; }

        private static UserMedia UserMedia1 { get; set; }

        #endregion

        #region constructor

        public UserMediaRepositoryTest()
        {
        }

        #endregion

        #region initialize
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            UserMedia1 = new UserMedia("test")
            {
                StorageFormat = "Test",
                Type = "Test"
            };
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            _repo = new UserMediaRepository(CloudStorageAccount.Parse("UseDevelopmentStorage=true"), "TestEntries");

            _repo.DeleteTable();
            _repo.CreateTable();
        }

        #endregion

        #region cleanup

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        #endregion

        #region test methods

        [TestMethod]
        public void InsertUserMedia_ValidEntity_PresentInTable()
        {
            _repo.Insert(UserMedia1);

            UserMedia returnedEntity = _repo.GetEntity(UserMedia.FormatPartitionKey(UserMedia1.UserId), UserMedia.FormatRowKey(UserMedia1.UserMediaId));

            Assert.AreEqual(UserMedia1.UserId, returnedEntity.UserId, "UserId does not match");
            Assert.AreEqual(UserMedia1.UserMediaId, returnedEntity.UserMediaId, "Order does not match");
            Assert.AreEqual(UserMedia1.StorageFormat, returnedEntity.StorageFormat, "StorageFormat does not match");
            Assert.AreEqual(UserMedia1.Type, returnedEntity.Type, "Type does not match");
        }

        [TestMethod]
        public void InsertUserMediaAsync_ValidEntity_PresentInTable()
        {
            Task<TableResult> insertTask = _repo.InsertAsync(UserMedia1);
            insertTask.Wait();

            UserMedia returnedEntity = (UserMedia)insertTask.Result.Result;

            Assert.IsFalse(insertTask.IsFaulted, "Task is faulted");
            Assert.AreEqual(UserMedia1.UserId, returnedEntity.UserId, "UserId does not match");
            Assert.AreEqual(UserMedia1.UserMediaId, returnedEntity.UserMediaId, "Order does not match");
            Assert.AreEqual(UserMedia1.StorageFormat, returnedEntity.StorageFormat, "StorageFormat does not match");
            Assert.AreEqual(UserMedia1.Type, returnedEntity.Type, "Type does not match");
        }

        [TestMethod]
        public void InsertBatchUserMediaAsync_ValidEntity_PresentInTaskResult()
        {
            var userMediaList = new List<UserMedia>();

            for (int i = 1; i <= 150; i++)
            {
                userMediaList.Add(
                    new UserMedia("test")
                    {
                        StorageFormat = "Test",
                        Type = "Test"
                    });
            }

            Task<IList<TableResult>> insertTask = _repo.InsertBatchAsync(userMediaList);
            insertTask.Wait();

            Assert.AreEqual(insertTask.Result.Count, 150, "Unexpected result count returned");
            Assert.IsFalse(insertTask.IsFaulted, "Task is faulted");
        }

        [TestMethod]
        public void GetUserMedia_ValidEntity_UserMediaIsReturned()
        {
            _repo.Insert(UserMedia1);

            UserMedia returnedEntity = _repo.GetEntity(UserMedia.FormatPartitionKey(UserMedia1.UserId), UserMedia.FormatRowKey(UserMedia1.UserMediaId));

            Assert.AreEqual(UserMedia1.UserId, returnedEntity.UserId, "UserId does not match");
            Assert.AreEqual(UserMedia1.UserMediaId, returnedEntity.UserMediaId, "Order does not match");
            Assert.AreEqual(UserMedia1.StorageFormat, returnedEntity.StorageFormat, "StorageFormat does not match");
            Assert.AreEqual(UserMedia1.Type, returnedEntity.Type, "Type does not match");
        }

        [TestMethod]
        public void GetUserMediaAsync_ValidEntity_UserMediaIsInTaskResult()
        {
            _repo.Insert(UserMedia1);

            Task<TableResult> getTask = _repo.GetEntityAsync(UserMedia.FormatPartitionKey(UserMedia1.UserId), UserMedia.FormatRowKey(UserMedia1.UserMediaId));
            getTask.Wait();

            UserMedia returnedEntity = (UserMedia)getTask.Result.Result;

            Assert.AreEqual(UserMedia1.UserId, returnedEntity.UserId, "UserId does not match");
            Assert.AreEqual(UserMedia1.UserMediaId, returnedEntity.UserMediaId, "Order does not match");
            Assert.AreEqual(UserMedia1.StorageFormat, returnedEntity.StorageFormat, "StorageFormat does not match");
            Assert.AreEqual(UserMedia1.Type, returnedEntity.Type, "Type does not match");
        }

        [TestMethod]
        public void GetPartition_InsertEntitiesInTwoPartitions_CorrectPartitionAndCountReturned()
        {
            var userMediaList = new List<UserMedia>();

            for (int i = 1; i <= 150; i++)
            {
                var entity = new UserMedia("test")
                {
                    StorageFormat = "Test",
                    Type = "Test"
                };

                if (i > 75)
                {
                    entity.PartitionKey = UserMedia.FormatPartitionKey("two");
                    entity.UserId = "two";

                }

                userMediaList.Add(entity);
            }

            Task<IList<TableResult>> insertTask = _repo.InsertBatchAsync(userMediaList);
            insertTask.Wait();

            IEnumerable<UserMedia> getResult = _repo.GetPartition(userMediaList[0].PartitionKey);

            Assert.AreEqual(getResult.Count(), 75, "Unexpected result count returned");
            Assert.AreEqual(getResult.First().PartitionKey, userMediaList[0].PartitionKey, "Wrong partition key returned");

        }

        [TestMethod]
        public void GetPartitionAsync_InsertEntitiesInTwoPartitions_CorrectPartitionAndCountReturned()
        {
            var userMediaList = new List<UserMedia>();

            for (int i = 1; i <= 150; i++)
            {
                var entity = new UserMedia("one")
                {
                    StorageFormat = "Test",
                    Type = "Test"
                };

                if (i > 75)
                {
                    entity.PartitionKey = UserMedia.FormatPartitionKey("two");
                    entity.UserId = "two";
                }

                userMediaList.Add(entity);
            }

            Task<IList<TableResult>> insertTask = _repo.InsertBatchAsync(userMediaList);
            insertTask.Wait();

            Assert.AreEqual(insertTask.Result.Where(r => r.HttpStatusCode == 201 &&
                    ((UserMedia)r.Result).PartitionKey == userMediaList[0].PartitionKey).Select(r => r).Count(),
                    75,
                    "Unexpected result count returned for first group");

            Assert.AreEqual(insertTask.Result.Where(r => r.HttpStatusCode == 201 &&
                    ((UserMedia)r.Result).PartitionKey == userMediaList[149].PartitionKey).Select(r => r).Count(),
                    75,
                    "Unexpected result count returned for second group");
        }

        [TestMethod]
        public void GetPartitionAsync_InsertEntitiesInTwoPartitions_CorrectAmountOfSegmentsAndCountsReturned()
        {
            var userMediaList = new List<UserMedia>();

            for (int i = 1; i <= 1050; i++)
            {
                var entity = new UserMedia("one")
                {
                    StorageFormat = "Test",
                    Type = "Test"
                };


                userMediaList.Add(entity);
            }

            Task<IList<TableResult>> insertTask = _repo.InsertBatchAsync(userMediaList);
            insertTask.Wait();

            Task<TableQuerySegment<UserMedia>> getResultTask = _repo.GetPartitionAsync(userMediaList[0].PartitionKey);
            getResultTask.Wait();

            var getResult = (TableQuerySegment<UserMedia>)getResultTask.Result;
            var segmentList = new List<TableQuerySegment<UserMedia>>();
            segmentList.Add(getResult);

            TableContinuationToken cToken = getResult.ContinuationToken;
            while (cToken != null)
            {
                var task = _repo.GetPartitionAsync(
                    userMediaList[0].PartitionKey,
                    cToken
                );

                task.Wait();

                segmentList.Add(task.Result);

                cToken = task.Result.ContinuationToken;
            }

            Assert.AreEqual(segmentList.Select(r => r.Results.Count()).Sum(), 1050, "Unexpected result count returned");
            Assert.AreEqual(getResult.First().PartitionKey, userMediaList[0].PartitionKey, "Wrong partition key returned");
        }
        #endregion
    }
}
