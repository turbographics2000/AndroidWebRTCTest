using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using SimplePeerConnectionM;
using System;
using Synamon.Neutrans.WebRTC;

public class WebRTC : MonoBehaviour
{

    public WebRtcVideoPlayer remotePlayer;

    private WebSocket signaling;
    private PeerConnectionM peer;
    private FrameQueue frameQueueRemote;

    [Serializable]
    class SignalingMessage
    {
        public string type;
        public string sdp;
        public string candidate;
        public int sdpMLineIndex;
        public string sdpMid;

        public SignalingMessage(string type, string sdp)
        {
            this.type = type;
            this.sdp = sdp;
        }

        public SignalingMessage(string candidate, int sdpMLineIndex, string sdpMid)
        {
            this.candidate = candidate;
            this.sdpMLineIndex = sdpMLineIndex;
            this.sdpMid = sdpMid;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID
        frameQueueRemote = new FrameQueue(5);
        remotePlayer.frameQueue = frameQueueRemote;

        var systemClass = new AndroidJavaClass("java.lang.System");
        string libname = "jingle_peerconnection_so";
        systemClass.CallStatic("loadLibrary", new object[1] { libname });

        /*
         * Below is equivalent of this java code:
         * PeerConnectionFactory.InitializationOptions.Builder builder = 
         *   PeerConnectionFactory.InitializationOptions.builder(UnityPlayer.currentActivity);
         * PeerConnectionFactory.InitializationOptions options = 
         *   builder.createInitializationOptions();
         * PeerConnectionFactory.initialize(options);
         */

        var playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
        var webrtcClass = new AndroidJavaClass("org.webrtc.PeerConnectionFactory");
        var initOptionsClass = new AndroidJavaClass("org.webrtc.PeerConnectionFactory$InitializationOptions");
        var builder = initOptionsClass.CallStatic<AndroidJavaObject>("builder", new object[1] { activity });
        var options = builder.Call<AndroidJavaObject>("createInitializationOptions");
        if (webrtcClass != null)
        {
            //androidLog("log", "PeerConnectionFactory.initialize calling");
            webrtcClass.CallStatic("initialize", new object[1] { options });
            //androidLog("log", "PeerConnectionFactory.initialize called.");
        }
#endif

        //signalingConnect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void signalingConnect()
    {
        signaling = new WebSocket("ws://172.16.1.49:8888");
        signaling.OnOpen += (s, e) =>
        {
            Debug.Log("signaling open");
        };
        signaling.OnMessage += (s, e) =>
        {
            var msg = JsonUtility.FromJson<SignalingMessage>(e.Data);
            if (string.IsNullOrEmpty(msg.sdp))
            {
                peer.SetRemoteDescription(msg.type, msg.sdp);
                if (msg.type == "offer")
                {
                    peer.CreateAnswer();
                }
            }
            else if (string.IsNullOrEmpty(msg.candidate))
            {
                peer.AddIceCandidate(msg.candidate, msg.sdpMLineIndex, msg.sdpMid);
            }
        };
        signaling.OnClose += (s, e) =>
        {
            Debug.Log("signaling close");
        };
        signaling.Connect();
    }

    void setupPeer()
    {
        peer = new PeerConnectionM(new[] { "stun:stun.l.google.com:19302" }, "", "", "", "");
        peer.OnLocalSdpReadytoSend += Peer_OnLocalSdpReadytoSend;
        peer.OnIceCandidateReadytoSend += Peer_OnIceCandidateReadytoSend;
        peer.OnRemoteVideoFrameReady += OnI420RemoteFrameReady;
    }

    private void Peer_OnIceCandidateReadytoSend(int id, string candidate, int sdpMLineIndex, string sdpMid, string senderId, string mediaType)
    {
        var msg = JsonUtility.ToJson(new SignalingMessage(candidate, sdpMLineIndex, sdpMid));
        signaling.Send(msg);
    }

    private void Peer_OnLocalSdpReadytoSend(int id, string type, string sdp, string senderId, string mediaType)
    {
        var msg = JsonUtility.ToJson(new SignalingMessage(type, sdp));
        signaling.Send(msg);
    }

    public void OnI420RemoteFrameReady(int id,
    IntPtr dataY, IntPtr dataU, IntPtr dataV, IntPtr dataA,
    int strideY, int strideU, int strideV, int strideA,
    uint width, uint height)
    {
        var w = (int)width;
        var h = (int)height;
        var size = 4 * w * h;
        //AndroidLog("log", "OnI420RemoteFrameReady called! w=" + width + " h=" + height + " thread:" + Thread.CurrentThread.ManagedThreadId);

        var packet = frameQueueRemote.GetDataBufferWithoutContents(size);
        if (packet == null)
        {
            //AndroidLog("OnI420RemoteFrameReady: FramePacket is null!");
            return;
        }
        CopyYuvToBuffer(dataY, dataU, dataV, strideY, strideU, strideV, width, height, packet.Buffer, size);
        packet.width = w;
        packet.height = h;

        frameQueueRemote.Push(packet);
    }

    void CopyYuvToBuffer(IntPtr dataY, IntPtr dataU, IntPtr dataV,
        int strideY, int strideU, int strideV,
        uint width, uint height, byte[] buffer, int size)
    {
        unsafe
        {
            byte* ptrY = (byte*)dataY.ToPointer();
            byte* ptrU = (byte*)dataU.ToPointer();
            byte* ptrV = (byte*)dataV.ToPointer();
            int srcOffsetY = 0;
            int srcOffsetU = 0;
            int srcOffsetV = 0;
            int destOffset = 0;
            for (int i = 0; i < height; i++)
            {
                srcOffsetY = i * strideY;
                srcOffsetU = (i / 2) * strideU;
                srcOffsetV = (i / 2) * strideV;
                destOffset = i * (int)width * 4;
                for (int j = 0; j < width; j += 2)
                {
                    {
                        byte y = ptrY[srcOffsetY];
                        byte u = ptrU[srcOffsetU];
                        byte v = ptrV[srcOffsetV];
                        srcOffsetY++;
                        srcOffsetU++;
                        srcOffsetV++;
                        destOffset += 4;
                        buffer[destOffset] = y;
                        buffer[destOffset + 1] = u;
                        buffer[destOffset + 2] = v;
                        buffer[destOffset + 3] = 0xff;

                        // use same u, v values
                        byte y2 = ptrY[srcOffsetY];
                        srcOffsetY++;
                        destOffset += 4;
                        if (destOffset + 3 >= size) return;
                        buffer[destOffset] = y2;
                        buffer[destOffset + 1] = u;
                        buffer[destOffset + 2] = v;
                        buffer[destOffset + 3] = 0xff;
                    }
                }
            }
        }
    }

}
