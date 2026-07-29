// Translated from InternalsPlugin.hpp
// Original comments retained and C++-only constructs commented out to ease diffing
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
        ELMS_2025 = 0X10, ELMS_2026,
        Unknown = 0xFF
    }

    //#########################################################################
    //# Version01 Structures                                                   #
    //##########################################################################

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TelemVect3
    {
        // union
        // {
        //     struct { double x, y, z; };
        //     double data[3];
        // };

        [JsonIgnore] public double x, y, z;

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
            // code translated from C++ implementation
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
            ori[0][0] = 1.0 - (yy + zz);
            ori[0][1] = xy - wz;
            ori[0][2] = xz + wy;
            ori[1][0] = xy + wz;
            ori[1][1] = 1.0 - (xx + zz);
            ori[1][2] = yz - wx;
            ori[2][0] = xz - wy;
            ori[2][1] = yz + wx;
            ori[2][2] = 1.0 - (xx + yy);
        }

        // Convert a matrix to this quaternion
        public void ConvertMatToQuat(TelemVect3[] ori)
        {
            double trace = ori[0][0] + ori[1][1] + ori[2][2] + 1.0;
            if (trace > 0.0625f)
            {
                double sqrtTrace = Math.Sqrt(trace);
                double s = 0.5 / sqrtTrace;
                w = 0.5 * sqrtTrace;
                x = (ori[2][1] - ori[1][2]) * s;
                y = (ori[0][2] - ori[2][0]) * s;
                z = (ori[1][0] - ori[0][1]) * s;
            }
            else if ((ori[0][0] > ori[1][1]) && (ori[0][0] > ori[2][2]))
            {
                double sqrtTrace = Math.Sqrt(1.0 + ori[0][0] - ori[1][1] - ori[2][2]);
                double s = 0.5 / sqrtTrace;
                w = (ori[2][1] - ori[1][2]) * s;
                x = 0.5 * sqrtTrace;
                y = (ori[0][1] + ori[1][0]) * s;
                z = (ori[0][2] + ori[2][0]) * s;
            }
            else if (ori[1][1] > ori[2][2])
            {
                double sqrtTrace = Math.Sqrt(1.0 + ori[1][1] - ori[0][0] - ori[2][2]);
                double s = 0.5 / sqrtTrace;
                w = (ori[0][2] - ori[2][0]) * s;
                x = (ori[0][1] + ori[1][0]) * s;
                y = 0.5 * sqrtTrace;
                z = (ori[1][2] + ori[2][1]) * s;
            }
            else
            {
                double sqrtTrace = Math.Sqrt(1.0 + ori[2][2] - ori[0][0] - ori[1][1]);
                double s = 0.5 / sqrtTrace;
                w = (ori[1][0] - ori[0][1]) * s;
                x = (ori[0][2] + ori[2][0]) * s;
                y = (ori[1][2] + ori[2][1]) * s;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        [JsonIgnore] public double[] mTemperature;        // Kelvin (subtract 273.15 to get Celsius), left/center/right (not to be confused with inside/center/outside!)
        [JsonIgnore] public double mWear;                  // wear (0.0-1.0, fraction of maximum) ... this is not necessarily proportional with grip loss
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        [JsonIgnore] public byte[] mTerrainName;         // the material prefixes from the TDF file
        [JsonIgnore] public byte mSurfaceType;    // 0=dry, 1=wet, 2=grass, 3=dirt, 4=gravel, 5=rumblestrip, 6=special
        [JsonIgnore] public bool mFlat;                    // whether tire is flat
        [JsonIgnore] public bool mDetached;                // whether wheel is detached
        [JsonIgnore] public byte mStaticUndeflectedRadius; // tire radius in centimeters

        [JsonIgnore] public double mVerticalTireDeflection;// how much is tire deflected from its (speed-sensitive) radius
        [JsonIgnore] public double mWheelYLocation;        // wheel's y location relative to vehicle y location
        [JsonIgnore] public double mToe;                   // current toe angle w.r.t. the vehicle

        [JsonIgnore] public double mTireCarcassTemperature;       // rough average of temperature samples from carcass (Kelvin)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        [JsonIgnore] public double[] mTireInnerLayerTemperature; // rough average of temperature samples from innermost layer of rubber (before carcass) (Kelvin)

        [JsonIgnore] public float mOptimalTemp;
        [JsonIgnore] public byte mCompoundIndex;
        [JsonIgnore] public byte mCompoundType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
        [JsonIgnore] public byte[] mExpansion;
    }

    // Our world coordinate system is left-handed, with +y pointing up.
    // The local vehicle coordinate system is as follows:
    //   +x points out the left side of the car (from the driver's perspective)
    //   +y points out the roof
    //   +z points out the back of the car
    // Rotations are as follows:
    //   +x pitches up
    //   +y yaws to the right
    //   +z rolls to the right
    // Note that ISO vehicle coordinates (+x forward, +y right, +z upward) are
    // right-handed.  If you are using that system, be sure to negate any rotation
    // or torque data because things rotate in the opposite direction.  In other
    // words, a -z velocity in rFactor is a +x velocity in ISO, but a -z rotation
    // in rFactor is a -x rotation in ISO!!!

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TelemInfoV01
    {
        // Time
        [JsonIgnore] public int mID;                      // slot ID (note that it can be re-used in multiplayer after someone leaves)
        [JsonIgnore] public double mDeltaTime;             // time since last update (seconds)
        [JsonIgnore] public double mElapsedTime;           // game session time
        [JsonIgnore] public int mLapNumber;               // current lap number
        [JsonIgnore] public double mLapStartET;            // time this lap was started
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        [JsonIgnore] public byte[] mVehicleName;         // current vehicle name
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        [JsonIgnore] public byte[] mTrackName;           // current track name

        // Position and derivatives
        [JsonIgnore] public TelemVect3 mPos;               // world position in meters
        [JsonIgnore] public TelemVect3 mLocalVel;          // velocity (meters/sec) in local vehicle coordinates
        [JsonIgnore] public TelemVect3 mLocalAccel;        // acceleration (meters/sec^2) in local vehicle coordinates

        // Orientation and derivatives
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        [JsonIgnore] public TelemVect3[] mOri;            // rows of orientation matrix (use TelemQuat conversions if desired), also converts local
                                                                                                     // vehicle vectors into world X, Y, or Z using dot product of rows 0, 1, or 2 respectively
        [JsonIgnore] public TelemVect3 mLocalRot;          // rotation (radians/sec) in local vehicle coordinates
        [JsonIgnore] public TelemVect3 mLocalRotAccel;     // rotational acceleration (radians/sec^2) in local vehicle coordinates

        // Vehicle status
        [JsonIgnore] public int mGear;                    // -1=reverse, 0=neutral, 1+=forward gears
        [JsonIgnore] public double mEngineRPM;             // engine RPM
        [JsonIgnore] public double mEngineWaterTemp;       // Celsius
        [JsonIgnore] public double mEngineOilTemp;         // Celsius
        [JsonIgnore] public double mClutchRPM;             // clutch RPM

        // Driver input
        [JsonIgnore] public double mUnfilteredThrottle;    // ranges  0.0-1.0
        [JsonIgnore] public double mUnfilteredBrake;       // ranges  0.0-1.0
        [JsonIgnore] public double mUnfilteredSteering;    // ranges -1.0-1.0 (left to right)
        [JsonIgnore] public double mUnfilteredClutch;      // ranges  0.0-1.0

        // Filtered input (various adjustments for rev or speed limiting, TC, ABS?, speed sensitive steering, clutch work for semi-automatic shifting, etc.)
        [JsonIgnore] public double mFilteredThrottle;      // ranges  0.0-1.0
        [JsonIgnore] public double mFilteredBrake;         // ranges  0.0-1.0
        [JsonIgnore] public double mFilteredSteering;      // ranges -1.0-1.0 (left to right)
        [JsonIgnore] public double mFilteredClutch;        // ranges  0.0-1.0

        // Misc
        [JsonIgnore] public double mSteeringShaftTorque;   // torque around steering shaft (used to be mSteeringArmForce, but that is not necessarily accurate for feedback purposes)
        [JsonIgnore] public double mFront3rdDeflection;    // deflection at front 3rd spring
        [JsonIgnore] public double mRear3rdDeflection;     // deflection at rear 3rd spring

        // Aerodynamics
        [JsonIgnore] public double mFrontWingHeight;       // front wing height
        [JsonIgnore] public double mFrontRideHeight;       // front ride height
        [JsonIgnore] public double mRearRideHeight;        // rear ride height
        [JsonIgnore] public double mDrag;                  // drag
        [JsonIgnore] public double mFrontDownforce;        // front downforce
        [JsonIgnore] public double mRearDownforce;         // rear downforce

        // State/damage info
        [JsonIgnore] public double mFuel;                  // amount of fuel (liters)
        [JsonIgnore] public double mEngineMaxRPM;          // rev limit
        [JsonIgnore] public byte mScheduledStops; // number of scheduled pitstops
        [JsonIgnore] public bool mOverheating;            // whether overheating icon is shown
        [JsonIgnore] public bool mDetached;               // whether any parts (besides wheels) have been detached
        [JsonIgnore] public bool mHeadlights;             // whether headlights are on
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        [JsonIgnore] public byte[] mDentSeverity; // dent severity at 8 locations around the car (0=none, 1=some, 2=more)
        [JsonIgnore] public double mLastImpactET;          // time of last impact
        [JsonIgnore] public double mLastImpactMagnitude;   // magnitude of last impact
        [JsonIgnore] public TelemVect3 mLastImpactPos;     // location of last impact

        // Expanded
        [JsonIgnore] public double mEngineTorque;          // current engine torque (including additive torque) (used to be mEngineTq, but there's little reason to abbreviate it)
        [JsonIgnore] public int mCurrentSector;           // the current sector (zero-based) with the pitlane stored in the sign bit (example: entering pits from third sector gives 0x80000002)
        [JsonIgnore] public byte mSpeedLimiter;   // whether speed limiter is on
        [JsonIgnore] public byte mMaxGears;       // maximum forward gears
        [JsonIgnore] public byte mFrontTireCompoundIndex;   // index within brand
        [JsonIgnore] public byte mRearTireCompoundIndex;    // index within brand
        [JsonIgnore] public double mFuelCapacity;          // capacity in liters
        [JsonIgnore] public byte mFrontFlapActivated;       // whether front flap is activated
        [JsonIgnore] public byte mRearFlapActivated;        // whether rear flap is activated
        [JsonIgnore] public byte mRearFlapLegalStatus;      // 0=disallowed, 1=criteria detected but not allowed quite yet, 2=allowed
        [JsonIgnore] public byte mIgnitionStarter;          // 0=off 1=ignition 2=ignition+starter

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
        [JsonIgnore] public byte[] mFrontTireCompoundName;         // name of front tire compound
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
        [JsonIgnore] public byte[] mRearTireCompoundName;          // name of rear tire compound

        [JsonIgnore] public byte mSpeedLimiterAvailable;    // whether speed limiter is available
        [JsonIgnore] public byte mAntiStallActivated;       // whether (hard) anti-stall is activated
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        [JsonIgnore] public byte[] mUnused;                //
        [JsonIgnore] public float mVisualSteeringWheelRange;         // the *visual* steering wheel range

        [JsonIgnore] public double mRearBrakeBias;                   // fraction of brakes on rear
        [JsonIgnore] public double mTurboBoostPressure;              // current turbo boost pressure if available
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        [JsonIgnore] public float[] mPhysicsToGraphicsOffset;       // offset from static CG to graphical center
        [JsonIgnore] public float mPhysicalSteeringWheelRange;       // the *physical* steering wheel range

        // deltabest
        [JsonIgnore] public double mDeltaBest;

        [JsonIgnore] public double mBatteryChargeFraction; // Battery charge as fraction [0.0-1.0]

        // electric boost motor
        [JsonIgnore] public double mElectricBoostMotorTorque; // current torque of boost motor (can be negative when in regenerating mode)
        [JsonIgnore] public double mElectricBoostMotorRPM; // current rpm of boost motor
        [JsonIgnore] public double mElectricBoostMotorTemperature; // current temperature of boost motor
    [JsonIgnore] public double mElectricBoostWaterTemperature; // current water temperature of boost motor cooler if present (0 otherwise)
    [JsonIgnore] public byte mElectricBoostMotorState; // 0=unavailable 1=inactive, 2=propulsion, 3=regeneration
  [JsonIgnore] public bool mLapInvalidated;
  [JsonIgnore] public bool mABSActive;
  [JsonIgnore] public bool mTCActive;
  [JsonIgnore] public bool mSpeedLimiterActive;
  [JsonIgnore] public byte mWiperState;
    [JsonIgnore] public byte mTC;
    [JsonIgnore] public byte mTCMax;
  [JsonIgnore] public byte mTCSlip;
  [JsonIgnore] public byte mTCSlipMax;
  [JsonIgnore] public byte mTCCut;
  [JsonIgnore] public byte mTCCutMax;
  [JsonIgnore] public byte mABS;
  [JsonIgnore] public byte mABSMax;
  [JsonIgnore] public byte mMotorMap;
  [JsonIgnore] public byte mMotorMapMax;
  [JsonIgnore] public byte mMigration;
  [JsonIgnore] public byte mMigrationMax;
  [JsonIgnore] public byte mFrontAntiSway;
  [JsonIgnore] public byte mFrontAntiSwayMax;
  [JsonIgnore] public byte mRearAntiSway;
  [JsonIgnore] public byte mRearAntiSwayMax;
  [JsonIgnore] public byte mLiftAndCoastProgress;
  [JsonIgnore] public byte mTrackLimitsSteps; // Normalized track limits points (TrackLimitPoints * TrackLimitStepsPerPoint)
  [JsonIgnore] public float mRegen; //kW
  [JsonIgnore] public float mSoC;
  [JsonIgnore] public float mVirtualEnergy;
  [JsonIgnore] public float mTimeGapCarAhead;
  [JsonIgnore] public float mTimeGapCarBehind;
  [JsonIgnore] public float mTimeGapPlaceAhead;
  [JsonIgnore] public float mTimeGapPlaceBehind;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    [JsonIgnore] public byte[] mVehicleModel;
  [JsonIgnore] public IP_VehicleClass mVehicleClass;
  [JsonIgnore] public IP_VehicleChampionship mVehicleChampionship;

    // Future use
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    [JsonIgnore] public byte[] mExpansion; // for future use (note that the slot ID has been moved to mID above)

    // keeping this at the end of the structure to make it easier to replace in future versions
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    [JsonIgnore] TelemWheelV01[] mWheel;       // wheel info (front left, front right, rear left, rear right)
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct GraphicsInfoV01
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        [JsonIgnore] public byte[] mExpansion;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct GraphicsInfoV02 // : GraphicsInfoV01
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        [JsonIgnore] public byte[] mExpansion;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CameraControlInfoV01
    {
        [JsonIgnore] public IntPtr mHWND; // HWND mHWND; // window handle (platform-specific)
        [JsonIgnore] public byte mType; // camera type
        [JsonIgnore] public byte mFreeCamera; // free camera
        [JsonIgnore] public byte mIndex; // camera index
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 249)]
        [JsonIgnore] public byte[] mExpansion; // padding to 256
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MessageInfoV01
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        [JsonIgnore] public byte[] mMessage; // message buffer
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct VehicleScoringInfoV01
    {
        [JsonIgnore] public int mID;                      // slot ID (note that it can be re-used in multiplayer after someone leaves)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [JsonIgnore] public byte[] mDriverName;          // driver name
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        [JsonIgnore] public byte[] mVehicleName;         // vehicle name
        [JsonIgnore] public short mTotalLaps;              // laps completed
        [JsonIgnore] public sbyte mSector;           // 0=sector3, 1=sector1, 2=sector2 (don't ask why)
        [JsonIgnore] public sbyte mFinishStatus;     // 0=none, 1=finished, 2=dnf, 3=dq
        [JsonIgnore] public double mLapDist;               // current distance around track
        [JsonIgnore] public double mPathLateral;          // lateral position with respect to *very approximate* "center" path
        [JsonIgnore] public double mTrackEdge;             // track edge (w.r.t. "center" path) on same side of track as vehicle

        [JsonIgnore] public double mBestSector1;           // best sector 1
        [JsonIgnore] public double mBestSector2;           // best sector 2 (plus sector 1)
        [JsonIgnore] public double mBestLapTime;           // best lap time
        [JsonIgnore] public double mLastSector1;           // last sector 1
        [JsonIgnore] public double mLastSector2;           // last sector 2 (plus sector 1)
        [JsonIgnore] public double mLastLapTime;           // last lap time
        [JsonIgnore] public double mCurSector1;            // current sector 1 if valid
        [JsonIgnore] public double mCurSector2;            // current sector 2 (plus sector 1) if valid
        // no current laptime because it instantly becomes "last"

        [JsonIgnore] public short mNumPitstops;            // number of pitstops made
        [JsonIgnore] public short mNumPenalties;           // number of outstanding penalties
        [JsonIgnore] public bool mIsPlayer;                // is this the player's vehicle

        [JsonIgnore] public sbyte mControl;          // who's in control: -1=nobody (shouldn't get this), 0=local player, 1=local AI, 2=remote, 3=replay (shouldn't get this)
        [JsonIgnore] public bool mInPits;                  // between pit entrance and pit exit (not always accurate for remote vehicles)
        [JsonIgnore] public byte mPlace;          // 1-based position
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [JsonIgnore] public byte[] mVehicleClass;        // vehicle class

        // Dash Indicators
        [JsonIgnore] public double mTimeBehindNext;        // time behind vehicle in next higher place
        [JsonIgnore] public int mLapsBehindNext;          // laps behind vehicle in next higher place
        [JsonIgnore] public double mTimeBehindLeader;      // time behind leader
        [JsonIgnore] public int mLapsBehindLeader;        // laps behind leader
        [JsonIgnore] public double mLapStartET;            // time this lap was started

        // Position and derivatives
        [JsonIgnore] public TelemVect3 mPos;               // world position in meters
        [JsonIgnore] public TelemVect3 mLocalVel;          // velocity (meters/sec) in local vehicle coordinates
        [JsonIgnore] public TelemVect3 mLocalAccel;        // acceleration (meters/sec^2) in local vehicle coordinates

        // Orientation and derivatives
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        [JsonIgnore] public TelemVect3[] mOri;            // rows of orientation matrix (use TelemQuat conversions if desired), also converts local
                                                                                                     // vehicle vectors into world X, Y, or Z using dot product of rows 0, 1, or 2 respectively
        [JsonIgnore] public TelemVect3 mLocalRot;          // rotation (radians/sec) in local vehicle coordinates
        [JsonIgnore] public TelemVect3 mLocalRotAccel;     // rotational acceleration (radians/sec^2) in local vehicle coordinates

        // tag.2012.03.01 - stopped casting some of these so variables now have names and mExpansion has shrunk, overall size and old data locations should be same
        [JsonIgnore] public byte mHeadlights;     // status of headlights
        [JsonIgnore] public byte mPitState;       // 0=none, 1=request, 2=entering, 3=stopped, 4=exiting
        [JsonIgnore] public byte mServerScored;   // whether this vehicle is being scored by server (could be off in qualifying or racing heats)
        [JsonIgnore] public byte mIndividualPhase;// game phases (described below) plus 9=after formation, 10=under yellow, 11=under blue (not used)

        [JsonIgnore] public int mQualification;           // 1-based, can be -1 when invalid

        [JsonIgnore] public double mTimeIntoLap;           // estimated time into lap
        [JsonIgnore] public double mEstimatedLapTime;      // estimated laptime used for 'time behind' and 'time into lap' (note: this may changed based on vehicle and setup!?)

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        [JsonIgnore] public byte[] mPitGroup;            // pit group (same as team name unless pit is shared)
        [JsonIgnore] public byte mFlag;           // primary flag being shown to vehicle (currently only 0=green or 6=blue)
        [JsonIgnore] public bool mUnderYellow;             // whether this car has taken a full-course caution flag at the start/finish line
        [JsonIgnore] public byte mCountLapFlag;   // 0 = do not count lap or time, 1 = count lap but not time, 2 = count lap and time
        [JsonIgnore] public bool mInGarageStall;           // appears to be within the correct garage stall

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        [JsonIgnore] public byte[] mUpgradePack;  // Coded upgrades
        [JsonIgnore] public float mPitLapDist;             // location of pit in terms of lap distance

        [JsonIgnore] public float mBestLapSector1;         // sector 1 time from best lap (not necessarily the best sector 1 time)
        [JsonIgnore] public float mBestLapSector2;         // sector 2 time from best lap (not necessarily the best sector 2 time)

        [JsonIgnore] public ulong mSteamID;            // SteamID of the current driver (if any)

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [JsonIgnore] public byte[] mVehFilename;        // filename of veh file used to identify this vehicle.

        [JsonIgnore] public short mAttackMode;

        // 2020.11.12 - Took 1 byte from mExpansion to transmit fuel percentage
        [JsonIgnore] public byte mFuelFraction; // Percentage of fuel or battery left in vehicle. 0x00 = 0%; 0xFF = 100%

        // 2021.05.28 - Took 1 byte from mExpansion to transmit DRS (RearFlap) state - consider making this a bitfield if further bools are needed later on
        [JsonIgnore] public bool mDRSState;

        // Future use
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        [JsonIgnore] public byte[] mExpansion;    // for future use
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ScoringInfoV01
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        [JsonIgnore] public byte[] mTrackName;           // current track name
        [JsonIgnore] public int mSession;                 // current session (0=testday 1-4=practice 5-8=qual 9=warmup 10-13=race)
        [JsonIgnore] public double mCurrentET;             // current time
        [JsonIgnore] public double mEndET;                 // ending time
        [JsonIgnore] public int  mMaxLaps;                // maximum laps
        [JsonIgnore] public double mLapDist;               // distance around track
        [JsonIgnore] public IntPtr mResultsStream;          // results stream additions since last update (newline-delimited and NULL-terminated)

        [JsonIgnore] public int mNumVehicles;             // current number of vehicles

        // Game phases:
        // 0 Before session has begun
        // 1 Reconnaissance laps (race only)
        // 2 Grid walk-through (race only)
        // 3 Formation lap (race only)
        // 4 Starting-light countdown has begun (race only)
        // 5 Green flag
        // 6 Full course yellow / safety car
        // 7 Session stopped
        // 8 Session over
        // 9 Paused (tag.2015.09.14 - this is new, and indicates that this is a heartbeat call to the plugin)
        [JsonIgnore] public byte mGamePhase;

        // Yellow flag states (applies to full-course only)
        // -1 Invalid
        //  0 None
        //  1 Pending
        //  2 Pits closed
        //  3 Pit lead lap
        //  4 Pits open
        //  5 Last lap
        //  6 Resume
        //  7 Race halt (not currently used)
        [JsonIgnore] public sbyte mYellowFlagState;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        [JsonIgnore] public sbyte[] mSectorFlag;      // whether there are any local yellows at the moment in each sector (not sure if sector 0 is first or last, so test)
        [JsonIgnore] public byte mStartLight;       // start light frame (number depends on track)
        [JsonIgnore] public byte mNumRedLights; 
        [JsonIgnore] public bool mInRealtime;                // in realtime as opposed to at the monitor
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [JsonIgnore] public byte[] mPlayerName;            // player name (including possible multiplayer override)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        [JsonIgnore] public byte[] mPlrFileName;           // may be encoded to be a legal filename

    // weather
    [JsonIgnore] public double mDarkCloud;               // cloud darkness? 0.0-1.0
    [JsonIgnore] public double mRaining;                 // raining severity 0.0-1.0
    [JsonIgnore] public double mAmbientTemp;             // temperature (Celsius)
    [JsonIgnore] public double mTrackTemp;               // temperature (Celsius)
    [JsonIgnore] public TelemVect3 mWind;                // wind speed
    [JsonIgnore] public double mMinPathWetness;          // minimum wetness on main path 0.0-1.0
    [JsonIgnore] public double mMaxPathWetness;          // maximum wetness on main path 0.0-1.0

    // multiplayer
    [JsonIgnore] public byte mGameMode; // 1 = server, 2 = client, 3 = server and client
    [JsonIgnore] public bool mIsPasswordProtected; // is the server password protected
    [JsonIgnore] public short mServerPort; // the port of the server (if on a server)
    [JsonIgnore] public int mServerPublicIP; // the public IP address of the server (if on a server)
    [JsonIgnore] public int mMaxPlayers; // maximum number of vehicles that can be in the session
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    [JsonIgnore] public byte[] mServerName; // name of the server
    [JsonIgnore] public float mStartET; // start time (seconds since midnight) of the event

  //
  double mAvgPathWetness;          // average wetness on main path 0.0-1.0
  float mSessionTimeRemaining;
  float mTimeOfDay;
  bool mIsFixedSetup;
  byte mTrackGripLevel;
  byte mCloudCoverage;
  byte mTrackLimitsStepsPerPenalty;
  byte mTrackLimitsStepsPerPoint;
    // Future use
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 197)]
    [JsonIgnore] public byte[] mExpansion;

  // keeping this at the end of the structure to make it easier to replace in future versions
  // tbd VehicleScoringInfoV01 *mVehicle; // array of vehicle scoring info's
    }


struct CommentaryRequestInfoV01
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    [JsonIgnore] public char[] mName;                  // one of the event names in the commentary INI file
  double mInput1;                  // first value to pass in (if any)
  double mInput2;                  // first value to pass in (if any)
  double mInput3;                  // first value to pass in (if any)
  bool mSkipChecks;                // ignores commentary detail and random probability of event

  // constructor (for noobs, this just helps make sure everything is initialized to something reasonable)
  // tbd CommentaryRequestInfoV01()       { mName[0] = 0; mInput1 = 0.0; mInput2 = 0.0; mInput3 = 0.0; mSkipChecks = false; }
};


//#########################################################################
//# Version02 Structures                                                   #
//##########################################################################

struct PhysicsOptionsV01
{
  [JsonIgnore] byte mTractionControl;  // 0 (off) - 3 (high)
  [JsonIgnore] byte mAntiLockBrakes;   // 0 (off) - 2 (high)
  [JsonIgnore] byte mStabilityControl; // 0 (off) - 2 (high)
  [JsonIgnore] byte mAutoShift;        // 0 (off), 1 (upshifts), 2 (downshifts), 3 (all)
  [JsonIgnore] byte mAutoClutch;       // 0 (off), 1 (on)
  [JsonIgnore] byte mInvulnerable;     // 0 (off), 1 (on)
  [JsonIgnore] byte mOppositeLock;     // 0 (off), 1 (on)
  [JsonIgnore] byte mSteeringHelp;     // 0 (off) - 3 (high)
  [JsonIgnore] byte mBrakingHelp;      // 0 (off) - 2 (high)
  [JsonIgnore] byte mSpinRecovery;     // 0 (off), 1 (on)
  [JsonIgnore] byte mAutoPit;          // 0 (off), 1 (on)
  [JsonIgnore] byte mAutoLift;         // 0 (off), 1 (on)
  [JsonIgnore] byte mAutoBlip;         // 0 (off), 1 (on)

  [JsonIgnore] byte mFuelMult;         // fuel multiplier (0x-7x)
  [JsonIgnore] byte mTireMult;         // tire wear multiplier (0x-7x)
  [JsonIgnore] byte mMechFail;         // mechanical failure setting; 0 (off), 1 (normal), 2 (timescaled)
  [JsonIgnore] byte mAllowPitcrewPush; // 0 (off), 1 (on)
  [JsonIgnore] byte mRepeatShifts;     // accidental repeat shift prevention (0-5; see PLR file)
  [JsonIgnore] byte mHoldClutch;       // for auto-shifters at start of race: 0 (off), 1 (on)
  [JsonIgnore] byte mAutoReverse;      // 0 (off), 1 (on)
  [JsonIgnore] byte mAlternateNeutral; // Whether shifting up and down simultaneously equals neutral

  // tag.2014.06.09 - yes these are new, but no they don't change the size of the structure nor the address of the other variables in it (because we're just using the existing padding)
  [JsonIgnore] byte mAIControl;        // Whether player vehicle is currently under AI control
  [JsonIgnore] byte mUnused1;          //
  [JsonIgnore] byte mUnused2;          //

  float mManualShiftOverrideTime;  // time before auto-shifting can resume after recent manual shift
  float mAutoShiftOverrideTime;    // time before manual shifting can resume after recent auto shift
  float mSpeedSensitiveSteering;   // 0.0 (off) - 1.0
};


struct EnvironmentInfoV01
{
    // TEMPORARY buffers (you should copy them if needed for later use) containing various paths that may be needed.  Each of these
    // could be relative ("UserData\") or full ("C:\BlahBlah\rFactorProduct\UserData\").
    // mPath[ 0 ] points to the UserData directory.
    // mPath[ 1 ] points to the CustomPluginOptions.JSON filename.
    // mPath[ 2 ] points to the latest results file
    // (in the future, we may add paths for the current garage setup, fully upgraded physics files, etc., any other requests?)
    // tbd const char *mPath[ 16 ];
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    [JsonIgnore] byte[] mExpansion;   // future use
};


// deprecated (callbacks are no longer invoked in DX11) since V8
//struct ScreenInfoV01
//{
//  HWND mAppWindow;                      // Application window handle
//  void *mDevice;                        // Cast type to LPDIRECT3DDEVICE9
//  void *mRenderTarget;                  // Cast type to LPDIRECT3DTEXTURE9
//  long mDriver;                         // Current video driver index

//  long mWidth;                          // Screen width
//  long mHeight;                         // Screen height
//  long mPixelFormat;                    // Pixel format
//  long mRefreshRate;                    // Refresh rate
//  long mWindowed;                       // Really just a boolean whether we are in windowed mode

//  long mOptionsWidth;                   // Width dimension of screen portion used by UI
//  long mOptionsHeight;                  // Height dimension of screen portion used by UI
//  long mOptionsLeft;                    // Horizontal starting coordinate of screen portion used by UI
//  long mOptionsUpper;                   // Vertical starting coordinate of screen portion used by UI

//  [JsonIgnore] byte mOptionsLocation;       // 0=main UI, 1=track loading, 2=monitor, 3=on track
//  char mOptionsPage[ 31 ];              // the name of the options page

//  [JsonIgnore] byte mExpansion[ 224 ];      // future use
//};


// replaces the ScreenInfoV01 structure that was deprecated since V8
//struct ApplicationStateV01 {
//  HWND mAppWindow;                      // application window handle
//  unsigned long mWidth;                 // screen width
//  unsigned long mHeight;                // screen height
//  unsigned long mRefreshRate;           // refresh rate
//  unsigned long mWindowed;              // really just a boolean whether we are in windowed mode
//  [JsonIgnore] byte mOptionsLocation;       // 0=main UI, 1=track loading, 2=monitor, 3=on track
//  char mOptionsPage[ 31 ];              // the name of the options page
//  [JsonIgnore] byte mExpansion[ 204 ];      // future use
//};


//struct CustomControlInfoV01
//{
//  // The name passed through CheckHWControl() will be the mUntranslatedName prepended with an underscore (e.g. "Track Map Toggle" -> "_Track Map Toggle")
//  char mUntranslatedName[ 64 ];         // name of the control that will show up in UI (but translated if available)
//  long mRepeat;                         // 0=registers once per hit, 1=registers once, waits briefly, then starts repeating quickly, 2=registers as long as key is down
//  [JsonIgnore] byte mExpansion[ 64 ];       // future use
//};


//struct WeatherControlInfoV01
//{
//  // The current conditions are passed in with the API call. The following ET (Elapsed Time) value should typically be far
//  // enough in the future that it can be interpolated smoothly, and allow clouds time to roll in before rain starts. In
//  // other words you probably shouldn't have mCloudiness and mRaining suddenly change from 0.0 to 1.0 and expect that
//  // to happen in a few seconds without looking crazy.
//  double mET;                           // when you want this weather to take effect

//  // mRaining[1][1] is at the origin (2013.12.19 - and currently the only implemented node), while the others
//  // are spaced at <trackNodeSize> meters where <trackNodeSize> is the maximum absolute value of a track vertex
//  // coordinate (and is passed into the API call).
//  double mRaining[ 3 ][ 3 ];            // rain (0.0-1.0) at different nodes

//  double mCloudiness;                   // general cloudiness (0.0=clear to 1.0=dark), will be automatically overridden to help ensure clouds exist over rainy areas
//  double mAmbientTempK;                 // ambient temperature (Kelvin)
//  double mWindMaxSpeed;                 // maximum speed of wind (ground speed, but it affects how fast the clouds move, too)

//  bool mApplyCloudinessInstantly;       // preferably we roll the new clouds in, but you can instantly change them now
//  bool mUnused1;                        //
//  bool mUnused2;                        //
//  bool mUnused3;                        //

//  [JsonIgnore] byte mExpansion[ 508 ];      // future use (humidity, pressure, air density, etc.)
//};


//#########################################################################
//# Version07 Structures                                                   #
//##########################################################################

//struct CustomVariableV01
//{
//  char mCaption[ 128 ];                 // Name of variable. This will be used for storage. In the future, this may also be used in the UI (after attempting to translate).
//  long mNumSettings;                    // Number of available settings. The special value 0 should be used for types that have limitless possibilities, which will be treated as a string type.
//  long mCurrentSetting;                 // Current setting (also the default setting when returned in GetCustomVariable()). This is zero-based, so: ( 0 <= mCurrentSetting < mNumSettings )

//  // future expansion
//  [JsonIgnore] byte mExpansion[ 256 ];
//};

//struct CustomSettingV01
//{
//  char mName[ 128 ];                    // Enumerated name of setting (only used if CustomVariableV01::mNumSettings > 0). This will be stored in the JSON file for informational purposes only. It may also possibly be used in the UI in the future.
//};

//struct MultiSessionParticipantV01
//{
//  // input only
//  long mID;                             // slot ID (if loaded) or -1 (if currently disconnected)
//  char mDriverName[ 32 ];               // driver name
//  char mVehicleName[ 64 ];              // vehicle name
//  [JsonIgnore] byte mUpgradePack[ 16 ];     // coded upgrades

//  float mBestPracticeTime;              // best practice time
//  long mQualParticipantIndex;           // once qualifying begins, this becomes valid and ranks participants according to practice time if possible
//  float mQualificationTime[ 4 ];        // best qualification time in up to 4 qual sessions
//  float mFinalRacePlace[ 4 ];           // final race place in up to 4 race sessions
//  float mFinalRaceTime[ 4 ];            // final race time in up to 4 race sessions

//  // input/output
//  bool mServerScored;                   // whether vehicle is allowed to participate in current session
//  long mGridPosition;                   // 1-based grid position for current race session (or upcoming race session if it is currently warmup), or -1 if currently disconnected
//// long mPitIndex;
//// long mGarageIndex;

//  // future expansion
//  [JsonIgnore] byte mExpansion[ 128 ];
//};

//struct MultiSessionRulesV01
//{
//  // input only
//  long mSession;                        // current session (0=testday 1-4=practice 5-8=qual 9=warmup 10-13=race)
//  long mSpecialSlotID;                  // slot ID of someone who just joined, or -2 requesting to update qual order, or -1 (default/general)
//  char mTrackType[ 32 ];                // track type from GDB
//  long mNumParticipants;                // number of participants (vehicles)

//  // input/output
//  MultiSessionParticipantV01 *mParticipant;       // array of partipants (vehicles)
//  long mNumQualSessions;                // number of qualifying sessions configured
//  long mNumRaceSessions;                // number of race sessions configured
//  long mMaxLaps;                        // maximum laps allowed in current session (LONG_MAX = unlimited) (note: cannot currently edit in *race* sessions)
//  long mMaxSeconds;                     // maximum time allowed in current session (LONG_MAX = unlimited) (note: cannot currently edit in *race* sessions)
//  char mName[ 32 ];                     // untranslated name override for session (please use mixed case here, it should get uppercased if necessary)

//  // future expansion
//  [JsonIgnore] byte mExpansion[ 256 ];
//};


enum TrackRulesCommandV01               //
{
  TRCMD_ADD_FROM_TRACK = 0,             // crossed s/f line for first time after full-course yellow was called
  TRCMD_ADD_FROM_PIT,                   // exited pit during full-course yellow
  TRCMD_ADD_FROM_UNDQ,                  // during a full-course yellow, the admin reversed a disqualification
  TRCMD_REMOVE_TO_PIT,                  // entered pit during full-course yellow
  TRCMD_REMOVE_TO_DNF,                  // vehicle DNF'd during full-course yellow
  TRCMD_REMOVE_TO_DQ,                   // vehicle DQ'd during full-course yellow
  TRCMD_REMOVE_TO_UNLOADED,             // vehicle unloaded (possibly kicked out or banned) during full-course yellow
  TRCMD_MOVE_TO_BACK,                   // misbehavior during full-course yellow, resulting in the penalty of being moved to the back of their current line
  TRCMD_LONGEST_LINE,                   // misbehavior during full-course yellow, resulting in the penalty of being moved to the back of the longest line
  //------------------
  TRCMD_MAXIMUM                         // should be last
};

struct TrackRulesActionV01
{
  // input only
  TrackRulesCommandV01 mCommand;        // recommended action
  long mID;                             // slot ID if applicable
  [JsonIgnore] byte mLine;                  // line this command applies to (if applicable)
};
    // TrackRules enums and structs
    public enum TrackRulesColumnV01
    {
        TRCOL_LEFT_LANE = 0,                  // left (inside)
        TRCOL_MIDDLE_LANE,                    // middle
        TRCOL_RIGHT_LANE,                     // right (outside)
        //------------------
        TRCOL_MAX_LANES,                      // should be after the valid static lane choices
        //------------------
        TRCOL_INVALID = TRCOL_MAX_LANES,      // currently invalid (hasn't crossed line or in pits/garage)
        TRCOL_FREECHOICE,                     // free choice (dynamically chosen by driver)
        TRCOL_PENDING,                        // depends on another participant's free choice (dynamically set after another driver chooses)
        //------------------
        TRCOL_MAXIMUM                         // should be last
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TrackRulesParticipantV01
    {
        // input only
        [JsonIgnore] public int mID;                             // slot ID
        [JsonIgnore] public short mFrozenOrder;                   // 0-based place when caution came out (not valid for formation laps)
        [JsonIgnore] public short mPlace;                         // 1-based place (typically used for the initialization of the formation lap track order)
        [JsonIgnore] public float mYellowSeverity;                // a rating of how much this vehicle is contributing to a yellow flag (the sum of all vehicles is compared to TrackRulesV01::mSafetyCarThreshold)
        [JsonIgnore] public double mCurrentRelativeDistance;      // equal to ( ( ScoringInfoV01::mLapDist * this->mRelativeLaps ) + VehicleScoringInfoV01::mLapDist )

        // input/output
        [JsonIgnore] public int mRelativeLaps;                   // current formation/caution laps relative to safety car (should generally be zero except when safety car crosses s/f line); this can be decremented to implement 'wave around' or 'beneficiary rule' (a.k.a. 'lucky dog' or 'free pass')
        [JsonIgnore] public TrackRulesColumnV01 mColumnAssignment;// which column (line/lane) that participant is supposed to be in
        [JsonIgnore] public int mPositionAssignment;             // 0-based position within column (line/lane) that participant is supposed to be located at (-1 is invalid)
        [JsonIgnore] public byte mPitsOpen;              // whether the rules allow this particular vehicle to enter pits right now (input is 2=false or 3=true; if you want to edit it, set to 0=false or 1=true)
        [JsonIgnore] public bool mUpToSpeed;                      // while in the frozen order, this flag indicates whether the vehicle can be followed (this should be false for somebody who has temporarily spun and hasn't gotten back up to speed yet)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        [JsonIgnore] public byte[] mUnused;
        [JsonIgnore] public double mGoalRelativeDistance;         // calculated based on where the leader is, and adjusted by the desired column spacing and the column/position assignments
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
        [JsonIgnore] public byte[] mMessage;                  // a message for this participant to explain what is going on (untranslated; it will get run through translator on client machines)

        // future expansion
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 192)]
        [JsonIgnore] public byte[] mExpansion; // for future use (preserve size)
    }

    public enum TrackRulesStageV01
    {
      TRSTAGE_FORMATION_INIT = 0,           // initialization of the formation lap
      TRSTAGE_FORMATION_UPDATE,             // update of the formation lap
      TRSTAGE_NORMAL,                       // normal (non-yellow) update
      TRSTAGE_CAUTION_INIT,                 // initialization of a full-course yellow
      TRSTAGE_CAUTION_UPDATE,               // update of a full-course yellow
      //------------------
      TRSTAGE_MAXIMUM                       // should be last
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TrackRulesV01
    {
      // input only
      [JsonIgnore] public double mCurrentET;                    // current time
      [JsonIgnore] public TrackRulesStageV01 mStage;            // current stage
      [JsonIgnore] public TrackRulesColumnV01 mPoleColumn;      // column assignment where pole position seems to be located
      [JsonIgnore] public int mNumActions;                     // number of recent actions
      [JsonIgnore] public IntPtr mAction;         // array of recent actions
      [JsonIgnore] public int mNumParticipants;                // number of participants (vehicles)

      [JsonIgnore] public bool mYellowFlagDetected;             // whether yellow flag was requested or sum of participant mYellowSeverity's exceeds mSafetyCarThreshold
      [JsonIgnore] public byte mYellowFlagLapsWasOverridden;     // whether mYellowFlagLaps (below) is an admin request (0=no 1=yes 2=clear yellow)

      [JsonIgnore] public bool mSafetyCarExists;                // whether safety car even exists
      [JsonIgnore] public bool mSafetyCarActive;                // whether safety car is active
      [JsonIgnore] public int mSafetyCarLaps;                  // number of laps
      [JsonIgnore] public float mSafetyCarThreshold;            // the threshold at which a safety car is called out (compared to the sum of TrackRulesParticipantV01::mYellowSeverity for each vehicle)
      [JsonIgnore] public double mSafetyCarLapDist;             // safety car lap distance
      [JsonIgnore] public float mSafetyCarLapDistAtStart;       // where the safety car starts from

      [JsonIgnore] public float mPitLaneStartDist;              // where the waypoint branch to the pits breaks off (this may not be perfectly accurate)
      [JsonIgnore] public float mTeleportLapDist;               // the front of the teleport locations (a useful first guess as to where to throw the green flag)

      // future input expansion
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        [JsonIgnore] public byte[] mInputExpansion;

      // input/output
      [JsonIgnore] public sbyte mYellowFlagState;         // see ScoringInfoV01 for values
      [JsonIgnore] public short mYellowFlagLaps;                // suggested number of laps to run under yellow (may be passed in with admin command)

      [JsonIgnore] public int mSafetyCarInstruction;           // 0=no change, 1=go active, 2=head for pits
      [JsonIgnore] public float mSafetyCarSpeed;                // maximum speed at which to drive
      [JsonIgnore] public float mSafetyCarMinimumSpacing;       // minimum spacing behind safety car (-1 to indicate no limit)
      [JsonIgnore] public float mSafetyCarMaximumSpacing;       // maximum spacing behind safety car (-1 to indicate no limit)

      [JsonIgnore] public float mMinimumColumnSpacing;          // minimum desired spacing between vehicles in a column (-1 to indicate indeterminate/unenforced)
      [JsonIgnore] public float mMaximumColumnSpacing;          // maximum desired spacing between vehicles in a column (-1 to indicate indeterminate/unenforced)

      [JsonIgnore] public float mMinimumSpeed;                  // minimum speed that anybody should be driving (-1 to indicate no limit)
      [JsonIgnore] public float mMaximumSpeed;                  // maximum speed that anybody should be driving (-1 to indicate no limit)

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
        [JsonIgnore] public byte[] mMessage;                  // a message for everybody to explain what is going on (which will get run through translator on client machines)
      [JsonIgnore] public IntPtr mParticipant;         // array of partipants (vehicles)

      // future input/output expansion
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        [JsonIgnore] public byte[] mInputOutputExpansion;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PitMenuV01
    {
      [JsonIgnore] public int mCategoryIndex;                  // index of the current category
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [JsonIgnore] public byte[] mCategoryName;             // name of the current category (untranslated)

      [JsonIgnore] public int mChoiceIndex;                    // index of the current choice (within the current category)
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [JsonIgnore] public byte[] mChoiceString;              // name of the current choice (may have some translated words)
      [JsonIgnore] public int mNumChoices;                     // total number of choices (0 <= mChoiceIndex < mNumChoices)

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        [JsonIgnore] public byte[] mExpansion;      // for future use
    }

    //#########################################################################
    //# Plugin classes used to access internals                                #
    //##########################################################################

    // deprecated (callbacks are no longer invoked in DX11) since V8
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ScreenInfoV01
    {
      [JsonIgnore] public IntPtr mAppWindow;                      // Application window handle
      [JsonIgnore] public IntPtr mDevice;                        // Cast type to LPDIRECT3DDEVICE9
      [JsonIgnore] public IntPtr mRenderTarget;                  // Cast type to LPDIRECT3DTEXTURE9
      [JsonIgnore] public int mDriver;                         // Current video driver index

      [JsonIgnore] public int mWidth;                          // Screen width
      [JsonIgnore] public int mHeight;                         // Screen height
      [JsonIgnore] public int mPixelFormat;                    // Pixel format
      [JsonIgnore] public int mRefreshRate;                    // Refresh rate
      [JsonIgnore] public int mWindowed;                       // Really just a boolean whether we are in windowed mode

      [JsonIgnore] public int mOptionsWidth;                   // Width dimension of screen portion used by UI
      [JsonIgnore] public int mOptionsHeight;                  // Height dimension of screen portion used by UI
      [JsonIgnore] public int mOptionsLeft;                    // Horizontal starting coordinate of screen portion used by UI
      [JsonIgnore] public int mOptionsUpper;                   // Vertical starting coordinate of screen portion used by UI

      [JsonIgnore] public byte mOptionsLocation;       // 0=main UI, 1=track loading, 2=monitor, 3=on track
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
        [JsonIgnore] public byte[] mOptionsPage;              // the name of the options page

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 224)]
        [JsonIgnore] public byte[] mExpansion;      // future use
    }

    // replaces the ScreenInfoV01 structure that was deprecated since V8
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ApplicationStateV01 {
      [JsonIgnore] public IntPtr mAppWindow;                      // application window handle
      [JsonIgnore] public uint mWidth;                 // screen width
      [JsonIgnore] public uint mHeight;                // screen height
      [JsonIgnore] public uint mRefreshRate;           // refresh rate
      [JsonIgnore] public uint mWindowed;              // really just a boolean whether we are in windowed mode
      [JsonIgnore] public byte mOptionsLocation;       // 0=main UI, 1=track loading, 2=monitor, 3=on track
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
        [JsonIgnore] public byte[] mOptionsPage;              // the name of the options page
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 204)]
        [JsonIgnore] public byte[] mExpansion;      // future use
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CustomControlInfoV01
    {
      // The name passed through CheckHWControl() will be the mUntranslatedName prepended with an underscore (e.g. "Track Map Toggle" -> "_Track Map Toggle")
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        [JsonIgnore] public byte[] mUntranslatedName;         // name of the control that will show up in UI (but translated if available)
      [JsonIgnore] public int mRepeat;                         // 0=registers once per hit, 1=registers once, waits briefly, then starts repeating quickly, 2=registers as long as key is down
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        [JsonIgnore] public byte[] mExpansion;       // future use
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct WeatherControlInfoV01
    {
      // The current conditions are passed in with the API call. The following ET (Elapsed Time) value should typically be far
      // enough in the future that it can be interpolated smoothly, and allow clouds time to roll in before rain starts. In
      // other words you probably shouldn't have mCloudiness and mRaining suddenly change from 0.0 to 1.0 and expect that
      // to happen in a few seconds without looking crazy.
      [JsonIgnore] public double mET;                           // when you want this weather to take effect

      // mRaining[1][1] is at the ori
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3*3)]
        [JsonIgnore] public double[] mRaining; // placeholder for mRaining matrix
      [JsonIgnore] public double mCloudiness;
      [JsonIgnore] public double mWindSpeed;
      [JsonIgnore] public double mWindDirection;
      [JsonIgnore] public double mRainIntensity;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 187)]
        [JsonIgnore] public byte[] mExpansion; // future use
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CustomVariableV01
    {
      [JsonIgnore] public int mType; // placeholder
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        [JsonIgnore] public byte[] mName;                    // Enumerated name of setting (only used if CustomVariableV01::mNumSettings > 0). This will be stored in the JSON file for informational purposes only. It may also possibly be used in the UI in the future.
      [JsonIgnore] public int mNumSettings; 
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        [JsonIgnore] public byte[] mExpansion;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CustomSettingV01
    {
      [JsonIgnore] public int mID; 
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        [JsonIgnore] public byte[] mName;                    // name of setting
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        [JsonIgnore] public byte[] mExpansion;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MultiSessionParticipantV01
    {
      [JsonIgnore] public int mID; // slot ID
      [JsonIgnore] public int mGridPosition; // 1-based grid position
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        [JsonIgnore] public byte[] mExpansion; // future expansion
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MultiSessionRulesV01
    {
      [JsonIgnore] public int mSession;                        // current session (0=testday 1-4=practice 5-8=qual 9=warmup 10-13=race)
      [JsonIgnore] public int mSpecialSlotID;                  // slot ID of someone who just joined, or -2 requesting to update qual order, or -1 (default/general)
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [JsonIgnore] public byte[] mTrackType;                // track type from GDB
      [JsonIgnore] public int mNumParticipants;                // number of participants (vehicles)

      // input/output
      [JsonIgnore] public IntPtr mParticipant;       // array of partipants (vehicles)
      [JsonIgnore] public int mNumQualSessions;                // number of qualifying sessions configured
      [JsonIgnore] public int mNumRaceSessions;                // number of race sessions configured
      [JsonIgnore] public int mMaxLaps;                        // maximum laps allowed in current session (LONG_MAX = unlimited) (note: cannot currently edit in *race* sessions)
      [JsonIgnore] public int mMaxSeconds;                     // maximum time allowed in current session (LONG_MAX = unlimited) (note: cannot currently edit in *race* sessions)
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [JsonIgnore] public byte[] mName;                     // untranslated name override for session (please use mixed case here, it should get uppercased if necessary)

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        [JsonIgnore] public byte[] mExpansion; // future expansion
    }

    //#########################################################################
    //# Plugin classes used to access internals                                #
    //##########################################################################
}
