using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace Hypocrites.Player
{
    using Defines;
    using Event;
    using Hypocrites.Grid;
    using Hypocrites.MiniMap;
    using System.Runtime.CompilerServices;
    using Unity.VisualScripting;
    using UnityEngine.UIElements;

    [RequireComponent(typeof(Player))]
    public class PlayerController : MonoBehaviour
    {
        public static bool cameraOn = false;
        public bool smoothTransition = false;
        public float transitionSpeed = 10f;
        public float transitionRotationSpeed = 500f;
        public float transitionMegnification = 2;
        [SerializeField]
        Minimap miniMap;

        Vector3 targetGridPos;
        Vector3 prevTargetGridPos;
        Vector3 targetRotation;

        Player player;

        private void Awake()
        {
            player = GetComponent<Player>();
        }

        private void Start()
        {
            targetGridPos = Vector3Int.RoundToInt(transform.position);
            miniMap.StartRemoveFog(CGrid.Instance.GetNodeFromWorldPosition(targetGridPos));
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        void MovePlayer()
        {
            if (targetRotation.y > 270f)
            {
                if (targetRotation.y > 360f) targetRotation.y = 90f;
                else targetRotation.y = 0f;
            }
            else if (targetRotation.y < 0f) targetRotation.y = 270f;

            if (!smoothTransition) transform.rotation = Quaternion.Euler(targetRotation);
            else transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(targetRotation), Time.deltaTime * transitionRotationSpeed);

            if (CGrid.Instance.GetNodeFromWorldPosition(targetGridPos).Walkable)
            {
                Vector3 targetPosition = targetGridPos;
                prevTargetGridPos = targetGridPos;

                if (!smoothTransition) transform.position = targetPosition;
                else transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
            }
            else targetGridPos = prevTargetGridPos;
        }

        bool AtRest
        {
            get
            {
                return Vector3.Distance(transform.position, targetGridPos) < 0.05f &&
                       Vector3.Distance(transform.eulerAngles, targetRotation) < 0.05f &&
                       !cameraOn && !DialogueManager.Instance.IsTyping;
            }
        }

        public void Rotate(Directions dir)
        {
            Vector3 rotation = Vector3.zero;

            if (AtRest)
            {
                if (dir.Contains(Directions.RIGHT))
                    rotation += Vector3.up * 90f;

                if (dir.Contains(Directions.LEFT))
                    rotation -= Vector3.up * 90f;

                if (dir.Contains(Directions.DOWN))
                    rotation += Vector3.up * 180f;

                targetRotation += rotation;
            }
        }

        public void Move(Directions dir)
        {
            Vector3 movement = Vector3.zero;

            if (AtRest)
            {
                if (dir.Contains(Directions.UP))
                    movement += transform.forward * transitionMegnification;

                if (dir.Contains(Directions.RIGHT))
                    movement += transform.right * transitionMegnification;

                if (dir.Contains(Directions.DOWN))
                    movement -= transform.forward * transitionMegnification;

                if (dir.Contains(Directions.LEFT))
                    movement -= transform.right * transitionMegnification;

                targetGridPos += movement;

                EventManager.Instance.Roll(player.Status.Luck);

                miniMap.RemoveFog(CGrid.Instance.GetNodeFromWorldPosition(targetGridPos));
            }
        }
    }
}