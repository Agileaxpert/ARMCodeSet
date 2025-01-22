using ARMCommon.Interface;
using StackExchange.Redis;
using System.Text;
namespace ARMCommon.Helpers
{
    public class RedisHelper : IRedisHelper
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IConfiguration _configuration;
        public RedisHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            _redis = ConnectionMultiplexer.Connect(GetRedisConfig());
        }

        public string StringGet(string key)
        {
            var db = _redis.GetDatabase();
            return db.StringGet(key);
        }

        public bool StringSet(string key, string value, int expiry = 0)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            
            var db = _redis.GetDatabase();
            if (expiry > 0)
            {
                return db.StringSet(key, bytes, TimeSpan.FromSeconds(expiry));
            }
            else
            {
                return db.StringSet(key, bytes);
            }
        }
        public async Task<string> StringGetAsync(string key)
        {
            var db = _redis.GetDatabase();
            return await db.StringGetAsync(key);
        }
        public async Task<bool> StringSetAsync(string key, string value, int expiry = 0)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);

            var db = _redis.GetDatabase();
            if (expiry > 0)
            {
                return await db.StringSetAsync(key, bytes, TimeSpan.FromSeconds(expiry));
            }
            else
            {
                return await db.StringSetAsync(key, bytes);
            }
        }
        public bool KeyDelete(string key)
        {
            var db = _redis.GetDatabase();
            return db.KeyDelete(key);
        }

        public async Task<bool> DeleteKeysByPatternAsync(string pattern)
        {
            var multiplexer = await ConnectionMultiplexer.ConnectAsync(GetRedisConfig());
            var db = multiplexer.GetDatabase();

            var keysToDelete = new List<RedisKey>();

            var server = multiplexer.GetServer(GetRedisConfig().EndPoints.First());
            var cursor = 0L;
            do
            {
                var scanResult = server.Execute("SCAN", cursor.ToString(), "MATCH", pattern + "*");
                var result = (RedisResult[])scanResult;

                cursor = (long)result[0];
                var keys = (RedisKey[])result[1];

                keysToDelete.AddRange(keys);
            } while (cursor != 0);

            if (keysToDelete.Any())
            {
                var deletedCount = await db.KeyDeleteAsync(keysToDelete.ToArray());
                return deletedCount > 0;
            }

            return false;
        }

        public bool KeyExists(string key)
        {
            var db = _redis.GetDatabase();
            return db.KeyExists(key);
        }
        public async Task<bool> KeyExistsAsync(string key)
        {
            var db = _redis.GetDatabase();
            return await db.KeyExistsAsync(key);
        }
        public async Task<bool> KeyDeleteAsync(string key)
        {
            var db = _redis.GetDatabase();
            return await db.KeyDeleteAsync(key);
        }
        public long KeysDelete(RedisKey[] keys)
        {
            var db = _redis.GetDatabase();
            return db.KeyDelete(keys);
        }
        public async Task<long> KeysDeleteAsync(RedisKey[] keys)
        {
            var db = _redis.GetDatabase();
            var result = await db.KeyDeleteAsync(keys);
            return result;
        }
        public string HashGet(string key, string field)
        {
            var db = _redis.GetDatabase();
            return db.HashGet(key, field);
        }
        public async Task<string> HashGetAsync(string key, string field)
        {
            var db = _redis.GetDatabase();
            return await db.HashGetAsync(key, field);
        }
        public HashEntry[] HashGetAll(string key)
        {
            var db = _redis.GetDatabase();
            return db.HashGetAll(key);
        }
        public async Task<HashEntry[]> HashGetAllAsync(string key)
        {
            var db = _redis.GetDatabase();
            return await db.HashGetAllAsync(key);
        }

        public async Task<Dictionary<string, string>> HashGetAllDictAsync(string key)
        {
            var db = _redis.GetDatabase();
            var allfields = await db.HashGetAllAsync(key);
            var dict = new Dictionary<string, string>();
            if (allfields != null)
            {
                foreach (var entry in allfields)
                {
                    dict.Add(entry.Name, entry.Value.ToString());
                }
            }
            return dict;
        }

        public bool HashSet(string key, string field, string value,  int expiry = 0)
        {
            var db = _redis.GetDatabase();
            var result = db.HashSet(key, field, value);
            if (expiry > 0)
            {
                db.KeyExpireAsync(key, TimeSpan.FromSeconds(expiry));
            }
            return result;
        }
        public async Task<bool> HashSetAsync(string key, string field, string value,  int expiry = 0)
        {
            var db = _redis.GetDatabase();
            var result = await db.HashSetAsync(key, field, value);
            if (expiry > 0)
            {
                await db.KeyExpireAsync(key, TimeSpan.FromSeconds(expiry));
            }
            return result;
        }
        public bool HashSetEntries(string key, HashEntry[] hashEntries, int expiry = 0)
        {
            var db = _redis.GetDatabase();
            db.HashSetAsync(key, hashEntries);
            if (expiry > 0)
            {
                db.KeyExpire(key, TimeSpan.FromSeconds(expiry));
            }
            return true;
        }
        public async Task<bool> HashSetEntriesAsync(string key, HashEntry[] hashEntries, int expiry = 0)
        {
            var db = _redis.GetDatabase();
            await db.HashSetAsync(key, hashEntries);
            if (expiry > 0)
            {
                await db.KeyExpireAsync(key, TimeSpan.FromSeconds(expiry));
            }
            return true;
        }

        
        public RedisType KeyType(string key)
        {
            var db = _redis.GetDatabase();
            return db.KeyType(key);
        }
        public async Task<RedisType> KeyTypeAsync(string key)
        {
            var db = _redis.GetDatabase();
            return await db.KeyTypeAsync(key);
        }
        public RedisResult Execute(string command)
        {
            var db = _redis.GetDatabase();
            return db.Execute(command);
        }
        public async Task<RedisResult> ExecuteAsync(string command)
        {
            var db = _redis.GetDatabase();
            var result = await db.ExecuteAsync(command);
            return result;
        }
        public ConfigurationOptions GetRedisConfig()
        {
            string redisHost = _configuration["AppConfig:RedisHost"];
            string redisPass = _configuration["AppConfig:RedisPassword"];

            HashSet<string> redisCommands = new HashSet<string>
                {
                    "CLUSTER",
                    "PING", "ECHO", "CLIENT",
                    "SUBSCRIBE", "UNSUBSCRIBE", "NULL",
                    "INFO", "CONFIG"
                };

            ConfigurationOptions config = new ConfigurationOptions
            {
                SyncTimeout = int.MaxValue,
                KeepAlive = 60,
                Password = redisPass,
                AbortOnConnectFail = true,
                AllowAdmin = true,
                CommandMap = CommandMap.Create(redisCommands, available: false)
            };

            config.EndPoints.Add(redisHost);

            return config;
        }


    }
}
