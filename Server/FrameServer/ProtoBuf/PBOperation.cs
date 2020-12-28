// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: PB_Operation.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
/// <summary>Holder for reflection information generated from PB_Operation.proto</summary>
public static partial class PBOperationReflection {

  #region Descriptor
  /// <summary>File descriptor for PB_Operation.proto</summary>
  public static pbr::FileDescriptor Descriptor {
    get { return descriptor; }
  }
  private static pbr::FileDescriptor descriptor;

  static PBOperationReflection() {
    byte[] descriptorData = global::System.Convert.FromBase64String(
        string.Concat(
          "ChJQQl9PcGVyYXRpb24ucHJvdG8qogEKDlBCX1JlcXVlc3RDb2RlEgkKBUxP",
          "R0lOEAASCQoFTUFUQ0gQARIQCgxNQVRDSF9DQU5DTEUQAhIQCgxCQVRUTEVf",
          "UkVBRFkQAxIXChNVUF9QTEFZRVJfT1BFUkFUSU9OEAQSFwoTUkVRVUVTVF9M",
          "QUNLX0ZSQU1FUxAFEhUKEVJFUVVFU1RfR0FNRV9PVkVSEAYSDQoJUkVDT05O",
          "RUNUEAcq3gEKD1BCX1Jlc3BvbnNlQ29kZRISCg5MT0dJTl9SRVNQT05TRRAA",
          "EhIKDk1BVENIX1JFU1BPTlNFEAESGQoVTUFUQ0hfQ0FOQ0xFX1JFU1BPTlNF",
          "EAISEAoMQkFUVExFX0VOVEVSEAMSEAoMQkFUVExFX1NUQVJUEAQSGgoWRE9X",
          "Tl9QTEFZRVJfT1BFUkFUSU9OUxAFEhgKFFJFU1BPTlNFX0xBQ0tfRlJBTUVT",
          "EAYSFgoSUkVTUE9OU0VfR0FNRV9PVkVSEAcSFgoSUkVDT05ORUNUX1JFU1BP",
          "TlNFEAhiBnByb3RvMw=="));
    descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
        new pbr::FileDescriptor[] { },
        new pbr::GeneratedClrTypeInfo(new[] {typeof(global::PB_RequestCode), typeof(global::PB_ResponseCode), }, null, null));
  }
  #endregion

}
#region Enums
public enum PB_RequestCode {
  [pbr::OriginalName("LOGIN")] Login = 0,
  [pbr::OriginalName("MATCH")] Match = 1,
  [pbr::OriginalName("MATCH_CANCLE")] MatchCancle = 2,
  [pbr::OriginalName("BATTLE_READY")] BattleReady = 3,
  [pbr::OriginalName("UP_PLAYER_OPERATION")] UpPlayerOperation = 4,
  [pbr::OriginalName("REQUEST_LACK_FRAMES")] RequestLackFrames = 5,
  [pbr::OriginalName("REQUEST_GAME_OVER")] RequestGameOver = 6,
  [pbr::OriginalName("RECONNECT")] Reconnect = 7,
}

public enum PB_ResponseCode {
  [pbr::OriginalName("LOGIN_RESPONSE")] LoginResponse = 0,
  [pbr::OriginalName("MATCH_RESPONSE")] MatchResponse = 1,
  [pbr::OriginalName("MATCH_CANCLE_RESPONSE")] MatchCancleResponse = 2,
  [pbr::OriginalName("BATTLE_ENTER")] BattleEnter = 3,
  [pbr::OriginalName("BATTLE_START")] BattleStart = 4,
  [pbr::OriginalName("DOWN_PLAYER_OPERATIONS")] DownPlayerOperations = 5,
  [pbr::OriginalName("RESPONSE_LACK_FRAMES")] ResponseLackFrames = 6,
  [pbr::OriginalName("RESPONSE_GAME_OVER")] ResponseGameOver = 7,
  [pbr::OriginalName("RECONNECT_RESPONSE")] ReconnectResponse = 8,
}

#endregion


#endregion Designer generated code
