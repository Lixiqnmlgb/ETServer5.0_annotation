using System;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class ActorLocationSenderAwakeSystem : AwakeSystem<ActorLocationSender>
    {
        public override void Awake(ActorLocationSender self)
        {
            self.LastRecvTime = TimeHelper.Now();
            self.FailTimes = 0;
            self.ActorId = 0;
            
            StartAsync(self).Coroutine();
        }
        
        public async ETVoid StartAsync(ActorLocationSender self)
        {
	        using (await CoroutineLockComponent.Instance.Wait(self.Id))
	        {//设定ActorId
				Log.Debug($"actorId:{self.ActorId}");
		        self.ActorId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(self.Id);
	        }
        }
    }
	
    [ObjectSystem]
    public class ActorLocationSenderDestroySystem : DestroySystem<ActorLocationSender>
    {
        public override void Destroy(ActorLocationSender self)
        {
            self.Id = 0;
            self.LastRecvTime = 0;
            self.ActorId = 0;
            self.FailTimes = 0;
        }
    }
    
    public static class ActorLocationSenderSystem
    {
		private static async ETTask<IActorResponse> Run(this ActorLocationSender self, IActorRequest iActorRequest)
		{
			long instanceId = self.InstanceId;
			//等待协程锁调用结束 
			using (await CoroutineLockComponent.Instance.Wait(self.Id))
			{
				if (self.InstanceId != instanceId)
				{
					throw new RpcException(ErrorCode.ERR_ActorRemove, "");
				}
				//获取到ActorMessageSender
				ActorMessageSender actorMessageSender = Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(self.ActorId);
				try
				{
					// ERR_NotFoundActor是需要抛异常的，但是这里不能抛
					IActorResponse response = await actorMessageSender.CallWithoutException(iActorRequest);
					//如果出现异常
					switch (response.Error)
					{//没找到Actor
						case ErrorCode.ERR_NotFoundActor:
							// 如果没找到Actor,重试
							++self.FailTimes;

							// 失败MaxFailTimes次则清空actor发送队列，返回失败
							if (self.FailTimes > ActorLocationSender.MaxFailTimes)
							{
								// 失败直接删除actorproxy
								Log.Info($"actor send message fail, actorid: {self.Id}");
								self.GetParent<ActorLocationSenderComponent>().Remove(self.Id);
								throw new RpcException(response.Error, "");
							}

							// 等待0.5s再发送
							await Game.Scene.GetComponent<TimerComponent>().WaitAsync(500);
							if (self.InstanceId != instanceId)
							{
								throw new RpcException(ErrorCode.ERR_ActorRemove, "");
							}
							self.ActorId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(self.Id);
							IActorResponse iActorResponse = await Run(self, iActorRequest);
							if (self.InstanceId != instanceId)
							{
								throw new RpcException(ErrorCode.ERR_ActorRemove, "");
							}
							return iActorResponse;
							//没邮箱组件
						case ErrorCode.ERR_ActorNoMailBoxComponent:
							self.GetParent<ActorLocationSenderComponent>().Remove(self.Id);
							throw new RpcException(response.Error, "");

						default:
							//上一次接收的时间
							self.LastRecvTime = TimeHelper.Now();
							//失败次数
							self.FailTimes = 0;
							break;
					}
					
					return response;
				}
				catch (Exception)
				{
					//出现异常 移除...
					self.GetParent<ActorLocationSenderComponent>().Remove(self.Id);
					throw;
				}
			}
		}

	    public static async ETVoid Send(this ActorLocationSender self, IActorLocationMessage request)
	    {
		    if (request == null)
		    {
			    throw new Exception($"actor location send message is null");
		    }
		    
			await Run(self, request);
	    }

		public static async ETTask<IActorLocationResponse> Call(this ActorLocationSender self, IActorLocationRequest request)
		{
			if (request == null)
			{
				throw new Exception($"actor location call message is null");
			}
			
			return await Run(self, request) as IActorLocationResponse;
		}
    }
}