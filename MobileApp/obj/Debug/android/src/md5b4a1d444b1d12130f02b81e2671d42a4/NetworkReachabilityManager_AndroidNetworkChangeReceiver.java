package md5b4a1d444b1d12130f02b81e2671d42a4;


public class NetworkReachabilityManager_AndroidNetworkChangeReceiver
	extends android.content.BroadcastReceiver
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_onReceive:(Landroid/content/Context;Landroid/content/Intent;)V:GetOnReceive_Landroid_content_Context_Landroid_content_Intent_Handler\n" +
			"";
		mono.android.Runtime.register ("Couchbase.Lite.NetworkReachabilityManager+AndroidNetworkChangeReceiver, Couchbase.Lite, Version=1.2.1.0, Culture=neutral, PublicKeyToken=null", NetworkReachabilityManager_AndroidNetworkChangeReceiver.class, __md_methods);
	}


	public NetworkReachabilityManager_AndroidNetworkChangeReceiver () throws java.lang.Throwable
	{
		super ();
		if (getClass () == NetworkReachabilityManager_AndroidNetworkChangeReceiver.class)
			mono.android.TypeManager.Activate ("Couchbase.Lite.NetworkReachabilityManager+AndroidNetworkChangeReceiver, Couchbase.Lite, Version=1.2.1.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onReceive (android.content.Context p0, android.content.Intent p1)
	{
		n_onReceive (p0, p1);
	}

	private native void n_onReceive (android.content.Context p0, android.content.Intent p1);

	java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
