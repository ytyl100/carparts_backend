// ChargingStationManagement.Infrastructure/FileStorage/UserDataStorage.cs
using ChargingStationManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ChargingStationManagement.Infrastructure.FileStorage
{
    public interface IUserDataStorage
    {
        Task SaveUsersAsync(IEnumerable<User> users);
        Task<IEnumerable<User>> LoadUsersAsync();
        Task SaveUserAsync(User user);
        Task<User> LoadUserAsync(string userId);
        Task SaveUserSessionsAsync(string userId, IEnumerable<Session> sessions);
        Task<IEnumerable<Session>> LoadUserSessionsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task SaveUserTransactionsAsync(string userId, IEnumerable<Transaction> transactions);
        Task<IEnumerable<Transaction>> LoadUserTransactionsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    }

    public class UserDataStorage : IUserDataStorage
    {
        private readonly IJsonFileStorage _jsonStorage;
        private readonly ILogger<UserDataStorage> _logger;

        public UserDataStorage(
            IJsonFileStorage jsonStorage,
            ILogger<UserDataStorage> logger)
        {
            _jsonStorage = jsonStorage;
            _logger = logger;
        }

        public async Task SaveUsersAsync(IEnumerable<User> users)
        {
            try
            {
                var userList = users?.ToList() ?? new List<User>();
                var filePath = $"users/all_{DateTime.UtcNow:yyyyMMddHHmmss}.json";

                await _jsonStorage.WriteAsync(filePath, userList);

                // 同时保存到最新文件
                await _jsonStorage.WriteAsync("users/latest.json", userList);

                _logger.LogInformation($"保存了 {userList.Count} 个用户到文件");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存用户数据失败");
                throw;
            }
        }

        public async Task<IEnumerable<User>> LoadUsersAsync()
        {
            try
            {
                // 首先尝试加载最新文件
                if (await _jsonStorage.FileExistsAsync("users/latest.json"))
                {
                    return await _jsonStorage.ReadAsync<List<User>>("users/latest.json")
                        ?? new List<User>();
                }

                // 如果没有最新文件，查找最新的备份文件
                var files = await _jsonStorage.ListFilesAsync("users", "all_*.json");
                var latestFile = files.OrderByDescending(f => f).FirstOrDefault();

                if (!string.IsNullOrEmpty(latestFile))
                {
                    return await _jsonStorage.ReadAsync<List<User>>($"users/{latestFile}")
                        ?? new List<User>();
                }

                return new List<User>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载用户数据失败");
                return new List<User>();
            }
        }

        public async Task SaveUserAsync(User user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                var filePath = $"users/individual/{user.UserId}.json";
                await _jsonStorage.WriteAsync(filePath, user);

                _logger.LogDebug($"保存用户到文件: {user.UserId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存用户数据失败: {user?.UserId}");
                throw;
            }
        }

        public async Task<User> LoadUserAsync(string userId)
        {
            try
            {
                var filePath = $"users/individual/{userId}.json";

                if (await _jsonStorage.FileExistsAsync(filePath))
                {
                    return await _jsonStorage.ReadAsync<User>(filePath);
                }

                _logger.LogWarning($"用户文件不存在: {userId}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"加载用户数据失败: {userId}");
                return null;
            }
        }

        public async Task SaveUserSessionsAsync(string userId, IEnumerable<Session> sessions)
        {
            try
            {
                var sessionList = sessions?.ToList() ?? new List<Session>();
                var date = DateTime.UtcNow.ToString("yyyyMMdd");
                var filePath = $"sessions/user/{userId}/{date}.json";

                await _jsonStorage.WriteAsync(filePath, sessionList);

                _logger.LogDebug($"保存用户充电会话: {userId}, 记录数: {sessionList.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存用户充电会话失败: {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<Session>> LoadUserSessionsAsync(
            string userId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var directoryPath = $"sessions/user/{userId}";
                var allSessions = new List<Session>();

                if (!await _jsonStorage.FileExistsAsync(directoryPath))
                {
                    return allSessions;
                }

                var files = await _jsonStorage.ListFilesAsync(directoryPath, "*.json");

                foreach (var file in files.OrderBy(f => f))
                {
                    try
                    {
                        var fileDate = Path.GetFileNameWithoutExtension(file);
                        if (DateTime.TryParseExact(fileDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var date))
                        {
                            // 过滤日期范围
                            if (startDate.HasValue && date.Date < startDate.Value.Date)
                                continue;

                            if (endDate.HasValue && date.Date > endDate.Value.Date)
                                continue;

                            var sessions = await _jsonStorage.ReadAsync<List<Session>>(
                                Path.Combine(directoryPath, file));

                            if (sessions != null)
                            {
                                allSessions.AddRange(sessions);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"加载会话文件失败: {file}");
                    }
                }

                return allSessions.OrderByDescending(s => s.StartTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"加载用户充电会话失败: {userId}");
                return new List<Session>();
            }
        }

        public async Task SaveUserTransactionsAsync(string userId, IEnumerable<Transaction> transactions)
        {
            try
            {
                var transactionList = transactions?.ToList() ?? new List<Transaction>();
                var date = DateTime.UtcNow.ToString("yyyyMMdd");
                var filePath = $"transactions/user/{userId}/{date}.json";

                await _jsonStorage.WriteAsync(filePath, transactionList);

                _logger.LogDebug($"保存用户交易记录: {userId}, 记录数: {transactionList.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存用户交易记录失败: {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<Transaction>> LoadUserTransactionsAsync(
            string userId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var directoryPath = $"transactions/user/{userId}";
                var allTransactions = new List<Transaction>();

                if (!await _jsonStorage.FileExistsAsync(directoryPath))
                {
                    return allTransactions;
                }

                var files = await _jsonStorage.ListFilesAsync(directoryPath, "*.json");

                foreach (var file in files.OrderBy(f => f))
                {
                    try
                    {
                        var fileDate = Path.GetFileNameWithoutExtension(file);
                        if (DateTime.TryParseExact(fileDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var date))
                        {
                            // 过滤日期范围
                            if (startDate.HasValue && date.Date < startDate.Value.Date)
                                continue;

                            if (endDate.HasValue && date.Date > endDate.Value.Date)
                                continue;

                            var transactions = await _jsonStorage.ReadAsync<List<Transaction>>(
                                Path.Combine(directoryPath, file));

                            if (transactions != null)
                            {
                                allTransactions.AddRange(transactions);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"加载交易文件失败: {file}");
                    }
                }

                return allTransactions.OrderByDescending(t => t.TransactionTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"加载用户交易记录失败: {userId}");
                return new List<Transaction>();
            }
        }
    }
}