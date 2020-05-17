using ETModel;

namespace ETHotfix
{
	[Event(EventIdType.LoginFinish)]
	public class LoginFinish_RemoveLoginUI: AEvent
	{
		public override void Run()
		{
			//移除掉UILogin组件
			Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILogin);
			//卸载掉AB资源
			ETModel.Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle(UIType.UILogin.StringToAB());
		}
	}
}
