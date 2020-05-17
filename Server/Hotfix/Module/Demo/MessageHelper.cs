using ETModel;

namespace ETHotfix
{
	public static class MessageHelper
	{
		public static void Broadcast(IActorMessage message)
		{
			Unit[] units = Game.Scene.GetComponent<UnitComponent>().GetAll();
			//actor位置发送组件
			ActorMessageSenderComponent actorLocationSenderComponent = Game.Scene.GetComponent<ActorMessageSenderComponent>();
			//遍历每一个单位
			foreach (Unit unit in units)
			{
				//获取到它们的网关组件  里面存储的是网关服务器里的Session Id 用于跟客户端进行通信的
				UnitGateComponent unitGateComponent = unit.GetComponent<UnitGateComponent>();
				if (unitGateComponent.IsDisconnect)
				{
					continue;
				}
				//通过网关sessionID找到 玩家是在哪个网关服务器(进程)
				ActorMessageSender actorMessageSender = actorLocationSenderComponent.Get(unitGateComponent.GateSessionActorId);
				//内部就去进行创建会话实体 连接到网关.. 将消息发送给网关 网关那边去处理一下就好了..  
				//不过demo里并没有看到网关有处理这条协议
				actorMessageSender.Send(message);
				
				//建议:其实进程之间的通信,是通过socket来实现的,那维护好对应的socket不就行了
				//最好省去各种由Id找到另一个Id的逻辑,不利于应对后续的变化 
			}
		}
	}
}
