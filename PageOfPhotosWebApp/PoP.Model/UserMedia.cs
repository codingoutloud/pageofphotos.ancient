using System;

using Microsoft.WindowsAzure.Storage.Table;

namespace PoP.Models
{
    /// <summary>
    /// UserMedia model class.  This contains records of user activity within Page of Photos
    /// It inherits the TableEntity class which contains the following properties: 
    /// ETag: used for optimistic concurrency for updates, see more here: http://msdn.microsoft.com/en-us/library/windowsazure/dd179427
    /// PartitionKey: unique identifier for the partition within a give table, first part of an entity's primary key  
    /// RowKey: unique identifier for an entity within a partition, second part of an entity's primary key
    /// Timestamp: readonly value of timestamp of storage operation, maintained by Microsoft
    /// For more information on the table service data model, see here: http://msdn.microsoft.com/en-us/library/windowsazure/dd179338.aspx
    /// </summary>
    public class UserMedia : TableEntity
    {
        public int UserId { get; set; }
        public string UserMediaId { get; set; }
        public string Type { get; set; }
        public string StorageFormat { get; set; }
        public string Url { get; set; }
        public string ThumbUrl { get; set; }
        public string UserName { get; set; }

        // this empty constructor is required to support Microsoft.WindowsAzure.Storage.Table.TableQuery class,
        // which requires a parameterless constructor due to the new constraint on TElement
        public UserMedia()
        {
        }

        // constructor overload that take elements needed to create partition and row keys.  
        // Additionally creates and sets the keys as a convience so developer does not worry
        // about setting keys in the application layer.
        public UserMedia(int userId)
        {
            // http://stackoverflow.com/questions/417108/why-are-there-dashes-in-a-net-guid
            // create guid and remove dashes as they are extraneous
            string userMediaId = Guid.NewGuid().ToString().Replace("-","");

            UserId = userId;
            UserMediaId = userMediaId;

            SetKeys(FormatPartitionKey(userId), FormatRowKey(userMediaId));
        }

        private void SetKeys(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        // format of unique identifier for the partition within a give table, 
        // first part of an entity's primary key.  
        public static string FormatPartitionKey(int userId)
        {
            return userId.ToString();
        }

        // format of unique identifier for an entity within a partition, 
        // second part of an entity's primary key
        public static string FormatRowKey(string userMediaId)
        {
            return userMediaId;
        }
    }
}
