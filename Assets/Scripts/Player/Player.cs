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

        /* ����â UI �ݹ� */
        public delegate void OnHpChanged(int max, int cur);
        public OnHpChanged onHpChanged;

        /* ����â UI �ݹ� */
        public delegate void OnMemberJoined();
        public OnMemberJoined onMemberJoined;
        public delegate void OnMemberLeft(string name);
        public OnMemberLeft onMemberLeft;

        private void Awake()
        {
            Members = new List<PlayerData>();

            /* �÷��̾� �� �շ��� ���� ���� �˻� */
            List<PlayerData> members = Database.Instance.Members;
            Status = members[0];
            for (int i = 1; i < members.Count; ++i)
            {
                if (members[i].IsMember)
                    Members.Add(members[i]);
            }
        }

        #region ���� ���� �޼ҵ�
        /// <summary>
        /// Database���� Ư�� ������ ������ �˻��Ѵ�
        /// </summary>
        /// <param name="name">�˻��� ���� �̸�</param>
        /// <returns>���� ����, ���ٸ� null ��ȯ</returns>
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
        /// ���� �շ��� ���� �� Ư�� ������ ������ �˻��Ѵ�
        /// </summary>
        /// <param name="name">�˻��� ���� �̸�</param>
        /// <returns>���� ����, ���ٸ� null ��ȯ</returns>
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
        /// Ư�� ���Ḧ ������ �����Ѵ�
        /// </summary>
        /// <param name="name">�շ��� ������ �̸�, �����ͺ��̽��� �����ؾ� ��</param>
        /// <exception cref="Exception">�����ͺ��̽��� �ش� ������ ������ ���ٸ� ���� �߻�</exception>
        public void JoinMember(string name)
        {
            PlayerData member = GetMemberFromDatabase(name);

            if (member == null)
            {
                throw new Exception($"\"{name}\" �̸��� ���� ���� ������ �����ͺ��̽� �������� �ʽ��ϴ�.");
            }
            else
            {
                member.IsMember = true;
                Members.Add(member);
            }

            onMemberJoined();
        }

        /// <summary>
        /// ������ ���Ḧ �����Ѵ�
        /// </summary>
        /// <param name="name">������ ������ �̸�</param>
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
