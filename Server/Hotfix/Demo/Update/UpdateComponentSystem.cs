using System;
using System.IO;
using System.Collections.Generic;
namespace ET.Server
{
    [EntitySystemOf(typeof(UpdateComponent))]
    [FriendOf(typeof(UpdateComponent))]
    public static partial class UpdateComponentSystem
    {
        private const string ConfigPath = "../Config/UpdateConfig.json";

        [EntitySystem]
        private static void Awake(this UpdateComponent self)
        {
            LoadConfig(self);
        }
        
        public static void LoadConfig(this UpdateComponent self)
        {
            if (!File.Exists(ConfigPath))
            {
                Log.Warning($"UpdateConfig.json not found at {ConfigPath}");
                return;
            }

            try
            {
                //todo 配置文件改成使用Excel 
                string json = File.ReadAllText(ConfigPath);
                self.UpdateConfig = MongoHelper.FromJson<UpdateConfig>(json);
                Log.Info("UpdateConfig loaded successfully");
            }
            catch (Exception e)
            {
                Log.Error($"Failed to load UpdateConfig: {e.Message}");
            }
           
        }

        
        /// <summary>
        /// 从MongoDB加载所有区服状态和白名单到内存
        /// </summary>
        public static async ETTask LoadAllVersion(this UpdateComponent self)
        {
            foreach (AppVersionItem  versionItem in self.UpdateConfig.AppVersionList)
            {
                await self.GetUpdateVersion(versionItem.Version);
            }
            foreach (ResourceItem  versionItem in self.UpdateConfig.ResourceList)
            {
                await self.GetUpdateVersion(versionItem.Version.ToString());
            }
        }
        
        private static async ETTask<UpdateVersion> GetUpdateVersion(this UpdateComponent self, string version)
        {
            
            //如果部署多个就不能缓存内存白名单，一般只部署一个
            long versionId = version.GetLongHashCode();
            UpdateVersion updateVersion = self.GetChild<UpdateVersion>(versionId);
            if (updateVersion != null)
            {
                return updateVersion;
            }

            DBComponent dbComponent = self.Scene().GetComponent<DBManagerComponent>().GetZoneDB(self.Root().Zone());
            updateVersion = await dbComponent.Query<UpdateVersion>(versionId);
            if (updateVersion != null)
            {
                self.AddChild(updateVersion);
            }
            return updateVersion;
        }

        private static async ETTask<UpdateVersion> GetOrCreateUpdateVersion(this UpdateComponent self, string version)
        {
            //如果部署多个就不能缓存内存白名单，一般只部署一个
            long versionId = version.GetLongHashCode();
            UpdateVersion updateVersion = await self.GetUpdateVersion(version);
            if (updateVersion == null)
            {
                updateVersion = self.AddChildWithId<UpdateVersion>(versionId);
                updateVersion.Version = version;
            }
            return updateVersion;
        }
        
        
        // ==================== 白名单相关 ====================

        /// <summary>
        /// 检查IP是否在白名单中：该版本配置了名单 → 名单内才放行；版本无名单 → 不限制（全放行）
        /// </summary>
        public static async ETTask<bool> IsIPWhitelisted(this UpdateComponent self, string version, string ip)
        {
            UpdateVersion updateVersion = await self.GetUpdateVersion(version);
            HashSet<string> whitelistIPs = updateVersion?.WhitelistIPs;
            return  whitelistIPs.Count == 0 || whitelistIPs.Contains(ip);
        }

 
        
        /// <summary>
        /// 添加白名单IP并持久化
        /// </summary>
        public static async ETTask AddWhitelistIP(this UpdateComponent self,string version, string ip)
        {
            UpdateVersion updateVersion = await self.GetOrCreateUpdateVersion(version);
            updateVersion.WhitelistIPs.Add(ip);
            DBComponent dbComponent = self.Scene().GetComponent<DBManagerComponent>().GetZoneDB(self.Root().Zone());
            await dbComponent.Save(updateVersion);
        }

        /// <summary>
        /// 移除白名单IP并持久化
        /// </summary>
        public static async ETTask RemoveWhitelistIP(this UpdateComponent self, string version, string ip)
        {
            UpdateVersion updateVersion = await self.GetUpdateVersion(version);
            if (updateVersion?.WhitelistIPs.Count == 0)
                return; 
            updateVersion.WhitelistIPs.Remove(ip);
            DBComponent dbComponent = self.Scene().GetComponent<DBManagerComponent>().GetZoneDB(self.Root().Zone());
            await dbComponent.Save(updateVersion);
        }

        /// <summary>
        /// 清除白名单
        /// </summary>
        public static async ETTask ClearAllWhitelist(this UpdateComponent self)
        {
            foreach (AppVersionItem  versionItem in self.UpdateConfig.AppVersionList)
            {
                await self.ClearWhitelist(versionItem.Version);
            }
            foreach (ResourceItem  versionItem in self.UpdateConfig.ResourceList)
            {
                await self.ClearWhitelist(versionItem.Version.ToString());
            }
        }
        

        /// <summary>
        /// 清除白名单：指定version清除更新
        /// </summary>
        public static async ETTask ClearWhitelist(this UpdateComponent self, string version)
        {
            UpdateVersion updateVersion = await self.GetUpdateVersion(version);
            if (updateVersion?.WhitelistIPs.Count == 0)
                return; 
            updateVersion.WhitelistIPs.Clear();
            DBComponent dbComponent = self.Scene().GetComponent<DBManagerComponent>().GetZoneDB(self.Root().Zone());
            await dbComponent.Save(updateVersion);
            
        }
    }
}