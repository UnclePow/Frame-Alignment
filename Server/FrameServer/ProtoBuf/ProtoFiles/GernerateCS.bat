set "SRC_DIR=C:\Users\18757\source\repos\FrameServer\FrameServer\ProtoBuf\ProtoFiles"
set "DST_DIR1=C:\Users\18757\source\repos\FrameServer\FrameServer\ProtoBuf"
set "DST_DIR2=E:\Files\unity3d\FrameAlignment\Assets\Scripts\ProtoBuf"

protoc -I="%SRC_DIR%" --csharp_out="%DST_DIR1%" --csharp_out="%DST_DIR2%" PB_Login.proto 
protoc -I="%SRC_DIR%" --csharp_out="%DST_DIR1%" --csharp_out="%DST_DIR2%" PB_Match.proto 
protoc -I="%SRC_DIR%" --csharp_out="%DST_DIR1%" --csharp_out="%DST_DIR2%" PB_Battle.proto 
protoc -I="%SRC_DIR%" --csharp_out="%DST_DIR1%" --csharp_out="%DST_DIR2%" PB_Operation.proto 

pause
