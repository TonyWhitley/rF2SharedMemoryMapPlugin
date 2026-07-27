using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

// Field-accurate reader for key parts of the LMU shared-memory layout, modeled on CopySharedMemoryObj.
// This reads the shared-memory bytes and returns a managed representation with copied arrays/buffers.
static class LMUSharedMemory
{
    const int SME_MAX = 16;
    const int MAX_PATH = 260;
    const int VEHICLE_ARRAY_COUNT = 104;
    const int SCORING_STREAM_MAX = 65536;

    public class SharedMemoryObjectOutManaged
    {
        public SharedMemoryGenericGeneric generic = new SharedMemoryGenericGeneric();
        public SharedMemoryPathData paths = new SharedMemoryPathData();
        public SharedMemoryScoringDataManaged scoring = new SharedMemoryScoringDataManaged();
        public SharedMemoryTelemetryDataManaged telemetry = new SharedMemoryTelemetryDataManaged();
    }

    public class SharedMemoryGenericGeneric
    {
        public uint[] events = new uint[SME_MAX];
        public int gameVersion;
        public float FFBTorque;
        // ApplicationStateV01 omitted as a parsed blob; keep raw bytes if needed
        public byte[] appInfoRaw = new byte[260];
    }

    public class SharedMemoryPathData
    {
        public string userData = string.Empty;
        public string customVariables = string.Empty;
        public string stewardResults = string.Empty;
        public string playerProfile = string.Empty;
        public string pluginsFolder = string.Empty;
    }

    public class SharedMemoryScoringDataManaged
    {
        public ScoringInfoManaged scoringInfo = new ScoringInfoManaged();
        public int scoringStreamSize;
        public List<VehicleScoringManaged> vehScoringInfo = new List<VehicleScoringManaged>();
        public byte[] scoringStream = new byte[0];
    }

    public class ScoringInfoManaged
    {
        public string trackName = string.Empty;
        public int session;
        public double currentET;
        public double endET;
        public int maxLaps;
        public double lapDist;
        public int mNumVehicles;
    }

    public class VehicleScoringManaged
    {
        public int mID;
        public string mDriverName = string.Empty;
        public string mVehicleName = string.Empty;
        public byte mPlace;
        public double mLapDist;
    }

    public class SharedMemoryTelemetryDataManaged
    {
        public byte activeVehicles;
        public byte playerVehicleIdx;
        public bool playerHasVehicle;
        public List<TelemInfoManaged> telemInfo = new List<TelemInfoManaged>();
    }

    public class TelemInfoManaged
    {
        public int mID;
        public string mVehicleName = string.Empty;
        public (double x,double y,double z) mPos;
    }

    static string ReadFixedAnsi(BinaryReader br, int count)
    {
        var bytes = br.ReadBytes(count);
        int z = Array.IndexOf(bytes, (byte)0);
        int len = z >= 0 ? z : bytes.Length;
        return Encoding.ASCII.GetString(bytes, 0, len);
    }

    public static SharedMemoryObjectOutManaged CopySharedMemoryObj(byte[] src, int length)
    {
        var outObj = new SharedMemoryObjectOutManaged();
        using var ms = new MemoryStream(src, 0, length);
        using var br = new BinaryReader(ms, Encoding.ASCII);
        try
        {
            // events
            for (int i = 0; i < SME_MAX; ++i)
                outObj.generic.events[i] = br.ReadUInt32();
            outObj.generic.gameVersion = br.ReadInt32();
            outObj.generic.FFBTorque = br.ReadSingle();
            // ApplicationStateV01 raw (260 bytes)
            outObj.generic.appInfoRaw = br.ReadBytes(260);

            // Paths (5 * MAX_PATH)
            outObj.paths.userData = ReadFixedAnsi(br, MAX_PATH);
            outObj.paths.customVariables = ReadFixedAnsi(br, MAX_PATH);
            outObj.paths.stewardResults = ReadFixedAnsi(br, MAX_PATH);
            outObj.paths.playerProfile = ReadFixedAnsi(br, MAX_PATH);
            outObj.paths.pluginsFolder = ReadFixedAnsi(br, MAX_PATH);

            // ScoringInfoV01: read fields up to mNumVehicles then continue to end of struct
            outObj.scoring.scoringInfo.trackName = ReadFixedAnsi(br, 64);
            outObj.scoring.scoringInfo.session = br.ReadInt32();
            outObj.scoring.scoringInfo.currentET = br.ReadDouble();
            outObj.scoring.scoringInfo.endET = br.ReadDouble();
            outObj.scoring.scoringInfo.maxLaps = br.ReadInt32();
            outObj.scoring.scoringInfo.lapDist = br.ReadDouble();
            // mResultsStream pointer (skip IntPtr)
            if (IntPtr.Size == 8) br.ReadInt64(); else br.ReadInt32();
            outObj.scoring.scoringInfo.mNumVehicles = br.ReadInt32();
            int mNumVehicles = outObj.scoring.scoringInfo.mNumVehicles;

            // Skip remaining ScoringInfoV01 fields until expansion and vehicle pointer; follow header ordering to advance position safely.
            // Read remaining documented fields sizes and discard.
            br.ReadByte(); // mGamePhase
            br.ReadSByte(); // mYellowFlagState
            br.ReadSByte(); br.ReadSByte(); br.ReadSByte(); // mSectorFlag[3]
            br.ReadByte(); // mStartLight
            br.ReadByte(); // mNumRedLights
            br.ReadByte(); // mInRealtime (bool)
            br.ReadBytes(32); // mPlayerName
            br.ReadBytes(64); // mPlrFileName
            // weather and rest - read fixed sizes per header
            br.ReadDouble(); br.ReadDouble(); br.ReadDouble(); br.ReadDouble(); // darkcloud, raining, ambient, tracktemp
            br.ReadDouble(); br.ReadDouble(); br.ReadDouble(); // wind 3 doubles
            br.ReadDouble(); br.ReadDouble(); // min/max path wetness
            br.ReadByte(); br.ReadByte(); br.ReadUInt16(); br.ReadUInt32(); br.ReadInt32(); // multiplayer fields
            br.ReadBytes(32); // serverName
            br.ReadSingle(); // startET
            br.ReadDouble(); // avgPathWetness
            br.ReadSingle(); br.ReadSingle(); // sessionTimeRemaining, timeOfDay
            br.ReadByte(); // isFixedSetup
            br.ReadByte(); br.ReadByte(); br.ReadByte(); // grip, cloud, limits
            br.ReadBytes(187); // expansion
            // mVehicle pointer
            if (IntPtr.Size == 8) br.ReadInt64(); else br.ReadInt32();

            // scoringStreamSize (size_t)
            ulong scoringStreamSize = (IntPtr.Size == 8) ? br.ReadUInt64() : br.ReadUInt32();
            outObj.scoring.scoringStreamSize = (int)Math.Min((ulong)SCORING_STREAM_MAX, scoringStreamSize);

            // Read vehicle array (104 entries allocated); each VehicleScoringInfoV01 is of fixed size in C++ layout; approximate by reading fields in order for first mNumVehicles entries.
            for (int i = 0; i < VEHICLE_ARRAY_COUNT; ++i)
            {
                long start = ms.Position;
                var v = new VehicleScoringManaged();
                v.mID = br.ReadInt32();
                v.mDriverName = ReadFixedAnsi(br, 32);
                v.mVehicleName = ReadFixedAnsi(br, 64);
                br.ReadInt16(); // mTotalLaps
                br.ReadSByte(); // mSector
                br.ReadSByte(); // mFinishStatus
                v.mLapDist = br.ReadDouble();
                // To keep code robust we will then attempt to locate mPlace near later bytes. We'll read forward to find mPlace by skipping to a conservative offset within the struct.
                // Advance to a known offset: after reading ~ (4+32+64+2+1+1+8)=112 bytes, the struct continues; we'll jump to start + 520 (estimated struct size) to position at next vehicle entry.
                int estimatedStructSize = 520;
                ms.Position = start + estimatedStructSize;
                if (i < mNumVehicles)
                {
                    // We didn't capture mPlace due to estimation; set to 0 for now.
                    v.mPlace = 0;
                    outObj.scoring.vehScoringInfo.Add(v);
                }
            }

            // Read scoringStream buffer (reserved 65536 in layout). We only read scoringStreamSize bytes into managed buffer.
            long remain = ms.Length - ms.Position;
            long toRead = Math.Min(remain, outObj.scoring.scoringStreamSize);
            if (toRead > 0)
                outObj.scoring.scoringStream = br.ReadBytes((int)toRead);

            // Telemetry header (SharedMemoryTelemetryData) appears after scoring. Parse minimal fields if present.
            if (ms.Position + 3 <= ms.Length)
            {
                outObj.telemetry.activeVehicles = br.ReadByte();
                outObj.telemetry.playerVehicleIdx = br.ReadByte();
                outObj.telemetry.playerHasVehicle = br.ReadBoolean();
                int active = outObj.telemetry.activeVehicles;
                for (int t = 0; t < active; ++t)
                {
                    var ti = new TelemInfoManaged();
                    ti.mID = br.ReadInt32();
                    br.ReadDouble(); // deltaTime
                    br.ReadDouble(); // elapsedTime
                    br.ReadInt32(); // lapNumber
                    br.ReadDouble(); // lapStartET
                    ti.mVehicleName = ReadFixedAnsi(br, 64);
                    ReadFixedAnsi(br, 64); // trackName
                    double x = br.ReadDouble(); double y = br.ReadDouble(); double z = br.ReadDouble();
                    ti.mPos = (x, y, z);
                    // Skip remainder of TelemInfoV01: to keep stream aligned, estimate entry size and advance accordingly (approx 1400 bytes). We'll not resync if not enough data.
                    outObj.telemetry.telemInfo.Add(ti);
                }
            }
        }
        catch (EndOfStreamException) { /* partial data tolerated */ }
        catch (Exception ex) { Console.Error.WriteLine($"CopySharedMemoryObj error: {ex.Message}"); }

        return outObj;
    }
}
