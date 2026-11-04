using System;


namespace ET.Server
{
	[MessageHandler(SceneType.Gate)]
	public class R2G_GetLoginKeyHandler : MessageHandler<Scene, R2G_GetLoginKey, G2R_GetLoginKey>
	{
		protected override async ETTask Run(Scene scene, R2G_GetLoginKey request, G2R_GetLoginKey response)
		{
			long key = RandomGenerator.RandInt64();
			GateSessionKeyComponent gateSessionKeyComponent = scene.GetComponent<GateSessionKeyComponent>();
			while (gateSessionKeyComponent.Get(key) != null)   // 20s 窗口内存量很小，基本不进循环
			{
				key = RandomGenerator.RandInt64();
			}
			gateSessionKeyComponent.Add(key, request.Account);
			response.Key = key;
			response.GateId = scene.Id;
			await ETTask.CompletedTask;
		}
	}
}