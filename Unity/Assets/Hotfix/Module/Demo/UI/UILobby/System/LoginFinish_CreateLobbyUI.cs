using ETModel;

namespace ETHotfix
{
	[Event(EventIdType.LoginFinish)]
	public class LoginFinish_CreateLobbyUI: AEvent
	{
		public override void Run()
		{
			//创建UILobby 实体
			UI ui = UILobbyFactory.Create();
			//添加到UIComponent中
			Game.Scene.GetComponent<UIComponent>().Add(ui);
		}
	}
}
