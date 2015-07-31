using UnityEngine;
using System.Collections;
using System.Threading;
using System;
using Rug.Osc;

[Serializable]
public class OscRecieveData
{
	public string address;
	public object value;
	public string valueString;
}

/*
 * Rug.Oscライブラリを利用したレシーバーラッパークラス
 * https://bitbucket.org/rugcode/rug.osc/overview
 * 
 * - recieveDatasにOSCアドレスを指定して任意データを受信する
 * - GetRecieveDataValue()でOSCアドレス指定で受信データを取得する
 */ 
public class OscRecieveController : MonoBehaviour
{
	// 受信ポート
	public int port = 55005;

	// アドレス別受信データ
	public OscRecieveData[] recieveDatas;

	// 確認用受信rawデータ
	[TextArea(5,10)]
	public string latestRawData = "";

	private OscReceiver reciever;
	private OscAddressManager listener;
	private Thread thread;


	void Awake()
	{

	}
	
	void Start()
	{
		reciever = new OscReceiver(port);
		reciever.Connect();

		listener = new OscAddressManager();
		foreach(OscRecieveData data in recieveDatas)
		{
			data.value = OscNull.Value;
			data.valueString = data.value.ToString();
			listener.Attach(data.address, OnRecieved);
		}

		thread = new Thread(new ThreadStart(ListenLoop));
		thread.Start();
	}

	void Update()
	{
		
	}


	// アドレス別の受信イベント
	public void OnRecieved(OscMessage msg)
	{
		string address = msg.Address;
		object value = msg[0];
		
		foreach(OscRecieveData data in recieveDatas)
		{
			if(data.address == address)
			{
				data.value = value;
				data.valueString = value.ToString();
				break;
			}
		}
	}

	// 受信スレッド
	private void ListenLoop()
	{
		try
		{
			while(reciever.State != OscSocketState.Closed)
			{
				if(reciever.State == OscSocketState.Connected)
				{
					OscPacket packet = reciever.Receive();
					if(listener.ShouldInvoke(packet) == OscPacketInvokeAction.Invoke)
						listener.Invoke(packet);

					latestRawData = packet.ToString();
				}
			}
		}
		catch(Exception e)
		{
			Debug.LogError(e.Message);
		}
	}

	// アドレスを指定して受信データの取得
	public object GetRecieveDataValue(string oscAddress)
	{
		object value = null;
		foreach(OscRecieveData data in recieveDatas)
		{
			if(data.address == oscAddress)
			{
				value = data.value;
				break;
			}
		}
		return value;
	}
}
