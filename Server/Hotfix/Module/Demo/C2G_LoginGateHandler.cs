using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Gate)]
	public class C2G_LoginGateHandler : AMRpcHandler<C2G_LoginGate, G2C_LoginGate>
	{
		protected override async ETTask Run(Session session, C2G_LoginGate request, G2C_LoginGate response, Action reply)
		{
			//通过key获取帐号
			string account = Game.Scene.GetComponent<GateSessionKeyComponent>().Get(request.Key);
			//获取不到则为异常
			if (account == null)
			{
				response.Error = ErrorCode.ERR_ConnectGateKeyError;
				response.Message = "Gate key验证失败!";
				reply();
				return;
			}
			//获取得到
			//创建Plaeyr实体 传递account给它的Awake方法
			Player player = ComponentFactory.Create<Player, string>(account);
			//添加到PlayerComponent进行缓存 key是实例Id 其Awake方法内部只是像单例模式将静态变量Instance指向了它自己
			Game.Scene.GetComponent<PlayerComponent>().Add(player);
			//给会话实体添加SessionPlayerComponent 并且缓存Player
			session.AddComponent<SessionPlayerComponent>().Player = player;
			//添加一个邮箱MailBoxComponent 将MailboxType.GateSession邮箱类型 传递给Awake方法 内部进行缓存而已
			session.AddComponent<MailBoxComponent, string>(MailboxType.GateSession);
			//将PlayerId返回给客户端
			response.PlayerId = player.Id;
			reply();

			session.Send(new G2C_TestHotfixMessage() { Info = "recv hotfix message success" });
			await ETTask.CompletedTask;
		}
	}
}