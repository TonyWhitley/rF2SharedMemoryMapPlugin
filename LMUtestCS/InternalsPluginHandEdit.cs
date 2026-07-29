// Translated from InternalsPlugin.hpp
// Original comments retained and non-translated C++ constructs commented out to ease diffing
using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace lmuSharedMemory.InternalsPlugin
{
    // rF2 and plugins must agree on structure packing, so set it explicitly here.
    // Whatever the current packing is will be restored at the end of this include
    // with another #pragma in the original C++ header.

    public enum IP_VehicleClass : byte
    {
        Hypercar = 0x00,
        LMP2_ELMS = 0x02,
        LMP2,
        LMP3,
        GTE,
        GT3,
        PaceCar = 0x08,
        Unknown = 0xFF
    }

    public enum IP_VehicleChampionship : byte
    {
        WEC_2023 = 0x00, WEC_2024, WEC_2025, WEC_2026,
        ELMS_2025 = 0x10, ELMS_2026,
        Unknown = 0xFF
    }

    //#########################################################################
    //# Version01 Structures                                                   #
    //##########################################################################

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TelemVect3
    {
        [JsonIgnore] public double x;
        [JsonIgnore] public double y;
        [JsonIgnore] public double z;

        public void Set(double a, double b, double c) { x = a; y = b; z = c; }

        // Allowed to reference as [0], [1], or [2], instead of .x, .y, or .z, respectively
        public double this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (i)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TelemQuat
    {
        [JsonIgnore] public double w, x, y, z;

        // Convert this quaternion to a matrix
        public void ConvertQuatToMat(TelemVect3[] ori)
        {
            // expects ori length >= 3
            double x2 = x + x;
            double xx = x * x2;
            double y2 = y + y;
            double yy = y * y2;
            double z2 = z + z;
            double zz = z * z2;
            double xz = x * z2;
            double xy = x * y2;
            double wy = w * y2;
            double wx = w * x2;
            double wz = w * z2;
            double yz = y * z2;
            ori[0].Set(1.0 - (yy + zz), xy - wz, xz + wy);
            ori[1].Set(xy + wz, 1.0 - (xx + zz), yz - wx);
            ori[2].Set(xz - wy, yz + wx, 1.0 - (xx + yy));
        }

        // Convert a matrix to this quaternion
        public void ConvertMatToQuat(TelemVect3[] ori)
        {
            double trace = ori[0].x + ori[1].y + ori[2].z + 1.0;
            if (trace > 0.0625f)
            {
                double sqrtTrace = Math.Sqrt(trace);
                double s = 0.5 / sqrtTrace;
                w = 0.5 * sqrtTrace;
                x = (ori[2].y - ori[1].z) * s;
                y = (ori[0].z - ori[2].x) * s;
                z = (ori[1].x - ori[0].y) * s;
            }
            else if ((ori[0].x > ori[1].y) && (ori[0].x > ori[2].z))
            {
                double sqrtTrace = Math.Sqrt(1.0 + ori[0].x - ori[1].y - ori[2].z);
                double s = 0.5 / sqrtTrace;
                w = (ori[2].y - ori[1].z) * s;
                x = 0.5 * sqrtTrace;
                y = (ori[0].y + ori[1].x) * s;
                z = (ori[0].z + ori[2].x) * s;
            }
            else if (ori[1].y > ori[2].z)
            {
                double sqrtTrace = Math.Sqrt(1.0 + ori[1].y - ori[0].x - ori[2].z);
                double s = 0.5 / sqrtTrace;
                w = (ori[0].z - ori[2].x) * s;
                x = (ori[0].y + ori[1].x) * s;
                y = 0.5 * sqrtTrace;
                z = (ori[1].z + ori[2].y) * s;
            }
            else
            {
                double sqrtTrace = Math.Sqrt(1.0 + ori[2].z - ori[0].x - ori[1].y);
                double s = 0.5 / sqrtTrace;
                w = (ori[1].x - ori[0].y) * s;
                x = (ori[0].z + ori[2].x) * s;
                y = (ori[1].z + ori[2].y) * s;
                z = 0.5 * sqrtTrace;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TelemWheelV01
    {
        [JsonIgnore] public double mSuspensionDeflection;  // meters
        [JsonIgnore] public double mRideHeight;            // meters
        [JsonIgnore] public double mSuspForce;             // pushrod load in Newtons
        [JsonIgnore] public double mBrakeTemp;             // Celsius
        [JsonIgnore] public double mBrakePressure;         // currently 0.0-1.0, depending on driver input and brake balance; will convert to true brake pressure (kPa) in future

        [JsonIgnore] public double mRotation;              // radians/sec
        [JsonIgnore] public double mLateralPatchVel;       // lateral velocity at contact patch
        [JsonIgnore] public double mLongitudinalPatchVel;  // longitudinal velocity at contact patch
        [JsonIgnore] public double mLateralGroundVel;      // lateral velocity at contact patch
        [JsonIgnore] public double mLongitudinalGroundVel; // longitudinal velocity at contact patch
        [JsonIgnore] public double mCamber;                // radians (positive is left for left-side wheels, right for right-side wheels)
        [JsonIgnore] public double mLateralForce;          // Newtons
        [JsonIgnore] public double mLongitudinalForce;     // Newtons
        [JsonIgnore] public double mTireLoad;              // Newtons

        [JsonIgnore] public double mGripFract;             // an approximation of what fraction of the contact patch is sliding
        [JsonIgnore] public double mPressure;              // kPa (tire pressure)
        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 3)]
        [JsonIgnore] public double[] mTemperature;        // Kelvin (subtract 273.15 to get Celsius), left/center/right (not to be confused with inside/center/outside!)
        [JsonIgnore] public double mWear;                  // wear (0.0-1.0, fraction of maximum) ... this is not necessarily proportional with grip loss
        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 16)]
        [JsonIgnore] public byte[] mTerrainName;         // the material prefixes from the TDF file
        [JsonIgnore] public byte mSurfaceType;    // 0=dry, 1=wet, 2=grass, 3=dirt, 4=gravel, 5=rumblestrip, 6=special
        [JsonIgnore] public bool mFlat;                    // whether tire is flat
        [JsonIgnore] public bool mDetached;                // whether wheel is detached
        [JsonIgnore] public byte mStaticUndeflectedRadius; // tire radius in centimeters

        [JsonIgnore] public double mVerticalTireDeflection;// how much is tire deflected from its (speed-sensitive) radius
        [JsonIgnore] public double mWheelYLocation;        // wheel's y location relative to vehicle y location
        [JsonIgnore] public double mToe;                   // current toe angle

        [JsonIgnore] public double mTireCarcassTemperature;       // Kelvin
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] 
        [JsonIgnore] public double[] mTireInnerLayerTemperature;

        [JsonIgnore] public float mOptimalTemp;
        [JsonIgnore] public byte mCompoundIndex;
        [JsonIgnore] public byte mCompoundType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)] 
        [JsonIgnore] public byte[] mExpansion;
        [JsonIgnore] public byte mTranslate;      // 0 = do not attempt to translate, 1 = attempt to translate

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 126)]
        [JsonIgnore] public byte[] mExpansion2; // renamed to avoid duplicate member name
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct VehicleScoringInfoV01
    {
        [JsonIgnore] public int mID;                      // slot ID
        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 32)] 
        [JsonIgnore] public byte[] mDriverName;          // driver name
        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 64)]
        [JsonIgnore] public byte[] mVehicleName;         // vehicle name
        [JsonIgnore] public short mTotalLaps;              // laps completed
        [JsonIgnore] public sbyte mSector;           // 0=sector3, 1=sector1, 2=sector2
        [JsonIgnore] public sbyte mFinishStatus;     // 0=none, 1=finished, 2=dnf, 3=dq
        [JsonIgnore] public double mLapDist;               // current distance around track
        [JsonIgnore] public double mPathLateral;          // lateral position
        [JsonIgnore] public double mTrackEdge;             // track edge

        [JsonIgnore] public double mBestSector1;
        [JsonIgnore] public double mBestSector2;
        [JsonIgnore] public double mBestLapTime;
        [JsonIgnore] public double mLastSector1;
        [JsonIgnore] public double mLastSector2;
        [JsonIgnore] public double mLastLapTime;
        [JsonIgnore] public double mCurSector1;
        [JsonIgnore] public double mCurSector2;

        [JsonIgnore] public short mNumPitstops;
        [JsonIgnore] public short mNumPenalties;
        [JsonIgnore] public bool mIsPlayer;

        [JsonIgnore] public sbyte mControl;          // who's in control
        [JsonIgnore] public bool mInPits;
        [JsonIgnore] public byte mPlace;          // 1-based position
        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 32)]
        [JsonIgnore] public byte[] mVehicleClass;        // vehicle class

        // Dash Indicators
        [JsonIgnore] public double mTimeBehindNext;
        [JsonIgnore] public int mLapsBehindNext;
        [JsonIgnore] public double mTimeBehindLeader;
        [JsonIgnore] public int mLapsBehindLeader;
        [JsonIgnore] public double mLapStartET;

        // Position and derivatives
        [JsonIgnore] public TelemVect3 mPos;               // world position in meters
        [JsonIgnore] public TelemVect3 mLocalVel;          // velocity
        [JsonIgnore] public TelemVect3 mLocalAccel;        // acceleration

        // Orientation and derivatives
        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 3)]
        [JsonIgnore] public TelemVect3[] mOri;            // rows of orientation matrix
        [JsonIgnore] public TelemVect3 mLocalRot;          // rotation
        [JsonIgnore] public TelemVect3 mLocalRotAccel;     // rotational acceleration

        [JsonIgnore] public byte mHeadlights;     // status of headlights
        [JsonIgnore] public byte mPitState;       // 0=none, 1=request, 2=entering, 3=stopped, 4=exiting
        [JsonIgnore] public byte mServerScored;   // whether this vehicle is being scored by server
        [JsonIgnore] public byte mIndividualPhase;// game phases

        [JsonIgnore] public int mQualification;           // 1-based, can be -1 when invalid

        [JsonIgnore] public double mTimeIntoLap;           // estimated time into lap
        [JsonIgnore] public double mEstimatedLapTime;      // estimated laptime

        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 24)] 
        [JsonIgnore] public byte[] mPitGroup;            // pit group
        [JsonIgnore] public byte mFlag;           // primary flag
        [JsonIgnore] public bool mUnderYellow;             // whether this car has taken a full-course caution
        [JsonIgnore] public byte mCountLapFlag;   // 0 = do not count lap or time, 1 = count lap but not time, 2 = count lap and time
        [JsonIgnore] public bool mInGarageStall;           // appears to be within the correct garage stall

        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 16)] 
        [JsonIgnore] public byte[] mUpgradePack;  // Coded upgrades
        [JsonIgnore] public float mPitLapDist;             // location of pit in terms of lap distance

        [JsonIgnore] public float mBestLapSector1;         // sector 1 time from best lap
        [JsonIgnore] public float mBestLapSector2;         // sector 2 time from best lap

        [JsonIgnore] public ulong mSteamID;            // SteamID of the current driver

        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 32)] 
        [JsonIgnore] public byte[] mVehFilename;        // filename of veh file

        [JsonIgnore] public short mAttackMode;

        [JsonIgnore] public byte mFuelFraction; // Percentage of fuel

        [JsonIgnore] public bool mDRSState;

        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 4)] 
        [JsonIgnore] public byte[] mExpansion;    // for future use
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ScoringInfoV01
    {
        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 64)] 
        [JsonIgnore] public byte[] mTrackName;           // current track name
        [JsonIgnore] public int mSession;                 // current session
        [JsonIgnore] public double mCurrentET;             // current time
        [JsonIgnore] public double mEndET;                 // ending time
        [JsonIgnore] public int mMaxLaps;                // maximum laps
        [JsonIgnore] public double mLapDist;               // distance around track
        [JsonIgnore] public IntPtr mResultsStream;          // results stream additions since last update

        [JsonIgnore] public int mNumVehicles;             // current number of vehicles

        [JsonIgnore] public byte mGamePhase;

        [JsonIgnore] public sbyte mYellowFlagState;

        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 3)] 
        [JsonIgnore] public sbyte[] mSectorFlag;
        [JsonIgnore] public byte mStartLight;       // start light frame
        [JsonIgnore] public byte mNumRedLights;
    }

    public enum TrackRulesColumnV01
    {
        TRCOL_LEFT_LANE = 0,
        TRCOL_MIDDLE_LANE,
        TRCOL_RIGHT_LANE,
        TRCOL_MAX_LANES,
        TRCOL_INVALID = TRCOL_MAX_LANES,
        TRCOL_FREECHOICE,
        TRCOL_PENDING,
        TRCOL_MAXIMUM
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TrackRulesParticipantV01
    {
        [JsonIgnore] public int mID;                             // slot ID
        [JsonIgnore] public short mFrozenOrder;                   // 0-based place when caution came out
        [JsonIgnore] public short mPlace;                         // 1-based place
        [JsonIgnore] public float mYellowSeverity;                // rating of contribution to yellow flag
        [JsonIgnore] public double mCurrentRelativeDistance;      // relative distance

        [JsonIgnore] public int mRelativeLaps;                   // current formation/caution laps relative to safety car
        [JsonIgnore] public TrackRulesColumnV01 mColumnAssignment;// column assignment
        [JsonIgnore] public int mPositionAssignment;             // 0-based position within column
        [JsonIgnore] public byte mPitsOpen;              // whether the rules allow this vehicle to enter pits
        [JsonIgnore] public bool mUpToSpeed;                      // flag indicates whether the vehicle can be followed
        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 2)] 
        [JsonIgnore] public bool[] mUnused;
        [JsonIgnore] public double mGoalRelativeDistance;         // calculated goal relative distance
        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 96)] 
        [JsonIgnore] public byte[] mMessage;                  // message for this participant

        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 192)] 
        [JsonIgnore] public byte[] mExpansion; // future expansion
    }

    public enum TrackRulesStageV01
    {
        TRSTAGE_FORMATION_INIT = 0,
        TRSTAGE_FORMATION_UPDATE,
        TRSTAGE_NORMAL,
        TRSTAGE_CAUTION_INIT,
        TRSTAGE_CAUTION_UPDATE,
        TRSTAGE_MAXIMUM
    }

    // Note: TrackRulesActionV01 type referenced in original header is not defined in the snippet; keep as IntPtr
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TrackRulesV01
    {
        [JsonIgnore] public double mCurrentET;                    // current time
        [JsonIgnore] public TrackRulesStageV01 mStage;            // current stage
        [JsonIgnore] public TrackRulesColumnV01 mPoleColumn;      // column assignment where pole position seems to be located
        [JsonIgnore] public int mNumActions;                     // number of recent actions
        [JsonIgnore] public IntPtr mAction;         // array of recent actions (pointer)
        [JsonIgnore] public int mNumParticipants;                // number of participants

        [JsonIgnore] public bool mYellowFlagDetected;             // whether yellow flag was requested
        [JsonIgnore] public byte mYellowFlagLapsWasOverridden;     // whether mYellowFlagLaps is admin request

        [JsonIgnore] public bool mSafetyCarExists;
        [JsonIgnore] public bool mSafetyCarActive;
        [JsonIgnore] public int mSafetyCarLaps;
        [JsonIgnore] public float mSafetyCarThreshold;
        [JsonIgnore] public double mSafetyCarLapDist;
        [JsonIgnore] public float mSafetyCarLapDistAtStart;

        [JsonIgnore] public float mPitLaneStartDist;
        [JsonIgnore] public float mTeleportLapDist;

        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 256)] 
        [JsonIgnore] public byte[] mInputExpansion;

        [JsonIgnore] public sbyte mYellowFlagState;
        [JsonIgnore] public short mYellowFlagLaps;

        [JsonIgnore] public int mSafetyCarInstruction;
        [JsonIgnore] public float mSafetyCarSpeed;
        [JsonIgnore] public float mSafetyCarMinimumSpacing;
        [JsonIgnore] public float mSafetyCarMaximumSpacing;

        [JsonIgnore] public float mMinimumColumnSpacing;
        [JsonIgnore] public float mMaximumColumnSpacing;

        [JsonIgnore] public float mMinimumSpeed;
        [JsonIgnore] public float mMaximumSpeed;

        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 96)] 
        [JsonIgnore] public byte[] mMessage;
        [JsonIgnore] public IntPtr mParticipant;         // array of participants (pointer)

        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 256)] 
        [JsonIgnore] public byte[] mInputOutputExpansion;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PitMenuV01
    {
        [JsonIgnore] public int mCategoryIndex;                  // index of the current category
        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 32)] 
        [JsonIgnore] public byte[] mCategoryName;             // name of the current category

        [JsonIgnore] public int mChoiceIndex;                    // index of the current choice
        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 32)] 
        [JsonIgnore] public byte[] mChoiceString;              // name of the current choice
        [JsonIgnore] public int mNumChoices;                     // total number of choices

        [MarshalAs (UnmanagedType.ByValArray, SizeConst = 256)] 
        [JsonIgnore] public byte[] mExpansion;      // for future use
    }
}
