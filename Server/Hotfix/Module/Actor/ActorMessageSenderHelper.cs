using ETModel;

namespace ETHotfix
{
	public static class ActorMessageSenderHelper
	{
		public static void Send(this ActorMessageSender self, IActorMessage message)
		{
			Log.Info($"ActorMessageSenderHelper:{self.Address}");
			//连接网关内网服务器
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.Address);
			message.ActorId = self.ActorId;
			//发送数据
			session.Send(message);
		}
		
		public static async ETTask<IActorResponse> Call(this ActorMessageSender self, IActorRequest message)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.Address);
			message.ActorId = self.ActorId;
			return (IActorResponse)await session.Call(message);
		}
		
		public static async ETTask<IActorResponse> CallWithoutException(this ActorMessageSender self, IActorRequest message)
		{
			//内网组件获取到会话实体Session
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.Address);
			//设置消息的ActorId
			message.ActorId = self.ActorId;
			//通过内网组件的Session会话实体进行发送消息
			return (IActorResponse)await session.CallWithoutException(message);
		}
	}
}