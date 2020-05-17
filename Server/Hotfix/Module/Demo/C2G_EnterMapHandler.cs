using System;
using System.Net;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Gate)]
	public class C2G_EnterMapHandler : AMRpcHandler<C2G_EnterMap, G2C_EnterMap>
	{
		protected override async ETTask Run(Session session, C2G_EnterMap request, G2C_EnterMap response, Action reply)
		{
			Player player = session.GetComponent<SessionPlayerComponent>().Player;
			//获取到第一个地图服务器的内网地址
			IPEndPoint mapAddress = StartConfigComponent.Instance.MapConfigs[0].GetComponent<InnerConfig>().IPEndPoint;
			//创建socket连接到地图服务器
			Session mapSession = Game.Scene.GetComponent<NetInnerComponent>().Get(mapAddress);
			//给地图服务器发送创建单位的协议 
			//GateSessionId=session.InstanceId 传递会话实体的实例ID,后面可以通过这个重新找回会话实体
			M2G_CreateUnit createUnit = (M2G_CreateUnit)await mapSession.Call(new G2M_CreateUnit() { PlayerId = player.Id, GateSessionId = session.InstanceId });
			//将地图服务器返回的数据 单位Id赋值缓存 并且响应给客户端 单位Id缓存到Session上
			player.UnitId = createUnit.UnitId;
			response.UnitId = createUnit.UnitId;
			//虽然响应给客户端了 但是客户端没有处理M2G_CreateUnit协议
			reply();
		}
	}
}