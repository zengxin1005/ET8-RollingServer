namespace ET.Server
{
	[MessageSessionHandler(SceneType.Gate)]
	public class C2G_RandomNickNameHandler : MessageSessionHandler<C2G_RandomNickName, G2C_RandomNickName>
	{
		protected override async ETTask Run(Session session, C2G_RandomNickName request, G2C_RandomNickName response)
		{
			//todo 配置文件改成使用Excel读取
			char[] Letters =
			{
				'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
				'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
			};
			int length = RandomGenerator.RandomNumber(4, 9); // [4, 9) ⇒ 4~8 个字母
			System.Text.StringBuilder sb = new(length);
			for (int i = 0; i < length; i++)
			{
				sb.Append(RandomGenerator.RandomArray(Letters));
			}

			response.NickName = sb.ToString();
			
			await ETTask.CompletedTask;
		}
	}
}
