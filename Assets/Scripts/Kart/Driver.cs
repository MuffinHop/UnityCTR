using System;
using CTRFramework;
using CTRFramework.Shared;
using CTRFramework.Vram;
using UnityEngine;

namespace OGData.Kart
{
    public class Driver
    {
        public TextureLayout[] WheelSprites = new TextureLayout[4];
        public ushort WheelSize;
        public short WheelRotation;
        public uint TireColor;
        public short ClockReceive;
        public short HazardTimer;
        public Event InstBombThrow;
        public Event InstBubbleThrow;
        public Event InstTntRecv; // on your head
        public Event InstSelf;
        public Event InstTntSend; // drop
        public int InvincibleTimer;
        public int InvisibleTimer;
        public uint InstFlagBackup;
        public sbyte NumWumpas;
        public sbyte NumCrystals;
        public sbyte NumTimeCrates;
        public sbyte AccelConst;
        public sbyte TurnConst;
        public sbyte TurboConst;
        public sbyte HeldItemID;
        public sbyte NumHeldItems;
        public short SuperEngineTimer;
        public short ItemRollTimer;
        public short NoItemTimer;
        public int LapTime;
        public byte LapIndex;
        public byte ClockSend;
        public short JumpMeter;
        public short JumpMeterTimer;
        public byte DriverID;
        public sbyte SimpleTurnState;
        public byte AnimationIndex;
        public byte AnimationFrame;
        public short NumTurbos;
        public ushort TimeAgainstWall;
        
        // 0 - OnInit, First function for spawn, drifting, damage, etc
        // 1 - OnUpdate, updates per frame for any generic purpose
        // 2 - OnInput, convert controller presses into physics variables
        // 3 - OnAudio, engine sounds (always same)
        // 4 ----------- changes from state to state
        // 5 ----------- always same, or zero
        // 6 - OnPhysics
        // 7 - OnCollide (turbo pads and AIs)
        // 8 ----------- always same, or zero
        // 9 - OnWeapon
        // 10 - OnRender, convert position + rotation into instance matrix
        // 11 - OnAnimate
        // 12 ---------- always same, or zero
        public Event[] Functions = new Event[0x0D];
        public Vector3i CoordSpeed;
        public sbyte[] Fill18PreQuadBlock = new sbyte[0xC];
        public QuadBlock CurrentBlockTouching;
        public sbyte[] Fill18_postQuadBlock = new sbyte[0x18];
        public sbyte StepFlagSet;
        public sbyte WaterFlag;
        public short Filler2;
        public short AmpTurnState; // 0xC0
        public short CurrentTerrain;
        public sbyte[] SkidMarks = new sbyte[0x200];
        public int UnknownSkid;
        public uint ActionsFlagSet;
        public uint ActionsFlagSetPrevFrame;
        public uint QuadBlockHeight;
        public Vector3i PositionCurrent;
        public Vector3i PositionPrevious; // previous frame pos
        public Vector4s RotationCurrent;
        public Vector4s RotationPrevious; // previous frame rot
        public int Unk_turboRelated;
        public Event[] DriverAudioQuips = new Event[4]; // 4 in OG
        
        // Here should be couple of matrices, Assume for quaternion
        
        // continues updating while driver is airborne,
        // used for VisMem (sometimes?)
        public QuadBlock UnderDriverQuadBlock;

        public QuadBlock LastValidQuadBlockTouched;

        public Event[] TerrainEvent = new Event[2];
        // 0x358
        // is it ice, gravel, or what?
        short TerrainMeta1;
        // 0x35C
        short TterrainMeta2;

        
        public sbyte[] Fill16 = new sbyte[0x16];

        // force jump (turtles)
        public bool ForceJump;
        public Vector3s KDNormal;
        public sbyte KartState;
        public sbyte ZOffset;
        public char[] Fill14 = new char[0x14];
        public short Speed;
        public short SpeedApprox;
        public char[] FillA = new char[0x4];
        public short AxisRotationY;
        public short AxisRotationX;
        public short Angle;
        public short UnknownFlagSet1;
        public short UnknownFlagSet2;
        public Vector3i SpeedVector;
        public Vector3s UnkVector; // 0x3AC
        public int TurningAcceleration; // 0x3B4
        public char[] Fill2E = new char[0x22]; // 0x3B8

        public short MultDrift;
        public short TurboMeterLeft;
        public ushort OutsideTurboTimer;
        public short TurboAudioCooldown;
        public short Reserves;
        public short MaxGroundSpeed;
        public short NumFramesSpentSteering;
        public short TurnSign; // 0x3E8
        public short PreviousFrameMultDrift; // 0x3EA
        public short TimeUntilDriftSpinout;
        public short Jump_TenBuffer; // ten frame jump buffer
        public short Jump_CooldownMS; // so you can't spam jump too fast
        public short jump_CoyoteTimerMS; // the speedrunners call this "coyote jump"
        // if not zero, and if touch ground,
        // it forces player to jump
        public short Jump_ForcedMS;
        public short Jump_VelY;
        public short UnknownJump3;
        public short JumpLandingBoost;
        public ushort NoInputTimer;
        public ushort BurnTimer;
        public ushort SquishTimer;
        public short StateDriving_0x60;
        public short StateDriving_0x280;
        public short Traction;
        public short Gravity;
        public short Jump;
        public short AccelSlide; // while accelerating sliding
        public short AccelFriction; //while accelerating friction
        public short Sliding; 
        public short DeaccelFriction;
        public short BrakingFriction;
        public short DriftOpening;
        public short DriftFriction;
        public short AccelNoReserves; // Acceleration when no reserves
        public short AccelWthReserves;
        public short CharacterSpeed; // 0x42C CHARACTER
        public short NegativeSpeedometerOffset;
        public short MaxSpeed_SingleTurbo; // 0x430
        public short MaxSpeed_SacredFire; // 0x432
        public short ReserveSpeed; // 0x434
        public short MaskSpeed; // 0x436
        public short DamagedSpeed; // 0x438
        public sbyte CharacterTurn; // 0x43A CHARACTER
        public sbyte ReverseTurningSpeed; // 0x43B
        public short TurnSpeedDecreaseStat; // 0x43C
        public short TurningInputResponseStat; // 0x43E
        public sbyte[] MoreConstsPreTurbo = new sbyte[0x36]; // 0x440
        public sbyte TurboMaxRoom; // 0x476 point where turbo meter is empty
        public sbyte ConstTurboLowRoomWarning; // 0x477 point where turbo meter goes red
        public sbyte[] MoreConstsAfterTurbo = new sbyte[0xA];
        public short DriverRank;
        public uint DistanceToFinishCurr;
        public uint DistanceToFinishCheckpoint;
        public uint DistanceDrivenBackwards;
        // raincloud when you hit red potion
        public Event RainCloud; // 0x4a0
        public Event SomethingTrackingMe; // 0x4a4 is chasing this driver (missile/warpball)
        public Event PlantEatingMe;
        public int TntTimer; // 0x4ac
        public int TensDiscountFromRelicRace; // 0x4b0
        
        
        
        public struct PickupHUD
        {
            // 0x4b4
            public short StartX;
            public short StartY;

            // 0x4b8
            public int Cooldown;

            // 0x4bc
            public int Unk;

            // 0x4c0
            public int Remaining;
        } ;
        public struct LetterHUD
        {
            // 0x4c4
            public short Cooldown;

            // 0x4c6
            public short ModelID;

            // 0x4c8
            public short StartX;
            public short StartY;

            // 0x4cc
            public int NumCollected;
        };
        
        public struct BattleHUD
        {
            // 0x4d0
            public int Cooldown;

            // 0x4d4
            public short StartX;
            public short StartY;

            // 0x4d8
            public int Unk;

            // 0x4dc
            public int ScoreDelta; // -1, 0, 1

            // 0x4e0
            // juiced-up cooldown (unused)

            // 0x4e4
            // player lives in life limit battle mode (3,6,9)
            public int PlayerLives;

            // 0x4e8
            // team in battle mode (blue,red,green,yellow)
            public int Team;
        };

        public uint FramesSinceSinglePlayerRaceEnded; // 0x4ec

        // number milliseconds in race
        public uint NumberOfMillisecondsInRace; // 0x514
        public uint TimeSpentInMud; // 0x524
        public uint TimeSpentWith10Wumpa; // 0x52C
        public uint TimeSpentDrifting; // 0x538
        public string[] EndRaceComments = new string[8];
        public uint DriftBoostTimer; // 0x582

        // bool stillFalling (mask grab)
        public bool StillFalling; // 0x58d (used for mask grab)
        public bool Whistle; // 0x58d (used for mask grab)
        public bool engineRevMaskGrab; // 0x594 (mask grab)

        // FUN_80058f54
        // param1 is current rotation
        // param2 is speed of rotation
        // param3 is desired rotation
        // Rotation Interpolation
        int RotationInterpolation(int currentRotation,int speedOfRotation,int desiredRotation)
        {
            int iVar1 = 0;
            int iVar2 = 0;

            // if desired rotation is less than current
            if (desiredRotation < currentRotation)
            {
                // subtract current by a rate of "speed"
                iVar2 = currentRotation - speedOfRotation;

                // if new rotation is less than desired
                if (currentRotation - speedOfRotation < desiredRotation)
                {
                    // Just use desired rotation
                    return desiredRotation;
                }
            }

            // if desired rotation is not less than current
            else
            {
                // make a copy of current
                iVar1 = currentRotation;
                if (
                    // desired <= current,
                    // we know desired is not less than current,
                    // so this really checks if it is current
                    (desiredRotation <= currentRotation) ||
                    (
                        // if new rotation overshoots desired,
                        // probably should say >=
                        currentRotation + speedOfRotation <= desiredRotation)
                )
                {
                    // copy desired
                    iVar2 = desiredRotation;

                    // add to rotation at rate of "speed"
                    iVar1 = currentRotation + speedOfRotation;
                    // if current = desired
                    // if current is overshot past desired

                    // current = desired
                    iVar2 = iVar1;
                }
            }

            // return new current
            return iVar2;
        }
        public void PlayerDrivingInterpolate(Camera camera, Time time) // FUN_8005fc8c
        {
            Quaternion anglesCamera = camera.transform.rotation;
            // Get Camera Rotation
            int cameraAngleY = (int)(anglesCamera.w * 65536f);
            int cameraAngleAbsolute = Math.Abs(cameraAngleY);
            // elapsed milliseconds per frame, ~32
            int elapsedTimeInMS = (int)(Time.fixedDeltaTime * 1000f);
            int shiftCamAngle = cameraAngleAbsolute >> 3;
            if (shiftCamAngle == 0) {
                shiftCamAngle = 1;
            }
            /* TODO
              if (PhysType.DriftMax < shiftCamAngle) {
                shiftCamAngle = PhysType.DriftMax;
              }
             */
            cameraAngleAbsolute = RotationInterpolation(RotationPrevious.W,8,shiftCamAngle);
            RotationPrevious.W = (short)cameraAngleAbsolute;
            
            int futureAngle = RotationInterpolation(cameraAngleAbsolute,RotationPrevious.W  * elapsedTimeInMS >> 5,0);
            
            uint actionsFlagSet = ActionsFlagSet;
            short turnSign = TurnSign;

            RotationCurrent.W = (short)futureAngle;

            int speedApprox = SpeedApprox;
            int simpleTurnState256 = SimpleTurnState * 0x100;
            if (speedApprox < 1) {
                if (UnknownFlagSet1 < 0) {
                    turnSign = -1;
                    TurnSign = -1;
                }
                if (-1 < speedApprox) {
                    if (-1 < UnknownFlagSet1) {
                        turnSign = 1;
                        TurnSign = 1;
                    }
                }
            }
            else {
                if (-1 < UnknownFlagSet1) {
                    turnSign = 1;
                    TurnSign = 1;
                }
            }
            
            if (turnSign < 0) {
                simpleTurnState256 = SimpleTurnState * -0x100;
                actionsFlagSet = actionsFlagSet ^ 0x10;
            }
            if (speedApprox < 0) {
                speedApprox = -speedApprox;
            }
            
            
            if (
                ((actionsFlagSet & 1) != 0) &&
                // Kart is not on any turbo pad.
                ((StepFlagSet & 3) == 0)
            )
            {
                // Map value from [oldMin, oldMax] to [newMin, newMax]
                // inverting newMin and newMax will give an inverse range mapping
                simpleTurnState256 = Misc.MapToRange(speedApprox,0x10,0x300,0,simpleTurnState256);
            }
            int terrainMeta1 = TerrainMeta1;
            int turningAcceleration = TurningAcceleration;
            short turningAccelerationS16 = (short)turningAcceleration;
            if (simpleTurnState256 == 0)
            {
                // Interpolate rotation by speed
                turnSign = (short)RotationInterpolation(
                    turningAcceleration,
                    (TurningInputResponseStat + (int)TurnConst * 0x32) * (terrainMeta1 + 0x28) >> 8,
                    0);
            }
            else {
                bool isTurnSignNegative = simpleTurnState256 < 0;
                if (isTurnSignNegative) {
                    simpleTurnState256 = -simpleTurnState256;
                    turningAcceleration = -turningAcceleration;
                }
                turningAccelerationS16 = (short)turningAcceleration;
                if (turningAcceleration < simpleTurnState256) {
                    turningAcceleration = turningAcceleration + ((TurningInputResponseStat + TurnConst * 100) * (terrainMeta1 + 0x28) >> 8); // 100%
                    turningAccelerationS16 = (short)turningAcceleration;
                    if (simpleTurnState256 < turningAcceleration) {
                        turningAccelerationS16 = (short)simpleTurnState256;
                    }
                }
                else {
                    if (simpleTurnState256 < turningAcceleration) {
                        turningAcceleration = turningAcceleration - ((TurningInputResponseStat + TurnConst * 0x32) * (terrainMeta1 + 0x28) >> 8); // 50%
                        turningAccelerationS16 = (short)turningAcceleration;
                        if (turningAcceleration < simpleTurnState256) {
                            turningAccelerationS16 = (short)simpleTurnState256;
                        }
                    }
                }
                if (isTurnSignNegative) {
                    turningAccelerationS16 = (short)-turningAccelerationS16;
                }
            }
            // timeUntilDriftSpinout spin out timer.
            int timeUntilDriftSpinout = TimeUntilDriftSpinout;
            turningAcceleration = turningAccelerationS16;
            TurningAcceleration = turningAcceleration;
            int deltaRotation = 0;
            if (timeUntilDriftSpinout != 0) {
                int deltaTime = (timeUntilDriftSpinout - elapsedTimeInMS);

                // Map value from [oldMin, oldMax] to [newMin, newMax]
                // inverting newMin and newMax will give an inverse range mapping
                deltaRotation = Misc.MapToRange(timeUntilDriftSpinout,0,0x140,0,PreviousFrameMultDrift);
                turningAcceleration = turningAcceleration + deltaRotation;
                if (deltaTime < 0) {
                    deltaTime = 0;
                }
                TimeUntilDriftSpinout = (short)deltaTime;
            }
            // iVar11 = character_Speed << 10;
            int characterMagnitude = CharacterSpeed << 0x10;
            // iVar6 = iVar11 >> 10; back to, kind of unoptimized hahaha
            int characterSpeedStat = characterMagnitude >> 0x10;
        }
        
        
        
        
        
    }
}