using System;
using ETModel;
using PF;
using UnityEngine;

namespace ETHotfix
{
	[MessageHandler(AppType.Map)]
	public class G2M_CreateUnitHandler : AMRpcHandler<G2M_CreateUnit, M2G_CreateUnit>
	{
		protected override async ETTask Run(Session session, G2M_CreateUnit request, M2G_CreateUnit response, Action reply)
		{
			//并且生成ID 创建Unit实体对象
			Unit unit = ComponentFactory.CreateWithId<Unit>(IdGenerater.GenerateId());
			unit.AddComponent<MoveComponent>();
			unit.AddComponent<UnitPathComponent>();
			//设置实体位置
			unit.Position = new Vector3(-10, 0, -10);
			
			//添加邮箱
			await unit.AddComponent<MailBoxComponent>().AddLocation();
			//添加单位网关组件 主要是缓存该单位在网关服务器的Session Id 
			//GateSessionActorId = request.GateSessionId
			unit.AddComponent<UnitGateComponent, long>(request.GateSessionId);
			//缓存这个单位
			Game.Scene.GetComponent<UnitComponent>().Add(unit);
			//设置响应的字段 UnitId
			response.UnitId = unit.Id;
			
			
			// 广播协议:创建unit
			M2C_CreateUnits createUnits = new M2C_CreateUnits();
			//获取所有的玩家 然后进行广播
			Unit[] units = Game.Scene.GetComponent<UnitComponent>().GetAll();
			foreach (Unit u in units)
			{
				UnitInfo unitInfo = new UnitInfo();
				unitInfo.X = u.Position.x;
				unitInfo.Y = u.Position.y;
				unitInfo.Z = u.Position.z;
				unitInfo.UnitId = u.Id;
				createUnits.Units.Add(unitInfo);
			}
			MessageHelper.Broadcast(createUnits);

			//响应网关的请求
			reply();
		}
	}
}