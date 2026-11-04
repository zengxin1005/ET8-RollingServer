using System;

namespace ET.Server
{
    [FriendOf(typeof(DBManagerComponent))]
    public static partial class DBManagerComponentSystem
    {
        public static DBComponent GetZoneDB(this DBManagerComponent self, int zone)
        {
            DBComponent dbComponent = self.GetChild<DBComponent>(zone);
            if (dbComponent != null)
            {
                return dbComponent;
            }

            StartZoneConfig startZoneConfig = StartZoneConfigCategory.Instance.Get(zone);
            if (startZoneConfig.DBConnection == "")
            {
                throw new Exception($"zone: {zone} not found mongo connect string");
            }

            dbComponent = self.AddChildWithId<DBComponent, string, string>(zone, startZoneConfig.DBConnection, startZoneConfig.DBName);
            return dbComponent;
        }
        
        
        
        public static MySQLComponent GetZoneMySQL(this DBManagerComponent self, int zone)
        {
            MySQLComponent mySQLComponent = self.GetChild<MySQLComponent>(zone);
            if (mySQLComponent != null)
            {
                return mySQLComponent;
            }
            StartZoneConfig startZoneConfig = StartZoneConfigCategory.Instance.Get(zone);
            if (startZoneConfig.DBConnection == "")
            {
                throw new Exception($"zone: {zone} not found mysql connect string");
            }

            mySQLComponent = self.AddChildWithId<MySQLComponent, string, string>(zone, startZoneConfig.DBConnection, startZoneConfig.DBName);
                
            return mySQLComponent;
        }
        
        public static MySQLComponent CreateZoneMySQL(this DBManagerComponent self, int zone, string connectionString, string dbName)
        {
            MySQLComponent mySQLComponent = self.GetChild<MySQLComponent>(zone);
            if (mySQLComponent != null)
            {
                throw new Exception($"zone: {zone} already created mySQL");
            }
            //连接配置示例
            // "DBConnection": "Server=localhost;Port=3306;Database=game_db;Uid=root;Pwd=123456;Pooling=true;Min Pool Size=5;Max Pool Size=100;",
            // "DBName": "game_db"
            mySQLComponent = self.AddChildWithId<MySQLComponent, string, string>(zone, connectionString, dbName);
            return mySQLComponent;
        }
        
        
        public static RedisComponent GetZoneRedis(this DBManagerComponent self, int zone)
        {
            RedisComponent redisComponent = self.GetChild<RedisComponent>(zone);
            if (redisComponent != null)
            {
                return redisComponent;
            }
            return null;
        }
        public static RedisComponent CreateZoneRedis(this DBManagerComponent self, int zone, string connectionString, int databaseNumber = 0)
        {
            /*
             connectionString 说明：
            1. 单机模式（最常用）
      
            "127.0.0.1:6379,password=123456,defaultDatabase=0,connectTimeout=5000,syncTimeout=5000"
            2. 无密码（本地开发）
  
            "127.0.0.1:6379,defaultDatabase=0"
            3. 带密码和连接池
 
            "127.0.0.1:6379,password=123456,defaultDatabase=0,connectTimeout=5000,syncTimeout=5000,abortConnect=false"
            4. 集群模式

            "server1:6379,server2:6379,server3:6379,password=123456,defaultDatabase=0,abortConnect=false"
            5. 云服务（如阿里云）
            "r-xxxxxxxxxx.redis.rds.aliyuncs.com:6379,password=xxxxxx,defaultDatabase=0,ssl=true"
             
            databaseNumber 说明：
            Redis 默认有 16 个数据库（0-15），建议：

            0：缓存数据（可丢失）

            1：业务数据（排行榜、在线状态）

            2：分布式锁

            3：消息队列

            4-15：备用
            */
            RedisComponent redisComponent = self.GetChild<RedisComponent>(zone);
            if (redisComponent != null)
            {
                throw new Exception($"zone: {zone} already created redis");
            }
            return self.AddChildWithId<RedisComponent, string, int>(zone, connectionString, databaseNumber);
        }
        
        
        public static ClickHouseComponent GetZoneClickHouse(this DBManagerComponent self, int zone)
        {
            ClickHouseComponent clickHouseComponent = self.GetChild<ClickHouseComponent>(zone);
            if (clickHouseComponent != null)
            {
                return clickHouseComponent;
            }
            return null;
        }
        public static ClickHouseComponent CreateZoneClickHouse(this DBManagerComponent self, int zone, string connectionString, string dbName = "default")
        {
            /*
             connectionString 说明：
            "Host=127.0.0.1;Port=8123;Database=game_analytics;User=default;Password=;"
            */
            ClickHouseComponent clickHouseComponent = self.GetChild<ClickHouseComponent>(zone);
            if (clickHouseComponent != null)
            {
                throw new Exception($"zone: {zone} already created clickHouse");
            }
            return self.AddChildWithId<ClickHouseComponent, string,string>(zone, connectionString, dbName);
        }
        
    }
}