using System;
using ETModel;

namespace ETHotfix
{
	// 用来测试消息包含复杂类型，是否产生gc
	[MessageHandler(AppType.Gate)]
	public class C2G_PlayerInfoHandler : AMRpcHandler<C2G_PlayerInfo, G2C_PlayerInfo>
	{
		protected override async ETTask Run(Session session, C2G_PlayerInfo request, G2C_PlayerInfo response, Action reply)
		{
			//引用了PlayerInfo 那么这里需要先new 然后加入数据 最后才能够正常序列化和反序列化
			response.PlayerInfo = new PlayerInfo();
			response.PlayerInfos.Add(new PlayerInfo() {RpcId = 1});
			response.PlayerInfos.Add(new PlayerInfo() {RpcId = 2});
			response.PlayerInfos.Add(new PlayerInfo() {RpcId = 3});
			//测试Repeated->List的使用
			response.TestRepeatedInt32.Add(4);
			response.TestRepeatedInt32.Add(5);
			response.TestRepeatedInt32.Add(6);
			response.TestRepeatedInt64.Add(7);
			response.TestRepeatedInt64.Add(8);
			//测试Repeated->List的使用
			response.TestRepeatedString.Add("9");
			response.TestRepeatedString.Add("10");
			//reply是AMRpcHandler父类的Handle方法内部定义的
			//调用后 会将response传递给对应的session进行发送处理
			reply();
			await ETTask.CompletedTask;
		}
	}
}
