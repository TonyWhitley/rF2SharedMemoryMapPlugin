/******************************************************************************
 *  Copyright (c) 2025 Studio 397 BV and Motorsport Games Inc.
 *  All rights reserved.
 *
 *  This header is part of the Studio 397 Plugin SDK. It may be used solely
 *  for the purpose of developing plugins or extensions for supported Studio 397
 *  products. Redistribution or modification of this header is not permitted.
 *
 *  This file contains proprietary information of Studio 397 B.V. and is
 *  provided on a strictly "as is" basis, without warranty of any kind, either
 *  express or implied. Studio 397 B.V. shall not be liable for any damages
 *  arising out of the use of this file or any plugins created with it.
 ******************************************************************************/
#pragma once
#include <cstdint>    // for uint32_t, uint8_t
#include <optional>   // for std::optional
#include <utility>    // for std::exchange
#include <iostream>   // for std::cerr, std::endl
#include <string>     // for std::stoul
#include "SharedMemoryInterface.hpp"
#include "InternalsPlugin.hpp"

int main(int argc, char* argv[])
{
    int retVal = 0;
    if (argc < 2) {
        std::cerr << "Usage: child.exe <LMU-pid>\n";
        return 1;
    }
    // Get the LMU Handle
    DWORD parentPid = 0;
    try {
        parentPid = static_cast<DWORD>(std::stoul(argv[1]));
    }
    catch (...) {
        std::cerr << "Invalid parent PID argument.\n";
        return 1;
    }
    auto smLock = SharedMemoryLock::MakeSharedMemoryLock();
    //if (!smLock.has_value()) {
    //    std::cerr << "Cannot initialize SharedMemoryLock.\n";
    //    return 1;
    //}
    static SharedMemoryObjectOut copiedMem;

    //  LMU INTERNAL SMM 

    // Try to open a handle to the parent process with SYNCHRONIZE right.
    // SYNCHRONIZE is enough to wait on the process handle for exit.
    HANDLE hParent = OpenProcess(SYNCHRONIZE | PROCESS_QUERY_LIMITED_INFORMATION, FALSE, parentPid);
    HANDLE hEvent = OpenEventA(SYNCHRONIZE, FALSE, "LMU_Data_Event");
    HANDLE hMapFile = OpenFileMapping(FILE_MAP_ALL_ACCESS, FALSE, L"LMU_Data");
    if (hParent && hEvent && hMapFile) {
          if (SharedMemoryLayout* pBuf = (SharedMemoryLayout*)MapViewOfFile(hMapFile, FILE_MAP_ALL_ACCESS, 0, 0, sizeof(SharedMemoryLayout))) {
            HANDLE objectHandlesArray[2] = { hParent, hEvent };
            for (DWORD waitObject = WaitForMultipleObjects(2, objectHandlesArray, FALSE, INFINITE); waitObject != WAIT_OBJECT_0; waitObject = WaitForMultipleObjects(2, objectHandlesArray, FALSE, INFINITE)) {
                if (waitObject == WAIT_OBJECT_0 + 1) {
                    smLock.Lock();
                    CopySharedMemoryObj(copiedMem, pBuf->data);
                    smLock.Unlock();
                    // >>>>> ProcessSharedMemory(copiedMem); <<<<<<
                }
                else {
                    std::cerr << "Wait failed: " << GetLastError() << "\n";
                    break;
                }
            }
            UnmapViewOfFile(pBuf);
        }
        else {
            std::cerr << "Could not map view of file. Error: " << GetLastError() << std::endl;
            retVal = 1;
        }
    }
    else {
        std::cerr << "Something went wrong durin initialization. Error: " << GetLastError() << std::endl;
        retVal = 1;
    }
    if (hMapFile)
        CloseHandle(hMapFile);
    if (hEvent)
        CloseHandle(hEvent);
    if (hParent)
        CloseHandle(hParent);

    //  END OF LMU INTERNAL SMM 

    //  HACKING TO USE CC SMM (partially)
    //  Internal version provides all buffers, CC provides them as 8 or 9 individual ones.
    //  Access a CC buffer using the same procedure

    // Try to open a handle to the parent process with SYNCHRONIZE right.
    // SYNCHRONIZE is enough to wait on the process handle for exit.
    HANDLE hParent = OpenProcess(SYNCHRONIZE | PROCESS_QUERY_LIMITED_INFORMATION, FALSE, parentPid);
    HANDLE hEvent = OpenEventA(SYNCHRONIZE, FALSE, "LMU_Data_Event");
    HANDLE hMapFile = OpenFileMapping(FILE_MAP_ALL_ACCESS, FALSE, L"$LMU.3.8.SMMP_Telemetry$"); //!!!!!!!!!!!!!!!!!!! L"LMU_Data");
    if (hParent && hEvent && hMapFile) {
      //!!!!!!!!!!! Note that Scoring didn't map...
      if (SharedMemoryTelemetryData* pBuf = (SharedMemoryTelemetryData*)MapViewOfFile(hMapFile, FILE_MAP_ALL_ACCESS, 0, 0, sizeof(SharedMemoryTelemetryData))) {
        //!!!!!!!!!!!!   if (SharedMemoryLayout* pBuf = (SharedMemoryLayout*)MapViewOfFile(hMapFile, FILE_MAP_ALL_ACCESS, 0, 0, sizeof(SharedMemoryLayout))) {
        HANDLE objectHandlesArray[2] = { hParent, hEvent };
        for (DWORD waitObject = WaitForMultipleObjects(2, objectHandlesArray, FALSE, INFINITE); waitObject != WAIT_OBJECT_0; waitObject = WaitForMultipleObjects(2, objectHandlesArray, FALSE, INFINITE)) {
          if (waitObject == WAIT_OBJECT_0 + 1) {
            smLock.Lock();
            //                    CopySharedMemoryObj(copiedMem, pBuf->data);
            smLock.Unlock();
            // >>>>> ProcessSharedMemory(copiedMem); <<<<<<
          }
          else {
            std::cerr << "Wait failed: " << GetLastError() << "\n";
            break;
          }
        }
        UnmapViewOfFile(pBuf);
      }
      else {
        std::cerr << "Could not map view of file. Error: " << GetLastError() << std::endl;
        retVal = 1;
      }
    }
    else {
      std::cerr << "Something went wrong durin initialization. Error: " << GetLastError() << std::endl;
      retVal = 1;
    }
    if (hMapFile)
      CloseHandle(hMapFile);
    if (hEvent)
      CloseHandle(hEvent);
    if (hParent)
      CloseHandle(hParent);

    return retVal;
}
