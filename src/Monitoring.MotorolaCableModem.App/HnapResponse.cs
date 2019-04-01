using System;
using System.Collections.Generic;

namespace Monitoring.MotorolaCableModem.App
{
    public class HnapResponse
    {
        public GetMultipleHnapsResponse GetMultipleHNAPsResponse { get; set; }
    }

    public class GetMultipleHnapsResponse
    {
        public GetMotoStatusStartupSequenceResponse GetMotoStatusStartupSequenceResponse { get; set; }
        public GetMotoStatusConnectionInfoResponse GetMotoStatusConnectionInfoResponse { get; set; }
        public GetMotoStatusDownstreamChannelInfoResponse GetMotoStatusDownstreamChannelInfoResponse { get; set; }
        public GetMotoStatusUpstreamChannelInfoResponse GetMotoStatusUpstreamChannelInfoResponse { get; set; }
        public GetMotoLagStatusResponse GetMotoLagStatusResponse { get; set; }
        public string GetMultipleHNAPsResult { get; set; }
    }

    public class GetMotoStatusStartupSequenceResponse
    {
        public string MotoConnDSFreq { get; set; }
        public string MotoConnDSComment { get; set; }
        public string MotoConnConnectivityStatus { get; set; }
        public string MotoConnConnectivityComment { get; set; }
        public string MotoConnBootStatus { get; set; }
        public string MotoConnBootComment { get; set; }
        public string MotoConnConfigurationFileStatus { get; set; }
        public string MotoConnConfigurationFileComment { get; set; }
        public string MotoConnSecurityStatus { get; set; }
        public string MotoConnSecurityComment { get; set; }
        public string GetMotoStatusStartupSequenceResult { get; set; }
    }

    public class GetMotoStatusConnectionInfoResponse
    {
        public string MotoConnSystemUpTime { get; set; }
        public string MotoConnNetworkAccess { get; set; }
        public string GetMotoStatusConnectionInfoResult { get; set; }
    }

    public class GetMotoStatusDownstreamChannelInfoResponse
    {
        public string MotoConnDownstreamChannel { get; set; }
        public string GetMotoStatusDownstreamChannelInfoResult { get; set; }
    }

    public class GetMotoStatusUpstreamChannelInfoResponse
    {
        public string MotoConnUpstreamChannel { get; set; }
        public string GetMotoStatusUpstreamChannelInfoResult { get; set; }
    }

    public class GetMotoLagStatusResponse
    {
        public string MotoLagCurrentStatus { get; set; }
        public string GetMotoLagStatusResult { get; set; }
    }

}
