using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

class Program
{
    const string LMU_SHARED_MEMORY_FILE = "LMU_Data";
    const string LMU_SHARED_MEMORY_EVENT = "LMU_Data_Event";

    static int Main(string[] args)
    {
        int parentPid;
        if (args.Length >= 1)
        {
            if (!int.TryParse(args[0], out parentPid))
            {
                Console.Error.WriteLine("Invalid parent PID argument.");
                return 1;
            }
        }
        else
        {
            var found = FindLMUPid();
            if (!found.HasValue)
            {
                Console.Error.WriteLine("Could not locate 'Le Mans Ultimate.exe' process. Provide PID as argument or start the game.");
                return 1;
            }
            parentPid = found.Value;
            Console.WriteLine($"Found Le Mans Ultimate.exe PID: {parentPid}");
        }

        try
        {
            // Open existing memory-mapped file and event created by the LMU process.
            using var mmf = MemoryMappedFile.OpenExisting(LMU_SHARED_MEMORY_FILE);
            using EventWaitHandle lmuEvent = EventWaitHandle.OpenExisting(LMU_SHARED_MEMORY_EVENT);

            var parent = Process.GetProcessById(parentPid);
            using var parentExited = new AutoResetEvent(false);
            parent.EnableRaisingEvents = true;
            parent.Exited += (s, e) => parentExited.Set();

            WaitHandle[] waitHandles = new WaitHandle[] { parentExited, lmuEvent };

            Console.WriteLine("Listening for LMU shared-memory events. Press Ctrl+C to quit.");

            while (true)
            {
                int signaled = WaitHandle.WaitAny(waitHandles);
                if (signaled == 0)
                {
                    Console.WriteLine("Parent process exited - shutting down.");
                    break;
                }
                else if (signaled == 1)
                {
                    // LMU signalled new data is available. Read the memory-mapped file.
                    try
                    {
                        using var stream = mmf.CreateViewStream();
                        long length = stream.Length;
                        if (length <= 0)
                        {
                            Console.Error.WriteLine("Mapped view has zero length.");
                            continue;
                        }
                        byte[] buffer = new byte[Math.Min(length, 1024 * 1024)]; // read up to 1 MB
                        int read = stream.Read(buffer, 0, buffer.Length);
                        ProcessSharedMemory(buffer, read);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error reading shared memory: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Initialization failed: {ex.Message}");
            return 1;
        }

        return 0;
    }

    static void ProcessSharedMemory(byte[] data, int length)
    {
    var handleBuffer = GCHandle.Alloc(data, GCHandleType.Pinned);
    var mappedData = (lmuSharedMemory.LMUData.SharedMemoryObjectOut)Marshal.PtrToStructure(handleBuffer.AddrOfPinnedObject(), typeof(lmuSharedMemory.LMUData.SharedMemoryObjectOut));
    handleBuffer.Free();

    Console.WriteLine($"Circuit: {mappedData.scoring.scoringInfo.mTrackName}");
    var v = mappedData.scoring.vehScoringInfo[0];
    Console.WriteLine($"Vehicle[0]: ID={v.mID}, Driver='{v.mDriverName}', Veh='{v.mVehicleName}', Place={v.mPlace}, LapDist={v.mLapDist}");

    Console.WriteLine($"Shared memory update: {length} bytes");
        //try
        //{
        //    var parsed = LMUSharedMemory.CopySharedMemoryObj(data, length);
        //    // Events
        //    for (int i = 0; i < parsed.generic.events.Length; i++)
        //    {
        //        if (parsed.generic.events[i] != 0)
        //            Console.WriteLine($"Event[{i}] = {parsed.generic.events[i]}");
        //    }
        //    // Paths
        //    if (!string.IsNullOrEmpty(parsed.paths.userData)) Console.WriteLine($"Path[userData]: {parsed.paths.userData}");
        //    if (!string.IsNullOrEmpty(parsed.paths.pluginsFolder)) Console.WriteLine($"Path[plugins]: {parsed.paths.pluginsFolder}");
        //    // Scoring summary
        //    Console.WriteLine($"Scoring: numVehicles={parsed.scoring.scoringInfo.mNumVehicles}, streamBytes={parsed.scoring.scoringStream?.Length ?? 0}");
        //    if (parsed.scoring.vehScoringInfo.Count > 0)
        //    {
        //        var v = parsed.scoring.vehScoringInfo[0];
        //        Console.WriteLine($"Vehicle[0]: ID={v.mID}, Driver='{v.mDriverName}', Veh='{v.mVehicleName}', Place={v.mPlace}, LapDist={v.mLapDist}");
        //    }
        //    // Telemetry summary
        //    Console.WriteLine($"Telemetry: activeVehicles={parsed.telemetry.activeVehicles}");
        //    if (parsed.telemetry.telemInfo.Count > 0)
        //    {
        //        var t = parsed.telemetry.telemInfo[0];
        //        Console.WriteLine($"Telem[0]: ID={t.mID}, Veh='{t.mVehicleName}', Pos=({t.mPos.x:0.###},{t.mPos.y:0.###},{t.mPos.z:0.###})");
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Console.Error.WriteLine($"Error parsing shared memory: {ex.Message}");
        //}
    }

    static int? FindLMUPid()
    {
        foreach (var p in Process.GetProcesses())
        {
            try
            {
                string path = p.MainModule?.FileName;
                if (!string.IsNullOrEmpty(path) && path.EndsWith("Le Mans Ultimate.exe", StringComparison.OrdinalIgnoreCase))
                    return p.Id;
            }
            catch { /* ignore inaccessible processes */ }
        }
        return null;
    }
}
