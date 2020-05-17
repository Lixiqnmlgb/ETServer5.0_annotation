using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class MessageDispatcherComponentAwakeSystem : AwakeSystem<MessageDispatcherComponent>
	{
		public override void Awake(MessageDispatcherComponent self)
		{
			self.Load();
		}
	}

	[ObjectSystem]
	public class MessageDispatcherComponentLoadSystem : LoadSystem<MessageDispatcherComponent>
	{
		public override void Load(MessageDispatcherComponent self)
		{
			self.Load();
		}
	}

	/// <summary>
	/// 消息分发组件
	/// </summary>
	public static class MessageDispatcherComponentHelper
	{
		public static void Load(this MessageDispatcherComponent self)
		{
			self.Handlers.Clear();
			//获取到配置的appType 在服务器启动的时候有缓存
			AppType appType = StartConfigComponent.Instance.StartConfig.AppType;
			//获取到所有加了MessageHandler特性的类型
			List<Type> types = Game.EventSystem.GetTypes(typeof(MessageHandlerAttribute));
			//对获取到的类型进行遍历
			foreach (Type type in types)
			{//获取该类型自定义的属性
				object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
				if (attrs.Length == 0)
				{//如果属性的个数是0 遍历下一个元素
					continue;
				}
				//如果第一个属性是MessageHandler 
				MessageHandlerAttribute messageHandlerAttribute = attrs[0] as MessageHandlerAttribute;
				if (!messageHandlerAttribute.Type.Is(appType))
				{//判断它的type参数,如果不等于当前的appType 继续遍历下一个元素 如果类型一致 下面要对其进行缓存的
					continue;
				}
				//根据类型,创建实例实例 判断实例如果继承了IMHandler的接口
				IMHandler iMHandler = Activator.CreateInstance(type) as IMHandler;
				if (iMHandler == null)
				{
					Log.Error($"message handle {type.Name} 需要继承 IMHandler");
					continue;
				}
				//通过接口内部的GetMessageType方法 获取到要处理的proto类型 因为我们是继承自AMHanlde 所以实际是获取AMHanlde里的GetMessageType
				Type messageType = iMHandler.GetMessageType();
				//通过类型获取协议号
				ushort opcode = Game.Scene.GetComponent<OpcodeTypeComponent>().GetOpcode(messageType);
				if (opcode == 0)
				{//异常 协议号都是自动生成的 不可能为0
					Log.Error($"消息opcode为0: {messageType.Name}");
					continue;
				}
				//注册 也就是缓存起来 同时将协议ID与处理的实例关联起来 .... 说简单点就是创建个字典 添加到字典内部去
				self.RegisterHandler(opcode, iMHandler);
			}
		}
		//MessageDispatcherComponent的扩展方法 将协议号和处理协议的实例,都缓存到其内部的Handlers中
		public static void RegisterHandler(this MessageDispatcherComponent self, ushort opcode, IMHandler handler)
		{
			if (!self.Handlers.ContainsKey(opcode))
			{
				self.Handlers.Add(opcode, new List<IMHandler>());
			}
			self.Handlers[opcode].Add(handler);
		}

		/// <summary>
		/// 扩展MessageDispatcherComponent类,添加了Handle方法
		/// </summary>
		/// <param name="self">消息分发处理组件</param>
		/// <param name="session">会话实体</param>
		/// <param name="messageInfo">包含协议号和proto实体的对象</param>
		public static void Handle(this MessageDispatcherComponent self, Session session, MessageInfo messageInfo)
		{
			List<IMHandler> actions;
			//通过协议号获取到缓存的处理对象(继承自IMHandler的) 如果是空 则说明我们开发的时候 并没有写这条协议的处理方法
			if (!self.Handlers.TryGetValue(messageInfo.Opcode, out actions))
			{
				Log.Error($"消息没有处理: {messageInfo.Opcode} {JsonHelper.ToJson(messageInfo.Message)}");
				return;
			}
			//如果获取到 就进行遍历 
			//因为一条协议 可能有多个对象要进行处理 所以以列表缓存所有要处理的方法
			foreach (IMHandler ev in actions)
			{
				try
				{
					//调用IMHandler对象的Handle方法
					ev.Handle(session, messageInfo.Message);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
	}
}