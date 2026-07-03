using System.Collections.Generic;
using UnityEngine;

namespace TK.Gameplay
{
    [RequireComponent(typeof(LineRenderer))]
    public class ArmLineRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private PlayerMovement player;

        [Header("Trail Settings")]
        [SerializeField] private float minDistance = 0.03f;
        [SerializeField] private float pointLifetime = 1f;
        [SerializeField] private int maxPoints = 80;

        [Header("Retract Settings")]
        [SerializeField] private float retractSpeed = 12f;
        [SerializeField] private float retractFinishDistance = 0.05f;

        [Header("Render Settings")]
        [SerializeField] private float width = 0.5f;

        private LineRenderer lr;

        private List<Vector3> path = new List<Vector3>();
        private List<float> pointTimes = new List<float>();

        private Vector3 lastPos;

        private enum TrailState
        {
            Drawing,
            Retracting,
            Idle
        }

        private TrailState state = TrailState.Drawing;

        void Start()
        {
            lr = GetComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.widthMultiplier = width;

            ForceRefresh();
        }

        void Update()
        {
            Vector3 handPos = player.HandPosition;

            HandleStateTransitions();
            SimulateTrail(handPos);
            AnchorRoot();
            Draw();
        }

        void HandleStateTransitions()
        {
            if (player.HasCollected && state == TrailState.Drawing)
                state = TrailState.Retracting;
        }

        void SimulateTrail(Vector3 handPos)
        {
            switch (state)
            {
                case TrailState.Drawing:
                    AddPoints(handPos);
                    DecayPoints();
                    break;

                case TrailState.Retracting:
                    RetractTrail(handPos);
                    break;
            }
        }

        void AddPoints(Vector3 handPos)
        {
            float dist = Vector3.Distance(handPos, lastPos);

            if (dist >= minDistance)
            {
                Vector3 dir = (handPos - lastPos).normalized;

                while (dist >= minDistance)
                {
                    Vector3 newPoint = lastPos + dir * minDistance;

                    path.Add(newPoint);
                    pointTimes.Add(Time.time);

                    lastPos = newPoint;
                    dist = Vector3.Distance(handPos, lastPos);
                }
            }

            while (path.Count > maxPoints)
            {
                path.RemoveAt(0);
                pointTimes.RemoveAt(0);
            }
        }

        void DecayPoints()
        {
            for (int i = path.Count - 1; i >= 0; i--)
            {
                if (Time.time - pointTimes[i] > pointLifetime)
                {
                    path.RemoveAt(i);
                    pointTimes.RemoveAt(i);
                }
            }
        }

        void RetractTrail(Vector3 handPos)
        {
            if (path.Count == 0)
            {
                state = TrailState.Idle;
                return;
            }

            Vector3 start = startPoint.position;

            for (int i = 0; i < path.Count; i++)
            {
                float t = (float)i / (path.Count - 1);

                Vector3 target = Vector3.Lerp(start, handPos, t);

                path[i] = Vector3.Lerp(
                    path[i],
                    target,
                    retractSpeed * Time.deltaTime
                );
            }

            if (Vector3.Distance(path[0], handPos) < retractFinishDistance)
            {
                path.Clear();
                pointTimes.Clear();
                state = TrailState.Idle;
            }
        }

        void Draw()
        {
            if (path.Count == 0)
            {
                lr.positionCount = 0;
                return;
            }

            lr.positionCount = path.Count + 2;

            lr.SetPosition(0, startPoint.position);

            for (int i = 0; i < path.Count; i++)
                lr.SetPosition(i + 1, path[i]);

            lr.SetPosition(path.Count + 1, player.HandPosition);
        }

        void AnchorRoot()
        {
            if (path.Count > 1)
            {
                path[0] = Vector3.Lerp(startPoint.position, player.HandPosition, 0.2f);
                path[1] = Vector3.Lerp(startPoint.position, player.HandPosition, 0.4f);
            }
        }

        // ==========================
        // FIX FOR INTRO DELAY
        // ==========================
        public void ForceRefresh()
        {
            lastPos = player.HandPosition;

            path.Clear();
            pointTimes.Clear();

            Vector3 mid = Vector3.Lerp(
                startPoint.position,
                player.HandPosition,
                0.35f
            );

            path.Add(mid);
            pointTimes.Add(Time.time);

            path.Add(player.HandPosition);
            pointTimes.Add(Time.time);

            Draw();
        }
    }
}