﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// native impl: 
// https://chromium.googlesource.com/external/webrtc/+/51e2046dbcbbb0375c383594aa4f77aa8ed67b06/examples/unityplugin/simple_peer_connection.cc
// https://chromium.googlesource.com/external/webrtc/+/51e2046dbcbbb0375c383594aa4f77aa8ed67b06/examples/unityplugin/unity_plugin_apis.cc

namespace SimplePeerConnectionM
{
    //// A class for ice candidate.
    //public class IceCandidate
    //{
    //    public IceCandidate(string candidate, int sdpMLineIndex, string sdpMid)
    //    {
    //        mCandidate = candidate;
    //        msdpMLineIndex = sdpMLineIndex;
    //        mSdpMid = sdpMid;
    //    }
    //    string mCandidate;
    //    int msdpMLineIndex;
    //    string mSdpMid;
    //    public string Candidate
    //    {
    //        get { return mCandidate; }
    //        set { mCandidate = value; }
    //    }
    //    public int sdpMLineIndex
    //    {
    //        get { return msdpMLineIndex; }
    //        set { msdpMLineIndex = value; }
    //    }
    //    public string SdpMid
    //    {
    //        get { return mSdpMid; }
    //        set { mSdpMid = value; }
    //    }
    //}

    // A managed wrapper up class for the native c style peer connection APIs.
    public class PeerConnectionM
    {
        public string senderId;
        public string mediaType;

        //private const string dllPath = "webrtc_unity_plugin";
        private const string dllPath = "libjingle_peerconnection_so";

        //[DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        //private static extern int InitializePeerConnection(string[] turnUrls, int noOfUrls, string username, string credential, bool isReceiver);

        //create a peerconnection with turn servers
        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreatePeerConnection(string[] turnUrls, int noOfUrls,
            string username, string credential);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool ClosePeerConnection(int peerConnectionId);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool AddStream(int peerConnectionId, bool audioOnly);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool AddDataChannel(int peerConnectionId);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CreateOffer(int peerConnectionId);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CreateAnswer(int peerConnectionId);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SendDataViaDataChannel(int peerConnectionId, string data);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SetAudioControl(int peerConnectionId, bool isMute, bool isRecord);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LocalDataChannelReadyInternalDelegate();
        public delegate void LocalDataChannelReadyDelegate(int id);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RegisterOnLocalDataChannelReady(
            int peerConnectionId, LocalDataChannelReadyInternalDelegate callback);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DataFromDataChannelReadyInternalDelegate(string s);
        public delegate void DataFromDataChannelReadyDelegate(int id, string s);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RegisterOnDataFromDataChannelReady(
            int peerConnectionId, DataFromDataChannelReadyInternalDelegate callback);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FailureMessageInternalDelegate(string msg);
        public delegate void FailureMessageDelegate(int id, string msg);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RegisterOnFailure(int peerConnectionId,
            FailureMessageInternalDelegate callback);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void AudioBusReadyInternalDelegate(IntPtr data, int bitsPerSample,
            int sampleRate, int numberOfChannels, int numberOfFrames);
        public delegate void AudioBusReadyDelegate(int id, IntPtr data, int bitsPerSample,
            int sampleRate, int numberOfChannels, int numberOfFrames);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RegisterOnAudioBusReady(int peerConnectionId,
            AudioBusReadyInternalDelegate callback);

        // Video callbacks.
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void I420FrameReadyInternalDelegate(
            IntPtr dataY, IntPtr dataU, IntPtr dataV, IntPtr dataA,
            int strideY, int strideU, int strideV, int strideA,
            uint width, uint height);
        public delegate void I420FrameReadyDelegate(int id,
            IntPtr dataY, IntPtr dataU, IntPtr dataV, IntPtr dataA,
            int strideY, int strideU, int strideV, int strideA,
            uint width, uint height);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RegisterOnLocalI420FrameReady(int peerConnectionId,
            I420FrameReadyInternalDelegate callback);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RegisterOnRemoteI420FrameReady(int peerConnectionId,
            I420FrameReadyInternalDelegate callback);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LocalSdpReadytoSendInternalDelegate(string type, string sdp);
        public delegate void LocalSdpReadytoSendDelegate(int id, string type, string sdp, string senderId, string mediaType);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RegisterOnLocalSdpReadytoSend(int peerConnectionId,
            LocalSdpReadytoSendInternalDelegate callback);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void IceCandiateReadytoSendInternalDelegate(
            string candidate, int sdpMLineIndex, string sdpMid);
        public delegate void IceCandiateReadytoSendDelegate(
            int id, string candidate, int sdpMLineIndex, string sdpMid, string senderId, string mediaType);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RegisterOnIceCandiateReadytoSend(
            int peerConnectionId, IceCandiateReadytoSendInternalDelegate callback);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SetRemoteDescription(int peerConnectionId, string type, string sdp);

        [DllImport(dllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool AddIceCandidate(int peerConnectionId, string sdp,
          int sdpMLineIndex, string sdpMid);


        public PeerConnectionM(string[] iceServers, string username, string credential, string senderId, string mediaType)
        {
            mPeerConnectionId = CreatePeerConnection(iceServers, iceServers?.Length ?? 0, username, credential);
            this.senderId = senderId;
            this.mediaType = mediaType;
            RegisterCallbacks();
        }
        public void ClosePeerConnection()
        {
            ClosePeerConnection(mPeerConnectionId);
            mPeerConnectionId = -1;
        }
        // Return -1 if Peerconnection is not available.
        public int GetUniqueId()
        {
            return mPeerConnectionId;
        }
        public void AddStream(bool audioOnly)
        {
            AddStream(mPeerConnectionId, audioOnly);
        }
        public void AddDataChannel()
        {
            AddDataChannel(mPeerConnectionId);
        }
        public void CreateOffer()
        {
            CreateOffer(mPeerConnectionId);
        }
        public void CreateAnswer()
        {
            CreateAnswer(mPeerConnectionId);
        }
        public void SendDataViaDataChannel(string data)
        {
            SendDataViaDataChannel(mPeerConnectionId, data);
        }
        public void SetAudioControl(bool isMute, bool isRecord)
        {
            SetAudioControl(mPeerConnectionId, isMute, isRecord);
        }
        public void SetRemoteDescription(string type, string sdp)
        {
            SetRemoteDescription(mPeerConnectionId, type, sdp);
        }
        public void AddIceCandidate(string candidate, int sdpMLineIndex, string sdpMid)
        {
            AddIceCandidate(mPeerConnectionId, candidate, sdpMLineIndex, sdpMid);
        }
        private void RegisterCallbacks()
        {
            localDataChannelReadyDelegate = new LocalDataChannelReadyInternalDelegate(
                RaiseLocalDataChannelReady);
            RegisterOnLocalDataChannelReady(mPeerConnectionId, localDataChannelReadyDelegate);
            dataFromDataChannelReadyDelegate = new DataFromDataChannelReadyInternalDelegate(
                RaiseDataFromDataChannelReady);
            RegisterOnDataFromDataChannelReady(mPeerConnectionId, dataFromDataChannelReadyDelegate);
            failureMessageDelegate = new FailureMessageInternalDelegate(RaiseFailureMessage);
            RegisterOnFailure(mPeerConnectionId, failureMessageDelegate);
            audioBusReadyDelegate = new AudioBusReadyInternalDelegate(RaiseAudioBusReady);
            RegisterOnAudioBusReady(mPeerConnectionId, audioBusReadyDelegate);
            localI420FrameReadyDelegate = new I420FrameReadyInternalDelegate(
              RaiseLocalVideoFrameReady);
            RegisterOnLocalI420FrameReady(mPeerConnectionId, localI420FrameReadyDelegate);
            remoteI420FrameReadyDelegate = new I420FrameReadyInternalDelegate(
              RaiseRemoteVideoFrameReady);
            RegisterOnRemoteI420FrameReady(mPeerConnectionId, remoteI420FrameReadyDelegate);
            localSdpReadytoSendDelegate = new LocalSdpReadytoSendInternalDelegate(
              RaiseLocalSdpReadytoSend);
            RegisterOnLocalSdpReadytoSend(mPeerConnectionId, localSdpReadytoSendDelegate);
            iceCandiateReadytoSendDelegate =
                new IceCandiateReadytoSendInternalDelegate(RaiseIceCandiateReadytoSend);
            RegisterOnIceCandiateReadytoSend(
                mPeerConnectionId, iceCandiateReadytoSendDelegate);
        }
        private void RaiseLocalDataChannelReady()
        {
            if (OnLocalDataChannelReady != null)
                OnLocalDataChannelReady(mPeerConnectionId);
        }
        private void RaiseDataFromDataChannelReady(string data)
        {
            if (OnDataFromDataChannelReady != null)
                OnDataFromDataChannelReady(mPeerConnectionId, data);
        }
        private void RaiseFailureMessage(string msg)
        {
            if (OnFailureMessage != null)
                OnFailureMessage(mPeerConnectionId, msg);
        }
        private void RaiseAudioBusReady(IntPtr data, int bitsPerSample,
          int sampleRate, int numberOfChannels, int numberOfFrames)
        {
            if (OnAudioBusReady != null)
                OnAudioBusReady(mPeerConnectionId, data, bitsPerSample, sampleRate,
                    numberOfChannels, numberOfFrames);
        }
        private void RaiseLocalVideoFrameReady(
            IntPtr dataY, IntPtr dataU, IntPtr dataV, IntPtr dataA,
            int strideY, int strideU, int strideV, int strideA,
            uint width, uint height)
        {
            if (OnLocalVideoFrameReady != null)
                OnLocalVideoFrameReady(mPeerConnectionId, dataY, dataU, dataV, dataA, strideY, strideU, strideV, strideA,
                  width, height);
        }
        private void RaiseRemoteVideoFrameReady(
           IntPtr dataY, IntPtr dataU, IntPtr dataV, IntPtr dataA,
           int strideY, int strideU, int strideV, int strideA,
           uint width, uint height)
        {
            if (OnRemoteVideoFrameReady != null)
                OnRemoteVideoFrameReady(mPeerConnectionId, dataY, dataU, dataV, dataA, strideY, strideU, strideV, strideA,
                  width, height);
        }
        private void RaiseLocalSdpReadytoSend(string type, string sdp)
        {
            if (OnLocalSdpReadytoSend != null)
                OnLocalSdpReadytoSend(mPeerConnectionId, type, sdp, senderId, mediaType);
        }
        private void RaiseIceCandiateReadytoSend(string candidate, int sdpMLineIndex, string sdpMid)
        {
            if (OnIceCandidateReadytoSend != null)
                OnIceCandidateReadytoSend(mPeerConnectionId, candidate, sdpMLineIndex, sdpMid, senderId, mediaType);
        }
        //public void AddQueuedIceCandidate(List<IceCandidate> iceCandidateQueue)
        //{
        //    if (iceCandidateQueue != null)
        //    {
        //        foreach (IceCandidate ic in iceCandidateQueue)
        //        {
        //            AddIceCandidate(mPeerConnectionId, ic.Candidate, ic.sdpMLineIndex, ic.SdpMid);
        //        }
        //    }
        //}
        private LocalDataChannelReadyInternalDelegate localDataChannelReadyDelegate = null;
        public event LocalDataChannelReadyDelegate OnLocalDataChannelReady;
        private DataFromDataChannelReadyInternalDelegate dataFromDataChannelReadyDelegate = null;
        public event DataFromDataChannelReadyDelegate OnDataFromDataChannelReady;
        private FailureMessageInternalDelegate failureMessageDelegate = null;
        public event FailureMessageDelegate OnFailureMessage;
        private AudioBusReadyInternalDelegate audioBusReadyDelegate = null;
        public event AudioBusReadyDelegate OnAudioBusReady;
        private I420FrameReadyInternalDelegate localI420FrameReadyDelegate = null;
        public event I420FrameReadyDelegate OnLocalVideoFrameReady;
        private I420FrameReadyInternalDelegate remoteI420FrameReadyDelegate = null;
        public event I420FrameReadyDelegate OnRemoteVideoFrameReady;
        private LocalSdpReadytoSendInternalDelegate localSdpReadytoSendDelegate = null;
        public event LocalSdpReadytoSendDelegate OnLocalSdpReadytoSend;
        private IceCandiateReadytoSendInternalDelegate iceCandiateReadytoSendDelegate = null;
        public event IceCandiateReadytoSendDelegate OnIceCandidateReadytoSend;
        private int mPeerConnectionId = -1;
    }
}
