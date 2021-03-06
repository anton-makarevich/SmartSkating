using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Sanet.SmartSkating.Backend.Azure.Models;
using Sanet.SmartSkating.Dto.Models;
using Sanet.SmartSkating.Dto.Services;

namespace Sanet.SmartSkating.Backend.Azure.Services
{
    public class AzureTablesDataService:IDataService
    {
        private readonly ILogger _log;
        private const string WayPointsTableName = "WayPointsTable";
        private const string SessionsTableName = "SessionsTable";
        private const string BleScansTableName = "BleScansTable";
        private const string DevicesTableName = "DevicesTable";

        private readonly CloudTable? _wayPointsTable;
        private readonly CloudTable? _sessionsTable;
        private readonly CloudTable? _scansTable;
        private readonly CloudTable? _devicesTable;

        private readonly bool _hasStorageAccess;
        
        public AzureTablesDataService(ILogger log)
        {
            _log = log;
            var connectionString = Environment.GetEnvironmentVariable("TableConnectionString");
            _hasStorageAccess = !string.IsNullOrEmpty(connectionString);
            if (!_hasStorageAccess)
            {
                ErrorMessage = "NoStorageAccess";
                log.LogCritical(ErrorMessage);
                return;
            }

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            
            _wayPointsTable = tableClient.GetTableReference(WayPointsTableName);
            _sessionsTable = tableClient.GetTableReference(SessionsTableName);
            _scansTable = tableClient.GetTableReference(BleScansTableName);
            _devicesTable = tableClient.GetTableReference(DevicesTableName);
        }
        
        public Task<bool> SaveWayPointAsync(WayPointDto wayPoint)
        {
            var entity = new WayPointEntity(wayPoint);
            return SaveEntityAsync(entity,_wayPointsTable);
        }

        private async Task<bool> SaveEntityAsync(TableEntity entity, CloudTable? table)
        {
            if (!_hasStorageAccess || table == null)
            {
                if (string.IsNullOrEmpty(ErrorMessage))
                    ErrorMessage = $"Table for {entity.GetType()} is not defined";
                return false;
            }

            try
            {
                await table.CreateIfNotExistsAsync();
                var insertOperation = TableOperation.InsertOrMerge(entity);
                await table.ExecuteAsync(insertOperation);

                return true;
            }
            catch (Exception exception)
            {
                ErrorMessage = ErrorMessage + "\n" + exception.Message;
                _log.LogError(exception, exception.Message);
                return false;
            }
        }

        public string ErrorMessage { get; private set; } = string.Empty;

        public async Task<List<WayPointDto>> GetWayPointForSessionAsync(string sessionId)
        {
            if (_wayPointsTable == null || !await _wayPointsTable.ExistsAsync()) return new List<WayPointDto>();
            var filterPk = TableQuery.GenerateFilterCondition(
                nameof(WayPointEntity.PartitionKey),
                QueryComparisons.Equal, sessionId);

            var query = new TableQuery<WayPointEntity>().Where(filterPk);
            var result = await _wayPointsTable.ExecuteQuerySegmentedAsync(query,null);
            return result.Results.Select(f=>f.ToDto()).ToList();
        }

        public Task<List<WayPointDto>> GetAllWayPointsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteWayPointAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveSessionAsync(SessionDto session)
        {
            var entity = new SessionEntity(session);
            return SaveEntityAsync(entity,_sessionsTable);
        }

        public Task<List<SessionDto>> GetAllSessionsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteSessionAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveBleScanAsync(BleScanResultDto bleScan)
        {
            var entity = new BleScanEntity(bleScan);
            return SaveEntityAsync(entity,_scansTable);
        }

        public Task<List<BleScanResultDto>> GetAllBleScansAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteBleScanAsync(string bleScanId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveDeviceAsync(DeviceDto deviceDto)
        {
            var entity = new DeviceEntity(deviceDto);
            return SaveEntityAsync(entity,_devicesTable);
        }

        public async Task<List<SessionDto>> GetAllSessionsForAccountAsync(string accountId)
        {
            if (_sessionsTable == null || !await _sessionsTable.ExistsAsync()) return new List<SessionDto>();
            var filterPk = TableQuery.GenerateFilterCondition(
                nameof(SessionEntity.PartitionKey),
                QueryComparisons.Equal, accountId);

            var query = new TableQuery<SessionEntity>().Where(filterPk);
            var result = await _sessionsTable.ExecuteQuerySegmentedAsync(query,null);
            return result.Results.Select(f=>f.ToDto()).ToList();
        }
    }
}