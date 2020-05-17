using System.Net;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class NetInnerComponentAwakeSystem : AwakeSystem<NetInnerComponent>
	{
		public override void Awake(NetInnerComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class NetInnerComponentAwake1System : AwakeSystem<NetInnerComponent, string>
	{
		public override void Awake(NetInnerComponent self, string a)
		{
			self.Awake(a);
		}
	}
	
	[ObjectSystem]
	public class NetInnerComponentLoadSystem : LoadSystem<NetInnerComponent>
	{
		public override void Load(NetInnerComponent self)
		{
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new InnerMessageDispatcher();
		}
	}

	[ObjectSystem]
	public class NetInnerComponentUpdateSystem : UpdateSystem<NetInnerComponent>
	{
		public override void Update(NetInnerComponent self)
		{
			self.Update();
		}
	}

	public static class NetInnerComponentHelper
	{
		public static void Awake(this NetInnerComponent self)
		{
			self.Awake(NetworkProtocol.TCP, Packet.PacketSizeLength4);
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new InnerMessageDispatcher();
			self.AppType = StartConfigComponent.Instance.StartConfig.AppType;
		}

		public static void Awake(this NetInnerComponent self, string address)
		{
			//初始化 也是Tcp协议 包体长度 用4个字节表示
			self.Awake(NetworkProtocol.TCP, address, Packet.PacketSizeLength4);
			//内网通信 使用的是Bson进行序列化和反序列化的
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new InnerMessageDispatcher();
			self.AppType = StartConfigComponent.Instance.StartConfig.AppType;
		}

		public static void Update(this NetInnerComponent self)
		{
			self.Update();
		}
	}
}