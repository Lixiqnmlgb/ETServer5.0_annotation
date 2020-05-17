using System;

namespace ETModel
{
	public abstract class AMRpcHandler<Request, Response>: IMHandler where Request : class, IRequest where Response : class, IResponse 
	{
		protected abstract ETTask Run(Session session, Request request, Response response, Action reply);

		public async ETVoid Handle(Session session, object message)
		{
			try
			{
				Request request = message as Request;
				if (request == null)
				{
					Log.Error($"消息类型转换错误: {message.GetType().Name} to {typeof (Request).Name}");
				}

				int rpcId = request.RpcId;
				long instanceId = session.InstanceId;
				//创建响应的对象
				Response response = Activator.CreateInstance<Response>();
				//定义Reply方法
				void Reply()
				{
					// 等回调回来,session可以已经断开了,所以需要判断session InstanceId是否一样
					if (session.InstanceId != instanceId)
					{
						return;
					}

					response.RpcId = rpcId;
					//响应消息给客户端
					session.Reply(response);
				}

				try
				{
					//将session, request, response, Reply放在参数传递给Run方法
					await this.Run(session, request, response, Reply);
				}
				catch (Exception e)
				{
					Log.Error(e);
					response.Error = ErrorCode.ERR_RpcFail;
					response.Message = e.ToString();
					Reply();
				}
				
			}
			catch (Exception e)
			{
				Log.Error($"解释消息失败: {message.GetType().FullName}\n{e}");
			}
		}

		public Type GetMessageType()
		{
			return typeof (Request);
		}
	}
}