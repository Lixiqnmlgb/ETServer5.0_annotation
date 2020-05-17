using ETModel;

namespace ETHotfix
{
	/// <summary>
	/// 内网消息分发
	/// </summary>
	public class InnerMessageDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, ushort opcode, object message)
		{
			Log.Info($"内网收到消息:{opcode}:{message.GetType()}");
			// 收到actor消息,放入actor队列
			switch (message)
			{
				case IActorRequest iActorRequest:
				{
					HandleIActorRequest(session, iActorRequest).Coroutine();
					break;
				}
				case IActorMessage iactorMessage:
				{
					HandleIActorMessage(session, iactorMessage).Coroutine();
					break;
				}
				default:
				{
					Game.Scene.GetComponent<MessageDispatcherComponent>().Handle(session, new MessageInfo(opcode, message));
					break;
				}
			}
		}
		/// <summary>
		/// 处理IActorRequest类型的消息
		/// </summary>
		/// <param name="session"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		private async ETVoid HandleIActorRequest(Session session, IActorRequest message)
		{
			using (await CoroutineLockComponent.Instance.Wait(message.ActorId))
			{
				Entity entity = (Entity)Game.EventSystem.Get(message.ActorId);
				//entity等于空 异常
				if (entity == null)
				{
					Log.Warning($"not found actor: {message}");
					ActorResponse response = new ActorResponse
					{
						Error = ErrorCode.ERR_NotFoundActor,
						RpcId = message.RpcId
					};
					session.Reply(response);
					return;
				}
	
				MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
				//mailBoxComponent等于空 异常
				if (mailBoxComponent == null)
				{
					ActorResponse response = new ActorResponse
					{
						Error = ErrorCode.ERR_ActorNoMailBoxComponent,
						RpcId = message.RpcId
					};
					session.Reply(response);
					Log.Error($"actor not add MailBoxComponent: {entity.GetType().Name} {message}");
					return;
				}
				//如无异常 添加到mailBoxComponent中
				await mailBoxComponent.Add(session, message);
			}
		}
		/// <summary>
		/// 处理IActorMessage类型的消息
		/// </summary>
		/// <param name="session"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		private async ETVoid HandleIActorMessage(Session session, IActorMessage message)
		{
			using (await CoroutineLockComponent.Instance.Wait(message.ActorId))
			{
				//ActorId 是单位在网关服务器上的会话ID
				Entity entity = (Entity)Game.EventSystem.Get(message.ActorId);
				if (entity == null)
				{
					Log.Error($"not found actor: {message}");
					return;
				}
	
				MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
				if (mailBoxComponent == null)
				{
					Log.Error($"actor not add MailBoxComponent: {entity.GetType().Name} {message}");
					return;
				}
				//压入邮箱处理
				await mailBoxComponent.Add(session, message);
			}
		}
	}
}
