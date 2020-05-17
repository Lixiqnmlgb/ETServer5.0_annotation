using ETModel;

namespace ETHotfix
{
    public class OuterMessageDispatcher : IMessageDispatcher
    {
        public void Dispatch(Session session, ushort opcode, object message)
        {
            DispatchAsync(session, opcode, message).Coroutine();
        }

        public async ETVoid DispatchAsync(Session session, ushort opcode, object message)
        {
            // 根据消息接口判断是不是Actor消息，不同的接口做不同的处理
            switch (message)
            {
                //如果是actorLocationRequest这类型的协议
                case IActorLocationRequest actorLocationRequest: // gate session收到actor rpc消息，先向actor 发送rpc请求，再将请求结果返回客户端
                    {
                        
                        //获取会话实体挂载的Player组件,获取到Player玩家实体的单位ID,这个是在登录地图服务器后进行缓存的
                        //C2G_EnterMapHandler 进入服务器协议的处理方法内部对Player进行了缓存
                        //获取单位ID
                        long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
                        //第一 需要挂载有ActorLocationSenderComponent组件 
                        //获取到Sender组件
                        ActorLocationSender actorLocationSender = Game.Scene.GetComponent<ActorLocationSenderComponent>().Get(unitId);
                        //缓存请求的RpcId 发送方设定的
                        int rpcId = actorLocationRequest.RpcId; 
                        //缓存会话实体的实例ID
                        long instanceId = session.InstanceId;
                        //发送处理请求 将message传递过去  获取到响应的proto
                        IResponse response = await actorLocationSender.Call(actorLocationRequest);
                        response.RpcId = rpcId;

                        // session可能已经断开了，所以这里需要判断
                        if (session.InstanceId == instanceId)
                        {
                            //响应给客户端
                            session.Reply(response);
                        }
                        break;
                    }
                //如果是IActorLocationMessage这类型的协议
                case IActorLocationMessage actorLocationMessage:
                    {
                        //获取到玩家的单位ID
                        long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
                        Log.Info($"处理Actor消息,单位Id:{unitId}");
                        //然后获取到Sender组件 
                        ActorLocationSender actorLocationSender = Game.Scene.GetComponent<ActorLocationSenderComponent>().Get(unitId);
                        //将接收到的proto传递过去进行处理即可
                        actorLocationSender.Send(actorLocationMessage);
                        break;
                    }
                case IActorRequest actorRequest:  // 分发IActorRequest消息，目前没有用到，需要的自己添加
                    {
                        break;
                    }
                case IActorMessage actorMessage:  // 分发IActorMessage消息，目前没有用到，需要的自己添加
                    {
                        break;
                    }
                default:
                    {
                        // 非Actor消息
                        Game.Scene.GetComponent<MessageDispatcherComponent>().Handle(session, new MessageInfo(opcode, message));
                        break;
                    }
            }
        }
    }
}
