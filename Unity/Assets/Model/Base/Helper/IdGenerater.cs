namespace ETModel
{
	public static class IdGenerater
	{
		private static long instanceIdGenerator;
		
		private static long appId;
		
		//appid是启动服务器读取到的配置 或者进程调度时候传递值进来的
		public static long AppId
		{
			set
			{
				appId = value;
				//实例ID=appId*2的48次方
				instanceIdGenerator = appId << 48;
			}
		}

		private static ushort value;

		public static long GenerateId()
		{
			long time = TimeHelper.ClientNowSeconds();
			//appId*2的48次方+time*2的16次方+ value自增后的值
			return (appId << 48) + (time << 16) + ++value;
		}
		
		public static long GenerateInstanceId()
		{
			return ++instanceIdGenerator;
		}

		public static int GetAppId(long v)
		{//v/2的48次方...
			return (int)(v >> 48);
		}
	}
}