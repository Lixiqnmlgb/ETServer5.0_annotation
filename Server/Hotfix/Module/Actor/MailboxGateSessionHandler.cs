using System;
using ETModel;

namespace ETHotfix
{
	/// <summary>
	/// gate session类型的Mailbox，收到的actor消息直接转发给客户端
	/// 多服情况下,网关服务器挂载该组件,处理Actor消息
	/// </summary>
	[MailboxHandler(AppType.Gate, MailboxType.GateSession)]
	public class MailboxGateSessionHandler : IMailboxHandler
	{
		public async ETTask Handle(Session session, Entity entity, object actorMessage)
		{
			try
			{
				Log.Info($"AppType.Gate IActorMessage 发送消息给客户端");
				IActorMessage iActorMessage = actorMessage as IActorMessage;
				// 发送给客户端
				Session clientSession = entity as Session;
				iActorMessage.ActorId = 0;
				clientSession.Send(iActorMessage);
				await ETTask.CompletedTask;
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
