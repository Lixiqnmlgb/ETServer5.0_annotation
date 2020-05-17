using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class LocationProxyComponentSystem : AwakeSystem<LocationProxyComponent>
	{
		public override void Awake(LocationProxyComponent self)
		{
			self.Awake();
		}
	}

	public static class LocationProxyComponentEx
	{
		public static void Awake(this LocationProxyComponent self)
		{
			StartConfigComponent startConfigComponent = StartConfigComponent.Instance;

			StartConfig startConfig = startConfigComponent.LocationConfig;//位置服务器的配置 管理各个单位所在的进程
			self.LocationAddress = startConfig.GetComponent<InnerConfig>().IPEndPoint;//获取内网配置地址 IP:端口
		}

		public static async ETTask Add(this LocationProxyComponent self, long key, long instanceId)
		{
			//获取内网组件 连接位置服务器 
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			//将该单位的id,以及实例Id都传递给位置服务器
			await session.Call(new ObjectAddRequest() { Key = key, InstanceId = instanceId });
		}

		public static async ETTask Lock(this LocationProxyComponent self, long key, long instanceId, int time = 1000)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectLockRequest() { Key = key, InstanceId = instanceId, Time = time });
		}

		public static async ETTask UnLock(this LocationProxyComponent self, long key, long oldInstanceId, long instanceId)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectUnLockRequest() { Key = key, OldInstanceId = oldInstanceId, InstanceId = instanceId});
		}

		public static async ETTask Remove(this LocationProxyComponent self, long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectRemoveRequest() { Key = key });
		}

		public static async ETTask<long> Get(this LocationProxyComponent self, long key)
		{
			//通过内网地址 获取到会话实体
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			//获取到物体的实例ID
			ObjectGetResponse response = (ObjectGetResponse)await session.Call(new ObjectGetRequest() { Key = key });
			return response.InstanceId;
		}
	}
}