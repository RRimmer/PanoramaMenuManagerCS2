using Dapper;
using MenuManager;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MenuManagerCore;

public class DataBaseService
{
    private readonly PluginConfig _config;
    private readonly string _connectionString;
    private readonly ILogger<DataBaseService> _logger;
    private readonly bool _useSqlite;

    public DataBaseService(PluginConfig config, string modulePath)
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<DataBaseService>();
        _config = config;

        if (string.IsNullOrWhiteSpace(_config.DatabaseHost) ||
            string.IsNullOrWhiteSpace(_config.DatabaseUser) ||
            string.IsNullOrWhiteSpace(_config.DatabasePassword) ||
            string.IsNullOrWhiteSpace(_config.DatabaseName))
        {
            _useSqlite = true;
            PrepareSqliteNative(modulePath);
            SQLitePCL.Batteries.Init();
            string sqliteDbFile = Path.Combine(modulePath, "menumanager.db");
            _connectionString = $"Data Source={sqliteDbFile}";
            _logger.LogWarning("MySQL configuration missing. Using SQLite database at: {Path}", sqliteDbFile);
        }
        else
        {
            _useSqlite = false;
            _connectionString = BuildMySqlConnectionString();
        }
    }

    private void PrepareSqliteNative(string modulePath)
    {
        string nativePath = ResolveSqliteNativePath(modulePath);
        if (!File.Exists(nativePath))
        {
            _logger.LogError("SQLite native library was not found. Expected {Path}", nativePath);
            return;
        }

        Assembly provider = typeof(SQLitePCL.SQLite3Provider_e_sqlite3).Assembly;
        try
        {
            NativeLibrary.SetDllImportResolver(provider, (libraryName, _, _) =>
            {
                if (libraryName == "e_sqlite3")
                {
                    return NativeLibrary.Load(nativePath);
                }

                return IntPtr.Zero;
            });
        }
        catch (InvalidOperationException)
        {
            _logger.LogInformation("SQLite native resolver is already set.");
        }

        _logger.LogInformation("SQLite native library: {Path}", nativePath);
    }

    private static string ResolveSqliteNativePath(string modulePath)
    {
        string rid;
        string fileName;
        if (OperatingSystem.IsWindows())
        {
            rid = "win-x64";
            fileName = "e_sqlite3.dll";
        }
        else if (File.Exists("/lib/ld-musl-x86_64.so.1"))
        {
            rid = "linux-musl-x64";
            fileName = "libe_sqlite3.so";
        }
        else
        {
            rid = "linux-x64";
            fileName = "libe_sqlite3.so";
        }

        string runtimePath = Path.Combine(modulePath, "runtimes", rid, "native", fileName);
        if (File.Exists(runtimePath))
        {
            return runtimePath;
        }

        string beside = Path.Combine(modulePath, fileName);
        if (File.Exists(beside))
        {
            return beside;
        }

        return runtimePath;
    }

    private string BuildMySqlConnectionString()
    {
        MySqlConnectionStringBuilder builder = new()
        {
            Server = _config.DatabaseHost,
            Port = (uint)_config.DatabasePort,
            UserID = _config.DatabaseUser,
            Password = _config.DatabasePassword,
            Database = _config.DatabaseName,
            Pooling = true
        };

        return builder.ConnectionString;
    }

    private Task<IDbConnection> GetOpenConnectionAsync()
    {
        try
        {
            IDbConnection connection = _useSqlite ? new SqliteConnection(_connectionString) : new MySqlConnection(_connectionString);
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            return Task.FromResult(connection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while opening database connection ({DbType})", _useSqlite ? "SQLite" : "MySQL");
            throw;
        }
    }

    public async Task TestAndCheckDataBaseTableAsync()
    {
        try
        {
            using IDbConnection connection = await GetOpenConnectionAsync();

            if (_useSqlite)
            {
                const string createTableQuery = """
                                                CREATE TABLE IF NOT EXISTS player_menus (
                                                    steamid INTEGER PRIMARY KEY,
                                                    menu_type TEXT NOT NULL DEFAULT 'Default',
                                                    pagination INTEGER NULL DEFAULT NULL,
                                                    sounds_enabled INTEGER NULL DEFAULT NULL,
                                                    volume REAL NULL DEFAULT NULL,
                                                    notifications INTEGER NULL DEFAULT NULL,
                                                    menu_position TEXT NULL DEFAULT NULL,
                                                    csgo_dead_hint INTEGER NULL DEFAULT NULL,
                                                    menu_chosen INTEGER NULL DEFAULT NULL
                                                );
                                                """;
                _ = await connection.ExecuteAsync(createTableQuery);
                await EnsureColumnAsync(connection, "notifications", "INTEGER", "TINYINT");
                await EnsureColumnAsync(connection, "menu_position", "TEXT", "VARCHAR(16)");
                await EnsureColumnAsync(connection, "csgo_dead_hint", "INTEGER", "TINYINT");
                await EnsureColumnAsync(connection, "menu_chosen", "INTEGER", "TINYINT");
                _logger.LogInformation("SQLite Database checked/created successfully.");
            }
            else
            {
                bool tableExists = await connection.QueryFirstOrDefaultAsync<string>(
                    "SHOW TABLES LIKE 'player_menus';") != null;

                if (!tableExists)
                {
                    const string createTableQuery = """
                                                    CREATE TABLE `player_menus` (
                                                        `steamid` BIGINT UNSIGNED PRIMARY KEY, 
                                                        `menu_type` VARCHAR(64) NOT NULL DEFAULT 'Default',
                                                        `pagination` TINYINT NULL DEFAULT NULL,
                                                        `sounds_enabled` TINYINT NULL DEFAULT NULL,
                                                        `volume` FLOAT NULL DEFAULT NULL,
                                                        `notifications` TINYINT NULL DEFAULT NULL,
                                                        `menu_position` VARCHAR(16) NULL DEFAULT NULL,
                                                        `csgo_dead_hint` TINYINT NULL DEFAULT NULL,
                                                        `menu_chosen` TINYINT NULL DEFAULT NULL
                                                    );
                                                    """;

                    _ = await connection.ExecuteAsync(createTableQuery);
                    _logger.LogInformation("Table 'player_menus' created successfully (MySQL).");
                }

                await EnsureColumnAsync(connection, "notifications", "INTEGER", "TINYINT");
                await EnsureColumnAsync(connection, "menu_position", "TEXT", "VARCHAR(16)");
                await EnsureColumnAsync(connection, "csgo_dead_hint", "INTEGER", "TINYINT");
                await EnsureColumnAsync(connection, "menu_chosen", "INTEGER", "TINYINT");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection failed or table creation error");
        }
    }

    public async Task<ConcurrentDictionary<ulong, PlayerSettings>> LoadAllMenuSettings()
    {
        ConcurrentDictionary<ulong, PlayerSettings> result = new();
        try
        {
            using IDbConnection connection = await GetOpenConnectionAsync();

            IEnumerable<dynamic> rows = await connection.QueryAsync("SELECT * FROM player_menus");

            foreach (dynamic row in rows)
            {
                dynamic steamId = Convert.ToUInt64(row.steamid);
                PlayerSettings settings = new();

                string typeStr = row.menu_type.ToString();
                settings.MenuType = Enum.TryParse(typeStr, true, out MenuType parsedType)
                    ? parsedType
                    : MenuType.Default;

                if (row.pagination != null)
                {
                    settings.UsePagination = Convert.ToInt32(row.pagination) == 1;
                }

                if (row.sounds_enabled != null)
                {
                    settings.SoundsEnabled = Convert.ToInt32(row.sounds_enabled) == 1;
                }

                if (row.volume != null)
                {
                    settings.Volume = Convert.ToSingle(row.volume);
                }

                if (TryRead(row, "notifications", out object? notifications))
                {
                    settings.Notifications = Convert.ToInt32(notifications) == 1;
                }

                if (TryRead(row, "menu_position", out object? position))
                {
                    settings.MenuPosition = Misc.NormalizePosition(Convert.ToString(position));
                }

                if (TryRead(row, "csgo_dead_hint", out object? deadHint))
                {
                    settings.CsgoDeadHint = Convert.ToInt32(deadHint) == 1;
                }

                if (TryRead(row, "menu_chosen", out object? menuChosen))
                {
                    settings.MenuChosen = Convert.ToInt32(menuChosen) == 1;
                }

                result.TryAdd(steamId, settings);
            }

            _logger.LogInformation("Loaded {ResultCount} player menu preferences from {DbType}.", result.Count,
                _useSqlite ? "SQLite" : "MySQL");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading menu settings");
        }

        return result;
    }

    public async Task SaveMenuSetting(ulong steamId, PlayerSettings settings)
    {
        try
        {
            using IDbConnection connection = await GetOpenConnectionAsync();

            string query = _useSqlite
                ? """
                  INSERT OR REPLACE INTO player_menus (steamid, menu_type, pagination, sounds_enabled, volume, notifications, menu_position, csgo_dead_hint, menu_chosen) 
                  VALUES (@SteamId, @MenuType, @Pagination, @SoundsEnabled, @Volume, @Notifications, @MenuPosition, @CsgoDeadHint, @MenuChosen);
                  """
                : """
                  INSERT INTO `player_menus` (`steamid`, `menu_type`, `pagination`, `sounds_enabled`, `volume`, `notifications`, `menu_position`, `csgo_dead_hint`, `menu_chosen`) 
                  VALUES (@SteamId, @MenuType, @Pagination, @SoundsEnabled, @Volume, @Notifications, @MenuPosition, @CsgoDeadHint, @MenuChosen)
                  ON DUPLICATE KEY UPDATE 
                     `menu_type` = @MenuType,
                     `pagination` = @Pagination,
                     `sounds_enabled` = @SoundsEnabled,
                     `volume` = @Volume,
                     `notifications` = @Notifications,
                     `menu_position` = @MenuPosition,
                     `csgo_dead_hint` = @CsgoDeadHint,
                     `menu_chosen` = @MenuChosen;
                  """;

            _ = await connection.ExecuteAsync(query, new
            {
                SteamId = steamId,
                MenuType = settings.MenuType.ToString(),
                Pagination = settings.UsePagination == true ? 1 : 0,
                SoundsEnabled = settings.SoundsEnabled == true ? 1 : 0,
                settings.Volume,
                Notifications = settings.Notifications.HasValue ? (int?)(settings.Notifications.Value ? 1 : 0) : null,
                settings.MenuPosition,
                CsgoDeadHint = settings.CsgoDeadHint.HasValue ? (int?)(settings.CsgoDeadHint.Value ? 1 : 0) : null,
                MenuChosen = settings.MenuChosen ? 1 : 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving menu setting for SteamID {SteamId}", steamId);
        }
    }

    private async Task EnsureColumnAsync(IDbConnection connection, string name, string sqliteType, string mySqlType)
    {
        try
        {
            string sql = _useSqlite
                ? $"ALTER TABLE player_menus ADD COLUMN {name} {sqliteType} NULL DEFAULT NULL"
                : $"ALTER TABLE `player_menus` ADD COLUMN `{name}` {mySqlType} NULL DEFAULT NULL";
            _ = await connection.ExecuteAsync(sql);
        }
        catch (Exception ex) when (ex.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                                   ex.Message.Contains("exists", StringComparison.OrdinalIgnoreCase))
        {
        }
    }

    private static bool TryRead(dynamic row, string name, out object? value)
    {
        if (row is IDictionary<string, object> fields)
        {
            foreach (KeyValuePair<string, object> field in fields)
            {
                if (!field.Key.Equals(name, StringComparison.OrdinalIgnoreCase) || field.Value is null or DBNull)
                {
                    continue;
                }

                value = field.Value;
                return true;
            }
        }

        value = null;
        return false;
    }
}