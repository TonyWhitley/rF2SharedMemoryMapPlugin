#include "rFactor2SharedMemoryMap.hpp"
#include <stdlib.h>
#include <cstddef>                              // offsetof
#include "DirectMemoryReader.h"
#include "Utils.h"

bool DirectMemoryReader::Initialize()
{
  __try {
    DEBUG_MSG(DebugLevel::DevInfo, "Initializing DMR.");

    auto const startTicks = TicksNow();

    auto const module = ::GetModuleHandle(nullptr);
    mpStatusMessage = reinterpret_cast<char*>(Utils::FindPatternForPointerInMemory(module,
      reinterpret_cast<unsigned char*>("\x74\x23\x48\x8D\x15\x5D\x31\xF5\x00\x48\x2B\xD3"),
      "xxxxx????xxx", 5u));

    if (mpStatusMessage == nullptr) {
      DEBUG_MSG(DebugLevel::Errors, "ERROR: Failed to resolve status message.");
      return false;
    }

    mppMessageCenterMessages = reinterpret_cast<char**>(Utils::FindPatternForPointerInMemory(module,
      reinterpret_cast<unsigned char*>("\x48\x8B\x05\xAA\xD1\xFF\x00\xC6\x80\xB8\x25\x00\x00\x01"),
      "xxx????xxxxxxx", 3u));

    if (mppMessageCenterMessages == nullptr) {
      DEBUG_MSG(DebugLevel::Errors, "ERROR: Failed to resolve message array.");
      return false;
    }

    mpCurrPitSpeedLimit = reinterpret_cast<float*>(Utils::FindPatternForPointerInMemory(module,
      reinterpret_cast<unsigned char*>("\x57\x48\x83\xEC\x20\xF3\x0F\x10\x2D\x35\x9C\xEF\x00\x48\x8B\xF1"),
      "xxxxxxxxx????xxx", 9u));

    if (mpCurrPitSpeedLimit == nullptr) {
      DEBUG_MSG(DebugLevel::Errors, "ERROR: Failed to resolve speed limit pointer.");
      return false;
    }

    ReadSCRPluginConfig();

    mpLSIMessages = reinterpret_cast<char*>(Utils::FindPatternForPointerInMemory(module,
      reinterpret_cast<unsigned char*>("\x42\x88\x8C\x38\x0F\x02\x00\x00\x84\xC9\x75\xEB\x48\x8D\x15\x70\x3A\x05\x01\x48\x8D\x0D\xB1\x20\x05\x01\xE8"),
      "xxxxxxxxxxxxxxx????xxx????x", 22u));

    if (mpLSIMessages == nullptr) {
      DEBUG_MSG(DebugLevel::Errors, "ERROR: Failed to resolve LSI message pointer.");
      return false;
    }

    auto const pRoot = reinterpret_cast<char*>(Utils::FindPatternForPointerInMemory(module,
      reinterpret_cast<unsigned char*>("\x33\xDB\x4C\x8D\x3D\xC7\x3F\x06\x01\x8B\xF3\x45\x85\xF6"),
      "xxxxx????xxxxx", 5u));

    if (pRoot == nullptr) {
      DEBUG_MSG(DebugLevel::Errors, "ERROR: Failed to resolve SC speed pointer.");
      return false;
    }

    mpSCSpeed = reinterpret_cast<float*>(pRoot + 0xC7C0);

    auto const endTicks = TicksNow();

    if (SharedMemoryPlugin::msDebugOutputLevel >= DebugLevel::DevInfo) {
      // Successful scan: ~20ms
      DEBUG_FLOAT2(DebugLevel::DevInfo, "Scan time seconds: ", (endTicks - startTicks) / MICROSECONDS_IN_SECOND);

      auto const addr1 = reinterpret_cast<char*>(reinterpret_cast<uintptr_t>(::GetModuleHandle(nullptr)) + 0x14D4C20uLL);
      auto const addr2 = *reinterpret_cast<char**>(reinterpret_cast<uintptr_t>(::GetModuleHandle(nullptr)) + 0x14D31E0uLL);
      auto const addr3 = reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(::GetModuleHandle(nullptr)) + 0x14B4D0CuLL);
      auto const addr4 = reinterpret_cast<char*>(reinterpret_cast<uintptr_t>(::GetModuleHandle(nullptr)) + 0x14D3268uLL);
      auto const addr5 = reinterpret_cast<char*>(reinterpret_cast<uintptr_t>(::GetModuleHandle(nullptr)) + 0x14B3750uLL);

      DEBUG_ADDR2(DebugLevel::DevInfo, "A1", mpStatusMessage);
      DEBUG_ADDR2(DebugLevel::DevInfo, "A11", addr1);
      DEBUG_ADDR2(DebugLevel::DevInfo, "O1", reinterpret_cast<uintptr_t>(mpStatusMessage) - reinterpret_cast<uintptr_t>(module));
      DEBUG_ADDR2(DebugLevel::DevInfo, "O11", 0x14D4C20uLL);

      DEBUG_ADDR2(DebugLevel::DevInfo, "A2", *mppMessageCenterMessages);
      DEBUG_ADDR2(DebugLevel::DevInfo, "A21", addr2);
      DEBUG_ADDR2(DebugLevel::DevInfo, "O2", reinterpret_cast<uintptr_t>(mppMessageCenterMessages) - reinterpret_cast<uintptr_t>(module));
      DEBUG_ADDR2(DebugLevel::DevInfo, "O21", 0x14D31E0uLL);

      DEBUG_ADDR2(DebugLevel::DevInfo, "A3", mpCurrPitSpeedLimit);
      DEBUG_ADDR2(DebugLevel::DevInfo, "A31", addr3);
      DEBUG_ADDR2(DebugLevel::DevInfo, "O3", reinterpret_cast<uintptr_t>(mpCurrPitSpeedLimit) - reinterpret_cast<uintptr_t>(module));
      DEBUG_ADDR2(DebugLevel::DevInfo, "O31", 0x14B4D0CuLL);

      DEBUG_ADDR2(DebugLevel::DevInfo, "A4", mpLSIMessages);
      DEBUG_ADDR2(DebugLevel::DevInfo, "A41", addr4);
      DEBUG_ADDR2(DebugLevel::DevInfo, "O4", reinterpret_cast<uintptr_t>(mpLSIMessages) - reinterpret_cast<uintptr_t>(module));
      DEBUG_ADDR2(DebugLevel::DevInfo, "O41", 0x14D3268uLL);

      DEBUG_ADDR2(DebugLevel::DevInfo, "A5", mpSCSpeed);
      DEBUG_ADDR2(DebugLevel::DevInfo, "A51", addr5);
      DEBUG_ADDR2(DebugLevel::DevInfo, "O5", reinterpret_cast<uintptr_t>(mpSCSpeed) - reinterpret_cast<uintptr_t>(module));
      DEBUG_ADDR2(DebugLevel::DevInfo, "O51", 0x14B3750uLL);
    }
  }
  __except (::GetExceptionCode() == EXCEPTION_ACCESS_VIOLATION ? EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH) {
    DEBUG_MSG(DebugLevel::Errors, "ERROR: Exception while reading memory, disabling DMA.");
    return false;
  }

  return true;
}


bool DirectMemoryReader::Read(rF2Extended& extended)
{
  __try {
    if (mpStatusMessage == nullptr || mppMessageCenterMessages == nullptr || mpCurrPitSpeedLimit == nullptr || mpSCSpeed == nullptr) {
      assert(false && "DMR not available, should not call.");
      return false;
    }

    extended.mSCSpeedMS = *mpSCSpeed;
    if (mPrevSCSpeed != extended.mSCSpeedMS) {
      mPrevSCSpeed = extended.mSCSpeedMS;
      DEBUG_FLOAT2(DebugLevel::DevInfo, "SC Speed changed: ", extended.mSCSpeedMS);
    }

    strcpy_s(extended.mStatusMessage, mpStatusMessage);
    if (strcmp(mPrevStatusMessage, extended.mStatusMessage) != 0) {
      DEBUG_MSG2(DebugLevel::DevInfo, "Status message updated: ", extended.mStatusMessage);

      strcpy_s(mPrevStatusMessage, extended.mStatusMessage);
      extended.mTicksStatusMessageUpdated = ::GetTickCount64();
    }

    auto const pBegin = *mppMessageCenterMessages;
    if (pBegin == nullptr) {
      DEBUG_MSG2(DebugLevel::DevInfo, "No message array pointer assigned.", extended.mStatusMessage);
      return true;  // Retry next time or fail?  Have counter for N failures?
    }

    char msgBuff[rF2MappedBufferHeader::MAX_STATUS_MSG_LEN];

    auto seenSplit = false;
    auto pCurr = pBegin + 0xC0 * 0x2F + 0x68;
    auto const pPastEnd = pBegin + 0xC0 * 0x30;
    for (int i = 0;
      i < 0x30 && pCurr >= pBegin;
      ++i) {
      assert(pCurr >= pBegin && pCurr < pPastEnd);

      if (*pCurr != '\0') {
        if (*pCurr == ' ') {  // some messages are split, they begin with space though.
          pCurr -= 0xC0;
          seenSplit = true;
          continue;
        }

        if (seenSplit) {
          strcpy_s(msgBuff, pCurr);

          auto j = i - 1;
          auto pAhead = pCurr + 0xC0;
          assert(pAhead >= pBegin && pAhead < pPastEnd);

          while (j >= 0
            && *pAhead == ' '
            && *pCurr != '\0'
            && pAhead < pPastEnd) {
            assert(pAhead >= pBegin && pAhead < pPastEnd);
            assert(j >= 0 && j < 0x30);

            strcat_s(msgBuff, pAhead);

            --j;
            pAhead += 0xC0;
          }
        }

        if (!seenSplit)
          strcpy_s(extended.mLastHistoryMessage, pCurr);
        else
          strcpy_s(extended.mLastHistoryMessage, msgBuff);

        if (strcmp(mPrevLastHistoryMessage, extended.mLastHistoryMessage) != 0) {
          if (!seenSplit)
            DEBUG_MSG2(DebugLevel::DevInfo, "Last history message updated: ", extended.mLastHistoryMessage);
          else
            DEBUG_MSG2(DebugLevel::DevInfo, "Last history message updated (concatenated): ", extended.mLastHistoryMessage);

          strcpy_s(mPrevLastHistoryMessage, extended.mLastHistoryMessage);
          extended.mTicksLastHistoryMessageUpdated = ::GetTickCount64();
        }

        break;
      }

      pCurr -= 0xC0;
    }
  }
  __except (::GetExceptionCode() == EXCEPTION_ACCESS_VIOLATION ? EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH) {
    DEBUG_MSG(DebugLevel::Errors, "ERROR: Exception while reading memory, disabling DMA.");
    return false;
  }


  return true;
}

bool DirectMemoryReader::ReadOnNewSession(rF2Extended& extended)
{
  __try {
    if (mpStatusMessage == nullptr || mppMessageCenterMessages == nullptr || mpCurrPitSpeedLimit == nullptr || mpSCSpeed == nullptr) {
      assert(false && "DMR not available, should not call.");
      return false;
    }

    OnNewSession(extended);

    extended.mCurrentPitSpeedLimit = *mpCurrPitSpeedLimit;
    DEBUG_FLOAT2(DebugLevel::DevInfo, "Current pit speed limit: ", extended.mCurrentPitSpeedLimit);
  }
  __except (::GetExceptionCode() == EXCEPTION_ACCESS_VIOLATION ? EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH) {
    DEBUG_MSG(DebugLevel::Errors, "ERROR: Excepction while reading memory, disabling DMA.");
    return false;
  }

  return true;
}

bool DirectMemoryReader::ReadOnLSIVisible(rF2Extended& extended)
{
  __try {
    if (mpStatusMessage == nullptr || mppMessageCenterMessages == nullptr || mpCurrPitSpeedLimit == nullptr || mpLSIMessages == nullptr || mpSCSpeed == nullptr) {
      assert(false && "DMR not available, should not call.");
      return false;
    }

    auto const pPhase = mpLSIMessages + 0x50uLL;
    if (pPhase[0] != '\0') {
      strcpy_s(extended.mLSIPhaseMessage, pPhase);
      if (strcmp(mPrevLSIPhaseMessage, extended.mLSIPhaseMessage) != 0) {
        DEBUG_MSG2(DebugLevel::DevInfo, "LSI Phase message updated: ", extended.mLSIPhaseMessage);

        strcpy_s(mPrevLSIPhaseMessage, extended.mLSIPhaseMessage);
        extended.mTicksLSIPhaseMessageUpdated = ::GetTickCount64();
      }
    }

    auto const pPitState = mpLSIMessages + 0xD0uLL;
    if (pPitState[0] != '\0') {
      strcpy_s(extended.mLSIPitStateMessage, pPitState);
      if (strcmp(mPrevLSIPitStateMessage, extended.mLSIPitStateMessage) != 0) {
        DEBUG_MSG2(DebugLevel::DevInfo, "LSI Pit State message updated: ", extended.mLSIPitStateMessage);

        strcpy_s(mPrevLSIPitStateMessage, extended.mLSIPitStateMessage);
        extended.mTicksLSIPitStateMessageUpdated = ::GetTickCount64();
      }
    }

    auto const pOrderInstruction = mpLSIMessages + 0x150uLL;
    if (pOrderInstruction[0] != '\0') {
      strcpy_s(extended.mLSIOrderInstructionMessage, pOrderInstruction);
      if (strcmp(mPrevLSIOrderInstructionMessage, extended.mLSIOrderInstructionMessage) != 0) {
        DEBUG_MSG2(DebugLevel::DevInfo, "LSI Order Instruction message updated: ", extended.mLSIOrderInstructionMessage);

        strcpy_s(mPrevLSIOrderInstructionMessage, extended.mLSIOrderInstructionMessage);
        extended.mTicksLSIOrderInstructionMessageUpdated = ::GetTickCount64();
      }
    }

    auto const pRulesInstruction = mpLSIMessages + 0x1D0uLL;
    if (mSCRPluginEnabled
      && pRulesInstruction[0] != '\0') {
      strcpy_s(extended.mLSIRulesInstructionMessage, pRulesInstruction);
      if (strcmp(mPrevLSIRulesInstructionMessage, extended.mLSIRulesInstructionMessage) != 0) {
        DEBUG_MSG2(DebugLevel::DevInfo, "LSI Rules Instruction message updated: ", extended.mLSIRulesInstructionMessage);

        strcpy_s(mPrevLSIRulesInstructionMessage, extended.mLSIRulesInstructionMessage);
        extended.mTicksLSIRulesInstructionMessageUpdated = ::GetTickCount64();
      }
    }
  }
  __except (::GetExceptionCode() == EXCEPTION_ACCESS_VIOLATION ? EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH)
  {
    DEBUG_MSG(DebugLevel::Errors, "ERROR: Exception while reading memory, disabling DMA.");
    return false;
  }

  return true;
}


void DirectMemoryReader::ReadSCRPluginConfig()
{
  char wd[MAX_PATH] = {};
  ::GetCurrentDirectory(MAX_PATH, wd);

  auto const configFilePath = lstrcatA(wd, R"(\UserData\player\CustomPluginVariables.JSON)");

  auto configFileContents = Utils::GetFileContents(configFilePath);
  if (configFileContents == nullptr) {
    DEBUG_MSG(DebugLevel::Errors, "Failed to load CustomPluginVariables.JSON file");
    return;
  }

  auto onExit = Utils::MakeScopeGuard([&]() {
    delete[] configFileContents;
  });

  ReadSCRPluginConfigValues(configFileContents);
}


void DirectMemoryReader::ReadSCRPluginConfigValues(char* const configFileContents)
{
  // See if plugin is enabled:
  auto curLine = strstr(configFileContents, "StockCarRules.dll");
  while (curLine != nullptr) {
    // Cut off next line from the current text.
    auto const nextLine = strstr(curLine, "\r\n");
    if (nextLine != nullptr)
      *nextLine = '\0';

    auto onExitOrNewIteration = Utils::MakeScopeGuard([&]() {
      // Restore the original line.
      if (nextLine != nullptr)
        *nextLine = '\r';
    });

    auto const closingBrace = strchr(curLine, '}');
    if (closingBrace != nullptr) {
      // End of {} for a plugin.
      return;
    }

    if (!mSCRPluginEnabled) {
      // Check if plugin is enabled.
      auto const enabled = strstr(curLine, " \" Enabled\":1");
      if (enabled != nullptr)
        mSCRPluginEnabled = true;
    }

    if (mSCRPluginDoubleFileType == -1L) {
      auto const dft = strstr(curLine, " \"DoubleFileType\":");
      if (dft != nullptr) {
        char value[2] = {};
        value[0] = *(dft + sizeof("\"DoubleFileType\":"));
        mSCRPluginDoubleFileType = atol(value);
      }
    }

    if (mSCRPluginEnabled && mSCRPluginDoubleFileType != -1L)
      return;

    curLine = nextLine != nullptr ? (nextLine + 2 /*skip \r\n*/) : nullptr;
  }

  // If we're here, consider SCR plugin as not enabled.
  mSCRPluginEnabled = false;
  mSCRPluginDoubleFileType = -1L;

  return;
}

void DirectMemoryReader::OnNewSession(rF2Extended& extended)
{
  mPrevLSIPhaseMessage[0] = '\0';
  extended.mLSIPhaseMessage[0] = '\0';
  extended.mTicksLSIPhaseMessageUpdated = ::GetTickCount64();

  mPrevLSIPitStateMessage[0] = '\0';
  extended.mLSIPitStateMessage[0] = '\0';
  extended.mTicksLSIPitStateMessageUpdated = ::GetTickCount64();

  mPrevLSIOrderInstructionMessage[0] = '\0';
  extended.mLSIOrderInstructionMessage[0] = '\0';
  extended.mTicksLSIOrderInstructionMessageUpdated = ::GetTickCount64();

  mPrevLSIRulesInstructionMessage[0] = '\0';
  extended.mLSIRulesInstructionMessage[0] = '\0';
  extended.mTicksLSIRulesInstructionMessageUpdated = ::GetTickCount64();
}
