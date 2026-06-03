// ChargingStationManagement.Infrastructure/FileStorage/JsonFileStorage.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ChargingStationManagement.Infrastructure.FileStorage
{
    public interface IJsonFileStorage
    {
        Task<T> ReadAsync<T>(string filePath);
        Task WriteAsync<T>(string filePath, T data);
        Task AppendAsync<T>(string filePath, T data);
        Task<bool> FileExistsAsync(string filePath);
        Task DeleteAsync(string filePath);
        Task<IEnumerable<string>> ListFilesAsync(string directoryPath, string searchPattern = "*");
    }

    public class JsonFileStorage : IJsonFileStorage
    {
        private readonly ILogger<JsonFileStorage> _logger;
        private readonly JsonFileStorageSettings _settings;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonFileStorage(
            IOptions<JsonFileStorageSettings> settings,
            ILogger<JsonFileStorage> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        public async Task<T> ReadAsync<T>(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);

                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning($"文件不存在: {fullPath}");
                    return default;
                }

                var json = await File.ReadAllTextAsync(fullPath);
                var result = JsonSerializer.Deserialize<T>(json, _jsonOptions);

                _logger.LogDebug($"从文件读取数据: {fullPath}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取JSON文件失败: {filePath}");
                throw;
            }
        }

        public async Task WriteAsync<T>(string filePath, T data)
        {
            try
            {
                var fullPath = GetFullPath(filePath);
                var directory = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogDebug($"创建目录: {directory}");
                }

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                await File.WriteAllTextAsync(fullPath, json);

                _logger.LogDebug($"写入JSON文件: {fullPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"写入JSON文件失败: {filePath}");
                throw;
            }
        }

        public async Task AppendAsync<T>(string filePath, T data)
        {
            try
            {
                var fullPath = GetFullPath(filePath);
                var directory = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(data, _jsonOptions);

                // 如果文件存在，先读取现有数据
                if (File.Exists(fullPath))
                {
                    var existingJson = await File.ReadAllTextAsync(fullPath);
                    if (!string.IsNullOrEmpty(existingJson))
                    {
                        // 追加到数组
                        json = existingJson.TrimEnd(']') + "," + json.TrimStart('[') + "]";
                    }
                }
                else
                {
                    // 新文件，创建数组
                    json = "[" + json + "]";
                }

                await File.WriteAllTextAsync(fullPath, json);
                _logger.LogDebug($"追加到JSON文件: {fullPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"追加到JSON文件失败: {filePath}");
                throw;
            }
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            var fullPath = GetFullPath(filePath);
            return Task.FromResult(File.Exists(fullPath));
        }

        public Task DeleteAsync(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogDebug($"删除文件: {fullPath}");
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除文件失败: {filePath}");
                throw;
            }
        }

        public Task<IEnumerable<string>> ListFilesAsync(string directoryPath, string searchPattern = "*")
        {
            try
            {
                var fullPath = GetFullPath(directoryPath);

                if (!Directory.Exists(fullPath))
                {
                    return Task.FromResult(Enumerable.Empty<string>());
                }

                var files = Directory.GetFiles(fullPath, searchPattern)
                    .Select(f => Path.GetRelativePath(fullPath, f));

                return Task.FromResult(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"列出文件失败: {directoryPath}");
                throw;
            }
        }

        private string GetFullPath(string filePath)
        {
            if (Path.IsPathRooted(filePath))
            {
                return filePath;
            }

            return Path.Combine(_settings.BasePath, filePath);
        }
    }

    public class JsonFileStorageSettings
    {
        public string BasePath { get; set; } = "Data/JsonStorage";

        // 备份配置
        public bool EnableBackup { get; set; } = true;
        public int MaxBackupFiles { get; set; } = 10;
        public string BackupPath { get; set; } = "Data/Backup";

        // 压缩配置
        public bool EnableCompression { get; set; } = false;
        public long CompressionThreshold { get; set; } = 1024 * 1024; // 1MB
    }
}