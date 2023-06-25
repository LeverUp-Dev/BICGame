using UnityEngine;
using System.Collections.Generic;

namespace Hypocrites.Player
{
    using DB.Data;
    using DB.Save;
    using Hypocrites.DB;
    using System;
    using Utility;

    public class Player : MonoBehaviour
    {
        public PlayerData Status { get; private set; }
        public List<PlayerData> Members { get; private set; }

        /* 상태창 UI 콜백 */
        public delegate void OnHpChanged(int max, int cur);
        public OnHpChanged onHpChanged;

        /* 동료창 UI 콜백 */
        public delegate void OnMemberJoined();
        public OnMemberJoined onMemberJoined;
        public delegate void OnMemberLeft(string name);
        public OnMemberLeft onMemberLeft;

        private void Awake()
        {
            Members = new List<PlayerData>();

            /* 플레이어 및 합류한 동료 정보 검색 */
            List<PlayerData> members = Database.Instance.Members;
            Status = members[0];
            for (int i = 1; i < members.Count; ++i)
            {
                if (members[i].IsMember)
                    Members.Add(members[i]);
            }
        }

        #region 동료 관련 메소드
        /// <summary>
        /// Database에서 특정 동료의 정보를 검색한다
        /// </summary>
        /// <param name="name">검색할 동료 이름</param>
        /// <returns>동료 정보, 없다면 null 반환</returns>
        public PlayerData GetMemberFromDatabase(string name)
        {
            List<PlayerData> members = Database.Instance.Members;

            for (int i = 1; i < members.Count; ++i)
            {
                if (members[i].Name.Equals(name))
                    return members[i];
            }

            return null;
        }

        /// <summary>
        /// 현재 합류한 동료 중 특정 동료의 정보를 검색한다
        /// </summary>
        /// <param name="name">검색할 동료 이름</param>
        /// <returns>동료 정보, 없다면 null 반환</returns>
        public PlayerData GetMember(string name)
        {
            for (int i = 0; i < Members.Count; ++i)
            {
                if (Members[i].Name.Equals(name))
                    return Members[i];
            }

            return null;
        }

        /// <summary>
        /// 특정 동료를 팀으로 영입한다
        /// </summary>
        /// <param name="name">합류할 동료의 이름, 데이터베이스에 존재해야 함</param>
        /// <exception cref="Exception">데이터베이스에 해당 동료의 정보가 없다면 예외 발생</exception>
        public void JoinMember(string name)
        {
            PlayerData member = GetMemberFromDatabase(name);

            if (member == null)
            {
                throw new Exception($"\"{name}\" 이름을 가진 동료 정보가 데이터베이스 존재하지 않습니다.");
            }
            else
            {
                member.IsMember = true;
                Members.Add(member);
            }

            onMemberJoined();
        }

        /// <summary>
        /// 팀에서 동료를 제외한다
        /// </summary>
        /// <param name="name">제외할 동료의 이름</param>
        public void LeaveMember(string name)
        {
            for (int i = 0; i < Members.Count; ++i)
            {
                if (Members[i].Name.Equals(name))
                {
                    Members.RemoveAt(i);
                    onMemberLeft(name);
                    return;
                }
            }
        }
        #endregion

        public void Dealt(int damage)
        {
            Status.Dealt(damage);

            onHpChanged(100, Status.Health);
        }
    }
}
