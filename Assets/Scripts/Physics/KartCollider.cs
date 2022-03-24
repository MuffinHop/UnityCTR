using System;
using OpenCTR.Level;
using Unity.VisualScripting;
using UnityEngine;

namespace Player
{
    public class KartCollider : MonoBehaviour
    {
        private int _collisions;
        private Vector3 _averageNormal;
        public Vector3 GetAverageNormal() => _averageNormal;
        public bool GetGrounded() => _collisions > 0;
        private int _trackPos = 0;
        public int GetTrackPosition() => _trackPos;
        private LevelHandler _levelHandler;
        private Rigidbody _rigidbody;
        private Vector3 _lastVelocity, _lastAngularVelocity;
        
        /****EVENTS****/
        public Action<Rigidbody, Collision> Landed;
        public Action<Rigidbody, Collision> HitWall;
        public Action<Rigidbody, Collision> PlayerKilled;
        public Action<int> HitTurboPad;
        public Action<int> HitSuperTurboPad;

        private void Start()
        {
            _levelHandler = FindObjectOfType<LevelHandler>();
            _rigidbody = GetComponent<Rigidbody>();
            Physics.IgnoreLayerCollision(gameObject.layer,LayerMask.NameToLayer("NonSolid"));
        }

        private void Update()
        {
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, 0.2f, -transform.up, out hit, 2f))
            {
                if (hit.collider.transform.CompareTag("TurboPad"))
                {
                    HitTurboPad.Invoke(Time.frameCount);
                }
            }
        }
        private void FixedUpdate()
        {
            _lastVelocity = _rigidbody.velocity;
            _lastAngularVelocity = _rigidbody.angularVelocity;
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                HitWall.Invoke(_rigidbody, collision);
                return;
            }
            if (collision.collider.gameObject.CompareTag("Kill"))
            {
                PlayerKilled.Invoke(_rigidbody, collision);
                return;
            }
            _collisions++;
            if (Landed != null)
            {
                Landed.Invoke(_rigidbody, collision);
            }
        }
        private void OnCollisionExit(Collision collision)
        {
            if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                return;
            }
            if (collision.collider.gameObject.CompareTag("Kill"))
            {
                return;
            }
            _collisions--;
        }
        void OnCollisionStay(Collision collision)
        {
            var collider = _rigidbody.GetComponent<Collider>();
            if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                return;
            }
            if (collision.collider.gameObject.CompareTag("Kill"))
            {
                return;
            }
            int howMuchCanBeSkipped = _levelHandler.MaxTrackPosition / 4; 
            _averageNormal = Vector3.zero;
            
            RaycastHit hit;
            foreach (var item in collision.contacts)
            {
                _averageNormal += item.normal;
                Ray ray = new Ray( transform.position, (item.point - transform.position).normalized);
                if( Physics.Raycast( ray, out hit, collision.transform.gameObject.layer ) )
                {
                    int position = (int)hit.textureCoord.x;
                    if (position == 255) return;
                    if (_levelHandler.MaxTrackPosition - howMuchCanBeSkipped < _trackPos &&
                        position < howMuchCanBeSkipped)
                    {
                        _trackPos = position;
                        _levelHandler.LapUI.FinishLap(DateTime.Now);
                        return;
                    }
                    if (_trackPos+howMuchCanBeSkipped<position) return; //255 is OOB or Wall, don't count.
                    _trackPos = Math.Max(position, _trackPos);
                }
            }
            _averageNormal.Normalize();
        }
    }
}