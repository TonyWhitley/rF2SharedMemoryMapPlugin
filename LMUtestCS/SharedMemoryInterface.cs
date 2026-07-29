// Begin copied content from LMUtestCS/InternalsPlugin.hpp - kept verbatim as comments
// -----------------------------------------------------------------------------
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

/*
#ifndef _INTERNALS_PLUGIN_HPP_
#define _INTERNALS_PLUGIN_HPP_

#include "PluginObjects.hpp"     // base class for plugin objects to derive from
#include <cmath>                 // for sqrt()
#include <windows.h>             // for HWND
// rF2 and plugins must agree on structure packing, so set it explicitly here.
// Whatever the current packing is will be restored at the end of this include
// with another #pragma.
#pragma pack( push, 4 )

enum class IP_VehicleClass : uint8_t {
    Hypercar = 0x00,
    LMP2_ELMS = 0x02,
    LMP2,
    LMP3,
    GTE,
    GT3,
    PaceCar = 0x08,
    Unknown = 0xFF
};

enum class IP_VehicleChampionship : uint8_t {
    WEC_2023 = 0x00, WEC_2024, WEC_2025, WEC_2026,
    ELMS_2025 = 0X10, ELMS_2026,
    Unknown = 0xFF
};

//#########################################################################
//# Version01 Structures                                                   #
//##########################################################################

struct TelemVect3
{
    union
    {
        struct
        {
            double x, y, z;
        };

        double data[3];
    };


  void Set( const double a, const double b, const double c )  { x = a; y = b; z = c; }

  // Allowed to reference as [0], [1], or [2], instead of .x, .y, or .z, respectively
        double &operator[]( long i )               { return( data [ i ] ); }
  const double &operator[]( long i ) const         { return( data [ i ] ); }
};


struct TelemQuat
{
  double w, x, y, z;

  // Convert this quaternion to a matrix
  void ConvertQuatToMat( TelemVect3 ori[3] ) const
  {
    const double x2 = x + x;
    const double xx = x * x2;
    const double y2 = y + y;
    const double yy = y * y2;
    const double z2 = z + z;
    const double zz = z * z2;
    const double xz = x * z2;
    const double xy = x * y2;
    const double wy = w * y2;
    const double wx = w * x2;
    const double wz = w * z2;
    const double yz = y * z2;
    ori[0][0] = (double) 1.0 - ( yy + zz );
    ori[0][1] = xy - wz;
    ori[0][2] = xz + wy;
    ori[1][0] = xy + wz;
    ori[1][1] = (double) 1.0 - ( xx + zz );
    ori[1][2] = yz - wx;
    ori[2][0] = xz - wy;
    ori[2][1] = yz + wx;
    ori[2][2] = (double) 1.0 - ( xx + yy );
  }





  // Convert a matrix to this quaternion
  void ConvertMatToQuat( const TelemVect3 ori[3] )
  {
    const double trace = ori[0][0] + ori[1][1] + ori[2][2] + (double) 1.0;
    if( trace > 0.0625f )
    {
      const double sqrtTrace = sqrt( trace );
      const double s = (double) 0.5 / sqrtTrace;
      w = (double) 0.5 * sqrtTrace;
      x = ( ori[2][1] - ori[1][2] ) * s;
      y = ( ori[0][2] - ori[2][0] ) * s;
      z = ( ori[1][0] - ori[0][1] ) * s;
    }
    else if( ( ori[0][0] > ori[1][1] ) && ( ori[0][0] > ori[2][2] ) )
    {
      const double sqrtTrace = sqrt( (double) 1.0 + ori[0][0] - ori[1][1] - ori[2][2] );
      const double s = (double) 0.5 / sqrtTrace;
      w = ( ori[2][1] - ori[1][2] ) * s;
      x = (double) 0.5 * sqrtTrace;
      y = ( ori[0][1] + ori[1][0] ) * s;
      z = ( ori[0][2] + ori[2][0] ) * s;
    }
    else if( ori[1][1] > ori[2][2] )
    {
      const double sqrtTrace = sqrt( (double) 1.0 + ori[1][1] - ori[0][0] - ori[2][2] );
      const double s = (double) 0.5 / sqrtTrace;
      w = ( ori[0][2] - ori[2][0] ) * s;
      x = ( ori[0][1] + ori[1][0] ) * s;
      y = (double) 0.5 * sqrtTrace;
      z = ( ori[1][2] + ori[2][1] ) * s;
    }
    else
    {
      const double sqrtTrace = sqrt( (double) 1.0 + ori[2][2] - ori[0][0] - ori[1][1] );
      const double s = (double) 0.5 / sqrtTrace;
      w = ( ori[1][0] - ori[0][1] ) * s;
      x = ( ori[0][2] + ori[2][0] ) * s;
      y = ( ori[1][2] + ori[2][1] ) * s;
      z = (double) 0.5 * sqrtTrace;
    }
  }
};


struct TelemWheelV01
{
  double mSuspensionDeflection;  // meters
  double mRideHeight;            // meters
  double mSuspForce;             // pushrod load in Newtons
  double mBrakeTemp;             // Celsius
  double mBrakePressure;         // currently 0.0-1.0, depending on driver input and brake balance; will convert to true brake pressure (kPa) in future

  double mRotation;              // radians/sec
  double mLateralPatchVel;       // lateral velocity at contact patch
  double mLongitudinalPatchVel;  // longitudinal velocity at contact patch
  double mLateralGroundVel;      // lateral velocity at contact patch
  double mLongitudinalGroundVel; // longitudinal velocity at contact patch
  double mCamber;                // radians (positive is left for left-side wheels, right for right-side wheels)
  double mLateralForce;          // Newtons
  double mLongitudinalForce;     // Newtons
  double mTireLoad;              // Newtons

  double mGripFract;             // an approximation of what fraction of the contact patch is sliding
  double mPressure;              // kPa (tire pressure)
  double mTemperature[3];        // Kelvin (subtract 273.15 to get Celsius), left/center/right (not to be confused with inside/center/outside!)
  double mWear;                  // wear (0.0-1.0, fraction of maximum) ... this is not necessarily proportional with grip loss
  char mTerrainName[16];         // the material prefixes from the TDF file
  unsigned char mSurfaceType;    // 0=dry, 1=wet, 2=grass, 3=dirt, 4=gravel, 5=rumblestrip, 6=special
  bool mFlat;                    // whether tire is flat
  bool mDetached;                // whether wheel is detached
  unsigned char mStaticUndeflectedRadius; // tire radius in centimeters

  double mVerticalTireDeflection;// how much is tire deflected from its (speed-sensitive) radius
  double mWheelYLocation;        // wheel's y location relative to vehicle y location
  double mToe;                   // current toe angle w.r.t. the vehicle

  double mTireCarcassTemperature;       // rough average of temperature samples from carcass (Kelvin)
  double mTireInnerLayerTemperature[3]; // rough average of temperature samples from innermost layer of rubber (before carcass) (Kelvin)

  float_t mOptimalTemp;
  unsigned char mCompoundIndex;
  unsigned char mCompoundType;
  unsigned char mExpansion[18];
};

//
// (rest of InternalsPlugin.hpp omitted here for brevity in the comment block - full file content kept in original header in the repo)
// -----------------------------------------------------------------------------
// End copied content
*/

using Newtonsoft.Json;

using System;
using System.Runtime.InteropServices;

namespace XlmuSharedMemory.LMUData
{
    // Shared-memory constants
    public static class SharedMemoryConsts
    {
        public const string LMU_SHARED_MEMORY_FILE = "LMU_Data";
        public const string LMU_SHARED_MEMORY_EVENT = "LMU_Data_Event";
        public const int SME_MAX = 16;
        public const int MAX_PATH = 260;
        public const int VEHICLE_ARRAY_COUNT = 104;
        public const int SCORING_STREAM_MAX = 65536;
    }

    public enum SharedMemoryEvent : uint
    {
        SME_ENTER,
        SME_EXIT,
        SME_STARTUP,
        SME_SHUTDOWN,
        SME_LOAD,
        SME_UNLOAD,
        SME_START_SESSION,
        SME_END_SESSION,
        SME_ENTER_REALTIME,
        SME_EXIT_REALTIME,
        SME_UPDATE_SCORING,
        SME_UPDATE_TELEMETRY,
        SME_INIT_APPLICATION,
        SME_UNINIT_APPLICATION,
        SME_SET_ENVIRONMENT,
        SME_FFB,
        SME_MAX
    }

    // Vehicle class and championship enums (from InternalsPlugin.hpp)
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
        WEC_2023 = 0x00,
        WEC_2024,
        WEC_2025,
        WEC_2026,
        ELMS_2025 = 0x10,
        ELMS_2026,
        Unknown = 0xFF
    }

    // TelemVect3 - matches C++ layout (x,y,z) with Pack=4
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct TelemVect3
    {
        public double x;
        public double y;
        public double z;
    }

    // TelemQuat - quaternion
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct TelemQuat
    {
        public double w;
        public double x;
        public double y;
        public double z;
    }

    // TelemWheelV01 - full field parity
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct TelemWheelV01
    {
        [JsonIgnore] public double mSuspensionDeflection;                         // meters
        [JsonIgnore] public double mRideHeight;                                   // meters
        [JsonIgnore] public double mSuspForce;                                    // pushrod load in Newtons
        public double mBrakeTemp;                                                 // Celsius
        [JsonIgnore] public double mBrakePressure;                                // currently 0.0-1.0, depending on driver input and brake balance; will convert to true brake pressure (kPa) in future

        public double mRotation;                                                  // radians/sec
        [JsonIgnore] public double mLateralPatchVel;                              // lateral velocity at contact patch
        [JsonIgnore] public double mLongitudinalPatchVel;                         // longitudinal velocity at contact patch
        [JsonIgnore] public double mLateralGroundVel;                             // lateral velocity at contact patch
        [JsonIgnore] public double mLongitudinalGroundVel;                        // longitudinal velocity at contact patch
        [JsonIgnore] public double mCamber;                                       // radians (positive is left for left-side wheels, right for right-side wheels)
        [JsonIgnore] public double mLateralForce;                                 // Newtons
        [JsonIgnore] public double mLongitudinalForce;                            // Newtons
        [JsonIgnore] public double mTireLoad;                                     // Newtons

        [JsonIgnore] public double mGripFract;                                    // an approximation of what fraction of the contact patch is sliding
        public double mPressure;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            public double[] mTemperature;                                             // Kelvin (subtract 273.15 to get Celsius), left/center/right (not to be confused with inside/center/outside!)
            public double mWear;                                                      // wear (0.0-1.0, fraction of maximum) ... this is not necessarily proportional with grip loss
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]
            [JsonIgnore] public byte[] mTerrainName;                                  // the material prefixes from the TDF file
            public byte mSurfaceType;                                                 // 0=dry, 1=wet, 2=grass, 3=dirt, 4=gravel, 5=rumblestrip, 6=special
            public byte mFlat;                                                        // whether tire is flat
            public byte mDetached;                                                    // whether wheel is detached
        public byte mStaticUndeflectedRadius;

            [JsonIgnore] public double mVerticalTireDeflection;                       // how much is tire deflected from its (speed-sensitive) radius
            [JsonIgnore] public double mWheelYLocation;                               // wheel's y location relative to vehicle y location
            [JsonIgnore] public double mToe;                                          // current toe angle w.r.t. the vehicle

            [JsonIgnore] public double mTireCarcassTemperature;                       // rough average of temperature samples from carcass (Kelvin)
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            [JsonIgnore] public double[] mTireInnerLayerTemperature;                  // rough average of temperature samples from innermost layer of rubber (before carcass) (Kelvin)

        public float mOptimalTemp;
        public byte mCompoundIndex;
        public byte mCompoundType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
            [JsonIgnore] byte[] mExpansion;                                           // for future use
    }

    // TelemInfoV01 - full translation to match InternalsPlugin.hpp fields and comments
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct TelemInfoV01
    {
        // Time
        public int mID;                      // slot ID (note that it can be re-used in multiplayer after someone leaves)
        [JsonIgnore] public double mDeltaTime;                                    // time since last update (seconds)
        public double mElapsedTime;                                               // game session time
        [JsonIgnore] public int mLapNumber;                                       // current lap number
        [JsonIgnore] public double mLapStartET;                                   // time this lap was started
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
        [JsonIgnore] public byte[] mVehicleName;                                  // current vehicle name
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
        [JsonIgnore] public byte[] mTrackName;                                    // current track name

        // Position and derivatives
        public TelemVect3 mPos;               // world position in meters
        public TelemVect3 mLocalVel;          // velocity (meters/sec) in local vehicle coordinates
        [JsonIgnore] public TelemVect3 mLocalAccel;        // acceleration (meters/sec^2) in local vehicle coordinates

        // Orientation and derivatives
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public TelemVect3[] mOri;            // rows of orientation matrix
        [JsonIgnore] public TelemVect3 mLocalRot;          // rotation (radians/sec) in local vehicle coordinates
        [JsonIgnore] public TelemVect3 mLocalRotAccel;     // rotational acceleration (radians/sec^2) in local vehicle coordinates

        // Vehicle status
        public int mGear;                    // -1=reverse, 0=neutral, 1+=forward gears
        public double mEngineRPM;             // engine RPM
        public double mEngineWaterTemp;       // Celsius
        public double mEngineOilTemp;         // Celsius
        [JsonIgnore] public double mClutchRPM;             // clutch RPM

        // Driver input
        public double mUnfilteredThrottle;    // ranges  0.0-1.0
        public double mUnfilteredBrake;       // ranges  0.0-1.0
        [JsonIgnore] public double mUnfilteredSteering;    // ranges -1.0-1.0 (left to right)
        public double mUnfilteredClutch;      // ranges  0.0-1.0

                                                                                      // Filtered input (various adjustments for rev or speed limiting, TC, ABS?, speed sensitive steering, clutch work for semi-automatic shifting, etc.)
            [JsonIgnore] public double mFilteredThrottle;                             // ranges  0.0-1.0
            [JsonIgnore] public double mFilteredBrake;                                // ranges  0.0-1.0
            [JsonIgnore] public double mFilteredSteering;                             // ranges -1.0-1.0 (left to right)
            [JsonIgnore] public double mFilteredClutch;                               // ranges  0.0-1.0

                                                                                      // Misc
            [JsonIgnore] public double mSteeringShaftTorque;                          // torque around steering shaft (used to be mSteeringArmForce, but that is not necessarily accurate for feedback purposes)
            [JsonIgnore] public double mFront3rdDeflection;                           // deflection at front 3rd spring
            [JsonIgnore] public double mRear3rdDeflection;                            // deflection at rear 3rd spring

                                                                                      // Aerodynamics
            [JsonIgnore] public double mFrontWingHeight;                              // front wing height
            [JsonIgnore] public double mFrontRideHeight;                              // front ride height
            [JsonIgnore] public double mRearRideHeight;                               // rear ride height
            [JsonIgnore] public double mDrag;                                         // drag
            [JsonIgnore] public double mFrontDownforce;                               // front downforce
            [JsonIgnore] public double mRearDownforce;                                // rear downforce

                                                                                      // State/damage info
            public double mFuel;                                                      // amount of fuel (liters)
            public double mEngineMaxRPM;                                              // rev limit
            public byte mScheduledStops;                                              // number of scheduled pitstops
            public byte mOverheating;                                                 // whether overheating icon is shown
            public byte mDetached;                                                    // whether any parts (besides wheels) have been detached
            [JsonIgnore] public byte mHeadlights;                                     // whether headlights are on
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            [JsonIgnore] public byte[] mDentSeverity;                                 // dent severity at 8 locations around the car (0=none, 1=some, 2=more)
            public double mLastImpactET;                                              // time of last impact
            [JsonIgnore] public double mLastImpactMagnitude;                          // magnitude of last impact
        [JsonIgnore] public TelemVect3 mLastImpactPos;     // location of last impact

                                                                                      // Expanded
            [JsonIgnore] public double mEngineTorque;                                 // current engine torque (including additive torque) (used to be mEngineTq, but there's little reason to abbreviate it)
            [JsonIgnore] public int mCurrentSector;                                   // the current sector (zero-based) with the pitlane stored in the sign bit (example: entering pits from third sector gives 0x80000002)
            public byte mSpeedLimiter;                                                // whether speed limiter is on
            [JsonIgnore] public byte mMaxGears;                                       // maximum forward gears
            public byte mFrontTireCompoundIndex;                                      // index within brand
            [JsonIgnore] public byte mRearTireCompoundIndex;                          // index within brand
            [JsonIgnore] public double mFuelCapacity;                                 // capacity in liters
            [JsonIgnore] public byte mFrontFlapActivated;                             // whether front flap is activated
            public byte mRearFlapActivated;                                           // whether rear flap is activated
            public byte mRearFlapLegalStatus;                                         // 0=disallowed, 1=criteria detected but not allowed quite yet, 2=allowed
            [JsonIgnore] public byte mIgnitionStarter;                                // 0=off 1=ignition 2=ignition+starter

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 18)]
            public byte[] mFrontTireCompoundName;                                     // name of front tire compound
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 18)]
            [JsonIgnore] public byte[] mRearTireCompoundName;                         // name of rear tire compound

            public byte mSpeedLimiterAvailable;                                       // whether speed limiter is available
            [JsonIgnore] public byte mAntiStallActivated;                             // whether (hard) anti-stall is activated
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
            [JsonIgnore] public byte[] mUnused;                                       //
            [JsonIgnore] public float mVisualSteeringWheelRange;                      // the *visual* steering wheel range

            [JsonIgnore] public double mRearBrakeBias;                                // fraction of brakes on rear
            [JsonIgnore] public double mTurboBoostPressure;                           // current turbo boost pressure if available
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            [JsonIgnore] public float[] mPhysicsToGraphicsOffset;                     // offset from static CG to graphical center
            [JsonIgnore] public float mPhysicalSteeringWheelRange;                    // the *physical* steering wheel range

                                                                                      // deltabest
        [JsonIgnore] public double mDeltaBest;             // omitted in error by S397

            [JsonIgnore] public double mBatteryChargeFraction;                        // Battery charge as fraction [0.0-1.0]

                                                                                      // electric boost motor
            [JsonIgnore] public double mElectricBoostMotorTorque;                     // current torque of boost motor (can be negative when in regenerating mode)
            [JsonIgnore] public double mElectricBoostMotorRPM;                        // current rpm of boost motor
            [JsonIgnore] public double mElectricBoostMotorTemperature;                // current temperature of boost motor
            [JsonIgnore] public double mElectricBoostWaterTemperature;                // current water temperature of boost motor cooler if present (0 otherwise)

            [JsonIgnore] public byte mElectricBoostMotorState;                        // 0=unavailable 1=inactive, 2=propulsion, 3=regeneration
            [JsonIgnore] public byte mLapInvalidated;
            [JsonIgnore] public byte mABSActive;
            [JsonIgnore] public byte mTCActive;
            [JsonIgnore] public byte mSpeedLimiterActive;
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
            [JsonIgnore] public byte mTrackLimitsSteps;                               // Normalized track limits points (TrackLimitPoints * TrackLimitStepsPerPoint)
            [JsonIgnore] public float mRegen;                                         // kW
            [JsonIgnore] public float mStateOfCharge;                                 // battery state of charge (percent)
            public float mVirtualEnergy;
            [JsonIgnore] public float mTimeGapCarAhead;
            [JsonIgnore] public float mTimeGapCarBehind;
            [JsonIgnore] public float mTimeGapPlaceAhead;
            [JsonIgnore] public float mTimeGapPlaceBehind;
		    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 30)]
            [JsonIgnore] public char[] mVehicleModel;
        [JsonIgnore] public IP_VehicleClass mVehicleClass;
        [JsonIgnore] public IP_VehicleChampionship mVehicleChampionship;

        // Future use
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        [JsonIgnore] public byte[] mExpansion; // for future use

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public TelemWheelV01[] mWheel; // wheel info (front left, front right, rear left, rear right)
    }

    // VehicleScoringInfoV01 - full translation with comments
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct VehicleScoringInfoV01
    {
        public int mID;                      // slot ID
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] mDriverName;                                                // driver name
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
            [JsonIgnore] public byte[] mVehicleName;                                  // vehicle name
        public short mTotalLaps;            // laps completed
        public sbyte mSector;               // 0=sector3, 1=sector1, 2=sector2
        public sbyte mFinishStatus;         // 0=none,1=finished,2=dnf,3=dq
        public double mLapDist;             // current distance around track
        public double mPathLateral;         // lateral position w.r.t center path
        public double mTrackEdge;           // track edge on same side of track as vehicle

        public double mBestSector1;         // best sector 1
        public double mBestSector2;         // best sector 2 (plus sector 1)
        public double mBestLapTime;         // best lap time
        public double mLastSector1;         // last sector 1
        public double mLastSector2;         // last sector 2 (plus sector 1)
        public double mLastLapTime;         // last lap time
        public double mCurSector1;          // current sector 1 if valid
        public double mCurSector2;          // current sector 2 if valid

        public short mNumPitstops;          // number of pitstops made
        public short mNumPenalties;         // number of outstanding penalties
            public byte mIsPlayer;                                                    // is this the player's vehicle

            public sbyte mControl;                                                    // who's in control: -1=nobody (shouldn't get this), 0=local player, 1=local AI, 2=remote, 3=replay (shouldn't get this)
            public byte mInPits;                                                      // between pit entrance and pit exit (not always accurate for remote vehicles)
            public byte mPlace;                                                       // 1-based position
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] mVehicleClass;                                              // vehicle class

                                                                                      // Dash Indicators
            public double mTimeBehindNext;                                            // time behind vehicle in next higher place
            [JsonIgnore] public int mLapsBehindNext;                                  // laps behind vehicle in next higher place
            [JsonIgnore] public double mTimeBehindLeader;                             // time behind leader
            [JsonIgnore] public int mLapsBehindLeader;                                // laps behind leader
            public double mLapStartET;                                                // time this lap was started

        // Position and derivatives
        [JsonIgnore] public TelemVect3 mPos;             // world position in meters
        public TelemVect3 mLocalVel;        // velocity in local vehicle coordinates
        public TelemVect3 mLocalAccel;      // acceleration in local vehicle coordinates

        // Orientation and derivatives
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        [JsonIgnore] public TelemVect3[] mOri;           // rows of orientation matrix
        [JsonIgnore] public TelemVect3 mLocalRot;        // rotation in local vehicle coordinates
        [JsonIgnore] public TelemVect3 mLocalRotAccel;   // rotational acceleration

        [JsonIgnore] public byte mHeadlights;            // status of headlights
        public byte mPitState;              // pit state
            [JsonIgnore] public byte mServerScored;                                   // whether this vehicle is being scored by server (could be off in qualifying or racing heats)
            [JsonIgnore] public byte mIndividualPhase;                                // game phases (described below) plus 9=after formation, 10=under yellow, 11=under blue (not used)

            [JsonIgnore] public int mQualification;                                   // 1-based, can be -1 when invalid

            [JsonIgnore] public double mTimeIntoLap;                                  // estimated time into lap
            [JsonIgnore] public double mEstimatedLapTime;                             // estimated laptime used for 'time behind' and 'time into lap' (note: this may changed based on vehicle and setup!?)

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 24)]
            [JsonIgnore] public byte[] mPitGroup;                                     // pit group (same as team name unless pit is shared)
        public byte mFlag;                  // primary flag shown to vehicle
        [MarshalAs(UnmanagedType.I1)]
            [JsonIgnore] public byte mUnderYellow;                                    // whether this car has taken a full-course caution flag at the start/finish line
        public byte mCountLapFlag;          // 0 = do not count lap or time, 1 = count lap but not time, 2 = count lap and time
            public byte mInGarageStall;                                               // appears to be within the correct garage stall

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]
            [JsonIgnore] public byte[] mUpgradePack;                                  // Coded upgrades

            public float mPitLapDist;                                                 // location of pit in terms of lap distance

            [JsonIgnore] public float mBestLapSector1;                                // sector 1 time from best lap (not necessarily the best sector 1 time)
            [JsonIgnore] public float mBestLapSector2;                                // sector 2 time from best lap (not necessarily the best sector 2 time)

            [JsonIgnore] public Int64 mSteamID;                                       // SteamID of the current driver (if any)

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
            [JsonIgnore] public char[] mVehFilename;                                  // filename of veh file used to identify this vehicle.

            [JsonIgnore] public short mAttackMode;

            // 2020.11.12 - Took 1 byte from mExpansion to transmit fuel percentage
            public byte mFuelFraction;                                                // Percentage of fuel or battery left in vehicle. 0x00 = 0%; 0xFF = 100%

            // 2021.05.28 - Took 1 byte from mExpansion to transmit DRS (RearFlap) state - consider making this a bitfield if further bools are needed later on
            [JsonIgnore] public byte mDRSState;
            // Future use
            // tag.2012.04.06 - SEE ABOVE!
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
            [JsonIgnore] public byte[] mExpansion;                                    // for future use
    }

    // ScoringInfoV01 - full translation
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct ScoringInfoV01
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string mTrackName;           // current track name
        public int mSession;                 // current session
        public double mCurrentET;           // current time
        public double mEndET;               // ending time
        public int mMaxLaps;                // maximum laps
        public double mLapDist;             // distance around track
        public IntPtr mResultsStream;       // results stream additions since last update

        public int mNumVehicles;             // current number of vehicles

        public byte mGamePhase;              // Game phase
        public sbyte mYellowFlagState;      // Yellow flag state
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public sbyte[] mSectorFlag;         // whether there are local yellows in each sector
        public byte mStartLight;            // start light frame
        public byte mNumRedLights;          // number of red lights in start sequence
        [MarshalAs(UnmanagedType.I1)]
        public bool mInRealtime;            // in realtime as opposed to at the monitor
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string mPlayerName;          // player name
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string mPlrFileName;         // may be encoded to be a legal filename

        // weather
        public double mDarkCloud;           // cloud darkness 0.0-1.0
        public double mRaining;             // raining severity 0.0-1.0
        public double mAmbientTemp;         // temperature (Celsius)
        public double mTrackTemp;           // temperature (Celsius)
        public TelemVect3 mWind;            // wind speed
        public double mMinPathWetness;      // minimum wetness on main path 0.0-1.0
        public double mMaxPathWetness;      // maximum wetness on main path 0.0-1.0

        // multiplayer
        public byte mGameMode;              // 1 = server, 2 = client, 3 = server and client
        [MarshalAs(UnmanagedType.I1)]
        public bool mIsPasswordProtected;   // is the server password protected
        public ushort mServerPort;          // the port of the server
        public uint mServerPublicIP;        // the public IP address of the server
        public int mMaxPlayers;             // maximum number of vehicles that can be in the session
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string mServerName;          // name of the server
        public float mStartET;              // start time (seconds since midnight) of the event

        public double mAvgPathWetness;      // average wetness on main path

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public byte[] mExpansion;           // Future use

        public IntPtr mVehicle; // VehicleScoringInfoV01* (pointer to array)
    }

    // ApplicationStateV01 kept as raw blob (260 bytes per reader in LMUSharedMemory)
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct ApplicationStateV01
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public byte[] Raw;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct SharedMemoryScoringData
    {
        public ScoringInfoV01 scoringInfo;
        public UIntPtr scoringStreamSize; // size_t
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SharedMemoryConsts.VEHICLE_ARRAY_COUNT)]
        public VehicleScoringInfoV01[] vehScoringInfo; // MUST NOT BE MOVED!
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SharedMemoryConsts.SCORING_STREAM_MAX)]
        public byte[] scoringStream;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct SharedMemoryTelemetryData
    {
        public byte activeVehicles;
        public byte playerVehicleIdx;
        [MarshalAs(UnmanagedType.I1)]
        public bool playerHasVehicle;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SharedMemoryConsts.VEHICLE_ARRAY_COUNT)]
        public TelemInfoV01[] telemInfo;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct SharedMemoryPathData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SharedMemoryConsts.MAX_PATH)]
        public string userData;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SharedMemoryConsts.MAX_PATH)]
        public string customVariables;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SharedMemoryConsts.MAX_PATH)]
        public string stewardResults;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SharedMemoryConsts.MAX_PATH)]
        public string playerProfile;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SharedMemoryConsts.MAX_PATH)]
        public string pluginsFolder;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct SharedMemoryGeneric
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SharedMemoryConsts.SME_MAX)]
        public uint[] events;
        public int gameVersion;
        public float FFBTorque;
        public ApplicationStateV01 appInfo;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct SharedMemoryObjectOut
    {
        public SharedMemoryGeneric generic;
        public SharedMemoryPathData paths;
        public SharedMemoryScoringData scoring;
        public SharedMemoryTelemetryData telemetry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct SharedMemoryLayout
    {
        public SharedMemoryObjectOut data;
    }
}
