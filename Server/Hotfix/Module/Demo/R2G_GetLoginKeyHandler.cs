using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Gate)]
	public class R2G_GetLoginKeyHandler : AMRpcHandler<R2G_GetLoginKey, G2R_GetLoginKey>
	{
		protected override async ETTask Run(Session session, R2G_GetLoginKey request, G2R_GetLoginKey response, Action reply)
		{
			//随机分配一个Key 
			long key = RandomHelper.RandInt64();
			//将key和帐号缓存起来 并且内部定了个任务 20秒后就移除掉这个key 
			Game.Scene.GetComponent<GateSessionKeyComponent>().Add(key, request.Account);
			response.Key = key;
			reply();
			await ETTask.CompletedTask;
		}
	}
}