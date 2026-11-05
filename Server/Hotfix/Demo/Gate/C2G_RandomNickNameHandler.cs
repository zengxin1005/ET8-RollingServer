namespace ET.Server
{
	[MessageSessionHandler(SceneType.Gate)]
	public class C2G_RandomNickNameHandler : MessageSessionHandler<C2G_RandomNickName, G2C_RandomNickName>
	{
		protected override async ETTask Run(Session session, C2G_RandomNickName request, G2C_RandomNickName response)
		{
			if (!NickNameRandomPool.Instance.TryRandom(request.Sex, out string nickName))
			{
				response.Error = ErrorCore.ERR_NickNamePoolEmpty;
				response.Message = "当前性别暂无可用随机昵称，请手动输入";
				return;
			}
			response.NickName = nickName;
			await ETTask.CompletedTask;
		}
	}
}
