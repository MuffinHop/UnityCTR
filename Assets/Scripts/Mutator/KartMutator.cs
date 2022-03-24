using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Testing
{
    public class KartMutator : MonoBehaviour
    {
        [SerializeField] private TestData testData;
        [SerializeField] private GameObject startKart;
        [SerializeField] private GameObject secondBest;
        private static int numberOfEntries = 20;
        private List<GameObject> mutatedKarts;
        private double[] strengths = new double[numberOfEntries];
        private float[] kartStartTime = new float[numberOfEntries];
        private double bestStrength = 0f;
        private float timeWhenBestLink = 0f;
        private Vector3 _startPosition;


        private void Start()
        {
            evolutions = 0;
            _startPosition = startKart.transform.position;
            startKart.SetActive(false);
            secondBest.SetActive(false);
            MutateAll(startKart);
        }

        private int evolutions;

        void MutateAll(GameObject baseKart)
        {
            if (evolutions > 0)
            {
                for (int i = 0; i < mutatedKarts.Count; i++)
                {
                    Destroy(mutatedKarts[i]);
                    strengths[i] = 0f;
                    kartStartTime[i] = Time.fixedTime;
                    var srcPosition = _startPosition;

                    /*Record next = testData.GetRecord(0.45f);
                    var nextPosition = 8.0f * new Vector3(
                        next.x / 65536.0f,
                        next.y / 65536.0f + 0.04f,
                        -next.z / 65536.0f);
                    mutatedKarts[i].transform.forward = Vector3.Normalize(nextPosition - srcPosition);*/
                    Kart kart = mutatedKarts[i].GetComponent<Kart>();
                    kart.GetRigidbody.position = testData.GetRecord(0f).GetRealPosition();
                }
            }

            mutatedKarts = new List<GameObject>();

            mutatedKarts.Add(Instantiate(baseKart));
            mutatedKarts[0].SetActive(true);
            kartStartTime[0] = Time.fixedTime;
            mutatedKarts[0].name = "Evolution " + evolutions + " Child " + 0;
            evolutions++;
            testData.Reset();
        }

        void Mutate(GameObject baseKart, int i)
        {
            //Destroy(mutatedKarts[i]);
            strengths[i] = 0f;
            //mutatedKarts[i] = Instantiate(baseKart);
            mutatedKarts[i].SetActive(true);
            Kart kart = mutatedKarts[i].GetComponent<Kart>();
            Kart.Clone(baseKart.GetComponent<Kart>(), kart);
            kart.DisableCamera();
            kart.ResetKart(testData.GetRecord(0f).GetRealPosition(), baseKart.transform.rotation);
            var srcPosition = _startPosition;
            kartStartTime[i] = Time.fixedTime;
            mutatedKarts[i].transform.position = srcPosition + Vector3.up * 0.3f;
            float divider = 1.0f + (float)bestStrength / 10f;
            float multiplier = 1.0f + (Time.fixedTime - timeWhenBestLink);

            String infoRange = "";
            kart.Mutate(multiplier, divider);
            kart.HasTurned = false;

            mutatedKarts[i].name = "Evolution " + evolutions + " Child " + i;
            kart.KartTimer = 0f;
            evolutions++;

        }

        private float VerifyRange(float t)
        {
            return Mathf.Min(0.7f + t / 12f, 4f);
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
                for (int i = 0; i < mutatedKarts.Count; i++)
                {

                    float t = mutatedKarts[i].GetComponent<Kart>().KartTimer;
                    Record record = testData.GetRecord(t);
                    if (record == null)
                    {
                        continue;
                    }

                    var srcPosition = record.GetRealPosition();
                    if (t > 0.2f)
                    {
                        Gizmos.color = Color.cyan;
                        Vector3 velocity = record.GetRealVelocity();
                        Gizmos.DrawRay(srcPosition, velocity);
                    }

                    Gizmos.color = Color.white;
                    Kart kart = mutatedKarts[i].GetComponent<Kart>();
                    Record record2 = testData.GetRecord(t - 0.1f);
                    Gizmos.DrawWireSphere(srcPosition,
                        Mathf.Max(
                            Mathf.Max(record.GetRealVelocity().magnitude, record2.GetRealVelocity().magnitude) * (2f + Time.fixedDeltaTime * 3f), 0.3f + Time.time*0.001f));

                }
        }

        private void LateUpdate()
        {
            if (Time.time * 4f > mutatedKarts.Count && numberOfEntries > mutatedKarts.Count)
            {
                mutatedKarts.Add(Instantiate(startKart));
                mutatedKarts[mutatedKarts.Count - 1].SetActive(true);
                kartStartTime[mutatedKarts.Count - 1] = Time.fixedTime;
                mutatedKarts[mutatedKarts.Count - 1].name =
                    "Evolution " + evolutions + " Child " + (mutatedKarts.Count - 1);
            }
        }

        void FixedUpdate()
        {
            if ((Time.time - timeWhenBestLink) > 240f + bestStrength)
            {
                timeWhenBestLink = Time.time;
                for (int i = 0; i < mutatedKarts.Count; i++)
                {
                    Mutate(startKart, i);
                }

                return;
            }

            for (int i = 0; i < mutatedKarts.Count; i++)
            {
                Kart kart = mutatedKarts[i].GetComponent<Kart>();
                float t = kart.KartTimer;
                Record record = testData.GetRecord(t);
                if (record == null)
                {
                    continue;
                }

                Record record2 = testData.GetRecord(t - 0.1f);

                var srcPosition = record.GetRealPosition();
                if (Vector3.Distance(kart.GetRigidbody.position, srcPosition) > Mathf.Max(
                    Mathf.Max(record.GetRealVelocity().magnitude, record2.GetRealVelocity().magnitude) * (2f + Time.fixedDeltaTime * 3f), 0.3f + Time.time*0.001f))
                {
                    if (bestStrength < kart.KartTimer * 2f + 10f * strengths[i] / kart.KartTimer)
                    {
                        bestStrength = kart.KartTimer * 2f + 10f * strengths[i] / kart.KartTimer;
                        timeWhenBestLink = Time.time;
                        strengths[i] = 0f;
                        var secondKart = secondBest.GetComponent<Kart>();
                        var firstKart = startKart.GetComponent<Kart>();
                        Kart.Clone(firstKart, secondKart);
                        Kart.Clone(mutatedKarts[i].GetComponent<Kart>(), firstKart);
                        float divider = 1.0f + (float)bestStrength / 30f;
                        float multiplier = 1.0f + (Time.fixedTime - timeWhenBestLink);
                        Debug.Log(bestStrength + " | div:" + divider + " | mult:" + multiplier);
                    }

                    Mutate(startKart, i);
                    continue;
                }
                else
                {
                    Vector3 velocity = record.GetRealVelocity();
                    Vector3 a = velocity.normalized;
                    Vector3 b = kart.transform.forward;
                    float positionStrength = 2f * Time.fixedDeltaTime /
                                             (1f + 6f * Mathf.Pow(
                                                 Vector3.Distance(kart.GetRigidbody.position, srcPosition), 2f));
                    float velocityStrength = 2f * Time.fixedDeltaTime / (1f + Mathf.Pow(Vector3.Angle(a, b), 2f));
                    float velocityStrength2 = 2f * Time.fixedDeltaTime /
                                              (1f + Vector3.Distance(a, b) * Vector3.Distance(a, b) * 32f);
                    strengths[i] += positionStrength + velocityStrength + velocityStrength2;
                }
            }
        }
    }
}