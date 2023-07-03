using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Hypocrites
{
    public class MiniMapFOV : MonoBehaviour
    {
        [Range(0f, 360f)][SerializeField] float viewAngle = 0f;
        [SerializeField] float viewRadius = 1f;
        [SerializeField] LayerMask TargetMask;
        [SerializeField] LayerMask ObstacleMask;

        private void OnDrawGizmos()
        {
            Vector3 myPos = transform.position + Vector3.up * 0.5f;
            Gizmos.DrawWireSphere(myPos, viewRadius);

            float lookingAngle = transform.eulerAngles.y;  //캐릭터가 바라보는 방향의 각도
            Vector3 rightDir = AngleToDir(transform.eulerAngles.y + viewAngle * 0.5f);
            Vector3 leftDir = AngleToDir(transform.eulerAngles.y - viewAngle * 0.5f);
            Vector3 lookDir = AngleToDir(lookingAngle);

            Debug.DrawRay(myPos, rightDir * viewRadius, Color.blue);
            Debug.DrawRay(myPos, leftDir * viewRadius, Color.blue);
            Debug.DrawRay(myPos, lookDir * viewRadius, Color.cyan);
        }

        Vector3 AngleToDir(float angle)
        {
            float radian = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radian), 0f, Mathf.Cos(radian));
        }
    }
}
