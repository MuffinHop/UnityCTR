using OpenCTR.Level;
using Testing;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Player
{
    public class Kart : MonoBehaviour
    {
        public float KartTimer;
        public Rigidbody GetRigidbody => _rigidbody;
        public bool HasTurned;
        public int BoostReserve = 0;
        public float BoostSpeedMultiplier = 1f;
        [SerializeField] private float _maxTurn;
        [SerializeField] private float _turnSpeed;
        [SerializeField] private float _relaxTurn;
        [SerializeField] private float _turnSpeedSlowdown;
        [SerializeField] private float _accelerationSpeed;
        [SerializeField] private float _deaccelerationSpeed;
        [SerializeField] private float _maxEngineVelocity;
        [SerializeField] private Vector3 _gravityForce;
        [SerializeField] private Vector3 _angularSidewaysFriction;
        [SerializeField, Range(0f, 32f)] private float _generalFriction;
        [SerializeField] private float _turnLimiter;
        [SerializeField] private float _turnLimiterNeg;
        [FormerlySerializedAs("_speedMinimumForTurn")] [SerializeField] private float _veloTurnMultiplier;
        [SerializeField, Range(0f, 6f)] private float _gravityPushback;
        [SerializeField] private Camera _camera;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private TextMeshProUGUI _textMesh;
        [SerializeField] private readonly float _jump_CoyoteTimerMS = 5f * 1f / 30f; // Coyote timer - how long can the kart jump in air
        [SerializeField] private float _freeFallTime;
        [SerializeField] private TestData _testData;
        [SerializeField] private LevelHandler _level;
        [SerializeField] private Transform _maskPickUpParticleEffect;
        [SerializeField] private Transform _allSkids;
        [SerializeField] private ParticleSystem _smoke;
        [SerializeField] private float sacredFireMultiplier = 1.31171f; //31.171% sacred fire
        [SerializeField] private float ultimateFireMultiplier = 1.9482f; //94.92% ultimate fire
        [SerializeField] private float[] driftMultipliers = new float[]
        {
            slowGreenFireMultiplier, fastGreenFireMultiplier, yellowFireMultiplier
        };
        [SerializeField] private RectTransform _driftMeter;
        [SerializeField] private float _rotationValue = 0f;
        [SerializeField] private float _driftBoostTime = 0f;
        [SerializeField] private int _driftBoosts = 0;
        [SerializeField] private float _acceleratedTimer;

        
        private PlayerController _playerControls;
        private KartCollider _kartCollider;
        private Vector3 _motorOffset;
        private Vector3 _previousFrameVelocity;
        private int _trackPosition;
        private float _killedTimer;
        private int _jumpMeter = 0x00;
        private bool _jumping = false;
        private bool _drifting = false;
        private int _driftDirection = 0;
        private int _frameHitTurboPad = 0;
        private int _lastFrameReserve = 0;
        private bool _grounded;
        private Vector3 _cameraDistanceFromPlayer;
        private bool _left, _right, _accelerate, _L1Button, _R1Button;
        private bool _allowGravity;
        private static float slowGreenFireMultiplier = 1.1558f; //15.58% slow green fire
        private static float fastGreenFireMultiplier = 1.194f; //19.4% fast green fire
        private static float yellowFireMultiplier = 1.2337f; //23.37% yellow fire
        
        private void Start()
        {
            _playerControls = new PlayerController();
            _playerControls.Enable();
            _playerControls.PlayerControls.DpadLeft.started += DpadLeftDown;
            _playerControls.PlayerControls.DpadRight.started += DpadRightDown;
            _playerControls.PlayerControls.ActionSouth.started += AccelerationDown;
            _playerControls.PlayerControls.L1.started += L1Down;
            _playerControls.PlayerControls.R1.started += R1Down;
            _playerControls.PlayerControls.DpadLeft.canceled += DpadLeftUp;
            _playerControls.PlayerControls.DpadRight.canceled += DpadRightUp;
            _playerControls.PlayerControls.ActionSouth.canceled += AccelerationUp;
            _playerControls.PlayerControls.L1.canceled += L1Up;
            _playerControls.PlayerControls.R1.canceled += R1Up;
            
            
            _kartCollider = _rigidbody.GetComponent<KartCollider>();
            _rigidbody.useGravity = false;
            _cameraDistanceFromPlayer = _camera.transform.localPosition;
            _motorOffset = _rigidbody.transform.localPosition;
            _rigidbody.transform.parent = null;
            _camera.transform.parent = null;
            KartTimer = 0f;
            _allowGravity = true;
            
            
            _kartCollider.HitWall += KartHitWall;
            _kartCollider.PlayerKilled += PlayerKilled;
            _kartCollider.HitTurboPad += HitTurboPad;
            _kartCollider.Landed += Landed;
        }

        public void ResetKart(Vector3 position, Quaternion rotation)
        {
            _left = false;
            _right = false;
            _accelerate = false;
            _grounded = false;
            _rotationValue = 0f;
            _rigidbody.velocity = new Vector3(0f,0f,0f); 
            _rigidbody.angularVelocity = new Vector3(0f,0f,0f);
            _rigidbody.position = position;
            _rigidbody.rotation = Quaternion.identity;
            transform.position = position;
            transform.rotation = rotation;
            _allowGravity = true;
        }
        public void DisableCamera()
        {
            _camera.transform.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_playerControls != null)
            {
                _playerControls.PlayerControls.DpadLeft.started -= DpadLeftDown;
                _playerControls.PlayerControls.DpadRight.started -= DpadRightDown;
                _playerControls.PlayerControls.ActionSouth.started -= AccelerationDown;
                _playerControls.PlayerControls.L1.started -= L1Down;
                _playerControls.PlayerControls.R1.started -= R1Down;
                _playerControls.PlayerControls.DpadLeft.canceled -= DpadLeftUp;
                _playerControls.PlayerControls.DpadRight.canceled -= DpadRightUp;
                _playerControls.PlayerControls.ActionSouth.canceled -= AccelerationUp;
                _playerControls.PlayerControls.L1.canceled -= L1Up;
                _playerControls.PlayerControls.R1.canceled -= R1Up;

                _playerControls.Disable();
            }

            if (_kartCollider != null)
            {
                _kartCollider.HitWall -= KartHitWall;
                _kartCollider.PlayerKilled -= PlayerKilled;
                _kartCollider.HitTurboPad -= HitTurboPad;
                _kartCollider.Landed += Landed;
            }

            if (_camera != null)
            {
                Destroy(_camera.transform.gameObject);
            }

            if (_rigidbody != null)
            {
                Destroy(_rigidbody.transform.gameObject);
            }
        }

        private void Update()
        {
            transform.position = _rigidbody.position - _motorOffset;

            Quaternion cameraRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            Vector3 cameraVector = _cameraDistanceFromPlayer;
            Vector3 newCameraVector = cameraRotation * cameraVector;
            _camera.transform.position = transform.position + newCameraVector;
            _camera.transform.LookAt(transform.position + new Vector3(0f,_cameraDistanceFromPlayer.y,0f));
            _camera.fieldOfView = 45f + _rigidbody.velocity.magnitude / 6;
            if (_right || _left)
            {
                HasTurned = true;
            }

            _trackPosition = _kartCollider.GetTrackPosition();
            var allSkidParticles = _allSkids.GetComponentsInChildren<ParticleSystem>();
            foreach (var skidParticles in allSkidParticles)
            {
                if (_drifting && _grounded)
                {
                    skidParticles.Play();
                }
                else
                {
                    skidParticles.Pause();
                }
            }

            if (_drifting && _driftBoosts < 3)
            {
                _driftBoostTime += Time.deltaTime;
                Image sprite = _driftMeter.GetComponent<Image>();
                _driftMeter.localPosition = new Vector3(600f + 150f * _driftBoostTime, -800f, 0f);
                _driftMeter.sizeDelta = new Vector2(300f * _driftBoostTime, 32f);
                if (_driftBoostTime < 0.5f)
                {
                    sprite.color = Color.green;
                }
                else
                {
                    sprite.color = Color.red;
                }

                if (_driftBoostTime > 1.0f && _driftBoosts < 3)
                {
                    _driftBoostTime = 0f;
                    _drifting = false;
                }
            }
            else
            {
                if (_driftMeter != null)
                {
                    _driftMeter.localPosition = new Vector3(600f, -800f, 0f);
                    _driftMeter.sizeDelta = new Vector2(0f, 32f);
                }
            }

            ParticleSystem.MainModule settings = _smoke.main;
            var emissionModule = _smoke.emission;
            if (BoostSpeedMultiplier == 1f)
            {
                settings.startColor = new ParticleSystem.MinMaxGradient(new Color(0f, 0, 0f, 0.25f));
                emissionModule.rateOverTime = 10f;
            } else if (BoostSpeedMultiplier == slowGreenFireMultiplier)
            {
                settings.startColor = new ParticleSystem.MinMaxGradient(new Color(0f, 0.5f, 0f, 0.25f));
                emissionModule.rateOverTime = 15f;
            } else if (BoostSpeedMultiplier == fastGreenFireMultiplier)
            {
                settings.startColor = new ParticleSystem.MinMaxGradient(new Color(0f, 1f, 0f, 0.25f));
                emissionModule.rateOverTime = 20f;
            } else if (BoostSpeedMultiplier == yellowFireMultiplier)
            {
                settings.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 0f, 0.5f));
                emissionModule.rateOverTime = 30f;
            } else if (BoostSpeedMultiplier == sacredFireMultiplier)
            {
                settings.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.5f, 0f, 0.5f));
                emissionModule.rateOverTime = 40f;
            } else if (BoostSpeedMultiplier == ultimateFireMultiplier)
            {
                settings.startColor = new ParticleSystem.MinMaxGradient(new Color(0.5f, 0.5f, 1f, 0.5f));
                emissionModule.rateOverTime = 50f;
            }
        }

        private void FixedUpdate()
        {
            if (_killedTimer > 0f)
            {
                _rigidbody.constraints = RigidbodyConstraints.FreezePosition;
                _killedTimer -= Time.fixedDeltaTime;
            }
            else
            {
                _rigidbody.constraints = RigidbodyConstraints.None;
                _maskPickUpParticleEffect.gameObject.SetActive(false);
            }
            BoostReserve = (int)Mathf.Max(0, BoostReserve - 1000*Time.fixedDeltaTime);
            if (BoostReserve == 0)
            {
                BoostSpeedMultiplier = 1f;
            }
            // Kart visuals should be slightly offset
            transform.position = _rigidbody.position - _motorOffset;
            
            // Get relative velocity of Motor-rigidbody compared to kart.
            Vector3 newSpeed = transform.InverseTransformDirection(_rigidbody.velocity);
            
            // Acceleration and De-Acceleration have timers, this helps with turning against the wall
            if (_accelerate) {
                _acceleratedTimer = Mathf.Min(_acceleratedTimer + Time.fixedDeltaTime * _turnLimiter, 1f);
            } else {
                _acceleratedTimer = Mathf.Max(_acceleratedTimer - Time.fixedDeltaTime * _turnLimiterNeg, 0f);
            }

            float speedSlowdownDueToDrift = 1f;
            if (!_drifting)
            {
                float turnSlowDownInAir = _grounded ? 1f : 0.95f;
                _rotationValue += (_left ? -Time.fixedDeltaTime * _turnSpeed : 0f) +
                                  (_right ? Time.fixedDeltaTime * _turnSpeed : 0f);
                _rotationValue = Mathf.Min(
                                     Mathf.Abs(_rotationValue),
                                     _maxTurn * turnSlowDownInAir *
                                     Mathf.Max(Mathf.Min(Mathf.Abs(newSpeed.z) * _veloTurnMultiplier, 1f),
                                         _acceleratedTimer))
                                 * Mathf.Sign(_rotationValue);

                _rotationValue = Mathf.Sign(_rotationValue) *
                                 Mathf.Max(Mathf.Abs(_rotationValue) - _relaxTurn * Time.fixedDeltaTime, 0);

                transform.Rotate(0f, _rotationValue, 0f);
            }
            else // drifting
            {
                float turnSlowDownInAir = _grounded ? 1f : 0.95f;
                _rotationValue += (_left ? -Time.fixedDeltaTime * _turnSpeed : 0f) +
                                  (_right ? Time.fixedDeltaTime * _turnSpeed : 0f);
                
                _rotationValue = Mathf.Min(
                                     Mathf.Abs(_rotationValue),
                                     _maxTurn * turnSlowDownInAir *
                                     Mathf.Max(Mathf.Min(Mathf.Abs(newSpeed.z) * _veloTurnMultiplier, 1f),
                                         _acceleratedTimer))
                                 * Mathf.Sign(_rotationValue);
                if (_driftDirection < 0)
                {
                    _rotationValue = Mathf.Min(_rotationValue, -_maxTurn / 2f);
                }
                else
                {
                    _rotationValue = Mathf.Max(_rotationValue, _maxTurn / 2f);
                }

                _rotationValue = Mathf.Sign(_rotationValue) *
                                 Mathf.Max(Mathf.Abs(_rotationValue * 1.115f) - _relaxTurn * Time.fixedDeltaTime, 0);

                transform.Rotate(0f, _rotationValue, 0f);
                speedSlowdownDueToDrift = 0.75f;
            }

            float speedSlowdownDueToTurn = 1f - _turnSpeedSlowdown * _rotationValue;


            bool froggy = _L1Button && _R1Button && _grounded;
            froggy &= !froggy;
            float angle = Vector3.Angle(transform.forward, _gravityForce);
            float negFriction = 1f;
            if (!froggy) //froggy
            {
                float adjustMaxSpeed = (angle - 90f) * _deaccelerationSpeed;
                float finalMaxSpeed = (_maxEngineVelocity - adjustMaxSpeed) * BoostSpeedMultiplier *
                                      speedSlowdownDueToTurn * speedSlowdownDueToDrift;
                if (_grounded && _accelerate)
                {
                    if (Mathf.Abs(newSpeed.z) < finalMaxSpeed)
                    {
                        newSpeed.z += Time.fixedDeltaTime * _accelerationSpeed;
                    }
                }

                Vector3 gravityRelative = transform.InverseTransformDirection(_gravityForce);
                newSpeed.z += gravityRelative.z * _gravityPushback * Time.fixedDeltaTime;
                
                if (_grounded && !_accelerate)
                {
                    _rigidbody.drag += _generalFriction * Time.fixedDeltaTime;
                    _rigidbody.angularDrag += _generalFriction * Time.fixedDeltaTime;
                    negFriction = 1.0f - _generalFriction;
                }
                else
                {
                    _rigidbody.drag = 0f;
                    _rigidbody.angularDrag = 0f;
                }
            }
            
            newSpeed.Scale(_angularSidewaysFriction);

            newSpeed = transform.TransformDirection(newSpeed);
                _rigidbody.velocity = newSpeed + (_allowGravity ? _gravityForce * Time.fixedDeltaTime : Vector3.zero) *
                    ((_grounded && !_accelerate) ? Mathf.Max(negFriction, 0f) : 1f);
            
            _previousFrameVelocity = _rigidbody.velocity;
            
            _textMesh.text = Mathf.Floor(Mathf.Sqrt(Mathf.Pow(_rigidbody.velocity.x,2f) + Mathf.Pow( _rigidbody.velocity.z,2f))) + " mph!   ";
            if (_kartCollider.GetGrounded())
            {
                _allowGravity = true;
                Vector3 up = _kartCollider.GetAverageNormal();
                Vector3 vel = transform.forward;
                Vector3 forward = vel - up * Vector3.Dot (vel, up);
                var newRot = Quaternion.LookRotation(forward.normalized, up);
                transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.fixedDeltaTime * 60f/8f);
                _grounded = true;
                _freeFallTime = 0f;
            }
            else
            {
                if (_jumping)
                {
                    _jumpMeter += (int)(Time.fixedDeltaTime * 1000f);
                }
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f,transform.rotation.eulerAngles.y,0f),Time.deltaTime * 2f);
                // Acme-jump, kart will float for 5 frames on NTSC when on freefall - you can also jump during this period.
                _freeFallTime += Time.deltaTime;
                _grounded = false;
            }
        }

        private void LateUpdate()
        {
            transform.position = _rigidbody.position - _motorOffset;
            if (_testData != null)
            {
                Record r = _testData.GetRecord(KartTimer);
                _accelerate = r.x_press > 0;
                _left = r.left> 0;
                _right = r.right> 0;
            }

            KartTimer += Time.fixedDeltaTime;
        }

        public void Mutate(float multiplier, float divider)
        {
            float range;
            for (int j = 0; j < 6; j++)
            {
                switch ((int)Random.Range(0f, 13f))
                {
                    case 0:
                        range = HasTurned ? 8f : 0f;
                        _turnSpeed =
                            Mathf.Max(_turnSpeed + multiplier * Random.Range(-range, range) / divider, 0.0f);
                        break;
                    case 1:
                        range = HasTurned ? 8f : 0f;
                        _relaxTurn = Mathf.Max(
                            _relaxTurn + multiplier * Random.Range(-range, range) / divider,
                            0.0f);
                        break;
                    case 2:
                        range = HasTurned ? 8f : 0f;
                        _maxTurn =
                            Mathf.Max(_maxTurn + multiplier * Random.Range(-range, range) / divider, 0.0f);
                        break;
                    case 3:
                        range = HasTurned ? 0.6f : 0f;
                        _turnLimiter = Mathf.Max(_turnLimiter + multiplier * Random.Range(-range,range) / divider, 0.0f);
                        break;
                    case 4:
                        range = HasTurned ? 0.6f : 0f;
                        _turnLimiterNeg = Mathf.Max(_turnLimiterNeg + multiplier * Random.Range(-range,range) / divider, 0.0f);
                        break;
                    case 5:
                        range = HasTurned ? 0.3f : 0f;
                        _angularSidewaysFriction.x = Mathf.Min(Mathf.Max(_angularSidewaysFriction.x + multiplier * Random.Range(-range,range) / divider, 0.0f),0.3f);
                        break;
                    case 6:
                        range = HasTurned ? 0.4f : 0f;
                        _turnSpeedSlowdown = _turnSpeedSlowdown + multiplier * Random.Range(-range,range) / divider;
                        break;
                    case 7:
                        range = 0.3f;
                        _deaccelerationSpeed = Mathf.Max(
                            _deaccelerationSpeed + multiplier * Random.Range(-range, range) / divider,
                            0.0f);
                        break;
                    case 8:
                        range = 3f;
                        _accelerationSpeed = Mathf.Max(
                            _accelerationSpeed + multiplier * Random.Range(-range, range) / divider,
                            0.0f);
                        break;
                    case 9:
                        range = 3f;
                        _maxEngineVelocity = Mathf.Max(
                            _maxEngineVelocity + multiplier * Random.Range(-range, range) / divider,
                            0.0f);
                        //Gravity is 0,-28.125,0 units/s^2 900>>5
                        //_gravityForce.y = Mathf.Min(_gravityForce.y + multiplier * Random.Range(-range, range) / divider,0f);
                        break;
                    case 10:
                        range = 0.3f;
                        _generalFriction =
                            Mathf.Max(_generalFriction + multiplier * Random.Range(-range, range) / divider, 0.0f);
                        break;
                    case 11:
                        range = 3f;
                        _gravityPushback = Mathf.Max(_gravityPushback + multiplier * Random.Range(-range,range) / divider, 0.0f);
                        break;
                    case 12:
                        range = 0.01f;
                        _veloTurnMultiplier = Mathf.Max(_veloTurnMultiplier + multiplier * Random.Range(-range,range) / divider, 0.0f);
                        break;
                }
            }
            
            KartTimer = 0f;
        }

        public static void Clone(Kart src, Kart target)
        {
            target._accelerationSpeed = src._accelerationSpeed;
            target._deaccelerationSpeed = src._deaccelerationSpeed;
            target._maxEngineVelocity = src._maxEngineVelocity;
            target._maxTurn = src._maxTurn;
            target._turnSpeed = src._turnSpeed;
            target._relaxTurn = src._relaxTurn;
            target._turnSpeedSlowdown = src._turnSpeedSlowdown;
            target._gravityForce = src._gravityForce;
            target._angularSidewaysFriction = src._angularSidewaysFriction;
            target._generalFriction = src._generalFriction;
            target._turnLimiter = src._turnLimiter;
            target._veloTurnMultiplier = src._veloTurnMultiplier;
            target._gravityPushback = src._gravityPushback;
            
        }
        private void OnEnable()
        {
            if(_playerControls!=null)
                _playerControls.Enable();
        }

        private void OnDisable()
        {
            if(_playerControls!=null)
                _playerControls.Disable();
        }

        private void DpadLeftUp(InputAction.CallbackContext context)
        {
            _left = false;
        }

        private void AccelerationUp(InputAction.CallbackContext context)
        {
            _accelerate = false;
        }

        
        private void DpadRightUp(InputAction.CallbackContext context)
        {
            _right = false;
        }
        
        private void DpadLeftDown(InputAction.CallbackContext context)
        {
            _left = true;
        }
        
        private void DpadRightDown(InputAction.CallbackContext context)
        {
            _right = true;
        }
        private void AccelerationDown(InputAction.CallbackContext context)
        {
            _accelerate = true;
        }
        
        private void L1Down(InputAction.CallbackContext context)
        {
            _L1Button = true;
            _rigidbody.drag = 0f;
            _rigidbody.angularDrag = 0f;
            if (_grounded && !_drifting && !_jumping)
            {
                _rigidbody.velocity = new Vector3(_previousFrameVelocity.x,
                    17.953125f * 0.01f * 16f + Mathf.Max(_previousFrameVelocity.y, 0f), _previousFrameVelocity.z);
                _jumping = true;
                _grounded = false;
            } else if (_drifting && _R1Button && _driftBoosts < 3 && _driftBoostTime <= 1f) //drift boost
            {
                if (_driftBoostTime > 0.5f)
                {
                    AddBoost((int)Mathf.Lerp(448, 1920, 2.0f * (_driftBoostTime - 0.5f)),
                        driftMultipliers[_driftBoosts]);
                }

                _driftBoosts ++;
                _driftBoostTime = 0f;
            }

        }
        private void L1Up(InputAction.CallbackContext context)
        {
            _L1Button = false;
            if (_drifting && !_R1Button) 
            {
                _drifting = false;
                _driftBoostTime = 0f;
                _driftBoosts = 0;
            }
        }
        
        private void R1Down(InputAction.CallbackContext context)
        {
            _R1Button = true;
            _rigidbody.drag = 0f;
            _rigidbody.angularDrag = 0f;
            if (_grounded && !_drifting && !_jumping)
            {
                _rigidbody.velocity = new Vector3(_previousFrameVelocity.x,
                    17.953125f * 0.01f * 16f + Mathf.Max(_previousFrameVelocity.y, 0f), _previousFrameVelocity.z);
                _jumping = true;
                _grounded = false;
            } else if (_drifting && _L1Button && _driftBoosts < 3 && _driftBoostTime <= 1f) //drift boost
            {
                if (_driftBoostTime > 0.5f)
                {
                    AddBoost((int)Mathf.Lerp(448, 1920, 2.0f * (_driftBoostTime - 0.5f)),
                        driftMultipliers[_driftBoosts]);
                }

                _driftBoosts ++;
                _driftBoostTime = 0f;
            }

        }
        private void R1Up(InputAction.CallbackContext context)
        {
            _R1Button = false;
            if (_drifting && !_L1Button) 
            {
                _drifting = false;
                _driftBoostTime = 0f;
                _driftBoosts = 0;
            }
        }

        private void KartHitWall(Rigidbody rigidbody, Collision collider)
        {
            if (collider.transform.position.y < _rigidbody.position.y - 0.333f)
            {
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x,17.953125f * 0.01f * 16f,_rigidbody.velocity.z);
            }

            _jumping = false;
            _grounded = false;
        }

        private void PlayerKilled(Rigidbody rigidbody, Collision collider)
        {
            if (_killedTimer > 0f) return;
            _killedTimer = 2f;
            _maskPickUpParticleEffect.gameObject.SetActive(true);
            Vector3 position = _level.restartPts[_trackPosition].Position;
            position.Scale(new Vector3(1f, 1f, -1f));
            _rigidbody.position = position + Vector3.up;
            _rigidbody.rotation = Quaternion.Euler(_level.restartPts[_trackPosition].Rotation + new Vector3(0f,90f,0f));
        }

        private void HitTurboPad(int frame)
        {
            if (frame > _frameHitTurboPad + 2)
            {
                AddBoost(1400, sacredFireMultiplier);
            }
            else
            {
                BoostReserve = _lastFrameReserve;
            }

            _frameHitTurboPad = frame;
        }

        private void Landed(Rigidbody rigidbody, Collision collider)
        {
            if (_jumping)
            {
                HandleHangTimeTurbo();
                if(_left ^ _right && _L1Button | _R1Button)
                {
                    _drifting = true;
                    _driftDirection = _left ? -1 : 1;
                    _driftBoostTime = 0f;
                    _driftBoosts = 0;
                }

                _jumping = false;
            }
        }

        // Award boost to kart
        public void AddBoost(int addReserve, float speedMultiplier)
        {
            BoostReserve += addReserve;
            BoostSpeedMultiplier = Mathf.Max(BoostSpeedMultiplier, speedMultiplier);
            _lastFrameReserve = BoostReserve;
        }

        // Remove current boost amount being used
        void EmptyBoostReserve()
        {
            BoostReserve = 0;
        }
        
        private void HandleHangTimeTurbo()
        {
            //if Jump meter < 0x5A0
            if (_jumpMeter < 0x5a0)
            {
                //if Jump meter < 0x3C0
                if (_jumpMeter < 0x3c0)
                {
                    //if Jump meter > 0x27F
                    if (0x27f < _jumpMeter)
                    {
                        // Turbo_Increment
                        // add one second reserves
                        AddBoost(0x3c0,1);
                    }
                }

                //if Jump meter >= 0x3C0
                else
                {
                    // Turbo_Increment
                    // add one second reserves, plus speed
                    AddBoost(0x3c0,fastGreenFireMultiplier);
                }
            }

            //if Jump meter >= 0x5A0
            else
            {
                // Turbo_Increment
                // add one second reserves, plus speed
                AddBoost(0x3c0,yellowFireMultiplier);
            }

            _jumpMeter = 0;
        }
    }
}