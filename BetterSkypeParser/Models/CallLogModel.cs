using System;
using System.Collections.Generic;

namespace BetterSkypeParser.Models
{
#pragma warning disable IDE1006
    public class CallLogModel
    {
        public DateTime? startTime { get; set; }
        public DateTime? connectTime { get; set; }
        public DateTime? endTime { get; set; }
        public string? callDirection { get; set; }
        public string? callType { get; set; }
        public string? callState { get; set; }
        public string? originator { get; set; }
        public string? target { get; set; }
        public Participant? originatorParticipant { get; set; }
        public Participant? targetParticipant { get; set; }
        public string? callId { get; set; }
        public object? callAttributes { get; set; }
        public object? forwardingInfo { get; set; }
        public object? transferInfo { get; set; }
        public List<string>? participants { get; set; }
        public List<Participant>? participantList { get; set; }
        public string? threadId { get; set; }
        public string? sessionType { get; set; }
        public string? sharedCorrelationId { get; set; }
        public string? messageId { get; set; }
    }

    public class Participant
    {
        public string? id { get; set; }
        public string? type { get; set; }
        public object? displayName { get; set; }
        public object? applicationType { get; set; }
    }
#pragma warning restore IDE1006
}
