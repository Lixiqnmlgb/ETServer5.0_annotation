using System;
using ETModel;

namespace ETHotfix
{
	/// <summary>
	/// 消息分发类型的Mailbox,对mailbox中的消息进行分发处理
	/// 单服模式下,会挂载该组件,处理Actor消息
	/// </summary>
	[MailboxHandler(AppType.AllServer, MailboxType.MessageDispatcher)]
	public class MailboxMessageDispatcherHandler : IMailboxHandler
	{
		public async ETTask Handle(Session session, Entity entity, object actorMessage)
		{
			try
			{
				//获取Actor消息分发组件ActorMessageDispatcherComponent
				//调用它的Handle方法
				await Game.Scene.GetComponent<ActorMessageDispatcherComponent>().Handle(
					entity, new ActorMessageInfo() { Session = session, Message = actorMessage });
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
