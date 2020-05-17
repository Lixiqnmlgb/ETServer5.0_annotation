using System;
using System.Collections.Generic;

namespace ETModel
{
	public class ActorLocationSenderComponent: Component
	{
		public readonly Dictionary<long, ActorLocationSender> ActorLocationSenders = new Dictionary<long, ActorLocationSender>();

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();
			
			foreach (ActorLocationSender actorLocationSender in this.ActorLocationSenders.Values)
			{
				actorLocationSender.Dispose();
			}
			this.ActorLocationSenders.Clear();
		}
		//获取
		public ActorLocationSender Get(long id)
		{
			if (id == 0)
			{
				throw new Exception($"actor id is 0");
			}
			//如果包含了 就直接返回
			if (this.ActorLocationSenders.TryGetValue(id, out ActorLocationSender actorLocationSender))
			{
				return actorLocationSender;
			}
			//如果没有包含 
			//就创建一个sender组件 并且设置组件的id等于传递进来的Id(单位Id)
			actorLocationSender = ComponentFactory.CreateWithId<ActorLocationSender>(id);
			actorLocationSender.Parent = this;
			this.ActorLocationSenders[id] = actorLocationSender;
			return actorLocationSender;
		}
		//移除
		public void Remove(long id)
		{
			if (!this.ActorLocationSenders.TryGetValue(id, out ActorLocationSender actorMessageSender))
			{
				return;
			}
			this.ActorLocationSenders.Remove(id);
			actorMessageSender.Dispose();
		}
	}
}
