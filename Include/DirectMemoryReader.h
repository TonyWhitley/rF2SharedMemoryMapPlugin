/*
Author: The Iron Wolf (vleonavicius@hotmail.com)
Website: thecrewchief.org
*/

#pragma once

class DirectMemoryReader
{
public:
  DirectMemoryReader() {}

  bool Initialize();
  bool Read(rF2Extended& extended);
  bool ReadOnNewSession(rF2Extended& extended); 
  bool ReadOnLSIVisible(rF2Extended& extended);

  bool IsSCRPluginEnabled() const { return mSCRPluginEnabled; }
  long GetSCRPluginDoubleFileType() const { return mSCRPluginDoubleFileType; }

private:
  void ReadSCRPluginConfig();
  void ReadSCRPluginConfigValues(char* const pluginConfig);
  void OnNewSession(rF2Extended& extended);

private:
  char* mpStatusMessage = nullptr;
  char** mppMessageCenterMessages = nullptr;
  float* mpCurrPitSpeedLimit = nullptr;
  char* mpLSIMessages = nullptr;
  float* mpSCSpeed = nullptr;

  char mPrevStatusMessage[rF2MappedBufferHeader::MAX_STATUS_MSG_LEN];
  char mPrevLastHistoryMessage[rF2MappedBufferHeader::MAX_STATUS_MSG_LEN];
  char mPrevLSIPhaseMessage[rF2MappedBufferHeader::MAX_RULES_INSTRUCTION_MSG_LEN];
  char mPrevLSIPitStateMessage[rF2MappedBufferHeader::MAX_RULES_INSTRUCTION_MSG_LEN];
  char mPrevLSIOrderInstructionMessage[rF2MappedBufferHeader::MAX_RULES_INSTRUCTION_MSG_LEN];
  char mPrevLSIRulesInstructionMessage[rF2MappedBufferHeader::MAX_RULES_INSTRUCTION_MSG_LEN];
  float mPrevSCSpeed = 0.0f;

  bool mSCRPluginEnabled = false;
  long mSCRPluginDoubleFileType = -1L;
};