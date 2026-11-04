

namespace ET.Server
{
	[MessageLocationHandler(SceneType.Map)]
	public class G2M_SessionDisconnectHandler : MessageLocationHandler<Unit, G2M_SessionDisconnect>
	{
		protected override async ETTask Run(Unit unit, G2M_SessionDisconnect message)
		{
			
			DBComponent dbComponent = unit.Root().GetComponent<DBManagerComponent>().GetZoneDB(unit.ZoneId);
			await dbComponent.Save(unit);
			unit.Dispose();
			
		}
	}
}