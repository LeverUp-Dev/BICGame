using UnityEngine;
using System.Collections.Generic;

namespace Hypocrites.Player
{
    using DB.Data;
    using Hypocrites.DB;
    using System;

    public class Party : MonoBehaviour
    {
        public List<Member> Members { get; private set; }

        /* ����â UI �ݹ� */
        public delegate void OnMemberJoined();
        public OnMemberJoined onMemberJoined;
        public delegate void OnMemberLeft(string name);
        public OnMemberLeft onMemberLeft;

        private void Awake()
        {
            Members = new List<Member>();

            /* �÷��̾� �� �շ��� ���� ���� �˻� */
            List<Member> members = Database.Instance.Members;

            for (int i = 0; i < members.Count; ++i)
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
        public Member GetMemberFromDatabase(string name = null)
        {
            List<Member> members = Database.Instance.Members;

            if (members is null)
                return null;

            if (name is null)
                return members[0];

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
        public Member GetMember(string name = null)
        {
            if (Members is null)
                return null;

            if (name is null)
                return Members[0];

            for (int i = 1; i < Members.Count; ++i)
            {
                if (Members[i].Status.Equals(name))
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
            Member member = GetMemberFromDatabase(name);

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
                if (Members[i].Status.Equals(name))
                {
                    Members.RemoveAt(i);
                    onMemberLeft(name);
                    return;
                }
            }
        }
        #endregion
    }
}
